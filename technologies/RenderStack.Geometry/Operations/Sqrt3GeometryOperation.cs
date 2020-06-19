//#define DEBUG_CHECK

using System.Collections.Generic;
using System.Linq;

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
