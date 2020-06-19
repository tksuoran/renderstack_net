//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;

namespace example.Loading
{
    public class Time
    {
        static long start = Environment.TickCount;

        public static float Now 
        {
            get
            {
                long ticks = Environment.TickCount - start;
                long ms = ticks;
                float s = (float)ms / 1000.0f;
                return s;
            }
        }
    }
}