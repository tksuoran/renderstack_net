// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

using System;

using RenderStack.Math;

namespace WildMagic
{
    public class BezierCurve : SingleCurve
    {
        protected int       mDegree;
        protected int       mNumCtrlPoints;
        protected Vector3[] mCtrlPoint;
        protected Vector3[] mDer1CtrlPoint;
        protected Vector3[] mDer2CtrlPoint;
        protected Vector3[] mDer3CtrlPoint;
        protected float[,]  mChoose;

        // Construction and destruction.  BezierCurve3 accepts responsibility for
        // deleting the input arrays.
        public BezierCurve(int degree, Vector3[] ctrlPoint)
            : base((float)(0), (float)(1))
        {
            //assertion(degree >= 2, "The degree must be three or larger\n");

            int i, j;

            mDegree = degree;
            mNumCtrlPoints = mDegree + 1;
            mCtrlPoint = ctrlPoint;

            // Compute first-order differences.
            mDer1CtrlPoint = new Vector3[mNumCtrlPoints - 1];
            for(i = 0; i < mNumCtrlPoints - 1; ++i)
            {
                mDer1CtrlPoint[i] = mCtrlPoint[i + 1] - mCtrlPoint[i];
            }

            // Compute second-order differences.
            mDer2CtrlPoint = new Vector3[mNumCtrlPoints - 2];
            for(i = 0; i < mNumCtrlPoints - 2; ++i)
            {
                mDer2CtrlPoint[i] = mDer1CtrlPoint[i + 1] - mDer1CtrlPoint[i];
            }

            // Compute third-order differences.
            if(degree >= 3)
            {
                mDer3CtrlPoint = new Vector3[mNumCtrlPoints - 3];
                for(i = 0; i < mNumCtrlPoints - 3; ++i)
                {
                    mDer3CtrlPoint[i] = mDer2CtrlPoint[i + 1] - mDer2CtrlPoint[i];
                }
            }
            else
            {
                mDer3CtrlPoint = null;
            }

            // Compute combinatorial values Choose(N,K), store in mChoose[N][K].
            // The values mChoose[r][c] are invalid for r < c (use only the
            // entries for r >= c).
            mChoose = new float[mNumCtrlPoints, mNumCtrlPoints];

            mChoose[0, 0] = (float)1;
            mChoose[1, 0] = (float)1;
            mChoose[1, 1] = (float)1;
            for(i = 2; i <= mDegree; ++i)
            {
                mChoose[i, 0] = (float)1;
                mChoose[i, i] = (float)1;
                for(j = 1; j < i; ++j)
                {
                    mChoose[i, j] = mChoose[i - 1, j - 1] + mChoose[i - 1, j];
                }
            }
        }

        public int GetDegree()
        {
            return mDegree;
        }

        public Vector3[] GetControlPoints ()
        {
            return mCtrlPoint;
        }

        public override Vector3 GetPosition(float t)
        {
            float oneMinusT = (float)(1) - t;
            float powT = t;
            Vector3 result = oneMinusT * mCtrlPoint[0];

            for(int i = 1; i < mDegree; ++i)
            {
                float coeff = mChoose[mDegree, i] * powT;
                result = (result + coeff * mCtrlPoint[i]) * oneMinusT;
                powT *= t;
            }

            result += powT * mCtrlPoint[mDegree];

            return result;
        }
        public override Vector3 GetFirstDerivative(float t)
        {
            float oneMinusT = (float)(1) - t;
            float powT = t;
            Vector3 result = oneMinusT * mDer1CtrlPoint[0];

            int degreeM1 = mDegree - 1;
            for(int i = 1; i < degreeM1; ++i)
            {
                float coeff = mChoose[degreeM1, i] * powT;
                result = (result + coeff * mDer1CtrlPoint[i]) * oneMinusT;
                powT *= t;
            }

            result += powT * mDer1CtrlPoint[degreeM1];
            result *= (float)(mDegree);

            return result;
        }
        public override Vector3 GetSecondDerivative(float t)
        {
            float oneMinusT = (float)(1) - t;
            float powT = t;
            Vector3 result = oneMinusT * mDer2CtrlPoint[0];

            int degreeM2 = mDegree - 2;
            for(int i = 1; i < degreeM2; ++i)
            {
                float coeff = mChoose[degreeM2, i] * powT;
                result = (result + coeff * mDer2CtrlPoint[i]) * oneMinusT;
                powT *= t;
            }

            result += powT * mDer2CtrlPoint[degreeM2];
            result *= (float)(mDegree * (mDegree - 1));

            return result;
        }
        public override Vector3 GetThirdDerivative(float t)
        {
            if(mDegree < 3)
            {
                return Vector3.Zero;
            }

            float oneMinusT = (float)(1) - t;
            float powT = t;
            Vector3 result = oneMinusT * mDer3CtrlPoint[0];

            int degreeM3 = mDegree - 3;
            for(int i = 1; i < degreeM3; ++i)
            {
                float coeff = mChoose[degreeM3, i] * powT;
                result = (result + coeff * mDer3CtrlPoint[i]) * oneMinusT;
                powT *= t;
            }

            result += powT * mDer3CtrlPoint[degreeM3];
            result *= (float)(mDegree * (mDegree - 1) * (mDegree - 2));

            return result;
        }
    }
}
