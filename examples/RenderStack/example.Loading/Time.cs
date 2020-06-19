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