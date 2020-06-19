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

namespace example.Scene
{
    public partial class Application : GameWindow
    {
        private Viewport windowViewport;

        public Application(OpenTK.DisplayDevice display)
        :   base(640, 480, new GraphicsMode(), "RenderStack", 0, display, 
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

            //  Enable line below for low end shaders - Intel integrated
            //RenderStack.Graphics.Program.OverrideShaders = "res/shaders000/";

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();

            InitializeServices();
            InstallInputEventHandlers();

            renderer.Requested.Camera   = sceneManager.Camera;
            renderer.Requested.Viewport = windowViewport;
            renderer.CurrentGroup       = sceneManager.RenderGroup;
            renderer.BindGroup();

            Resize += new EventHandler<EventArgs>(Application_Resize);
            Unload += new EventHandler<EventArgs>(Application_Unload);
        }

        void Application_Unload(object sender, EventArgs e)
        {
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

        public void UpdateControls()
        {
            GameWindow window = this;
            float wheelNow = window.Mouse.WheelPrecise;
            float wheelDelta = wheel - wheelNow;
            wheel = wheelNow;
            FrameController cameraControls = sceneManager.CameraControls;
            cameraControls.RotateY.Adjust(-MouseXDelta / 2000.0f);
            cameraControls.RotateX.Adjust(-MouseYDelta / 2000.0f);
            cameraControls.TranslateZ.Adjust(wheelDelta * 0.2f);
            mouseXDelta = 0;
            mouseYDelta = 0;

            cameraControls.TranslateX.Inhibit   = !window.Focused;
            cameraControls.TranslateY.Inhibit   = !window.Focused;
            cameraControls.TranslateZ.Inhibit   = !window.Focused;
            cameraControls.RotateX.Inhibit      = !window.Focused;
            cameraControls.RotateY.Inhibit      = !window.Focused;
            cameraControls.RotateZ.Inhibit      = !window.Focused;
            cameraControls.TranslateX.Less      = window.Keyboard[OpenTK.Input.Key.A];
            cameraControls.TranslateX.More      = window.Keyboard[OpenTK.Input.Key.D];
            cameraControls.TranslateY.Less      = window.Keyboard[OpenTK.Input.Key.F];
            cameraControls.TranslateY.More      = window.Keyboard[OpenTK.Input.Key.R];
            cameraControls.TranslateZ.Less      = window.Keyboard[OpenTK.Input.Key.W];
            cameraControls.TranslateZ.More      = window.Keyboard[OpenTK.Input.Key.S];
            cameraControls.RotateX.Less         = window.Keyboard[OpenTK.Input.Key.Z];
            cameraControls.RotateX.More         = window.Keyboard[OpenTK.Input.Key.X];
            cameraControls.RotateY.Less         = window.Keyboard[OpenTK.Input.Key.V];
            cameraControls.RotateY.More         = window.Keyboard[OpenTK.Input.Key.C];
            cameraControls.RotateZ.Less         = window.Keyboard[OpenTK.Input.Key.E];
            cameraControls.RotateZ.More         = window.Keyboard[OpenTK.Input.Key.Q];

            cameraControls.FixedUpdate();
            cameraControls.AfterFixedUpdates();
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

            SwapBuffers();

            GhostManager.Process();
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
            UpdateControls();
        }

        private void UpdateOncePerFrame()
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        [STAThread]
        public static void Main()
        {
            OpenTK.DisplayDevice chosenDisplay = OpenTK.DisplayDevice.Default;

            using (var application = new Application(chosenDisplay))
            {
                application.Title = "example.Scene";

                application.Run(30);
            }
        }
    }
}