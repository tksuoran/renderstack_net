//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
