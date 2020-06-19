namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Triangle : Geometry
    {
        public Triangle(float r)
        {
            MakePoint(r * -0.28867513f, r *  0.5f, 0.0f, 0, 1);
            MakePoint(r *  0.57735027f, r *  0.0f, 0.0f, 1, 1);
            MakePoint(r * -0.28867513f, r * -0.5f, 0.0f, 1, 0);

            MakePolygon(0, 1, 2);
        }
    }
}
