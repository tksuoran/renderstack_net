#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class EllipsoidShape : Shape
    {
        private Jitter.Collision.Shapes.EllipsoidShape boxShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return boxShape; } }

        public EllipsoidShape(float xSize, float ySize, float zSize)
        {
            boxShape = new Jitter.Collision.Shapes.EllipsoidShape(xSize, ySize, zSize);
        }
    }
}

#endif