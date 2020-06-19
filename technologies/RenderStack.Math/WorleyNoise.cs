/* Copyright 1994, 2002 by Steven Worley
This software may be modified and redistributed without restriction
provided this comment header remains intact in the source code.
This code is provided with no warrantee, express or implied, for
any purpose.

A detailed description and application examples can be found in the
1996 SIGGRAPH paper "A Cellular Texture Basis Function" and
especially in the 2002 book "Texturing and Modeling, a Procedural
Approach, 3rd edition." There is also extra information on the web
site http://www.worley.com/cellular.html .

If you do find interesting uses for this tool, and especially if
you enhance it, please drop me an email at steve@worley.com. */

// Conversion to C# Timo Suoranta - Not tested yet

using System;

namespace RenderStack.Math
{
    public class WorleyNoise
    {
        /* This macro is a *lot* faster than using (long)floor() on an x86 CPU.
        It actually speeds up the entire Worley() call with almost 10%.
        Added by Stefan Gustavson, October 2003. */
        static long LFLOOR(float x)
        {
            return ((x) < 0 ? ((long)x - 1) : ((long)x));
        }

        /* A hardwired lookup table to quickly determine how many feature
        points should be in each spatial cube. We use a table so we don't
        need to make multiple slower tests.  A random number indexed into
        this array will give an approximate Poisson distribution of mean
        density 2.5. Read the book for the longwinded explanation. */
        static readonly int[] Poisson_count =
        {
            4,3,1,1,1,2,4,2,2,2,5,1,0,2,1,2,2,0,4,3,2,1,2,1,3,2,2,4,2,2,5,1,2,3,2,2,2,2,2,3,
            2,4,2,5,3,2,2,2,5,3,3,5,2,1,3,3,4,4,2,3,0,4,2,2,2,1,3,2,2,2,3,3,3,1,2,0,2,1,1,2,
            2,2,2,5,3,2,3,2,3,2,2,1,0,2,1,1,2,1,2,2,1,3,4,2,2,2,5,4,2,4,2,2,5,4,3,2,2,5,4,3,
            3,3,5,2,2,2,2,2,3,1,1,4,2,1,3,3,4,3,2,4,3,3,3,4,5,1,4,2,4,3,1,2,3,5,3,2,1,3,1,3,
            3,3,2,3,1,5,5,4,2,2,4,1,3,4,1,5,3,3,5,3,4,3,2,2,1,1,1,1,1,2,4,5,4,5,4,2,1,5,1,1,
            2,3,3,3,2,5,2,3,3,2,0,2,1,1,4,2,1,3,2,1,2,2,3,2,5,5,3,4,5,5,2,4,4,5,3,2,2,2,1,4,
            2,3,3,4,2,5,4,2,4,2,2,2,4,5,3,2
        };

        /* This constant is manipulated to make sure that the mean value of F[0]
        is 1.0. This makes an easy natural "scale" size of the cellular features. */
        const float DENSITY_ADJUSTMENT = 0.398150f;

        /* The main function! */
        void Worley(
            Vector3     at, 
            long        max_order,
            double[]    F, 
            Vector3[]   delta, 
            ulong[]     ID
        )
        {
            /* Initialize the F values to "huge" so they will be replaced by the
            first real sample tests. Note we'll be storing and comparing the
            SQUARED distance from the feature points to avoid lots of slow
            sqrt() calls. We'll use sqrt() only on the final answer. */
            for(int i = 0; i < max_order; i++)
            {
                F[i] = 999999.9;
            }
  
            /* Make our own local copy, multiplying to make mean(F[0])==1.0  */
            Vector3 new_at = DENSITY_ADJUSTMENT * at;

            /* Find the integer cube holding the hit point */
            Vector3 floor_at = Vector3.Floor(new_at); /* The macro makes this part a lot faster */
            IVector3 int_at = Vector3.IFloor(new_at); /* The macro makes this part a lot faster */

            /* A simple way to compute the closest neighbors would be to test all
            boundary cubes exhaustively. This is simple with code like: 
            {
            long ii, jj, kk;
            for (ii=-1; ii<=1; ii++) for (jj=-1; jj<=1; jj++) for (kk=-1; kk<=1; kk++)
            AddSamples(int_at[0]+ii,int_at[1]+jj,int_at[2]+kk, 
            max_order, new_at, F, delta, ID);
            }
            But this wastes a lot of time working on cubes which are known to be
            too far away to matter! So we can use a more complex testing method
            that avoids this needless testing of distant cubes. This doubles the 
            speed of the algorithm. */

            /* Test the central cube for closest point(s). */
            AddSamples(int_at, 0,0,0, max_order, new_at, F, delta, ID);

            /* We test if neighbor cubes are even POSSIBLE contributors by examining the
            combinations of the sum of the squared distances from the cube's lower 
            or upper corners.*/
            Vector3 v = new_at - floor_at;
            Vector3 iv = Vector3.One - v;
            Vector3 m = iv * iv;
            v *= v;
  
            /* Test 6 facing neighbors of center cube. These are closest and most 
            likely to have a close feature point. */
            if(v.X < F[max_order-1]) AddSamples(int_at, -1, 0, 0, max_order, new_at, F, delta, ID);
            if(v.Y < F[max_order-1]) AddSamples(int_at,  0,-1, 0, max_order, new_at, F, delta, ID);
            if(v.Z < F[max_order-1]) AddSamples(int_at,  0, 0,-1, max_order, new_at, F, delta, ID);
  
            if(m.X < F[max_order-1]) AddSamples(int_at, 1, 0, 0, max_order, new_at, F, delta, ID);
            if(m.Y < F[max_order-1]) AddSamples(int_at, 0, 1, 0, max_order, new_at, F, delta, ID);
            if(m.Z < F[max_order-1]) AddSamples(int_at, 0, 0, 1, max_order, new_at, F, delta, ID);
  
            /* Test 12 "edge cube" neighbors if necessary. They're next closest. */
            if(v.X + v.Y < F[max_order-1]) AddSamples(int_at,-1, -1,  0, max_order, new_at, F, delta, ID);
            if(v.X + v.Z < F[max_order-1]) AddSamples(int_at,-1,  0, -1, max_order, new_at, F, delta, ID);
            if(v.Y + v.Z < F[max_order-1]) AddSamples(int_at, 0, -1, -1, max_order, new_at, F, delta, ID);  
            if(m.X + m.Y < F[max_order-1]) AddSamples(int_at,+1, +1,  0, max_order, new_at, F, delta, ID);
            if(m.X + m.Z < F[max_order-1]) AddSamples(int_at,+1,  0, +1, max_order, new_at, F, delta, ID);
            if(m.Y + m.Z < F[max_order-1]) AddSamples(int_at, 0, +1, +1, max_order, new_at, F, delta, ID);  
            if(v.X + m.Y < F[max_order-1]) AddSamples(int_at,-1, +1,  0, max_order, new_at, F, delta, ID);
            if(v.X + m.Z < F[max_order-1]) AddSamples(int_at,-1,  0, +1, max_order, new_at, F, delta, ID);
            if(v.Y + m.Z < F[max_order-1]) AddSamples(int_at, 0, -1, +1, max_order, new_at, F, delta, ID);  
            if(m.X + v.Y < F[max_order-1]) AddSamples(int_at,+1, -1,  0, max_order, new_at, F, delta, ID);
            if(m.X + v.Z < F[max_order-1]) AddSamples(int_at,+1,  0, -1, max_order, new_at, F, delta, ID);
            if(m.Y + v.Z < F[max_order-1]) AddSamples(int_at, 0, +1, -1, max_order, new_at, F, delta, ID);  
  
            /* Final 8 "corner" cubes */
            if(v.X+v.Y+v.Z < F[max_order-1]) AddSamples(int_at, -1, -1, -1, max_order, new_at, F, delta, ID);
            if(v.X+v.Y+m.Z < F[max_order-1]) AddSamples(int_at, -1, -1, +1, max_order, new_at, F, delta, ID);
            if(v.X+m.Y+v.Z < F[max_order-1]) AddSamples(int_at, -1, +1, -1, max_order, new_at, F, delta, ID);
            if(v.X+m.Y+m.Z < F[max_order-1]) AddSamples(int_at, -1, +1, +1, max_order, new_at, F, delta, ID);
            if(m.X+v.Y+v.Z < F[max_order-1]) AddSamples(int_at, +1, -1, -1, max_order, new_at, F, delta, ID);
            if(m.X+v.Y+m.Z < F[max_order-1]) AddSamples(int_at, +1, -1, +1, max_order, new_at, F, delta, ID);
            if(m.X+m.Y+v.Z < F[max_order-1]) AddSamples(int_at, +1, +1, -1, max_order, new_at, F, delta, ID);
            if(m.X+m.Y+m.Z < F[max_order-1]) AddSamples(int_at, +1, +1, +1, max_order, new_at, F, delta, ID);
  
            /* We're done! Convert everything to right size scale */
            for(int i = 0; i < max_order; i++)
            {
                F[i] = (float)System.Math.Sqrt(F[i]) * (1.0 / DENSITY_ADJUSTMENT);
                delta[i] *= (1.0f / DENSITY_ADJUSTMENT);
            }
  
            return;
        }

        /* the function to merge-sort a "cube" of samples into the current best-found
        list of values. */
        static void AddSamples(
            IVector3    iv, 
            int idx, int idy, int idz, 
            long        max_order,
            Vector3     at, 
            double[]    F,
            Vector3[]   delta, 
            ulong[]     ID
        )
        {
            Vector3 d;
            Vector3 f;
            double  d2;
            long    count, i, j, index;
            ulong   this_id;
            long   xi = iv.X + idx;
            long   yi = iv.Y + idy;
            long   zi = iv.Z + idz;
  
            /* Each cube has a random number seed based on the cube's ID number.
            The seed might be better if it were a nonlinear hash like Perlin uses
            for noise but we do very well with this faster simple one.
            Our LCG uses Knuth-approved constants for maximal periods. */
            ulong seed = (ulong)(702395077 * xi + 915488749 * yi + 2120969693 * zi);
  
            /* How many feature points are in this cube? */
            count = Poisson_count[seed >> 24]; /* 256 element lookup table. Use MSB */

            seed = 1402024253 * seed + 586950981; /* churn the seed with good Knuth LCG */

            for(j = 0; j < count; j++) /* test and insert each point into our solution */
            {
                this_id = seed;
                seed = 1402024253 * seed + 586950981; /* churn */

                /* compute the 0..1 feature point location's XYZ */
                f.X = (seed + 0.5f) * (1.0f / 4294967296.0f); 
                seed = 1402024253 * seed + 586950981; /* churn */
                f.Y = (seed + 0.5f) * (1.0f / 4294967296.0f);
                seed = 1402024253 * seed + 586950981; /* churn */
                f.Z = (seed + 0.5f) * (1.0f / 4294967296.0f);
                seed = 1402024253 * seed + 586950981; /* churn */

                /* delta from feature point to sample location */
                d.X = xi + f.X - at.X; 
                d.Y = yi + f.Y - at.Y;
                d.Z = zi + f.Z - at.Z;

                /* Distance computation!  Lots of interesting variations are
                possible here!
                Biased "stretched"   A*dx*dx+B*dy*dy+C*dz*dz
                Manhattan distance   fabs(dx)+fabs(dy)+fabs(dz)
                Radial Manhattan:    A*fabs(dR)+B*fabs(dTheta)+C*dz
                Superquadratic:      pow(fabs(dx), A) + pow(fabs(dy), B) + pow(fabs(dz),C)

                Go ahead and make your own! Remember that you must insure that
                new distance function causes large deltas in 3D space to map into
                large deltas in your distance function, so our 3D search can find
                them! [Alternatively, change the search algorithm for your special
                cases.]
                */

                d2 = d.X * d.X + d.Y * d.Y + d.Z * d.Z; /* Euclidian distance, squared */

                if(d2 < F[max_order - 1]) /* Is this point close enough to rememember? */
                {
                    /* Insert the information into the output arrays if it's close enough.
                    We use an insertion sort.  No need for a binary search to find
                    the appropriate index.. usually we're dealing with order 2,3,4 so
                    we can just go through the list. If you were computing order 50
                    (wow!!) you could get a speedup with a binary search in the sorted
                    F[] list. */

                    index = max_order;
                    while(index > 0 && d2 < F[index - 1])
                    {
                        index--;
                    }

                    /* We insert this new point into slot # <index> */

                    /* Bump down more distant information to make room for this new point. */
                    for(i = max_order - 2; i >= index; i--)
                    {
                        F    [i + 1] = F[i];
                        ID   [i + 1] = ID[i];
                        delta[i + 1] = delta[i];
                    }

                    /* Insert the new point's information into the list. */
                    F    [index] = d2;
                    ID   [index] = this_id;
                    delta[index] = d;
                }
            }
        }
    }
}