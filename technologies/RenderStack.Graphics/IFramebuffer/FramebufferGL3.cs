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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Holds GL object name for Framebuffer. 
    /// Allows GL object garbage collection thread to function without GL context.
    /// 
    /// \note Mostly stable.
    public class FramebufferGL3Ghost : IDisposable
    {
        int framebufferObject;
        public FramebufferGL3Ghost(int framebufferObject)
        {
            this.framebufferObject = framebufferObject;
        }
        public void Dispose()
        {
            if(framebufferObject != 0)
            {
                GL.DeleteFramebuffers(1, ref framebufferObject);
                GhostManager.Delete();
                framebufferObject = 0;
            }
        }
    }
    /// \brief Abstraction for OpenGL framebuffer
    /// 
    /// \todo [Serializable]
    /// \note Mostly stable, somewhat experimental.
    public class FramebufferGL3 : IFramebuffer
    {
        private readonly RenderStack.Math.Viewport  viewport;
        public RenderStack.Math.Viewport            Viewport { get { return viewport; } }

        private int framebufferObject;
        private Dictionary<FramebufferAttachment, TextureGL>        textures        = new Dictionary<FramebufferAttachment,TextureGL>();
        private Dictionary<FramebufferAttachment, RenderBufferGL3>  renderbuffers   = new Dictionary<FramebufferAttachment,RenderBufferGL3>();

        //public int FramebufferObject { get { return framebufferObject; } }

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

        bool disposed;
        ~FramebufferGL3()
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
                if(framebufferObject != 0)
                {
                    GhostManager.Add(new FramebufferGL3Ghost(framebufferObject));
                    framebufferObject = 0;
                }
                System.GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        public FramebufferGL3(OpenTK.GameWindow window)
        {
            viewport = new RenderStack.Math.Viewport(window.Width, window.Height);
            window.Resize += new System.EventHandler<System.EventArgs>(window_Resize);
            framebufferObject = 0;
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
            foreach(IRenderBuffer renderbuffer in renderbuffers.Values)
            {
                renderbuffer.Resize(width, height);
            }
        }

        public FramebufferGL3(int width, int height)
        {
            viewport = new RenderStack.Math.Viewport(width, height);
            GL.GenFramebuffers(1, out framebufferObject);
            GhostManager.Gen();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer, 
                attachment,
                target,
                0,
                0
            );
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void AttachCubeFace(
            FramebufferAttachment   attachment, 
            TextureTarget           face,
            int                     level
        )
        {
            textures[attachment].Apply();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            TextureGL texture = textures[attachment];
            texture.FramebufferTexture2D(
                FramebufferTarget.Framebuffer, 
                attachment, 
                face,
                level
            ); 
        }

        public void AttachTextureLevel(FramebufferAttachment attachment, int level)
        {
            textures[attachment].Apply();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            textures[attachment].FramebufferTexture2D(
                FramebufferTarget.Framebuffer, 
                attachment,
                TextureTarget.Texture2D,
                level
            );
        }
        public void AttachTextureLayer(FramebufferAttachment attachment, int level, int layer)
        {
            textures[attachment].Apply();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            textures[attachment].FramebufferTextureLayer(
                FramebufferTarget.Framebuffer, 
                attachment,
                level,
                layer
            );
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
            var renderbuffer = new RenderBufferGL3(viewport.Width, viewport.Height, internalFormat, sampleCount);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
            renderbuffer.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer, 
                attachment, 
                RenderbufferTarget.Renderbuffer
            );
            if(Check() == false)
            {
                return null;
            }
            renderbuffers[attachment] = renderbuffer;
            return renderbuffer as IRenderBuffer;
        }

        public bool Check()
        {
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);

            if(framebufferObject == 0)
            {
                GL.DrawBuffer(DrawBufferMode.Back);
                GL.ReadBuffer(ReadBufferMode.Back);
                return;
            }

            if(
                (renderbuffers.ContainsKey(FramebufferAttachment.ColorAttachment0) == false) &&
                (textures.ContainsKey(FramebufferAttachment.ColorAttachment0) == false) 
            )
            {
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }
            else
            {
                if(
                    textures.ContainsKey(FramebufferAttachment.ColorAttachment1) ||
                    renderbuffers.ContainsKey(FramebufferAttachment.ColorAttachment1)
                )
                {
                    DrawBuffersEnum[] buffers = {
                        DrawBuffersEnum.ColorAttachment0,
                        DrawBuffersEnum.ColorAttachment1
                    };

                    GL.DrawBuffers(2, buffers);
                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                }
                else
                {
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                }
            }
        }
        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.ReadBuffer(ReadBufferMode.Back);
        }
        public void Blit(IFramebuffer target, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            FramebufferGL3 targetGL3 = (FramebufferGL3)target;
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebufferObject);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, targetGL3.framebufferObject);
            GL.BlitFramebuffer(0, 0, viewport.Width, viewport.Height, 0, 0, target.Viewport.Width, target.Viewport.Height, mask, filter);
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            FramebufferTarget target = FramebufferTarget.ReadFramebuffer;

            int objectType;
            int objectName;
            int componentType;
            int colorEncoding;
            int redSize;
            int greenSize;
            int blueSize;
            int alphaSize;
            int depthSize;
            int stencilSize;

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentObjectType,
                out objectType
            );
            sb.Append("ColorAttachment0 Type 0x");
            sb.Append(objectType.ToString("X"));
            sb.Append(" = ");
            sb.Append(System.Enum.GetName(typeof(All), objectType));
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentObjectName,
                out objectName
            );

            sb.Append("ColorAttachment0 ObjectName 0x");
            sb.Append(objectName.ToString("X"));
            sb.Append(" \n");

            {
                FramebufferAttachmentObjectType type = (FramebufferAttachmentObjectType)(objectType);
                if(type == FramebufferAttachmentObjectType.Renderbuffer)
                {
                    int samples;
                    int width;
                    int height;
                    int internalFormat;
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, objectName);
                    GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferSamples,  out samples);
                    GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth,    out width);
                    GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight,   out height);
                    GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferInternalFormat, out internalFormat);
                    sb.Append("ColorAttachment0 RenderBuffer Samples ");
                    sb.Append(samples.ToString());
                    sb.Append(" \n");
                    sb.Append("ColorAttachment0 RenderBuffer Width ");
                    sb.Append(width.ToString());
                    sb.Append(" \n");
                    sb.Append("ColorAttachment0 RenderBuffer Height ");
                    sb.Append(height.ToString());
                    sb.Append(" \n");
                    sb.Append("ColorAttachment0 RenderBuffer InternalFormat 0x");
                    sb.Append(internalFormat.ToString("X"));
                    sb.Append(" = ");
                    sb.Append(System.Enum.GetName(typeof(All), internalFormat));
                    sb.Append(" \n");
                }
            }

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentComponentType,
                out componentType
            );
            sb.Append("ColorAttachment0 ComponentType 0x");
            sb.Append(componentType.ToString("X"));
            sb.Append(" = ");
            sb.Append(System.Enum.GetName(typeof(All), componentType));
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentColorEncoding,
                out colorEncoding
            );
            sb.Append("ColorAttachment0 ColorEncoding 0x");
            sb.Append(colorEncoding.ToString("X"));
            sb.Append(" = ");
            sb.Append(System.Enum.GetName(typeof(All), colorEncoding));
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentRedSize,
                out redSize
            );
            sb.Append("RedSize ");
            sb.Append(redSize.ToString());
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentGreenSize,
                out greenSize
            );
            sb.Append("GreenSize ");
            sb.Append(greenSize.ToString());
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentBlueSize,
                out blueSize
            );
            sb.Append("BlueSize ");
            sb.Append(blueSize.ToString());
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.ColorAttachment0, 
                FramebufferParameterName.FramebufferAttachmentAlphaSize,
                out alphaSize
            );
            sb.Append("AlphaSize ");
            sb.Append(alphaSize.ToString());
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.DepthAttachment, 
                FramebufferParameterName.FramebufferAttachmentObjectType,
                out objectType
            );
            sb.Append("DepthAttachment Type 0x");
            sb.Append(objectType.ToString("X"));
            sb.Append(" = ");
            sb.Append(System.Enum.GetName(typeof(All), objectType));
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.DepthAttachment, 
                FramebufferParameterName.FramebufferAttachmentObjectName,
                out objectName
            );

            sb.Append("DepthAttachment ObjectName 0x");
            sb.Append(objectName.ToString("X"));
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.DepthAttachment, 
                FramebufferParameterName.FramebufferAttachmentComponentType,
                out componentType
            );
            sb.Append("DepthAttachment ComponentType 0x");
            sb.Append(componentType.ToString("X"));
            sb.Append(" = ");
            sb.Append(System.Enum.GetName(typeof(All), componentType));
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.DepthAttachment,
                FramebufferParameterName.FramebufferAttachmentDepthSize,
                out depthSize
            );
            sb.Append("DepthSize ");
            sb.Append(depthSize.ToString());
            sb.Append(" \n");

            GL.GetFramebufferAttachmentParameter(
                target, 
                FramebufferAttachment.StencilAttachment,
                FramebufferParameterName.FramebufferAttachmentStencilSize,
                out stencilSize
            );
            sb.Append("StencilSize ");
            sb.Append(stencilSize.ToString());
            sb.Append(" \n");

            return sb.ToString();
        }
    }
}
