//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

/*  Comment: Highly experimental  */ 
#if false 
using System.Collections.Generic;
using System.Linq;
using System;

using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Mesh;
using RenderStack.Scene;

using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    /*class RenderPass
    {
        Vector4     clearColor;
        Framebuffer framebuffer;
        Viewport    viewport;
        Camera      camera;
        Group       renderGroup;
    }
    class Sequence
    {
        Vector4 FadeIn;
        Vector4 Color;
        Vector4 FadeOut;
    }*/
    struct OrbitPoint
    {
        public float Theta;
        public float Phi;
        public float Distance;

        public OrbitPoint(float theta, float phi, float distance)
        {
            Theta       = theta;
            Phi         = phi;
            Distance    = distance;
        }
        public static OrbitPoint Mix(OrbitPoint a, OrbitPoint b, float t)
        {
            return new OrbitPoint(
                t * (b.Theta    - a.Theta)    + a.Theta,
                t * (b.Phi      - a.Phi)      + a.Phi,
                t * (b.Distance - a.Distance) + a.Distance
            );
        }

        public Vector3 Cartesian()
        {
            return new Vector3(
                Distance * (float)Math.Sin(Math.PI * Theta) * (float)Math.Cos(Math.PI * Phi),
                Distance * (float)Math.Sin(Math.PI * Theta) * (float)Math.Sin(Math.PI * Phi),
                Distance * (float)Math.Cos(Math.PI * Theta)
            );
        }

    }
    class Signal
    {
        private SortedList<float, float> values = new SortedList<float,float>();
        public SortedList<float, float> Values { get { return values; } }

        public Signal()
        {
        }

        public float At(float t)
        {
            KeyValuePair<float,float> prev = values.First();
            if(t <= prev.Key)
            {
                return prev.Value;
            }

            foreach(var next in values)
            {
                if(next.Key == t)
                {
                    return next.Value;
                }
                if(next.Key > t)
                {
                    float length    = next.Key - prev.Key;
                    float magnitude = (float)System.Math.Abs(length);
                    float u         = 0.5f;
                    if(magnitude > 0.0001f)
                    {
                        float offset = t - prev.Key;
                        u = offset / length;
                    }

                    return u * (next.Value - prev.Value) + prev.Value;
                }
                prev = next;
            }
            return prev.Value;
        }
    }
    class OrbitPath
    {
        private Frame frame;

        private SortedList<float, OrbitPoint> points = new SortedList<float,OrbitPoint>();
        public SortedList<float, OrbitPoint> Points { get { return points; } }

        public OrbitPath(Frame frame)
        {
            this.frame = frame;
        }

        public OrbitPoint At(float t)
        {
            KeyValuePair<float, OrbitPoint> prev = points.First();
            if(t <= prev.Key)
            {
                return prev.Value;
            }

            foreach(var next in points)
            {
                if(next.Key == t)
                {
                    return next.Value;
                }
                if(next.Key > t)
                {
                    float length    = next.Key - prev.Key;
                    float magnitude = (float)System.Math.Abs(length);
                    float u         = 0.5f;
                    if(magnitude > 0.0001f)
                    {
                        float offset = t - prev.Key;
                        u = offset / length;
                    }

                    return OrbitPoint.Mix(prev.Value, next.Value, u);
                }
                prev = next;
            }
            return prev.Value;
        }

        private Matrix4 lookAt = new Matrix4();

        public void Update(float t)
        {
            OrbitPoint  point       = At(t);
            Vector3     newPosition = point.Cartesian();

            lookAt.SetLookAt(newPosition, Vector3.Zero, Vector3.UnitY);
            frame.LocalToParent.Set(lookAt);
        }
    }
    public partial class Sandbox
    {
        //Vector3     clearColor;
        Model       model;
        Signal      exposure;
        OrbitPath   cameraPath;
        //Material    blinnPhong;
        float       LastDemoTime = 10.0f;

        public void DemoInit()
        {
            InitializeRenderers();
            windowViewport = new RenderStack.Viewport(this.Width, this.Height);
            Renderer.CurrentViewport = windowViewport;

            blinnPhong = Material.Create(Renderer.Programs.BlinnPhong);
            blinnPhong.Parameters.Add<Floats>("surface_color").Value = new Floats(0.5f, 0.5f, 0.5f);
            blinnPhong.Parameters.Add<Floats>("surface_rim_color").Value = new Floats(0.1f, 0.2f, 0.5f);
            blinnPhong.Parameters.Add<Floats>("surface_diffuse_reflectance_color").Value = new Floats(0.24f, 0.24f, 0.24f);
            blinnPhong.Parameters.Add<Floats>("surface_specular_reflectance_color").Value = new Floats(0.8f, 0.8f, 0.8f);
            blinnPhong.Parameters.Add<Floats>("surface_specular_reflectance_exponent").Value = new Floats(200.0f);

            gridMaterial = Material.Create(Renderer.Programs.Grid);

#if false
            var mesh = PolyMesh.CreateGreatRhombicosidodecahedron(1.0);
#else
            var mesh = PolyMesh.CreateTruncatedIcosahedron(1.0);
            var dodeca   = PolyMesh.CreateDodecahedron(1.0);
            Attach(mesh, dodeca, 5);
#endif

            var subd1       = new SubdivideGeometryOperation(mesh.Geometry).Destination;
            var subd2       = new SubdivideGeometryOperation(subd1).Destination;
            var cc1         = new CatmullClarkGeometryOperation(subd2).Destination;
            var cc2         = new CatmullClarkGeometryOperation(cc1).Destination;
            cc2.ComputePolygonCentroids();
            cc2.ComputePolygonNormals();
            cc2.ComputeCornerNormals(2.0f * (float)Math.PI);
            //cc2.BuildEdges();

            mesh                = new GeometryMesh(cc2);
            model                   = Model.Create("model", mesh, blinnPhong);
            //model                   = Model.Create("model", mesh, gridMaterial);

            camera                  = Camera.Create();
            camera.FovXRadians      = OpenTK.MathHelper.DegreesToRadians(90.0f);
            camera.FovYRadians      = OpenTK.MathHelper.DegreesToRadians(90.0f);
            camera.ProjectionType   = ProjectionType.PerspectiveVertical;
            camera.Frame.Parent     = model.Frame;
            cameraPath = new OrbitPath(camera.Frame);
            cameraPath.Points[ 0.0f] = new OrbitPoint( 0.0f,  0.0f, 20.0f);
            cameraPath.Points[ 2.0f] = new OrbitPoint( 0.0f,  0.0f, 12.0f);
            cameraPath.Points[ 3.0f] = new OrbitPoint( 0.5f,  0.2f, 15.0f);
            cameraPath.Points[ 4.0f] = new OrbitPoint( 1.0f,  0.5f, 12.0f);
            cameraPath.Points[ 5.0f] = new OrbitPoint( 1.4f,  1.0f, 16.0f);
            cameraPath.Points[ 7.0f] = new OrbitPoint( 2.0f,  1.5f, 13.0f);
            cameraPath.Points[10.0f] = new OrbitPoint( 1.6f,  0.0f, 20.0f);

            exposure = new Signal();
            exposure.Values[ 0.0f] = 0.0f;
            exposure.Values[ 1.0f] = 1.0f;
            exposure.Values[ 2.0f] = 2.0f;
            exposure.Values[ 3.0f] = 1.0f;
            exposure.Values[ 4.0f] = 5.0f;
            exposure.Values[ 8.0f] = 1.0f;
            exposure.Values[10.0f] = 0.0f;

            InitializeRendererParameters();
            DemoStart();
        }

        private Vector4 Color(float t)
        {
            PerlinNoise p = new PerlinNoise(42);

            float   x   = 0.0f + t * 1.0f;
            float   n   = (float)p.Noise(x, 0.0f, 0.0f);
            float   h   = 180.0f + 180.0f * n;
            float   s   = 0.9f;
            float   v   = 0.9f;

            HSVColor hsv = new HSVColor(h, s, v);
            RGBColor rgb = new RGBColor();
            hsv.RGB(out rgb);
            return new Vector4(rgb.R, rgb.G, rgb.B, 1.0f);
        }

        public void DemoStart()
        {
            Time.Initialize();
        }
        public void Attach(PolyMesh center, PolyMesh brush, int cornerCount)
        {
            //var center  = PolyMesh.CreateGreatRhombicosidodecahedron(1.0);
            //var brush   = PolyMesh.CreateDodecahedron(1.0);

            // Get brushPolygon
            Polygon brushPolygon = null;
            foreach(var polygon in brush.Geometry.Polygons)
            {
                if(polygon.Corners.Count == cornerCount)
                {
                    brushPolygon = polygon;
                    break;
                }
            }
            if(brushPolygon == null)
            {
                return;
            }

            List<Polygon> processPolygons = new List<Polygon>();
            foreach(var centerPolygon in center.Geometry.Polygons)
            {
                if(centerPolygon.Corners.Count != cornerCount)
                {
                    continue;
                }
                processPolygons.Add(centerPolygon);
            }

            foreach(var centerPolygon in processPolygons)
            {
                ReferenceFrame brushFrame = MakePolygonReference(brush.Geometry, brushPolygon);
                ReferenceFrame centerFrame = MakePolygonReference(center.Geometry, centerPolygon);

                var     cloner          = new CloneGeometryOperation(brush.Geometry, null);
                float   scale           = centerFrame.Scale / brushFrame.Scale;
                var     newGeometry     = cloner.Destination;
                var     scaleTransform  = Matrix4.CreateScale(scale);
                var     clonedPolygon   = cloner.polygonOldToNew[brushPolygon];
                if(scale != 1.0f)
                {
                    newGeometry.Transform(scaleTransform);
                    brushFrame.Transform(scaleTransform);
                }

                centerFrame.Normal *= -1.0f; //  Flip target normal..

                Matrix4 centerTransform = centerFrame.GetTransform();
                Matrix4 brushTransform  = brushFrame.GetTransform();
                Matrix4 inverseBrush    = Matrix4.Invert(brushTransform);
                Matrix4 align           = centerTransform * inverseBrush;
                newGeometry.Transform(align);

                //center.Geometry.MergeFast(newGeometry, centerPolygon, clonedPolygon);
                center.Geometry.Merge(newGeometry);
            }
            center.Geometry.ComputePolygonNormals();
            center.Geometry.ComputePolygonCentroids();
            center.Geometry.ComputeCornerNormals(2.0f * (float)Math.PI);
            center.BuildMeshFromGeometry();
        }
        public void DemoUpdate()
        {
            System.Threading.Thread.Sleep(0);
        }
        public void DemoRender()
        {
            float t = Time.Now;

            if(t > LastDemoTime)
            {
                DemoStart();
                t = 0.0f;
            }

            float e = exposure.At(t);
            (Renderer.GlobalParameters.Values["exposure"] as IParameterValue<Floats>).Value.Set(e);

            OrbitPoint o = cameraPath.At(t);

            AverageCpuUsage += CPUUsage;
            ++updateCounter;
            if(updateCounter == 10)
            {
                int cpu = (int)(AverageCpuUsage / updateCounter);
                int megabytesInUse = (int)(MemoryUsage / (1024.0f * 1024.0f));
                base.Title = 
                    t.ToString("00.0")
                    + " t = " + o.Theta.ToString("0.00")
                    + " p = " + o.Phi.ToString("0.00")
                    + " d = " + o.Distance.ToString("0.00")
                    + " e = " + e.ToString("0.00")
                    + " CPU Use " + cpu.ToString() 
                    + "% Mem Use " + megabytesInUse.ToString() + " MB"
                    + " GC0 " + GC.CollectionCount(0)
                    + " GC1 " + GC.CollectionCount(1)
                    + " GC2 " + GC.CollectionCount(2)
                    + " GC3 " + GC.CollectionCount(3)
                    ;
                AverageCpuUsage = 0.0f;
                updateCounter = 0;
            }

            cameraPath.Update(t);

            GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            camera.Update(Renderer.CurrentViewport);
            PartialGLStateResetToDefaults();
            Renderer.CurrentCamera      = camera;
            Renderer.CurrentViewport    = windowViewport;
            Renderer.CurrentModel       = model;
            Renderer.CurrentMaterial    = model.Material;
            Renderer.CurrentProgram     = Renderer.CurrentMaterial.Program;
            Renderer.RenderCurrent();

            SwapBuffers();
        }
    }
}


#endif