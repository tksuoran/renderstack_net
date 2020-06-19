//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;

using example.Renderer;
using example.Brushes;

namespace example.Sandbox
{
    public partial class Operations : Service
    {
        // Services
        BrushManager            brushManager;
        MaterialManager         materialManager;
        OperationStack          operationStack;
        SceneManager            sceneManager;
        SelectionManager        selectionManager;
        Sounds                  sounds;
        UserInterfaceManager    userInterfaceManager;

        public void Connect(
            BrushManager            brushManager,
            MaterialManager         materialManager,
            OperationStack          operationStack,
            SceneManager            sceneManager,
            SelectionManager        selectionManager,
            Sounds                  sounds,
            UserInterfaceManager    userInterfaceManager
        )
        {
            this.brushManager = brushManager;
            this.materialManager = materialManager;
            this.operationStack = operationStack;
            this.sceneManager = sceneManager;
            this.selectionManager = selectionManager;
            this.sounds = sounds;
            this.userInterfaceManager = userInterfaceManager;

            InitializationDependsOn(brushManager);
            InitializationDependsOn(materialManager);
            InitializationDependsOn(operationStack);
            InitializationDependsOn(sceneManager);
            InitializationDependsOn(selectionManager);
            InitializationDependsOn(sounds);
            InitializationDependsOn(userInterfaceManager);
        }

        protected override void InitializeService()
        {
        }

        public void Subdivide(Model model)
        {
            if(model == null)
            {
                return;
            }
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Geometry newGeometry = new SubdivideGeometryOperation(mesh.Geometry).Destination;
            newGeometry.ComputePolygonCentroids();
            newGeometry.ComputePolygonNormals();
            //newGeometry.ComputeCornerNormals(0.0f);
            newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (0.0f * (float)Math.PI));
            newGeometry.BuildEdges();

            MeshModified op = new MeshModified(
                model,
                new MeshModified.State(
                    model.Name, 
                    model.Batch
                ),
                new MeshModified.State(
                    "Subdivide(" + model.Name + ")", 
                    new Batch(
                        new GeometryMesh(newGeometry, NormalStyle.PolygonNormals),
                        model.Batch.Material
                    )
                )
            );

            operationStack.Do(op);

            //model.MeshSource = new GeometryMesh(newGeometry);
            //model.Name = "Subdivide(" + model.Name + ")";
        }

        public static ReferenceFrame MakePolygonReference(Geometry geometry, Polygon polygon)
        {
            ReferenceFrame frame = new ReferenceFrame();
            var     pointLocations      = geometry.PointAttributes.Find<Vector3>("point_locations");
            var     polygonNormals      = geometry.PolygonAttributes.Find<Vector3>("polygon_normals");
            var     polygonCentroids    = geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");

            Corner  firstCorner         = polygon.Corners.First();
            Corner  lastCorner          = polygon.Corners.Last();

            Point   firstPoint          = firstCorner.Point;
            Point   lastPoint           = lastCorner.Point;

            frame.Center        = polygonCentroids[polygon];
            frame.Normal        = polygonNormals[polygon];
            frame.LastPosition  = pointLocations[lastPoint];
            frame.FirstPosition = pointLocations[firstPoint];
            return frame;
        }
    }
}
