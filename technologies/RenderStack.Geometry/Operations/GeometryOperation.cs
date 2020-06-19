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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RenderStack.Math;

namespace RenderStack.Geometry
{
    /// \brief Base class for Geometry manipulation operations.
    /// 
    /// GeometryOperation applies some operation to a source Geometry,
    /// building a new geometry. Key support functionality is implemented
    /// in this base class. Mmapping information from source to
    /// destination is maintained here. 
    /// 
    /// New Geometry parts (Points, Corners and Polygons) can be a weighted
    /// sum of source parts.
    /// 
    /// \bug Attribute discontinuities on corner attributes are not properly handled (for example texture seams on sphere).
    /// 
    /// \note Mostly stable, somewhat experimental.
    public class GeometryOperation
    {
        public Geometry                     Source;
        public Geometry                     Destination     = new Geometry();
        public Dictionary<Point, Point>     pointOldToNew   = new Dictionary<Point,Point>();
        public Dictionary<Polygon, Polygon> polygonOldToNew = new Dictionary<Polygon,Polygon>();
        public Dictionary<Corner, Corner>   cornerOldToNew  = new Dictionary<Corner,Corner>();
        public Dictionary<Edge, Edge>       edgeOldToNew    = new Dictionary<Edge,Edge>();
        public Dictionary<Polygon, Point>   oldPolygonCentroidToNewPoints = new Dictionary<Polygon,Point>();

        public Dictionary<Point,    List<KeyValuePair<float, Point>>>   newPointSources         = new Dictionary<Point,List<KeyValuePair<float,Point>>>();
        public Dictionary<Point,    List<KeyValuePair<float, Corner>>>  newPointCornerSources   = new Dictionary<Point,List<KeyValuePair<float,Corner>>>();
        public Dictionary<Corner,   List<KeyValuePair<float, Corner>>>  newCornerSources        = new Dictionary<Corner,List<KeyValuePair<float,Corner>>>();
        public Dictionary<Polygon,  List<KeyValuePair<float, Polygon>>> newPolygonSources       = new Dictionary<Polygon,List<KeyValuePair<float,Polygon>>>();
        public Dictionary<Edge,     List<KeyValuePair<float, Edge>>>    newEdgeSources          = new Dictionary<Edge,List<KeyValuePair<float,Edge>>>();

        /// Creates a new Point to Destination from old Point.
        /// The new Point is linked to the old point in Source.
        /// Old point is set as source for the new Point with specified weight.
        /// 
        /// \param weight   Weight for old point as source</param>
        /// \param oldPoint Old point used as source for the new point</param>
        /// <returns>The new Point.</returns>
        public Point    MakeNewPointFromPoint(float weight, Point oldPoint)
        {
            Debug.Assert(oldPoint != null);

            Point newPoint = Destination.MakePoint();
            AddPointSource(newPoint, weight, oldPoint);
            pointOldToNew[oldPoint] = newPoint;
            return newPoint;
        }
        /// <summary>
        /// Creates a new Point to Destination from old Point.
        /// The new Point is linked to the old point in Source.
        /// Old point is set as source for the new Point with weight 1.0.
        /// </summary>
        /// <param name="oldPoint">Old point used as source for the new point</param>
        /// <returns>The new Point.</returns>
        public Point    MakeNewPointFromPoint(Point oldPoint)
        {
            Debug.Assert(oldPoint != null);

            Point newPoint = Destination.MakePoint();
            AddPointSource(newPoint, 1.0f, oldPoint);
            pointOldToNew[oldPoint] = newPoint;
            return newPoint;
        }
        /// <summary>
        /// Creates a new point to Destination from centroid of old Polygon.
        /// The new Point is linked to the old Polygon in Source.
        /// Each corner of the old Polygon is added as source for the new Point with weight 1.0.
        /// </summary>
        /// <param name="oldPolygon"></param>
        /// <returns></returns>
        public Point    MakeNewPointFromPolygonCentroid(Polygon oldPolygon)
        {
            Debug.Assert(oldPolygon != null);

            Point newPoint = Destination.MakePoint();
            oldPolygonCentroidToNewPoints[oldPolygon] = newPoint;
            AddPolygonCentroid(newPoint, 1.0f, oldPolygon);
            return newPoint;
        }
        public void     AddPolygonCentroid(Point newPoint, float weight, Polygon oldPolygon)
        {
            foreach(Corner oldCorner in oldPolygon.Corners)
            {
                AddPointSource(newPoint, weight, oldCorner);
                AddPointSource(newPoint, weight, oldCorner.Point);
            }
        }
        public void     AddPointRing(Point newPoint, float weight, Point oldPoint)
        {
            Debug.Assert(newPoint != null);
            Debug.Assert(oldPoint != null);

            foreach(Corner ringCorner in oldPoint.Corners)
            {
                Polygon ringPolygon     = ringCorner.Polygon;
                Corner  nextRingCorner  = ringPolygon.NextCorner(ringCorner);
                Point   nextRingPoint   = nextRingCorner.Point;
                AddPointSource(newPoint, weight, nextRingPoint);
            }
        }
        public Polygon  MakeNewPolygonFromPolygon(Polygon oldPolygon)
        {
            Debug.Assert(oldPolygon != null);

            Polygon newPolygon = Destination.MakePolygon();

            AddPolygonSource(newPolygon, 1.0f, oldPolygon);
            polygonOldToNew[oldPolygon] = newPolygon;

            return newPolygon;
        }
        public Corner   MakeNewCornerFromPolygonCentroid(Polygon newPolygon, Polygon oldPolygon)
        {
            Debug.Assert(newPolygon != null);
            Debug.Assert(oldPolygon != null);

            Point   newPoint    = oldPolygonCentroidToNewPoints[oldPolygon];
            Corner  newCorner   = newPolygon.MakeCorner(newPoint);
            DistributeCornerSources(newCorner, 1.0f, newPoint);
            return newCorner;
        }
        public Corner   MakeNewCornerFromCorner(Polygon newPolygon, Corner oldCorner)
        {
            Debug.Assert(newPolygon != null);
            Debug.Assert(oldCorner  != null);

            Point   oldPoint    = oldCorner.Point;
            Point   newPoint    = pointOldToNew[oldPoint];
            Corner  newCorner   = newPolygon.MakeCorner(newPoint);
            AddCornerSource(newCorner, 1.0f, oldCorner);
            return newCorner;
        }
        public void     AddPolygonCorners(Polygon newPolygon, Polygon oldPolygon)
        {
            Debug.Assert(newPolygon != null);
            Debug.Assert(oldPolygon != null);

            foreach(Corner oldCorner in oldPolygon.Corners)
            {
                Point   oldPoint    = oldCorner.Point;
                Point   newPoint    = pointOldToNew[oldPoint];
                Corner  newCorner   = newPolygon.MakeCorner(newPoint);
                AddCornerSource(newCorner, 1.0f, oldCorner);
            }
        }
        public void     AddPointSource(Point newPoint, float weight, Point oldPoint)
        {
            Debug.Assert(oldPoint != null);
            Debug.Assert(oldPoint != null);

            if(newPointSources.ContainsKey(newPoint) == false)
            {
                newPointSources[newPoint] = new List<KeyValuePair<float,Point>>();
            }
            newPointSources[newPoint].Add(new KeyValuePair<float,Point>(weight, oldPoint));
        }
        public void     AddPointSource(Point newPoint, float weight, Corner oldCorner)
        {
            Debug.Assert(newPoint != null);
            Debug.Assert(oldCorner != null);

            if(newPointCornerSources.ContainsKey(newPoint) == false)
            {
                newPointCornerSources[newPoint] = new List<KeyValuePair<float,Corner>>();
            }
            newPointCornerSources[newPoint].Add(new KeyValuePair<float,Corner>(weight, oldCorner));
        }
        public void     AddCornerSource(Corner newCorner, float weight, Corner oldCorner)
        {
            Debug.Assert(newCorner != null);
            Debug.Assert(oldCorner != null);

            if(newCornerSources.ContainsKey(newCorner) == false)
            {
                newCornerSources[newCorner] = new List<KeyValuePair<float,Corner>>();
            }
            newCornerSources[newCorner].Add(new KeyValuePair<float,Corner>(weight, oldCorner));
        }
        public void     DistributeCornerSources(Corner newCorner, float weight, Point newPoint)
        {
            Debug.Assert(newCorner != null);
            Debug.Assert(newPoint  != null);

            foreach(var kvp in newPointCornerSources[newPoint])
            {
                AddCornerSource(newCorner, weight * kvp.Key, kvp.Value);
            }
        }
        public void     AddPolygonSource(Polygon newPolygon, float weight, Polygon oldPolygon)
        {
            Debug.Assert(newPolygon != null);
            Debug.Assert(oldPolygon != null);

            if(newPolygonSources.ContainsKey(newPolygon) == false)
            {
                newPolygonSources[newPolygon] = new List<KeyValuePair<float,Polygon>>();
            }
            newPolygonSources[newPolygon].Add(new KeyValuePair<float,Polygon>(weight, oldPolygon));
        }
        public void     AddEdgeSource(Edge newEdge, float weight, Edge oldEdge)
        {
            Debug.Assert(newEdge.A != null);
            Debug.Assert(newEdge.B != null);
            Debug.Assert(oldEdge.A != null);
            Debug.Assert(oldEdge.B != null);

            if(newEdgeSources.ContainsKey(newEdge) == false)
            {
                newEdgeSources[newEdge] = new List<KeyValuePair<float,Edge>>();
            }
            newEdgeSources[newEdge].Add(new KeyValuePair<float,Edge>(weight, oldEdge));
        }

        protected static void InterpolateAttributeMaps<KeyType>(
            AttributeMapCollection<KeyType>                         original,
            AttributeMapCollection<KeyType>                         clone,
            Dictionary<KeyType,List<KeyValuePair<float,KeyType>>>   keyNewToOlds
        )
        {
            /*  For all original attribute maps  */ 
            foreach(KeyValuePair<string, object> kvp in original.AttributeMaps)
            {
                string  mapName         = kvp.Key;
                object  oldMap          = kvp.Value;    // real type is Dictionary<KeyType, ValueType>
                object  newMap;
                Type    mapValueType    = oldMap.GetType().GetGenericArguments()[1];

                if(mapName == "corner_indices")
                {
                    continue;
                }
                Type    mapKeyType      = oldMap.GetType().GetGenericArguments()[0];
#if false
                Debug.WriteLine("Attribute map name       : " + mapName);
                Debug.WriteLine("Attribute map key type   : " + mapKeyType.ToString());
                Debug.WriteLine("Attribute map value type : " + mapValueType.ToString());
#endif
                if(clone.AttributeMaps.ContainsKey(mapName) == true)
                {
                    //Debug.WriteLine("!");
                }
                newMap = System.Activator.CreateInstance(oldMap.GetType());
                clone.AttributeMaps[mapName] = newMap;

                IDictionary oldMapDictionary = (IDictionary)oldMap;
                IDictionary newMapDictionary = (IDictionary)newMap;

#if false
                Debug.WriteLine("Attribute map count      : " + oldMapDictionary.Count);
#endif

                /*  For each new object in keyNewToOlds.Keys  */ 
                foreach(var kvp2 in keyNewToOlds)
                {
                    KeyType newKey = kvp2.Key;
                    List<KeyValuePair<float,KeyType>> oldKeys = kvp2.Value;

                    //  Compute sum of weights, for normalizing
                    float sumWeights = 0.0f;
                    foreach(var kvp3 in oldKeys)
                    {
                        KeyType oldKey = kvp3.Value;
                        if(oldMapDictionary.Contains(oldKey))
                        {
                            sumWeights += kvp3.Key;
                        }
                    }
                    if(sumWeights == 0.0f)
                    {
                        continue;
                    }

                    object newValue = System.Activator.CreateInstance(mapValueType);
                    if(newValue is ILinear)
                    {
                        ILinear newLinearValue  = (ILinear)newValue;
                        foreach(var kvp3 in oldKeys)
                        {
                            float   weight  = kvp3.Key;
                            KeyType oldKey  = kvp3.Value;
                            object  oldValue;
                            if(oldMapDictionary.Contains(oldKey))
                            {
                                oldValue        = oldMapDictionary[oldKey];
                                //  TODO MUSTIFIX oldValue = null 
                                ILinear oldLinearValue  = (ILinear)oldValue;
                                newLinearValue.PlusWeightTimesOther(weight / sumWeights, oldLinearValue);
                            }
                            else
                            {
                                oldValue = null;
                            }
                        }
                        newMapDictionary[newKey] = newLinearValue;
                    }
                    else if(newValue is float)
                    {
                        float newFloatValue  = (float)newValue;
                        foreach(var kvp3 in oldKeys)
                        {
                            float   weight          = kvp3.Key;
                            KeyType oldKey          = kvp3.Value;
                            if(oldMapDictionary.Contains(oldKey))
                            {
                                object  oldValue        = oldMapDictionary[oldKey];
                                float   oldFloatValue   = (float)oldValue;
                                newFloatValue += (weight / sumWeights) * oldFloatValue;
                            }
                            else
                            {
                                //Debug.WriteLine("!");
                            }
                        }
                        newMapDictionary[newKey] = newFloatValue;
                    }
                    else
                    {
                        //Debug.WriteLine("!");
                    }
                }
            }
        }

        public void BuildDestinationEdgesWithSourcing()
        {
            Destination.BuildEdges();
            foreach(Edge oldEdge in Source.Edges.Keys)
            {
                Edge newEdge = new Edge(pointOldToNew[oldEdge.A], pointOldToNew[oldEdge.B]);
                AddEdgeSource(newEdge, 1.0f, oldEdge);
                edgeOldToNew[oldEdge] = new Edge(
                    pointOldToNew[oldEdge.A],
                    pointOldToNew[oldEdge.B]
                );
            }

        }
        public void InterpolateAllAttributeMaps()
        {
            InterpolateAttributeMaps<Point>   (Source.PointAttributes,   Destination.PointAttributes,    newPointSources);
            InterpolateAttributeMaps<Polygon> (Source.PolygonAttributes, Destination.PolygonAttributes,  newPolygonSources);
            InterpolateAttributeMaps<Corner>  (Source.CornerAttributes,  Destination.CornerAttributes,   newCornerSources);
            InterpolateAttributeMaps<Edge>    (Source.EdgeAttributes,    Destination.EdgeAttributes,     newEdgeSources);
        }
    };
}
