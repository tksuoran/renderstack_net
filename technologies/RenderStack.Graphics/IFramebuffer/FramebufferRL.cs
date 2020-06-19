using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using Caustic.OpenRL;

using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

namespace RenderStack.Graphics
{
    /// \brief Holds GL object name for Framebuffer. 
    /// Allows GL object garbage collection thread to function without GL context.
    /// 
    /// \note Mostly stable.
    public class FramebufferRLGhost : IDisposable
    {
        RLframebuffer framebufferObject;

        public FramebufferRLGhost(RLframebuffer framebufferObject)
        {
            this.framebufferObject = framebufferObject;
        }
        public void Dispose()
        {
            if(framebufferObject != IntPtr.Zero)
            {
                RL.DeleteFramebuffers(ref framebufferObject);
                GhostManager.Delete();
                framebufferObject = IntPtr.Zero;
            }
        }
    }
    /// \brief Abstraction for OpenGL framebuffer
    /// 
    /// \todo [Serializable]
    /// \note Mostly stable, somewhat experimental.
    public class FramebufferRL
    {
        private readonly RenderStack.Math.Viewport  viewport;
        public RenderStack.Math.Viewport            Viewport { get { return viewport; } }

        private RLframebuffer framebufferObject;
        private Dictionary<FramebufferAttachment, TextureRL>        textures        = new Dictionary<FramebufferAttachment,TextureRL>();
        private Dictionary<FramebufferAttachment, RenderBufferGL3>  renderbuffers   = new Dictionary<FramebufferAttachment,RenderBufferGL3>();

        //public RLframebuffer FramebufferObject { get { return framebufferObject; } }

        public TextureRL this[FramebufferAttachment attachment]
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

        bool disposed;
        ~FramebufferRL()
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
                foreach(var kvp in renderbuffers)
                {
                    kvp.Value.Dispose();
                }
                if(framebufferObject != IntPtr.Zero)
                {
                    GhostManager.Add(new FramebufferRLGhost(framebufferObject));
                    framebufferObject = IntPtr.Zero;
                }
                System.GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        void window_Resize(object sender, System.EventArgs e)
        {
            OpenTK.GameWindow window = (OpenTK.GameWindow)(sender);

            viewport.Width = window.Width;
            viewport.Height = window.Height;
        }

        public void Resize(int width, int height)
        {
            foreach(TextureRL texture in textures.Values)
            {
                texture.Resize(width, height);
            }
            foreach(IRenderBuffer renderbuffer in renderbuffers.Values)
            {
                renderbuffer.Resize(width, height);
            }
        }

        public FramebufferRL(int width, int height)
        {
            viewport = new RenderStack.Math.Viewport(width, height);
            RL.GenFramebuffers(1, out framebufferObject);
            GhostManager.Gen();
            RL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
        }

        public void UnbindTexture(
            FramebufferAttachment   attachment,
            TextureTarget           target
        )
        {
            //  \todo query current binding
            RL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            RL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer, 
                attachment,
                target, 
                IntPtr.Zero, 
                0
            );
            //  \todo restore previous binding
        }

        public void AttachTextureLevel(FramebufferAttachment attachment, int level)
        {
            textures[attachment].Apply();
            RL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            textures[attachment].FramebufferTexture2D(
                OpenTK.Graphics.OpenGL.FramebufferTarget.Framebuffer, 
                (OpenTK.Graphics.OpenGL.FramebufferAttachment)attachment,
                OpenTK.Graphics.OpenGL.TextureTarget.Texture2D,
                level
            );
        }
        public void AttachTextureLayer(FramebufferAttachment attachment, int level, int layer)
        {
            throw new InvalidOperationException();
        }

        public TextureRL AttachTexture(
            FramebufferAttachment   attachment, 
            PixelFormat             format, 
            PixelInternalFormat     internalFormat
        )
        {
            TextureRL texture = new TextureRL(viewport.Width, viewport.Height, format, internalFormat);
            textures[attachment] = texture;
            AttachTextureLevel(attachment, 0);
            return texture;
        }

        public bool Check()
        {
            var status = RL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if(status == FramebufferErrorCode.FramebufferComplete)
            {
                return true;
            }
            Trace.TraceError("framebuffer not complete: " + status.ToString());
            //Debugger.Break(); This does not work right yet with Monodevelop
            return false;
        }
        public void Begin()
        {
            RL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
        }
        public void End()
        {
        }
        public void Blit(IFramebuffer target, ClearBufferMask mask, OpenTK.Graphics.OpenGL.BlitFramebufferFilter filter)
        {
            throw new InvalidOperationException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
