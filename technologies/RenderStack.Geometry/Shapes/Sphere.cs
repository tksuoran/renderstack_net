using System.Collections.Generic;

using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Sphere : Geometry
    {
        //  Stack numbering example        
        //  with stackDivision = 2         
        //                                 
        //  stack         rel    known     
        //  base0  stack  stack  as        
        //  -------------------------------
        //   5      3      1.0    top      
        //   4      2                      
        //   3      1                      
        //   2      0      0.0    equator  
        //   1     -1                      
        //   0     -2                      
        //  -1     -3     -1.0    bottom   
        //                                 
        //  bottom: relStack = -1.0  <=>  stackBase0 = -1
        //  top:    relStack =  1.0  <=>  stack = stackDivision + 1  <=>  stackBase0 = 2 * stackDivision + 1
        protected class MakeInfo
        {
            public double       radius;
            public int          sliceCount;
            public int          stackDivision;
            public int          stackCount;
            public int          stackBase0Bottom;
            public int          stackBase0Top;

            public List<Point>  points;
            public Point        top;
            public Point        bottom;

            public Dictionary<Point, Vector3>   pointLocations;
            public Dictionary<Point, Vector3>   pointNormals;
            public Dictionary<Point, Vector2>   pointTexcoords;
            public Dictionary<Corner, Vector2>  cornerTexcoords;
            public Dictionary<Polygon, Vector3> polygonCentroids;
            public Dictionary<Polygon, Vector3> polygonNormals;

            public MakeInfo(double radius, int sliceCount, int stackDivision)
            {
                this.radius             = radius;
                this.sliceCount         = sliceCount;
                this.stackDivision      = stackDivision;
                this.stackCount         = (2 * stackDivision) + 1;
                this.stackBase0Bottom   = -1;
                this.stackBase0Top      = 2 * stackDivision + 1;
                points = new List<Point>();
            }
        };

        void MakeCorner(MakeInfo info, Polygon polygon, int slice, int stackBase0)
        {
            //int stack_base0 = stack + info.stackDivision;
            int     stack       = stackBase0 - info.stackDivision;
            double  relSlice    = (double)(slice) / (double)(info.sliceCount);
            double  relStack    = (double)(stack) / (double)(info.stackDivision + 1);
            bool    sliceSeam   = /*(slice == 0) ||*/ (slice == info.sliceCount);
            bool    bottom      = (stackBase0 == info.stackBase0Bottom);
            bool    top         = (stackBase0 == info.stackBase0Top);
            bool    uvDiscontinuity = sliceSeam || bottom || top;

            Point point;
            if(top)
            {
                point = info.top;
            }
            else if(bottom)
            {
                point = info.bottom;
            }
            else
            {
                if(slice == info.sliceCount)
                {
                    point = info.points[stackBase0];
                }
                else
                {
                    point = info.points[(slice * info.stackCount) + stackBase0];
                }
            }

            Corner corner = polygon.MakeCorner(point);

            if(uvDiscontinuity)
            {
                float s = 1.0f - (float)(relSlice);
                float t = 1.0f - (float)(0.5 * (1.0 + relStack));

                info.cornerTexcoords[corner] = new Vector2(s, t);
            }
        }
        public Sphere(double radius, int sliceCount, int stackDivision)
        {
            MakeInfo info = new MakeInfo(radius, sliceCount, stackDivision);

            info.pointLocations     = PointAttributes.FindOrCreate<Vector3>("point_locations");
            info.pointNormals       = PointAttributes.FindOrCreate<Vector3>("point_normals");
            info.pointTexcoords     = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
            info.polygonCentroids   = PolygonAttributes.FindOrCreate<Vector3>("polygon_centroids");
            info.polygonNormals     = PolygonAttributes.FindOrCreate<Vector3>("polygon_normals");
            info.cornerTexcoords    = CornerAttributes.FindOrCreate<Vector2>("corner_texcoords");

            int slice;
            int stack;
            for(slice = 0; slice < sliceCount; ++slice)
            {
                double relSlice = (double)(slice) / (double)(sliceCount);
                for(stack = -stackDivision; stack <= stackDivision; ++stack)
                {
                    double relStack = (double)(stack) / (double)(stackDivision + 1);
                    Point  point    = SpherePoint(info, relSlice, relStack);

                    info.points.Add(point);
                }
            }
            info.bottom = SpherePoint(info, 0.5f, -1.0f);
            info.top    = SpherePoint(info, 0.5f,  1.0f);

            #region bottom fan
            {
                for(slice = 0; slice < sliceCount; ++slice)
                {
                    int nextSlice  = (slice + 1);
                    int stackBase0 = 0;

                    double relSlice = ((double)(slice) + 0.5) / (double)(sliceCount);
                    double relStack = -1.0 + (0.5 / (double)(stackDivision + 1));

                    Point centroid = SpherePoint(info, relSlice, relStack);

                    var polygon = MakePolygon();
                    MakeCorner(info, polygon, slice, stackBase0);
                    MakeCorner(info, polygon, slice, info.stackBase0Bottom);
                    MakeCorner(info, polygon, nextSlice, stackBase0);

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }
            #endregion

            #region middle quads, bottom up
            for(
                stack = -stackDivision; 
                stack < stackDivision; 
                ++stack
            )
            {
                int stackBase0     = stack + stackDivision;
                int nextStackBase0 = stackBase0 + 1;

                double relStack = ((double)(stack) + 0.5) / (double)(stackDivision + 1);

                for(slice = 0; slice < sliceCount; ++slice)
                {
                    int    nextSlice = (slice + 1);
                    double relSlice  = ((double)(slice) + 0.5) / (double)(sliceCount);
                    Point  centroid  = SpherePoint(info, relSlice, relStack);

                    var polygon = MakePolygon();
                    MakeCorner(info, polygon, nextSlice, nextStackBase0);
                    MakeCorner(info, polygon, slice,     nextStackBase0);
                    MakeCorner(info, polygon, slice,     stackBase0);
                    MakeCorner(info, polygon, nextSlice, stackBase0);

                    info.polygonCentroids[polygon] = info.pointLocations[centroid];
                    info.polygonNormals  [polygon] = info.pointNormals  [centroid];
                }
            }
            #endregion

            #region top fan
            for(slice = 0; slice < sliceCount; ++slice)
            {
                int nextSlice  = (slice + 1);
                int stackBase0 = stackDivision + stackDivision;

                double relSlice = ((double)(slice) + 0.5) / (double)(sliceCount);
                double relStack = 1.0 - (0.5 / (double)(stackDivision + 1));

                Point centroid = SpherePoint(info, relSlice, relStack);

                var polygon = MakePolygon();
                MakeCorner(info, polygon, slice,     info.stackBase0Top);
                MakeCorner(info, polygon, slice,     stackBase0);
                MakeCorner(info, polygon, nextSlice, stackBase0);

                info.polygonCentroids[polygon] = info.pointLocations[centroid];
                info.polygonNormals  [polygon] = info.pointNormals  [centroid];
            }
            #endregion
        }

        protected Point SpherePoint(MakeInfo info, double relSlice, double relStack)
        {
            double  phi             = (System.Math.PI * 2.0 * relSlice);
            double  sin_phi         = System.Math.Sin(phi);
            double  cos_phi         = System.Math.Cos(phi);

            double  theta           = (System.Math.PI * 0.5  * relStack);
            double  sin_theta       = System.Math.Sin(theta);
            double  cos_theta       = System.Math.Cos(theta);

            double  stack_radius    = cos_theta;

            float   xVN             = (float)(stack_radius * cos_phi);
            float   yVN             = (float)(sin_theta);
            float   zVN             = (float)(stack_radius * sin_phi);

            float   xP              = (float)(info.radius * xVN);
            float   yP              = (float)(info.radius * yVN);
            float   zP              = (float)(info.radius * zVN);

            float   s               = 1.0f - (float)(relSlice);
            float   t               = 1.0f - (float)(0.5 * (1.0 + relStack));

            Point point = MakePoint();

            bool uvDiscontinuity = (relStack == -1.0) || (relStack == 1.0) || /*(relSlice == 0.0) ||*/ (relSlice == 1.0);

            info.pointLocations[point] = new Vector3(xP, yP, zP);
            info.pointNormals  [point] = new Vector3(xVN, yVN, zVN);
#if true
            if(uvDiscontinuity == false)
#endif
            {
                info.pointTexcoords[point] = new Vector2(s, t);
            }

            return point;
        }
    }
}
