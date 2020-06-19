#define EXTRA_DEBUG

using System.Diagnostics;
using RenderStack.Graphics;

namespace example.Sandbox
{
    public partial class Application
    {
        private bool                    render;
        private System.Threading.Thread renderThread;
        //private long                    frameCounter = 0;
        private Stopwatch               interFrameStopwatch = new Stopwatch();

        protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            HighLevelRenderer highLevelRenderer = RenderStack.Services.BaseServices.Get<HighLevelRenderer>();
            highLevelRenderer.BeginFrame();

            interFrameStopwatch.Stop();
            long elapsed = interFrameStopwatch.ElapsedTicks;
            interFrameStopwatch.Reset();
            interFrameStopwatch.Start();
            UserInterfaceManager ui = RenderStack.Services.BaseServices.Get<UserInterfaceManager>();
            if(ui != null)
            {
                ui.InterFrameTime = (float)(1000.0 * elapsed / Stopwatch.Frequency);
            }
            UpdateManager updateManager = RenderStack.Services.BaseServices.Get<UpdateManager>();
            updateManager.PerformFixedUpdates();
            if (Configuration.threadedRendering == false)
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
            var updateManager = RenderStack.Services.BaseServices.Get<UpdateManager>();
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

            var highLevelRenderer = RenderStack.Services.BaseServices.Get<HighLevelRenderer>();
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