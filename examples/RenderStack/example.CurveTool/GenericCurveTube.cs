//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

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

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;

namespace example.CurveTool
{
    /*  Comment: Experimental  */ 
    public class GenericCurveTube : CurveTube
    {
        public GenericCurveTube(ICurve curve):base(curve)
        {
        }

        private float cuspThreshold = 0.005f;

        public override void UpdateTubeMeshWithAdaptiveSubdivision()
        {
            TubeStackCount = 0;
            cuspThreshold = 0.020f / (float)(curve.Count);
            adaptivePoints.Clear();

            adaptivePoints.Add(0.0f);
            GenerateTubeVertexRing(0.0f, true);
            GenerateTubeVertexRing(0.0f, false);
            float prevT = 0.0f;
            int subdiv = 1;
            for(int i = 1; i < curve.Count * subdiv; ++i)
            {
                float t = (float)(i) / (float)(curve.Count * subdiv - 1);

                GenerateTubeVerticesForCurveSegment(prevT, t);
                //adaptivePoints.Add(t);
                //GenerateTubeVertexRing(t, false);
                prevT = t;
            }
            adaptivePoints.Add(1.0f);
            GenerateTubeVertexRing(1.0f, false);
            GenerateTubeVertexRing(1.0f, true);
        }
        public void GenerateTubeVerticesForCurveSegment(float t0, float t1)
        {
            float tm = t0 + (t1 - t0) * 0.5f;               //  middle t

            float a = 1.0f / 3.0f;
            float b = 2.0f / 3.0f;
            Vector3 P0      = curve.PositionAt(t0);
            Vector3 P1      = curve.PositionAt(b * t0 + a * t1);
            Vector3 P2      = curve.PositionAt(a * t0 + b * t1);
            Vector3 P3      = curve.PositionAt(t1);
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
            if(d1 < AdaptiveEpsilon || d2 < AdaptiveEpsilon || d3 < AdaptiveEpsilon)
            {
                if(t1 - t0 < cuspThreshold)
                {
                    //  Cusp was detected. At cusp points, direction turns 
                    //  extremely quickly, tangent vanishes, and we can not 
                    //  get tangent interpolation by subdividing t. Cusps 
                    //  are detected by t subdivision threshold.
                    GenerateTubeVerticesForCusp(P1P0_, P3P2_, t0, t1);
                }
                else
                {
                    //  Normal subdivision. Split curve to halves and
                    //  recursively process both halves.
                    GenerateTubeVerticesForCurveSegment(t0, tm);
                    GenerateTubeVerticesForCurveSegment(tm, t1);
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

