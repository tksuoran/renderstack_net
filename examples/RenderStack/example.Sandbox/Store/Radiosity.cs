//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;

using Buffer = RenderStack.Graphics.BufferGL;

namespace example.Sandbox
{
    public partial class Application
    {
    }

#if false
    public class RadiosityRenderer : Service
    {
        SceneManager sceneManager;
        CubeRenderer aoHemicubeRenderer;

        public void Connect(
            SceneManager sceneManager,
            CubeRenderer aoHemicubeRenderer
        )
        {
            this.sceneManager = sceneManager;
            this.aoHemicubeRenderer = aoHemicubeRenderer;
        }

        protected override void InitializeService()
        {
        }

        private void RenderLightVisibility()
        {
            Camera camera           = sceneManager.Camera;
            Group  reflectedGroup   = sceneManager.ReflectedGroup;

            /*  Render cube map  */ 
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
            //cubeRenderer.Render(ReflectionModel.Frame.LocalToWorld.Matrix, renderGroup);
            aoHemicubeRenderer.Render(camera.Frame.LocalToWorld.Matrix, reflectedGroup);
        }
        private void RenderRadiosity(Group renderGroup)
        {
            hemisphericalRenderer.Render(
                sceneManager.Camera.Frame.LocalToWorld.Matrix, 
                renderGroup
            );
        }
        private void UpdatePointHemicube(Model model, Point point, Group group)
        {
            //string mapName = "point_normals_for_ambient_occlusion";
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Matrix4     pointFrame      = new Matrix4();
            Geometry    geometry        = mesh.Geometry;
            var         pointLocations  = geometry.PointAttributes.Find<Vector3>("point_locations");
            Vector3     eye             = pointLocations[point];
            Vector3     normal          = geometry.ComputePointNormal(point);
            eye    = model.Frame.LocalToWorld.Matrix.TransformPoint(eye);
            normal = model.Frame.LocalToWorld.Matrix.TransformDirection(normal);
            //eye += 10.0f * hemicubeRenderer.Near * normal;
            //aoEye = eye;
            //aoBack = normal;
            Matrix4.CreateLookAt(eye, eye + normal, normal.MinAxis, out pointFrame);
            Vector3 positionInWorld = pointFrame.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
            renderer.PartialGLStateResetToDefaults();
            pointViewHemicubeRenderer.ProgramOverride = null;
            pointViewHemicubeRenderer.Render(pointFrame, group);
        }
    }
#endif
}