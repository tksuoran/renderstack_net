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
    public abstract class MultipleCurve : Curve
    {
        protected int       mNumSegments;
        protected float[]   mTimes;
        protected float[]   mLengths;
        protected float[]   mAccumLengths;

        // Construction and destruction for abstract base class.  MultipleCurve3
        // accepts responsibility for deleting the input array.
        public MultipleCurve(int numSegments, float[] times)
        :   base(times[0], times[numSegments])
        {
            mNumSegments = numSegments;
            mTimes = times;
            mLengths = null;
            mAccumLengths = null;
        }

        // Member access.
        public int GetSegments()
        {
            return mNumSegments;
        }

        public float[] GetTimes ()
        {
            return mTimes;
        }

        // Length-from-time and time-from-length.
        public override float GetLength(float t0, float t1)
        {
            //assertion(mTMin <= t0 && t0 <= mTMax, "Invalid input\n");
            //assertion(mTMin <= t1 && t1 <= mTMax, "Invalid input\n");
            //assertion(t0 <= t1, "Invalid input\n");

            if(mLengths == null)
            {
                InitializeLength();
            }

            int key0, key1;
            float dt0, dt1;
            GetKeyInfo(t0, out key0, out dt0);
            GetKeyInfo(t1, out key1, out dt1);

            float length;
            if(key0 < key1)
            {
                // Accumulate full-segment lengths.
                length = 0f;
                for(int i = key0 + 1; i < key1; ++i)
                {
                    length += mLengths[i];
                }
                
                // Add on partial first segment.
                length += GetLengthKey(key0, dt0, mTimes[key0 + 1] - mTimes[key0]);
                
                // Add on partial last segment.
                length += GetLengthKey(key1, 0f, dt1);
            }
            else
            {
                length = GetLengthKey(key0, dt0, dt1);
            }

            return length;

        }
        public override float GetTime(
            float   length, 
            int     iterations,
            float   tolerance
        )
        {
            if(mLengths == null)
            {
                InitializeLength();
            }

            if(length <= 0f)
            {
                return mTMin;
            }

            if(length >= mAccumLengths[mNumSegments - 1])
            {
                return mTMax;
            }

            int key;
            for(key = 0; key < mNumSegments; ++key)
            {
                if (length < mAccumLengths[key])
                {
                    break;
                }
            }
            if(key >= mNumSegments)
            {
                return mTimes[mNumSegments];
            }

            float len0, len1;
            if(key == 0)
            {
                len0 = length;
                len1 = mAccumLengths[0];
            }
            else
            {
                len0 = length - mAccumLengths[key - 1];
                len1 = mAccumLengths[key] - mAccumLengths[key - 1];
            }

            // If L(t) is the length function for t in [tmin,tmax], the derivative is
            // L'(t) = |x'(t)| >= 0 (the magnitude of speed).  Therefore, L(t) is a
            // nondecreasing function (and it is assumed that x'(t) is zero only at
            // isolated points; that is, no degenerate curves allowed).  The second
            // derivative is L"(t).  If L"(t) >= 0 for all t, L(t) is a convex
            // function and Newton's method for root finding is guaranteed to
            // converge.  However, L"(t) can be negative, which can lead to Newton
            // iterates outside the domain [tmin,tmax].  The algorithm here avoids
            // this problem by using a hybrid of Newton's method and bisection.

            // Initial guess for Newton's method is dt0.
            float dt1 = mTimes[key + 1] - mTimes[key];
            float dt0 = dt1 * len0 / len1;

            // Initial root-bounding interval for bisection.
            float lower = 0f, upper = dt1;

            for (int i = 0; i < iterations; ++i)
            {
                float difference = GetLengthKey(key, 0f, dt0) - len0;
                if(Math.Abs(difference) <= tolerance)
                {
                    // |L(mTimes[key]+dt0)-length| is close enough to zero, report
                    // mTimes[key]+dt0 as the time at which 'length' is attained.
                    return mTimes[key] + dt0;
                }

                // Generate a candidate for Newton's method.
                float dt0Candidate = dt0 - difference / GetSpeedKey(key, dt0);

                // Update the root-bounding interval and test for containment of the
                // candidate.
                if(difference > 0f)
                {
                    upper = dt0;
                    if(dt0Candidate <= lower)
                    {
                        // Candidate is outside the root-bounding interval.  Use
                        // bisection instead.
                        dt0 = (0.5f) * (upper + lower);
                    }
                    else
                    {
                        // There is no need to compare to 'upper' because the tangent
                        // line has positive slope, guaranteeing that the t-axis
                        // intercept is smaller than 'upper'.
                        dt0 = dt0Candidate;
                    }
                }
                else
                {
                    lower = dt0;
                    if(dt0Candidate >= upper)
                    {
                        // Candidate is outside the root-bounding interval.  Use
                        // bisection instead.
                        dt0 = (0.5f) * (upper + lower);
                    }
                    else
                    {
                        // There is no need to compare to 'lower' because the tangent
                        // line has positive slope, guaranteeing that the t-axis
                        // intercept is larger than 'lower'.
                        dt0 = dt0Candidate;
                    }
                }
            }

            // A root was not found according to the specified number of iterations
            // and tolerance.  You might want to increase iterations or tolerance or
            // integration accuracy.  However, in this application it is likely that
            // the time values are oscillating, due to the limited numerical
            // precision of 32-bit floats.  It is safe to use the last computed time.
            return mTimes[key] + dt0;
        }

        // These quantities are allocated by GetLength when they are needed the
        // first time.  The allocations occur in InitializeLength (called by
        // GetLength), so this member function must be 'const'. In order to
        // allocate the arrays in a 'const' function, they must be declared as
        // 'mutable'.

        protected void GetKeyInfo (float t, out int key, out float dt)
        {
            key = 0;
            dt = 0f;
            if(t <= mTimes[0])
            {
                return;
            }
            else if (t >= mTimes[mNumSegments])
            {
                key = mNumSegments - 1;
                dt = mTimes[mNumSegments] - mTimes[mNumSegments - 1];
            }
            else
            {
                for(int i = 0; i < mNumSegments; ++i)
                {
                    if (t < mTimes[i + 1])
                    {
                        key = i;
                        dt = t - mTimes[i];
                        break;
                    }
                }
            }
        }

        protected void InitializeLength ()
        {
            mLengths = new float[mNumSegments];
            mAccumLengths = new float[mNumSegments];

            // Arc lengths of the segments.
            int key;
            for(key = 0; key < mNumSegments; ++key)
            {
                mLengths[key] = GetLengthKey(key, 0f, mTimes[key + 1] - mTimes[key]);
            }

            // Accumulative arc length.
            mAccumLengths[0] = mLengths[0];
            for(key = 1; key < mNumSegments; ++key)
            {
                mAccumLengths[key] = mAccumLengths[key - 1] + mLengths[key];
            }
        }

        protected abstract float GetSpeedKey(int key, float t);
        protected abstract float GetLengthKey(int key, float t0, float t1);

        protected static float GetSpeedWithData (float t, object data)
        {
            KeyValuePair<MultipleCurve, int> kvp = (KeyValuePair<MultipleCurve, int>)(data);
            MultipleCurve multi = kvp.Key;
            int key = kvp.Value;
            return multi.GetSpeedKey(key, t);
        }
    }
}