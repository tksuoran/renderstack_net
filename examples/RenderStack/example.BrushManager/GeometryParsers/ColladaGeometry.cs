//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if false
using System.Collections.Generic;

using RenderStack.Geometry;
using RenderStack.Math;

using Collada;

namespace Sandbox
{
    public class ColladaGeometry : Geometry
    {
        public ColladaGeometry(string file)
        {
            var collada = Grendgine_Collada.Grendgine_Load_File(file);

            foreach(var geometry in collada.Library_Geometries.Geometry)
            {
                var mesh = geometry.Mesh;
                var sources = mesh.Source;
                var vertices = mesh.Vertices;
                foreach(var input in vertices.Input)
                {
                    switch(input.Semantic)
                    {
                    }
                }
                
                foreach(var polygon in geometry.Mesh.Polygons)
                {
                    // input points to sources either directly 
                    // or through sources defined in vertices
                    // (vertex semantics)
                    foreach(var input in polygon.Input)
                    {
                    }
                    foreach(int index in polygon.P.Value())
                    {
                        
                    }
                }
            }

        }
    }
}
#endif