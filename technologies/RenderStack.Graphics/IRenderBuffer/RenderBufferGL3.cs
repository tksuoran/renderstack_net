using System;
using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Holds GL object name for RenderBuffer. Allows GL object garbage collection thread to function without GL context.
    /// 
    /// \note Mostly stable.
    public class RenderBufferGhost : IDisposable
    {
        int renderBufferObject;
        public RenderBufferGhost(int renderBufferObject)
        {
            this.renderBufferObject = renderBufferObject;
        }
        public void Dispose()
        {
            if(renderBufferObject != 0)
            {
                GL.DeleteRenderbuffers(1, ref renderBufferObject);
                GhostManager.Delete();
                renderBufferObject = 0;
            }
        }
    }
    /// \brief Abstraction for OpenGL renderbuffer.
    /// 
    /// \todo [Serializable]
    /// \note Mostly stable.
    public class RenderBufferGL3 : IRenderBuffer
    {
        private readonly RenderStack.Math.Viewport  viewport;
        private int                                 renderBufferObject;
        private RenderbufferStorage                 internalFormat;
        private int                                 sampleCount;

        public RenderStack.Math.Viewport    Viewport        { get { return viewport; } }
        public RenderbufferStorage          InternalFormat  { get { return internalFormat; } }
        public int                          SampleCount     { get { return sampleCount; } }

        //public int RenderBufferObject { get { return renderBufferObject; } }

        private void SetSize()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBufferObject);
            //if(sampleCount > 0)
            {
                GL.RenderbufferStorageMultisample(
                    RenderbufferTarget.Renderbuffer, 
                    SampleCount,
                    InternalFormat,
                    viewport.Width,
                    viewport.Height
                );
            }
        }

        public void Resize(int width, int height)
        {
            viewport.Resize(width, height);
            SetSize();
        }

        bool disposed;
        ~RenderBufferGL3()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                //System.Diagnostics.Debug.WriteLine("DeleteRenderbuffers " + renderBufferObject);
                if(renderBufferObject != 0)
                {
                    GhostManager.Add(new RenderBufferGhost(renderBufferObject));
                    renderBufferObject = 0;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        public void FramebufferRenderbuffer(
            FramebufferTarget       framebufferTarget,
            FramebufferAttachment   attachment,
            RenderbufferTarget      renderbufferTarget
        )
        {
            GL.FramebufferRenderbuffer(
                framebufferTarget,  
                attachment, 
                renderbufferTarget, 
                renderBufferObject
            );
        }

        public RenderBufferGL3(int width, int height, RenderbufferStorage internalFormat, int sampleCount)
        {
            viewport = new RenderStack.Math.Viewport(width, height);
            this.internalFormat = internalFormat;
            this.sampleCount = sampleCount;

            GL.GenRenderbuffers(1, out renderBufferObject);
            GhostManager.Gen();
            //System.Diagnostics.Debug.WriteLine("GenRenderbuffer " + renderBufferObject);
            SetSize();
#if false
            else
            {
                GL.RenderbufferStorage(
                    RenderbufferTarget.Renderbuffer, 
                    internalFormat, 
                    width, 
                    height
                );
            }
#endif
        }
    }
}
