//#define DEBUG_CHECK

using System.Collections.Generic;

namespace RenderStack.Geometry
{
    /// \brief Performs Catmull-Clark subdivision operation to Geometry
    /// \bug Attribute discontinuities on corner attributes are not properly handled (for example texture seams on sphere).
    /// \note Mostly stable.
    public class CatmullClarkGeometryOperation : GeometryOperation
    {
        public Dictionary<Edge, Point> oldEdgeToNewEdgePoints = new Dictionary<Edge,Point>();
        public Point MakeNewPointFromEdge(Edge oldEdge)
        {
            Point newPoint = Destination.MakePoint();
            oldEdgeToNewEdgePoints[oldEdge] = newPoint;
            AddPointSource(newPoint, 1.0f, oldEdge.A);
            AddPointSource(newPoint, 1.0f, oldEdge.B);
            return newPoint;
        }
        public Corner MakeNewCornerFromEdgePoint(Polygon newPolygon, Edge oldEdge)
        {
            Point   edgeMidpoint    = oldEdgeToNewEdgePoints[oldEdge];
            Corner  newCorner       = newPolygon.MakeCorner(edgeMidpoint);
            DistributeCornerSources(newCorner, 1.0f, edgeMidpoint);
            return newCorner;
        }

        //  E = average of two neighboring face points and original endpoints
        //
        //  Compute P'             F = average F of all n face points for faces touching P
        //                         R = average R of all n edge midpoints for edges touching P
        //       F + 2R + (n-3)P   P = old point location
        //  P' = ----------------  
        //             n           -> F weight is     1/n
        //                         -> R weight is     2/n
        //       F   2R   (n-3)P   -> P weight is (n-3)/n
        //  P' = - + -- + ------   
        //       n    n      n     
        //                         
        //  For each corner in the old polygon, add one quad
        //  (centroid, previous edge 'edge point', corner, next edge 'edge midpoint')
        public CatmullClarkGeometryOperation(Geometry src)
        {
            Source = src;

            //                        (n-3)P  
            //  Make initial P's with ------  
            //                           n    
            foreach(Point oldPoint in Source.Points)
            {
                float n         = (float)oldPoint.Corners.Count;
                float weight    = (n - 3.0f) / n;
                MakeNewPointFromPoint(weight, oldPoint);
            }

            //  Make edge points
            //  "average of two neighboring face points and original endpoints"
            //  Add midpoint (R) to each new end points 
            //    R = average R of all n edge midpoints for edges touching P
            //   2R  we add both edge end points with weight 1 so total edge weight is 2
            //   --
            //    n
            foreach(var kvp in Source.Edges)
            {
                Edge    oldEdge     = kvp.Key;
                Point   newPoint    = MakeNewPointFromEdge(oldEdge);    //  these get weights 1 + 1
                foreach(Polygon oldPolygon in kvp.Value)
                {
                    float weight = 1.0f / (float)oldPolygon.Corners.Count;
                    AddPolygonCentroid(newPoint, weight, oldPolygon);
                }
                Point newPointA = pointOldToNew[oldEdge.A];
                Point newPointB = pointOldToNew[oldEdge.B];
                float nA        = (float)(oldEdge.A.Corners.Count);
                float nB        = (float)(oldEdge.B.Corners.Count);
                float weightA   = 1.0f / nA;
                float weightB   = 1.0f / nB;
                AddPointSource(newPointA, weightA, oldEdge.A);
                AddPointSource(newPointA, weightA, oldEdge.B);
                AddPointSource(newPointB, weightB, oldEdge.A);
                AddPointSource(newPointB, weightB, oldEdge.B);
            }

            foreach(Polygon oldPolygon in Source.Polygons)
            {
                //  Make centroid point
                MakeNewPointFromPolygonCentroid(oldPolygon);

                //  Add polygon centroids (F) to all corners' point sources
                //  F = average F of all n face points for faces touching P
                //   F    <- because F is average of all centroids, it adds extra /n
                //  ---
                //   n
                foreach(Corner oldCorner in oldPolygon.Corners)
                {
                    Point   oldPoint        = oldCorner.Point;
                    Point   newPoint        = pointOldToNew[oldPoint];
                    float   pointWeight     = 1.0f / (float)(oldPoint.Corners.Count);
                    float   cornerWeight    = 1.0f / (float)(oldPolygon.Corners.Count);
                    AddPolygonCentroid(newPoint, pointWeight * pointWeight * cornerWeight, oldPolygon);
                }
            }

            //  Subdivide polygons, clone (and corners);
            for(uint polygonIndex = 0; polygonIndex < src.Polygons.Count; ++polygonIndex)
            {
                Polygon oldPolygon  = Source.Polygons[(int)polygonIndex];

                for(int i = 0; i < oldPolygon.Corners.Count; ++i) // Corner oldCorner in oldPolygon.Corners)
                {
                    Corner  oldCorner       = oldPolygon.Corners[i];
                    Corner  previousCorner  = oldPolygon.Corners[(oldPolygon.Corners.Count + i - 1) % oldPolygon.Corners.Count];
                    Corner  nextCorner      = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];
                    Edge    previousEdge    = new Edge(previousCorner.Point, oldCorner.Point);
                    Edge    nextEdge        = new Edge(oldCorner.Point, nextCorner.Point);
                    Polygon newPolygon      = MakeNewPolygonFromPolygon(oldPolygon);
                    MakeNewCornerFromPolygonCentroid(newPolygon, oldPolygon);
                    MakeNewCornerFromEdgePoint      (newPolygon, previousEdge);
                    MakeNewCornerFromCorner         (newPolygon, oldCorner);
                    MakeNewCornerFromEdgePoint      (newPolygon, nextEdge);
                }
            }

            Destination.BuildEdges();
            //  TODO can we map some edges to old edges? yes. Do it.
            InterpolateAllAttributeMaps();
        }
    }
}
