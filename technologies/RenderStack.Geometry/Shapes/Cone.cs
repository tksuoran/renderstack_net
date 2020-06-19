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

using System;
using System.Collections.Generic;

using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    [System.Serializable]
    /*  Comment: Mostly stable.  */
    public class Cone : Geometry
    {
        //  Stack numbering example                                                                 
        //  with stackDivision = 2                                                                  
        //                                                                                          
        //  stack                        rel    known                                               
        //  base0  stack                 stack  as                                                  
        //  ----------------------------------------------------------------------------------------
        //   5      3         /\         1.0    top        (singular = single vertex in this case)  
        //   4      2        /--\                                                                   
        //   3      1       /----\                                                                  
        //   2      0      /======\      0.0    equator                                             
        //   1     -1     /--------\                                                                
        //   0     -2    /----------\                                                               
        //  -1     -3   /============\   -1.0    bottom    (not singular in this case)              
        //                                                                                          
        //  bottom: relStack = -1.0  <=>  stack = -stackDivision - 1
        //  top:    relStack =  1.0  <=>  stack =  stackDivision + 1
        protected class MakeInfo
        {
            public double   minX;
            public double   maxX;
            public double   bottomRadius;
            public double   topRadius;
            public bool     useBottom;
            public bool     useTop;
            public int      sliceCount;
            public int      stackDivision;

            public int      bottomNotSingular;
            public int      topNotSingular;
            public int      stackCount;

            public double   relSlice;
            public double   relStack;

            public Dictionary<KeyValuePair<int,int>, Point>  points;
            public Point        top;
            public Point        bottom;

            public Dictionary<Point, Vector3>   pointLocations;
            public Dictionary<Point, Vector3>   pointNormals;
            public Dictionary<Point, Vector2>   pointTexcoords;
            public Dictionary<Corner, Vector3>  cornerNormals;
            public Dictionary<Corner, Vector2>  cornerTexcoords;
            public Dictionary<Polygon, Vector3> polygonCentroids;
            public Dictionary<Polygon, Vector3> polygonNormals;

            public MakeInfo(
                double  minX, 
                double  maxX, 
                double  bottomRadius, 
                double  topRadius, 
                bool    useBottom, 
                bool    useTop, 
                int     sliceCount,
                int     stackDivision
            )
            {
                this.minX               = minX;
                this.maxX               = maxX;
                this.bottomRadius       = bottomRadius;
                this.topRadius          = topRadius;
                this.useBottom          = useBottom;
                this.useTop             = useTop;
                this.sliceCount         = sliceCount;
                this.stackDivision      = stackDivision;
                this.stackCount         = (2 * stackDivision) + 1 + bottomNotSingular + topNotSingular;

                this.bottomNotSingular  = (bottomRadius != 0.0) ? 1 : 0;
                this.topNotSingular     = (topRadius != 0.0) ? 1 : 0;
                points = new Dictionary<KeyValuePair<int,int>,Point>();
            }
        };

        private Point GetPoint(MakeInfo info, int slice, int stack)
        {
            return info.points[new KeyValuePair<int, int>(slice, stack)];
        }

        //  relStackIn is in range -1..1
        //  relStack is in range 0..1
        Point ConePoint(MakeInfo info, double relSlice, double relStackIn)
        {
            //var pointLocations      = PointAttributes.FindOrCreate<Vector3>("point_locations");
            //var pointNormals        = PointAttributes.FindOrCreate<Vector3>("point_normals");
            //var pointTexCoords      = PointAttributes.FindOrCreate<Vector2>("point_texcoords");

            double phi              = System.Math.PI * 2.0 * relSlice;
            double sinPhi           = System.Math.Sin(phi);
            double cosPhi           = System.Math.Cos(phi);
            double relStack         = 0.5 + 0.5 * relStackIn;
            double oneMinusRelStack = 1.0 - relStack;

            Vector3 position = new Vector3(
                (float)(oneMinusRelStack * (info.minX)                  + relStack * (info.maxX)),
                (float)(oneMinusRelStack * (info.bottomRadius * sinPhi) + relStack * (info.topRadius * sinPhi)),
                (float)(oneMinusRelStack * (info.bottomRadius * cosPhi) + relStack * (info.topRadius * cosPhi))
            );
            Vector3 bottom = new Vector3(
                (float)(info.minX),
                (float)(info.bottomRadius * sinPhi),
                (float)(info.bottomRadius * cosPhi)
            );
            Vector3 top = new Vector3(
                (float)(info.maxX),
                (float)(info.topRadius * sinPhi),
                (float)(info.topRadius * cosPhi)
            );

            Vector3 bitangent  = Vector3.Normalize(top - bottom);  /*  generatrix  */ 
            Vector3 tangent    = new Vector3(
                0.0f,
                (float)System.Math.Sin(phi + (System.Math.PI * 0.5)),
                (float)System.Math.Cos(phi + (System.Math.PI * 0.5))
            );
            Vector3 normal = Vector3.Cross(
                bitangent,
                tangent
            );
            normal = Vector3.Normalize(normal);

            double s = info.relSlice;
            double t = 0.5 + 0.5 * info.relStack;

            Point point = MakePoint();

            info.pointLocations[point] = new Vector3((float)position.X,  (float)position.Y,  (float)position.Z);
            info.pointNormals  [point] = new Vector3((float)normal.X,    (float)normal.Y,    (float)normal.Z);
            info.pointTexcoords[point] = new Vector2((float)s,           (float)t);

            return point;
        }
        Corner MakeCorner(MakeInfo info, Polygon polygon, int slice, int stack)
        {
            return MakeCorner(info, polygon, slice, stack, false);
        }
        Corner MakeCorner(MakeInfo info, Polygon polygon, int slice, int stack, bool cap)
        {
            double relSlice = (double)slice / (double)info.sliceCount;
            double relStack = (double)stack / (double)(info.stackDivision + 1);
            // centroid: double relSlice = ((double)slice + 0.5) / (double)info.sliceCount;
            //double relStack = -1.0 + (0.5 / (double)(info.stackDivision + 1));

            bool    sliceSeam   = (slice == 0) || (slice == info.sliceCount);
            bool    bottom      = stack == -info.stackDivision - 1;
            bool    top         = stack == info.stackDivision + 1;
            bool    uvDiscontinuity = sliceSeam || bottom || top;
            Point point;
            if(top && (info.topRadius == 0.0))
            {
                point = info.top;
            }
            else if(bottom && (info.bottomRadius == 0.0))
            {
                point = info.bottom;
            }
            else
            {
                if(slice == info.sliceCount)
                {
                    point = info.points[new KeyValuePair<int, int>(0, stack)];
                }
                else
                {
                    point = info.points[new KeyValuePair<int, int>(slice, stack)];
                }
            }

            Corner corner = polygon.MakeCorner(point);

            if(uvDiscontinuity)
            {
                float s = (float)(relSlice);
                float t = (float)(0.5 + 0.5 * relStack);

                info.cornerTexcoords[corner] = new Vector2(s, t);
            }

            if(cap && bottom && (info.bottomRadius != 0.0f) && info.useBottom)
            {
                info.cornerNormals[corner] = new Vector3(-1.0f, 0.0f, 0.0f);
            }
            if(cap && top && (info.topRadius != 0.0f) && info.useTop)
            {
                info.cornerNormals[corner] = new Vector3(1.0f, 0.0f, 0.0f);
            }
            return corner;
        }

        public Cone(
            double  minX, 
            double  maxX, 
            double  bottomRadius, 
            double  topRadius, 
            bool    useBottom, 
            bool    useTop, 
            int     sliceCount,
            int     stackDivision
        )
        {
            MakeInfo info = new MakeInfo(minX, maxX, bottomRadius, topRadius, useBottom, useTop, sliceCount, stackDivision);

            info.pointLocations     = PointAttributes.FindOrCreate<Vector3>("point_locations");
            info.pointNormals       = PointAttributes.FindOrCreate<Vector3>("point_normals");
            info.pointTexcoords     = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
            info.polygonCentroids   = PolygonAttributes.FindOrCreate<Vector3>("polygon_centroids");
            info.polygonNormals     = PolygonAttributes.FindOrCreate<Vector3>("polygon_normals");
            info.cornerNormals      = CornerAttributes.FindOrCreate<Vector3>("corner_normals");
            info.cornerTexcoords    = CornerAttributes.FindOrCreate<Vector2>("corner_texcoords");

            /*  Other vertices  */
            for(int slice = 0; slice <= sliceCount; ++slice)
            {
                double relSlice = (double)slice / (double)sliceCount;
                for(int stack = -stackDivision - info.bottomNotSingular; stack <= stackDivision + info.topNotSingular; ++stack)
                {
                    double relStack = (double)stack / (double)(stackDivision + 1);

                    info.points[new KeyValuePair<int,int>(slice, stack)] = ConePoint(info, relSlice, relStack);
                }
            }

            /*  Bottom parts  */
            if(bottomRadius == 0.0)
            {
                info.bottom = MakePoint(minX, 0.0, 0.0);  /*  apex  */ 
                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    int     stack               =  -stackDivision;  //  second last stack, bottom is -stackDivision - 1
                    double  relSliceCentroid    = ((double)slice + 0.5) / (double)sliceCount;
                    double  relStackCentroid    = -1.0 + (0.5 / (double)(stackDivision + 1));
                    Point   centroid            = ConePoint(info, relSliceCentroid, relStackCentroid);
                    Polygon polygon             = MakePolygon();

                    MakeCorner(info, polygon, slice + 1, stack);
                    MakeCorner(info, polygon, slice,     stack);
                    Corner tip = MakeCorner(info, polygon, slice, -stackDivision - 1);

                    Vector3 n1 = info.pointNormals[GetPoint(info, slice,     stack)];
                    Vector3 n2 = info.pointNormals[GetPoint(info, slice + 1, stack)];
                    Vector3 averageNormal = Vector3.Normalize(n1 + n2);
                    info.cornerNormals[tip] = averageNormal;

                    Vector2 t1 = info.pointTexcoords[GetPoint(info, slice,     stack)];
                    Vector2 t2 = info.pointTexcoords[GetPoint(info, slice + 1, stack)];
                    Vector2 averageTexCoord = (t1 + t2) / 2.0f;
                    info.cornerTexcoords[tip] = averageTexCoord;

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }
            else
            {
                if(useBottom == true)
                {
                    Polygon polygon = MakePolygon();
                    info.polygonCentroids[polygon] = new Vector3((float)minX, 0.0f, 0.0f);
                    info.polygonNormals  [polygon] = new Vector3(-1.0f,        0.0f, 0.0f);

                    for(int slice = 0; slice < sliceCount; ++slice)
                    {
                        int reverseSlice = sliceCount - 1 - slice;

                        MakeCorner(info, polygon, reverseSlice, -stackDivision - 1, true);
                    }
                }
            }

            /*  Middle quads, bottom up  */ 
            for(
                int stack = -stackDivision - info.bottomNotSingular; 
                stack < stackDivision + info.topNotSingular; 
                ++stack
            )
            {
                double relStackCentroid = ((double)stack + 0.5) / (double)(stackDivision + 1);

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    double relSliceCentroid = ((double)(slice) + 0.5) / (double)(sliceCount);

                    Point centroid = ConePoint(info, relSliceCentroid, relStackCentroid);

                    Polygon polygon = MakePolygon();
                    MakeCorner(info, polygon, slice + 1, stack + 1);
                    MakeCorner(info, polygon, slice,     stack + 1);
                    MakeCorner(info, polygon, slice,     stack    );
                    MakeCorner(info, polygon, slice + 1, stack    );

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }

            /*  Top parts  */ 
            if(topRadius == 0.0)
            {
                info.top = MakePoint(maxX, 0.0, 0.0);  /*  apex  */ 

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    int     stack               = stackDivision;
                    double  relSliceCentroid    = ((double)(slice) + 0.5) / (double)(sliceCount);
                    double  relStackCentroid    = 1.0 - (0.5 / (double)(stackDivision + 1));
                    Point   centroid            = ConePoint(info, relSliceCentroid, relStackCentroid);
                    Polygon polygon             = MakePolygon();

                    Corner tip = MakeCorner(info, polygon, slice, stackDivision + 1);
                    MakeCorner(info, polygon, slice, stack);
                    MakeCorner(info, polygon, slice + 1, stack);

                    Vector3 n1 = info.pointNormals[GetPoint(info, slice, stack)];
                    Vector3 n2 = info.pointNormals[GetPoint(info, slice + 1, stack)];
                    Vector3 averageNormal = Vector3.Normalize(n1 + n2);
                    info.cornerNormals[tip] = averageNormal;

                    Vector2 t1 = info.pointTexcoords[GetPoint(info, slice, stack)];
                    Vector2 t2 = info.pointTexcoords[GetPoint(info, slice + 1, stack)];
                    Vector2 averageTexcoord = (t1 + t2) / 2.0f;
                    info.cornerTexcoords[tip] = averageTexcoord;

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }
            else
            {
                if(useTop == true)
                {
                    Polygon polygon = MakePolygon();
                    info.polygonCentroids[polygon] = new Vector3((float)maxX, 0.0f, 0.0f);
                    info.polygonNormals  [polygon] = new Vector3(1.0f, 0.0f, 0.0f);

                    for(int slice = 0; slice < sliceCount; ++slice)
                    {
                        MakeCorner(info, polygon, slice, stackDivision + 1, true);
                    }
                }
            }

            //MergePoints();
        }
    }
}
