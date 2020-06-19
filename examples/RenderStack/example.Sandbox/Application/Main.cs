using System.Diagnostics;
using example.Renderer;

namespace example.Sandbox
{
    public partial class Application : OpenTK.GameWindow
    {
        //[STAThread]
        public static void Main(string[] args)
        {
            ParseArgs(args);

            using(AMDGPUPerf pref = new AMDGPUPerf())
            {

#if CATCH
            try
#endif
                {
                    if(Configuration.trace)
                    {
                        TraceInfo();
                    }
                    for(int i = 0; i < 1; ++i)
                    {
                        var device = OpenTK.DisplayDevice.Default;
                        float rate = device.RefreshRate;
                        Trace.TraceInformation("Refresh rate:   " + rate);
                        using(var simple = new Application(device))
                        {
                            simple.Title = title = "RenderStack Sandbox " + AssemblyUtils.RetrieveLinkerTimestamp().ToString() + " Timo Suoranta";
                            Time.Initialize();

                            simple.Run(1, rate);
                        }
                    }
                    //System.GC.Collect();
                    //System.Threading.Thread.Sleep(60000);
                }
#if CATCH
                catch(OpenTK.GraphicsException e)
                {
                    Trace.TraceError("Caught GraphicsException");
                    Trace.TraceError("OpenGL error:\n" + e.ToString());
                    StopMessage(
                        "OpenGL error has occurred.\n"
                        + "Please report this to tksuoran@gmail.com\n"
                        + "You can press CTRL-C to copy text from this window.\n"
                        + "Thank you.\n\n"
                        + e.ToString(),
                        "Neure Sandbox - OpenGL Error"
                    );
                }
                catch(Sorry e)
                {
                    Trace.TraceError("Caught Sorry");
                    ExclamationMessage(
                        "Your graphics driver does not seem to support recent OpenGL.\n"
                        + "Missing OpenGL features:\n"
                        + e.Message + "\n"
                        + "You can try to update your driver and try again.\n",
                        "Neure Sandbox - Unsupported OpenGL version"
                    );
                }
                catch(System.Exception e)
                {
                    Trace.TraceError("Caught Exception");
                    Trace.TraceError("OpenGL error:\n" + e.ToString());
                    StopMessage(
                        "An error has occurred.\n"
                        + "Please report this to tksuoran@gmail.com\n"
                        + "You can press CTRL-C to copy text from this window.\n"
                        + "Thank you.\n\n"
                        + e.ToString(),
                        "Neure Sandbox - Generic Error"
                    );
                }
#endif
            }
        }
    }
}
