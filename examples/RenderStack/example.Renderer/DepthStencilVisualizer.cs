//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class DepthStencilVisualizer : Service
    {
        public override string Name
        {
            get { return "DepthStencilVisualizer"; }
        }

        MaterialManager materialManager;
        IRenderer       renderer;

        public void Connect(
            MaterialManager materialManager,
            IRenderer       renderer
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;

            InitializationDependsOn(materialManager);
            InitializationDependsOn(renderer);
        }

        protected override void InitializeService()
        {
            renderer.Resize += new EventHandler<EventArgs>(renderer_Resize);

            depthTexture = new TextureGL(renderer.Width, renderer.Height, PixelFormat.Red, PixelInternalFormat.R32f);
            quadRenderer = new QuadRenderer(renderer);
            {
                var m = materialManager.MakeMaterial("VisualizeDepth");
                m.DepthState = DepthState.Disabled;
                m.FaceCullState = FaceCullState.Disabled;
            }

            stencilTexture = new TextureGL(renderer.Width, renderer.Height, PixelFormat.Red, PixelInternalFormat.R8);
            stencilQuadRenderer = new QuadRenderer(renderer);
            {
                var m = materialManager.MakeMaterial("VisualizeStencil");
                m.DepthState = DepthState.Disabled;
                m.FaceCullState = FaceCullState.Disabled;
            }

            depthReadBuffer = new float[extra + renderer.Width * renderer.Height];
            stencilReadBuffer = new byte[extra + renderer.Width * renderer.Height];

        }

        private TextureGL       depthTexture;
        private TextureGL       stencilTexture;
        private QuadRenderer    quadRenderer;
        private QuadRenderer    stencilQuadRenderer;
        private Material        visualizeDepth;
        private Material        visualizeStencil;
        private float[]         depthReadBuffer;
        private byte[]          stencilReadBuffer;
        private int extra = 0;

        void renderer_Resize(object sender, EventArgs e)
        {
            depthTexture.Resize(renderer.Width, renderer.Height);
            stencilTexture.Resize(renderer.Width, renderer.Height);
            depthReadBuffer = new float[extra + renderer.Width * renderer.Height];
            stencilReadBuffer = new byte[extra + renderer.Width * renderer.Height];
            UpdateQuad();
        }
        public void UpdateQuad()
        {
            float w = renderer.Width;
            float h = renderer.Height;
            quadRenderer.Begin();
            quadRenderer.Quad(
                new Vector3(0.5f * w, 0.5f * h, 0.0f),
                new Vector3(       w,        h, 0.0f)
            );
            quadRenderer.End();

            stencilQuadRenderer.Begin();
            stencilQuadRenderer.Quad(
                new Vector3(0.5f * w,        0, 0.0f),
                new Vector3(       w, 0.5f * h, 0.0f)
            );
            stencilQuadRenderer.End();
        }
        public void VisualizeDepth()
        {
            if(quadRenderer != null)
            {
                renderer.SetFrame(renderer.DefaultFrame);
                renderer.Requested.Mesh     = quadRenderer.Mesh;
                renderer.Requested.Material = materialManager["VisualizeDepth"];
                // \todo fixme renderer.Requested.Material.Floats("main_camera_near_far").Set(sceneManager.Camera.Projection.Near, sceneManager.Camera.Projection.Far);
                renderer.Requested.Material.Sync();
                renderer.Requested.Program  = renderer.Requested.Material.Program;
                renderer.Requested.MeshMode = MeshMode.PolygonFill;
                renderer.SetTexture("t_left", depthTexture);
                renderer.RenderCurrent();
            }
            if(stencilQuadRenderer != null)
            {
                renderer.SetFrame(renderer.DefaultFrame);
                renderer.Requested.Mesh     = stencilQuadRenderer.Mesh;
                renderer.Requested.Material = materialManager["VisualizeStencil"];
                renderer.Requested.Program  = renderer.Requested.Material.Program;
                renderer.Requested.MeshMode = MeshMode.PolygonFill;
                renderer.SetTexture("t_left", stencilTexture);
                renderer.RenderCurrent();
            }
        }

        public void GrabStencil()
        {
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.ReadPixels<byte>(0, 0, renderer.Width, renderer.Height, PixelFormat.StencilIndex, PixelType.Byte, stencilReadBuffer);
            stencilTexture.Upload(stencilReadBuffer, 0);
        }
        public void GrabDepth()
        {
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.ReadPixels<float>(0, 0, renderer.Width, renderer.Height, PixelFormat.DepthComponent, PixelType.Float, depthReadBuffer);
            depthTexture.Upload(depthReadBuffer, 0);
        }
        
    }
}