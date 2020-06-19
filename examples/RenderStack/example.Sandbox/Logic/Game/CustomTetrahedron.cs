﻿//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using RenderStack.Geometry;
using RenderStack.Math;

namespace example.Sandbox
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class CustomTetrahedron : Geometry
    {
        public CustomTetrahedron(float width, float height, float length) // width height length
        {
            var nose    = MakePoint(         0.0f,   0.0f, -2.0f * length / 3.0f);
            var backLB  = MakePoint(-0.5f * width,   0.0f,  1.0f * length / 3.0f);
            var backRB  = MakePoint( 0.5f * width,   0.0f,  1.0f * length / 3.0f);
            var backCT  = MakePoint(         0.0f, height,  1.0f * length / 3.0f);
            MakePolygon(backRB, backLB, backCT);    // back
            MakePolygon(backLB, backRB, nose);      // bottom
            MakePolygon(backCT, backLB, nose);      // left side
            MakePolygon(backRB, backCT, nose);      // right side
        }
    }
}
