//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_BEPU_PHYSICS

using BEPUphysics.CollisionShapes;

namespace RenderStack.Physics
{
    public class CapsuleShape : Shape
    {
        private BEPUphysics.CollisionShapes.ConvexShapes.CapsuleShape coneShape;

        internal override BEPUphysics.CollisionShapes.EntityShape shape { get { return coneShape; } }

        public CapsuleShape(float height, float radius)
        {
            coneShape = new BEPUphysics.CollisionShapes.ConvexShapes.CapsuleShape(height, radius);
            ComputeMassProperties();
        }
    }
}

#endif