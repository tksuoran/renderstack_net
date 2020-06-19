// #define DEBUG_BUFFER_OBJECTS

using System;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public interface IBuffer : IDisposable
    {
        BufferTarget        BufferTargetGL      { get; }
        DrawElementsType    DrawElementsTypeGL  { get; }
        VertexFormat        VertexFormat        { get; }
        //UInt32              BufferObjectGL      { get; }
        //IntPtr              BufferObjectRL      { get; }

        //VertexStreamRL VertexStreamRL(IProgram program);
        VertexStreamGL VertexStreamGL(AttributeMappings mappings);
        //void                UpdateAll();
        void                UpdateGL();
        void                UpdateRL();
        void                UseGL();
        void                UseRL();
        IBufferRange        CreateIndexBufferRange(BeginMode beginMode);
        IBufferRange        CreateVertexBufferRange();
        IBufferRange        CreateUniformBufferRange(IUniformBlock uniformBlock);
        void                BindRange(int bindingPoint, IntPtr offsetBytes, IntPtr size);
    }

    public class BufferFactory
    {
        public static IBuffer Create(VertexFormat vertexFormat, BufferUsageHint usageHint)
        {
#if false
            if(Configuration.useOpenRL)
            {
                return new BufferGLRL(vertexFormat, usageHint);
            }
            else
#endif
            {
                return new BufferGL(vertexFormat, usageHint);
            }
        }
        public static IBuffer Create(DrawElementsType indexType, BufferUsageHint usageHint)
        {
#if false
            if(Configuration.useOpenRL)
            {
                return new BufferGLRL(indexType, usageHint);
            }
            else
#endif
            {
                return new BufferGL(indexType, usageHint);
            }
        }
    }
}
