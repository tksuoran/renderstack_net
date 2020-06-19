//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Management;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

#if ASSET_MONITOR
using RenderStack.AssetMonitor;
#endif

namespace Sandbox
{
    public partial class Application
    {
        private float   AverageCpuUsage     = 0.0f;
        private long    AverageFrameTicks   = 0;
        private int     updateCounter       = 0;
        private long    lastRenderTime      = -1;
        private string  frameTime           = "";
        private long    lastUpdate          = -1;
        private long    frameUpdateCount    = 10;
        private int gc0 = 0;
        private int gc1 = 0;
        private int gc2 = 0;

        public void UpdateStatistics()
        {
            long now = System.Environment.TickCount;

            if(lastRenderTime != -1)
            {
                /*base.Title = 
                      " GC0 " + GC.CollectionCount(0)
                    + " GC1 " + GC.CollectionCount(1)
                    + " GC2 " + GC.CollectionCount(2)
                    + " GC3 " + GC.CollectionCount(3);*/

                long frameTimeTicks = now - lastRenderTime;
                AverageFrameTicks += frameTimeTicks;

#if MEASURE_CPU
                AverageCpuUsage += CPUUsage;
#endif
                ++updateCounter;
                if(updateCounter == frameUpdateCount)
                {
#if MEASURE_CPU
                    int cpu = (int)(AverageCpuUsage / updateCounter);
                    int megabytesInUse = (int)(MemoryUsage / (1024.0f * 1024.0f));
#endif
                    //AverageFrameTicks /= 100;
                    float mspf = (float)(AverageFrameTicks) / (float)(frameUpdateCount);
                    frameTime = mspf.ToString("0.00");

                    int newGc0 = GC.CollectionCount(0);
                    int newGc1 = GC.CollectionCount(1);
                    int newGc2 = GC.CollectionCount(2);
#if false
                    if(
                        (newGc0 != gc0) ||
                        (newGc1 != gc1) ||
                        (newGc2 != gc2) 
                    )
                    {
                        base.Title = 
                              " GC0 " + GC.CollectionCount(0)
                            + " GC1 " + GC.CollectionCount(1)
                            + " GC2 " + GC.CollectionCount(2);
                        gc0 = GC.CollectionCount(0);
                        gc1 = GC.CollectionCount(1);
                        gc2 = GC.CollectionCount(2);
                    }
#endif

#if false
                    base.Title = 
                        title 
#if MEASURE_CPU
                        + " CPU Use " + cpu.ToString() 
                        + "% Mem Use " + megabytesInUse.ToString() + " MB"
#endif
                        + " GC0 " + GC.CollectionCount(0)
                        + " GC1 " + GC.CollectionCount(1)
                        + " GC2 " + GC.CollectionCount(2)
                        ;
#endif
                    AverageCpuUsage = 0.0f;
                    AverageFrameTicks = 0;
                    updateCounter = 0;
                }

            }
            lastRenderTime = now;
        }

        public void PerformFixedUpdates()
        {
            long current = System.Environment.TickCount;
            if(lastUpdate == -1)
            {
                UpdateFixed();
                lastUpdate = current;
            }
            else
            {
                if(lastUpdate >= current)
                {
                    //updateMsg = "OK!";
                    return;
                }
                while(lastUpdate < current)
                {
                    UpdateFixed();
                    lastUpdate += 10;
                }
            }
        }

        private void UpdateFixed()
        {
            if(userInterfaceManager != null)
            {
                userInterfaceManager.UpdateControls();
            }
            if(Configuration.physics)
            {
                sceneManager.UpdatePhysics();
            }
        }

        private void UpdateOncePerFrame()
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }

#if ASSET_MONITOR
            if(AssetMonitor.Instance != null)
            {
                AssetMonitor.Instance.Update();
            }
#endif

#if DEMO
            DemoUpdate();
#else

            if(sounds != null)
            {
                sounds.PlayQueue();
            }
#endif
        }

#if false
        protected override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            UpdateOnePerFrame();
        }
#endif
    }
}
