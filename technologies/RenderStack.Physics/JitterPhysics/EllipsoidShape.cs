//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

using Jitter.LinearMath;

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