#if false // not in use yet - maybe never
//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;

using RenderStack.Math;
using RenderStack.Graphics;
using RenderStack.Scene;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    public class RenderItem : IComparable<RenderItem>
    {
        public Model    Model;
        public Batch    Batch;
        public float    SortValue;

        public RenderItem(Model model, Batch batch)
        {
            Model = model;
            Batch = batch;
            SortValue = 0.0f;
        }

        public void UpdateSortValue(RenderPass pass)
        {
            Vector3 modelPosition = Model.Frame.LocalToWorld.Matrix.GetColumn3(3);
            Vector3 cameraPosition = pass.Camera.Frame.LocalToWorld.Matrix.GetColumn3(3);
            SortValue = modelPosition.DistanceSquared(cameraPosition);
        }

        public int CompareTo(RenderItem other)
        {
            if(other.Batch.Material.Program.SortValue < Batch.Material.Program.SortValue)
            {
                return -1;
            }
            if(other.Batch.Material.Program.SortValue == Batch.Material.Program.SortValue)
            {
                if(SortValue <  other.SortValue) return -1;
                if(SortValue == other.SortValue) return 0;
                if(SortValue >  other.SortValue) return 1;
            }
            return 1;
        }
    }
    public partial class Model : IDisposable
    {
        private readonly List<Group> groups = new List<Group>();

        public List<Group> Groups { get { return groups; } }
    }

    public partial class RenderPass
    {
        private readonly List<Model>        models      = new List<Model>();
        private readonly List<RenderItem>   renderItems = new List<RenderItem>();

        private static Frame    frame = null;
        private static Camera   camera = null;

        public void CullAndSort()
        {
            //  Assumes all models have been updated
            //  TODO Add culling
            foreach(var item in renderItems)
            {
                item.UpdateSortValue(this);
            }

            //  TODO use custom temporal sorting
            renderItems.Sort();
        }
        public void Render()
        {
            Framebuffer     .Execute();
            Viewport        .Execute();
            BlendState      .Execute();
            FaceCullState   .Execute();
            MaskState       .Execute();
            DepthState      .Execute();
            StencilState    .Execute();
            Clear           .Execute();
            camera.UpdateFrame();
            camera.UpdateViewport(Viewport.Viewport);
            foreach(var item in renderItems)
            {
#if false
                if(item.Model.Frame != frame)
                {
                    camera.UpdateModelFrame(frame);
                }
#endif
                //item.Batch.Mesh.ApplyAttributes(
            }
        }
    }

    public partial class RenderPass
    {
        public SetFramebuffer           Framebuffer     = null;
        public SetViewport              Viewport        = null;
        public Camera                   Camera          = null;

        public List<Model>              Models          = null;
        public List<Camera>             Lights          = null;
        public readonly SortOrder       SortOrder       = SortOrder.NotSet;
        public readonly BlendState      BlendState      = new BlendState();
        public readonly Clear           Clear           = new Clear();
        public readonly FaceCullState   FaceCullState   = new FaceCullState();
        public readonly MaskState       MaskState       = new MaskState();
        public readonly DepthState      DepthState      = new DepthState();
        public readonly StencilState    StencilState    = new StencilState();

        public void Add(Model model)
        {
            foreach(var batch in model.Batches)
            {
                //batches
            }
        }

        public RenderPass(
            Framebuffer framebuffer,
            Viewport    viewport,
            Camera      camera
        )
        {
            Framebuffer = new SetFramebuffer(framebuffer);
            Viewport    = new SetViewport(viewport);
            Camera      = camera;
            Lights      = new List<Camera>();
            Models      = new List<Model>();
            SortOrder   = SortOrder.NoCare;
        }
    }
}
#endif