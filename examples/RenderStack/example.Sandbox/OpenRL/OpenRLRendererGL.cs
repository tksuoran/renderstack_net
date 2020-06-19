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

namespace example.Sandbox
{
    public partial class OpenRLRenderer : Service
    {
        public override string Name
        {
            get { return "OpenRLRenderer"; }
        }

        MaterialManager     materialManager;
        QuadRenderer        quadRenderer;
        IRenderer           renderer;
        SceneManager        sceneManager;

        private IProgram    blit;
        private TextureGL   texture;
        private Material    material;

        public  TextureGL   Texture { get { return texture; } }

        public void Connect(
            MaterialManager materialManager,
            IRenderer       renderer,
            SceneManager    sceneManager
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;

            InitializationDependsOn(renderer);
            InitializationDependsOn(materialManager);
        }

        protected override void InitializeService()
        {
            renderer.Resize += new EventHandler<EventArgs>(renderer_Resize);
            quadRenderer = new QuadRenderer(renderer);
            blit = renderer.Programs["VisualizeOpenRL"];
            texture = new TextureGL(128, 128, PixelFormat.Rgba, PixelInternalFormat.Rg32f);
            material = materialManager.MakeMaterial("VisualizeOpenRL");
            material.DepthState = DepthState.Disabled;
            material.FaceCullState = FaceCullState.Disabled;

            UpdateQuad();
            InitializeOpenRL();
        }

        private void UpdateQuad()
        {
            quadRenderer.Begin();
            quadRenderer.Quad(
                new Vector3(renderer.Width - texture.Size.Width, 0, 0.0f),
                new Vector3(renderer.Width, texture.Size.Height, 0.0f)
            );
            quadRenderer.End();
        }

        void renderer_Resize(object sender, EventArgs e)
        {
            UpdateQuad();
        }

        public void Update()
        {
            UpdateRL(sceneManager.Camera, texture.Size);
        }

        public void Visualize()
        {
            if(quadRenderer == null)
            {
                return;
            }

            renderer.SetFrame(renderer.DefaultFrame);
            renderer.Requested.Mesh     = quadRenderer.Mesh;
            renderer.Requested.Material = materialManager["VisualizeOpenRL"];
            renderer.Requested.Program  = blit;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetTexture("t_left", texture);
            renderer.RenderCurrent();
        }

        private unsafe void UpdateGL(IntPtr ptr)
        {
            float* f = (float*)(ptr);
            //System.Console.WriteLine("r: " + *f++ + " g: " + *f++ + " b: " + *f++ + " a: " + *f++);
            texture.Upload(ptr, 0, PixelInternalFormat.Rgba32f, PixelType.Float);
        }

    }
}