#if false
//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public partial class FramebufferManager : Service, IDisposable
    {
        public override string Name
        {
            get { return "FramebufferManager"; }
        }

        IRenderer               renderer;
        OpenTK.GameWindow       window;

        private IFramebuffer    @default;
        private IFramebuffer    linear;
        private IFramebuffer    multisampleResolve;

        public IFramebuffer             Default             { get { return @default; } }
        //public Framebuffer            Linear              { get { return linear; } }
        //public Framebuffer            MultisampleResolve  { get { return multisampleResolve; } }

        public void Connect(
            IRenderer           renderer,
            OpenTK.GameWindow   window
        )
        {
            this.window = window;
            this.renderer = renderer;
            InitializationDependsOn(renderer);
            initializeInMainThread = true;
        }

#if false
        public void ClearShadowMaps()
        {
            GL.Viewport(Shadow.X, Shadow.Y, Shadow.Width, Shadow.Height);
            GL.Disable(EnableCap.ScissorTest);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            Shadow.Begin();
            for(int i = 0; i < Configuration.lightCount; ++i)
            {
                Shadow.AttachTextureLayer(FramebufferAttachment.ColorAttachment0, 0, i);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
            // TODO this is a hack, we need to figure out how to properly manage viewport
            Shadow.End();
            GL.Viewport(0, 0, window.Width, window.Height);
        }
#endif

        public void CleanupWindowSizeResources()
        {
            if(linear != null)
            {
                linear.Dispose();
                linear = null;
            }
            if(multisampleResolve != null)
            {
                multisampleResolve.Dispose();
                multisampleResolve = null;
            }
        }

        public void CreateWindowSizeResources()
        {
            System.Diagnostics.Debug.WriteLine("CreateWindowSizeResources()");
            @default = FramebufferFactory.Create(window);

            if(Configuration.gammaCorrect)
            {
                bool sRgbEnable = GL.IsEnabled(EnableCap.FramebufferSrgb);

                linear = FramebufferFactory.Create(window.Width, window.Height);

                int samples = 4;

                linear.AttachRenderBuffer(
                    FramebufferAttachment.ColorAttachment0, 
                    PixelFormat.Rgb,
                    RenderbufferStorage.Rgb32f,
                    samples
                );
                linear.AttachRenderBuffer(
                    FramebufferAttachment.DepthAttachment, 
                    PixelFormat.DepthComponent, 
                    RenderbufferStorage.DepthComponent32,
                    samples
                );
                linear.Begin();
                linear.Check();
                linear.End();

                multisampleResolve = FramebufferFactory.Create(window.Width, window.Height);
                multisampleResolve.AttachTexture(
                    FramebufferAttachment.ColorAttachment0, 
                    PixelFormat.Rgb, 
                    PixelInternalFormat.Rgb32f
                );
                multisampleResolve.Begin();
                multisampleResolve.Check();
                multisampleResolve.End();
            }
            System.Diagnostics.Debug.WriteLine("CreateWindowSizeResources() done");
        }

        //  http://developer.dev.nvidia.com/node/35?display=default
        //  http://read.pudn.com/downloads105/sourcecode/windows/opengl/433841/DEMOS/OpenGL/src/simple_soft_shadows/simple_soft_shadows.cpp__.htm
#if false // \bug OpenGL spec file or OpenTK bug
        private void CreateJitterTexture()
        {
            List<Vector2> samples = null;

            Random  random          = new Random();
            int     jitterSize      = 32;
            int     jitterSamples   = 64;
            int     width           = jitterSize;
            int     height          = jitterSize;
            //int     depth           = jitterSamples * jitterSamples / 2;
            int     depth           = jitterSamples / 2;
            int     texelPitch      = 4;
            int     rowPitch        = width * texelPitch;
            int     slicePitch      = rowPitch * height;
            byte[]  data            = new byte[width * height * depth * texelPitch];

            Texture texture = new Texture(
                width, 
                height, 
                depth, PixelFormat.Rgba, 
                PixelInternalFormat.SignedRgba8
            );

            double[] v = new double[4];
            double[] d = new double[4];
            float startDistance = 0.5f;
            for(int i = 0; i < jitterSize; ++i)
            {
                for(int j = 0; j < jitterSize; ++j)
                {
                    double rot_offset = random.NextDouble() * 2 * 3.1415926;

                    float distance = startDistance;
                    for(int count = 0; count < jitterSamples; distance = distance * 0.95f)
                    {
                        samples = UniformPoissonDiskSampler.SampleCircle(Vector2.Zero, 1.0f, distance);
                        count = samples.Count;
                    }
                    startDistance = distance / 0.95f;

                    for(int k = 0; k < jitterSamples / 2; ++k)
                    {
                        data[k * slicePitch + j * rowPitch + i * 4 + 0] = (byte)(samples[k * 2 + 0].X * 255);
                        data[k * slicePitch + j * rowPitch + i * 4 + 1] = (byte)(samples[k * 2 + 0].Y * 255);
                        data[k * slicePitch + j * rowPitch + i * 4 + 2] = (byte)(samples[k * 2 + 1].X * 255);
                        data[k * slicePitch + j * rowPitch + i * 4 + 3] = (byte)(samples[k * 2 + 1].Y * 255);
                    }

#if false
                    for(int k = 0; k < jitterSamples * jitterSamples / 2; ++k)
                    {
                        int x, y;

                        x = k % (jitterSamples / 2);
                        y = (jitterSamples - 1) - k / (jitterSamples / 2);

                        v[0] = (double)(x * 2 + 0.5) / jitterSamples;
                        v[1] = (double)(y + 0.5) / jitterSamples;
                        v[2] = (double)(x * 2 + 1 + 0.5) / jitterSamples;
                        v[3] = v[1];

                        // jitter
                        v[0] += random.NextDouble() * 2 / jitterSamples;
                        v[1] += random.NextDouble() * 2 / jitterSamples;
                        v[2] += random.NextDouble() * 2 / jitterSamples;
                        v[3] += random.NextDouble() * 2 / jitterSamples;

                        // warp to disk
                        d[0] = Math.Sqrt(v[1]) * Math.Cos(2 * 3.1415926 * v[0] + rot_offset);
                        d[1] = Math.Sqrt(v[1]) * Math.Sin(2 * 3.1415926 * v[0] + rot_offset);
                        d[2] = Math.Sqrt(v[3]) * Math.Cos(2 * 3.1415926 * v[2] + rot_offset);
                        d[3] = Math.Sqrt(v[3]) * Math.Sin(2 * 3.1415926 * v[2] + rot_offset);

                        d[0] = (d[0] + 1.0) / 2.0;
                        d[1] = (d[1] + 1.0) / 2.0;
                        d[2] = (d[2] + 1.0) / 2.0;
                        d[3] = (d[3] + 1.0) / 2.0;
                        data[k * slicePitch + j * rowPitch + i * 4 + 0] = (byte)(d[0] * 255);
                        data[k * slicePitch + j * rowPitch + i * 4 + 1] = (byte)(d[1] * 255);
                        data[k * slicePitch + j * rowPitch + i * 4 + 2] = (byte)(d[2] * 255);
                        data[k * slicePitch + j * rowPitch + i * 4 + 3] = (byte)(d[3] * 255);
                    }
#endif
                }
            }

            texture.Upload(data, 0);
            texture.Wrap = TextureWrapMode.Repeat;

            renderer.GlobalParameters["jittermap"] = texture;
        }
#endif

        protected override void InitializeService()
        {
            //CreateJitterTexture(); not in use
            CreateWindowSizeResources();
        }

        public void Cleanup()
        {
            CleanupWindowSizeResources();
        }

        public void Dispose()
        {
            Cleanup();
        }

    }
}
#endif