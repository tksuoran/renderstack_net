﻿//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

using Buffer    = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;
using Sphere    = RenderStack.Geometry.Shapes.Sphere;

//  TODO:
//      http://www.niksula.hut.fi/~hkankaan/Homepages/bezierfast.html
namespace example.CurveTool
{
    public class CurveTool : Service
    {
        public override string Name
        {
            get { return "CurveTool"; }
        }
        #region Services
        //FramebufferManager          framebufferManager;
        MaterialManager             materialManager;
        IRenderer                   renderer;
        ISceneManager               sceneManager;
        //SelectionManager            selectionManager;
        //UserInterfaceManager        userInterfaceManager;
        //Application                 window;
        #endregion

        private Material            tMaterial;
        private Material            handleMaterial;
        private GeometryMesh        controlHandle;
        private Model               editHandle;
        private Group               handleGroup = new Group("CurveTool.handleGroup");
        private Group               tubeGroup   = new Group("CurveTool.tubeGroup");
        private Group               lineGroup   = new Group("CurveTool.lineGroup");

        private List<CurveSegment>  segments = new List<CurveSegment>();
        private CurveSegment        editSegment;    //  Current edit segment
        private CurveHandle         modifyHandle = null;
        private float               editT = 0.5f;   //  Current editing t, local to current segment
        //private CurveSegment        mouseSegment;   //  Current mouse hover segment
        private float               mouseT = 0.5f;  //  Current mouse t, local to current segment
        private float               scale = 0.4f;
        private IFramebuffer        framebuffer;

        public  MaterialManager     MaterialManager { get { return materialManager; } }
        public  Mesh                HandleMesh      { get { return controlHandle.GetMesh; } }
        public  Material            HandleMaterial  { get { return handleMaterial; } }
        public  Group               HandleGroup     { get { return handleGroup; } }
        public  Group               TubeGroup       { get { return tubeGroup; } }
        public  Group               LineGroup       { get { return lineGroup; } }
        public  Model               EditHandle      { get { return editHandle; } }
        public  bool                LockEditHandle  { get; set; }
        public  bool                ShowHandles     { set { handleGroup.Visible = value; } }
        public  float               MouseT          { get { return mouseT; } }
        public  bool                Enabled = false;

        public Floats TCB = new Floats(0.0f, 0.0f, 0.0f);

        public static float Mix(float a, float b, float t)
        {
            return a * t + (1.0f - t) * b;
        }

        public float EditT
        {
            get
            {
                return editT;
            }
            set
            {
                editT = value;
                CurvePointsToControlHandles();
                MoveEditHandleToCurve();
            }
        }


        //  When curve is updated, edit handle position must
        //  be updated so that it follows the curve.
        private void MoveEditHandleToCurve()
        {
            if(
                (LockEditHandle == false) && 
                (EditHandle != null)
            )
            {
                //EditHandle.Frame.UpdateHierarchical();
                Vector3 P = editSegment.Curve.PositionAt(editT);
                EditHandle.Frame.LocalToParent.SetTranslation(P);
                //EditHandle.Frame.UpdateHierarchical();
            }
        }

        public void Connect(
            MaterialManager materialManager,
            IRenderer       renderer,
            ISceneManager   sceneManager
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;

            InitializationDependsOn(materialManager, renderer, sceneManager);
        }

        void window_Resize(object sender, EventArgs e)
        {
            DestroyFramebuffers();
            CreateFramebuffers();
        }
        private void DestroyFramebuffers()
        {
            if(framebuffer != null)
            {
                framebuffer.Dispose();
                framebuffer = null;
            }
        }
        private void CreateFramebuffers()
        {
            framebuffer = FramebufferFactory.Create(renderer.Width, renderer.Height);
            framebuffer.AttachRenderBuffer(
                FramebufferAttachment.ColorAttachment0, 
                //PixelFormat.RedInteger, 
                RenderbufferStorage.R16f,
                0
            );
            framebuffer.AttachRenderBuffer(
                FramebufferAttachment.DepthAttachment, 
                //PixelFormat.DepthComponent, 
                RenderbufferStorage.DepthComponent32,
                0
            );
            framebuffer.Begin();
            framebuffer.Check();
            framebuffer.End();
        }

        protected override void InitializeService()
        {
            CreateFramebuffers();
            renderer.Resize += new EventHandler<EventArgs>(window_Resize);

            tMaterial = materialManager.MakeMaterial("tMaterial", "TFloat");

            //materialManager.MakeSimpleMaterial("CurveToolControlHandle", 1.0f, 1.0f,1.0f);
            handleMaterial = materialManager.MakeMaterial("CurveToolControlHandle", "Schlick"); 
            float diffuse = 0.5f;
            float specular = 1.0f;
            float roughness = 0.02f;
            float r = 1.0f; float g = 1.0f; float b = 1.0f;
            handleMaterial.Floats("surface_diffuse_reflectance_color"    ).Set(diffuse * r, diffuse * g, diffuse * b);
            handleMaterial.Floats("surface_specular_reflectance_color"   ).Set(specular * r, specular * r, specular *r);
            handleMaterial.Floats("surface_roughness"                    ).Set(roughness);
            handleMaterial.Sync();
        }

        public void Reset()
        {
            controlHandle = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Sphere(
                    scale * 1.1f, 
                    16, 
                    16
                ),
                NormalStyle.PointNormals
            );

            for(int i = 0; i < 1; ++i)
            {
                var segment = new CurveSegment(
                    this, 
                    scale,
                    new Vector3(i * 4.0f, i * 2.0f, 0.0f)
                );
                segments.Add(segment);
                segment.CurvePointsToControlHandles();
            }

            editSegment = segments.First();
            editT = 0.5f;
            editHandle = new Model("EditHandle", controlHandle, handleMaterial);
            //handleGroup.Models.Add(editHandle);

            LineGroup.Visible = Configuration.curveToolLines;
            sceneManager.RenderGroups.Add(lineGroup);
            sceneManager.RenderGroups.Add(tubeGroup);
            sceneManager.ShadowCasterGroups.Add(tubeGroup);
            sceneManager.RenderGroups.Add(handleGroup);
            sceneManager.IdGroups.Add(tubeGroup);
            sceneManager.IdGroups.Add(handleGroup);

            // Without this we can't render without update
            dirty = true;
            Update();
        }

        private bool dirty0 = false;
        private bool dirty { get { return dirty0; } set { dirty0 = value; } }
        private int px;
        private int py;
        public void CheckForUpdates()
        {
            if(modifyHandle != null)
            {
                CurvePointsToControlHandles();
            }
            if(dirty == true)
            {
                Update();
            }
        }
        public void RenderT(int px, int py, Camera camera)
        {
            this.px = px;
            this.py = py;
            RenderStack.Graphics.Debug.WriteLine("=== curveTool.RenderT();");

            renderer.Requested.Viewport = framebuffer.Viewport;
            renderer.Requested.Camera   = camera; // sceneManager.Camera;
            renderer.Requested.Material = tMaterial;
            renderer.Requested.Program  = renderer.Requested.Material.Program;

            framebuffer.Begin();
            camera.UpdateFrame();
            camera.UpdateViewport(renderer.Requested.Viewport);

            renderer.BeginScissorMouse(px, py);
            renderer.PartialGLStateResetToDefaults();
            renderer.CurrentGroup = sceneManager.RenderGroup;

            GL.Viewport(
                (int)renderer.Requested.Viewport.X, 
                (int)renderer.Requested.Viewport.Y, 
                (int)renderer.Requested.Viewport.Width,
                (int)renderer.Requested.Viewport.Height
            );
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.ClearStencil(0);
            GL.Clear(
                ClearBufferMask.ColorBufferBit | 
                ClearBufferMask.DepthBufferBit | 
                ClearBufferMask.StencilBufferBit
            );
            //  \todo use RenderStates
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            foreach(var segment in segments)
            {
                var model = segment.TubeModel;
                renderer.Requested.Mesh     = model.Batch.Mesh;
                renderer.Requested.Material = model.Batch.Material;
                renderer.Requested.Program  = model.Batch.Material.Program;
                renderer.SetFrame(model.Frame);
                renderer.RenderCurrent();
            }
            renderer.EndScissorMouse();

            mouseT = ReadMouseColor();
            framebuffer.End();
        }

        // Returns red color component under mouse 
        private float ReadMouseColor()
        {
            // Framebuffer framebuffer = framebufferManager.FloatRed;

            float[] pixels = new float[4];
            GL.ReadPixels<float>(px, py, 1, 1, PixelFormat.Rgb, PixelType.Float, pixels);

            return pixels[0];
        }

        public void UpdateCurve()
        {
            UpdateControlPoints();
        }
        public void CurvePointsToControlHandles()
        {
            bool change = false;
            foreach(var segment in segments)
            {
                if(segment.CurvePointsToControlHandles())
                {
                    change = true;
                }
            }
            if(change)
            {
                dirty = true;
            }
        }
        public void ControlHandlesToCurve()
        {
            foreach(var segment in segments)
            {
                segment.ControlHandlesToCurve();
            }
        }
        public void UpdateControlPoints()
        {
            foreach(var segment in segments)
            {
                segment.UpdateControlPoints();
            }
        }
        public void MouseClick(Model hoverModel)
        {
            //  Skip update if our old handle is still selected
            if(
                (modifyHandle != null) && 
                (modifyHandle.model != null) && 
                (modifyHandle.model.Selected == true)
            )
            {
                return;
            }
            CurveHandle handle = null;
            foreach(var segment in segments)
            {
                handle = segment.Handle(hoverModel);
                if(handle != null)
                {
                    break;
                }
            }

            modifyHandle = handle;

            if(handle == null)
            {
                return;
            }

            TCB[0] = modifyHandle.Parameters[0];
            TCB[1] = modifyHandle.Parameters[1];
            TCB[2] = modifyHandle.Parameters[2];

#if false // TODO
            if(selectionManager.HoverModel == TubeModel)
            {
                //  Move EditT to new position
                bool locked = LockEditHandle;
                LockEditHandle = false;
                EditT = MouseT;
                selectionManager.ClearSelection();
                selectionManager.Add(EditHandle);
                Update();
                LockEditHandle = locked;
            }
#endif
        }
        public void Update()
        {
            if(dirty == false)
            {
                return;
            }
            if(modifyHandle != null)
            {
                modifyHandle.Parameters[0] = TCB[0];
                modifyHandle.Parameters[1] = TCB[1];
                modifyHandle.Parameters[2] = TCB[2];
            }

            UpdateControlPoints();

            //MoveEditHandleToCurve();

            foreach(var segment in segments)
            {
                segment.Update();
            }
            dirty = false;
        }
    }
}
