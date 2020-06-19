//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

using example.Brushes;
using example.Renderer;

namespace example.Sandbox
{
    public class Palette : Area
    {
        // Services
        Brushes.BrushManager        brushManager;
        MaterialManager             materialManager;
        IRenderer                   renderer;

        private List<Brushes.Brush> brushes;
        private NinePatch           ninePatch;
        private bool                active          = false;
        private Group               brushModelGroup = new Group("brushModelGroup");
        private float               brushSize       = 40.0f;
        private Material            material;
        private Group               hoverGroup      = new Group("Palette.hoverGroup");
        private Camera              camera          = new Camera();

        public bool                 Active          { get { return active; } set { active = value; } }
        public Group                BrushModelGroup { get { return brushModelGroup; } }
        public Camera               Camera          { get { return camera; } }

        public Palette(
            Brushes.BrushManager    brushManager,
            MaterialManager         materialManager,
            IRenderer               renderer
        )
        {
            this.brushManager = brushManager;
            this.materialManager = materialManager;
            this.renderer = renderer;

            this.ninePatch = new NinePatch(Style.NinePatchStyle);

            material = materialManager["Palette"];

            brushes = brushManager.Lists["platonic"];
            foreach(Brush brush in brushes)
            {
                brushModelGroup.Add(brush.Model);
            }

            rects = new Rectangle[brushes.Count];
            for(int i = 0; i < brushes.Count; ++i)
            {
                rects[i] = new Rectangle();
            }

            FillBasePixels = new Vector2(brushSize,  brushes.Count * brushSize);
        }

        private Rectangle[] rects;

        private Brushes.Brush BrushAt(Vector2 position)
        {
            if(rects == null)
            {
                return null;
            }
            for(int i = 0; i < rects.Length; ++i)
            {
                if(rects[i].Hit(position))
                {
                    return brushes[i];
                }
            }
            return null;
        }

        public void Animate()
        {
            Vector2 cellSize    = new Vector2(brushSize, brushSize);
            Vector2 halfCell    = cellSize * 0.5f;
            Vector2 origo       = new Vector2(Rect.Min.X, Rect.Min.Y) + halfCell;
            float   scale       = (brushSize - 10.0f) / 2.0f;

            Vector3 brushCenter = new Vector3(origo, 0.0f);
            List<Brush> brushes = brushManager.Lists["platonic"];
            int i = 0;
            foreach(Brush brush in brushes)
            {
                //  Bypassing parent hierarchy to avoid need to update
                brush.Model.Frame.LocalToWorld.Set(
                    Matrix4.CreateTranslation(brushCenter.X, brushCenter.Y, brushCenter.Z) 
                    * Matrix4.CreateScale(scale / brush.BoundingBox.HalfSize.MaxAxis.Length/* bounding sphere Radius */)
                    * Matrix4.CreateRotation((float)Math.PI * 0.1f, Vector3.UnitX)
                    * Matrix4.CreateRotation(Time.Now, Vector3.UnitY)
                );

                rects[i].Min = brushCenter.Xy - halfCell;
                rects[i].Max = brushCenter.Xy + halfCell;

                // Advance to next cell slot
                brushCenter.X += cellSize.X;
                ++i;
                if(brushCenter.X + halfCell.X > Rect.Max.X)
                {
                    brushCenter.X = origo.X;
                    brushCenter.Y += cellSize.Y;
                }
            }
        }

        public override void DrawSelf(IUIContext context)
        {
            renderer.Push();

            hoverGroup.Clear();
            if(Rect.Hit(context.Mouse))
            {
                var hoverBrush = BrushAt(context.Mouse);
                if(hoverBrush != null)
                {
                    hoverGroup.Add(hoverBrush.Model);
                    if(context.MouseButtons[(int)(OpenTK.Input.MouseButton.Left)])
                    {
                        brushManager.CurrentBrush = BrushAt(context.Mouse);
                    }
                }
            }

            Animate();

            renderer.LockMaterial = true;
            var mat = renderer.Requested.Material;
            float width = 1.0f;
            mat.Floats("line_width").Set(width, width * width * 0.25f);
            mat.Floats("line_color").Set(0.5f, 0.5f, 0.6f, 1.0f);
            mat.Sync();
            renderer.Requested.Program = renderer.Programs["WideLineUniformColor"];
            renderer.Requested.MeshMode = MeshMode.EdgeLines; //material.MeshMode;
            renderer.CurrentGroup = brushModelGroup;
            renderer.RenderGroupInstances(renderer.CurrentGroup.AllInstances);
            if(hoverGroup.Models.Count > 0)
            {
                width = 2.0f;
                mat.Floats("line_width").Set(width, width * width * 0.25f);
                mat.Floats("line_color").Set(1.0f, 1.0f, 1.0f, 1.0f);
                mat.Sync();
                renderer.CurrentGroup = hoverGroup;
                renderer.RenderGroupInstances(renderer.CurrentGroup.AllInstances);
            }

            renderer.LockMaterial = false;
            renderer.Pop();
            renderer.UpdateCamera();
            renderer.SetFrame(renderer.DefaultFrame);
        }

    }
}
