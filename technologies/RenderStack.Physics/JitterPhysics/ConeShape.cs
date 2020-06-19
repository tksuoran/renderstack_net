#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class ConeShape : Shape
    {
        private Jitter.Collision.Shapes.ConeShape coneShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return coneShape; } }

        public ConeShape(float height, float radius)
        {
            coneShape = new Jitter.Collision.Shapes.ConeShape(height, radius);
        }
    }
}

#endif