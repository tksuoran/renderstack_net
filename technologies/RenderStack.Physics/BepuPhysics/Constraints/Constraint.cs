﻿//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_BEPU_PHYSICS

namespace RenderStack.Physics
{
    public abstract class Constraint
    {
        internal abstract BEPUphysics.UpdateableSystems.Updateable updateable { get; }
    }
}

#endif