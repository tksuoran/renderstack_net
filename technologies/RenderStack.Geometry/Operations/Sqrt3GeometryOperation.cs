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

//#define DEBUG_CHECK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RenderStack.Math;

namespace RenderStack.Geometry
{
    public class Sqrt3GeometryOperation : GeometryOperation
    {
        //  Sqrt(3): Replace edges with two triangles               
        //  For each corner in the old polygon, add one triangle    
        //  (centroid, corner, opposite centroid)                   
        //                                                          
        //  Centroids:                                              
        //                                                          
        //  (1) q := 1/3 (p_i + p_j + p_k)                          
        //                                                          
        //  Refine old vertices:                                    
        //                                                          
        //  (2) S(p) := (1 - alpha_n) p + alpha_n 1/n SUM p_i       
        //                                                          
        //  (6) alpha_n = (4 - 2 cos(2Pi/n)) / 9                    
        /// \brief Sqrt(3) subdivision operation.
        /// \note Mostly stable.
        public Sqrt3GeometryOperation(Geometry src)
        {
            Source = src;

            //  Make refined copies of old points
            foreach(Point oldPoint in src.Points)
            {
                float alpha             = (float)(4.0 - 2.0 * System.Math.Cos(2.0 * System.Math.PI / oldPoint.Corners.Count)) / 9.0f;
                float alphaPerN         = alpha / (float)oldPoint.Corners.Count;
                float alphaComplement   = 1.0f - alpha;

                Point newPoint = MakeNewPointFromPoint(alphaComplement, oldPoint);
                AddPointRing(newPoint, alphaPerN, oldPoint);
            }

            foreach(Polygon oldPolygon in src.Polygons)
            {
                MakeNewPointFromPolygonCentroid(oldPolygon);
            }

            for(uint polygonIndex = 0; polygonIndex < src.Polygons.Count; ++polygonIndex)
            {
                Polygon oldPolygon = Source.Polygons[(int)polygonIndex];

                for(int i = 0; i < oldPolygon.Corners.Count; ++i)
                {
                    Corner  oldCorner           = oldPolygon.Corners[i];
                    Corner  nextCorner          = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];
                    Edge    edge                = new Edge(oldCorner.Point, nextCorner.Point);
                    HashSet<Polygon> edgePolys  = src.Edges[edge];
                    Polygon oppositePolygon = edgePolys.Where(p => (p != oldPolygon)).FirstOrDefault();
                    if(oppositePolygon == null)
                    {
                        continue;
                    }

                    Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);
                    MakeNewCornerFromPolygonCentroid(newPolygon, oldPolygon);
                    MakeNewCornerFromCorner         (newPolygon, oldCorner);
                    MakeNewCornerFromPolygonCentroid(newPolygon, oppositePolygon);
                }
            }

            BuildDestinationEdgesWithSourcing();
            InterpolateAllAttributeMaps();
        }
    }
}
