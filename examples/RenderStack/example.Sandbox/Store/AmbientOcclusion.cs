using System;
using OpenTK.Graphics.OpenGL;
using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;
using example.Renderer;

namespace example.Sandbox
{
    public class AmbientOcclusionManager : Service
    {
        public override string Name
        {
            get { return "AmbientOcclusionManager"; }
        }

        HemicubeRenderer    aoHemicubeRenderer;
        IRenderer           renderer;
        SceneManager        sceneManager;

        public void Connect(
            HemicubeRenderer    aoHemicubeRenderer, 
            IRenderer           renderer, 
            SceneManager        sceneManager
        )
        {
            this.aoHemicubeRenderer = aoHemicubeRenderer;
            this.renderer           = renderer;
            this.sceneManager       = sceneManager;

            InitializationDependsOn(aoHemicubeRenderer);
            InitializationDependsOn(renderer);
        }

        protected override void InitializeService()
        {
        }

        Random r = new Random();
        Vector3 Random(float scale)
        {
            Vector3 a;
            do
            {
                a.X = -1.0f + 2.0f * (float)r.NextDouble();
                a.Y = -1.0f + 2.0f * (float)r.NextDouble();
                a.Z = -1.0f + 2.0f * (float)r.NextDouble();
            }
            while(a.LengthSquared > 1.0f);
            a = scale * Vector3.Normalize(a);
            return a;
        }
        Vector3 Random()
        {
            Vector3 a;
            do
            {
                a.X = -1.0f + 2.0f * (float)r.NextDouble();
                a.Y = -1.0f + 2.0f * (float)r.NextDouble();
                a.Z = -1.0f + 2.0f * (float)r.NextDouble();
            }
            while(a.LengthSquared > 1.0f);
            a = Vector3.Normalize(a);
            return a;
        }

#if false
        //Jitter.Dynamics.RigidBody testBody;
        bool RaycastCallback(Jitter.Dynamics.RigidBody body,JVector normal, float fraction)
        {
            return body == testBody;
        }
#endif

        //int sampleCount = 50;
        //float distance = 4.0f;
        public void UpdateAmbientOcclusion(Model model, Group occluders, string mapName)
        {
#if false // \todo fix
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Geometry geometry = mesh.Geometry;

            model.UpdateOctree();
            var shape = new TriangleMeshShape(model.Octree);
            shape.SphericalExpansion = 0.0f;

#if VISUALIZE_RAYS
            sceneManager.DebugLineRenderer.Begin();
#endif

            var pointLocations      = geometry.PointAttributes.Find<Vector3>("point_locations");
            var polygonCentroids    = geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");
            var pointNormals        = geometry.PointAttributes.Find<Vector3>(mapName);
            var cornerNormals       = geometry.CornerAttributes.Contains<Vector3>("corner_normals") ? geometry.CornerAttributes.Find<Vector3>("corner_normals") : null;
            var cornerColors        = geometry.CornerAttributes.FindOrCreate<Vector4>("corner_colors");
            cornerColors.Clear();
            int badCount = 0;
            foreach(Polygon polygon in geometry.Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    Point   point   = corner.Point;

                    //  Start from corner position slightly moved towards polygon center
                    Vector3 initialEye = Vector3.Mix(pointLocations[point], polygonCentroids[polygon], 0.001f);
                    Vector3 normal;

                    if(
                        (cornerNormals != null) && 
                        (cornerNormals.ContainsKey(corner) == true)
                    )
                    {
                        normal  = cornerNormals[corner];
                    }
                    else
                    {
                        normal  = pointNormals[point];
                    }

                    JMatrix orientation = JMatrix.Identity;
                    JMatrix invOrientation = JMatrix.Identity;
                    Vector3 modelPosition = Vector3.Zero;

                    //  Slightly up from surface
                    initialEye += normal * 0.01f;

                    int visibility = sampleCount;

                    JVector position  = Conversions.JVector(modelPosition);
                    JVector direction;
                    JVector jnormal;

                    Vector3 eye = initialEye;
                    JVector origin = Conversions.JVector(eye);

                    //  Sample a few directions
                    for(int c = 0; c < sampleCount; ++c)
                    {
                        //  Get a random direction until it points up from the surface
                        Vector3 sampleDirection;
                        float dot;
                        do
                        {
                            sampleDirection = Random();
                            dot = Vector3.Dot(normal, sampleDirection);
                        }
                        while(dot < 0.25f);

                        //  Scale
                        direction = Conversions.JVector(sampleDirection * distance);

                        bool hit = false;
                        {
                            float tempFraction;

                            if(shape is Multishape)
                            {
                                Multishape  ms = (shape as Multishape).RequestWorkingClone();
                                JVector     tempNormal; 
                                int         msLength = ms.Prepare(ref origin, ref direction);

                                for(int i = 0; i < msLength; i++)
                                {
                                    ms.SetCurrentShape(i);

                                    if(
                                        GJKCollide.Raycast(
                                            ms,  ref orientation, ref invOrientation, ref position,
                                            ref origin, ref direction, out tempFraction, out tempNormal
                                        )
                                    )
                                    {
                                        hit = true;
                                        break;
                                    }
                                }

                                ms.ReturnWorkingClone();
                            }
                            else
                            {
                                hit = GJKCollide.Raycast(
                                    shape, ref orientation, ref invOrientation, ref position,
                                    ref origin, ref direction, out tempFraction, out jnormal
                                );
                            }
                        }


                        if(hit)
                        {
                            visibility -= 1;
                        }

                        Vector3 root = new Vector3(origin.X, origin.Y, origin.Z);
                        Vector3 tip = root + 0.02f * new Vector3(direction.X, direction.Y, direction.Z);
#if VISUALIZE_RAYS
                        sceneManager.DebugLineRenderer.Line(
                            root,
                            Vector4.Zero,
                            tip,
                            (hit ? Vector4.UnitX : Vector4.UnitY)
                        );
#endif
                    }
                    float visibilityFloat = (float)(visibility) / (float)(sampleCount);
                    cornerColors[corner] = new Vector4(visibilityFloat, visibilityFloat, visibilityFloat, 1.0f);
                }
            }
            mesh.Geometry.ComputePolygonCentroids();
            mesh.Geometry.ComputePolygonNormals();
            mesh.Geometry.SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)Math.PI));
            mesh.Geometry.SmoothAverage("corner_colors", mapName);
            mesh.BuildMeshFromGeometry(BufferUsageHint.StaticDraw, NormalStyle.CornerNormals);
            //UpdateMeshCornerColors(mesh);
            Debug.WriteLine("bad count = " + badCount.ToString());

#if VISUALIZE_RAYS
            sceneManager.DebugLineRenderer.End();
            sceneManager.DebugFrame = model.Frame;
#endif

            //model.Name = "AmbientOcclusion(" + model.Name + ")";
#endif
        }

        public unsafe void UpdateMeshCornerColors(GeometryMesh mesh)
        {
            if(mesh.Geometry.CornerAttributes.Contains<Vector4>("corner_colors") == false)
            {
                return;
            }
            var cornerColors = mesh.Geometry.CornerAttributes.Find<Vector4>("corner_colors");
            var attributeColor = mesh.GetMesh.VertexBufferRange.VertexFormat.FindAttribute(VertexUsage.Color, 0);

            //GL.BindBuffer(mesh.GetMesh.VertexBufferRange.Buffer.BufferTarget, mesh.GetMesh.VertexBufferRange.BufferObject);
            renderer.Requested.Mesh = mesh.GetMesh;
            renderer.BindAttributesAndCheckForUpdates();
            IntPtr ptr = GL.MapBuffer(mesh.GetMesh.VertexBufferRange.BufferTargetGL, BufferAccess.ReadWrite);
            byte*  bytePtr = (byte*)ptr;

            int index = 0;
            foreach(Polygon polygon in mesh.Geometry.Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    byte*  offsetPtr = &bytePtr[mesh.GetMesh.VertexBufferRange.VertexFormat.Stride * index + attributeColor.Offset];
                    float* fPtr = (float*)(offsetPtr);
                    fPtr[0] = cornerColors[corner].X;
                    fPtr[1] = cornerColors[corner].Y;
                    fPtr[2] = cornerColors[corner].Z;
                    fPtr[3] = cornerColors[corner].W;
                    ++index;
                }
            }
            GL.UnmapBuffer(mesh.GetMesh.VertexBufferRange.BufferTargetGL);
        }

        public void UpdateAmbientOcclusionOld(Model model, Group occluders)
        {
            string mapName = "point_normals_for_ambient_occlusion";
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Matrix4 pointFrame = new Matrix4();
            Geometry geometry = mesh.Geometry;

            var pointLocations      = geometry.PointAttributes.Find<Vector3>("point_locations");
            var polygonCentroids    = geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");
            var pointNormals        = geometry.PointAttributes.Find<Vector3>(mapName);
            var cornerNormals       = geometry.CornerAttributes.Contains<Vector3>("corner_normals") ? geometry.CornerAttributes.Find<Vector3>("corner_normals") : null;
            var cornerColors        = geometry.CornerAttributes.FindOrCreate<Vector4>("corner_colors");
            cornerColors.Clear();
            float[] colorBuffer = new float[mesh.GetMesh.VertexBufferRange.Count * 4];
            foreach(Polygon polygon in geometry.Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    Point   point   = corner.Point;
                    Vector3 eye     = Vector3.Mix(pointLocations[point], polygonCentroids[polygon], 0.01f);
                    Vector3 normal;

                    if(
                        (cornerNormals != null) && 
                        (cornerNormals.ContainsKey(corner) == true)
                    )
                    {
                        normal  = cornerNormals[corner];
                    }
                    else
                    {
                        normal  = pointNormals[point];
                    }
                    eye    = model.Frame.LocalToWorld.Matrix.TransformPoint(eye);
                    normal = model.Frame.LocalToWorld.Matrix.TransformDirection(normal);
                    eye += normal * 0.01f;

                    //aoEye = eye;
                    //aoBack = normal;
                    Matrix4.CreateLookAt(eye, eye + normal, normal.MinAxis, out pointFrame);
                    Vector3 positionInWorld = pointFrame.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
                    aoHemicubeRenderer.Render(pointFrame, occluders);
                    float occlusion = aoHemicubeRenderer.Average;
                    cornerColors[corner] = new Vector4(occlusion, occlusion, occlusion, 1.0f);
                }
            }
            mesh.BuildMeshFromGeometry(BufferUsageHint.StaticDraw, NormalStyle.CornerNormals);

            model.Name = "AmbientOcclusion(" + model.Name + ")";
        }

        /// <summary>
        /// Updates ambient occlusion for single model.
        /// This version updates point normals.
        /// </summary>
        public void UpdateAmbientOcclusionModel(Model model, Group occluders)
        {
            string pointNormalsName = "point_normals_for_ambient_occlusion";
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh != null)
            {
                Geometry geometry = mesh.Geometry;
                geometry.ComputePointNormals(pointNormalsName);
            }
            //renderer.PartialGLStateResetToDefaults();
            //aoHemicubeRenderer.ProgramOverride = renderer.Programs["LightOccluder"];
            UpdateAmbientOcclusion(model, occluders, pointNormalsName);
        }
        /// <summary>
        /// Updates ambient occlusion for all models in receivers group
        /// </summary>
        public void UpdateAmbientOcclusion(Group receivers, Group occluders)
        {
            string pointNormalsName = "point_normals_for_ambient_occlusion";
            {
                foreach(var model in receivers.Models)
                {
                    //updateMsg = "ComputePointNormals for " + i + " / " + receivers.Models.Count;
                    //OnRenderFrame(null);
                    GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
                    if(mesh != null)
                    {
                        Geometry geometry = mesh.Geometry;
                        geometry.ComputePointNormals(pointNormalsName);
                    }
                }
                //renderer.PartialGLStateResetToDefaults();
                //aoHemicubeRenderer.ProgramOverride = renderer.Programs["LightOccluder"];
                foreach(var model in receivers.Models)
                {
                    //updateMsg = "UpdateAmbientOcclusion for " + i + " / " + receivers.Models.Count;
                    //OnRenderFrame(null);
                    UpdateAmbientOcclusionModel(model, occluders);
                }
                //updateMsg = "";
            }
        }
    }
}



#if false 
#define VISUALIZE_RAYS
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

using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;


using Buffer = RenderStack.Graphics.Buffer;

namespace Sandbox
{
    public class AmbientOcclusionManager : Service
    {
        public override string Name
        {
            get { return "AmbientOcclusionManager"; }
        }

        //  Services
        HemicubeRenderer        aoHemicubeRenderer;
        Renderer                renderer;
        SceneManager            sceneManager;
        //private Vector3 aoEye;
        //private Vector3 aoBack;

        public void Connect(HemicubeRenderer aoHemicubeRenderer, Renderer renderer, SceneManager sceneManager)
        {
            this.aoHemicubeRenderer = aoHemicubeRenderer;
            this.renderer           = renderer;
            this.sceneManager       = sceneManager;

            InitializationDependsOn(aoHemicubeRenderer);
            InitializationDependsOn(renderer);
        }

        protected override void InitializeService()
        {
        }

        Random r = new Random();
        Vector3 Random(float scale)
        {
            Vector3 a;
            do
            {
                a.X = -1.0f + 2.0f * (float)r.NextDouble();
                a.Y = -1.0f + 2.0f * (float)r.NextDouble();
                a.Z = -1.0f + 2.0f * (float)r.NextDouble();
            }
            while(a.LengthSquared > 1.0f);
            a = scale * Vector3.Normalize(a);
            return a;
        }
        Vector3 Random()
        {
            Vector3 a;
            do
            {
                a.X = -1.0f + 2.0f * (float)r.NextDouble();
                a.Y = -1.0f + 2.0f * (float)r.NextDouble();
                a.Z = -1.0f + 2.0f * (float)r.NextDouble();
            }
            while(a.LengthSquared > 1.0f);
            a = Vector3.Normalize(a);
            return a;
        }

        Jitter.Dynamics.RigidBody testBody;
        bool RaycastCallback(Jitter.Dynamics.RigidBody body,JVector normal, float fraction)
        {
            return body == testBody;
        }

        int sampleCount = 100;
        float distance = 1.0f;
        public void UpdateAmbientOcclusion(Model model, Group occluders)
        {
            string mapName = "point_normals_for_ambient_occlusion";
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Geometry geometry = mesh.Geometry;

            model.UpdateOctree();
            var shape = new TriangleMeshShape(model.Octree);

#if VISUALIZE_RAYS
            sceneManager.DebugLineRenderer.Begin();
#endif

            var pointLocations      = geometry.PointAttributes.Find<Vector3>("point_locations");
            var polygonCentroids    = geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");
            var pointNormals        = geometry.PointAttributes.Find<Vector3>(mapName);
            var cornerNormals       = geometry.CornerAttributes.Contains<Vector3>("corner_normals") ? geometry.CornerAttributes.Find<Vector3>("corner_normals") : null;
            var cornerColors        = geometry.CornerAttributes.FindOrCreate<Vector4>("corner_colors");
            cornerColors.Clear();
            int badCount = 0;
            foreach(Polygon polygon in geometry.Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    Point   point   = corner.Point;

                    //  Start from corner position slightly moved towards polygon center
                    Vector3 initialEye = Vector3.Mix(pointLocations[point], polygonCentroids[polygon], 0.01f);
                    Vector3 normal;

                    if(
                        (cornerNormals != null) && 
                        (cornerNormals.ContainsKey(corner) == true)
                    )
                    {
                        normal  = cornerNormals[corner];
                    }
                    else
                    {
                        normal  = pointNormals[point];
                    }

                    JMatrix orientation = JMatrix.Identity;
                    JMatrix invOrientation = JMatrix.Identity;
                    Vector3 modelPosition = Vector3.Zero;

                    //  Move to world space
#if false
                    initialEye  = model.Frame.LocalToWorld.Matrix.TransformPoint(initialEye);
                    normal      = model.Frame.LocalToWorld.Matrix.TransformDirection(normal);
                    modelPosition = model.Frame.LocalToWorld.Matrix.GetColumn3(3);
                    orientation = Conversions.JMatrix(model.Frame.LocalToWorld.Matrix);
                    invOrientation = Conversions.JMatrix(model.Frame.LocalToWorld.InverseMatrix);
#endif

                    //  Slightly up from surface
                    initialEye += normal * 0.01f;

                    int visibility = sampleCount;

                    JVector position  = Conversions.JVector(modelPosition);
                    JVector direction;
                    JVector jnormal;

                    Vector3 eye = initialEye;
                    JVector origin = Conversions.JVector(eye);
#if false
                    //  Test if ray origin would be inside shape,
                    //  if so, try to jitter slightly to get ray
                    //  origin outside the shape
                    float jitter = 0.001f;
                    bool inside = true;
                    for(int j = 0; j < 100; ++j)
                    {
                        inside = Jitter.Collision.GJKCollide.Pointcast(
                            model.PhysicsShape,
                            ref orientation,    // orientation
                            ref position,       // position
                            ref origin          // origin,
                        );
                        if(inside == false)
                        {
                            break;
                        }
                        eye = initialEye + Random() * jitter;
                        origin = Conversions.JVector(eye);
                    }
                    if(inside == true)
                    {
                        ++badCount;
                    }
#endif

                    //  Now cast a bunch of rays
                    for(int c = 0; c < sampleCount; ++c)
                    {
                        //  Get a random direction until it points up from the surface
                        Vector3 sampleDirection;
                        float dot;
                        do
                        {
                            sampleDirection = Random();
                            dot = Vector3.Dot(normal, sampleDirection);
                        }
                        while(dot < 0.15f);

                        //  Scale
                        direction = Conversions.JVector(sampleDirection * distance);

                        bool hit = false;
                        {
                            float tempFraction;
                            if(shape is Multishape)
                            {
                                Multishape  ms = (shape as Multishape).RequestWorkingClone();
                                JVector     tempNormal; 
                                int         msLength = ms.Prepare(ref origin, ref direction);

                                for(int i = 0; i < msLength; i++)
                                {
                                    ms.SetCurrentShape(i);

                                    if(
                                        GJKCollide.Raycast(
                                            ms, 
                                            ref orientation,
                                            ref invOrientation,
                                            ref position,
                                            ref origin,
                                            ref direction,
                                            out tempFraction,
                                            out tempNormal
                                        )
                                    )
                                    {
                                        hit = true;
                                        break;
                                    }
                                }

                                ms.ReturnWorkingClone();
                            }
                            else
                            {
                                hit = GJKCollide.Raycast(
                                    shape,
                                    ref orientation,
                                    ref invOrientation,
                                    ref position,
                                    ref origin,
                                    ref direction,
                                    out tempFraction,
                                    out jnormal
                                );
                            }
                        }


#if false
                        bool hit = sceneManager.World.CollisionSystem.Raycast(
                            model.RigidBody, 
                            origin, 
                            direction, 
                            out jnormal, 
                            out fraction
                        );
#endif

#if false
                        Jitter.Dynamics.RigidBody body;
                        bool hit = sceneManager.World.CollisionSystem.Raycast(
                            origin, 
                            direction,
                            RaycastCallback,
                            out body,
                            out jnormal, 
                            out fraction
                        );
#endif
                        //bool hit = model.Octree.IntersectsRay(origin, direction);

#if false
                        bool hit = Jitter.Collision.GJKCollide.Raycast(
                            model.PhysicsShape,     // shape
                            ref orientation,        // orientation
                            ref invOrientation,     // invorientation
                            ref position,           // position
                            ref origin,             // origin,
                            ref direction,          // direction,
                            out fraction,
                            out jnormal
                        );
#endif
                        if(hit)
                        {
                            visibility -= 1;
                        }

                        Vector3 root = new Vector3(origin.X, origin.Y, origin.Z);
                        Vector3 tip = root + 0.1f * new Vector3(direction.X, direction.Y, direction.Z);
#if VISUALIZE_RAYS
#if false
                        sceneManager.DebugLineRenderer.Line(
                            model.Frame.LocalToWorld.InverseMatrix.TransformPoint(root),
                            model.Frame.LocalToWorld.InverseMatrix.TransformPoint(tip),
                            (hit ? Vector4.UnitX : Vector4.UnitY)
                        );
#else
                        sceneManager.DebugLineRenderer.Line(
                            root,
                            Vector4.Zero,
                            tip,
                            (hit ? Vector4.UnitX : Vector4.UnitY)
                        );
#endif
#endif
                    }
                    float visibilityFloat = (float)(visibility) / (float)(sampleCount);
                    cornerColors[corner] = new Vector4(visibilityFloat, visibilityFloat, visibilityFloat, 1.0f);
                }
            }
            mesh.Geometry.ComputePolygonCentroids();
            mesh.Geometry.ComputePolygonNormals();
            mesh.Geometry.SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)Math.PI));
            mesh.Geometry.SmoothAverage("corner_colors");
            mesh.BuildMeshFromGeometry(NormalStyle.CornerNormals);
            //UpdateMeshCornerColors(mesh);
            Debug.WriteLine("bad count = " + badCount.ToString());

#if VISUALIZE_RAYS
            sceneManager.DebugLineRenderer.End();
            sceneManager.DebugFrame = model.Frame;
#endif

            //model.Name = "AmbientOcclusion(" + model.Name + ")";
        }

        public unsafe void UpdateMeshCornerColors(GeometryMesh mesh)
        {
            if(mesh.Geometry.CornerAttributes.Contains<Vector4>("corner_colors") == false)
            {
                return;
            }
            var cornerColors = mesh.Geometry.CornerAttributes.Find<Vector4>("corner_colors");
            var attributeColor = mesh.GetMesh.VertexBuffer.VertexFormat.FindAttribute(VertexUsage.Color, 0);

            GL.BindBuffer(mesh.GetMesh.VertexBuffer.BufferTarget, mesh.GetMesh.VertexBuffer.BufferObject);
            IntPtr ptr = GL.MapBuffer(mesh.GetMesh.VertexBuffer.BufferTarget, BufferAccess.ReadWrite);
            byte*  bytePtr = (byte*)ptr;

            int index = 0;
            foreach(Polygon polygon in mesh.Geometry.Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    byte*  offsetPtr = &bytePtr[mesh.GetMesh.VertexBuffer.VertexFormat.Stride * index + attributeColor.Offset];
                    float* fPtr = (float*)(offsetPtr);
                    fPtr[0] = cornerColors[corner].X;
                    fPtr[1] = cornerColors[corner].Y;
                    fPtr[2] = cornerColors[corner].Z;
                    fPtr[3] = cornerColors[corner].W;
                    ++index;
                }
            }
            GL.UnmapBuffer(mesh.GetMesh.VertexBuffer.BufferTarget);
        }

        public void UpdateAmbientOcclusionOld(Model model, Group occluders)
        {
            string mapName = "point_normals_for_ambient_occlusion";
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Matrix4 pointFrame = new Matrix4();
            Geometry geometry = mesh.Geometry;

            var pointLocations      = geometry.PointAttributes.Find<Vector3>("point_locations");
            var polygonCentroids    = geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");
            var pointNormals        = geometry.PointAttributes.Find<Vector3>(mapName);
            var cornerNormals       = geometry.CornerAttributes.Contains<Vector3>("corner_normals") ? geometry.CornerAttributes.Find<Vector3>("corner_normals") : null;
            var cornerColors        = geometry.CornerAttributes.FindOrCreate<Vector4>("corner_colors");
            cornerColors.Clear();
            float[] colorBuffer = new float[mesh.GetMesh.VertexBuffer.Count * 4];
            foreach(Polygon polygon in geometry.Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    Point   point   = corner.Point;
                    Vector3 eye     = Vector3.Mix(pointLocations[point], polygonCentroids[polygon], 0.01f);
                    Vector3 normal;

                    if(
                        (cornerNormals != null) && 
                        (cornerNormals.ContainsKey(corner) == true)
                    )
                    {
                        normal  = cornerNormals[corner];
                    }
                    else
                    {
                        normal  = pointNormals[point];
                    }
                    eye    = model.Frame.LocalToWorld.Matrix.TransformPoint(eye);
                    normal = model.Frame.LocalToWorld.Matrix.TransformDirection(normal);
                    eye += normal * 0.01f;

                    //aoEye = eye;
                    //aoBack = normal;
                    Matrix4.CreateLookAt(eye, eye + normal, normal.MinAxis, out pointFrame);
                    Vector3 positionInWorld = pointFrame.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
                    aoHemicubeRenderer.Render(pointFrame, occluders);
                    float occlusion = aoHemicubeRenderer.Average;
                    cornerColors[corner] = new Vector4(occlusion, occlusion, occlusion, 1.0f);
                }
            }
            mesh.BuildMeshFromGeometry(NormalStyle.CornerNormals);

            model.Name = "AmbientOcclusion(" + model.Name + ")";
        }

        /// <summary>
        /// Updates ambient occlusion for single model.
        /// This version updates point normals.
        /// </summary>
        public void UpdateAmbientOcclusionModel(Model model, Group occluders)
        {
            string mapName = "point_normals_for_ambient_occlusion";
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh != null)
            {
                Geometry geometry = mesh.Geometry;
                geometry.ComputePointNormals(mapName);
            }
            //renderer.PartialGLStateResetToDefaults();
            //aoHemicubeRenderer.ProgramOverride = renderer.Programs["LightOccluder"];
            UpdateAmbientOcclusion(model, occluders);
        }
        /// <summary>
        /// Updates ambient occlusion for all models in receivers group
        /// </summary>
        public void UpdateAmbientOcclusion(Group receivers, Group occluders)
        {
            if(RenderStack.Graphics.Configuration.GL3)
            {
                string mapName = "point_normals_for_ambient_occlusion";
                for(int i = 0; i < receivers.Models.Count; ++i)
                {
                    Model model = receivers.Models[i];
                    //updateMsg = "ComputePointNormals for " + i + " / " + receivers.Models.Count;
                    //OnRenderFrame(null);
                    GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
                    if(mesh != null)
                    {
                        Geometry geometry = mesh.Geometry;
                        geometry.ComputePointNormals(mapName);
                    }
                }
                //renderer.PartialGLStateResetToDefaults();
                //aoHemicubeRenderer.ProgramOverride = renderer.Programs["LightOccluder"];
                for(int i = 0; i < receivers.Models.Count; ++i)
                {
                    Model model = receivers.Models[i];
                    //updateMsg = "UpdateAmbientOcclusion for " + i + " / " + receivers.Models.Count;
                    //OnRenderFrame(null);
                    UpdateAmbientOcclusion(model, occluders);
                }
                //updateMsg = "";
            }
        }
    }
}

#endif