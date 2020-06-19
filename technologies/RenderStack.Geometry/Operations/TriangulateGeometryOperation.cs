//#define DEBUG_CHECK

namespace RenderStack.Geometry
{
    /*  Comment: Mostly stable.  */
    public class TriangulateGeometryOperation : GeometryOperation
    {
        public TriangulateGeometryOperation(Geometry src)
        {
            Source = src;

            foreach(Point oldPoint in Source.Points)
            {
                MakeNewPointFromPoint(oldPoint);
            }

            foreach(Polygon oldPolygon in Source.Polygons)
            {
                MakeNewPointFromPolygonCentroid(oldPolygon);
            }

            for(uint polygonIndex = 0; polygonIndex < Source.Polygons.Count; ++polygonIndex)
            {
                Polygon oldPolygon = Source.Polygons[(int)polygonIndex];

                if(oldPolygon.Corners.Count == 3)
                {
                    Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);
                    AddPolygonCorners(newPolygon, oldPolygon);
                    continue;
                }

                for(int i = 0; i < oldPolygon.Corners.Count; ++i)
                {
                    Corner  oldCorner   = oldPolygon.Corners[i];
                    Corner  nextCorner  = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];
                    Polygon newPolygon  = Destination.MakePolygon();

                    MakeNewCornerFromPolygonCentroid(newPolygon, oldPolygon);
                    MakeNewCornerFromCorner         (newPolygon, oldCorner);
                    MakeNewCornerFromCorner         (newPolygon, nextCorner);
                }
            }

            BuildDestinationEdgesWithSourcing();
            InterpolateAllAttributeMaps();
        }
    }
}
