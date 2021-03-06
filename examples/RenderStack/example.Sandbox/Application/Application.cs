﻿using System.Diagnostics;
using OpenTK.Graphics;

namespace example.Sandbox
{
    /*  Comment: Highly experimental  */ 
    public partial class Application : OpenTK.GameWindow
    {
        private OpenTK.DisplayDevice    device;
        private static string           title;

        public Application(OpenTK.DisplayDevice device)
        :   base(
            1920 / 2,
            64,
            //(int)(display.Width * 0.85f), 
            //(int)(display.Height * 0.85f),
            new GraphicsMode(
                new ColorFormat(8, 8, 8, 0), // r g b a
                24, 8, 2 /* depth stencil msaa  */ 
            ),
            "RenderStack", 
            OpenTK.GameWindowFlags.Default,
            device, 
            1, 5, 0, null
#if false
            Configuration.forceGL2 ? 2 : 3, 
            Configuration.forceGL2 ? 1 : 3, 
            (Configuration.forceGL2 ? 0 : GraphicsContextFlags.ForwardCompatible)
#if DEBUG 
            | (Configuration.forceGL2 ? 0 : GraphicsContextFlags.Debug)
#endif
            //| GraphicsContextFlags.Embedded
#endif
        )
        {
            Visible = !Configuration.loadingWindow;;
            this.device = device;
            VSync = Configuration.vsync ? OpenTK.VSyncMode.On : OpenTK.VSyncMode.Off;

            if(Configuration.performanceMonitoring)
            {
                try
                {
                    performance = new Performance();
                }
                catch(System.Exception)
                {
                    Trace.TraceError("Warning: No RAM / CPU counters");
                }
            }
        }
    }
}
