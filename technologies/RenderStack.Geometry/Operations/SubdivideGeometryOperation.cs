//#define DEBUG_CHECK

using System.Collections.Generic;

namespace RenderStack.Geometry
{
    /*  Comment: Mostly stable.  */
    public class SubdivideGeometryOperation : GeometryOperation
    {
        //  Add midpoints to edges and connect to polygon center
        //  For each corner in the old polygon, add one quad
        //  (centroid, previous edge midpoint, corner, next edge midpoint)
        public Dictionary<Edge, Point> oldEdgeToNewMidpoints = new Dictionary<Edge,Point>();

        public void UpdateNewEdgeFromCorners(Corner oldCorner1, Corner oldCorner2)
        {
            Point   point1  = oldCorner1.Point;
            Point   point2  = oldCorner2.Point;

            Edge    edge    = new Edge(point1, point2);
            Point   newMidPoint;
            if(oldEdgeToNewMidpoints.ContainsKey(edge) == false)
            {
                newMidPoint = Destination.MakePoint();
                oldEdgeToNewMidpoints[edge] = newMidPoint; 
            }
            else
            {
                newMidPoint = oldEdgeToNewMidpoints[edge];
            }
            AddPointSource(newMidPoint, 0.5f, point1);
            AddPointSource(newMidPoint, 0.5f, point2);
            AddPointSource(newMidPoint, 0.5f, oldCorner1);
            AddPointSource(newMidPoint, 0.5f, oldCorner2);
        }
        public Corner MakeNewCornerFromEdgeMidpoint(Polygon newPolygon, Edge oldEdge)
        {
            Point   edgeMidpoint    = oldEdgeToNewMidpoints[oldEdge];
            Corner  newCorner       = newPolygon.MakeCorner(edgeMidpoint);
            DistributeCornerSources(newCorner, 1.0f, edgeMidpoint);
            return newCorner;
        }

        public SubdivideGeometryOperation(Geometry src)
        {
            Source = src;

            foreach(Point oldPoint in Source.Points)
            {
                MakeNewPointFromPoint(oldPoint);
            }

            foreach(Polygon oldPolygon in Source.Polygons)
            {
                MakeNewPointFromPolygonCentroid(oldPolygon);
                for(int i = 0; i < oldPolygon.Corners.Count; ++i)
                {
                    Corner  corner1 = oldPolygon.Corners[i];
                    Corner  corner2 = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];
                    UpdateNewEdgeFromCorners(corner1, corner2);
                }
            }

            for(uint polygonIndex = 0; polygonIndex < src.Polygons.Count; ++polygonIndex)
            {
                Polygon oldPolygon  = Source.Polygons[(int)polygonIndex];

                for(int i = 0; i < oldPolygon.Corners.Count; ++i)
                {
                    Corner  oldCorner       = oldPolygon.Corners[i];
                    Corner  previousCorner  = oldPolygon.Corners[(oldPolygon.Corners.Count + i - 1) % oldPolygon.Corners.Count];
                    Corner  nextCorner      = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];
                    Edge    previousEdge    = new Edge(previousCorner.Point,    oldCorner.Point);
                    Edge    nextEdge        = new Edge(oldCorner.Point,         nextCorner.Point);

                    Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);
                    MakeNewCornerFromPolygonCentroid(newPolygon, oldPolygon);
                    MakeNewCornerFromEdgeMidpoint   (newPolygon, previousEdge);
                    MakeNewCornerFromCorner         (newPolygon, oldCorner);
                    MakeNewCornerFromEdgeMidpoint   (newPolygon, nextEdge);

                }
            }

            BuildDestinationEdgesWithSourcing();
            InterpolateAllAttributeMaps();
        }
    }
}
