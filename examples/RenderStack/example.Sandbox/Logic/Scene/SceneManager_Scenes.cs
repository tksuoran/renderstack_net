//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

//#define OPENRL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.LightWave;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Physics;
using RenderStack.Scene;
using RenderStack.Services;

using example.Brushes;
using example.Renderer;

using Debug = System.Diagnostics.Debug;
using Attribute = RenderStack.Graphics.Attribute;
using Shape = RenderStack.Physics.Shape;
using Material = example.Renderer.Material;

namespace example.Sandbox
{
    public partial class SceneManager : Service
    {
        public void AddFloor(float size)
        {
            AddFloor(size, 3, -0.5f);
        }
        public static float testNoise(float x, float z)
        {
            return 3.0f * (float)Perlin.Noise(
                new Vector3(x * 0.3f, 0.0f, z * 0.1)
            );
        }
        public void AddFloor(float size, int subdivisions, float y)
        {
#if false
            Message("SceneManager: AddFloor Geometry...");
            Geometry g = new Cube(size, 1.0, size);
            for(int i = 0; i < subdivisions; ++i)
            {
                Message("SceneManager: AddFloor Subdivide " + i + "/" + subdivisions + "...");
                g = new SubdivideGeometryOperation(g).Destination;
            }
            Message("SceneManager: AddFloor GeometryMesh...");
#endif
            GeometryMesh floorMesh;
            Shape shape;
            if(RuntimeConfiguration.gameTest)
            {
                //HeightField.Evaluate evaluate = testNoise;
                int div = 40;
                float[,] heights = new float[div, div];
                float scaleX = size / heights.GetLength(0);
                float scaleZ = size / heights.GetLength(1);
                for(int x = 0; x < div; x++)
                {
                    float relX = (float)x / (float)div;
                    for(int z = 0; z < div; z++)
                    {
                        float relZ = (float)z / (float)div;
                        float xP  = relX * size;
                        float zP  = relZ * size;
                        float rx = 6.0f * (relX - 0.5f);
                        float rz = 6.0f * (relZ - 0.5f);
                        float d2 = rx*rx + rz*rz;
                        if(d2 > 1.0f) d2 = 1.0f;
                        float yP  = 1.0f + d2 * 0.5f * (0.5f + 0.5f * testNoise(xP, zP));
                        heights[x, z] = yP;
                    }
                }
                Geometry g = new HeightField(
                    heights, 
                    scaleX, 
                    scaleZ
                );
                floorMesh = new GeometryMesh(g, NormalStyle.PointNormals);
                shape = new TerrainShape(heights, scaleX, scaleZ);
                floorModel = new Model(
                    "Cube (floor)",
                    floorMesh,
                    //materialManager["Schlick"],
                    materialManager["Grid"],
                    -size / 2.0f, y, -size / 2.0f
                );
            }
            else
            {
                Geometry g = new Cube(
                    new Vector3(size, 1.0f, size), 
                    new IVector3(4 * subdivisions, 1, 4 * subdivisions)
                );
                floorMesh = new GeometryMesh(g, NormalStyle.PolygonNormals);
                shape = new BoxShape(size, 1.0f, size);
                floorModel = new Model(
                    "Cube (floor)",
                    floorMesh,
                    //materialManager["Schlick"],
                    materialManager["Grid"],
                    0.0f, y, 0.0f
                );
            }
            floorSize = size;

            floorModel.PhysicsShape = shape;
            floorModel.Static = true;

            AddModel(floorModel);
        }
        public static Matrix4 Motion(LWItem item, float time)
        {
            LWSEnvelope eX  = item.Motion.Channel(RenderStack.LightWave.LWChannel.X);
            LWSEnvelope eY  = item.Motion.Channel(RenderStack.LightWave.LWChannel.Y);
            LWSEnvelope eZ  = item.Motion.Channel(RenderStack.LightWave.LWChannel.Z);
            LWSEnvelope eSX = item.Motion.Channel(RenderStack.LightWave.LWChannel.SX);
            LWSEnvelope eSY = item.Motion.Channel(RenderStack.LightWave.LWChannel.SY);
            LWSEnvelope eSZ = item.Motion.Channel(RenderStack.LightWave.LWChannel.SZ);
            LWSEnvelope eH  = item.Motion.Channel(RenderStack.LightWave.LWChannel.H);
            LWSEnvelope eP  = item.Motion.Channel(RenderStack.LightWave.LWChannel.P);
            LWSEnvelope eB  = item.Motion.Channel(RenderStack.LightWave.LWChannel.B);
            float x     = (eX != null) ? eX.eval(time) : 0.0f;
            float y     = (eY != null) ? eY.eval(time) : 0.0f;
            float z     = (eZ != null) ? eZ.eval(time) : 0.0f;
            float sx    = (eSX != null) ? eSX.eval(time) : 1.0f;
            float sy    = (eSY != null) ? eSY.eval(time) : 1.0f;
            float sz    = (eSZ != null) ? eSZ.eval(time) : 1.0f;
            float h     = (eH != null) ? eH.eval(time) : 0.0f;
            float p     = (eP != null) ? eP.eval(time) : 0.0f;
            float b     = (eB != null) ? eB.eval(time) : 0.0f;
            Matrix4 translation = Matrix4.CreateTranslation(x, y, -z);
            Matrix4 rotation = Matrix4.CreateFromEulerAngles(h, p, b);
            Matrix4 scale = Matrix4.CreateScale(sx, sy, sz);
            return translation * rotation * scale;
        }
        public void MakeLightWaveScene()
        {
            Reset();

            LWScene scene = LWSceneParser.Load("res/Scenes/boxi3.lws");

            Debug.WriteLine(scene.Objects.Count + " objects");
            Debug.WriteLine(scene.Lights.Count + " lights");
            Debug.WriteLine(scene.Cameras.Count + " cameras");

            Material wood = materialManager["wood"];
            var loadedModels = new Dictionary<string,RenderStack.LightWave.LWModel>();
            foreach(var @object in scene.Objects)
            {
                try
                {
                    string name = "res/Objects/" + @object.Name.Split('/').Last();
                    LWModel lwModel = null;
                    if(loadedModels.ContainsKey(name))
                    {
                        lwModel = loadedModels[name];
                    }
                    else
                    {
                        loadedModels[name] = lwModel = RenderStack.LightWave.LWModelParser.Load(name);
                    }

                    foreach(var layer in lwModel.Layers.Values)
                    {
                        var mesh = new GeometryMesh(layer.Geometry, NormalStyle.CornerNormals);
                        var model = new Model(layer.Name, mesh, wood, Motion(@object, 0.0f));
                        AddModel(model);
                    }

                    Debug.WriteLine("\tObject '" + @object.Name + "' " + @object.Bones.Count + " bones @ ");
                }
                catch(System.Exception)
                {
                }
            }
            foreach(var item in scene.Cameras)
            {
                Debug.WriteLine("\tCamera '" + item.Name + "'");
            }
            foreach(var item in scene.Lights)
            {
                Debug.WriteLine("\tLight '" + item.Name + "'");
            }

            AddCameras();
            camera.Frame.LocalToParent.Set(
                Motion(scene.Cameras.First(), 0.0f)
            );
            AddCameraUserControls();
        }
        public class TreeTemplate
        {
            private int                 sliceCount;
            private int                 stackDivision;
            private int                 coneCount;
            private float               height;
            private float               radius;
            private float               radAdd;
            private List<GeometryMesh>  meshes = new List<GeometryMesh>();
            private List<Shape>         shapes = new List<Shape>();

            public  int                 SliceCount      { get { return sliceCount; } }
            public  int                 StackDivision   { get { return stackDivision; } }
            public  int                 ConeCount       { get { return coneCount; } }
            public  float               Height          { get { return height; } }
            public  float               Radius          { get { return radius; } }
            public  float               RadAdd          { get { return radAdd; } }
            public  List<GeometryMesh>  Meshes          { get { return meshes; } }
            public  List<Shape>         Shapes          { get { return shapes; } }

            public TreeTemplate(
                int     sliceCount, 
                int     stackDivision, 
                int     coneCount, 
                float   height, 
                float   radius, 
                float   radAdd
            )
            {
                var attributePosition = new Attribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3);
                var attributeNormal   = new Attribute(VertexUsage.Normal,   VertexAttribPointerType.Float, 0, 3);   /*  content normals     */
                VertexFormat vertexFormat = new VertexFormat();
                vertexFormat.Add(attributePosition);
                vertexFormat.Add(attributeNormal);

                this.sliceCount = sliceCount;
                this.stackDivision = stackDivision;
                this.coneCount = coneCount;
                this.height = height;
                this.radius = radius;
                this.radAdd = radAdd;
                float       coneHeight  = height / (float)coneCount;
                Matrix4     rotZ = Matrix4.CreateRotation(
                    RenderStack.Math.Conversions.DegreesToRadians(90.0f),
                    Vector3.UnitZ
                );
                float           cylHeight = coneHeight;
                float           cylRadius = height / 20.0f;
                Geometry        cylinderGeometry = new RenderStack.Geometry.Shapes.Cylinder(-cylHeight, cylHeight, cylRadius, sliceCount);
                cylinderGeometry.Transform(rotZ);
                GeometryMesh    cylinderMesh = new GeometryMesh(cylinderGeometry, NormalStyle.CornerNormals, vertexFormat);
                Shape           cylinderShape = new CylinderShape(cylHeight, cylRadius);
                cylinderMesh.GetMesh.Name = "cylinder";
                meshes.Add(cylinderMesh);
                shapes.Add(cylinderShape);
                for(int c = 0; c < coneCount; c++ )
                {
                    float topRadius    = (coneCount - 1 - c) * radius / (float)coneCount;
                    float bottomRadius = topRadius + radAdd;
                    float R = bottomRadius;
                    float r = topRadius;
                    float fullConeHeight = (R * coneHeight) / (R - r);
                    float minX = -fullConeHeight / 3.0f;
                    float maxX = 2.0f * fullConeHeight / 3.0f;
                    float offset = -minX;
                    Geometry coneGeometry = new RenderStack.Geometry.Shapes.Cone(minX, maxX, bottomRadius, 0.0f, true, true, sliceCount, stackDivision);
                    coneGeometry.Transform(rotZ);
                    GeometryMesh coneMesh = new GeometryMesh(coneGeometry, NormalStyle.CornerNormals, vertexFormat);
                    Shape coneShape = new ConeShape(fullConeHeight, R);
                    coneMesh.GetMesh.Name = "cone" + c.ToString();
                    meshes.Add(coneMesh);
                    shapes.Add(coneShape);
                }
            }
        }
        public void MakeTree(Vector3 pos, TreeTemplate template)
        {
            int i = 0;
            Material    wood        = materialManager["wood"]  ;
            Material    leaves      = materialManager["leaves"]  ;
            float       coneHeight  = template.Height / (float)template.ConeCount;
            Matrix4     rotZ = Matrix4.CreateRotation(
                RenderStack.Math.Conversions.DegreesToRadians(90.0f),
                Vector3.UnitZ
            );
            float           cylHeight = coneHeight;
            float           cylRadius = template.Height / 20.0f;
            GeometryMesh    cylinderMesh = template.Meshes[i];
            Shape           cylinderShape = template.Shapes[i];
            ++i;
            Model           rootModel = new Model("TreeRoot", cylinderMesh, wood, pos.X, pos.Y + cylHeight / 2.0f, pos.Z);
            AddModel(rootModel, null);
            Model           below = rootModel;
            float           prevOffset = cylHeight / 2.0f;
            for(int c = 0; c < template.ConeCount; c++ )
            {
                float topRadius    = (template.ConeCount - 1 - c) * template.Radius / (float)template.ConeCount;
                float bottomRadius = topRadius + template.RadAdd;
                float R = bottomRadius;
                float r = topRadius;
                float fullConeHeight = (R * coneHeight) / (R - r);
                float minX = -fullConeHeight / 3.0f;
                //float maxX = 2.0f * fullConeHeight / 3.0f;
                float offset = -minX;
                GeometryMesh coneMesh = template.Meshes[i];
                Shape coneShape = template.Shapes[i];
                ++i;
                Model coneModel = new Model("TreeCone", coneMesh, leaves, 0.0f, prevOffset + offset, 0.0f);
                coneModel.Frame.Parent = below.Frame;
                AddModel(coneModel, null);
                Bender bender = new Bender(Vector3.UnitZ, 0.1f, 2.0f);
                bender.Frame = coneModel.Frame;
                Add(bender);
                below = coneModel;
                prevOffset = offset;
            }
        }
        public void MakeTreeScene()
        {
            Configuration.idBuffer = false;
#if DEBUG
            float size = 40.0f;
#else
            float size = 40.0f;
#endif
            Reset();

            AddFloor(size, 5, -0.5f);

            var tree1 = new TreeTemplate(22, 0, 4, 3.0f, 0.6f, 0.6f);
            var tree2 = new TreeTemplate(26, 0, 6, 7.0f, 1.0f, 0.6f);

            Random r = new Random();
            List<Vector2>   positions = UniformPoissonDiskSampler.SampleCircle(Vector2.Zero, size / 2.0f, 3.0f);
            foreach(var pos in positions)
            {
                Vector3 p3d = new Vector3(pos.X, 0.0f, pos.Y);
                //Vector3 p3d = new Vector3(0.0, 0.0f, 0.0);
                double type = r.NextDouble();
                if(type > 0.5)
                {
                    MakeTree(p3d, tree1);
                }
                else
                {
                    MakeTree(p3d, tree2);
                }
            }

            AddCameras();
            AddCameraUserControls();
        }
        public Geometry Sqrt3(Geometry geometry)
        {
            geometry.BuildEdges();
            geometry = new Sqrt3GeometryOperation(geometry).Destination;
            geometry = new Sqrt3GeometryOperation(geometry).Destination;
            geometry = new Sqrt3GeometryOperation(geometry).Destination;
            geometry = new Sqrt3GeometryOperation(geometry).Destination;
            geometry = new Sqrt3GeometryOperation(geometry).Destination;
            return geometry;
        }
        public Geometry Mush(float amplitude, Geometry geometry)
        {
            var noise = new NoiseGenerator(4.0f, amplitude, 1.2f, 0.5f, 16);
            geometry.Noise(noise);
            return geometry;
        }
        public void MakeSimpleScene()
        {
            System.Console.WriteLine("MakeSimpleScene");
            int subdiv = 1;
            Reset();

            float scale = 1.0f;
            //sceneManager.AddFloor(22.0f * scale, 0, -1.0f);
            AddFloor(22.0f * scale, 5, -0.5f);

            /*  Renderable meshes  */
            Geometry cubeGeometry = new RenderStack.Geometry.Shapes.Cube(1.0f, 1.0f, 1.0f);
            //Geometry cubeGeometry = new RenderStack.Geometry.Shapes.Cube(Vector3.One, new IVector3(20, 20, 20), 1.0f);
            cubeGeometry = Mush(0.04f, cubeGeometry);
            /*cubeGeometry.BuildEdges();
            cubeGeometry = new SubdivideGeometryOperation(cubeGeometry).Destination;
            cubeGeometry = new SubdivideGeometryOperation(cubeGeometry).Destination;
            cubeGeometry = new SubdivideGeometryOperation(cubeGeometry).Destination;
            cubeGeometry = new CatmullClarkGeometryOperation(cubeGeometry).Destination;
            cubeGeometry.ComputePolygonCentroids();
            cubeGeometry.ComputePolygonNormals();
            cubeGeometry.ComputeCornerNormals(0.0f);
            cubeGeometry.BuildEdges();*/
            GeometryMesh cubeMesh = new GeometryMesh(cubeGeometry, NormalStyle.PolygonNormals);

#if OPENRL
            Geometry geodesatedBoxGeometry = new RenderStack.Geometry.Shapes.Cube(
                new Vector3(1.0f, 1.0f, 1.0f), new IVector3(2, 2, 2)
            );
#else
            Geometry geodesatedBoxGeometry = new RenderStack.Geometry.Shapes.Cube(
                new Vector3(1.0f, 1.0f, 1.0f), new IVector3(6, 6, 6)
            );
#endif
            geodesatedBoxGeometry.Geodesate(0.75f);

            //  This is the best geo shape but somewhat slow to create
#if false
            Geometry subdividedIcosahedron = new RenderStack.Geometry.Shapes.Icosahedron(1.00);
            //subdividedIcosahedron = Sqrt3(subdividedIcosahedron);
            //subdividedIcosahedron = Mush(0.01f, subdividedIcosahedron);
            GeometryMesh sphereMesh2 = new GeometryMesh(
                subdividedIcosahedron,
                NormalStyle.PolygonNormals
            );
#endif

#if false
            GeometryMesh discMesh = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Disc(1.0, 0.8, 32, 2),
                NormalStyle.PolygonNormals
            );
            GeometryMesh triangleMesh = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Triangle(0.8f / 0.57735027f),
                NormalStyle.PolygonNormals
            );
#endif


            GeometryMesh sphereMesh = new GeometryMesh(
                //new RenderStack.Geometry.Shapes.Sphere(0.75, 24, 14),
                geodesatedBoxGeometry,
                NormalStyle.CornerNormals
            );
#if false
#endif
            //Geometry cylinderGeometry = new RenderStack.Geometry.Shapes.Cylinder(-0.5f, 0.5f, 0.5f, 24);
#if OPENRL
            Geometry cylinderGeometry = new RenderStack.Geometry.Shapes.Cone(-0.5f, 0.5f, 0.5f, 0.5f, true, true, 5, 1);
#else
            Geometry cylinderGeometry = new RenderStack.Geometry.Shapes.Cone(-0.5f, 0.5f, 0.5f, 0.5f, true, true, 24, 1);
#endif
            cylinderGeometry.Transform(
                Matrix4.CreateRotation(
                    RenderStack.Math.Conversions.DegreesToRadians(90.0f),
                    Vector3.UnitZ
                ) 
            );
            //cylinderGeometry = Mush(0.08f, cylinderGeometry);

            GeometryMesh cylinderMesh = new GeometryMesh(cylinderGeometry, NormalStyle.CornerNormals);
#if OPENRL
            Geometry coneGeometry = new RenderStack.Geometry.Shapes.Cone(-1.0f / 3.0f, 2.0f / 3.0f, 0.75f, 0.0f, true, false, 5, 5);
#else
            Geometry coneGeometry = new RenderStack.Geometry.Shapes.Cone(-1.0f / 3.0f, 2.0f / 3.0f, 0.75f, 0.0f, true, false, 24*subdiv, 10*subdiv);
#endif
            coneGeometry.Transform(
                Matrix4.CreateRotation(
                    RenderStack.Math.Conversions.DegreesToRadians(90.0f),
                    Vector3.UnitZ
                )
            );
            //coneGeometry = Mush(0.08f, coneGeometry);
            GeometryMesh coneMesh = new GeometryMesh(coneGeometry, NormalStyle.CornerNormals);

            /*  Physics shapes  */
            Shape cubeShape     = new BoxShape(1.0f, 1.0f, 1.0f);
            Shape sphereShape   = new SphereShape(0.75f);
            Shape cylinderShape = new CylinderShape(1.0f, 0.5f);
            Shape coneShape     = new ConeShape(1.0f, 0.75f);

            /*  Models  */ 
            float gap = 2.5f;

            //var defaultPng = materialManager.Texture("res/images/default.png", true);

            Material pearl      = materialManager["pearl"]  ;
            Material gold       = materialManager["gold"]   ;
            //Material red        = materialManager.MakeTextured("red", defaultPng);
            Material red        = materialManager["red"]    ;
            //Material green      = materialManager["green"]  ;
            Material transparent = materialManager["transparent"]  ;
            Material cyan       = materialManager["cyan"]   ;
            Material blue       = materialManager["blue"]   ;
            Material magenta    = materialManager["magenta"];
            Material pink       = materialManager["pink"]   ;
            Material hsv        = materialManager["HSVColorFill"];
            Material hsv2       = materialManager["HSVColorFill2"];
            for(int z = 0; z < 1; ++z)
            {
                float Z = (float)(1 - z) * gap;
                AddModel(new Model("cube",     cubeMesh,     z == 0 ? pearl   : pearl, -2.5f * gap, 0.5f,  Z), cubeShape);
                AddModel(new Model("box",      cubeMesh,     z == 0 ? gold    : pearl, -1.5f * gap, 0.5f,  Z), cubeShape);
                AddModel(new Model("sphere",   sphereMesh,   z == 0 ? red     : pearl, -0.5f * gap, 0.75f, Z), sphereShape);
                AddModel(new Model("cylinder", cylinderMesh, z == 0 ? cyan    : pearl,  0.5f * gap, 0.5f,  Z), cylinderShape);
                AddModel(new Model("cylinder", cylinderMesh, z == 0 ? blue    : pearl,  1.5f * gap, 0.5f,  Z), cylinderShape);
                AddModel(new Model("cone",     coneMesh,     z == 0 ? magenta : pearl,  2.5f * gap, 1.0f / 3.0f, Z), coneShape);
                AddModel(new Model("cone",     coneMesh,     z == 0 ? pink    : pearl,  3.5f * gap, 1.0f / 3.0f, Z), coneShape);
                //AddModel(new Model("disc",     discMesh,     hsv,      4.5f * gap, 2.0f, Z), null);
                //AddModel(new Model("triangle", triangleMesh, hsv2,     4.5f * gap, 2.0f, Z), null);
                //AddModel(new Model("sphere",   sphereMesh2,  gold,    -0.5f * gap, 0.75f, Z), sphereShape);
                //AddModel(new Model("sphere",   sphereMesh,   transparent,   -0.5f * gap, 0.75f, Z), sphereShape);
            }

            AddCameras();
            AddCameraUserControls();

#if false
            if(RenderStack.Graphics.Configuration.useOpenRL)
            {
                var rl = Services.Get<OpenRLRenderer>();
                rl.BuildScene(renderGroup);
            }
#endif
        }
        public void MakeNoiseScene()
        {
            Reset();

            float scale = 1.0f;
            AddFloor(30.0f * scale, 5, -0.5f);

            int subdiv = 3;
            Geometry coneGeometry = new RenderStack.Geometry.Shapes.Cone(
                -1.0f / 3.0f, 2.0f / 3.0f, 0.75f, 0.0f, true, false, 24 * subdiv, 10 * subdiv
            );
            coneGeometry.Transform(
                Matrix4.CreateRotation(
                    RenderStack.Math.Conversions.DegreesToRadians(90.0f),
                    Vector3.UnitZ
                )
            );
            //coneGeometry = Mush(0.08f, coneGeometry);
            var coneMesh  = new GeometryMesh(coneGeometry, NormalStyle.CornerNormals);
            var coneShape = new ConeShape(1.0f, 0.75f);
            var gold      = materialManager["noisy"];
            var magenta   = materialManager["magenta"];

            userInterfaceManager.CurrentMaterial = "noisy";

            Geometry ellipsoidGeometry = new RenderStack.Geometry.Shapes.Ellipsoid(
                new Vector3(0.5f, 1.0f, 1.5f), 20, 12 
            );

            var ellipsoidMesh = new GeometryMesh(ellipsoidGeometry, NormalStyle.PointNormals);
            var ellipsoidShape = new EllipsoidShape(0.5f, 1.0f, 1.5f);

            for(float x = -4.0f; x <= 4.0f; x += 8.0f)
            {
                for(float z = -4.0f; z <= 4.0f; z += 8.0f)
                {
                    AddModel(new Model("cone",      coneMesh,      gold,    x, 1.0f / 3.0f, z), coneShape).RigidBody.IsActive = false;
                    AddModel(new Model("ellipsoid", ellipsoidMesh, magenta, x, 2.0f, z), ellipsoidShape).RigidBody.IsActive = false;
                }
            }

            AddCameras();
            AddCameraUserControls();
        }
        public void MakeBoxScene()
        {
            Reset();

            float scale = 1.0f;
            //sceneManager.AddFloor(22.0f * scale, 0, -1.0f);
            AddFloor(30.0f * scale, 5, -0.5f);

#if true
            float gap = 10.0f;

            Material magenta = materialManager["magenta"];
            for(float x = 0.5f; x <= 2.0f; x += 0.5f)
            {
                for(float z = 0.5f; z <= 2.0f; z += 0.5f)
                {
                    Geometry g = new RenderStack.Geometry.Shapes.Cube(
                        new Vector3(x, 1.0f, z), new IVector3(2, 2, 2)
                    );
                    //g.Geodesate();

                    GeometryMesh mesh = new GeometryMesh(
                        g,
                        NormalStyle.CornerNormals
                    );
                    Shape shape = new BoxShape(x, 1.0f, z);
                    //Shape shape = new RenderStack.Physics.SphereShape(1.0f);
                    AddModel(
                        new Model(
                            "Cube(" + x +", 1.0, " + z + ")",
                            mesh,
                            magenta,
                            -10.0f + (float)(x * gap),
                            0.5f,
                            -10.0f + (float)(z * gap)
                        ), 
                        shape
                    );
                }
            }
#endif

            AddCameras();
            AddCameraUserControls();
        }
        public void MakeStereoscopicScene()
        {
            Reset();

            float size = AddStereoscopicTest();
            AddFloor(size);
            FloorModel.RigidBody.Material.Restitution      = 1.0f;
            FloorModel.RigidBody.Material.StaticFriction   = 0.0f;
            FloorModel.RigidBody.Material.DynamicFriction  = 0.0f;

            AddCameras();
            AddCameraUserControls();
        }
        public float AddStereoscopicTest()
        {
            float           radius  = 0.5f;
            float           spread  = 3.0f;
            int             xCount  = 6;
            int             zCount  = 6;
            bool            slow    = Configuration.slow;
            int             step    = slow ? 2 : 1;
            GeometryMesh    mesh    = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Sphere(
                    radius, 
                    slow ? 14 : 20, 
                    slow ? 8 : 12
                ),
                NormalStyle.PointNormals
            );

            SphereShape shape = new SphereShape(radius);

            for(int x = 0; x < xCount; x += step)
            {
                float xRel = (float)(x) / (float)(xCount - 1);
                float r, g, b;
                RenderStack.Math.Conversions.HSVtoRGB(xRel * 360.0f, 1.0f, 1.0f, out r, out g, out b);
                var material = materialManager.MakeSimpleMaterial("stereo" + xRel.ToString(), r, g, b);
                xRel = (2.0f * xRel) - 1.0f;
                for(int z = 0; z < zCount; z += step)
                {
                    Model newModel = new Model(
                        "stereo sphere " + x + " " + z,
                        mesh,
                        material,
                        spread * ((float)(x) - (float)(xCount - 1.0) / 2.0f),   //  X
                        radius,                                                 //  Y
                        spread * ((float)(z) - (float)(zCount - 1.0) / 2.0f)    //  z
                    );
                    newModel.PhysicsShape = shape;
                    newModel.Static = false;
                    AddModel(newModel);
                    newModel.RigidBody.Material.Restitution      = 1.0f;
                    newModel.RigidBody.Material.StaticFriction   = 0.0f;
                    newModel.RigidBody.Material.DynamicFriction  = 0.0f;
                }
            }

            float size = 2.0f * (spread * ((float)(xCount - 1.0f) / 2.0f) + radius * 4.0f);
            return size;
        }
        public void MakeGameScene()
        {
            RuntimeConfiguration.gameTest = true;
            Configuration.idBuffer = false;
            Reset();
            AddCameras();
            AddCameraUserControls();
            AddFloor(35.0f);
            var game = Services.Get<Game>();
            game.Reset();
            userInterfaceManager.UserControls = game.Player.Controller;
        }
        public void MakeCurveScene()
        {
            var curveTool = Services.Get<CurveTool.CurveTool>();
            if(curveTool != null)
            {
                RuntimeConfiguration.curveTool = true;
                Reset();
                AddCameras();
                AddCameraUserControls();
                AddFloor(35.0f);
                floorModel = null;
                floorSize = 35.0f;
                curveTool.Reset();
                curveTool.Enabled = true;
            }
        }
        public void MakeBrushScene()
        {
            Reset();
            float size = AddBrushModels() + 5.0f;
            AddFloor(size);
            AddCameras(1.0f);
            AddCameraUserControls();
#if false
            if(RenderStack.Graphics.Configuration.useOpenRL)
            {
                var rl = Services.Get<OpenRLRenderer>();
                if(rl != null)
                {
                    rl.BuildScene(renderGroup);
                }
            }
#endif
        }
        public float AddBrushModels()
        {
            if(brushManager == null || brushManager.Brushes == null)
            {
                return 10.0f;
            }

            List<ModelInfo> addList = new List<ModelInfo>();

            bool  tooSmall;
            float size = 2.0f;
            float gap = 1.0f;
            do
            {
                Message(
                    "SceneManager: AddTestSceneObjects trying size " 
                    + size.ToString() 
                    + "..."
                );
                tooSmall = false;
                RectangleBinPack packer = new RectangleBinPack(size, size);

                //foreach(Brush brush in brushManager.Brushes)
                //Brush brush = Palette.Brushes.First();
                int step = Configuration.slow ? 6 : 3;
                for(int i = 0; i < brushManager.Brushes.Count; i += step)
                {
                    Brush brush = brushManager.Brushes[i];
                    // these are broken in one way or another, or can be created from others
                    if(
                        brush.Name.Contains("elongated") ||
                        brush.Name.Contains("augmented") ||
                        brush.Name.Contains("small") ||
                        brush.Name.Contains("dodecadodecahedron") ||
                        brush.Name.Contains("great stellated dodecahedron") ||
                        brush.Name.Contains("gyrobifastigium") 
                    )
                    {
                        continue;
                    }
                    //Brush brush = Palette.Brushes[i];
                    RectangleBinPack.Node node = packer.Insert(
                        brush.BoundingBox.Size.X + gap * 2.0f, 
                        brush.BoundingBox.Size.Z + gap * 2.0f
                    );
                    if(node == null)
                    {
                        tooSmall = true;
                        size += 0.25f;
                        addList.Clear();
                        break;
                    }
                    ModelInfo info = new ModelInfo();
                    info.brush = brush;
                    info.x = -size / 2.0f + node.X + brush.BoundingBox.HalfSize.X + gap;
                    info.z = -size / 2.0f + node.Y + brush.BoundingBox.HalfSize.Z + gap;
                    addList.Add(info);
                }
            }
            while(tooSmall == true);

            int count = addList.Count;
            Trace.TraceInformation(
                "AddTestSceneObjects using size: " 
                + size.ToString() 
                + " objects: " + count.ToString()
            );

            foreach(ModelInfo info in addList)
            {
#if DEBUG
                int layerCount = 1;
#else
                int layerCount = 1;
#endif
                for(int i = 0; i < layerCount; ++i)
                {
                    Model newModel = new Model(
                        info.brush.Model.Name,
                        info.brush.Model.Batch.MeshSource,
                        materialManager["Default"],
                        //materialManager["transparent"],
                        info.x,                                 //  X
                        0.2f + -info.brush.BoundingBox.Min.Y + (float)i * (gap * 0.1f + info.brush.BoundingBox.Size.Y),   //  Y
                        info.z                                  //  Z
                    );

                    if(Configuration.physics)
                    {
                        newModel.PhysicsShape = info.brush.ScaledShape(1.0f);
                        newModel.Static = false;
                    }
                    AddModel(newModel);
                }

                /*Message(
                    "SceneManager: AddTestSceneObjects AddModel " 
                    + j.ToString()
                    + "/"
                    + count.ToString()
                );*/
            }

            return size;
        }
    }
}
