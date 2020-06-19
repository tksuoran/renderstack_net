using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using RenderStack.Math;

namespace example.Renderer
{
    public class TimerScope : IDisposable
    {
        private readonly Timer timer;

        public TimerScope(Timer timer)
        {
            this.timer = timer;
            timer.Begin();
        }
        public void Dispose()
        {
            timer.End();
        }
    }
    public class Timer : IDisposable
    {
        private static readonly List<Timer> timers = new List<Timer>();
        private static int                  index = 0;
        private static Timer                activeTimer;
        private static readonly HashSet<Timer> active = new HashSet<Timer>();

        public static ICollection<Timer>    Timers { get { return timers.AsReadOnly(); } }
        public static void                  BeginFrame()
        {
            foreach(var timer in timers)
            {
                timer.BeginFrameInstance();
            }
        }
        public static void                  EndFrame()
        {
            foreach(var timer in timers)
            {
                timer.EndFrameInstance();
            }

            ++index;
            if(index == 4)
            {
                index = 0;
            }
        }

        private bool[]      beginQuery = new bool[4];
        private bool[]      endQuery = new bool[4];
        private bool[]      pendingQuery = new bool[4];
        private int[]       queryObjects = new int[4];
        private Stopwatch   stopwatch;
        private int         lastIndex = -1; // nothing to wait
        private long        lastResult;
        private bool        UseGPU;
        private bool        usedGPUThisFrame = false;
        public  Vector3     Color;
        public  string      Label;
        public  long        LastResult { get { return lastResult; } }

        public override string ToString()
        {
            if(UseGPU)
            {
                return Label + " " + GPUTime.ToString("0.00") + " / " + CPUTime.ToString("0.00");
            }
            return Label + " " + CPUTime.ToString("0.00");
        }
        public float CPUTime
        {
            get
            {
                return (float)(1000.0 * (double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency);
            }
        }
        public float GPUTime
        {
            get
            {
                if(UseGPU)
                {
                    if(RenderStack.Graphics.Configuration.canUseTimerQuery && (Configuration.AMDGPUPerf == false))
                    {
                        return lastResult / 1000000.0f;
                    }
                }
                return 0.0f;
            }
        }

        public Timer(string label, double r, double g, double b, bool useGPU):this(useGPU)
        {
            Label = label;
            Color = new Vector3(r, g, b);
        }
        public Timer(string label, double r, double g, double b):this(label, r, g, b, false)
        {
        }
        public Timer():this(true)
        {
        }
        public Timer(bool useGPU)
        {
            UseGPU = useGPU;
            timers.Add(this);
            if(UseGPU)
            {
                if(RenderStack.Graphics.Configuration.canUseTimerQuery && (Configuration.AMDGPUPerf == false))
                {
                    GL.GenQueries(queryObjects.Length, queryObjects);
                }
            }
            stopwatch = new Stopwatch();
        }
        ~Timer()
        {
            Dispose();
        }
        private bool disposed = false;
        public void Dispose()
        {
            if(!disposed)
            {
                timers.Remove(this);
                // \todo fix GL object life cycle management
                //GL.DeleteQueries(queryObjects.Length, queryObjects);
            }
        }

        public void Begin()
        {
            if(UseGPU)
            {
                if(activeTimer != null)
                {
                    throw new System.Exception("!");
                }
                activeTimer = this;
                if(RenderStack.Graphics.Configuration.canUseTimerQuery && (Configuration.AMDGPUPerf == false))
                {
                    //System.Console.WriteLine("GL.BeginQuery(QueryTarget.TimeElapsed, " + queryObjects[index] + ");");
                    GL.BeginQuery(QueryTarget.TimeElapsed, queryObjects[index]);
                    active.Add(this);
                    beginQuery[index] = true;
                    usedGPUThisFrame = true;
                    lastIndex = index;
                }
            }
            stopwatch.Start();
        }
        public void End()
        {
            if(UseGPU)
            {
                if(RenderStack.Graphics.Configuration.canUseTimerQuery && (Configuration.AMDGPUPerf == false))
                {
                    if(activeTimer != this)
                    {
                        throw new System.Exception("!");
                    }
                    //System.Console.WriteLine("GL.EndQuery(QueryTarget.TimeElapsed); (" + queryObjects[index] + ")");
                    GL.EndQuery(QueryTarget.TimeElapsed);
                    active.Remove(this);
                    endQuery[index] = true;
                    pendingQuery[index] = true;
                    if(lastIndex != index)
                    {
                        throw new System.InvalidOperationException("query crossed frame");
                    }
                }
                else
                {
                    GL.Finish();
                }
                activeTimer = null;
            }
            stopwatch.Stop();
        }

        private void BeginFrameInstance()
        {
            stopwatch.Reset();
        }
        private void EndFrameInstance()
        {
            if(usedGPUThisFrame == false)
            {
                lastResult = 0;
            }
            if(UseGPU == false)
            {
                return;
            }

            //  Can not use timer queries when AMDGPUPerf is used
            //  \todo Interleave
            if(
                (RenderStack.Graphics.Configuration.canUseTimerQuery == false) || 
                (Configuration.AMDGPUPerf == true)
            )
            {
                return;
            }

            if(usedGPUThisFrame == false)
            {
                return;
            }

            int available;
             // Start polling from lastWritter + 1 eg. 3 frames behind
            int pollIndex = index + 1;
            if(pollIndex == 4) pollIndex = 0;
            for(int stepCount = 0; stepCount < 5; ++stepCount)
            {
                if(pendingQuery[pollIndex])
                {
                    bool ok = GL.IsQuery(queryObjects[pollIndex]);
                    if(ok == false)
                    {
                        bool ok0 = GL.IsQuery(queryObjects[0]);
                        bool ok1 = GL.IsQuery(queryObjects[1]);
                        bool ok2 = GL.IsQuery(queryObjects[2]);
                        bool ok3 = GL.IsQuery(queryObjects[3]);
                        bool allOk = ok0 && ok1 && ok2 && ok3;
                    }

                    GL.GetQueryObject(queryObjects[pollIndex], GetQueryObjectParam.QueryResultAvailable, out available);
                    if(available != 0)
                    {
                        long time;
                        //  \todo fix when OpenTK bindings bug is fixed
                        GL.GetQueryObjectui64(queryObjects[pollIndex], (ArbTimerQuery)GetQueryObjectParam.QueryResult, out time);
                        lastResult = time;
                        pendingQuery[pollIndex] = false;
                    }
                }
                pollIndex = (pollIndex == 3) ? 0 : pollIndex + 1;
            }

            for(int i = 0; i < 4; ++i)
            {
                beginQuery[i] = false;
                endQuery[i] = false;
            }

            //  Reset for next time
            usedGPUThisFrame = false;
        }
    }
}
