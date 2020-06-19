// #define DEBUG_BUFFER_OBJECTS

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UInt16  = System.UInt16;
using UInt32  = System.UInt32;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class BufferGLRL : IBuffer, IDisposable
    {
        private BufferGL            bufferGL;
        //private BufferRL            bufferRL;

        internal BufferGL           BufferGL            { get { return bufferGL; } }
        //internal BufferRL           BufferRL            { get { return bufferRL; } }

        public  BufferTarget        BufferTargetGL      { get { return bufferGL.BufferTargetGL; } }
        public  DrawElementsType    DrawElementsTypeGL  { get { return bufferGL.DrawElementsTypeGL; } }
        public  VertexFormat        VertexFormat        { get { return bufferGL.VertexFormat; } }
        //public  UInt32              BufferObjectGL      { get { return bufferGL.BufferObjectGL; } }
        //public  IntPtr              BufferObjectRL      { get { return bufferRL.BufferObjectRL; } }

        #region Dispose
        ~BufferGLRL()
        {
            Dispose();
        }
        public void Dispose()
        {
            //  \note Do NOT dispose bufferGL or bufferRL. We can have references to these
            //        directly somewhere without BufferGLRL, and they can live longer. This
            //        is by design.
        }
        #endregion Dispose

        public void BindRange(int bindingPoint, IntPtr offsetBytes, IntPtr size)
        {
            throw new InvalidOperationException();
        }

        public bool Match(IBuffer other)
        {
            //  These should always return the same value..
            return bufferGL.Match(other); // && bufferRL.Match(other);
        }

        public IBufferRange CreateIndexBufferRange(BeginMode beginMode)
        {
#if false
            var gl = (BufferRangeGL)bufferGL.CreateIndexBufferRange(beginMode);
            var rl = (BufferRangeRL)bufferRL.CreateIndexBufferRange(beginMode);
            return new BufferRangeGLRL(gl, rl);
#else
           return (BufferRangeGL)bufferGL.CreateIndexBufferRange(beginMode);
#endif
        }
        public IBufferRange CreateVertexBufferRange()
        {
#if false
            var gl = (BufferRangeGL)bufferGL.CreateVertexBufferRange();
            var rl = (BufferRangeRL)bufferRL.CreateVertexBufferRange();
            return new BufferRangeGLRL(gl, rl);
#else
           return (BufferRangeGL)bufferGL.CreateVertexBufferRange();
#endif
        }
        public IBufferRange CreateUniformBufferRange(IUniformBlock uniformBlock)
        {
#if false
            var gl = (BufferRangeGL)bufferGL.CreateUniformBufferRange(uniformBlock);
            var rl = (BufferRangeRL)bufferRL.CreateUniformBufferRange(uniformBlock);
            return new BufferRangeGLRL(gl, rl);
#else
           return (BufferRangeGL)bufferGL.CreateUniformBufferRange(uniformBlock);
#endif
        }

#if false
        public VertexStreamRL VertexStreamRL(IProgram program)
        {
            return bufferRL.VertexStreamRL(program);
        }
#endif

        public VertexStreamGL VertexStreamGL(AttributeMappings mappings)
        {
            return bufferGL.VertexStreamGL(mappings);
        }

#if false
        internal BufferGLRL(BufferTarget target, BufferUsageHint usageHint)
        {
            bufferGL = new BufferGL(target, usageHint);
            bufferRL = new BufferRL((Caustic.OpenRL.BufferTarget)target, (Caustic.OpenRL.BufferUsageHint)usageHint);
        }

        internal BufferGLRL(VertexFormat vertexFormat, BufferUsageHint usageHint)
        {
            bufferGL = new BufferGL(vertexFormat, usageHint);
            bufferRL = new BufferRL(vertexFormat, (Caustic.OpenRL.BufferUsageHint)usageHint);
        }

        internal BufferGLRL(DrawElementsType indexType, BufferUsageHint usageHint)
        {
            bufferGL = new BufferGL(indexType, usageHint);
            bufferRL = new BufferRL((Caustic.OpenRL.DrawElementsType)indexType, (Caustic.OpenRL.BufferUsageHint)usageHint);
        }
#endif

        public static void PerformDelayedUpdates()
        {
            BufferGL.PerformDelayedUpdates();
        }

        public void UpdateGL()
        {
            bufferGL.UpdateGL();
        }
#if false
        public void UpdateRL()
        {
            bufferRL.UpdateRL();
        }
#endif
        public void UpdateAll()
        {
            lock(this)
            {
                bufferGL.UpdateAll();
                //bufferRL.UpdateAll();
            }
        }

        public void UseGL()
        {
            bufferGL.UseGL();
        }
#if false
        public void UseRL()
        {
            bufferRL.UseRL();
        }
#endif
    }
}
