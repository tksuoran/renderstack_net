//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using RenderStack.Math;

namespace example.Brushes
{
    [System.Serializable]
    public class InertiaData
    {
        public Matrix4  inertia;
        public float    Mass;

        public InertiaData()
        {
        }

        public Matrix4 Inertia
        {
            get
            {
                return inertia;
            }
        }
        public InertiaData(
            Matrix4 inertia,
            float   mass
        )
        {
            this.inertia = inertia;
            this.Mass = mass;
        }
    }
}