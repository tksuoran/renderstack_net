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
    /*  Comment: Experimental, some known issues.  */
    public class TruncateGeometryOperation : GeometryOperation
    {
        //  the edge length of a regular n-gon is 2a*tan(pi/n) where a is the apothem
        //  the apothem does not change when you truncate
        //  a = s / (2*tan(Pi/n))
        //  s = 2*a*tan(Pi/n)
        //  
        //  Make new points from every old corner
        //  (Not in Open version of truncate) Make new polygons from every old point
        public Dictionary<Edge, List<Edge>>     oldEdgeToNews       = new Dictionary<Edge,List<Edge>>();
        public Dictionary<Point, List<Edge>>    oldPointToOpenEdges = new Dictionary<Point,List<Edge>>();
        public Dictionary<Point, Point>         newPointToOld       = new Dictionary<Point,Point>();

        public void UpdateTruncatedEdge(Polygon newPolygon, Corner oldCorner1, Corner oldCorner2, float t)
        {
            Point   oldPoint    = oldCorner1.Point;
            Point   nextPoint   = oldCorner2.Point;
            Edge    oldEdge     = new Edge(oldPoint, nextPoint);

            Point   newPoint2;
            Point   newPoint3;

            if(oldEdgeToNews.ContainsKey(oldEdge) == false)
            {
                oldEdgeToNews[oldEdge] = new List<Edge>();

                newPoint2           = Destination.MakePoint();
                newPoint3           = Destination.MakePoint();
                AddPointSource(newPoint2, 1.0f - t, oldPoint);
                AddPointSource(newPoint2, t,        nextPoint);
                AddPointSource(newPoint3, t,        oldPoint);
                AddPointSource(newPoint3, 1.0f - t, nextPoint);
                newPointToOld[newPoint2] = oldPoint;
                newPointToOld[newPoint3] = nextPoint;
                //  This is new the 'middle' edge, the edges that remain touching.
                //  For the other two 'open' edges, see the second pass.
                Edge newEdge = new Edge(newPoint2, newPoint3);
                oldEdgeToNews[oldEdge].Add(newEdge);
                AddEdgeSource(newEdge, 1.0f, oldEdge);
            }
            else
            {
                newPoint2 = oldEdgeToNews[oldEdge].First().B;
                newPoint3 = oldEdgeToNews[oldEdge].First().A;
            }

            newPolygon.MakeCorner(newPoint2);
            newPolygon.MakeCorner(newPoint3);
            AddPointSource(newPoint2, 1.0f - t, oldCorner1);
            AddPointSource(newPoint2, t,        oldCorner2);
            AddPointSource(newPoint3, t,        oldCorner1);
            AddPointSource(newPoint3, 1.0f - t, oldCorner2);
        }
        public TruncateGeometryOperation(Geometry src, bool close)
        {
            Source = src;

            //  Pass 1: Create truncated versions of old polygons
            foreach(Polygon oldPolygon in Source.Polygons)
            {
                Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);

                double  n1      = (double)(oldPolygon.Corners.Count);
                double  n2      = (double)(2 * oldPolygon.Corners.Count);
                float   ratio   = (float)(System.Math.Tan(System.Math.PI / n2) / System.Math.Tan(System.Math.PI / n1));
                float   t       = 0.5f - 0.5f * ratio;

                //  Pass 1A: Create truncated corners
                for(int i = 0; i < oldPolygon.Corners.Count; ++i)
                {
                    Corner  oldCorner   = oldPolygon.Corners[i];
                    Corner  nextCorner  = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];

                    UpdateTruncatedEdge(newPolygon, oldCorner, nextCorner, t);
                }

                //  Pass 1B: Collect open edges from new polygon to per point dictionary
                //  Note the order of points in new edge is important in this case.
                for(int i = 0; i < newPolygon.Corners.Count; i += 2)
                {
                    Corner  newCorner       = newPolygon.Corners[i];
                    Corner  previousCorner  = newPolygon.Corners[(i + newPolygon.Corners.Count - 1) % newPolygon.Corners.Count];
                    Point   newPoint        = newCorner.Point;
                    Point   previousPoint   = previousCorner.Point;
                    Point   oldPoint        = newPointToOld[newPoint];
                    Edge    newEdge         = new Edge(newPoint, previousPoint);

                    if(oldPointToOpenEdges.ContainsKey(oldPoint) == false)
                    {
                        oldPointToOpenEdges[oldPoint] = new List<Edge>();
                    }
                    oldPointToOpenEdges[oldPoint].Add(newEdge);
                }
            }

            //  Pass 2: Connect open edges
            if(close)
            {
                foreach(Point oldPoint in src.Points)
                {
                    if(oldPointToOpenEdges.ContainsKey(oldPoint) == false)
                    {
                        continue;
                    }
                    if(oldPointToOpenEdges[oldPoint].Count < 3)
                    {
                        continue;
                    }
                    Polygon newPolygon  = Destination.MakePolygon();
                    //  TODO This polygon has no sources.. inherit average of surrounding polygons?

                    Edge    edge        = oldPointToOpenEdges[oldPoint].First();
                    Edge    startEdge   = edge;
                    //  TODO MUSTFIX    This loop does not terminate for some geometries
                    //                  such as tetrahemihexahedron
                    do
                    {
                        bool nextEdgeFound = false;
                        int edgeIndex = 0;
                        foreach(Edge nextEdge in oldPointToOpenEdges[oldPoint])
                        {
                            ++edgeIndex;
                            if(nextEdge.A == edge.B)
                            {
                                Point   newPoint = edge.A;
                                Corner  newCorner = newPolygon.MakeCorner(newPoint);
                                DistributeCornerSources(newCorner, 1.0f, newPoint);
                                edge = nextEdge;
                                nextEdgeFound = true;
                                break;
                            }
                        }
                        /*  This can happen  */ 
                        if(nextEdgeFound == false)
                        {
                            break;
                        }
                    }
                    while(edge.Equals(startEdge) == false);

                    if(newPolygon.Corners.Count < 3)
                    {
                        Destination.RemovePolygon(newPolygon);
                    }
                }
            }

            Destination.BuildEdges();
            InterpolateAllAttributeMaps();
        }
    }
}
