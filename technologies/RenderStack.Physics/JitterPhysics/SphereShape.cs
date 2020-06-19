//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

using Jitter.LinearMath;

namespace RenderStack.Physics
{
    public class SphereShape : Shape
    {
        private Jitter.Collision.Shapes.SphereShape sphereShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return sphereShape; } }

        public SphereShape(float radius)
        {
            sphereShape = new Jitter.Collision.Shapes.SphereShape(radius);
        }
    }
}

#endif