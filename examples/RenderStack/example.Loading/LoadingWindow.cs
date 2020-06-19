//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Loading
{
    public class LoadingWindow : OpenTK.GameWindow, ILoadingWindow
    {
        private object syncFormVisible = new object();
        private float startTime;
        private float t;

        private float start = 0.0f;
        private float end = 1.0f;
        private float expectedTime = 20.0f;

        public object SyncFormVisible { get { return syncFormVisible; } }

        public void Span(float expectedTime, float start, float end)
        {
            this.start = start;
            this.end = end;
            this.expectedTime = expectedTime;
            startTime = Time.Now;
        }

        void ILoadingWindow.Run()
        {
            Run(60, 0);
        }
        void ILoadingWindow.Close()
        {
            base.Close();
        }

        public LoadingWindow()
        :   base(
            500, 50,
            new GraphicsMode(
                new ColorFormat(0, 0, 1, 0), // r g b a
                0, 0, 0
            ),
            "Loading...", 
            OpenTK.GameWindowFlags.Default,
            OpenTK.DisplayDevice.Default, 
            2, 
            1, 
            GraphicsContextFlags.Default
        )
        {
            this.Visible = true;
            this.VSync = OpenTK.VSyncMode.On;
        }

        public void Message(string message)
        {
            Title = "Loading... " + message;
        }

        protected override void  OnLoad(EventArgs e)
        {
            GL.ClearColor   (0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear        (ClearBufferMask.ColorBufferBit);
            SwapBuffers();
            GL.Enable(EnableCap.ScissorTest);
            startTime = Time.Now;
            lock(syncFormVisible)
            {
                System.Threading.Monitor.Pulse(syncFormVisible);
            }
            this.Visible = true;
        }

        protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            if(expectedTime > 0.0)
            {
                float position = Time.Now - startTime;
                float relativeToExpected = position / expectedTime;
                float newT = start + (end - start) * relativeToExpected;
                if(newT > t)
                {
                    t = start + (end - start) * relativeToExpected;
                }
            }
            int w = (int)((float)Width * t);
            if(w >= Width)
            {
                GL.Disable      (EnableCap.ScissorTest);
                GL.ClearColor   (0.0f, 0.0f, 1.0f, 0.0f);
                GL.Clear        (ClearBufferMask.ColorBufferBit);
                SwapBuffers();
                return;
            }
            GL.ClearColor   (0.0f, 0.0f, 1.0f, 0.0f);
            GL.Scissor      (0, 0, w, Height);
            GL.Clear        (ClearBufferMask.ColorBufferBit);
            GL.ClearColor   (0.0f, 0.0f, 0.0f, 0.0f);
            GL.Scissor      (w, 0, Width - w, Height);
            GL.Clear        (ClearBufferMask.ColorBufferBit);
            SwapBuffers();
        }
    }
}
