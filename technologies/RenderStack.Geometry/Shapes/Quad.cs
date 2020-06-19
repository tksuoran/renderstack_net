namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class QuadXY : Geometry
    {
        public QuadXY(
            double x, double y, double z
        )
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint(-x,  y,  z, 0, 1);  /*  0  */ 
            MakePoint(-x, -y,  z, 0, 0);  /*  4  */ 
            MakePoint( x, -y,  z, 1, 0);  /*  5  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 

            MakePolygon(  0, 1, 2, 3  );
        }
    }
    [System.Serializable]
    public class QuadXZ : Geometry
    {
        public QuadXZ(
            double x, double y, double z
        )
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint(-x,  y,  z, 0, 1);  /*  0  */ 
            MakePoint(-x,  y, -z, 0, 0);  /*  4  */ 
            MakePoint( x,  y, -z, 1, 0);  /*  5  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 

            MakePolygon(  0, 1, 2, 3  );
        }
    }
    [System.Serializable]
    public class QuadYZ : Geometry
    {
        public QuadYZ(
            double x, double y, double z
        )
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint( x, -y,  z, 0, 1);  /*  0  */ 
            MakePoint( x, -y, -z, 0, 0);  /*  4  */ 
            MakePoint( x,  y, -z, 1, 0);  /*  5  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 

            MakePolygon(  0, 1, 2, 3  );
        }
    }
}
