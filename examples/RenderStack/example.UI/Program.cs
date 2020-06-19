//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;

using Matrix4 = RenderStack.Math.Matrix4;
using Vector3 = RenderStack.Math.Vector3;

namespace example.UI
{
    public partial class Application : GameWindow
    {
        private Viewport windowViewport;

        public Application(OpenTK.DisplayDevice display)
        :   base(640, 480, new GraphicsMode(), "example.UI", 0, display, 
            2, 1, GraphicsContextFlags.Default
            //  In releases, ask for 3.3 only if you really need it.
            //  Test forward compatible during development, but never ship with it set
            //3, 3, GraphicsContextFlags.ForwardCompatible
        )
        {
            windowViewport = new Viewport(640, 480);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            //  Check GL version and feature capabilities
            RenderStack.Graphics.Configuration.Initialize();
            RenderStack.Graphics.Configuration.useVertexArrayObject = true;

            RenderStack.Graphics.Debug.WriteLine("----- OnLoad -----");

            //  Enable line below for low end shaders - Intel integrated
            RenderStack.Graphics.ProgramDeprecated.OverrideShaders = "res/shaders000/";

            VSync = VSyncMode.Off;

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();
            this.Visible = true;

            InitializeServices();

            RenderStack.Graphics.Debug.WriteLine("----- OnLoad End -----");
            RenderStack.Graphics.Debug.FrameTerminator();

            Resize += new EventHandler<EventArgs>(Application_Resize);
            Unload += new EventHandler<EventArgs>(Application_Unload);
        }

        void Application_Unload(object sender, EventArgs e)
        {
#if false
            GhostManager.Process();
#if DEBUG
            GhostManager.CheckAllDeleted();
#endif
#endif
        }

        void Application_Resize(object sender, EventArgs e)
        {
            windowViewport.Resize(this.Width, this.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            PerformFixedUpdates();
            UpdateOncePerFrame();

            renderer.Requested.Camera      = sceneManager.Camera;
            renderer.Requested.Viewport    = windowViewport;

            renderer.PartialGLStateResetToDefaults();

            renderer.RenderCurrentClear();

            GL.Viewport(
                renderer.Requested.Viewport.X,
                renderer.Requested.Viewport.Y,
                renderer.Requested.Viewport.Width,
                renderer.Requested.Viewport.Height
            );
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);

            renderer.CurrentGroup = sceneManager.RenderGroup;
            renderer.RenderGroup();

            Render2D();

            SwapBuffers();

            GhostManager.Process();

            RenderStack.Graphics.Debug.FrameTerminator();
        }

        private long lastUpdate = -1;
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
            userInterfaceManager.UpdateControls();
        }

        private float   AverageCpuUsage     = 0.0f;
        private long    AverageFrameTicks   = 0;
        private int     updateCounter       = 0;
        private long    lastRenderTime      = -1;
        private string  frameTime           = "";
        //private long    lastUpdate          = -1;
        private long    frameUpdateCount    = 10;
        private int gc0 = 0;
        private int gc1 = 0;
        private int gc2 = 0;

        private void UpdateOncePerFrame()
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
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

        [STAThread]
        public static void Main()
        {
            OpenTK.DisplayDevice chosenDisplay = OpenTK.DisplayDevice.Default;

            using(var application = new Application(chosenDisplay))
            {
                application.Run(30);
            }
        }
    }
}