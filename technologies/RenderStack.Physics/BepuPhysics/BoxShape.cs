//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_BEPU_PHYSICS

using BEPUphysics.CollisionShapes;

namespace RenderStack.Physics
{
    public class BoxShape : Shape
    {
        private BEPUphysics.CollisionShapes.ConvexShapes.BoxShape boxShape;

        internal override BEPUphysics.CollisionShapes.EntityShape shape { get { return boxShape; } }

        public BoxShape(float xSize, float ySize, float zSize)
        {
            boxShape = new BEPUphysics.CollisionShapes.ConvexShapes.BoxShape(xSize, ySize, zSize);
            ComputeMassProperties();
        }
    }
}

#endif