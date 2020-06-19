//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RenderStack.Math;

namespace example.CurveTool
{
    public class GenericCurve : ICurve
    {
        private WildMagic.NURBSCurve    nurbs;
        private WildMagic.TCBSpline     spline;
        private List<ControlPoint>      P = new List<ControlPoint>();
        private bool                    dirty0 = true;
        private bool                    dirty { get { return dirty0; } set { dirty0 = value; } }

        public int Count { get { return P.Count; } }
        public bool Dirty { get { return dirty; } set { if(value == true){ dirty = true; } } }
        public ControlPoint this[int index]
        {
            get
            {
                return P[index];
            }
            set
            {
                if(P[index] != value)
                {
                    P[index] = value;
                    dirty = true;
                }
            }
        }

        public void Add(ControlPoint point)
        {
            P.Add(point);
            dirty = true;
        }

        public void UpdateNURBS()
        {
            Vector3[]   positions   = new Vector3[P.Count];
            float[]     weights     = new float[P.Count];
            float[]     knots       = new float[P.Count];
            for(int i = 0; i < P.Count; ++i)
            {
                P[i].Parameters[0] = 1.0f;
                positions[i] = P[i].Position;
                weights[i] = P[i].Parameters[0];
                knots[i] = (float)(i) / (float)(P.Count - 1);
            }
            nurbs = new WildMagic.NURBSCurve(
                P.Count,
                positions,
                weights,
                2,
#if true
                false, 
                true
#else
                true,   // loop
                false   // open  (knots)
#endif
            );
        }
        public void UpdateTCBSpline()
        {
            Vector3[]   positions   = new Vector3[P.Count];
            float[]     times       = new float[P.Count];
            float[]     tension     = new float[P.Count];
            float[]     continuity  = new float[P.Count];
            float[]     bias        = new float[P.Count];
            for(int i = 0; i < P.Count; ++i)
            {
                positions[i]    = P[i].Position;
                times[i]        = (float)(i) / (float)(P.Count - 1);
                tension[i]      = P[i].Parameters[0];
                continuity[i]   = P[i].Parameters[1];
                bias[i]         = P[i].Parameters[2];
            }
            spline = new WildMagic.TCBSpline(P.Count - 1, times, positions, tension, continuity, bias);
        }

        public Vector3 PositionAt(float t)
        {
            if(dirty)
            {
                UpdateNURBS();
                //UpdateTCBSpline();
                dirty = false;
            }
            return nurbs.GetPosition(t);
            //return spline.GetPosition(globalT);
        }
        public Vector3 TangentAt(float t)
        {
            if(dirty)
            {
                UpdateNURBS();
                //UpdateTCBSpline();
                dirty = false;
            }
            return nurbs.GetTangent(t);
            //return spline.GetPosition(globalT);
        }
    }
#if true
#else
    public class CatmullRomCurve : ICurve
    {
        public bool Closed { get { return this.closed; } set { this.closed = value; } }

        public List<Vector3> P = new List<Vector3>();

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

        public int Count { get { return P.Count; } }

        private bool closed;
        private float deltaT;

        public CatmullRomCurve()
        {
        }
        public CatmullRomCurve(IList<Vector3> vertices)
        {
            for(int i = 0; i < vertices.Count; i++)
            {
                Add(vertices[i]);
            }
        }

        public static Vector3 CatmullRom(
            Vector3 P1, 
            Vector3 P2, 
            Vector3 P3, 
            Vector3 P4, 
            float   t
        )
        {
            return new Vector3(
                CatmullRom(P1.X, P2.X, P3.X, P4.X, t ),
                CatmullRom(P1.Y, P2.Y, P3.Y, P4.Y, t),
                CatmullRom(P1.Z, P2.Z, P3.Z, P4.Z, t)
            );
        }

        public static float CatmullRom(
            float value1, 
            float value2, 
            float value3, 
            float value4, 
            float t
        )
        {
            double tSquared = t * t;
            double tCubed = tSquared * t;
            return (float)(
                0.5 * 
                (
                    2.0 * value2 +
                    (value3 - value1) * t +
                    (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * tSquared +
                    (3.0 * value2 - value1 - 3.0 * value3 + value4) * tCubed)
                );
        }

        public int NextIndex(int index)
        {
            if(index == P.Count - 1)
            {
                return 0;
            }
            return index + 1;
        }

        public int PreviousIndex(int index)
        {
            if(index == 0)
            {
                return P.Count - 1;
            }
            return index - 1;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < P.Count; i++)
            {
                builder.Append(P[i].ToString());
                if(i < P.Count - 1)
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        public List<Vector3> GetVertices(int divisions)
        {
            List<Vector3> verts = new List<Vector3>();

            float timeStep = 1f / (float)divisions;

            for(float i = 0; i < 1f; i += timeStep)
            {
                verts.Add(PositionAt(i));
            }

            return verts;
        }

        public Vector3 PositionAt(float globalT)
        {
            Vector3 temp;

            if(P.Count < 2)
            {
                throw new Exception("You need at least 2 control points to calculate a position.");
            }

            if(Closed)
            {
                this.Add(P[0]);

                deltaT = 1f / (float)(P.Count - 1);

                int p = (int)(globalT / deltaT);

                int p0 = p - 1;
                if (p0 < 0) p0 = p0 + (P.Count - 1); else if (p0 >= (int)P.Count - 1) p0 = p0 - (P.Count - 1);
                int p1 = p;
                if (p1 < 0) p1 = p1 + (P.Count - 1); else if (p1 >= (int)P.Count - 1) p1 = p1 - (P.Count - 1);
                int p2 = p + 1;
                if (p2 < 0) p2 = p2 + (P.Count - 1); else if (p2 >= (int)P.Count - 1) p2 = p2 - (P.Count - 1);
                int p3 = p + 2;
                if (p3 < 0) p3 = p3 + (P.Count - 1); else if (p3 >= (int)P.Count - 1) p3 = p3 - (P.Count - 1);

                float localT = (globalT - deltaT * (float)p) / deltaT;

                temp = CatmullRom(P[p0], P[p1], P[p2], P[p3], localT);

                this.RemoveAt(P.Count - 1);
            }
            else
            {
                int p = (int)(globalT / deltaT);

                int p0 = p - 1;
                if (p0 < 0) p0 = 0; else if (p0 >= (int)P.Count - 1) p0 = P.Count - 1;
                int p1 = p;
                if (p1 < 0) p1 = 0; else if (p1 >= (int)P.Count - 1) p1 = P.Count - 1;
                int p2 = p + 1;
                if (p2 < 0) p2 = 0; else if (p2 >= (int)P.Count - 1) p2 = P.Count - 1;
                int p3 = p + 2;
                if (p3 < 0) p3 = 0; else if (p3 >= (int)P.Count - 1) p3 = P.Count - 1;

                float localT = (globalT - deltaT * (float)p) / deltaT;

                temp = CatmullRom(
                    P[p0], 
                    P[p1], 
                    P[p2], 
                    P[p3], 
                    localT
                );
            }

            return temp;
        }

        public void Add(Vector3 point)
        {
            P.Add(point);
            deltaT = 1f / (float)(P.Count - 1);
        }

        public void Remove(Vector3 point)
        {
            P.Remove(point);
            deltaT = 1f / (float)(P.Count - 1);
        }

        public void RemoveAt(int index)
        {
            P.RemoveAt(index);
            deltaT = 1f / (float)(P.Count - 1);
        }

        public float GetLength()
        {
            List<Vector3> verts = this.GetVertices(P.Count * 25);
            float length = 0;

            for(int i = 1; i < verts.Count; i++)
            {
                length += verts[i - 1].Distance(verts[i]);
            }

            if(Closed)
            {
                length += verts[P.Count - 1].Distance(verts[0]);
            }

            return length;
        }

    }
#endif
}
