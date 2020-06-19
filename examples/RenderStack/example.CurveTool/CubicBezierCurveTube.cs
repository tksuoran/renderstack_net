//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if false
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

using Buffer = RenderStack.Graphics.Buffer;
using Attribute = RenderStack.Graphics.Attribute;

namespace Sandbox.CurveTool
{
    /*  Comment: Experimental  */ 
    public class CubicBezierCurveTube : CurveTube
    {
        public CubicBezierCurveTube(ICurve curve):base(curve)
        {
        }

        public override void UpdateTubeMeshWithAdaptiveSubdivision()
        {
            tubeStackCount = 0;
            adaptivePoints.Clear();

            /*  First test if we have a case where are points are on single line  */ 
            if(((CubicBezier)(curve)).IsFlat(0.005f))
            {
                //  All points are on the same line. Just draw
                //  straight tube from start to end.
                Vector3 T   = Vector3.Normalize(curve[3] - curve[0]);
                Vector3 N   = T.MinAxis;
                Vector3 B   = Vector3.Normalize(Vector3.Cross(T, N));
                LastN       = Vector3.Normalize(Vector3.Cross(B, T));
                adaptivePoints.Add(0.0f);
                TubeRingCap(0.0f, curve[0], curve[3]);
                TubeRing(0.0f, curve[0], curve[3]);
                adaptivePoints.Add(1.0f);
                TubeRing(1.0f, curve[3], curve[3] + (curve[3] - curve[0]));
                TubeRingCap(1.0f, curve[3], curve[3] + (curve[3] - curve[0]));
            }
            else
            {
                //  Not all points are on the same line.
                //  This is the typical case.
                adaptivePoints.Add(0.0f);
                GenerateTubeVertexRing(0.0f, true);
                GenerateTubeVertexRing(0.0f, false);
                GenerateTubeVerticesForCurveSegment(curve[0], curve[1], curve[2], curve[3], 0.0f, 1.0f);
                adaptivePoints.Add(1.0f);
                GenerateTubeVertexRing(1.0f, false);
                GenerateTubeVertexRing(1.0f, true);
            }
        }
        public void GenerateTubeVerticesForCurveSegment(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t0, float t1)
        {
            float tm = t0 + (t1 - t0) * 0.5f;               //  middle t

            Vector3 P3P0    = P3 - P0;                      //  straight line from start to end
            Vector3 P1P0    = P1 - P0;                      
            Vector3 P3P2    = P3 - P2;                      
            Vector3 P3P0_   = Vector3.Normalize(P3P0);      //  tangent for straight line from start to end
            Vector3 P1P0_   = Vector3.Normalize(P1P0);      //  tangent for start
            Vector3 P3P2_   = Vector3.Normalize(P3P2);      //  tangent for end
            float   d1      = Vector3.Dot(P1P0_, P3P0_);    //  measure how much start tangent direction differs from straight line from start to end
            float   d2      = Vector3.Dot(P3P2_, P3P0_);    //  measure how much end tangent direction differs from straight line from start to end
            float   d3      = Vector3.Dot(P1P0_, P3P2_);    //  measure how much start tangent direction differs from end tangent direction

            //  Compares how much tangents P0..P1 and P2..P3 deviate from each other
            //  and P0..P3. If there is sufficient difference, we must subdivide,
            //  or handle cusp if we have one.
            if(d1 < AdaptiveEpsilon || d2 < AdaptiveEpsilon || d2 < AdaptiveEpsilon)
            {
                if(t1 - t0 < 0.005f)
                {
                    //  Cusp was detected. At cusp points, direction turns 
                    //  extremelyquickly, tangent vanishes, and we can not 
                    //  get tangent interpolation by subdividing t. Cusps 
                    //  are detected by t subdivision threshold.
                    GenerateTubeVerticesForCusp(P1P0_, P3P2_, t0, t1);
                }
                else
                {
                    //  Normal subdivision. Split curve to halves and
                    //  recursively process both halves.
                    Vector3 P01 = (1.0f / 2.0f) * (P0 + P1);
                    Vector3 P02 = (1.0f / 4.0f) * (P0 + 2.0f *  P1 + P2);
                    Vector3 PM  = (1.0f / 8.0f) * (P0 + 3.0f * (P1 + P2) + P3);
                    Vector3 P11 = (1.0f / 4.0f) * (P3 + 2.0f *  P2 + P1);
                    Vector3 P12 = (1.0f / 2.0f) * (P2 + P3);
                    GenerateTubeVerticesForCurveSegment(P0, P01, P02, PM, t0, tm);
                    GenerateTubeVerticesForCurveSegment(PM, P11, P12, P3, tm, t1);
                }
            }
            else
            {
                //  Sufficiently flat curve segment, just add ring at midpoint
                adaptivePoints.Add(tm);
                GenerateTubeVertexRing(tm, false);
            }
        }

    }
}


/*

http://stackoverflow.com/questions/1030596/drawing-hermite-curves-in-opengl

You can convert a cubic Hermite curve to a cubic
Bezier curve. It's actually quite simple.

A typical cubic Hermite curve is defined with
two points and two vectors:

P0 -- start point
V0 -- derivative at P0
P1 -- end point
V1 -- derivative at P1
The conversion to a cubic Bezier is simply:

B0 = P0
B1 = P0 + V0/3
B2 = P1 - V1/3
B3 = P1

--

Let the vector of control points for your Bezier
be [b0 b1 b2 b3] and those for your Hermite be
[h0 h1 v0 v1] (v0 and v1 being the derivative /
tangent at points h0 and h1). Then we can use a
matrix form to show the conversions:

Hermite to Bezier

[b0] = 1 [ 3  0  0  0] [h0]
[b1]   - [ 3  0  1  0] [h1]
[b2]   3 [ 0  3  0 -1] [v0]
[b3]     [ 0  3  0  0] [v1]

(this is exactly as in Naaff's response, above).

Bezier to Hermite

[h0] = [ 1  0  0  0] [b0]
[h1]   [ 0  0  0  1] [b1]
[v0]   [-3  3  0  0] [b2]
[v1]   [ 0  0 -3  3] [b3]

So in matrix form these are perhaps slightly more
complex than needed (after all Naaff's code was short
and to the point). It is useful, because we can go
beyond Hermites very easily now.

In particular we can bring in the other classic
cardinal cubic parametric curve: the Catmull-Rom
curve. It has control points [c_1 c0 c1 c2] (unlike
Bezier curves, the curve runs from the second to the
third control point, hence the customary numbering
from -1). The conversions to Bezier are then:

Catmull-Rom to Bezier

[b0] = 1 [ 0  6  0  0] [c_1]
[b1]   - [-1  6  1  0] [c0]
[b2]   6 [ 0  1  6 -1] [c1]
[b3]     [ 0  0  6  0] [c2]
Bezier to Catmull-Rom

[c_1] = [ 6 -6  0  1] [b0]
[c0]    [ 1  0  0  0] [b1]
[c1]    [ 0  0  0  1] [b2]
[c2]    [ 1  0 -6  6] [b3]

*/

#endif