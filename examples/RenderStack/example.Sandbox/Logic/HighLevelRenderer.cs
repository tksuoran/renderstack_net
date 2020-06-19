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
using example.VoxelRenderer;

using Buffer = RenderStack.Graphics.BufferGL;
using Debug = RenderStack.Graphics.Debug;

namespace example.Sandbox
{
    public partial class HighLevelRenderer : Service
    {
        public override string  Name
        {
            get { return "HighLevelRenderer"; }
        }

        CurveTool.CurveTool         curveTool;
        IDRenderer                  idRenderer;
        LineRenderer                lineRenderer;
        MainSceneRenderer           mainSceneRenderer;
        ManipulatorManager          manipulatorManager;
        Map                         map;
        MaterialManager             materialManager;
        QuadRenderer                quadRenderer;
        SceneManager                sceneManager;
        SelectionManager            selectionManager;
        ShadowRenderer              shadowRenderer;
        StereoscopicRenderer        stereoscopicRenderer;
        UserInterfaceManager        userInterfaceManager;
        IRenderer                   renderer;
        VoxelEditor                 voxelEditor;
        OpenTK.GameWindow           window;

        private bool                resize = false;
        private Camera              camera2D;
        private Timers              timers;
        private Viewport            windowViewport;
        public  Camera              Camera2D        { get { return camera2D; } }
        public  Timers              Timers          { get { return timers; } }
        public  Viewport            WindowViewport  { get { return windowViewport; } }

        public void Connect(
            CurveTool.CurveTool     curveTool,
            IDRenderer              idRenderer,
            LineRenderer            lineRenderer,
            MainSceneRenderer       mainSceneRenderer,
            ManipulatorManager      manipulatorManager,
            Map                     map,
            MaterialManager         materialManager,
            IRenderer               renderer,
            SceneManager            sceneManager,
            SelectionManager        selectionManager,
            ShadowRenderer          shadowRenderer,
            StereoscopicRenderer    stereoscopicRenderer,
            UserInterfaceManager    userInterfaceManager,
            VoxelEditor             voxelEditor,
            OpenTK.GameWindow       window
        )
        {
            this.curveTool = curveTool;
            this.idRenderer = idRenderer;
            this.lineRenderer = lineRenderer;
            this.mainSceneRenderer = mainSceneRenderer;
            this.manipulatorManager = manipulatorManager;
            this.map = map;
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.selectionManager = selectionManager;
            this.shadowRenderer = shadowRenderer;
            this.stereoscopicRenderer = stereoscopicRenderer;
            this.userInterfaceManager = userInterfaceManager;
            this.voxelEditor = voxelEditor;
            this.window = window;

            InitializationDependsOn(map);
            InitializationDependsOn(materialManager);
        }

        protected override void InitializeService()
        {
            // \todo fix
            quadRenderer = new QuadRenderer(renderer);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            window.SwapBuffers();

            windowViewport = new Viewport(window.Width, window.Height);
            renderer.Requested.Viewport = windowViewport;

            camera2D = new Camera(); 
            camera2D.Name = "camera2D";
            Update2DCamera();

            if(Configuration.voxelTest)
            {
                map.UpdateChunksAround(0, 0);
                map.UpdateRender();
            }

            timers = new Timers();

            InitializeCommonMaterials();
        }

        public void InitializeCommonMaterials()
        {
            materialManager.MakeSchlick("Schlick");

            materialManager.MakeAnisotropic("Tube", 1.0f, 1.0f, 1.0f, 0.025f, 0.3f);
            try
            {
                materialManager.MakeMaterial("TFloat");
            }
            catch(System.Exception)
            {
            }

            var grid = materialManager.MakeMaterial("Grid");
            grid.Floats("grid_size"                             ).Set(1.0f, 1.0f);
            grid.Floats("surface_diffuse_reflectance_color"     ).Set(0.44f, 0.44f, 0.44f);
            grid.Floats("surface_specular_reflectance_color"    ).Set(1.0f, 1.0f, 1.0f);
            grid.Floats("surface_specular_reflectance_exponent" ).Set(100.0f);
            grid.Floats("surface_roughness"                     ).Set(0.02f);
            grid.Sync();

            if(Configuration.minimal == false)
            {
                materialManager.MakeMaterial("WideLineUniformColor");

                {
                    Image whiteImage = new Image(16, 16, 1.0f, 1.0f, 1.0f, 1.0f);
                    var white = materialManager.Textures["White"] = new TextureGL(whiteImage, true);
                }

                //  ColorFill uses vertex colors
                materialManager.MakeMaterial("EdgeLines", "ColorFill").MeshMode = MeshMode.EdgeLines;
                materialManager.MakeMaterial("ColorFill");

                var hsv = materialManager.MakeMaterial("HSVColorFill");
                var hsv2 = materialManager.MakeMaterial("HSVColorFill2");

                {
                    var m = materialManager.MakeMaterial("noisy", "SchlickNoise");
                    m.Floats("surface_diffuse_reflectance_color"    ).Set(1.0f, 0.6f, 0.01f);
                    m.Floats("surface_specular_reflectance_color"   ).Set(1.0f, 0.8f, 0.01f);
                    m.Floats("surface_roughness"                    ).Set(0.03f);
                    m.Sync();
                }

                materialManager.MakeSimpleMaterial("pearl",   1.00f, 0.80f, 0.80f, 0.8f,  0.2f, 0.90f);
                materialManager.MakeSimpleMaterial("gold",    1.00f, 0.60f, 0.01f, 0.6f,  0.4f, 0.03f);
                materialManager.MakeSimpleMaterial("red",     1.00f, 0.05f, 0.00f, 0.4f,  0.4f, 0.04f);
                materialManager.MakeSimpleMaterial("green",   0.05f, 1.00f, 0.15f, 0.05f, 0.3f, 0.005f);
                materialManager.MakeSimpleMaterial("cyan",    0.05f, 0.80f, 1.00f, 0.2f,  0.4f, 0.80f);
                materialManager.MakeSimpleMaterial("blue",    0.15f, 0.20f, 0.80f, 0.4f,  1.0f, 0.01f);
                materialManager.MakeSimpleMaterial("magenta", 1.00f, 0.05f, 1.00f, 0.4f,  1.0f, 0.02f);
                materialManager.MakeSimpleMaterial("pink",    1.00f, 0.33f, 0.66f, 0.6f,  0.1f, 0.01f);
                materialManager.MakeSimpleMaterial("wood",    0.40f, 0.33f, 0.06f, 0.6f,  0.1f, 0.4f);
                materialManager.MakeSimpleMaterial("leaves",  0.10f, 0.50f, 0.20f, 0.5f,  0.5f, 0.1f);

                var transparent = materialManager.MakeMaterial("transparent", "Rim");
                transparent.Floats("fill_color").Set(0.05f, 0.05f, 1.0f);
                transparent.Sync();
                transparent.BlendState = new BlendState();
                transparent.BlendState.Enabled = true;
                transparent.BlendState.RGB.EquationMode = BlendEquationMode.FuncAdd;
                transparent.BlendState.RGB.SourceFactor = BlendingFactorSrc.OneMinusSrcAlpha;
                transparent.BlendState.RGB.DestinationFactor = BlendingFactorDest.SrcAlpha;
                transparent.BlendState.Alpha.EquationMode = BlendEquationMode.FuncAdd;
                transparent.BlendState.Alpha.SourceFactor = BlendingFactorSrc.One;
                transparent.BlendState.Alpha.DestinationFactor = BlendingFactorDest.One;
                transparent.BlendState.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                transparent.MaskState = new MaskState();
                transparent.MaskState.Depth = false;

                var paletteMaterial = materialManager.MakeMaterial("Palette", "manipulator");
                paletteMaterial.Floats("surface_diffuse_reflectance_color").Set(1.0f, 1.0f, 1.0f);
                paletteMaterial.Sync();
            }
        }

        private void Update2DCamera()
        {
            camera2D.Frame.UpdateHierarchical(sceneManager.UpdateSerial);
            camera2D.Projection.ProjectionType = ProjectionType.OrthogonalRectangle;
            camera2D.Projection.OrthoLeft      = 0;
            camera2D.Projection.OrthoTop       = 0;
            camera2D.Projection.OrthoWidth     = window.Width;
            camera2D.Projection.OrthoHeight    = window.Height;
            camera2D.Projection.Near           = -1000.0f;
            camera2D.Projection.Far            =  1000.0f;
            camera2D.UpdateFrame();
            camera2D.UpdateViewport(windowViewport);
        }
        public void Use2DCamera()
        {
            //  2D camera is not updated by SceneManager.Update()
            Camera2D.Frame.UpdateHierarchical(sceneManager.UpdateSerial);

            renderer.Requested.Camera = Camera2D;
            renderer.UpdateCamera();
        }

        private void HandleResize()
        {
            if(resize)
            {
                windowViewport.Width = window.Width;
                windowViewport.Height = window.Height;
                Update2DCamera();

                renderer.HandleResize();

                resize = false;
            }
        }

        public void Resize(int w, int h)
        {
            resize = true;
        }

        #region Profiling
        #endregion

        public void BeginFrame()
        {
            AMDGPUPerf.Instance.BeginFrame();
            Timer.BeginFrame();
        }
        public void EndFrame()
        {
            AMDGPUPerf.Instance.EndFrame();
            Timer.EndFrame();
        }
        public void Render()
        {
            int px, py;
            MouseInScreen(out px, out py);

            Debug.WriteLine("=== Render");

            HandleResize();

            renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            renderer.Global.Floats("alpha").Set(1.0f);
            renderer.Global.Sync();

            // \todo LineRenderer should have Clear()
            if(lineRenderer != null)
            {
                lineRenderer.Begin();
                lineRenderer.End();
            }

            if(true)
            {
                #region Id
                if(
                    (selectionManager != null) &&
                    (RuntimeConfiguration.disableReadPixels == false)
                )
                {
                    if(Configuration.idBuffer && (idRenderer != null))
                    {
                        using(var t = new TimerScope(timers.ID))
                        {
                            idRenderer.Render(px, py);
                            selectionManager.ProcessIdBuffer(idRenderer.Hover, px, py);
                        }
                    }

                    selectionManager.Update(px, py);
                }
                #endregion
                #region Shadowmaps
                if(sceneManager.UpdateShadowMap && (RenderStack.Graphics.Configuration.useGl1 == false))
                {
                    using(var t = new TimerScope(timers.Shadow))
                    {
                        shadowRenderer.UpdateShadowMaps();
                    }
                }
                #endregion
                #region CurveTool RenderT
                if((curveTool != null) && curveTool.Enabled)
                {
                    if(RenderStack.Graphics.Configuration.useGl1 == false)
                    {
                        curveTool.RenderT(px, py, sceneManager.Camera);
                    }
                    curveTool.CheckForUpdates();
                }
                #endregion
                #region Main scene mono/stereo
                using (var t = new TimerScope(timers.Render3D))
                {
                    renderer.PartialGLStateResetToDefaults();
                    if(RenderStack.Graphics.Configuration.useGl1 == false)
                    {
                        renderer.SetTexture("t_shadowmap", shadowRenderer.ShadowMap);
                    }

                    StereoMode mode = (userInterfaceManager != null) ? userInterfaceManager.CurrentStereoMode : null;
                    if((mode != null) && (mode.Program != null) && (mode.Program.Valid == true))
                    {
                        stereoscopicRenderer.Render(mode);
                    }
                    else
                    {
                        renderer.Requested.Viewport = windowViewport;
                        renderer.Requested.Camera   = sceneManager.Camera;
                        renderer.Requested.Viewport = windowViewport;
                        if(RenderStack.Graphics.Configuration.canUseFramebufferObject)
                        {
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        }
                        mainSceneRenderer.Render(0);
                    }
                }
                #endregion
                #region voxelTest
                if (Configuration.voxelTest)
                {
                    voxelEditor.RenderCubes(px, py, sceneManager.Camera);
                }
                #endregion
            }
            else
            {
                #region Alternative path without normal 3d scene
                // path without normal 3d scene
                renderer.Requested.Viewport = windowViewport;
                renderer.Requested.Camera   = sceneManager.Camera;
                renderer.Requested.Viewport = windowViewport;
                if(RenderStack.Graphics.Configuration.canUseFramebufferObject)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }
                renderer.RenderCurrentClear();
                renderer.ApplyViewport();
                #endregion
            }

            renderer.Requested.Viewport = windowViewport;
            renderer.Requested.Camera   = sceneManager.Camera;
            renderer.Requested.Viewport = windowViewport;
            renderer.ApplyViewport();

            if(RuntimeConfiguration.debugInfo && Configuration.graphicalUserInterface)
            {
                using(var t = new TimerScope(timers.GUI))
                {
                    userInterfaceManager.Render();
                }
            }

            window.SwapBuffers();
        }
        public void MouseInScreen(out int px, out int py)
        {
            px = window.Mouse.X;
            py = window.Height - 1 - window.Mouse.Y;
        }
    }
}
