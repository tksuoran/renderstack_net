using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Caustic.OpenRL;
using OpenRLContext = System.IntPtr;
using OpenRLContextAttribute = System.IntPtr;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public partial class OpenRLRenderer : Service
    {
        private RLtexture       mainFramebufferTexture;
        private RLframebuffer   mainFramebuffer;
        private RLbuffer        pixelBuffer;

        private ProgramRL       frameProgram;
        private ProgramRL       primitiveProgram;

        private void InitializeOpenRL()
        {
            // Create the OpenRL context and make it current

            RL.GenBuffers(1, out pixelBuffer);
            RL.GenTextures(1, out mainFramebufferTexture);

            CreateFramebuffer();

            // Tell OpenRL the size and location of the image we will render into the framebuffer.
            RL.Viewport(0,  0,  texture.Size.Width,  texture.Size.Height);

            System.Text.StringBuilder uniforms = new System.Text.StringBuilder();
            uniforms.Append(CameraUniformBlock.UniformBlockRL.SourceRL);

            ShaderRL.Replace("UNIFORMS;", uniforms.ToString());

            frameProgram = ProgramRL.Load("simpleFrame");
            primitiveProgram = ProgramRL.Load("simplePrimitive");

            ErrorCode code = RL.GetError();
        }

        private void CreateFramebuffer()
        {
            // Create the buffer to copy the rendered image into
            RL.BindBuffer(BufferTarget.PixelPackBuffer, pixelBuffer);
            RL.BufferData(BufferTarget.PixelPackBuffer, texture.Size.Width * texture.Size.Height * 4 * sizeof(float), System.IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Create the framebuffer texture
            RL.BindTexture(TextureTarget.Texture2D,  mainFramebufferTexture);

            // We have no texture data to specify so we pass NULL for texture data.
            RL.TexImage2D(TextureTarget.Texture2D,  0,  PixelInternalFormat.Rgba, texture.Size.Width, texture.Size.Height, 0, PixelFormat.Rgba, PixelType.Float, System.IntPtr.Zero);

            // Create the framebuffer object to render to
            // and attach the texture that will store the rendered image.
            RL.GenFramebuffers(1, out mainFramebuffer);
            RL.BindFramebuffer(FramebufferTarget.Framebuffer, mainFramebuffer);
            RL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,  
                FramebufferAttachment.ColorAttachment0,  
                TextureTarget.Texture2D,
                mainFramebufferTexture,  
                0
            );

            FramebufferErrorCode ok = RL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        }
        private void DeleteFramebuffer()
        {
            if(mainFramebuffer != System.IntPtr.Zero)
            {
                RL.DeleteFramebuffers(ref mainFramebuffer);
                mainFramebuffer = System.IntPtr.Zero;
            }
        }

        private Dictionary<Model, Primitive> primitives = new Dictionary<Model,Primitive>();

        public void BuildScene(Group group)
        {
            if(RenderStack.Graphics.Configuration.useOpenRL == false)
            {
                return;
            }

            foreach(var kvp in primitives)
            {
                kvp.Value.Dispose();
            }
            primitives.Clear();

            foreach(var model in group.Models)
            {
                var primitive = new Primitive(model, primitiveProgram);
                primitives[model] = primitive;
            }

            RL.BindFramebuffer(FramebufferTarget.Framebuffer, mainFramebuffer);
            RL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            RL.Clear(ClearBufferMask.ColorBufferBit);
            RL.BindPrimitive(PrimitiveTarget.Primitive, 0);
        }

        public void UpdateRL(Camera camera, Viewport viewport)
        {
            foreach(var primitive in primitives)
            {
                primitive.Value.Update();
            }
            RL.BindFramebuffer(FramebufferTarget.Framebuffer, mainFramebuffer);
            RL.Clear(ClearBufferMask.ColorBufferBit);

            RL.BindPrimitive(PrimitiveTarget.Primitive, 0);
            frameProgram.Use(0);
            camera.Frame.UpdateHierarchicalNoCache();
            camera.UpdateFrame();
            camera.UpdateViewport(texture.Size);
            camera.UniformBufferRL.Sync();
            camera.UniformBufferRL.UseRL(frameProgram);

            RL.RenderFrame();

            RL.Clear(ClearBufferMask.ColorBufferBit);
            RL.RenderFrame();

            RL.BindBuffer(BufferTarget.PixelPackBuffer, pixelBuffer);
            RL.BindTexture(TextureTarget.Texture2D, mainFramebufferTexture);
            RL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, System.IntPtr.Zero);
            {
                IntPtr ptr = RL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
                if(ptr != IntPtr.Zero)
                {
                    UpdateGL(ptr);
                }
                RL.UnmapBuffer(BufferTarget.PixelPackBuffer);
            }
            RL.BindBuffer(BufferTarget.PixelPackBuffer, System.IntPtr.Zero);
        }
    }
}
