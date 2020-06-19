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
    /// \brief Holds GL object name for Texture. Allows GL object garbage collection thread to function without GL context.
    /// \note Mostly stable.
    public class TextureGLGhost : IDisposable
    {
        int textureObject;

        public TextureGLGhost(int textureObject)
        {
            this.textureObject = textureObject;
        }
        public void Dispose()
        {
            if(textureObject != 0)
            {
                GL.DeleteTexture(textureObject);
                //Debug.WriteLine("DeleteTexture: " + textureObject.ToString());
                GhostManager.Delete();
                textureObject = 0;
            }
        }
    }

    /// \brief Abstraction for OpenGL texture
    /// \todo Add [Serializable] support
    /// \note Mostly stable.
    public class TextureGL : ITexture, IDisposable, IUniformValue
    {
        #region IUniformValue
        public bool     IsCompatibleWith(object o)
        {
            return o is TextureGL;
        }
        public IUniformValue GetUninitialized()
        {
            return new TextureGL();
        }
        public void CopyFrom(IUniformValue src)
        {
            TextureGL other = (TextureGL)src;
            textureObject = other.textureObject;
        }
        public int CompareTo(object o)
        {
            TextureGL other = (TextureGL)o;
            if(
                (textureObject == other.textureObject) &&
                (BindTarget == other.BindTarget)
            )
            {
                return 0;
            }
            return -1;
        }
        #endregion 

        private Viewport            size;
        private bool                dirty = true; 
        private int                 textureObject = 0;
        private int                 layerCount;
        private int                 depth;
        private bool                hasMipmaps = false;

        public bool                 Dirty           { get { return dirty; } set { dirty = value; } }
        public int                  Depth           { get { return depth; } } 
        public int                  LayerCount      { get { return layerCount; } } 
        public Viewport             Size            { get { return size; } }
        public int                  Index           { get { return int.MaxValue; } set { } } // to satisfy IUniformValue
        //public int                  TextureObject   { get { return textureObject; } }
        public TextureTarget        BindTarget      { get; private set; }
        public PixelFormat          Format          { get; private set; }
        public PixelInternalFormat  InternalFormat  { get; private set; }
        public TextureTarget        TexImageTarget  { get; private set; }
        public bool                 HasMipmaps      { get { return hasMipmaps; } }

        private bool disposed;
        ~TextureGL()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(textureObject != 0)
                {
                    GhostManager.Add(new TextureGLGhost(textureObject));
                    Debug.WriteLine("GhostTexture: " + textureObject.ToString());
                    textureObject = 0;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        /*public void Apply(ref SamplerContext context)
        {
            GL.Uniform1(context.UniformIndex, context.TextureIndex);
            GL.ActiveTexture(TextureUnit.Texture0 + context.TextureIndex);
            Apply();
            ++context.TextureIndex;
        }*/

        /*  Creates a dummy Texture for uniform cache store  */ 
        private TextureGL()
        {
            size = new Viewport(0, 0);
            disposed = true;
            GC.SuppressFinalize(this);
        }
        public TextureGL(int size, PixelFormat format, PixelInternalFormat internalFormat)
        {
            this.size       = new Viewport(size, size);
            BindTarget      = TextureTarget.TextureCubeMap;
            TexImageTarget  = 0; // Not used
            Format          = format;
            InternalFormat  = internalFormat;
            textureObject   = 0;

            Initialize();
        }
        public TextureGL(int width, int height, PixelFormat format, PixelInternalFormat internalFormat)
        {
            size            = new Viewport(width, height);
            BindTarget      = TextureTarget.Texture2D;
            TexImageTarget  = TextureTarget.Texture2D;
            Format          = format;
            InternalFormat  = internalFormat;
            textureObject   = 0;

            Initialize();
        }
        public TextureGL(int width, int height, PixelFormat format, PixelInternalFormat internalFormat, int layerCount)
        {
            size        = new Viewport(width, height);
            this.layerCount = layerCount;
            BindTarget      = TextureTarget.Texture2DArray;
            TexImageTarget  = TextureTarget.Texture2DArray;
            Format          = format;
            InternalFormat  = internalFormat;
            textureObject   = 0;

            Initialize();
        }
        public TextureGL(Image[] images)
        {
            if(images.Length != 6)
            {
                System.Diagnostics.Trace.TraceError("Error: Need 6 images for cube maps");
                throw new ArgumentException();
            }
            size = new Viewport(images[0].Width, images[0].Height);
            BindTarget      = TextureTarget.TextureCubeMap;
            TexImageTarget  = TextureTarget.TextureCubeMapPositiveX;
            Format          = PixelFormat.Rgba;
            InternalFormat  = PixelInternalFormat.Rgba;
            textureObject   = 0;

            GL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            GL.BindTexture(BindTarget, textureObject);

            int border = 0;

            for(int face = 0; face < 6; ++face)
            {
                int level = 0;

                //  TODO USE nvtt right for cube textures!!!
                images[face].GenerateMipmaps();
                hasMipmaps = true;
                for(;;)
                {
                    if(
                        (images[face].Mipmaps.ContainsKey(level) == false) || 
                        (images[face].Mipmaps[level] == null)
                    )
                    {
                        Debug.WriteLine("No mipmaps");
                        hasMipmaps = false;
                        break;
                    }
                    GCHandle ptr = GCHandle.Alloc(
                        images[face].Mipmaps[level].Pixels, 
                        GCHandleType.Pinned
                    );
                    try
                    {
                        GL.TexImage2D(
                            TextureTarget.TextureCubeMapPositiveX + face,
                            level,
                            InternalFormat,
                            images[face].Mipmaps[level].Width,
                            images[face].Mipmaps[level].Height,
                            border,
                            Format,
                            PixelType.UnsignedByte,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                    }
                    finally
                    {
                        ptr.Free();
                    }
                    if(
                        (images[face].Mipmaps[level].Width == 1) &&
                        (images[face].Mipmaps[level].Height == 1)
                    )
                    {
                        break;
                    }
                    ++level;
                }
            }
        }
        public TextureGL(Image image)
        {
            size            = new Viewport(image.Width, image.Height);
            BindTarget      = TextureTarget.Texture2D;
            TexImageTarget  = TextureTarget.Texture2D;
            Format          = PixelFormat.Rgba;
            InternalFormat  = PixelInternalFormat.Rgba;
            textureObject   = 0;

            GL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            GL.BindTexture(BindTarget, textureObject);

            int level = 0;
            int border = 0;

            image.GenerateMipmaps();

            hasMipmaps = true;
            for(;;)
            {
                if(image.Mipmaps.ContainsKey(level) == false || image.Mipmaps[level] == null)
                {
                    Debug.WriteLine("Warning: No mipmaps");
                    hasMipmaps = false;
                    break;
                }

                GCHandle ptr = GCHandle.Alloc(
                    image.Mipmaps[level].Pixels, 
                    GCHandleType.Pinned
                );
                try
                {
                    GL.TexImage2D(
                        TexImageTarget,
                        level,
                        InternalFormat,
                        image.Mipmaps[level].Width,
                        image.Mipmaps[level].Height,
                        border,
                        Format,
                        PixelType.UnsignedByte,
                        (System.IntPtr)ptr.AddrOfPinnedObject()
                    );
                }
                finally
                {
                    ptr.Free();
                }
                if(
                    (image.Mipmaps[level].Width == 1) &&
                    (image.Mipmaps[level].Height == 1)
                )
                {
                    break;
                }
                ++level;
            }
        }
        public TextureGL(int width, int height, int depth, PixelFormat format, PixelInternalFormat internalFormat)
        {
            size        = new Viewport(width, height);
            this.depth      = depth;
            BindTarget      = TextureTarget.Texture3D;
            TexImageTarget  = TextureTarget.Texture3D;
            Format          = format;
            InternalFormat  = internalFormat;
            textureObject   = 0;

            Initialize();
        }
        public TextureGL(Image image, bool generateMipmaps)
        {
            size            = new Viewport(image.Width, image.Height);
            BindTarget      = TextureTarget.Texture2D;
            TexImageTarget  = TextureTarget.Texture2D;
            Format          = PixelFormat.Rgba;
            InternalFormat  = PixelInternalFormat.Rgba;
            textureObject   = 0;

            GL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            GL.BindTexture(BindTarget, textureObject);

            int level = 0;
            int border = 0;

            if(generateMipmaps)
            {
                image.GenerateMipmaps();

                hasMipmaps = true;
            }
            for(;;)
            {
                if(image.Mipmaps.ContainsKey(level) == false || image.Mipmaps[level] == null)
                {
                    Debug.WriteLine("Warning: No mipmaps");
                    hasMipmaps = false;
                    break;
                }

                GCHandle ptr = GCHandle.Alloc(
                    image.Mipmaps[level].Pixels, 
                    GCHandleType.Pinned
                );
                try
                {
                    GL.TexImage2D(
                        TexImageTarget,
                        level,
                        InternalFormat,
                        image.Mipmaps[level].Width,
                        image.Mipmaps[level].Height,
                        border,
                        Format,
                        PixelType.UnsignedByte,
                        (System.IntPtr)ptr.AddrOfPinnedObject()
                    );
                }
                finally
                {
                    ptr.Free();
                }
                if(
                    (image.Mipmaps[level].Width == 1) &&
                    (image.Mipmaps[level].Height == 1)
                )
                {
                    break;
                }
                ++level;
            }
        }

        public void Resize(int width, int height)
        {
            size.Resize(width, height);
            AllocateSize();
        }

        /*  TODO reset all mipmap levels  */ 
        private void AllocateSize()
        {
            GL.BindTexture(BindTarget, textureObject);
            switch(BindTarget)
            {
                case TextureTarget.Texture2D:
                {
                    GL.TexImage2D(
                        TexImageTarget,
                        0,
                        InternalFormat,
                        size.Width,
                        size.Height,
                        0,
                        Format,
                        OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                        new System.IntPtr(0)
                    );
                    break;
                }
                case TextureTarget.Texture2DArray:
                {
                    GL.TexImage3D(
                        TexImageTarget,
                        0,
                        InternalFormat,
                        size.Width,
                        size.Height,
                        layerCount,
                        0,
                        Format,
                        OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                        new System.IntPtr(0)
                    );
                    break;
                }
                case TextureTarget.Texture3D:
                {
                    GL.TexImage3D(
                        TexImageTarget,
                        0,
                        InternalFormat,
                        size.Width,
                        size.Height,
                        Depth,
                        0,
                        Format,
                        OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                        new System.IntPtr(0)
                    );
                    break;
                }
                case TextureTarget.TextureCubeMap:
                {
                    for(int face = 0; face < 6; ++face)
                    {
                        GL.TexImage2D(
                            TextureTarget.TextureCubeMapPositiveX + face,
                            0,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            0,
                            Format,
                            OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                            new System.IntPtr(0)
                        );
                    }
                    break;
                }
            }
        }

        private void Initialize()
        {
            GL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            AllocateSize();
        }

        public void Upload(System.Byte[] data, int level)
        {
            if(data == null)
            {
                return;
            }

            GL.BindTexture(BindTarget, textureObject);
            int border = 0;
            GCHandle ptr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                switch(BindTarget)
                {
                    case TextureTarget.Texture2D:
                    {
                        GL.TexImage2D(
                            TexImageTarget,
                            level,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            border,
                            Format,
                            PixelType.UnsignedByte,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                    case TextureTarget.Texture2DArray:
                    {
                        GL.TexImage3D(
                            TexImageTarget,
                            level,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            layerCount,
                            border,
                            Format,
                            OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                    case TextureTarget.Texture3D:
                    {
                        GL.TexImage3D(
                            TexImageTarget,
                            level,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            Depth,
                            border,
                            Format,
                            OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                }
            }
            finally
            {
                ptr.Free();
            }
        }
        public void Upload(System.Single[] data, int level)
        {
            if(data == null)
            {
                return;
            }

            GL.BindTexture(BindTarget, textureObject);

            int border = 0;
            GCHandle ptr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                switch(BindTarget)
                {
                    case TextureTarget.Texture2D:
                    {
                        GL.TexImage2D(
                            TexImageTarget,
                            level,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            border,
                            Format,
                            PixelType.Float,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                    case TextureTarget.Texture2DArray:
                    {
                        GL.TexImage3D(
                            TexImageTarget,
                            level,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            layerCount,
                            border,
                            Format,
                            PixelType.Float,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                    case TextureTarget.Texture3D:
                    {
                        GL.TexImage3D(
                            TexImageTarget,
                            level,
                            InternalFormat,
                            size.Width,
                            size.Height,
                            Depth,
                            border,
                            Format,
                            PixelType.Float,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                }
            }
            finally
            {
                ptr.Free();
            }
        }
        public void Upload(IntPtr ptr, int level, PixelInternalFormat internalFormat, PixelType pixelType)
        {
            if(ptr == IntPtr.Zero)
            {
                return;
            }

            GL.BindTexture(BindTarget, textureObject);
            int border = 0;

            switch(BindTarget)
            {
                case TextureTarget.Texture2D:
                {
                    GL.TexImage2D(
                        TexImageTarget,
                        level,
                        internalFormat,
                        size.Width,
                        size.Height,
                        border,
                        Format,
                        pixelType,
                        ptr
                    );
                    break;
                }
                case TextureTarget.Texture2DArray:
                {
                    GL.TexImage3D(
                        TexImageTarget,
                        level,
                        internalFormat,
                        size.Width,
                        size.Height,
                        layerCount,
                        border,
                        Format,
                        pixelType,
                        ptr
                    );
                    break;
                }
                case TextureTarget.Texture3D:
                {
                    GL.TexImage3D(
                        TexImageTarget,
                        level,
                        internalFormat,
                        size.Width,
                        size.Height,
                        Depth,
                        border,
                        Format,
                        pixelType,
                        ptr
                    );
                    break;
                }
            }
        }

        public void Unbind()
        {
            GL.BindTexture(BindTarget, 0);
        }
        public void Apply()
        {
            GL.BindTexture(BindTarget, textureObject);
        }
        public void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget face, int level)
        {
            GL.FramebufferTexture2D(
                target, 
                attachment, 
                face,
                textureObject, 
                level
            ); 
        }
        public void FramebufferTextureLayer(FramebufferTarget target, FramebufferAttachment attachment, int level, int layer)
        {
            GL.FramebufferTextureLayer(
                target, 
                attachment, 
                textureObject, 
                level,
                layer
            ); 
        }
        public void GenerateMipmap()
        {
            throw new NotImplementedException();
        }
    }
}
