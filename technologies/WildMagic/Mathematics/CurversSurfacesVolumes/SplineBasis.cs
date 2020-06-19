// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

using System;

using RenderStack.Math;

namespace WildMagic
{
    public class BSplineBasis
    {
        public BSplineBasis()
        {
        }

        // Open uniform or periodic uniform.  The knot array is internally
        // generated with equally spaced elements.
        public BSplineBasis(int numCtrlPoints, int degree, bool open)
        {
            Create(numCtrlPoints, degree, open);
        }
        public void Create(int numCtrlPoints, int degree, bool open)
        {
            mUniform = true;

            int i, numKnots = Initialize(numCtrlPoints, degree, open);
            float factor = ((float)(1)) / (mNumCtrlPoints - mDegree);
            if(mOpen)
            {
                for(i = 0; i <= mDegree; ++i)
                {
                    mKnot[i] = (float)0;
                }

                for(/**/; i < mNumCtrlPoints; ++i)
                {
                    mKnot[i] = (i - mDegree) * factor;
                }

                for(/**/; i < numKnots; ++i)
                {
                    mKnot[i] = (float)1;
                }
            }
            else
            {
                for(i = 0; i < numKnots; ++i)
                {
                    mKnot[i] = (i - mDegree) * factor;
                }
            }
        }

        // Open nonuniform.  The knot array must have n-d elements.  The elements
        // must be nondecreasing.  Each element must be in [0,1].  The caller is
        // responsible for deleting knot.  An internal copy is made, so to
        // dynamically change knots you must use the SetKnot function.
        public BSplineBasis(int numCtrlPoints, int degree, float[] knot)
        {
            Create(numCtrlPoints, degree, knot);
        }
        public void Create(int numCtrlPoints, int degree, float[] knot)
        {
            mUniform = false;

            int i, numKnots = Initialize(numCtrlPoints, degree, true);
            for(i = 0; i <= mDegree; ++i)
            {
                mKnot[i] = (float)0;
            }

            for(int j = 0; i < mNumCtrlPoints; ++i, ++j)
            {
                mKnot[i] = knot[j];
            }

            for(/**/; i < numKnots; ++i)
            {
                mKnot[i] = (float)1;
            }
        }


        public int GetNumCtrlPoints()
        {
            return mNumCtrlPoints;
        }
        public int GetDegree()
        {
            return mDegree;
        }
        public bool IsOpen()
        {
            return mOpen;
        }
        public bool IsUniform()
        {
            return mUniform;
        }

        // The knot values can be changed only if the basis function is nonuniform
        // and the input index is valid (0 <= i <= n-d-1).  If these conditions
        // are not satisfied, GetKnot returns MAX_float.
        public void SetKnot(int i, float knot)
        {
            if(!mUniform)
            {
                // Access only allowed to elements d+1 <= j <= n.
                int j = i + mDegree + 1;
                if(mDegree + 1 <= j && j <= mNumCtrlPoints - 1)
                {
                    mKnot[j] = knot;
                }
            }
        }
        public float GetKnot(int i)
        {
            if(!mUniform)
            {
                // Access only allowed to elements d+1 <= j <= n.
                int j = i + mDegree + 1;
                if(mDegree + 1 <= j && j <= mNumCtrlPoints - 1)
                {
                    return mKnot[j];
                }
            }

            return Single.MaxValue;
        }

        // Access basis functions and their derivatives.
        public float GetD0(int i)
        {
            return mBD0[mDegree, i];
        }
        public float GetD1(int i)
        {
            return mBD1[mDegree, i];
        }
        public float GetD2(int i)
        {
            return mBD2[mDegree, i];
        }
        public float GetD3(int i)
        {
            return mBD3[mDegree, i];
        }

        // Evaluate basis functions and their derivatives.
        public void Compute(float t, int order, out int minIndex, out int maxIndex)
        {
            // assertion(order <= 3, "Only derivatives to third order supported\n");

            if(order >= 1)
            {
                if(mBD1 == null)
                {
                    mBD1 = Allocate();
                }

                if(order >= 2)
                {
                    if(mBD2 == null)
                    {
                        mBD2 = Allocate();
                    }

                    if(order >= 3)
                    {
                        if(mBD3 == null)
                        {
                            mBD3 = Allocate();
                        }
                    }
                }
            }

            int i = GetKey(ref t);
            if(i == 3)
            {
                i = GetKey(ref t);
            }
            mBD0[0, i] = (float)1;

            if(order >= 1)
            {
                mBD1[0, i] = (float)0;
                if(order >= 2)
                {
                    mBD2[0, i] = (float)0;
                    if (order >= 3)
                    {
                        mBD3[0, i] = (float)0;
                    }
                }
            }

            float n0 = t - mKnot[i], n1 = mKnot[i + 1] - t;
            float invD0, invD1;
            int j;
            for(j = 1; j <= mDegree; j++)
            {
                invD0 = ((float)1) / (mKnot[i + j] - mKnot[i]);
                invD1 = ((float)1) / (mKnot[i + 1] - mKnot[i - j + 1]);

                mBD0[j, i] = n0 * mBD0[j - 1, i] * invD0;
                mBD0[j, i - j] = n1 * mBD0[j - 1, i - j + 1] * invD1;

                if(order >= 1)
                {
                    mBD1[j, i] = (n0 * mBD1[j - 1, i] + mBD0[j - 1, i]) * invD0;
                    mBD1[j, i - j] = (n1 * mBD1[j - 1, i - j + 1] - mBD0[j - 1, i - j + 1]) * invD1;

                    if(order >= 2)
                    {
                        mBD2[j, i] = (n0 * mBD2[j - 1, i] + ((float)2) * mBD1[j - 1, i]) * invD0;
                        mBD2[j, i - j] = (n1 * mBD2[j - 1, i - j + 1] - 
                            ((float)2) * mBD1[j - 1, i - j + 1]) * invD1;

                        if(order >= 3)
                        {
                            mBD3[j, i] = (n0 * mBD3[j - 1, i] + 
                                ((float)3) * mBD2[j - 1, i]) * invD0;
                            mBD3[j, i - j] = (n1 * mBD3[j - 1, i - j + 1] - 
                                ((float)3) * mBD2[j - 1, i - j + 1]) * invD1;
                        }
                    }
                }
            }

            for(j = 2; j <= mDegree; ++j)
            {
                for(int k = i - j + 1; k < i; ++k)
                {
                    n0 = t - mKnot[k];
                    n1 = mKnot[k + j + 1] - t;
                    invD0 = ((float)1) / (mKnot[k + j] - mKnot[k]);
                    invD1 = ((float)1) / (mKnot[k + j + 1] - mKnot[k + 1]);

                    mBD0[j, k] = n0 * mBD0[j - 1, k] * invD0 + n1 * mBD0[j - 1, k + 1] * invD1;

                    if(order >= 1)
                    {
                        mBD1[j, k] = (n0 * mBD1[j - 1, k] + mBD0[j - 1, k]) * invD0 + 
                            (n1 * mBD1[j - 1, k + 1] - mBD0[j - 1, k + 1]) * invD1;

                        if(order >= 2)
                        {
                            mBD2[j, k] = (n0 * mBD2[j - 1, k] +
                                ((float)2) * mBD1[j - 1, k]) * invD0 +
                                (n1 * mBD2[j - 1, k + 1] - ((float)2) * mBD1[j - 1, k + 1]) * invD1;

                            if(order >= 3)
                            {
                                mBD3[j, k] = (n0 * mBD3[j - 1, k] +
                                    ((float)3) * mBD2[j - 1, k]) * invD0 +
                                    (n1 * mBD3[j - 1, k + 1] - ((float)3) *
                                    mBD2[j - 1, k + 1]) * invD1;
                            }
                        }
                    }
                }
            }

            minIndex = i - mDegree;
            maxIndex = i;
        }

        protected int Initialize(int numCtrlPoints, int degree, bool open)
        {
            //assertion(numCtrlPoints >= 2, "Invalid input\n");
            //assertion(1 <= degree && degree <= numCtrlPoints-1, "Invalid input\n");

            mNumCtrlPoints = numCtrlPoints;
            mDegree = degree;
            mOpen = open;

            int numKnots = mNumCtrlPoints + mDegree + 1;
            mKnot = new float[numKnots];

            mBD0 = Allocate();
            mBD1 = null;
            mBD2 = null;
            mBD3 = null;

            return numKnots;
        }
        protected float[,] Allocate()
        {
            int numRows = mDegree + 1;
            int numCols = mNumCtrlPoints + mDegree;
            float[,] data = new float[numRows, numCols];
            return data;
        }

        // Determine knot index i for which knot[i] <= rfTime < knot[i+1].
        protected int GetKey(ref float t)
        {
            if(mOpen)
            {
                // Open splines clamp to [0,1].
                if(t <= (float)0)
                {
                    t = (float)0;
                    return mDegree;
                }
                else if(t >= (float)1)
                {
                    t = (float)1;
                    return mNumCtrlPoints - 1;
                }
            }
            else
            {
                // Periodic splines wrap to [0,1).
                if(t < (float)0 || t >= (float)1)
                {
                    t -= (float)Math.Floor(t);
                }
            }


            int i;

            if(mUniform)
            {
                i = mDegree + (int)((mNumCtrlPoints - mDegree)*t);
            }
            else
            {
                for(i = mDegree + 1; i <= mNumCtrlPoints; ++i)
                {
                    if(t < mKnot[i])
                    {
                        break;
                    }
                }
                --i;
            }

            return i;
        }

        protected int       mNumCtrlPoints;     // n + 1
        protected int       mDegree;            // d
        protected float[]   mKnot;              // knot[n + d + 2]
        protected bool      mOpen, mUniform;

        // Storage for the basis functions and their derivatives first three
        // derivatives.  The basis array is always allocated by the constructor
        // calls.  A derivative basis array is allocated on the first call to a
        // derivative member function.
        protected float[,] mBD0;  // bd0[d+1][n+d+1]
        protected float[,] mBD1;  // bd1[d+1][n+d+1]
        protected float[,] mBD2;  // bd2[d+1][n+d+1]
        protected float[,] mBD3;  // bd3[d+1][n+d+1]
    }
}