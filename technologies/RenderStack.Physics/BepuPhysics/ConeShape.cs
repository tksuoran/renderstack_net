#if USE_BEPU_PHYSICS

using BEPUphysics.CollisionShapes;

namespace RenderStack.Physics
{
    public class ConeShape : Shape
    {
        private BEPUphysics.CollisionShapes.ConvexShapes.ConeShape coneShape;

        internal override BEPUphysics.CollisionShapes.EntityShape shape { get { return coneShape; } }

        public ConeShape(float height, float radius)
        {
            coneShape = new BEPUphysics.CollisionShapes.ConvexShapes.ConeShape(height, radius);
            ComputeMassProperties();
        }
    }
}

#endif