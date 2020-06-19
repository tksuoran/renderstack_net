using System;

using RenderStack.Services;

namespace example.Sandbox
{
    public class Statistics : Service
    {
        public override string Name
        {
            get { return "Statistics"; }
        }

        private long    AverageFrameTicks   = 0;
        private int     updateCounter       = 0;
        private long    lastRenderTime      = -1;
        private readonly long frameUpdateCount    = 10;
#if false
        private float   AverageCpuUsage     = 0.0f;
        private int gc0 = 0;
        private int gc1 = 0;
        private int gc2 = 0;
#endif

        protected override void InitializeService()
        {
        }

        public float InterFrameTime;
        public float FrameTimeValue { get; private set; } = 0.0f;
        public string FrameTime { get; private set; } = "";

        public void Update()
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
                    FrameTimeValue = (float)(AverageFrameTicks) / (float)(frameUpdateCount);
                    FrameTime = FrameTimeValue.ToString("0.00");

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
                    AverageCpuUsage = 0.0f;
#endif
                    AverageFrameTicks = 0;
                    updateCounter = 0;
                }

            }
            lastRenderTime = now;
        }
    }

}
