//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;
using System.Collections.Generic;
using System.Diagnostics;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;

namespace example.Scene
{
    public partial class SceneManager : Service
    {
        public override string Name
        {
            get { return "SceneManager"; }
        }

        // Services
        MaterialManager             materialManager;
        private Camera              camera;
        private Group               renderGroup     = new Group();
        private FrameController     cameraControls  = new FrameController();

        public Camera               Camera          { get { return camera; } }
        public Group                RenderGroup     { get { return renderGroup; } }
        public FrameController      CameraControls  { get { return cameraControls; } }

        public void Connect(
            MaterialManager	materialManager
        )
        {
            this.materialManager    = materialManager;

            InitializationDependsOn(materialManager);
        }

        protected override void InitializeService()
        {
            AddFloor(20.0f);
            AddSimpleScene();

            InitializeCameras();
        }

        public Model AddModel(Model model)
        {
            if(model == null)
            {
                throw new System.ArgumentNullException();
            }

            renderGroup.Models.Add(model);

            return model;
        }

        public void AddFloor(float size)
        {
            Geometry g = new Cube(size, 1.0, size);
            g = new SubdivideGeometryOperation(g).Destination;
            g = new SubdivideGeometryOperation(g).Destination;
            g = new SubdivideGeometryOperation(g).Destination;
            GeometryMesh floorMesh = new GeometryMesh(g, NormalStyle.PolygonNormals);

            var floorModel = new Model(
                "Cube (floor)",
                floorMesh,
                materialManager.GridMaterial,
                0.0f, -0.5f, 0.0f
            );

            AddModel(floorModel);
        }

        public void AddSimpleScene()
        {
            //  Shapes here have local 0,0,0 at center of mass
            Geometry cubeGeometry = new RenderStack.Geometry.Shapes.Cube(1.0f, 1.0f, 1.0f);
            GeometryMesh cubeMesh = new GeometryMesh(cubeGeometry, NormalStyle.PolygonNormals);

            GeometryMesh sphereMesh = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Sphere(0.75, 20, 12),
                NormalStyle.CornerNormals
            );
            Geometry cylinderGeometry = new RenderStack.Geometry.Shapes.Cylinder(-0.5f, 0.5f, 0.5f, 24);
            cylinderGeometry.Transform(
                Matrix4.CreateRotation(
                    Conversions.DegreesToRadians(90.0f),
                    Vector3.UnitZ
                ) 
            );
            GeometryMesh cylinderMesh = new GeometryMesh(cylinderGeometry, NormalStyle.CornerNormals);
            Geometry coneGeometry = new RenderStack.Geometry.Shapes.Cone(-1.0f / 3.0f, 2.0f / 3.0f, 0.75f, 0.0f, true, false, 24, 10);
            coneGeometry.Transform(
                Matrix4.CreateRotation(
                    Conversions.DegreesToRadians(90.0f),
                    Vector3.UnitZ
                )
            );
            GeometryMesh coneMesh = new GeometryMesh(coneGeometry, NormalStyle.CornerNormals);

            /*  Models  */ 
            float gap = 2.5f;
            Material pearl      = materialManager["pearl"]  ;
            Material gold       = materialManager["gold"]   ;
            Material red        = materialManager["red"]    ;
            Material green      = materialManager["green"]  ;
            Material cyan       = materialManager["cyan"]   ;
            Material blue       = materialManager["blue"]   ;
            Material magenta    = materialManager["magenta"];
            Material pink       = materialManager["pink"]   ;
            AddModel(new Model("cube",     cubeMesh,     pearl,  -3.5f * gap, 0.5f,         0.0f));
            AddModel(new Model("box",      cubeMesh,     gold,   -2.5f * gap, 0.5f,         0.0f));
            AddModel(new Model("sphere",   sphereMesh,   red,    -1.5f * gap, 0.75f,        0.0f));
            AddModel(new Model("sphere",   sphereMesh,   green,  -0.5f * gap, 0.75f,        0.0f));
            AddModel(new Model("cylinder", cylinderMesh, cyan,    0.5f * gap, 0.5f,         0.0f));
            AddModel(new Model("cylinder", cylinderMesh, blue,    1.5f * gap, 0.5f,         0.0f));
            AddModel(new Model("cone",     coneMesh,     magenta, 2.5f * gap, 1.0f / 3.0f,  0.0f));
            AddModel(new Model("cone",     coneMesh,     pink,    3.5f * gap, 1.0f / 3.0f,  0.0f));
        }
        private void InitializeCameras()
        {
            camera = new Camera(); camera.Name = "camera";

            camera.FovYRadians      = Conversions.DegreesToRadians(50.0f);
            camera.ProjectionType   = ProjectionType.PerspectiveVertical;
            camera.Near             =  0.02f;
            camera.Far              = 40.00f;

            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    new Vector3(0.0f, 4.0f, 15.0f),
                    Vector3.Zero,
                    Vector3.UnitY
                )
            );

            cameraControls.Frame = camera.Frame;
        }
    }
}
