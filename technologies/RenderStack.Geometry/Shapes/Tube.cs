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
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Tube : Geometry
    {
        public Tube(
            IParametricCurve    curve,
            float               radius,
            int                 sliceCount,
            int                 stackCount
        )
        {
            float tStep = 1.0f / 512.0f;

            //  Compute initial N
            Vector3 pos         = curve.PositionAt(0.0f);
            Vector3 posNext     = curve.PositionAt(0.0f + tStep);
            Vector3 posNext2    = curve.PositionAt(0.0f + tStep + tStep);
            Vector3 d1          = posNext - pos;
            Vector3 d2          = (posNext2 - posNext) - d1;
            Vector3 T           = Vector3.Normalize(d1);
            Vector3 N           = Vector3.Normalize(d2);
            Vector3 B           = Vector3.Normalize(Vector3.Cross(T, N));
            N                   = Vector3.Normalize(Vector3.Cross(B, T));

            var pointLocations      = PointAttributes.FindOrCreate<Vector3>("point_locations");
            var pointNormals        = PointAttributes.FindOrCreate<Vector3>("point_normals");
            var pointTexCoords      = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
            var cornerNormals       = CornerAttributes.FindOrCreate<Vector3>("corner_normals");
            //var cornerTexCoords     = CornerAttributes.FindOrCreate<Vector2>("corner_texcoords");

            /*  Other vertices  */ 
            List<Point> points = new List<Point>();

            for(int stack = 0; stack <= stackCount; ++stack)
            {
                float t = (float)stack / (float)stackCount;

                pos     = curve.PositionAt(t);
                posNext = curve.PositionAt(t + tStep);
                d1      = posNext - pos;
                T       = Vector3.Normalize(d1);
                B       = Vector3.Normalize(Vector3.Cross(T, N));
                N       = Vector3.Normalize(Vector3.Cross(B, T));

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    float relPhi    = (float)slice / (float)sliceCount;
                    float phi       = (float)System.Math.PI * 2.0f * relPhi;
                    float sinPhi    = (float)System.Math.Sin(phi);
                    float cosPhi    = (float)System.Math.Cos(phi);

                    Vector3 v = pos;
                    v += N * sinPhi * radius;
                    v += B * cosPhi * radius;

                    Point point = MakePoint();

                    pointLocations[point] = v;
                    pointNormals  [point] = N * sinPhi + B * cosPhi;
                    pointTexCoords[point] = new Vector2(relPhi, t);

                    points.Add(point);
                }
            }

            /*  Bottom parts  */
            {
                Vector3 bottomPosition      = curve.PositionAt(0.0f);
                Vector3 bottomPositionNext  = curve.PositionAt(tStep);
                Vector3 bottomTangent       = Vector3.Normalize(bottomPosition - bottomPositionNext);
                Polygon polygon = MakePolygon();

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    int stack        = 0;
                    int reverseSlice = sliceCount - 1 - slice;

                    Corner corner = polygon.MakeCorner(
                        points[(stack * (sliceCount)) + reverseSlice]
                    );

                    cornerNormals[corner] = bottomTangent;
                }
            }

            /*  Middle quads, t = 0 ... t = 1  */ 
            for(int stack = 0; stack < stackCount; ++stack)
            {
                int nextStack = stack + 1;

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    int nextSlice = (slice + 1) % sliceCount;

                    MakePolygon(
                        points[(nextStack * (sliceCount)) + nextSlice],
                        points[(nextStack * (sliceCount)) + slice],
                        points[(stack     * (sliceCount)) + slice],
                        points[(stack     * (sliceCount)) + nextSlice]
                    );
                }
            }

            /*  Top parts  */ 
            {
                Vector3 topPosition      = curve.PositionAt(1.0f);
                Vector3 topPositionPrev  = curve.PositionAt(1.0f - tStep);
                Vector3 topTangent       = Vector3.Normalize(topPosition - topPositionPrev);
                Polygon polygon = MakePolygon();

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    int stack = stackCount;

                    Corner corner = polygon.MakeCorner(
                        points[(stack * (sliceCount)) + slice]
                    );

                    cornerNormals[corner] = topTangent;
                }
            }
        }
    }
}
