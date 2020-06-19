#if USE_BEPU_PHYSICS

using BEPUphysics.CollisionShapes;

namespace RenderStack.Physics
{
    public class SphereShape : Shape
    {
        private BEPUphysics.CollisionShapes.ConvexShapes.SphereShape sphereShape;

        internal override BEPUphysics.CollisionShapes.EntityShape shape { get { return sphereShape; } }

        public SphereShape(float radius)
        {
            sphereShape = new BEPUphysics.CollisionShapes.ConvexShapes.SphereShape(radius);
            ComputeMassProperties();
        }
    }
}

#endif