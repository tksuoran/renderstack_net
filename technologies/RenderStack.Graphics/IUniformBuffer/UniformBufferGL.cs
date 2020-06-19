// #define DEBUG_UNIFORM_BUFFER

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public class UniformBufferGL : IUniformBuffer
    {
        private UniformBufferData   data;
        private IBufferRange        bufferRange;
        private Callback            syncDelegate;

        public Callback SyncDelegate { get { return syncDelegate; } set { syncDelegate = value; } }
        public bool     Contains(string key) { return data.Parameters.ContainsKey(key); }
        public Floats   Floats  (string key) { return data.Floats(key);   }
        public Ints     Ints    (string key) { return data.Ints(key);     }
        public UInts    UInts   (string key) { return data.UInts(key);    }

        public UniformBufferGL(IUniformBlock uniformBlock)
        {
            //  Required always
            data = new UniformBufferData(uniformBlock);

            //  Required for GL3 and OpenRL only
            if(Configuration.canUseUniformBufferObject && (Configuration.useGl1 == false))
            {
                IBuffer buffer = BufferPool.Instance.GetUniformBufferGL(BufferUsageHint.DynamicDraw);
                bufferRange = buffer.CreateUniformBufferRange(uniformBlock);
                bufferRange.Name = uniformBlock.Name;
                Sync();
            }
        }

        public void Use()
        {
            //  Required for GL1 only
            data.UniformBlock.UniformBuffer = this;

            //  Required for GL3 and OpenRL only
            if(bufferRange != null)
            {
                bufferRange.UseUniformBufferGL();
            }
        }

        public void Sync()
        {
            //  Required for GL3 / OpenRL only
            if(bufferRange != null)
            {
                data.Sync(bufferRange);
            }

            //  Required for GL1 only
            if(syncDelegate != null)
            {
                syncDelegate();
            }
        }
    }
}
