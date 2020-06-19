using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Abstraction for OpenGL renderbuffer.
    /// 
    /// \todo [Serializable]
    /// \note Mostly stable.
    public class RenderBufferGL1 : IRenderBuffer
    {
        private readonly RenderStack.Math.Viewport  viewport;
        private RenderbufferStorage                 internalFormat;
        private int                                 sampleCount;

        public RenderStack.Math.Viewport    Viewport        { get { return viewport; } }
        public RenderbufferStorage          InternalFormat  { get { return internalFormat; } }
        public int                          SampleCount     { get { return sampleCount; } }

        private void SetSize()
        {
            // \todo
        }

        public void Resize(int width, int height)
        {
            viewport.Resize(width, height);
            SetSize();
        }

        public void Dispose()
        {
        }

        public RenderBufferGL1(int width, int height, RenderbufferStorage internalFormat, int sampleCount)
        {
            viewport = new RenderStack.Math.Viewport(width, height);
            this.internalFormat = internalFormat;
            this.sampleCount = sampleCount;

            SetSize();
        }
    }
}
