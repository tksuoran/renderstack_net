//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#define EXTRA_DEBUG

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.Mesh;

using Buffer = RenderStack.Graphics.BufferGL;
using Debug = RenderStack.Graphics.Debug;

namespace example.Sandbox
{
    public partial class Application
    {
        private bool                    render;
        private System.Threading.Thread renderThread;
        //private long                    frameCounter = 0;
        private Stopwatch               interFrameStopwatch = new Stopwatch();

        protected override void OnRenderFrame(OpenTK.FrameEventArgs ea)
        {
            var highLevelRenderer = Services.Get<HighLevelRenderer>();
            highLevelRenderer.BeginFrame();

            interFrameStopwatch.Stop();
            long elapsed = interFrameStopwatch.ElapsedTicks;
            interFrameStopwatch.Reset();
            interFrameStopwatch.Start();
            var ui = Services.Get<UserInterfaceManager>();
            if(ui != null)
            {
                ui.InterFrameTime = (float)(1000.0 * (double)elapsed / (double)Stopwatch.Frequency);
            }
            var updateManager = Services.Get<UpdateManager>();
            updateManager.PerformFixedUpdates();
            if(Configuration.threadedRendering == false)
            {
                Render();
            }

            highLevelRenderer.EndFrame();
        }
        private void RenderThread()
        {
            long i = 0;
            Context.MakeCurrent(WindowInfo);
            for(; render == true; ++i)
            {
                Render();
            }
        }
        protected void StartRenderThread()
        {
            Context.MakeCurrent(null);
            render = true;
            renderThread = new System.Threading.Thread(RenderThread);
            renderThread.Start();
        }
        protected void Render()
        {
            var updateManager = Services.Get<UpdateManager>();
            GhostManager.Process();
            updateManager.UpdateOncePerFrame();
            GhostManager.Process();
#if false
            var statistics = Services.Get<Statistics>();
            if(statistics != null)
            {
                statistics.Update();
            }
#endif

            GhostManager.Process();

            var highLevelRenderer = Services.Get<HighLevelRenderer>();
#if CATCH
            try
#endif
            {
                highLevelRenderer.Render();
            }
#if CATCH
            catch(OpenTK.GraphicsException e)
            {
                Trace.TraceError("Caught GraphicsException");
                Trace.TraceError("OpenGL error:\n" + e.ToString());
                StopMessage(
                    "OpenGL error has occured.\n"
                    + "Please report this to tksuoran@gmail.com\n"
                    + "You can press CTRL-C to copy text from this window.\n"
                    + "Thank you.\n\n"
                    + e.ToString(),
                    "RenderStack - OpenGL Error"
                );
                Close();
            }
            catch(System.Exception e)
            {
                Trace.TraceError("Caught Exception");
                Trace.TraceError("OpenGL error:\n" + e.ToString());
                StopMessage(
                    "Error has occured.\n"
                    + "Please report this to tksuoran@gmail.com\n"
                    + "You can press CTRL-C to copy text from this window.\n"
                    + "Thank you.\n\n"
                    + e.ToString(),
                    "RenderStack - Generic Error"
                );
                Close();
            }
#endif

            GhostManager.Process();
        }
    }
}