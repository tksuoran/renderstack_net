// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

using System;
using System.Collections.Generic;

using RenderStack.Math;

namespace WildMagic
{
    public class TCBSpline : MultipleCurve
    {
        // Construction and destruction.  TCBSpline3 accepts responsibility for
        // deleting the input arrays.
        public TCBSpline(
            int         numSegments, 
            float[]     times, 
            Vector3[]   points,
            float[]     tension, 
            float[]     continuity, 
            float[]     bias
        )
            : base(numSegments,times)
        {
            // TO DO.  Add 'boundary type' just as in natural splines.
            //assertion(mNumSegments >= 3, "Not enough segments\n");

            // All four of these arrays have mNumSegments+1 elements.
            mPoints = points;
            mTension = tension;
            mContinuity = continuity;
            mBias = bias;

            mA = new Vector3[mNumSegments];
            mB = new Vector3[mNumSegments];
            mC = new Vector3[mNumSegments];
            mD = new Vector3[mNumSegments];

            // For now, treat the first point as if it occurred twice.
            ComputePoly(0, 0, 1, 2);

            for(int i = 1; i < mNumSegments - 1; ++i)
            {
                ComputePoly(i - 1, i, i + 1, i + 2);
            }

            // For now, treat the last point as if it occurred twice.
            ComputePoly(
                mNumSegments - 2, 
                mNumSegments - 1, 
                mNumSegments,
                mNumSegments
            );

        }

        public Vector3[] GetPoints()
        {
            return mPoints;
        }
        public float[] GetTensions()
        {
            return mTension;
        }
        public float[] GetContinuities()
        {
            return mContinuity;
        }
        public float[] GetBiases()
        {
            return mBias;
        }

        public override Vector3 GetPosition(float t)
        {
            int key;
            float dt;
            GetKeyInfo(t, out key, out dt);
            dt /= (mTimes[key + 1] - mTimes[key]);
            return mA[key] + dt * (mB[key] + dt * (mC[key] + dt * mD[key]));
        }
        public override Vector3 GetFirstDerivative(float t)
        {
            int key;
            float dt;
            GetKeyInfo(t, out key, out dt);
            dt /= (mTimes[key + 1] - mTimes[key]);
            return mB[key] + dt * (mC[key] * ((float)(2)) + mD[key] * (((float)(3)) * dt));
        }
        public override Vector3 GetSecondDerivative(float t)
        {
            int key;
            float dt;
            GetKeyInfo(t, out key, out dt);
            dt /= (mTimes[key + 1] - mTimes[key]);
            return mC[key] * ((float)(2)) + mD[key] * (((float)(6)) * dt);
        }
        public override Vector3 GetThirdDerivative(float t)
        {
            int key;
            float dt;
            GetKeyInfo(t, out key, out dt);
            dt /= (mTimes[key + 1] - mTimes[key]);
            return ((float)6) * mD[key];
        }

        protected void ComputePoly(int i0, int i1, int i2, int i3)
        {
            Vector3 diff = mPoints[i2] - mPoints[i1];
            float dt = mTimes[i2] - mTimes[i1];

            // Build multipliers at P1.
            float oneMinusT0 = (float)(1) - mTension[i1];
            float oneMinusC0 = (float)(1) - mContinuity[i1];
            float onePlusC0 = (float)(1) + mContinuity[i1];
            float oneMinusB0 = (float)(1) - mBias[i1];
            float onePlusB0 = (float)(1) + mBias[i1];
            float adj0 = ((float)(2)) * dt / (mTimes[i2] - mTimes[i0]);
            float out0 = ((float)(0.5)) * adj0 * oneMinusT0 * onePlusC0 * onePlusB0;
            float out1 = ((float)(0.5)) * adj0 * oneMinusT0 * oneMinusC0 * oneMinusB0;

            // Build outgoing tangent at P1.
            Vector3 TOut = out1 * diff + out0 * (mPoints[i1] - mPoints[i0]);

            // Build multipliers at point P2.
            float oneMinusT1 = (float)(1) - mTension[i2];
            float oneMinusC1 = (float)(1) - mContinuity[i2];
            float onePlusC1 = (float)(1) + mContinuity[i2];
            float oneMinusB1 = (float)(1) - mBias[i2];
            float onePlusB1 = (float)(1) + mBias[i2];
            float adj1 = ((float)(2)) * dt / (mTimes[i3] - mTimes[i1]);
            float in0 = ((float)(0.5)) * adj1 * oneMinusT1 * oneMinusC1 * onePlusB1;
            float in1 = ((float)(0.5)) * adj1 * oneMinusT1 * onePlusC1 * oneMinusB1;

            // Build incoming tangent at P2.
            Vector3 TIn = in1 * (mPoints[i3] - mPoints[i2]) + in0 * diff;

            mA[i1] = mPoints[i1];
            mB[i1] = TOut;
            mC[i1] = ((float)(3)) * diff - ((float)(2)) * TOut - TIn;
            mD[i1] = ((float)(-2)) * diff + TOut + TIn;
        }

        protected override float GetSpeedKey(int key, float t)
        {
            Vector3 velocity = mB[key] + t * (mC[key] * ((float)(2)) + mD[key] * (((float)(3)) * t));

            return velocity.Length;
        }
        protected override float GetLengthKey(int key, float t0, float t1)
        {
            KeyValuePair<MultipleCurve, int> data = new KeyValuePair<MultipleCurve,int>(this, key);
            return Integrate.RombergIntegral(
                8, 
                t0, 
                t1, 
                GetSpeedWithData,
                data
            );
        }

        protected Vector3[] mPoints;
        protected float[]   mTension;
        protected float[]   mContinuity;
        protected float[]   mBias;
        protected Vector3[] mA;
        protected Vector3[] mB;
        protected Vector3[] mC;
        protected Vector3[] mD;
    }
}
