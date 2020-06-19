//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

using Jitter.LinearMath;

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