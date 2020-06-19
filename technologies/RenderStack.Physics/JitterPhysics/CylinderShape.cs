//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

using Jitter.LinearMath;

namespace RenderStack.Physics
{
    public class CylinderShape : Shape
    {
        private Jitter.Collision.Shapes.CylinderShape cylinderShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return cylinderShape; } }

        public CylinderShape(float height, float radius)
        {
            cylinderShape = new Jitter.Collision.Shapes.CylinderShape(height, radius);
        }
    }
}

#endif