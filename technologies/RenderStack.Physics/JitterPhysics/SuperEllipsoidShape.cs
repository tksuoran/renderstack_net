#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class SuperEllipsoidShape : Shape
    {
        private Jitter.Collision.Shapes.SuperEllipsoidShape boxShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return boxShape; } }

        public SuperEllipsoidShape(float xSize, float ySize, float zSize, float n1, float n2)
        {
            boxShape = new Jitter.Collision.Shapes.SuperEllipsoidShape(xSize, ySize, zSize, n1, n2);
        }
    }
}

#endif