namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Cuboctahedron : Geometry
    {
        public Cuboctahedron(double r)
        {
            double sq2 = System.Math.Sqrt(2.0);

            MakePoint(      0,      r,            0 );
            MakePoint(  r / 2,  r / 2,  r * sq2 / 2 );
            MakePoint(  r / 2,  r / 2, -r * sq2 / 2 );
            MakePoint(      r,      0,            0 );
            MakePoint(  r / 2, -r / 2,  r * sq2 / 2 );
            MakePoint(  r / 2, -r / 2, -r * sq2 / 2 );
            MakePoint(      0,     -r,            0 );
            MakePoint( -r / 2, -r / 2,  r * sq2 / 2 );
            MakePoint( -r / 2, -r / 2, -r * sq2 / 2 );
            MakePoint(     -r,      0,            0 );
            MakePoint( -r / 2,  r / 2,  r * sq2 / 2 );
            MakePoint( -r / 2,  r / 2, -r * sq2 / 2 );

            MakePolygon(  1, 4,  7, 10  );
            MakePolygon(  4, 3,  5,  6  );
            MakePolygon(  0, 2,  3,  1  );
            MakePolygon( 11, 8,  5,  2  );
            MakePolygon( 10, 9, 11,  0  );
            MakePolygon(  7, 6,  8,  9  );

            MakePolygon(  0, 1, 10  );
            MakePolygon(  3, 4,  1  );
            MakePolygon(  4, 6,  7  );
            MakePolygon( 10, 7,  9  );
            MakePolygon( 11, 2,  0  );
            MakePolygon(  2, 5,  3  );
            MakePolygon(  8, 6,  5  );
            MakePolygon(  9, 8, 11  );
        }
    }
}
