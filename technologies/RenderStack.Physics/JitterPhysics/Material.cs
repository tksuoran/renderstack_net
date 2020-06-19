//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public class Material
    {
        private Jitter.Dynamics.Material material;

        public float Restitution { get { return material.Restitution; } set { material.Restitution = value; } }
        public float StaticFriction { get { return material.StaticFriction; } set { material.StaticFriction = value; } }
        public float DynamicFriction { get { return material.DynamicFriction; } set { material.DynamicFriction = value; } }

        public Material(Jitter.Dynamics.Material material)
        {
            this.material = material;
        }
    }
}

#endif