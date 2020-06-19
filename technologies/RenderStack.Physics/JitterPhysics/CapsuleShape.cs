#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class CapsuleShape : Shape
    {
        private Jitter.Collision.Shapes.CapsuleShape coneShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return coneShape; } }

        public CapsuleShape(float height, float radius)
        {
            coneShape = new Jitter.Collision.Shapes.CapsuleShape(height, radius);
        }
    }
}

#endif