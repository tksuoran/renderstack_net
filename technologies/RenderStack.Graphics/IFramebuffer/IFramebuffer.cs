using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public interface IFramebuffer : System.IDisposable
    {
        Math.Viewport Viewport { get; }

        TextureGL this[FramebufferAttachment attachment]{ get; }

        TextureGL AttachCubeTexture(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat
        );
        void UnbindTexture(
            FramebufferAttachment   attachment,
            TextureTarget           target
        );
        void AttachCubeFace(
            FramebufferAttachment   attachment, 
            TextureTarget           face,
            int                     level
        );
        void AttachTextureLevel(FramebufferAttachment attachment, int level);
        void AttachTextureLayer(FramebufferAttachment attachment, int level, int layer);
        TextureGL AttachTexture(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat
        );
        TextureGL AttachTextureArray(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat,
            int                     layerCount
        );
        IRenderBuffer AttachRenderBuffer(
            FramebufferAttachment   attachment, 
            //PixelFormat             format, 
            RenderbufferStorage     internalFormat,
            int                     sampleCount
        );
        void Begin();
        void End();
        bool Check();
        void Blit(IFramebuffer target, ClearBufferMask mask, BlitFramebufferFilter filter);
    }
    public class FramebufferFactory
    {
        public static IFramebuffer Create(OpenTK.GameWindow window)
        {
            if(Configuration.useGl1)
            {
                return new FramebufferGL1(window);
            }
            return new FramebufferGL3(window);
        }
        public static IFramebuffer Create(int width, int height)
        {
            if(Configuration.useGl1)
            {
                return new FramebufferGL1(width, height) as IFramebuffer;
            }
            return new FramebufferGL3(width, height) as IFramebuffer;
        }
    }
}
