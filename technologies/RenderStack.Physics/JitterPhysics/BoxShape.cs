#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class BoxShape : Shape
    {
        private Jitter.Collision.Shapes.BoxShape boxShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return boxShape; } }

        public BoxShape(float xSize, float ySize, float zSize)
        {
            boxShape = new Jitter.Collision.Shapes.BoxShape(xSize, ySize, zSize);
        }
    }
}

#endif