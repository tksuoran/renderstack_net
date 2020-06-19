using System.Collections.Generic;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Octahedron : Geometry
    {
        public Octahedron(double r)
        {
            MakePoint( 0,  r,  0);
            MakePoint( 0, -r,  0);
            MakePoint(-r,  0,  0);
            MakePoint( 0,  0, -r);
            MakePoint( r,  0,  0);
            MakePoint( 0,  0,  r);
            MakePolygon( 0, 2, 3 );
            MakePolygon( 0, 3, 4 );
            MakePolygon( 0, 4, 5 );
            MakePolygon( 0, 5, 2 );
            MakePolygon( 3, 2, 1 );
            MakePolygon( 4, 3, 1 );
            MakePolygon( 5, 4, 1 );
            MakePolygon( 2, 5, 1 );
        }
    }
}
