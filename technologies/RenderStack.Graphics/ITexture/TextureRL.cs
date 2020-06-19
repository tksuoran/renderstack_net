using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    /// \brief Holds RL handle for Texture. Allows RL object garbage collection thread to function without RL context.
    public class TextureRLGhost : IDisposable
    {
        RLtexture textureObject;

        public TextureRLGhost(RLtexture textureObject)
        {
            this.textureObject = textureObject;
        }
        public void Dispose()
        {
            if(textureObject != IntPtr.Zero)
            {
                RL.DeleteTextures(ref textureObject);
                //Debug.WriteLine("DeleteTexture: " + textureObject.ToString());
                GhostManager.Delete();
                textureObject = IntPtr.Zero;
            }
        }
    }
    /// \brief Abstraction for OpenGL texture
    /// \todo Add [Serializable] support
    /// \note Mostly stable.
    public class TextureRL : ITexture, IDisposable, IUniformValue
    {
        #region IUniformValue
        public bool IsCompatibleWith(object o)
        {
            return o is TextureRL;
        }
        public IUniformValue GetUninitialized()
        {
            return new TextureRL();
        }
        public void CopyFrom(IUniformValue src)
        {
            TextureRL other = (TextureRL)src;
            textureObject = other.textureObject;
        }
        public int CompareTo(object o)
        {
            TextureRL other = (TextureRL)o;
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
        private RLtexture           textureObject = IntPtr.Zero;
        private int                 depth;
        private TextureTarget       bindTarget;
        private PixelFormat         format;
        private PixelInternalFormat internalFormat;
        private TextureTarget       texImageTarget;
        private bool                hasMipmaps;
        //private SamplerRL1          sampler;

        public Viewport                                     Size            { get { return size; } }
        public int                                          Depth           { get { return depth; } } 
        public int                                          LayerCount      { get { return 1; } } 
        public bool                                         Dirty           { get { return dirty; } set { dirty = value; } }
        public int                                          Index           { get { return int.MaxValue; } set { } } // to satisfy IUniformValue
        //public int                                          TextureObject   { get { throw new InvalidOperationException(); } }
        //public RLtexture                                    TextureObjectRL { get { return textureObject; } }
        public OpenTK.Graphics.OpenGL.TextureTarget         BindTarget      { get { return (OpenTK.Graphics.OpenGL.TextureTarget      )bindTarget; } }
        public OpenTK.Graphics.OpenGL.PixelFormat           Format          { get { return (OpenTK.Graphics.OpenGL.PixelFormat        )format; } }
        public OpenTK.Graphics.OpenGL.PixelInternalFormat   InternalFormat  { get { return (OpenTK.Graphics.OpenGL.PixelInternalFormat)internalFormat; } }
        public OpenTK.Graphics.OpenGL.TextureTarget         TexImageTarget  { get { return (OpenTK.Graphics.OpenGL.TextureTarget      )texImageTarget; } }
        //public ISampler                                     Sampler         { get { return sampler; } set { sampler = (SamplerRL)value; } }
        public bool                                         HasMipmaps      { get { return hasMipmaps; } }

        #region IDisposable
        bool disposed;
        ~TextureRL()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(textureObject != IntPtr.Zero)
                {
                    GhostManager.Add(new TextureRLGhost(textureObject));
                    Debug.WriteLine("GhostTexture: " + textureObject.ToString());
                    textureObject = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }
        #endregion IDisposable


        /*  Creates a dummy Texture for uniform cache store  */ 
        private TextureRL()
        {
            size = new Viewport(0, 0);
            disposed = true;
            GC.SuppressFinalize(this);
        }

        public TextureRL(int width, int height, PixelFormat format, PixelInternalFormat internalFormat)
        {
            size                = new Viewport(width, height);
            bindTarget          = TextureTarget.Texture2D;
            texImageTarget      = TextureTarget.Texture2D;
            this.format         = format;
            this.internalFormat = internalFormat;
            textureObject       = IntPtr.Zero;

            Initialize();
        }

        public TextureRL(Image image)
        {
            size            = new Viewport(image.Width, image.Height);
            bindTarget      = TextureTarget.Texture2D;
            texImageTarget  = TextureTarget.Texture2D;
            format          = PixelFormat.Rgba;
            internalFormat  = PixelInternalFormat.Rgba;
            textureObject   = IntPtr.Zero;

            RL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            RL.BindTexture(bindTarget, textureObject);

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
                    RL.TexImage2D(
                        texImageTarget,
                        level,
                        internalFormat,
                        image.Mipmaps[level].Width,
                        image.Mipmaps[level].Height,
                        border,
                        format,
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

        public TextureRL(int width, int height, int depth, PixelFormat format, PixelInternalFormat internalFormat)
        {
            size                = new Viewport(width, height);
            this.depth          = depth;
            bindTarget          = TextureTarget.Texture3D;
            texImageTarget      = TextureTarget.Texture3D;
            this.format         = format;
            this.internalFormat = internalFormat;
            textureObject       = IntPtr.Zero;

            Initialize();
        }

        public TextureRL(Image image, bool generateMipmaps)
        {
            size            = new Viewport(image.Width, image.Height);
            bindTarget      = TextureTarget.Texture2D;
            texImageTarget  = TextureTarget.Texture2D;
            format          = PixelFormat.Rgba;
            internalFormat  = PixelInternalFormat.Rgba;
            textureObject   = IntPtr.Zero;

            RL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            RL.BindTexture(bindTarget, textureObject);

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
                    RL.TexImage2D(
                        texImageTarget,
                        level,
                        internalFormat,
                        image.Mipmaps[level].Width,
                        image.Mipmaps[level].Height,
                        border,
                        format,
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
            SetSize();
        }

        /*  TODO reset all mipmap levels  */ 
        private void SetSize()
        {
            RL.BindTexture(bindTarget, textureObject);
            switch(bindTarget)
            {
                case TextureTarget.Texture2D:
                {
                    RL.TexImage2D(
                        texImageTarget,
                        0,
                        internalFormat,
                        size.Width,
                        size.Height,
                        0,
                        format,
                        PixelType.UnsignedByte,
                        IntPtr.Zero
                    );
                    break;
                }
                case TextureTarget.Texture3D:
                {
                    RL.TexImage3D(
                        texImageTarget,
                        0,
                        internalFormat,
                        size.Width,
                        size.Height,
                        Depth,
                        0,
                        format,
                        PixelType.UnsignedByte,
                        IntPtr.Zero
                    );
                    break;
                }
            }
        }

        private void Initialize()
        {
            RL.GenTextures(1, out textureObject);
            Debug.WriteLine("GenTextures: " + textureObject.ToString());
            GhostManager.Gen();

            SetSize();
        }

        public void Upload(System.Byte[] data, int level)
        {
            if(data == null)
            {
                return;
            }
            int border = 0;

            GCHandle ptr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                switch(bindTarget)
                {
                    case TextureTarget.Texture2D:
                    {
                        RL.TexImage2D(
                            texImageTarget,
                            level,
                            internalFormat,
                            size.Width,
                            size.Height,
                            border,
                            format,
                            PixelType.UnsignedByte,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                    case TextureTarget.Texture3D:
                    {
                        RL.TexImage3D(
                            texImageTarget,
                            level,
                            internalFormat,
                            size.Width,
                            size.Height,
                            Depth,
                            border,
                            format,
                            PixelType.UnsignedByte,
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
            int border = 0;

            GCHandle ptr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                switch(bindTarget)
                {
                    case TextureTarget.Texture2D:
                    {
                        RL.TexImage2D(
                            texImageTarget,
                            level,
                            internalFormat,
                            size.Width,
                            size.Height,
                            border,
                            format,
                            PixelType.Float,
                            (System.IntPtr)ptr.AddrOfPinnedObject()
                        );
                        break;
                    }
                    case TextureTarget.Texture3D:
                    {
                        RL.TexImage3D(
                            texImageTarget,
                            level,
                            internalFormat,
                            size.Width,
                            size.Height,
                            Depth,
                            border,
                            format,
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

        public void Unbind()
        {
            RL.BindTexture(bindTarget, IntPtr.Zero);
        }
        public void Apply()
        {
            RL.BindTexture(bindTarget, textureObject);
        }
        public void FramebufferTexture2D(OpenTK.Graphics.OpenGL.FramebufferTarget target, OpenTK.Graphics.OpenGL.FramebufferAttachment attachment, OpenTK.Graphics.OpenGL.TextureTarget face, int level)
        {
            RL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer, 
                (FramebufferAttachment)attachment, 
                (TextureTarget)face,
                textureObject, 
                0
            ); 
        }
        public void FramebufferTextureLayer(OpenTK.Graphics.OpenGL.FramebufferTarget target, OpenTK.Graphics.OpenGL.FramebufferAttachment attachment, int level, int layer)
        {
            throw new System.InvalidOperationException();
            /*RL.FramebufferTextureLayer(
                target, 
                attachment, 
                textureObject, 
                0,
                layer
            );*/ 
        }
        public void GenerateMipmap()
        {
            throw new NotImplementedException();
        }
    }
}
