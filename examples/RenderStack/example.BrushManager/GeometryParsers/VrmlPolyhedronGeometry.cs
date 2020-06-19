//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

using RenderStack.Geometry;
using RenderStack.Math;

namespace example.Brushes
{
    /*  Comment: Experimental  */ 
    [System.Serializable]
    public class VrmlPolyhedronGeometry : Geometry
    {
        public VrmlPolyhedronGeometry(string file)
        {
            var vrmlPolyhedron = new VrmlPolyhedron(file);
            try
            {
                foreach(Vector3 location in vrmlPolyhedron.Locations)
                {
                    Vector3 p = location;
                    p.Y += 1.0f;
                    MakePoint(p);
                }
                foreach(List<int> polygon in vrmlPolyhedron.Polygons)
                {
                    MakePolygon(polygon);
                }
            }
            catch(System.Exception e)
            {
                System.Diagnostics.Trace.TraceError("Could not parse " + file + ": " + e.ToString());
            }

        }
    }
}
