using System.Collections.Generic;

namespace RenderStack.Graphics
{
    public delegate void Callback();
    public interface IUniformBuffer
    {
        Callback SyncDelegate { get; set; }

        bool    Contains(string key);
        Floats  Floats  (string key);
        Ints    Ints    (string key);
        UInts   UInts   (string key);
        void    Sync    ();
        void    Use     ();
    }
    public class UniformBufferFactory
    {
        public static IUniformBuffer Create(IUniformBlock uniformBlock)
        {
            /*if(Configuration.useGl1)
            {
                return (IUniformBuffer)new UniformBufferGL1(uniformBlock);
            }*/
            return (IUniformBuffer)new UniformBufferGL(uniformBlock);
        }
    }
}
