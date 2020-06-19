//#define DEBUG_CHECK

using System.Collections.Generic;

namespace RenderStack.Geometry
{
    /// \brief Creates a clone of Geometry. Copies all Polygons, Points, Corners, and copies their (and edges) attributes.
    /// \note Mostly stable
    public class CloneGeometryOperation : GeometryOperation
    {
        public CloneGeometryOperation(Geometry src, HashSet<uint> selectedPolygonIndices)
        {
            Source = src;

            foreach(Point oldPoint in src.Points)
            {
                MakeNewPointFromPoint(oldPoint);
            }

            //  Clone polygons (and corners);
            for(uint polygonIndex = 0; polygonIndex < src.Polygons.Count; ++polygonIndex)
            {
                if(
                    (selectedPolygonIndices == null) || 
                    selectedPolygonIndices.Contains(polygonIndex)
                )
                {
                    Polygon oldPolygon = Source.Polygons[(int)polygonIndex];
                    Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);
                    AddPolygonCorners(newPolygon, oldPolygon);
                }
            }

            BuildDestinationEdgesWithSourcing();
            InterpolateAllAttributeMaps();
        }
    }
}