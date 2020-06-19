using System;
using OpenTK.Graphics.OpenGL;
using RenderStack.Math;

namespace RenderStack.Graphics
{
    public interface ITexture : IDisposable, IUniformValue
    {
        int                 Depth           { get; } 
        int                 LayerCount      { get; } 
        Viewport            Size            { get; }
        TextureTarget       BindTarget      { get; }
        PixelFormat         Format          { get; }
        PixelInternalFormat InternalFormat  { get; }
        TextureTarget       TexImageTarget  { get; }
        bool                HasMipmaps      { get; }

        void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget face, int level);
        void FramebufferTextureLayer(FramebufferTarget target, FramebufferAttachment attachment, int level, int layer);
        void Resize(int width, int height);
        void Upload(byte[] data, int level);
        void Upload(float[] data, int level);
        void Unbind();
        void Apply();
        void GenerateMipmap();
    }

    public class TextureFactory
    {
        /*ITexture Create()
        {
            return new TextureGL();
        }*/
        ITexture Create(int size, PixelFormat format, PixelInternalFormat internalFormat)
        {
            return (ITexture)new TextureGL(size, format, internalFormat);
        }

        ITexture Create(int width, int height, PixelFormat format, PixelInternalFormat internalFormat)
        {
            return (ITexture)new TextureGL(width, height, format, internalFormat);
        }

        ITexture Create(int width, int height, PixelFormat format, PixelInternalFormat internalFormat, int layerCount)
        {
            return (ITexture)new TextureGL(width, height, format, internalFormat, layerCount);
        }

        ITexture CreateTextureGL(Image[] images)
        {
            return (ITexture)new TextureGL(images);
        }

        ITexture Create(Image image)
        {
            return (ITexture)new TextureGL(image);
        }

        ITexture Create(int width, int height, int depth, PixelFormat format, PixelInternalFormat internalFormat)
        {
            return (ITexture)new TextureGL(width, height, depth, format, internalFormat);
        }

        ITexture Create(Image image, bool generateMipmaps)
        {
            return (ITexture)new TextureGL(image, generateMipmaps);
        }

    }
}
