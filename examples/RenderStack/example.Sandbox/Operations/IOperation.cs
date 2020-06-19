//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

namespace example.Sandbox
{
    public interface IOperation
    {
        void Execute(Application sandbox);
        void Undo(Application sandbox);
    }
}