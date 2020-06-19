//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

using Jitter.LinearMath;

namespace RenderStack.Physics
{
    public class ConeShape : Shape
    {
        private Jitter.Collision.Shapes.ConeShape coneShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return coneShape; } }

        public ConeShape(float height, float radius)
        {
            coneShape = new Jitter.Collision.Shapes.ConeShape(height, radius);
        }
    }
}

#endif