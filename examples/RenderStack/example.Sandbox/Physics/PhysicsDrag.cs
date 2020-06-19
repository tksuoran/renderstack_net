//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Linq;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Physics;
using RenderStack.Scene;
using RenderStack.Services;
using RenderStack.UI;

using example.Renderer;

using Material = example.Renderer.Material;

namespace example.Sandbox
{
    public class PhysicsDrag : Service
    {
        public override string Name
        {
            get { return "PhysicsDrag"; }
        }

        LineRenderer            lineRenderer;
        SelectionManager        selectionManager;
        SceneManager            sceneManager;
        Application             window;

        private Model           model;
        private float           lockDistance;
        private PointOnPoint    constraint;
        private Vector3         snapInLocal;
        //private RenderStack.Mesh.Material materialStore;
        private Material        materialStore;

        public Model Model { get { return model; } }

        public void Connect(
            LineRenderer        lineRenderer,
            SelectionManager    selectionManager,
            SceneManager        sceneManager,
            Application         window
        )
        {
            this.lineRenderer = lineRenderer;
            this.selectionManager = selectionManager;
            this.sceneManager = sceneManager;
            this.window = window;
        }
        protected override void InitializeService()
        {
        }
        public void Begin()
        {
            if(
                (Configuration.physics == false) ||
                (selectionManager == null) ||
                (selectionManager.HoverModel == null) ||
                (selectionManager.HoverModel.RigidBody == null) ||
                (selectionManager.HoverModel.Static == true)
            )
            {
                return;
            }

            model = selectionManager.HoverModel;
            materialStore = model.Batch.Material;
            var materialManager = Services.Get<MaterialManager>();
            model.Batch.Material = materialManager["EdgeLines"];
            sceneManager.UnparentModel(model);
            Frame       frame = model.Frame;
            RigidBody   body = model.RigidBody;
            //sceneManager.ActivateModel(model);
            body.IsStatic = false;
            body.IsActive = true;
            body.AllowDeactivation = false;
            frame.Updated = false;
            //frame.UpdateHierarchical();
            Vector3 snap        = selectionManager.HoverPosition;
            Vector3 camera      = sceneManager.Camera.Frame.LocalToWorld.Matrix.GetColumn3(3);
            Vector3 direction   = Vector3.Normalize(snap - camera);
            lockDistance = camera.Distance(snap);

            snapInLocal = model.Frame.LocalToWorld.InverseMatrix.TransformPoint(snap);
            //JVector jpos = new JVector(snapInLocal.X, snapInLocal.Y, snapInLocal.Z);
            constraint = new PointOnPoint(
                body,
                snapInLocal
            );
            constraint.Anchor = snap; //new JVector(snap.X, snap.Y, snap.Z);
            sceneManager.World.AddConstraint(constraint);
        }
        public void Update(int px, int py, Viewport viewport)
        {
            var tr = Services.Get<TextRenderer>();

            //  TODO When model is deleted, PhysicsDrag needs to be deactivated
            if((constraint == null) || (model == null) || (model.RigidBody == null))
            {
                tr.DebugLine("PhysicsDrag.Update() aborted");
                return;
            }

            Matrix4 clipToWorld = sceneManager.Camera.WorldToClip.InverseMatrix;
            Vector3 tip = clipToWorld.UnProject(
                px, 
                py, 
                1.0f, 
                viewport.X,
                viewport.Y,
                viewport.Width,
                viewport.Height
            );

            Vector3 camera = sceneManager.Camera.Frame.LocalToWorld.Matrix.GetColumn3(3);
            Vector3 direction = Vector3.Normalize(tip - camera);
            Vector3 newAnchorPosition = camera + lockDistance * direction;

            tr.DebugLine("PhysicsDrag.Update() model = " + model.Name + " lockDistance = " + lockDistance);
            tr.DebugLine("  hover = " + selectionManager.HoverPosition.ToString());
            tr.DebugLine("  snapInLocal = " + snapInLocal);
            tr.DebugLine("  newAnchorPosition = " + newAnchorPosition);
            tr.DebugLine("  camera = " + camera.ToString());
            tr.DebugLine("  camera description = " + sceneManager.Camera.ToString());

            model.RigidBody.AngularVelocity *= 0.95f;
            model.RigidBody.LinearVelocity *= 0.95f;
            constraint.Anchor = newAnchorPosition; /*new JVector(
                newAnchorPosition.X,
                newAnchorPosition.Y,
                newAnchorPosition.Z
            );*/

            //model.Frame.UpdateHierarchical();
            Vector3 modelSnapPositionInWorld = model.Frame.LocalToWorld.Matrix.TransformPoint(snapInLocal);

            Vector3 o = model.Frame.LocalToWorld.Matrix.GetColumn3(3);
            Vector3 xStart = o; xStart.X -= 20.0f;
            Vector3 yStart = o;
            Vector3 zStart = o; zStart.Z -= 20.0f;
            Vector3 xEnd = o; xEnd.X += 20.0f;
            Vector3 yEnd = o; yEnd.Y = 0.0f;
            Vector3 zEnd = o; zEnd.Z += 20.0f;
            lineRenderer.Begin();
            lineRenderer.Line(newAnchorPosition, modelSnapPositionInWorld, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            lineRenderer.Line(o, modelSnapPositionInWorld, new Vector4(1.0f, 0.5f, 0.0f, 1.0f));
            lineRenderer.Line(o, newAnchorPosition, new Vector4(1.0f, 0.5f, 0.0f, 1.0f));
            lineRenderer.Line(xStart, xEnd, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            lineRenderer.Line(yStart, yEnd, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            lineRenderer.Line(zStart, zEnd, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            lineRenderer.End();
        }
        public void End()
        {
            if(constraint == null)
            {
                return;
            }

            model.RigidBody.AllowDeactivation = true;
            model.Batch.Material = materialStore;
            materialStore = null;
            sceneManager.World.RemoveConstraint(constraint);
            constraint = null;
            model = null;
        }
    }
}
