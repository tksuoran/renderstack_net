using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;
using RenderStack.UI;

namespace example.Renderer
{
    public class TextRenderer : Service, IDisposable
    {
        public override string Name
        {
            get { return "TextRenderer"; }
        }

        OpenTK.GameWindow       window;
        IRenderer               renderer;

        private Material        material;
        private TextBuffer      textBuffer;
        private Frame           frame = new Frame();
        private Viewport        viewport;
        private List<string>    debugLines = new List<string>();
        private FontStyle       fontStyle;

        public  Frame           Frame       { get { return frame; } }
        public  FontStyle       FontStyle   { get { return fontStyle; } }
        public  TextBuffer      TextBuffer  { get { return textBuffer; } }
        public  Material        Material    { get { return material; } }

        ~TextRenderer()
        {
            Dispose();
        }
        private bool disposed = false;
        public void Dispose()
        {
            if(!disposed)
            {
                //
            }
        }


        public void DebugLine(string msg)
        {
            debugLines.Add(msg);
        }
        public void DrawDebugLines()
        {
            SetupRenderer();

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
            IRenderer           renderer
        )
        {
            this.window = window;
            this.renderer = renderer;

            InitializationDependsOn(renderer);
        }

        protected override void InitializeService()
        {
            material = new Material("font", renderer.Programs["Font"], renderer.MaterialUB);
            material.BlendState                         = new BlendState();
            material.BlendState.Enabled                 = true;
            material.BlendState.RGB.EquationMode        = BlendEquationMode.FuncAdd;
            material.BlendState.RGB.DestinationFactor   = BlendingFactorDest.OneMinusSrcAlpha;
            material.BlendState.RGB.SourceFactor        = BlendingFactorSrc.One;
            material.DepthState     = DepthState.Disabled;
            material.FaceCullState  = FaceCullState.Disabled;

            fontStyle   = new FontStyle("res/fonts/small.fnt"); 
            textBuffer  = new TextBuffer(fontStyle);
            viewport    = new Viewport(window.Width, window.Height);
        }

        public void SetupRenderer()
        {
            renderer.Requested.Mesh     = TextBuffer.Mesh;
            renderer.Requested.Material = material;
            renderer.Requested.Program  = material.Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            renderer.Global.Floats("alpha").Set(1.0f);
            renderer.Global.Sync();
            renderer.SetTexture("t_font", fontStyle.Texture);
        }
        public void Message(string message)
        {
            SetupRenderer();

            renderer.SetFrame(frame);

            TextBuffer.Print(0.0f, 10.0f, 0.0f, message);
            Rectangle bounds = TextBuffer.FontStyle.Bounds;

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
        }
    }
}








#if false

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