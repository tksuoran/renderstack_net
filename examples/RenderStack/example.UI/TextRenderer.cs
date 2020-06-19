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

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

namespace example.UI
{
    public class TextRenderer : Service
    {
        public override string Name
        {
            get { return "TextRenderer"; }
        }

        OpenTK.GameWindow       window;
        Renderer                renderer;

        private TextBuffer      textBuffer;
        private Material        material;
        private Frame           frame = new Frame();
        private Viewport        viewport;
        private Camera          camera;
        private List<string>    debugLines = new List<string>();

        public  Frame           Frame           { get { return frame; } }
        public  TextBuffer      TextBuffer      { get { return textBuffer; } }
        public  Camera          Camera          { get { return camera; } }

        public void DebugLine(string msg)
        {
            debugLines.Add(msg);
        }

        public void DrawDebugLines()
        {
            int lineHeight = (int)Math.Ceiling(Style.Default.Font.LineHeight);
            int y = window.Height - lineHeight - 2;
            TextBuffer.BeginPrint();
            for(int i = 0; i < debugLines.Count; ++i)
            {
                if(debugLines[i] == null)
                {
                    continue;
                }
                TextBuffer.LowPrint(
                    10.0f, 
                    (float)y,
                    0.0f, 
                    debugLines[i]
                );
                y -= lineHeight;
            }
            TextBuffer.EndPrint();
            renderer.RenderCurrent();
            debugLines.Clear();
        }

        public void Connect(
            OpenTK.GameWindow   window,
            Renderer            renderer
        )
        {
            this.window = window;
            this.renderer = renderer;

            InitializationDependsOn(renderer);
        }


        private void InitializeStyles()
        {
            FontStyle       fontStyle           = new FontStyle("res/fonts/small.fnt"); 
            Vector2         padding             = new Vector2(6.0f, 6.0f); 
            Vector2         innerPadding        = new Vector2(2.0f, 2.0f);
            NinePatchStyle  ninePatchStyle      = new NinePatchStyle("ninepatch6.png");
            NinePatchStyle  foreNinePatchStyle  = new NinePatchStyle("ninepatch7.png");

            Style.Background = new Style(
                padding, 
                innerPadding, 
                fontStyle, 
                ninePatchStyle, 
                material
            );
            Style.Foreground = new Style(
                padding, 
                innerPadding, 
                fontStyle, 
                foreNinePatchStyle, 
                material
            );
            Style.Default = Style.Background;
        }

        protected override void InitializeService()
        {
            //  InitializeStyles() needs material
            material = new Material(
                "textured", 
                renderer.Programs["Textured"], 
                MeshMode.PolygonFill
            );

            InitializeStyles();

            // Style.Default is set in InitializeStyles()
            material.Parameters["texture"] = Style.Default.Font.Texture;

            this.textBuffer = new TextBuffer(Style.Default.Font);

            viewport = new Viewport(window.Width, window.Height);
            camera = new Camera();
            camera.ProjectionType = ProjectionType.OrthogonalRectangle;
            camera.OrthoLeft      = 0;
            camera.OrthoTop       = 0;
            camera.OrthoWidth     = window.Width;
            camera.OrthoHeight    = window.Height;
            camera.Near           = -1000.0f;
            camera.Far            =  1000.0f;
            camera.UpdateFrame();
            camera.UpdateViewport(viewport);

            Begin();
            Message("Loading...");
        }

        private void SetupRendererRequest()
        {
            renderer.Requested.Camera   = camera;
            renderer.Requested.Model    = null;
            renderer.Requested.Frame    = frame;
            renderer.Requested.Mesh     = TextBuffer.Mesh;
            renderer.Requested.Material = material;
            renderer.Requested.Program  = renderer.Programs["Textured"];
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
        }

        public void Begin()
        {
            camera.OrthoWidth     = window.Width;
            camera.OrthoHeight    = window.Height;
            camera.UpdateFrame();
            camera.UpdateViewport(viewport);

            renderer.PartialGLStateResetToDefaults();

            SetupRendererRequest();

            (renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 0.0f);
            (renderer.GlobalParameters["alpha"           ] as Floats).Set(1.0f);

            GL.Disable  (EnableCap.DepthTest);
            GL.Disable  (EnableCap.CullFace);
            GL.Enable   (EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }
        public void End()
        {
            (renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 0.0f);
        }
        public void Message(string message)
        {
            Begin();

            //TextBuffer.BeginPrint();
            TextBuffer.Print(0.0f, 10.0f, 0.0f, message);
            Rectangle bounds = TextBuffer.FontStyle.Bounds;
            //TextBuffer.EndPrint();

            // NOTE: Using fractional coordinates would be a bad idea
            frame.LocalToParent.SetTranslation(
                (float)(int)(window.Width  / 2 - bounds.Size.X / 2),
                (float)(int)(window.Height / 2 - bounds.Size.Y / 2),
                0.0f
            );

            OpenTK.VSyncMode oldSync = window.VSync;
            window.VSync = OpenTK.VSyncMode.Off;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            for(int i = 0; i < 2; ++i)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
                renderer.RenderCurrent();

                window.SwapBuffers();
            }
            window.VSync = oldSync;

            End();
        }
    }
}








#if false





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

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

namespace example.UI
{
    public class TextRenderer : Service
    {
        public override string Name
        {
            get { return "TextRenderer"; }
        }

        OpenTK.GameWindow   window;
        Renderer            renderer;

        private TextBuffer  textBuffer;
        private Material    material;
        private Frame       frame = new Frame();

        public  Frame       Frame           { get { return frame; } }
        public  TextBuffer  TextBuffer      { get { return textBuffer; } }
        public  Camera      Camera          { get { return camera; } }

        private Viewport    viewport;
        private Camera      camera;

        private List<string>        debugLines = new List<string>();
        public void DebugLine(string msg)
        {
            debugLines.Add(msg);
        }

        public void DrawDebugLines()
        {
            int lineHeight = (int)Math.Ceiling(Style.Default.Font.LineHeight);
            int y = window.Height - lineHeight - 2;
            for(int i = 0; i < debugLines.Count; ++i)
            {
                if(debugLines[i] == null)
                {
                    continue;
                }
                TextBuffer.Print(
                    10.0f, 
                    (float)y,
                    0.0f, 
                    debugLines[i]
                );
                y -= lineHeight;
                renderer.RenderCurrent();
            }
            debugLines.Clear();
        }

        public void Connect(
            OpenTK.GameWindow   window,
            Renderer            renderer
        )
        {
            this.window = window;
            this.renderer = renderer;

            InitializationDependsOn(renderer);
        }

        private void InitializeStyles()
        {
            FontStyle       fontStyle           = new FontStyle("res/fonts/small.fnt"); 
            Vector2         padding             = new Vector2(6.0f, 6.0f); 
            Vector2         innerPadding        = new Vector2(2.0f, 2.0f);
            NinePatchStyle  ninePatchStyle      = new NinePatchStyle("ninepatch6.png");
            NinePatchStyle  foreNinePatchStyle  = new NinePatchStyle("ninepatch7.png");

            Style.Background = new Style(
                padding, 
                innerPadding, 
                fontStyle, 
                ninePatchStyle, 
                material
            );
            Style.Foreground = new Style(
                padding, 
                innerPadding, 
                fontStyle, 
                foreNinePatchStyle, 
                material
            );
            Style.Default = Style.Background;
        }

        protected override void InitializeService()
        {
            //  InitializeStyles() needs material
            material = new Material(renderer.Programs["Textured"], MeshMode.PolygonFill);

            InitializeStyles();

            // Style.Default is set in InitializeStyles()
            material.Parameters["texture"] = Style.Default.Font.Texture;

            this.textBuffer = new TextBuffer(Style.Default.Font);

            viewport = new Viewport(window.Width, window.Height);
            camera = new Camera();
            camera.ProjectionType = ProjectionType.OrthogonalRectangle;
            camera.OrthoLeft      = 0;
            camera.OrthoTop       = 0;
            camera.OrthoWidth     = window.Width;
            camera.OrthoHeight    = window.Height;
            camera.Near           = -1000.0f;
            camera.Far            =  1000.0f;
            camera.UpdateCameraFrame();
            camera.UpdateViewport(viewport);

            Begin();
            Message("Loading...");
        }

        public void Begin()
        {
            camera.OrthoWidth     = window.Width;
            camera.OrthoHeight    = window.Height;
            camera.UpdateCameraFrame();
            camera.UpdateViewport(viewport);

            renderer.PartialGLStateResetToDefaults();

            renderer.CurrentCamera      = camera;
            renderer.CurrentProgram     = renderer.Programs["Textured"];
            renderer.CurrentMesh        = TextBuffer.Mesh;
            renderer.CurrentFrame       = frame;
            renderer.CurrentMaterial    = material;
            (renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 0.0f);
            (renderer.GlobalParameters["alpha"           ] as Floats).Set(1.0f);

            GL.Disable  (EnableCap.DepthTest);
            GL.Disable  (EnableCap.CullFace);
            GL.Enable   (EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }
        public void End()
        {
            (renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 0.0f);
        }
        public void Message(string message)
        {
            Begin();

            Rectangle bounds;

            TextBuffer.Print(0.0f, 10.0f, 0.0f, message, out bounds);

            // NOTE: Using fractional coordinates would be a bad idea
            frame.LocalToParent.SetTranslation(
                (float)(int)(window.Width  / 2 - bounds.Size.X / 2),
                (float)(int)(window.Height / 2 - bounds.Size.Y / 2),
                0.0f
            );

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            for(int i = 0; i < 2; ++i)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
                renderer.RenderCurrent();

                window.SwapBuffers();
            }

            End();
        }
    }
}

#endif