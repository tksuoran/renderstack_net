#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class CylinderShape : Shape
    {
        private Jitter.Collision.Shapes.CylinderShape cylinderShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return cylinderShape; } }

        public CylinderShape(float height, float radius)
        {
            cylinderShape = new Jitter.Collision.Shapes.CylinderShape(height, radius);
        }
    }
}

#endif