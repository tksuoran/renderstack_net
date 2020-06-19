using RenderStack.Graphics;

namespace RenderStack.Scene
{
    /// \note Mostly stable, somewhat experimental
    public class StereoParameters
    {
        public Floats EyeSeparation  { get; } = new Floats(0.065f);
        public Floats Perspective    { get; } = new Floats(1.0f);
        public Floats ViewportCenter { get; } = new Floats(0.0f, 0.0f, 4.0f);
        public Floats EyePosition    { get; } = new Floats(0.0f, 0.0f, 0.0f);
        public Floats ViewPlaneSize  { get; } = new Floats(2, 1);
    }
}
