﻿using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Abstraction for OpenGL framebuffer
    /// 
    /// \todo [Serializable]
    /// \note Mostly stable, somewhat experimental.
    public class FramebufferGL1 : IFramebuffer
    {
        private readonly RenderStack.Math.Viewport  viewport;
        public RenderStack.Math.Viewport            Viewport { get { return viewport; } }

        private Dictionary<FramebufferAttachment, TextureGL>          textures        = new Dictionary<FramebufferAttachment,TextureGL>();
        private Dictionary<FramebufferAttachment, RenderBufferGL1>  renderbuffers   = new Dictionary<FramebufferAttachment,RenderBufferGL1>();

        public TextureGL this[FramebufferAttachment attachment]
        {
            get
            {
                if(textures.ContainsKey(attachment))
                {
                    return textures[attachment];
                }
                return null;
            }
        }

        public int FramebufferObject { get { throw new System.NotImplementedException(); } }

        bool disposed;
        ~FramebufferGL1()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                foreach(var kvp in textures)
                {
                    kvp.Value.Dispose();
                }
                System.GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        public FramebufferGL1(OpenTK.GameWindow window)
        {
            viewport = new RenderStack.Math.Viewport(window.Width, window.Height);
            window.Resize += new System.EventHandler<System.EventArgs>(window_Resize);
        }

        void window_Resize(object sender, System.EventArgs e)
        {
            OpenTK.GameWindow window = (OpenTK.GameWindow)(sender);

            viewport.Width = window.Width;
            viewport.Height = window.Height;
        }

        public void Resize(int width, int height)
        {
            foreach(TextureGL texture in textures.Values)
            {
                texture.Resize(width, height);
            }
        }

        public FramebufferGL1(int width, int height)
        {
            viewport = new RenderStack.Math.Viewport(width, height);
        }

        public TextureGL AttachCubeTexture(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat
        )
        {
            if(viewport.Width != viewport.Height)
            {
                throw new System.InvalidOperationException();
            }
            TextureGL texture = new TextureGL(viewport.Width, format, internalFormat);
            textures[attachment] = texture;
            AttachCubeFace(attachment, TextureTarget.TextureCubeMapNegativeX, 0);
            return texture;
        }

        public void UnbindTexture(
            FramebufferAttachment   attachment,
            TextureTarget           target
        )
        {
            // \todo
        }

        public void AttachCubeFace(
            FramebufferAttachment   attachment, 
            TextureTarget           face,
            int                     level
        )
        {
            textures[attachment].Apply();

            // \todo
        }

        public void AttachTextureLevel(FramebufferAttachment attachment, int level)
        {
            textures[attachment].Apply();
            // \todo
        }
        public void AttachTextureLayer(FramebufferAttachment attachment, int level, int layer)
        {
            textures[attachment].Apply();
            // \todo
        }

        public TextureGL AttachTexture(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat
        )
        {
            TextureGL texture = new TextureGL(viewport.Width, viewport.Height, format, internalFormat);
            textures[attachment] = texture;
            AttachTextureLevel(attachment, 0);
            return texture;
        }

        public TextureGL AttachTextureArray(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat,
            int                     layerCount
        )
        {
            TextureGL texture = new TextureGL(viewport.Width, viewport.Height, format, internalFormat, layerCount);
            textures[attachment] = texture;
            AttachTextureLayer(attachment, 0, 0);
            return texture;
        }

        public IRenderBuffer AttachRenderBuffer(
            FramebufferAttachment   attachment, 
            //PixelFormat             format, 
            RenderbufferStorage     internalFormat,
            int                     sampleCount
        )
        {
            var renderbuffer = new RenderBufferGL1(viewport.Width, viewport.Height, internalFormat, sampleCount);
            renderbuffers[attachment] = renderbuffer;
            return renderbuffer as IRenderBuffer;
        }

        public bool Check()
        {
            return true;
        }
        public void Begin()
        {
        }
        public void End()
        {
            if(textures.ContainsKey(FramebufferAttachment.ColorAttachment0) == false)
            {
                return;
            }
            GL.PushAttrib(AttribMask.TextureBit);
            var texture = textures[FramebufferAttachment.ColorAttachment0];
            texture.Apply();
            GL.CopyTexImage2D(
                texture.BindTarget, 
                0,
                texture.InternalFormat,
                0,
                0,
                texture.Size.Width,
                texture.Size.Height,
                0
            );
            GL.PopAttrib();
        }
        public void Blit(IFramebuffer target, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            // \todo
        }
    }
}
