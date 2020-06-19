﻿//  Copyright (C) 2011 by Timo Suoranta                                            
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
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Geometry;
using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.Mesh;
using RenderStack.Physics;

namespace example.Renderer
{
    // \todo [Serializable]

    // \brief A renderable 3D object which supports shadow casting, ID rendering, physics, selection
    public partial class Model
    {
        private Frame       frame = new Frame();
        private string      name;
        private bool        selected;
    }
#if true
    public partial class Model : IPhysicsObject
    {
        private bool        useRotation = true;
        public Shape        PhysicsShape    { get; set; } 
        public RigidBody    RigidBody       { get; set; }
        public bool         Static          { get; set; }
        public bool         ShadowCaster    { get { return true; } } // \todo hack ?
        public bool         UsePosition     { get { return true; } }
        public bool         UseRotation     { get { return useRotation; } set { useRotation = value; } }
    }
#endif
    public partial class Model : IDisposable
    {
        // \todo List of Batches instead of a single Batch
        public string       Name            { get { return name; } set { name = value; } }
        public bool         Selected        { get { return selected; } set { selected = value; } }
        public Frame        Frame           { get { return frame; } }
        public Batch        Batch           { get; set; }
        public UInt32       ShadowIDOffset  { get; set; }

        public Model(string name, IMeshSource meshSource, Matrix4 localToParent)
        {
            this.name   = name;
            Batch       = new Batch(meshSource);
            frame.Name  = name + " frame";
            frame.LocalToParent.Set(localToParent);
            //Static      = true;
        }
        public Model(string name, IMeshSource meshSource, Material material)
        {
            Name        = name;
            Batch       = new Batch(meshSource, material);
            frame.Name  = name + " frame";
            //Static      = true;
        }
        public Model(string name, IMeshSource meshSource, Material material, float x, float y, float z)
        {
            Name        = name;
            Batch       = new Batch(meshSource, material);
            frame.Name  = name + " frame";
            frame.LocalToParent.SetTranslation(x, y, z);
            //Static      = true;
        }
        public Model(string name, IMeshSource meshSource, Material material, Vector3 p)
        {
            Name        = name;
            Batch       = new Batch(meshSource, material);
            frame.Name  = name + " frame";
            frame.LocalToParent.SetTranslation(p);
            //Static      = true;
        }
        public Model(string name, IMeshSource meshSource, Material material, Matrix4 localToParent)
        {
            Name        = name;
            Batch       = new Batch(meshSource, material);
            frame.Name  = name + " frame";
            frame.LocalToParent.Set(localToParent);
            //Static      = true;
        }

        private bool disposed = false;
        ~Model()
        {
            Dispose();
        }
        public void Dispose()
        {
            disposed = true;
        }

        public override string ToString()
        {
            Vector3 pos = Frame.LocalToWorld.Matrix.GetColumn3(3);
            return Name + " @ " + pos + " (" + Batch.Material.Name + ")";
        }

#if false
        public void UpdateOctree()
        {
            GeometryMesh            mesh            = Batch.MeshSource as GeometryMesh;
            Geometry                geometry        = mesh.Geometry;
            var                     pointLocations  = geometry.PointAttributes.Find<Vector3>("point_locations");
            Dictionary<Point,int>   pointIndices    = new Dictionary<Point,int>();

            List<Vector3>               positions = new List<JVector>();
            List<TriangleVertexIndices> triangles = new List<TriangleVertexIndices>();

            for(int i = 0; i < geometry.Points.Count; ++i)
            {
                Point   point   = geometry.Points[i];
                Vector3 p       = pointLocations[point];
                positions.Add(new JVector(p.X, p.Y, p.Z));
                pointIndices[point] = i;
            }

            foreach(Polygon polygon in geometry.Polygons)
            {
                Point   firstPoint      = polygon.Corners.First().Point;
                int     firstIndex      = pointIndices[firstPoint];
                int     previousIndex   = firstIndex;
                foreach(Corner corner in polygon.Corners)
                {
                    Point   point           = corner.Point;
                    int     currentIndex    = pointIndices[point];

                    if(previousIndex != firstIndex)
                    {
                        triangles.Add(
                            new TriangleVertexIndices(
                                firstIndex,
                                currentIndex,
                                previousIndex
                            )
                        );
                    }
                    previousIndex = currentIndex;
                }
            }
            Octree = new Octree(positions, triangles);
        }
#endif
    }
}
