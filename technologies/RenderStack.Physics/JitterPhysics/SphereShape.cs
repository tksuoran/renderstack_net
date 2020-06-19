#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class SphereShape : Shape
    {
        private Jitter.Collision.Shapes.SphereShape sphereShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return sphereShape; } }

        public SphereShape(float radius)
        {
            sphereShape = new Jitter.Collision.Shapes.SphereShape(radius);
        }
    }
}

#endif