// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

using System;

using RenderStack.Math;

namespace WildMagic
{
    public class NURBSCurve : SingleCurve
    {
        // Construction and destruction.  The caller is responsible for deleting
        // the input arrays if they were dynamically allocated.  Internal copies
        // of the arrays are made, so to dynamically change control points,
        // control weights, or knots, you must use the 'SetControlPoint',
        // 'GetControlPoint', 'SetControlWeight', 'GetControlWeight', and 'Knot'
        // member functions.

        // The homogeneous input points are (x,y,z,w) where the (x,y,z) values are
        // stored in the ctrlPoint array and the w values are stored in the
        // ctrlWeight array.  The output points from curve evaluations are of
        // the form (x',y',z') = (x/w,y/w,z/w).

        // Uniform spline.  The number of control points is n+1 >= 2.  The degree
        // of the spline is d and must satisfy 1 <= d <= n.  The knots are
        // implicitly calculated in [0,1].  If open is 'true', the spline is
        // open and the knots are
        //   t[i] = 0,               0 <= i <= d
        //          (i-d)/(n+1-d),   d+1 <= i <= n
        //          1,               n+1 <= i <= n+d+1
        //
        // If open is 'false', the spline is periodic and the knots are
        //   t[i] = (i-d)/(n+1-d),   0 <= i <= n+d+1
        //
        // If loop is 'true', extra control points are added to generate a closed
        // curve.  For an open spline, the control point array is reallocated and
        // one extra control point is added, set to the first control point
        // C[n+1] = C[0].  For a periodic spline, the control point array is
        // reallocated and the first d points are replicated.  In either case the
        // knot array is calculated accordingly.

        public NURBSCurve(
            int         numCtrlPoints, 
            Vector3[]   ctrlPoint,
            float[]     ctrlWeight, 
            int         degree, 
            bool        loop, 
            bool        open
        )
        : base((float)0, (float)1)
        {
            mLoop = loop;

            if(numCtrlPoints < 2)
            {
                throw new ArgumentOutOfRangeException("number of control points too low");
            }
            if(degree < 1)
            {
                throw new ArgumentOutOfRangeException("degree too low");
            }
            if(degree > numCtrlPoints - 1)
            {
                throw new ArgumentOutOfRangeException("degree too high / too few control points");
            }

            mNumCtrlPoints = numCtrlPoints;
            mReplicate = (loop ? (open ? 1 : degree) : 0);
            CreateControl(ctrlPoint, ctrlWeight);
            mBasis.Create(mNumCtrlPoints + mReplicate, degree, open);
        }

        // Open, nonuniform spline.  The knot array must have n-d elements.  The
        // elements must be nondecreasing.  Each element must be in [0,1].
        public NURBSCurve(
            int         numCtrlPoints,
            Vector3[]   ctrlPoint,
            float[]     ctrlWeight, 
            int         degree, 
            bool        loop, 
            float[]     knot
        )
        : base((float)0, (float)1)
        {
            mLoop = loop;

            if(numCtrlPoints < 2)
            {
                throw new ArgumentOutOfRangeException("number of control points too low");
            }
            if(degree < 1)
            {
                throw new ArgumentOutOfRangeException("degree too low");
            }
            if(degree > numCtrlPoints - 1)
            {
                throw new ArgumentOutOfRangeException("degree too high / too few control points");
            }

            mNumCtrlPoints = numCtrlPoints;
            mReplicate = (loop ? 1 : 0);
            CreateControl(ctrlPoint, ctrlWeight);
            mBasis.Create(mNumCtrlPoints + mReplicate, degree, knot);
        }

        public int GetNumCtrlPoints()
        {
            return mNumCtrlPoints;
        }
        public int GetDegree()
        {
            return mBasis.GetDegree();
        }
        public bool IsOpen()
        {
            return mBasis.IsOpen();
        }
        public bool IsUniform()
        {
            return mBasis.IsUniform();
        }
        public bool IsLoop()
        {
            return mLoop;
        }

        // Control points and weights may be changed at any time.  The input index
        // should be valid (0 <= i <= n).  If it is invalid, the return value of
        // GetControlPoint is a vector whose components are all MAX_float, and the
        // return value of GetControlWeight is MAX_float.
        // undefined.
        public void SetControlPoint(int i, Vector3 ctrl)
        {
            if(0 <= i && i < mNumCtrlPoints)
            {
                // Set the control point.
                mCtrlPoint[i] = ctrl;

                // Set the replicated control point.
                if(i < mReplicate)
                {
                    mCtrlPoint[mNumCtrlPoints+i] = ctrl;
                }
            }
        }
        public Vector3 GetControlPoint(int i)
        {
            if(0 <= i && i < mNumCtrlPoints)
            {
                return mCtrlPoint[i];
            }

            return Vector3.MaxValue;
        }
        public void SetControlWeight(int i, float weight)
        {
            if(0 <= i && i < mNumCtrlPoints)
            {
                // Set the control weight.
                mCtrlWeight[i] = weight;

                // Set the replicated control weight.
                if(i < mReplicate)
                {
                    mCtrlWeight[mNumCtrlPoints + i] = weight;
                }
            }
        }
        public float GetControlWeight(int i)
        {
            if (0 <= i && i < mNumCtrlPoints)
            {
                return mCtrlWeight[i];
            }

            return Single.MaxValue;
        }

        // The knot values can be changed only if the basis function is nonuniform
        // and the input index is valid (0 <= i <= n-d-1).  If these conditions
        // are not satisfied, GetKnot returns MAX_float.
        public void SetKnot(int i, float knot)
        {
            mBasis.SetKnot(i, knot);
        }
        public float GetKnot(int i)
        {
            return mBasis.GetKnot(i);
        }

        // The spline is defined for 0 <= t <= 1.  If a t-value is outside [0,1],
        // an open spline clamps t to [0,1].  That is, if t > 1, t is set to 1;
        // if t < 0, t is set to 0.  A periodic spline wraps to to [0,1].  That
        // is, if t is outside [0,1], then t is set to t-floor(t).
        public override Vector3 GetPosition(float t)
        {
            Vector3 pos;
            Vector3 dummy;
            Get(t, out pos, out dummy, out dummy, out dummy);
            return pos;
        }
        public override Vector3 GetFirstDerivative(float t)
        {
            Vector3 der1;
            Vector3 dummy;
            Get(t, out dummy, out der1, out dummy, out dummy);
            return der1;
        }
        public override Vector3 GetSecondDerivative(float t)
        {
            Vector3 der2;
            Vector3 dummy;
            Get(t, out dummy, out dummy, out der2, out dummy);
            return der2;
        }
        public override Vector3 GetThirdDerivative(float t)
        {
            Vector3 der3;
            Vector3 dummy;
            Get(t, out dummy, out dummy, out dummy, out der3);
            return der3;
        }

        // If you need position and derivatives at the same time, it is more
        // efficient to call these functions.  Pass the addresses of those
        // quantities whose values you want.  You may pass 0 in any argument
        // whose value you do not want.
        public void Get(
            float t, 
            out Vector3 pos,
            out Vector3 der1,
            out Vector3 der2,
            out Vector3 der3
        )
        {
            int i, imin, imax;
            //if(der3 != null)
            {
                mBasis.Compute(t, 0, out imin, out imax);
                mBasis.Compute(t, 1, out imin, out imax);
                mBasis.Compute(t, 2, out imin, out imax);
                mBasis.Compute(t, 3, out imin, out imax);
            }
#if false
            //else if(der2 != null)
            {
                mBasis.Compute(t, 0, imin, imax);
                mBasis.Compute(t, 1, imin, imax);
                mBasis.Compute(t, 2, imin, imax);
            }
            //else if(der1 != null)
            {
                mBasis.Compute(t, 0, imin, imax);
                mBasis.Compute(t, 1, imin, imax);
            }
            //else  // pos
            {
                mBasis.Compute(t, 0, imin, imax);
            }
#endif

            float tmp;

            // Compute position.
            Vector3 X = Vector3.Zero;
            float w = (float)0;
            for (i = imin; i <= imax; ++i)
            {
                tmp = mBasis.GetD0(i) * mCtrlWeight[i];
                X += tmp * mCtrlPoint[i];
                w += tmp;
            }
            float invW = ((float)1) / w;
            Vector3 P = invW * X;
            //if(pos != null)
            {
                pos = P;
            }

#if false
            if (!der1 && !der2 && !der3)
            {
                return;
            }
#endif

            // Compute first derivative.
            Vector3 XDer1 = Vector3.Zero;
            float wDer1 = (float)0;
            for (i = imin; i <= imax; ++i)
            {
                tmp = mBasis.GetD1(i) * mCtrlWeight[i];
                XDer1 += tmp * mCtrlPoint[i];
                wDer1 += tmp;
            }
            Vector3 PDer1 = invW*(XDer1 - wDer1*P);
            //if (der1)
            {
                der1 = PDer1;
            }

#if false
            if (!der2 && !der3)
            {
                return;
            }
#endif

            // Compute second derivative.
            Vector3 XDer2 = Vector3.Zero;
            float wDer2 = (float)0;
            for (i = imin; i <= imax; ++i)
            {
                tmp = mBasis.GetD2(i) * mCtrlWeight[i];
                XDer2 += tmp * mCtrlPoint[i];
                wDer2 += tmp;
            }
            Vector3 PDer2 = invW * (XDer2 - ((float)2) * wDer1 * PDer1 - wDer2 * P);
            //if (der2)
            {
                der2 = PDer2;
            }

#if false
            if (!der3)
            {
                return;
            }
#endif

            // Compute third derivative.
            Vector3 XDer3 = Vector3.Zero;
            float wDer3 = (float)0;
            for (i = imin; i <= imax; ++i)
            {
                tmp = mBasis.GetD3(i) * mCtrlWeight[i];
                XDer3 += tmp * mCtrlPoint[i];
                wDer3 += tmp;
            }
            //if (der3)
            {
                der3 = invW * (XDer3 - ((float)3) * wDer1 * PDer2 - ((float)3) * wDer2 * PDer1 - wDer3 * P);
            }
        }

        // Access the basis function to compute it without control points.  This
        // is useful for least squares fitting of curves.
        public BSplineBasis GetBasis()
        {
            return mBasis;
        }

        // Replicate the necessary number of control points when the Create
        // function has loop equal to true, in which case the spline curve must
        // be a closed curve.
        protected void CreateControl(Vector3[] ctrlPoint, float[] ctrlWeight)
        {
            int newNumCtrlPoints = mNumCtrlPoints + mReplicate;

            mCtrlPoint = new Vector3[newNumCtrlPoints];
            //memcpy(mCtrlPoint, ctrlPoint, mNumCtrlPoints*sizeof(Vector3<float>));

            mCtrlWeight = new float[newNumCtrlPoints];
            //memcpy(mCtrlWeight, ctrlWeight, mNumCtrlPoints*sizeof(float));

            int i;
            for(i = 0; i < mNumCtrlPoints; ++i)
            {
                mCtrlPoint[i] = ctrlPoint[i];
                mCtrlWeight[i] = ctrlWeight[i];
            }
            for(i = 0; i < mReplicate; ++i)
            {
                mCtrlPoint[mNumCtrlPoints + i] = ctrlPoint[i];
                mCtrlWeight[mNumCtrlPoints + i] = ctrlWeight[i];
            }
        }

        protected int           mNumCtrlPoints;
        protected Vector3[]     mCtrlPoint;     // ctrl[n+1]
        protected float[]       mCtrlWeight;    // weight[n+1]
        protected bool          mLoop;
        protected BSplineBasis  mBasis = new BSplineBasis();
        protected int           mReplicate;     // the number of replicated control points
    }

}
