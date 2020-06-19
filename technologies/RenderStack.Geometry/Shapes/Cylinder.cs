using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Cylinder : Geometry
    {
        public Cylinder(
            double  minX, 
            double  maxX, 
            double  baseRadius, 
            int     sliceCount
        )
        {
            var cornerNormals = CornerAttributes.FindOrCreate<Vector3>("corner_normals");

            Polygon bottomPolygon       = MakePolygon();
            Polygon topPolygon          = MakePolygon();
            Point   previousBottomPoint = null;
            Point   previousTopPoint    = null;
            Point   firstBottomPoint    = null;
            Point   firstTopPoint       = null;

            double previousSinPhi   = 0;
            double previousCosPhi   = 0;
            double firstSinPhi      = 0;
            double firstCosPhi      = 0;

            /*  Other vertices  */ 
            for(int slice = 0; slice <= sliceCount; ++slice)
            {
                double  relSlice        = (double)slice / (double)sliceCount;
                double  phi             = System.Math.PI * 2.0 * relSlice;
                double  sinPhi          = System.Math.Sin(phi);
                double  cosPhi          = System.Math.Cos(phi);
                double  y               = baseRadius * sinPhi;
                double  z               = baseRadius * cosPhi;
                Point   bottomPoint     = MakePoint(minX, y, z);
                Point   topPoint        = MakePoint(maxX, y, z);
                Corner  bottomCorner    = bottomPolygon.MakeCorner(bottomPoint);
                Corner  topCorner       = topPolygon.MakeCorner(topPoint);
                cornerNormals[bottomCorner] = new Vector3(-1.0f, 0.0f, 0.0f);
                cornerNormals[topCorner]    = new Vector3( 1.0f, 0.0f, 0.0f);
                if(previousBottomPoint != null && previousTopPoint != null)
                {
                    Polygon polygon = MakePolygon();
                    Corner  c0      = polygon.MakeCorner(previousBottomPoint);
                    Corner  c1      = polygon.MakeCorner(bottomPoint);
                    Corner  c2      = polygon.MakeCorner(topPoint);
                    Corner  c3      = polygon.MakeCorner(previousTopPoint);
                    cornerNormals[c0] = new Vector3(0.0f, (float)previousSinPhi,  (float)previousCosPhi);
                    cornerNormals[c1] = new Vector3(0.0f, (float)sinPhi,          (float)cosPhi);
                    cornerNormals[c2] = new Vector3(0.0f, (float)sinPhi,          (float)cosPhi);
                    cornerNormals[c3] = new Vector3(0.0f, (float)previousSinPhi,  (float)previousCosPhi);
                }
                else
                {
                    firstSinPhi         = sinPhi;
                    firstCosPhi         = cosPhi;
                    firstBottomPoint    = bottomPoint;
                    firstTopPoint       = topPoint;
                }
                previousSinPhi      = sinPhi;
                previousCosPhi      = cosPhi;
                previousBottomPoint = bottomPoint;
                previousTopPoint    = topPoint;
            }

            {
                Polygon polygon = MakePolygon();
                Corner  c0      = polygon.MakeCorner(previousBottomPoint);
                Corner  c1      = polygon.MakeCorner(firstBottomPoint);
                Corner  c2      = polygon.MakeCorner(firstTopPoint);
                Corner  c3      = polygon.MakeCorner(previousTopPoint);
                cornerNormals[c0] = new Vector3(0.0f, (float)previousSinPhi,  (float)previousCosPhi);
                cornerNormals[c1] = new Vector3(0.0f, (float)firstSinPhi,     (float)firstCosPhi);
                cornerNormals[c2] = new Vector3(0.0f, (float)firstSinPhi,     (float)firstCosPhi);
                cornerNormals[c3] = new Vector3(0.0f, (float)previousSinPhi,  (float)previousCosPhi);
            }

            bottomPolygon.Reverse();
        }
    }
}
