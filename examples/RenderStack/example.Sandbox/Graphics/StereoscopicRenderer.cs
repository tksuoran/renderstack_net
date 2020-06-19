//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#define EXTRA_DEBUG

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;
using Debug = RenderStack.Graphics.Debug;

namespace example.Sandbox
{
    public class StereoscopicRenderer : Service
    {
        public override string  Name
        {
            get { return "StereoscopicRenderer"; }
        }

        HighLevelRenderer       highLevelRenderer;
        MainSceneRenderer       mainSceneRenderer;
        MaterialManager         materialManager;
        QuadRenderer            quadRenderer;
        IRenderer               renderer;
        SceneManager            sceneManager;
        OpenTK.GameWindow       window;

        public void Connect(
            HighLevelRenderer   highLevelRenderer,
            MainSceneRenderer   mainSceneRenderer,
            MaterialManager     materialManager,
            IRenderer           renderer,
            SceneManager        sceneManager,
            OpenTK.GameWindow   window
        )
        {
            this.highLevelRenderer = highLevelRenderer;
            this.mainSceneRenderer = mainSceneRenderer;
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.window = window;

            InitializationDependsOn(renderer);
        }

        Material stereoMerge;

        protected override void InitializeService()
        {
            blend = renderer.Programs["Blend"];

            renderer.Resize += new EventHandler<EventArgs>(renderer_Resize);

            stereoMerge = materialManager.MakeMaterial("Stereo", null);
            stereoMerge.FaceCullState = FaceCullState.Disabled;
            stereoMerge.DepthState = DepthState.Disabled;
            stereoMerge.MeshMode = MeshMode.PolygonFill;
            stereoMerge.Floats("t").Set(0.5f);
            stereoMerge.Sync();

            quadRenderer = new QuadRenderer(renderer);
            UpdateQuad();
            CreateViews();
        }

        void renderer_Resize(object sender, EventArgs e)
        {
            UpdateQuad();
            UpdateViews();
        }

        private IFramebuffer[]  views = new IFramebuffer[2];

        void UpdateViews()
        {
            DestroyViews();
            CreateViews();
        }
        void DestroyViews()
        {
            for(int i = 0; i < views.Length; ++i)
            {
                if(views[i] != null)
                {
                    views[i].Dispose();
                    views[i] = null;
                }
            }
        }
        void CreateViews()
        {
            for(int i = 0; i < views.Length; ++i)
            {
                views[i] = FramebufferFactory.Create(window.Width, window.Height);
                views[i].AttachTexture(
                    FramebufferAttachment.ColorAttachment0, 
                    PixelFormat.Rgba, 
                    PixelInternalFormat.Rgb8
                );
                views[i].AttachRenderBuffer(
                    FramebufferAttachment.DepthAttachment, 
                    //PixelFormat.DepthComponent, 
                    RenderbufferStorage.Depth24Stencil8,
                    0
                );
                views[i].Begin();
                views[i].Check();
                views[i].End();
            }
        }

        void UpdateQuad()
        {
            quadRenderer.Begin();
            quadRenderer.Quad(Vector3.Zero, new Vector3(window.Width, window.Height, 0.0f));
            quadRenderer.End();
        }

        private IProgram blend;

        public void Render(StereoMode mode)
        {
#if true
            float[] offset = { -1.0f, 1.0f };
            float stereoSeparation = sceneManager.Camera.Projection.StereoParameters.EyeSeparation[0];
            IFramebuffer framebuffer = views[0];
            for(int viewIndex = 0; viewIndex < 2; ++viewIndex)
            {
                framebuffer = views[viewIndex];

                renderer.Requested.Camera   = sceneManager.Camera;
                renderer.Requested.Viewport = framebuffer.Viewport;
                framebuffer.Begin();
                //framebufferManager.Default.Begin();

                sceneManager.Camera.Projection.StereoParameters.EyeSeparation[0] = offset[viewIndex] * stereoSeparation;
                mainSceneRenderer.Render(viewIndex);
                framebuffer.End();
            }
            sceneManager.Camera.Projection.StereoParameters.EyeSeparation[0] = stereoSeparation;
#endif

#if true
            Debug.WriteLine("=== Render Stereo Combine");

            renderer.Requested.Viewport = highLevelRenderer.WindowViewport;
            renderer.Requested.Camera   = sceneManager.Camera;
            renderer.RenderCurrentClear();

            highLevelRenderer.Use2DCamera();

            renderer.Requested.Mesh     = quadRenderer.Mesh;
            renderer.Requested.Material = stereoMerge;
            renderer.Requested.Program  = (mode != null && mode.Program != null) ? mode.Program : blend;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetFrame(renderer.DefaultFrame);
            renderer.SetTexture("t_left", views[0][FramebufferAttachment.ColorAttachment0]);
            renderer.SetTexture("t_right", views[1][FramebufferAttachment.ColorAttachment0]);
            renderer.RenderCurrent();
#endif
        }

    }
}
