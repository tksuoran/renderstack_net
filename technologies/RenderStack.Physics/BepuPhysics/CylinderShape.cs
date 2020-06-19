#if USE_BEPU_PHYSICS

using BEPUphysics.CollisionShapes;

namespace RenderStack.Physics
{
    public class CylinderShape : Shape
    {
        private BEPUphysics.CollisionShapes.ConvexShapes.CylinderShape cylinderShape;

        internal override BEPUphysics.CollisionShapes.EntityShape shape { get { return cylinderShape; } }

        public CylinderShape(float height, float radius)
        {
            cylinderShape = new BEPUphysics.CollisionShapes.ConvexShapes.CylinderShape(height, radius);
            ComputeMassProperties();
        }
    }
}

#endif