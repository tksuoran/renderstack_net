namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Dodecahedron : Geometry
    {
        public Dodecahedron(double r)
        {
            double sq3 = System.Math.Sqrt(3.0);
            double sq5 = System.Math.Sqrt(5.0);
            double a = 2.0 / (sq3 + sq3 * sq5);
            double b = 1.0 / (3.0 * a);

            MakePoint( r / sq3,  r / sq3,  r / sq3);
            MakePoint( r / sq3,  r / sq3, -r / sq3);
            MakePoint( r / sq3, -r / sq3,  r / sq3);
            MakePoint( r / sq3, -r / sq3, -r / sq3);
            MakePoint(-r / sq3,  r / sq3,  r / sq3);
            MakePoint(-r / sq3,  r / sq3, -r / sq3);
            MakePoint(-r / sq3, -r / sq3,  r / sq3);
            MakePoint(-r / sq3, -r / sq3, -r / sq3);
            MakePoint(       0,  r * a,  r * b);
            MakePoint(       0,  r * a, -r * b);
            MakePoint(       0, -r * a,  r * b);
            MakePoint(       0, -r * a, -r * b);
            MakePoint( r * a,  r * b,        0);
            MakePoint( r * a, -r * b,        0);
            MakePoint(-r * a,  r * b,        0);
            MakePoint(-r * a, -r * b,        0);
            MakePoint( r * b,        0,  r * a);
            MakePoint( r * b,        0, -r * a);
            MakePoint(-r * b,        0,  r * a);
            MakePoint(-r * b,        0, -r * a);

            MakePolygon(  6, 18, 4,  8, 10 );
            MakePolygon( 10,  8, 0, 16,  2 );
            MakePolygon(  3, 17, 1,  9, 11 );
            MakePolygon(  5, 19, 7, 11,  9 );
            MakePolygon(  3, 13, 2, 16, 17 );
            MakePolygon( 17, 16, 0, 12,  1 );
            MakePolygon( 19, 18, 6, 15,  7 );
            MakePolygon(  5, 14, 4, 18, 19 );
            MakePolygon(  0,  8, 4, 14, 12 );
            MakePolygon( 12, 14, 5,  9,  1 );
            MakePolygon( 13, 15, 6, 10,  2 );
            MakePolygon(  3, 11, 7, 15, 13 );
        }
    }
}
