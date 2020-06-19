//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

#define CCW

using System.Collections.Generic;

using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class SuperEllipsoid : Geometry
    {
        //  Stack numbering example        
        //  with stackDivision = 2         
        //                                 
        //  stack         rel    known     
        //  base0  stack  stack  as        
        //  -------------------------------
        //   5      3      1.0    top      
        //   4      2                      
        //   3      1                      
        //   2      0      0.0    equator  
        //   1     -1                      
        //   0     -2                      
        //  -1     -3     -1.0    bottom   
        //                                 
        //  bottom: relStack = -1.0  <=>  stackBase0 = -1
        //  top:    relStack =  1.0  <=>  stack = stackDivision + 1  <=>  stackBase0 = 2 * stackDivision + 1
        protected class MakeInfo
        {
            public Vector3      radius;
            public int          sliceCount;
            public int          stackDivision;
            public int          stackCount;
            public int          stackBase0Bottom;
            public int          stackBase0Top;

            public double       n1;
            public double       n2;

            public List<Point>  points;
            public Point        top;
            public Point        bottom;

            public Dictionary<Point, Vector3>   pointLocations;
            public Dictionary<Point, Vector3>   pointNormals;
            public Dictionary<Point, Vector2>   pointTexcoords;
            public Dictionary<Corner, Vector2>  cornerTexcoords;
            public Dictionary<Polygon, Vector3> polygonCentroids;
            public Dictionary<Polygon, Vector3> polygonNormals;

            public MakeInfo(Vector3 radius, double n1, double n2, int sliceCount, int stackDivision)
            {
                this.radius             = radius;
                this.sliceCount         = sliceCount;
                this.stackDivision      = stackDivision;
                this.stackCount         = (2 * stackDivision) + 1;
                this.stackBase0Bottom   = -1;
                this.stackBase0Top      = 2 * stackDivision + 1;
                this.n1 = n1;
                this.n2 = n2;
                points = new List<Point>();
            }
        };

        void MakeCorner(MakeInfo info, Polygon polygon, int slice, int stackBase0)
        {
            //int stack_base0 = stack + info.stackDivision;
            int     stack       = stackBase0 - info.stackDivision;
            double  relSlice    = (double)(slice) / (double)(info.sliceCount);
            double  relStack    = (double)(stack) / (double)(info.stackDivision + 1);
            bool    sliceSeam   = /*(slice == 0) ||*/ (slice == info.sliceCount);
            bool    bottom      = (stackBase0 == info.stackBase0Bottom);
            bool    top         = (stackBase0 == info.stackBase0Top);
            bool    uvDiscontinuity = sliceSeam || bottom || top;

            Point point;
            if(top)
            {
                point = info.top;
            }
            else if(bottom)
            {
                point = info.bottom;
            }
            else
            {
                if(slice == info.sliceCount)
                {
                    point = info.points[stackBase0];
                }
                else
                {
                    point = info.points[(slice * info.stackCount) + stackBase0];
                }
            }

            Corner corner = polygon.MakeCorner(point);

            if(uvDiscontinuity)
            {
                float s = 1.0f - (float)(relSlice);
                float t = 1.0f - (float)(0.5 * (1.0 + relStack));

                info.cornerTexcoords[corner] = new Vector2(s, t);
            }
        }
        public SuperEllipsoid(Vector3 radius, double n1, double n2, int sliceCount, int stackDivision)
        {
            MakeInfo info = new MakeInfo(radius, n1, n2, sliceCount, stackDivision);

            info.pointLocations     = PointAttributes.FindOrCreate<Vector3>("point_locations");
            info.pointNormals       = PointAttributes.FindOrCreate<Vector3>("point_normals");
            info.pointTexcoords     = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
            info.polygonCentroids   = PolygonAttributes.FindOrCreate<Vector3>("polygon_centroids");
            info.polygonNormals     = PolygonAttributes.FindOrCreate<Vector3>("polygon_normals");
            info.cornerTexcoords    = CornerAttributes.FindOrCreate<Vector2>("corner_texcoords");

            int slice;
            int stack;
            for(slice = 0; slice < sliceCount; ++slice)
            {
                double relSlice = (double)(slice) / (double)(sliceCount);
                for(stack = -stackDivision; stack <= stackDivision; ++stack)
                {
                    double relStack = (double)(stack) / (double)(stackDivision + 1);
                    Point  point    = MakePoint(info, relSlice, relStack);

                    info.points.Add(point);
                }
            }
            info.bottom = MakePoint(info, 0.5f, -1.0f);
            info.top    = MakePoint(info, 0.5f,  1.0f);

            #region bottom fan
            {
                for(slice = 0; slice < sliceCount; ++slice)
                {
                    int nextSlice  = (slice + 1);
                    int stackBase0 = 0;

                    double relSlice = ((double)(slice) + 0.5) / (double)(sliceCount);
                    double relStack = -1.0 + (0.5 / (double)(stackDivision + 1));

                    Point centroid = MakePoint(info, relSlice, relStack);

                    var polygon = MakePolygon();
#if CCW
                    MakeCorner(info, polygon, slice, stackBase0);
                    MakeCorner(info, polygon, slice, info.stackBase0Bottom);
                    MakeCorner(info, polygon, nextSlice, stackBase0);
#else
                    MakeCorner(info, polygon, nextSlice, stackBase0);
                    MakeCorner(info, polygon, slice, info.stackBase0Bottom);
                    MakeCorner(info, polygon, slice, stackBase0);
#endif

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }
            #endregion

            #region middle quads, bottom up
            for(
                stack = -stackDivision; 
                stack < stackDivision; 
                ++stack
            )
            {
                int stackBase0     = stack + stackDivision;
                int nextStackBase0 = stackBase0 + 1;

                double relStack = ((double)(stack) + 0.5) / (double)(stackDivision + 1);

                for(slice = 0; slice < sliceCount; ++slice)
                {
                    int    nextSlice = (slice + 1);
                    double relSlice  = ((double)(slice) + 0.5) / (double)(sliceCount);
                    Point  centroid  = MakePoint(info, relSlice, relStack);

                    var polygon = MakePolygon();
#if CCW
                    MakeCorner(info, polygon, nextSlice, nextStackBase0);
                    MakeCorner(info, polygon, slice,     nextStackBase0);
                    MakeCorner(info, polygon, slice,     stackBase0);
                    MakeCorner(info, polygon, nextSlice, stackBase0);
#else
                    MakeCorner(info, polygon, nextSlice, stackBase0);
                    MakeCorner(info, polygon, slice,     stackBase0);
                    MakeCorner(info, polygon, slice,     nextStackBase0);
                    MakeCorner(info, polygon, nextSlice, nextStackBase0);
#endif

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }
            #endregion

            #region top fan
            for(slice = 0; slice < sliceCount; ++slice)
            {
                int nextSlice  = (slice + 1);
                int stackBase0 = stackDivision + stackDivision;

                double relSlice = ((double)(slice) + 0.5) / (double)(sliceCount);
                double relStack = 1.0 - (0.5 / (double)(stackDivision + 1));

                Point centroid = MakePoint(info, relSlice, relStack);

                var polygon = MakePolygon();
#if CCW
                MakeCorner(info, polygon, slice,     info.stackBase0Top);
                MakeCorner(info, polygon, slice,     stackBase0);
                MakeCorner(info, polygon, nextSlice, stackBase0);
#else
                MakeCorner(info, polygon, nextSlice, stackBase0);
                MakeCorner(info, polygon, slice,     stackBase0);
                MakeCorner(info, polygon, slice,     info.stackBase0Top);
#endif

                info.polygonCentroids[polygon] = info.pointLocations[centroid];
                info.polygonNormals  [polygon] = info.pointNormals  [centroid];
            }
            #endregion
        }

        private double SPow(double x, double p)
        {
            return System.Math.Sign(x) * System.Math.Pow(System.Math.Abs(x), p);
        }

        protected Point MakePoint(MakeInfo info, double relSlice, double relStack)
        {
#if false
            double  beta            = (System.Math.PI * 2.0 * relSlice);
            double  sinBeta         = System.Math.Sin(beta);
            double  cosBeta         = System.Math.Cos(beta);

            double  theta           = (System.Math.PI * 0.5  * relStack);
            double  sinTheta        = System.Math.Sin(theta);
            double  cosTheta        = System.Math.Cos(theta);

            double  ctn1    = System.Math.Sign(cosTheta) * System.Math.Pow(System.Math.Abs(cosTheta), info.n1);
            double  xP      = ctn1 * System.Math.Sign(cosBeta) * System.Math.Pow(System.Math.Abs(cosBeta), info.n2);
            double  yP      = ctn1 * System.Math.Sign(sinBeta) * System.Math.Pow(System.Math.Abs(sinBeta), info.n2);
            double  zP      = System.Math.Sign(sinTheta) * System.Math.Pow(System.Math.Abs(sinTheta), info.n1);

            double  ct2mn1  = System.Math.Sign(cosTheta) * System.Math.Pow(System.Math.Abs(cosTheta), 2.0 - info.n1);
            double  xVN     = ct2mn1 * System.Math.Sign(cosBeta) * System.Math.Pow(System.Math.Abs(cosBeta), 2.0 - info.n2);
            double  yVN     = ct2mn1 * System.Math.Sign(sinBeta) * System.Math.Pow(System.Math.Abs(sinBeta), 2.0 - info.n2);
            double  zVN     = System.Math.Sign(sinTheta) * System.Math.Pow(System.Math.Abs(sinTheta), 2.0 - info.n1);
#endif
#if false
            double a1 = info.radius.X;
            double a2 = info.radius.Y;
            double a3 = info.radius.Z;
            double e1 = info.n1;
            double e2 = info.n2;
            double neta = (System.Math.PI * 0.5  * relStack);   //  -Pi / 2 .. Pi / 2
            double omega = (System.Math.PI * 2.0 * relSlice);   //      -Pi .. Pi
            double cosNeta = System.Math.Cos(neta);
            double sinNeta = System.Math.Sin(neta);
            double cosOmega = System.Math.Cos(omega);
            double sinOmega = System.Math.Sin(omega);

            /*  2.10  */
            double xP = a1 * SPow(cosNeta, e1) * SPow(cosOmega, e2);
            double yP = a2 * SPow(cosNeta, e1) * SPow(sinOmega, e2);
            double zP = a3 * SPow(sinNeta, e1);

            double a1Inv = 1.0 / info.radius.X;
            double a2Inv = 1.0 / info.radius.Y;
            double a3Inv = 1.0 / info.radius.Z;

            /*  2.39  */
            double xVN  = a1Inv * SPow(cosNeta, 2.0 - e1) * SPow(cosOmega, 2.0 - e2);
            double yVN  = a2Inv * SPow(cosNeta, 2.0 - e1) * SPow(sinOmega, 2.0 - e2);
            double zVN  = a3Inv * SPow(sinNeta, 2.0 - e1);
#endif
            double  phi             = (System.Math.PI * 2.0 * relSlice);
            double  sin_phi         = System.Math.Sin(phi);
            double  cos_phi         = System.Math.Cos(phi);

            double  theta           = (System.Math.PI * 0.5  * relStack);
            double  sin_theta       = System.Math.Sin(theta);
            double  cos_theta       = System.Math.Cos(theta);

            double  stack_radius    = cos_theta;

            float   xVN             = (float)(stack_radius * cos_phi);
            float   yVN             = (float)(sin_theta);
            float   zVN             = (float)(stack_radius * sin_phi);

            float   xP              = (float)(info.radius.X * xVN);
            float   yP              = (float)(info.radius.Y * yVN);
            float   zP              = (float)(info.radius.Z * zVN);

            xP = (float)SPow((double)xP, info.n1);
            yP = (float)SPow((double)yP, info.n1 * info.n2);
            zP = (float)SPow((double)zP, info.n2);


            float   s       = 1.0f - (float)(relSlice);
            float   t       = 1.0f - (float)(0.5 * (1.0 + relStack));

            Point point = MakePoint();

            bool uvDiscontinuity = (relStack == -1.0) || (relStack == 1.0) || /*(relSlice == 0.0) ||*/ (relSlice == 1.0);

            info.pointLocations[point] = new Vector3(xP, yP, zP);
            info.pointNormals  [point] = new Vector3(xVN, yVN, zVN);
            if(uvDiscontinuity == false)
            {
                info.pointTexcoords[point] = new Vector2(s, t);
            }

            return point;
        }
    }
}
