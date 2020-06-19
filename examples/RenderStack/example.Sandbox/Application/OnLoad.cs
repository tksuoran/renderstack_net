//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;

using example.Loading;
using example.Renderer;

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Sandbox
{
    public partial class Application
    {
        public LoadingScreenManager Loader;

        private bool LoadingFailed = false;
#if CATCH
        private Exception LoadingException;
#endif

        private void Center()
        {
            int width   = 3 * device.Bounds.Width / 4;
            int height  = 3 * device.Bounds.Height / 4;
            int x       = device.Bounds.Left + (device.Bounds.Width - width) / 2;
            int y       = device.Bounds.Top + (device.Bounds.Height - height) / 2;

            Bounds = new System.Drawing.Rectangle(x, y, width, height);
        }

        private static void MyNotify(ErrorCode code, string message)
        {
            System.Console.WriteLine(code.ToString() + " : " + message);
        }
        protected override void OnLoad(System.EventArgs e)
        {
            if(Configuration.loadingWindow)
            {
                Loader = new LoadingScreenManager();
            }
            long begin = System.Environment.TickCount;

            //  Check GL version and feature capabilities
            RenderStack.Graphics.Configuration.Initialize();
            RenderStack.Graphics.Configuration.useBinaryShaders = Configuration.useBinaryShaders;

            if(Configuration.forceGL1)
            {
                RenderStack.Graphics.Configuration.glslVersion = 0;
                RenderStack.Graphics.Configuration.glVersion = 140;
                RenderStack.Graphics.Configuration.useGl1 = true;
                RenderStack.Graphics.Configuration.canUseBaseVertex = false;
                RenderStack.Graphics.Configuration.canUseBinaryShaders = false;
                RenderStack.Graphics.Configuration.canUseFloatTextures = false;
                RenderStack.Graphics.Configuration.canUseGeometryShaders = false;
                RenderStack.Graphics.Configuration.canUseInstancing = false;
                RenderStack.Graphics.Configuration.canUseTesselationShaders = false;
                RenderStack.Graphics.Configuration.canUseUniformBufferObject = false;
            }

            RenderStack.Graphics.Configuration.ProgramSearchPathRL = "res/OpenRL/";
            if(RenderStack.Graphics.Configuration.glslVersion < 330)
            {
                RenderStack.Graphics.Configuration.ProgramSearchPathGL = "res/OpenGL1/";
            }
            else
            {
                RenderStack.Graphics.Configuration.ProgramSearchPathGL = "res/OpenGL3/";
            }

            var sb = new System.Text.StringBuilder();
            if(RenderStack.Graphics.Configuration.canUseFramebufferObject == false)
            {
                sb.AppendLine("framebuffer objects not supported (GL version 3.0 or GL_ARB_framebuffer_object extension)");
            }
            if(RenderStack.Graphics.Configuration.canUseInstancing == false)
            {
                sb.AppendLine("instancing not supported (GL version 3.0 or GL_ARB_draw_instanced extension)");
            }
            if(RenderStack.Graphics.Configuration.canUseBaseVertex == false)
            {
                sb.AppendLine("base vertex not supported (GL version 3.2 or GL_ARB_draw_elements_base_vertex extension)");
            }
            if(RenderStack.Graphics.Configuration.canUseTextureArrays == false)
            {
                sb.AppendLine("base vertex not supported (GL version 3.0 or GL_EXT_texture_array extension)");
            }
            if(RenderStack.Graphics.Configuration.canUseUniformBufferObject == false)
            {
                sb.AppendLine("uniform buffers not supported (GL version 3.0 or GL_ARB_uniform_buffer_object extension)");
            }
            if(sb.Length > 0)
            {
                RenderStack.Graphics.Configuration.useGl1 = true;
                Console.WriteLine("Using OpenGL 1.1 path:\n" + sb.ToString());
                //throw new Sorry(sb.ToString());
            }

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();

            AMDGPUPerf.Instance.OpenContext();

            this.Visible = !Configuration.loadingWindow;;

            if(!System.IO.Directory.Exists("data"))
            {
                System.IO.Directory.CreateDirectory("data");
            }

            if(Loader != null) Loader.Prepare();
#if CATCH
            try
#endif
            {
                Services.Instance.Initialize(this);
                Center();
                Resize += new EventHandler<EventArgs>(Application_Resize);
                Unload += new EventHandler<EventArgs>(Application_Unload);

                /*  This kicks UI  */
                var ui = Services.Get<UserInterfaceManager>();
                ui.Reset();

                System.GC.Collect();
            }
#if CATCH
            catch(Exception exception)
            {
                LoadingFailed = true;
                LoadingException = exception;
            }
            finally
            {
                Loader.Finish(LoadingFailed);
            }
            if(LoadingFailed == false)
            {
                this.Visible = true;
                Time.Initialize();
            }
            else
            {
                StopMessage("Sorry - Loading failed\n" + LoadingException.ToString(), "RenderStack Sandbox");
                Close();
            }
#else
            if(Loader != null) Loader.Finish(LoadingFailed);
            this.Visible = true;
            Time.Initialize();
#endif

            if(Configuration.threadedRendering)
            {
                StartRenderThread();
            }

            //VSync = OpenTK.VSyncMode.Off;

            long end = System.Environment.TickCount;
            long duration = end - begin;
            Console.WriteLine("Shader compilation time:         " + ShaderGL3.compileTime.Elapsed.ToString());
            //Console.WriteLine("Program link time:               " + Program.LinkTime.Elapsed.ToString());
            //Console.WriteLine("Program binary time:             " + Program.BinaryTime.Elapsed.ToString());
            System.Diagnostics.Trace.WriteLine("OnLoad() took " + duration.ToString() + " ticks");
            interFrameStopwatch.Start();
        }
    }
}

            /*
                Triangulate(
                Truncate(
                Flat(
                Flat(
                Truncate(
                Triangulate(
                Flat(
                Triangulate(
                Flat(
                Truncate(
                    Cube
                ))))))))))
*/

