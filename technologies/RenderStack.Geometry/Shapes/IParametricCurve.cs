using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    public interface IParametricCurve
    {
        Vector3 PositionAt(float t);
        Vector3 TangentAt(float t);
    }
}
