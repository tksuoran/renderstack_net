// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

using System;

using RenderStack.Math;

namespace WildMagic
{

    public abstract class Curve
    {
        // Curve parameter is t where tmin <= t <= tmax.
        protected float mTMin;
        protected float mTMax;

        public Curve(float tmin, float tmax)
        {
            mTMin = tmin;
            mTMax = tmax;
        }

        // Interval on which curve parameter is defined.  If you are interested
        // in only a subinterval of the actual domain of the curve, you may set
        // that subinterval with SetTimeInterval.  This function requires that
        // tmin < tmax.
        public float GetMinTime()
        {
            return mTMin;
        }
        public float GetMaxTime()
        {
            return mTMax;
        }
        public void SetTimeInterval(float tmin, float tmax)
        {
            mTMin = tmin;
            mTMax = tmax;
        }

        // Position and derivatives.
        public abstract Vector3 GetPosition(float t);
        public abstract Vector3 GetFirstDerivative(float t);
        public abstract Vector3 GetSecondDerivative(float t);
        public abstract Vector3 GetThirdDerivative(float t);

        // Differential geometric quantities.
        public float GetSpeed(float t)
        {
            Vector3 velocity = GetFirstDerivative(t);
            float speed = velocity.Length;
            return speed;
        }

        public abstract float GetLength(float t0, float t1);
        public float GetTotalLength()
        {
            return GetLength(mTMin, mTMax);
        }
        public Vector3 GetTangent(float t)
        {
            Vector3 velocity = GetFirstDerivative(t);
            return Vector3.Normalize(velocity);
        }
        public Vector3 GetNormal(float t)
        {
            Vector3 velocity = GetFirstDerivative(t);
            Vector3 acceleration = GetSecondDerivative(t);
            float VDotV = Vector3.Dot(velocity, velocity);
            float VDotA = Vector3.Dot(velocity, acceleration);
            Vector3 normal = VDotV * acceleration - VDotA * velocity;
            return Vector3.Normalize(normal);
        }
        public Vector3 GetBinormal(float t)
        {
            Vector3 velocity = GetFirstDerivative(t);
            Vector3 acceleration = GetSecondDerivative(t);
            float VDotV = Vector3.Dot(velocity, velocity);
            float VDotA = Vector3.Dot(velocity, acceleration);
            Vector3 normal = VDotV * acceleration - VDotA * velocity;
            normal = Vector3.Normalize(normal);
            velocity = Vector3.Normalize(velocity);
            Vector3 binormal = Vector3.Cross(velocity, normal);
            return binormal;
        }
        public void GetFrame(
            float       t, 
            out Vector3 position, 
            out Vector3 tangent,
            out Vector3 normal, 
            out Vector3 binormal
        )
        {
            position = GetPosition(t);
            Vector3 velocity = GetFirstDerivative(t);
            Vector3 acceleration = GetSecondDerivative(t);
            float VDotV = Vector3.Dot(velocity, velocity);
            float VDotA = Vector3.Dot(velocity, acceleration);
            normal = VDotV * acceleration - VDotA * velocity;
            normal = Vector3.Normalize(normal);
            tangent = velocity;
            tangent = Vector3.Normalize(tangent);
            binormal = Vector3.Cross(tangent, normal);
        }
        public float GetCurvature (float t)
        {
            Vector3 velocity = GetFirstDerivative(t);
            float   speedSqr = velocity.LengthSquared;

            if(speedSqr >= Single.Epsilon)
            {
                Vector3 acceleration = GetSecondDerivative(t);
                Vector3 cross = Vector3.Cross(velocity, acceleration);
                float   numer = cross.Length;
                float   denom = (float)(Math.Pow(speedSqr, 1.5));
                return numer / denom;
            }
            else
            {
                // Curvature is indeterminate, just return 0.
                return 0.0f;
            }
        }
        public float GetTorsion(float t)
        {
            Vector3 velocity = GetFirstDerivative(t);
            Vector3 acceleration = GetSecondDerivative(t);
            Vector3 cross = Vector3.Cross(velocity, acceleration);
            float denom = cross.LengthSquared;

            if(denom >= Single.Epsilon)
            {
                Vector3 jerk = GetThirdDerivative(t);
                float numer = Vector3.Dot(cross, jerk);

                return numer / denom;
            }
            else
            {
                // Torsion is indeterminate, just return 0.
                return (float)0;
            }
        }

        // Inverse mapping of s = Length(t) given by t = Length^{-1}(s).
        public float GetTime(float length)
        {
            return GetTime(length, 32, 1e-06f);
        }
        public abstract float GetTime(
            float   length,
            int     iterations,
            float   tolerance
        );

        // Subdivision.
        public void SubdivideByTime(int numPoints, out Vector3[] points)
        {
            //assertion(numPoints >= 2, "Subdivision requires at least two points\n");
            points = new Vector3[numPoints];

            float delta = (mTMax - mTMin) / (numPoints - 1);

            for(int i = 0; i < numPoints; ++i)
            {
                float t = mTMin + delta * i;
                points[i] = GetPosition(t);
            }
        }
        public void SubdivideByLength(int numPoints, out Vector3[] points)
        {
            //assertion(numPoints >= 2, "Subdivision requires at least two points\n");
            points = new Vector3[numPoints];

            float delta = GetTotalLength() / (numPoints - 1);

            for(int i = 0; i < numPoints; ++i)
            {
                float length = delta * i;
                float t = GetTime(length);
                points[i] = GetPosition(t);
            }
        }
    }

}
