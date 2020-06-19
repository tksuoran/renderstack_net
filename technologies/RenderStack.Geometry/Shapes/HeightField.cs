using System.Collections.Generic;
using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class HeightField : Geometry
    {
        public HeightField(float[,] heights, float scaleX, float scaleZ)
        {
            var pointLocations = PointAttributes.FindOrCreate<Vector3>("point_locations");
            var pointTexcoords = PointAttributes.FindOrCreate<Vector2>("point_texcoords");

            //  Generate vertices
            int xCount = heights.GetLength(0);
            int zCount = heights.GetLength(1);
            for(int x = 0; x < xCount; x++)
            {
                float s = (float)x / (float)xCount;
                float xP = (float)x * scaleX;
                for(int z = 0; z < zCount; z++ )
                {
                    float t = (float)z / (float)zCount;
                    float zP = (float)z * scaleZ;
                    float yP = heights[x, z];

                    var point = MakePoint();

                    pointLocations[point] = new Vector3(xP, yP, zP);
                    pointTexcoords[point] = new Vector2(s, t);

                    Points[x * zCount + z] = point;
                }
            }

            //  Generate quads
            for(int x = 0; x < xCount - 1; x++)
            {
                for(int z = 0; z < zCount - 1; z++)
                {
                    var pol = MakePolygon();
                    pol.MakeCorner(Points[(x + 1) * zCount + z    ]);
                    pol.MakeCorner(Points[(x + 1) * zCount + z + 1]);
                    pol.MakeCorner(Points[(x    ) * zCount + z + 1]);
                    pol.MakeCorner(Points[(x    ) * zCount + z    ]);
                }
            }

            ComputePolygonCentroids();
            ComputePolygonNormals();
            SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)System.Math.PI));
        }
    }
}
