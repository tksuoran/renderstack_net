using System.Collections.Generic;
using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Cube : Geometry
    {
        public Cube(double x, double y, double z)
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint(-x,  y,  z, 0, 1);  /*  0  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 
            MakePoint( x, -y,  z, 1, 1);  /*  2  */ 
            MakePoint(-x, -y,  z, 0, 1);  /*  3  */ 
            MakePoint(-x,  y, -z, 0, 0);  /*  4  */ 
            MakePoint( x,  y, -z, 1, 0);  /*  5  */ 
            MakePoint( x, -y, -z, 1, 0);  /*  6  */ 
            MakePoint(-x, -y, -z, 0, 0);  /*  7  */ 

            MakePolygon(  0, 1, 2, 3  );
            MakePolygon(  0, 3, 7, 4  ); 
            MakePolygon(  0, 4, 5, 1  );  /*  top  */ 
            MakePolygon(  5, 4, 7, 6  );
            MakePolygon(  2, 1, 5, 6  );
            MakePolygon(  7, 3, 2, 6  );  /*  bottom  */ 
        }

        protected class MakeInfo
        {
            public Vector3                  Size;
            public IVector3                 Div;
            public float                    p;
            public Dictionary<long,Point>   Points;

            public Dictionary<Point, Vector3>   pointLocations;
            public Dictionary<Point, Vector3>   pointNormals;
            public Dictionary<Point, Vector2>   pointTexcoords;
            public Dictionary<Corner, Vector3>  cornerNormals;
            public Dictionary<Corner, Vector2>  cornerTexcoords;
            public Dictionary<Polygon, Vector3> polygonCentroids;
            public Dictionary<Polygon, Vector3> polygonNormals;

            public MakeInfo(Vector3 size, IVector3 div, float p)
            {
                this.Size = size;
                this.Div = div;
                this.p = p;
                Points = new Dictionary<long,Point>();
            }
        }

        private float SPow(float x, float p)
        {
            return (float)(System.Math.Sign(x) * System.Math.Pow(System.Math.Abs(x), p));
        }

        protected Point MakePoint(MakeInfo info, int x, int y, int z, Vector3 N, float s, float t)
        {
            long    key = y * info.Div.X * 4 * info.Div.Z * 4 + x * info.Div.Z * 4 + z;
            if(info.Points.ContainsKey(key) == true)
            {
                return info.Points[key];
            }

            float relX  = (float)x / (float)info.Div.X;
            float relY  = (float)y / (float)info.Div.Y;
            float relZ  = (float)z / (float)info.Div.Z;
            float relXP = SPow(relX, info.p);
            float relYP = SPow(relY, info.p);
            float relZP = SPow(relZ, info.p);

            float   xP  = relXP * info.Size.X;
            float   yP  = relYP * info.Size.Y;
            float   zP  = relZP * info.Size.Z;

            Point   point = MakePoint();

            bool    discontinuity = (x == -info.Div.X) || (x == info.Div.X) || (y == -info.Div.Y) || (y == info.Div.Y) || (z == -info.Div.Z) || (z == info.Div.Z);

            info.pointLocations[point] = new Vector3(xP, yP, zP);
            if(discontinuity == false)
            {
                info.pointNormals[point] = N;
                info.pointTexcoords[point] = new Vector2(s, t);
            }

            info.Points[key] = point;

            return point;
        }

        private Corner MakeCorner(MakeInfo info, Polygon polygon, int x, int y, int z, Vector3 N, float s, float t)
        {
            long    key = y * info.Div.X * 4 * info.Div.Z * 4 + x * info.Div.Z * 4 + z;
            if(info.Points.ContainsKey(key) == false)
            {
                key = 0;
            }

            Point   point = info.Points[key];
            bool    discontinuity = (x == -info.Div.X) || (x == info.Div.X) || (y == -info.Div.Y) || (y == info.Div.Y) || (z == -info.Div.Z) || (z == info.Div.Z);
            Corner  corner = polygon.MakeCorner(point);

            if(discontinuity == true)
            {
                info.cornerNormals[corner] = N;
                info.cornerTexcoords[corner] = new Vector2(s, t);
            }

            info.Points[key] = point;

            return corner;
        }
        public Cube(Vector3 size, IVector3 div)
        : this(size, div, 1.0f)
        {
        }
        public Cube(Vector3 size, IVector3 div, float p)
        {
            int x;
            int y;
            int z;

            MakeInfo info = new MakeInfo(0.5f * size, div, p);

            info.pointLocations     = PointAttributes.FindOrCreate<Vector3>("point_locations");
            info.pointNormals       = PointAttributes.FindOrCreate<Vector3>("point_normals");
            info.pointTexcoords     = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
            info.cornerNormals      = CornerAttributes.FindOrCreate<Vector3>("corner_normals");
            info.cornerTexcoords    = CornerAttributes.FindOrCreate<Vector2>("corner_texcoords");
            info.polygonCentroids   = PolygonAttributes.FindOrCreate<Vector3>("polygon_centroids");
            info.polygonNormals     = PolygonAttributes.FindOrCreate<Vector3>("polygon_normals");

            //  Generate vertices
            //  Top and bottom
            for(x = -info.Div.X; x <= info.Div.X; x++)
            {
                float relX = 0.5f + (float)x / info.Size.X;
                for(z = -info.Div.Z; z <= info.Div.Z; z++ )
                {
                    float relZ = 0.5f + (float)z / info.Size.Z;
                    MakePoint(info, x, info.Div.Y, z, Vector3.UnitY, relX, relZ);
                    MakePoint(info, x, -info.Div.Y, z, -Vector3.UnitY, relX, relZ);
                }
                for(y = -info.Div.Y; y <= info.Div.Y; y++)
                {
                    float relY = 0.5f + (float)y / info.Size.Y;

                    MakePoint(info, x, y, info.Div.Z, Vector3.UnitZ, relX, relY);
                    MakePoint(info, x, y, -info.Div.Z, -Vector3.UnitZ, relX, relY);
                }
            }

            //  Left and right
            for(z = -info.Div.Z; z <= info.Div.Z; z++)
            {
                float relZ = 0.5f + (float)z / info.Size.Z;
                for(y = -info.Div.Y; y <= info.Div.Y; y++)
                {
                    float relY = 0.5f + (float)y / info.Size.Y;
                    MakePoint(info, info.Div.X, y, z, Vector3.UnitX, relY, relZ);
                    MakePoint(info, -info.Div.X, y, z, -Vector3.UnitX, relY, relZ);
                }
            }

            //  Generate quads
            //  Top and bottom
            for(x = -info.Div.X; x < info.Div.X; x++)
            {
                float relX1 = 0.5f + (float)x / info.Size.X;
                float relX2 = 0.5f + (float)(x + 1) / info.Size.X;
                for(z = -info.Div.Z; z < info.Div.Z; z++ )
                {
                    float relZ1 = 0.5f + (float)z / info.Size.Z;
                    float relZ2 = 0.5f + (float)(z + 1) / info.Size.Z;
                    var top = MakePolygon();
                    MakeCorner(info, top, x + 1, info.Div.Y, z,     Vector3.UnitY, relX2, relZ1);
                    MakeCorner(info, top, x + 1, info.Div.Y, z + 1, Vector3.UnitY, relX2, relZ2);
                    MakeCorner(info, top, x,     info.Div.Y, z + 1, Vector3.UnitY, relX1, relZ2);
                    MakeCorner(info, top, x,     info.Div.Y, z,     Vector3.UnitY, relX1, relZ1);
                    var bottom = MakePolygon();
                    MakeCorner(info, bottom, x,     -info.Div.Y, z,     -Vector3.UnitY, relX1, relZ1);
                    MakeCorner(info, bottom, x,     -info.Div.Y, z + 1, -Vector3.UnitY, relX1, relZ2);
                    MakeCorner(info, bottom, x + 1, -info.Div.Y, z + 1, -Vector3.UnitY, relX2, relZ2);
                    MakeCorner(info, bottom, x + 1, -info.Div.Y, z,     -Vector3.UnitY, relX2, relZ1);
                    info.polygonNormals[top] = Vector3.UnitY;
                    info.polygonNormals[bottom] = -Vector3.UnitY;
                }
                for(y = -info.Div.Y; y < info.Div.Y; y++)
                {
                    float relY1 = 0.5f + (float)y / info.Size.Y;
                    float relY2 = 0.5f + (float)(y + 1) / info.Size.Y;
                    var back = MakePolygon();
                    MakeCorner(info, back, x,     y,     info.Div.Z, Vector3.UnitZ, relX1, relY1);
                    MakeCorner(info, back, x,     y + 1, info.Div.Z, Vector3.UnitZ, relX1, relY2);
                    MakeCorner(info, back, x + 1, y + 1, info.Div.Z, Vector3.UnitZ, relX2, relY2);
                    MakeCorner(info, back, x + 1, y,     info.Div.Z, Vector3.UnitZ, relX2, relY1);
                    var front = MakePolygon();
                    MakeCorner(info, front, x + 1, y,     -info.Div.Z, -Vector3.UnitZ, relX2, relY1);
                    MakeCorner(info, front, x + 1, y + 1, -info.Div.Z, -Vector3.UnitZ, relX2, relY2);
                    MakeCorner(info, front, x,     y + 1, -info.Div.Z, -Vector3.UnitZ, relX1, relY2);
                    MakeCorner(info, front, x,     y,     -info.Div.Z, -Vector3.UnitZ, relX1, relY1);
                    info.polygonNormals[back] = Vector3.UnitZ;
                    info.polygonNormals[front] = -Vector3.UnitZ;
                }
            }

            //  Left and right
            for(z = -info.Div.Z; z < info.Div.Z; z++)
            {
                float relZ1 = 0.5f + (float)z / info.Size.Z;
                float relZ2 = 0.5f + (float)(z + 1) / info.Size.Z;
                for(y = -info.Div.Y; y < info.Div.Y; y++)
                {
                    float relY1 = 0.5f + (float)y / info.Size.Y;
                    float relY2 = 0.5f + (float)(y + 1) / info.Size.Y;
                    var right = MakePolygon();
                    MakeCorner(info, right, info.Div.X, y,     z,     Vector3.UnitX, relY1, relZ1);
                    MakeCorner(info, right, info.Div.X, y,     z + 1, Vector3.UnitX, relY1, relZ2);
                    MakeCorner(info, right, info.Div.X, y + 1, z + 1, Vector3.UnitX, relY2, relZ2);
                    MakeCorner(info, right, info.Div.X, y + 1, z,     Vector3.UnitX, relY2, relZ1);
                    var left = MakePolygon();
                    MakeCorner(info, left, -info.Div.X, y + 1, z,     -Vector3.UnitX, relY2, relZ1);
                    MakeCorner(info, left, -info.Div.X, y + 1, z + 1, -Vector3.UnitX, relY2, relZ2);
                    MakeCorner(info, left, -info.Div.X, y,     z + 1, -Vector3.UnitX, relY1, relZ2);
                    MakeCorner(info, left, -info.Div.X, y,     z,     -Vector3.UnitX, relY1, relZ1);
                    info.polygonNormals[right] = Vector3.UnitX;
                    info.polygonNormals[left] = -Vector3.UnitX;
                }
            }
            ComputePolygonCentroids();
        }

        public Cube(double r)
        {
            double sq3 = System.Math.Sqrt(3.0);

            MakePoint(-r / sq3,  r / sq3,  r / sq3);
            MakePoint( r / sq3,  r / sq3,  r / sq3);
            MakePoint( r / sq3, -r / sq3,  r / sq3);
            MakePoint(-r / sq3, -r / sq3,  r / sq3);
            MakePoint(-r / sq3,  r / sq3, -r / sq3);
            MakePoint( r / sq3,  r / sq3, -r / sq3);
            MakePoint( r / sq3, -r / sq3, -r / sq3);
            MakePoint(-r / sq3, -r / sq3, -r / sq3);

            MakePolygon(  0, 1, 2, 3  );
            MakePolygon(  0, 3, 7, 4  );
            MakePolygon(  0, 4, 5, 1  );
            MakePolygon(  5, 4, 7, 6  );
            MakePolygon(  2, 1, 5, 6  );
            MakePolygon(  7, 3, 2, 6  );
        }
    }
}
