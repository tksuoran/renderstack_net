// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

using System;

using RenderStack.Math;

namespace WildMagic
{
    public abstract class SingleCurve : Curve
    {
        private static float GetSpeedWithData(float t, object data)
        {
            return ((Curve)data).GetSpeed(t);
        }

        // Abstract base class.
        public SingleCurve(float tmin, float tmax)
            : base(tmin, tmax)
        {
        }

        // Length-from-time and time-from-length.
        public override float GetLength(float t0, float t1)
        {
            //assertion(mTMin <= t0 && t0 <= mTMax, "Invalid input\n");
            //assertion(mTMin <= t1 && t1 <= mTMax, "Invalid input\n");
            //assertion(t0 <= t1, "Invalid input\n");

            return Integrate.RombergIntegral(
                8, 
                t0, 
                t1, 
                GetSpeedWithData,
                this
            );
        }

        public override float GetTime(
            float   length, 
            int     iterations,
            float   tolerance
        )
        {
            if(length <= 0f)
            {
                return mTMin;
            }

            if(length >= GetTotalLength())
            {
                return mTMax;
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

            // Initial guess for Newton's method.
            float ratio = length / GetTotalLength();
            float oneMinusRatio = (float)1 - ratio;
            float t = oneMinusRatio * mTMin + ratio * mTMax;

            // Initial root-bounding interval for bisection.
            float lower = mTMin, upper = mTMax;

            for(int i = 0; i < iterations; ++i)
            {
                float difference = GetLength(mTMin, t) - length;
                if(Math.Abs(difference) < tolerance)
                {
                    // |L(t)-length| is close enough to zero, report t as the time
                    // at which 'length' is attained.
                    return t;
                }

                // Generate a candidate for Newton's method.
                float tCandidate = t - difference/GetSpeed(t);

                // Update the root-bounding interval and test for containment of the
                // candidate.
                if(difference > (float)0)
                {
                    upper = t;
                    if(tCandidate <= lower)
                    {
                        // Candidate is outside the root-bounding interval.  Use
                        // bisection instead.
                        t = ((float)0.5) * (upper + lower);
                    }
                    else
                    {
                        // There is no need to compare to 'upper' because the tangent
                        // line has positive slope, guaranteeing that the t-axis
                        // intercept is smaller than 'upper'.
                        t = tCandidate;
                    }
                }
                else
                {
                    lower = t;
                    if(tCandidate >= upper)
                    {
                        // Candidate is outside the root-bounding interval.  Use
                        // bisection instead.
                        t = ((float)0.5) * (upper + lower);
                    }
                    else
                    {
                        // There is no need to compare to 'lower' because the tangent
                        // line has positive slope, guaranteeing that the t-axis
                        // intercept is larger than 'lower'.
                        t = tCandidate;
                    }
                }
            }

            // A root was not found according to the specified number of iterations
            // and tolerance.  You might want to increase iterations or tolerance or
            // integration accuracy.  However, in this application it is likely that
            // the time values are oscillating, due to the limited numerical
            // precision of 32-bit floats.  It is safe to use the last computed time.
            return t;
        }

    }
}
