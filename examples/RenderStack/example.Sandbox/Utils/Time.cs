//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Sandbox
{
    public class Time
    {
        public class Scope : IDisposable
        {
            string  message;
            float   start;
            float   end;

            public Scope()
            {
                start = Time.Now;
            }

            public Scope(string msg)
            {
                start = Time.Now;
                message = msg;
            }

            #region IDisposable Members

            //bool disposed = false;
            void IDisposable.Dispose()
            {
                end = Time.Now;
                float length = end - start;
                Trace.TraceInformation(
                    ((message != null) ? message : "") + " " + length
                );
            }

            #endregion
        }

        public int      StartTimeMS;
        public int      EndTimeMS;
        public int      TimeMS;
        public double   TimeS;
        public int      FrameCount;
        public int      LastFPSUpdateFrameCount;
        public int      LastFPSUpdateTimeMS;
        public int      FixedUpdateTimeMS;
        public int      FixedUpdateTimeDeltaMS; 

        public Time()
        {
        }

        static long start = 0;

        public static void Initialize()
        {
            start = Environment.TickCount;
        }

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

        public static int CurrentTime()
        {
            long ticks = Environment.TickCount;
            long ms = ticks;

            return (int)(ms);
        }

        public void Reset()
        {
            Update();
            FixedUpdateTimeMS = TimeMS;
            StartTimeMS = TimeMS;
            LastFPSUpdateTimeMS = TimeMS;
            LastFPSUpdateFrameCount = 0;
        }

        public void Update()
        {
            TimeMS = CurrentTime();
            TimeS = (float)(TimeMS) / 1000.0;
        }

        private void UpdateFPS()
        {
            int timeSinceLastFPSUpdateMS = TimeMS - LastFPSUpdateTimeMS;
            ++FrameCount;
            ++LastFPSUpdateFrameCount;
            if(timeSinceLastFPSUpdateMS > 1000)
            {
                double fps = 1000.0 * (double)LastFPSUpdateFrameCount / (double)timeSinceLastFPSUpdateMS;
                double mspf = 1000.0f / fps;

                LastFPSUpdateFrameCount = 0;
                LastFPSUpdateTimeMS = TimeMS;
            }
        }

    }
}
