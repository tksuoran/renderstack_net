//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Physics;

using example.Renderer;

using Sphere = RenderStack.Geometry.Shapes.Sphere;

namespace example.Brushes
{
    /*  Comment: Highly experimental  */ 
    public class Brush 
    {
        private BrushManager    brushManager;
        private string          name;

        public static float     Radius = 1.0f;

        public Model            Model;
        public BoundingBox      BoundingBox;
        public string           Name { get { return name; } }

        public Dictionary<int, Polygon> PolygonDictionary = new Dictionary<int,Polygon>();

        Dictionary<int, Shape>  scaledShapes = new Dictionary<int, Shape>();
        Dictionary<int, GeometryMesh> scaledMeshes = new Dictionary<int,GeometryMesh>();
        public Shape ScaledShape(float scale)
        {
            int iScale = (int)(scale * 300.0f);
            if(scaledShapes.ContainsKey(iScale))
            {
                return scaledShapes[iScale];
            }
            scaledShapes[iScale] = CreateScaledShape(scale);
            return scaledShapes[iScale];
        }
        public GeometryMesh ScaledMesh(float scale)
        {
            int iScale = (int)(scale * 300.0f);
            if(scaledMeshes.ContainsKey(iScale))
            {
                return scaledMeshes[iScale];
            }
            scaledMeshes[iScale] = CreateScaledMesh(scale);
            return scaledMeshes[iScale];
        }

        private GeometryMesh CreateScaledMesh(float scale)
        {
            GeometryMesh brushPolyMesh = Model.Batch.MeshSource as GeometryMesh;
            if(scale != 1.0f)
            {
                Geometry newGeometry = new CloneGeometryOperation(brushPolyMesh.Geometry, null).Destination;
                Matrix4  scaleTransform;
                Matrix4.CreateScale(scale, out scaleTransform);
                newGeometry.Transform(scaleTransform);
                newGeometry.ComputePolygonCentroids();
                newGeometry.ComputePolygonNormals();
                //newGeometry.ComputeCornerNormals(0.0f);
                newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (0.0f * (float)Math.PI));
                newGeometry.BuildEdges();

                return new GeometryMesh(newGeometry, NormalStyle.PolygonNormals);
            }
            return brushPolyMesh;
        }

        private Shape CreateScaledShape(float scale)
        {
            GeometryMesh    mesh            = Model.Batch.MeshSource as GeometryMesh;
            Geometry        geometry        = mesh.Geometry;
            var             pointLocations  = geometry.PointAttributes.Find<Vector3>("point_locations");

            List<Vector3> positions = new List<Vector3>();

            for(int i = 0; i < geometry.Points.Count; ++i)
            {
                Point point = geometry.Points[i];
                Vector3 p = scale * pointLocations[point];
                //Vector3 n = Vector3.Normalize(p);
                //p -= 0.001f * n;
                positions.Add(p);
            }

            //  See if inertia cache entry is available
            {
                InertiaData cacheEntry = brushManager.InertiaCache[Name];
                if(string.IsNullOrEmpty(Name) || cacheEntry == null)
                {
                    //  Cache not available
                    ConvexHullShape shape = new ConvexHullShape(positions);

                    if(string.IsNullOrEmpty(Name) == false)
                    {
                        brushManager.InertiaCache[Name] = new InertiaData(shape.Inertia, shape.Mass);
                    }
                    return shape;
                }
                else
                {
                    //  Cache available
                    float scaleSquared = scale * scale;
                    float scaleCubed = scale * scale * scale;
                    return new ConvexHullShape(
                        positions, 
                        cacheEntry.Inertia * (scaleSquared * scaleCubed),
                        scaleCubed * cacheEntry.Mass
                    );
                }

            }
        }

        public Brush(BrushManager brushManager, string name, GeometryMesh mesh)
        {
            this.brushManager = brushManager;
            this.name = name;

            Geometry g = mesh.Geometry;
            /*g = new SubdivideGeometryOperation(g).Destination;
            g = new SubdivideGeometryOperation(g).Destination;
            g = new CatmullClarkGeometryOperation(g).Destination;
            Model = new Model(name,  new GeometryMesh(g, NormalStyle.PointNormals), brushManager.MaterialManager.Materials["EdgeLines"]);
            */

            Model = new Model(name, mesh, brushManager.MaterialManager.Materials["BrushDefault"]);
            BoundingBox.Clear();

            var pointLocations = mesh.Geometry.PointAttributes.Find<Vector3>("point_locations");

            //  Setup polygon dictionary
            foreach(Polygon polygon in mesh.Geometry.Polygons)
            {
                int cornerCount = polygon.Corners.Count;
                if(PolygonDictionary.ContainsKey(cornerCount) == false)
                {
                    PolygonDictionary[cornerCount] = polygon;
                }
            }

            //  Compute bounding box
            foreach(Point point in mesh.Geometry.Points)
            {
                BoundingBox.ExtendBy(pointLocations[point]);
            }

            if(Configuration.physics)
            {
                CreateScaledShape(1.0f);
                Model.PhysicsShape = ScaledShape(1.0f);
            }
        }

    }

    public partial class BrushManager
    {
        public void RegisterBasicBrushes()
        {
            MakeBrush("Cube",           new GeometryMesh( new Cube(1.0),            NormalStyle.PointNormals));
            MakeBrush("Tetrahedron",    new GeometryMesh(new Tetrahedron(1.0),      NormalStyle.PolygonNormals));
            MakeBrush("Cuboctahedron",  new GeometryMesh(new Cuboctahedron(1.0),    NormalStyle.PolygonNormals));
            MakeBrush("Octahedron",     new GeometryMesh(new Octahedron(1.0),       NormalStyle.PolygonNormals));
            MakeBrush("Icosahedron",    new GeometryMesh(new Icosahedron(1.0),      NormalStyle.PolygonNormals));
            MakeBrush("Dodecahedron",   new GeometryMesh(new Dodecahedron(1.0),     NormalStyle.PolygonNormals));
            MakeBrush("Sphere",         new GeometryMesh(new Sphere(1.0, 14, 6),    NormalStyle.PointNormals));
            MakeBrush("Cone",           new GeometryMesh(new Cone(0.0, 2.0 * 1.0, 1.0, 0.0, true, false, 12, 2), NormalStyle.CornerNormals));
            MakeBrush("Cylinder",       new GeometryMesh(new Cylinder(0.0, 2.0 * 1.0, 1.0, 20), NormalStyle.CornerNormals));
        }
    }
}
