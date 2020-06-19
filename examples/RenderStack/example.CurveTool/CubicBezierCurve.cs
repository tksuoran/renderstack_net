//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using RenderStack.Math;

namespace example.CurveTool
{
    /*  Comment: Experimental  */ 
    public class CubicBezier 
    {
        public Vector3[] P = new Vector3[4];

        public Vector3 this[int index]
        {
            get
            {
                return P[index];
            }
            set
            {
                P[index] = value;
            }
        }

        public int Count { get { return 4; } }

        public CubicBezier()
        {
        }
        public CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P[0] = p0;
            P[1] = p1;
            P[2] = p2;
            P[3] = p3;
        }

        public bool IsFlat(float epsilon)
        {
            float   d1 = Vector3.PointLineDistance(P[1], P[0], P[3]);
            float   d2 = Vector3.PointLineDistance(P[2], P[0], P[3]);
            return (d1 < epsilon) && (d2 < epsilon);
        }

        public Vector3 PositionAt(float t)
        {
            Vector3 P01   = Vector3.Mix(P[0],  P[1],  t);
            Vector3 P12   = Vector3.Mix(P[1],  P[2],  t);
            Vector3 P23   = Vector3.Mix(P[2],  P[3],  t);
            Vector3 P0112 = Vector3.Mix(P01, P12, t);
            Vector3 P1223 = Vector3.Mix(P12, P23, t);

            return Vector3.Mix(P0112, P1223, t);
        }
        public Vector3 FirstDerivativeAt(float t)
        {
            //  http://www.wolframalpha.com/input/?i=first+derivative+of+(1-t)^3*P_0+%2B+3*(1-t)^2*t*P_1+%2B+3(1-t)^2*P_2+%2B+t^3*P_3
            return 
                  3.0f * P[3] * t * t
                - 3.0f * P[0] * (1.0f - t) * (1.0f - t) 
                + 3.0f * P[1] * (1.0f - t) * (1.0f - t)
                - 6.0f * P[1] * t * (1.0f - t) 
                - 6.0f * P[2] * (1.0f - t);
        }
        public Vector3 SecondDerivativeAt(float t)
        {
            //  http://www.wolframalpha.com/input/?i=second+derivative+of+(1-t)^3*P_0+%2B+3*(1-t)^2*t*P_1+%2B+3(1-t)^2*P_2+%2B+t^3*P_3
            return 
                  6.0f * P[0] * (1.0f - t)
                -12.0f * P[1] * (1.0f - t)
                + 6.0f * P[1] * t
                + 6.0f * P[3] * t
                + 6.0f * P[2];
        }
        public void AdjustControlPointsToMakeCurveGoThrough(float t, Vector3 p)
        {
            float it  = (1.0f - t);
            float it2 = it * it;
            float it3 = it * it * it;
            float t2  = t * t;
            float t3  = t * t * t;
            if(t <= float.Epsilon)
            {
                P[0] = p;
            }
            else if(t > float.Epsilon && t <= 0.5f)
            {
                //          P(t) - (1-t)^3 P0 - t^3 P3 - 3(1-t)t^2 P2  =  3(1-t)^2t P1
                Vector3 a = p    - it3   * P[0] - t3 * P[3] - 3.0f * it * t2 * P[2];
                P[1] = a / (3.0f * it2 * t);
            }
            else if(t > 0.5f && t < 1.0f - float.Epsilon)
            {
                //          P(t) - (1-t)^3 P0 - t^3 P3 - 3(1-t)^2t P1   =  3(1-t)t^2 P2
                Vector3 a = p    - it3   * P[0] - t3 * P[3] - 3.0f * it2 * t * P[1];
                P[2] = a / (3.0f * it * t2);
            }
            else
            {
                P[3] = p;
            }
        }

        /// <summary>
        /// Compute control points q1 and q2 for cubic bezier curve
        /// which goes through give points p0, p1, p2 and p3.
        /// </summary>
        /// <param name="p0">Curve start point</param>
        /// <param name="p1">Point on curve</param>
        /// <param name="p2">Point on curve</param>
        /// <param name="p3">Curve end point</param>
        /// <param name="q1">Computed control point</param>
        /// <param name="q2">Computed control point</param>
        /// <param name="u">t for first control point</param>
        /// <param name="v">t for second control point</param>
        public static void ApportionedChords(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            out Vector3 q1,
            out Vector3 q2,
            out float u,
            out float v
        )
        {
            Vector3 D1 = p1 - p0;
            Vector3 D2 = p2 - p1;
            Vector3 D3 = p3 - p2;
            float d1 = D1.Length;
            float d2 = D2.Length;
            float d3 = D3.Length;

            u = d1 / (d1 + d2 + d3);
            v = (d1 + d2) / (d1 + d2 + d3);

            float a   = 3.0f * (1.0f - u) * (1.0f - u) * u; 
            float b   = 3.0f * (1.0f - u) * u * u;
            float c   = 3.0f * (1.0f - v) * (1.0f - v) * v; 
            float d   = 3.0f * (1.0f - v) * v * v;
            float det = a * d - b * c;

            if(det == 0.0f)
            {
                throw new InvalidOperationException();
            }

            Vector3 t1;
            Vector3 t2;

            t1.X = p1.X - ((1.0f - u) * (1.0f - u) * (1.0f - u) * p0.X + u * u * u * p3.X);
            t1.Y = p1.Y - ((1.0f - u) * (1.0f - u) * (1.0f - u) * p0.Y + u * u * u * p3.Y);
            t1.Z = p1.Z - ((1.0f - u) * (1.0f - u) * (1.0f - u) * p0.Z + u * u * u * p3.Z);

            t2.X = p2.X - ((1.0f - v) * (1.0f - v) * (1.0f - v) * p0.X + v * v * v * p3.X);
            t2.Y = p2.Y - ((1.0f - v) * (1.0f - v) * (1.0f - v) * p0.Y + v * v * v * p3.Y);
            t2.Z = p2.Z - ((1.0f - v) * (1.0f - v) * (1.0f - v) * p0.Z + v * v * v * p3.Z);

            q1.X = d * t1.X - b * t2.X;
            q1.Y = d * t1.Y - b * t2.Y;
            q1.Z = d * t1.Z - b * t2.Z;
            q1.X /= det;
            q1.Y /= det;
            q1.Z /= det;

            q2.X = (-c) * t1.X + a * t2.X;
            q2.Y = (-c) * t1.Y + a * t2.Y;
            q2.Z = (-c) * t1.Z + a * t2.Z;
            q2.X /= det;
            q2.Y /= det;
            q2.Z /= det;
        }
    }
}
