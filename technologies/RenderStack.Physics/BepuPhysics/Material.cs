﻿//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_BEPU_PHYSICS

namespace RenderStack.Physics
{
    public class Material
    {
        private BEPUphysics.Materials.Material material;

        public float Restitution { get { return material.Bounciness; } set { material.Bounciness = value; } }
        public float StaticFriction { get { return material.StaticFriction; } set { material.StaticFriction = value; } }
        public float DynamicFriction { get { return material.KineticFriction; } set { material.KineticFriction = value; } }

        public Material(BEPUphysics.Materials.Material material)
        {
            this.material = material;
        }
    }
}

#endif