using System.Collections.Generic;

using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    [System.Serializable]
    /*  Comment: Mostly stable.  */
    public class Disc : Geometry
    {
        protected class MakeInfo
        {
            public double   innerRadius;
            public double   outerRadius;
            public int      sliceCount;
            public int      stackCount;

            public Dictionary<KeyValuePair<int,int>, Point>  points;
            public Point        center;

            public Dictionary<Point, Vector3>   pointLocations;
            public Dictionary<Point, Vector3>   pointNormals;
            public Dictionary<Point, Vector2>   pointTexcoords;
            public Dictionary<Corner, Vector3>  cornerNormals;
            public Dictionary<Corner, Vector2>  cornerTexcoords;
            public Dictionary<Polygon, Vector3> polygonCentroids;
            public Dictionary<Polygon, Vector3> polygonNormals;

            public MakeInfo(
                double  innerRadius,
                double  outerRadius,
                int     sliceCount,
                int     stackCount
            )
            {
                this.innerRadius    = innerRadius;
                this.outerRadius    = outerRadius;
                this.sliceCount     = sliceCount;
                this.stackCount     = stackCount;

                points = new Dictionary<KeyValuePair<int,int>,Point>();
            }
        };

        private Point GetPoint(MakeInfo info, int slice, int stack)
        {
            return info.points[new KeyValuePair<int, int>(slice, stack)];
        }

        //  relStackIn is in range -1..1
        //  relStack is in range 0..1
        Point MakePoint(MakeInfo info, double relSlice, double relStack)
        {
            double phi              = System.Math.PI * 2.0 * relSlice;
            double sinPhi           = System.Math.Sin(phi);
            double cosPhi           = System.Math.Cos(phi);
            double oneMinusRelStack = 1.0 - relStack;

            Vector3 position = new Vector3(
                (float)(oneMinusRelStack * (info.outerRadius * cosPhi) + relStack * (info.innerRadius * cosPhi)),
                (float)(oneMinusRelStack * (info.outerRadius * sinPhi) + relStack * (info.innerRadius * sinPhi)),
                0.0f
            );

            double s = relSlice;
            double t = relStack;

            Point point = MakePoint();

            info.pointLocations[point] = new Vector3((float)position.X,  (float)position.Y,  (float)position.Z);
            info.pointNormals  [point] = Vector3.UnitZ;
            info.pointTexcoords[point] = new Vector2((float)s,           (float)t);

            return point;
        }
        Corner MakeCorner(MakeInfo info, Polygon polygon, int slice, int stack)
        {
            double  relSlice        = (double)slice / (double)info.sliceCount;
            double  relStack        = (info.stackCount == 1) ? 1.0 : (double)stack / (double)(info.stackCount - 1);
            bool    sliceSeam       = (slice == 0) || (slice == info.sliceCount);
            bool    center          = (stack == 0) && (info.innerRadius == 0.0f);
            bool    uvDiscontinuity = sliceSeam || center;
            Point point;
            if(center)
            {
                point = info.center;
            }
            else
            {
                if(slice == info.sliceCount)
                {
                    point = info.points[new KeyValuePair<int, int>(0, stack)];
                }
                else
                {
                    point = info.points[new KeyValuePair<int, int>(slice, stack)];
                }
            }

            Corner corner = polygon.MakeCorner(point);

            if(uvDiscontinuity)
            {
                float s = (float)(relSlice);
                float t = (float)(relStack);

                info.cornerTexcoords[corner] = new Vector2(s, t);
            }

            return corner;
        }

        public Disc(
            double  outerRadius, 
            double  innerRadius, 
            int     sliceCount,
            int     stackCount
        )
        {
            MakeInfo info = new MakeInfo(outerRadius, innerRadius, sliceCount, stackCount);

            info.pointLocations     = PointAttributes.FindOrCreate<Vector3>("point_locations");
            info.pointNormals       = PointAttributes.FindOrCreate<Vector3>("point_normals");
            info.pointTexcoords     = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
            info.polygonCentroids   = PolygonAttributes.FindOrCreate<Vector3>("polygon_centroids");
            info.polygonNormals     = PolygonAttributes.FindOrCreate<Vector3>("polygon_normals");
            info.cornerNormals      = CornerAttributes.FindOrCreate<Vector3>("corner_normals");
            info.cornerTexcoords    = CornerAttributes.FindOrCreate<Vector2>("corner_texcoords");

            /*  Make points  */
            for(int stack = 0; stack < stackCount; ++stack)
            {
                double relStack = (stackCount == 1) ? 1.0 : (double)stack / (double)(stackCount - 1);
                for(int slice = 0; slice <= sliceCount; ++slice)
                {
                    double relSlice = (double)slice / (double)sliceCount;

                    info.points[new KeyValuePair<int,int>(slice, stack)] = MakePoint(info, relSlice, relStack);
                }
            }

            /*  Special case without center point  */
            if(stackCount == 1)
            {
                Polygon polygon = MakePolygon();
                info.polygonCentroids[polygon] = Vector3.Zero;
                info.polygonNormals  [polygon] = Vector3.UnitZ;

                for(int slice = sliceCount - 1; slice >= 0; --slice)
                {
                    MakeCorner(info, polygon, slice, 0);
                }
                return;
            }

            /*  Make center point if needed  */
            if(innerRadius == 0.0f)
            {
                info.center = MakePoint(0.0, 0.0, 0.0);
            }

            /*  Quads/triangles  */ 
            for(int stack = 0; stack < stackCount - 1; ++stack)
            {
                double relStackCentroid = (stackCount == 1) ? 0.5 : (double)stack / (double)(stackCount - 1);

                for(int slice = 0; slice < sliceCount; ++slice)
                {
                    double  relSliceCentroid    = ((double)(slice) + 0.5) / (double)(sliceCount);
                    Point   centroid            = MakePoint(info, relSliceCentroid, relStackCentroid);
                    Polygon polygon             = MakePolygon();

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = Vector3.UnitZ;
                    if((stack == 0) && (innerRadius == 0.0))
                    {
                        Corner tip = MakeCorner(info, polygon, slice, stack);
                        MakeCorner(info, polygon, slice + 1, stack + 1);
                        MakeCorner(info, polygon, slice, stack + 1);

                        Vector2 t1 = info.pointTexcoords[GetPoint(info, slice, stack)];
                        Vector2 t2 = info.pointTexcoords[GetPoint(info, slice + 1, stack)];
                        Vector2 averageTexcoord = (t1 + t2) / 2.0f;
                        info.cornerTexcoords[tip] = averageTexcoord;
                    }
                    else
                    {
                        MakeCorner(info, polygon, slice + 1, stack + 1);
                        MakeCorner(info, polygon, slice,     stack + 1);
                        MakeCorner(info, polygon, slice,     stack    );
                        MakeCorner(info, polygon, slice + 1, stack    );
                    }
                }
            }
            BuildEdges();
        }
    }
}
