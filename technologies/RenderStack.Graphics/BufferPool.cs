using System;
using System.Collections.Generic;
using BufferTarget      = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint   = OpenTK.Graphics.OpenGL.BufferUsageHint;
using DrawElementsType  = OpenTK.Graphics.OpenGL.DrawElementsType;

namespace RenderStack.Graphics
{
    public class BufferPool : IDisposable 
    {
        public static BufferPool Instance = new BufferPool();

        private Dictionary<KeyValuePair<VertexFormat, BufferUsageHint>, IBuffer>     vertexBuffers    = new Dictionary<KeyValuePair<VertexFormat,BufferUsageHint>, IBuffer>();
        private Dictionary<KeyValuePair<DrawElementsType, BufferUsageHint>, IBuffer> indexBuffers     = new Dictionary<KeyValuePair<DrawElementsType,BufferUsageHint>, IBuffer>();
        private Dictionary<BufferUsageHint, BufferGL>                                uniformBuffersGL = new Dictionary<BufferUsageHint, BufferGL>();

        public IBuffer GetVertexBuffer(VertexFormat vertexFormat, BufferUsageHint bufferUsageHint)
        {
            if(Configuration.useGl1 || Configuration.useOpenRL)
            {
                //  \todo Do index remapping due to lack on DrawElementsBaseVertex()
                return BufferFactory.Create(vertexFormat, bufferUsageHint);
            }
            //return BufferFactory.Create(vertexFormat, bufferUsageHint);
#if true
            lock(this)
            {
                var kvp = new KeyValuePair<VertexFormat, BufferUsageHint>(vertexFormat, bufferUsageHint);
                if(vertexBuffers.ContainsKey(kvp))
                {
                    //Console.WriteLine("Returning old vertex buffer, vertex format = " + vertexFormat.ToString());
                    if(vertexFormat.Stride != vertexBuffers[kvp].VertexFormat.Stride)
                    {
                        throw new Exception("");
                    }
                    return vertexBuffers[kvp];
                }
                //Console.WriteLine("Creating new vertex buffer, vertex format = " + vertexFormat.ToString());
                var buffer = BufferFactory.Create(vertexFormat, bufferUsageHint);
                vertexBuffers[kvp] = buffer;
                return buffer;
            }
#endif
        }
        public IBuffer GetIndexBuffer(DrawElementsType indexType, BufferUsageHint bufferUsageHint)
        {
            if(RenderStack.Graphics.Configuration.useGl1 || Configuration.useOpenRL)
            {
                //  \todo Do index remapping due to lack on DrawElementsBaseVertex()
                return BufferFactory.Create(indexType, bufferUsageHint);
            }
//            return new Buffer(indexType, bufferUsageHint);
#if true
            lock(this)
            {
                var kvp = new KeyValuePair<DrawElementsType, BufferUsageHint>(indexType, bufferUsageHint);
                if(indexBuffers.ContainsKey(kvp))
                {
                    //Debug.WriteLine("Returning old index buffer, index type = " + indexType.ToString());
                    return indexBuffers[kvp];
                }
                //Debug.WriteLine("Creating new index buffer, index type = " + indexType.ToString());
                var buffer = BufferFactory.Create(indexType, bufferUsageHint);
                indexBuffers[kvp] = buffer;
                return buffer;
            }
#endif
        }
        public BufferGL GetUniformBufferGL(BufferUsageHint usage)
        {
            if(Configuration.useGl1)
            {
                throw new InvalidOperationException("GL1 does not support uniform buffers");
            }
            //return new Buffer(BufferTarget.UniformBuffer, usage);
#if true
            lock(this)
            {
                if(uniformBuffersGL.ContainsKey(usage))
                {
                    //Debug.WriteLine("Returning old uniform buffer, usage = " + usage.ToString());
                    return uniformBuffersGL[usage];
                }
                //Debug.WriteLine("Creating new uniform buffer, usage = " + usage.ToString());
                var buffer = new BufferGL(BufferTarget.UniformBuffer, usage);
                uniformBuffersGL[usage] = buffer;
                return buffer;
            }
#endif
        }
#if false
        public BufferRL GetUniformBufferRL(BufferUsageHint usage)
        {
            //  Current OpenRL API rlUniformBlockBuffer() needs
            //  exactly one buffer object per uniform block
            //  so we can not share the same buffer to multiple
            //  uniform blocks.
            var buffer = new BufferRL(Caustic.OpenRL.BufferTarget.UniformBlockBuffer, (Caustic.OpenRL.BufferUsageHint)usage);
            return buffer;
        }
#endif

        #region IDisposable Members

        public void Dispose()
        {
            foreach(var buffer in vertexBuffers)
            {
                buffer.Value.Dispose();
            }
            vertexBuffers.Clear();
            foreach(var buffer in indexBuffers)
            {
                buffer.Value.Dispose();
            }
            indexBuffers.Clear();
        }

        #endregion
    }
}
