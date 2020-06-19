namespace RenderStack.Math
{
    /// \note Somewhat experimental. Used by Geometry.
    public interface ILinear
    {
        ILinear PlusWeightTimesOther(float weight, ILinear other);
    }
}
