#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class TerrainShape : Shape
    {
        private Jitter.Collision.Shapes.TerrainShape terrainShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return terrainShape; } }

        public TerrainShape(float[,] heights, float scaleX, float scaleZ)
        {
            terrainShape = new Jitter.Collision.Shapes.TerrainShape(heights, scaleX, scaleZ);
        }
    }
}

#endif