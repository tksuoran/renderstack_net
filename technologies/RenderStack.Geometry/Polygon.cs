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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

using RenderStack.Math;

namespace RenderStack.Geometry
{
    [Serializable]
    /// \brief A shape defined by the Points that are connected to it through the Corners.
    /// \note Mostly stable.  */
    public class Polygon : IDisposable
    {
        private List<Corner>                corners = new List<Corner>();

        [NonSerialized]
        private ReadOnlyCollection<Corner>  cornersReadOnly;

        public ReadOnlyCollection<Corner>   Corners { get { return cornersReadOnly; } }

        [OnDeserialized]
        //  After we have deserialized the buffer, store it to GL
        internal void OnDeserialized(StreamingContext context)
        {
            cornersReadOnly = new ReadOnlyCollection<Corner>(corners);
        }

        public void Dispose()
        {
            if(corners.Count != 0)
            {
                throw new Exception("Check");
            }
        }

        public Polygon()
        {
            cornersReadOnly = new ReadOnlyCollection<Corner>(corners);
        }

        public Corner NextCorner(Corner corner)
        {
            for(int i = 0; i < Corners.Count(); ++i)
            {
                Corner corner0 = Corners[i];
                if(corner0 == corner)
                {
                    return Corners[(i + 1) % Corners.Count];
                }
            }
            return null;
        }

        public Corner PreviousCorner(Corner corner)
        {
            for(int i = 0; i < Corners.Count; ++i)
            {
                Corner corner0 = Corners[i];
                if(corner0 == corner)
                {
                    return Corners[(Corners.Count + i - 1) % Corners.Count];
                }
            }
            return null;
        }

        public void CopyToCorners<T>(
            Dictionary<Corner,  T> cornerAttribute,
            Dictionary<Polygon, T> polygonAttribute
        )
        {
            T polygonValue = polygonAttribute[this];
            foreach(Corner corner in Corners)
            {
                cornerAttribute[corner] = polygonValue;
            }
        }
        public void ComputeNormal(
            Dictionary<Polygon, Vector3> polygonNormals,
            Dictionary<Point,   Vector3> pointLocations
        )
        {
            if(Corners.Count > 2)
            {
                Corner c0 = Corners.First();    /*  first   NOTE: Linq extension to enumerable  */
                Corner c1 = Corners[1];         /*  second  */ 
                Corner c2 = Corners.Last();     /*  last    NOTE: Linq extension to enumerable  */
                Point p0 = c0.Point;
                Point p1 = c1.Point;
                Point p2 = c2.Point;

                /*  Make sure all points are unique from others  */ 
                if(
                    (p0 != p1) &&
                    (p0 != p2) &&
                    (p1 != p2)
                )
                {
                    Vector3 pos0   = pointLocations[p0];
                    Vector3 pos1   = pointLocations[p1];
                    Vector3 pos2   = pointLocations[p2];
                    Vector3 normal = Vector3.Cross((pos2 - pos0), (pos1 - pos0));
                    normal = Vector3.Normalize(normal);
                    polygonNormals[this] = normal;
                }
                else
                {
                    throw new System.Exception("polygons with duplicate points");
                }

            }

        }

        public bool DebugCheck(Dictionary<Point, Vector3> pointLocations)
        {
            if(corners.Count < 3)
            {
                return false;
            }

            Vector3 centroid = new Vector3(0.0f, 0.0f, 0.0f);
            {
                int count = 0;
                foreach(Corner corner in Corners)
                {
                    Point   point   = corner.Point;
                    Vector3 pos0    = pointLocations[point];
                    centroid += pos0;
                    ++count;
                }
                centroid /= (float)(count);
            }

            Corner c0 = Corners.First();
            Corner c1 = Corners[1];
            Corner c2 = Corners.Last();
            Point p0 = c0.Point;
            Point p1 = c1.Point;
            Point p2 = c2.Point;

            /*  Make sure all points are unique from others  */ 
            if(
                (p0 != p1) &&
                (p0 != p2) &&
                (p1 != p2)
            )
            {
                Vector3 pos0   = pointLocations[p0];
                Vector3 pos1   = pointLocations[p1];
                Vector3 pos2   = pointLocations[p2];
                Vector3 normal = Vector3.Cross((pos2 - pos0), (pos1 - pos0));
                normal = Vector3.Normalize(normal);
                Vector3 centroidDirection = Vector3.Normalize(centroid);
                float dot = Vector3.Dot(normal, centroidDirection);
                if(dot < 0.0f)
                {
                    return false;
                }
                return dot > 0.0f;
            }
            else
            {
                return false;
            }

        }

        public void ComputeCentroid(
            Dictionary<Polygon, Vector3> polygonCentroids,
            Dictionary<Point,   Vector3> pointLocations
        )
        {
            Vector3 centroid = new Vector3(0.0f, 0.0f, 0.0f);
            int count = 0;
            foreach(Corner corner in Corners)
            {
                Point   point   = corner.Point;
                Vector3 pos0    = pointLocations[point];
                centroid += pos0;
                ++count;
            }
            if(count > 0)
            {
                centroid /= (float)(count);
                polygonCentroids[this] = centroid;
            }
            else
            {
                polygonCentroids[this] = centroid;
            }
        }
        public void SmoothNormalize(
            Dictionary<Corner,  Vector3> cornerAttribute,
            Dictionary<Polygon, Vector3> polygonAttribute,
            Dictionary<Polygon, Vector3> polygonNormals,
            float                        cosMaxSmoothingAngle
        )
        {
            foreach(Corner corner in Corners)
            {
                corner.SmoothNormalize(
                    cornerAttribute,
                    polygonAttribute,
                    polygonNormals,
                    cosMaxSmoothingAngle
                );
            }
        }
        public void SmoothAverage(
            Dictionary<Corner,  Vector4> newCornerAttribute,
            Dictionary<Corner,  Vector4> oldCornerAttribute,
            Dictionary<Corner,  Vector3> cornerNormals,
            Dictionary<Point,   Vector3> pointNormals
        )
        {
            foreach(Corner corner in Corners)
            {
                corner.SmoothAverage(
                    newCornerAttribute,
                    oldCornerAttribute,
                    cornerNormals,
                    pointNormals
                );
            }
        }
        public Corner Corner(Point point)
        {
            foreach(Corner corner in Corners)
            {
                if(corner.Point == point)
                {
                    return corner;
                }
            }
            throw new System.Collections.Generic.KeyNotFoundException();
        }
        public Corner MakeCorner(Point point)
        {
            Corner corner = new Corner(point, this);
            point.Corners.Add(corner);
            corners.Add(corner);
            return corner;
        }
        public void ReplacePoints(Dictionary<Point,Point> oldToNew)
        {
            for(int i = 0; i < corners.Count; ++i)
            {
                var oldCorner = corners[i];
                var oldPoint = oldCorner.Point;
                if(oldToNew.ContainsKey(oldPoint))
                {
                    oldPoint.Corners.Remove(oldCorner);
                    var newPoint = oldToNew[oldPoint];
                    var newCorner = new Corner(newPoint, this);
                    newPoint.Corners.Add(newCorner);
                    corners[i] = newCorner;
                }
            }
        }
        public void RemoveCorner(Corner corner)
        {
            corner.Point.Corners.Remove(corner);
            corners.Remove(corner);
        }
        public void Reverse()
        {
            corners.Reverse();
        }
    }
}
