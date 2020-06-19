namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Tetrahedron : Geometry
    {
        public Tetrahedron(double r)
        {
            double sq2 = System.Math.Sqrt(2.0);
            double sq3 = System.Math.Sqrt(3.0);

            MakePoint(                 0,          r,                  0   );
            MakePoint(                 0,   -r / 3.0,  r * 2.0 * sq2 / 3.0 );
            MakePoint(-r * sq3 * sq2 / 3.0, -r / 3.0,       -r * sq2 / 3.0 );
            MakePoint( r * sq3 * sq2 / 3.0, -r / 3.0,       -r * sq2 / 3.0 );
            MakePolygon( 0, 1, 2 );
            MakePolygon( 3, 1, 0 );
            MakePolygon( 0, 2, 3 );
            MakePolygon( 3, 2, 1 );
        }
    }
}
