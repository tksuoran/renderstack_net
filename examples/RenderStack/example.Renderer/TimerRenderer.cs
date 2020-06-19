using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;
using example.Renderer;

namespace example.Sandbox
{
    public class TimerRenderer : Service
    {
        public override string Name
        {
            get { return "TimerRenderer"; }
        }

        QuadRenderer        quadRenderer;
        Renderer.IRenderer  renderer;
        TextRenderer        textRenderer;
        OpenTK.GameWindow   window;

        public void Connect(
            Renderer.IRenderer  renderer,
            TextRenderer        textRenderer,
            OpenTK.GameWindow   window
        )
        {
            this.renderer = renderer;
            this.textRenderer = textRenderer;
            this.window = window;
        }
        protected override void InitializeService()
        {
            quadRenderer = new QuadRenderer(renderer);
        }
        public void Render()
        {
            renderer.Push();

            quadRenderer.Begin();
            Vector3 o = new Vector3(10.0f, 64, 0.0f);
            Vector3 a = o;
            float alpha = 0.50f;

            textRenderer.TextBuffer.BeginPrint();

            foreach(var timer in Timer.Timers)
            {
                quadRenderer.Quad(
                    o, 
                    o + new Vector3(10.0f * timer.CPUTime, 5.0f, 0.0f),
                    new Vector4(timer.Color, alpha)
                );
                o.Y += 6.0f;
                quadRenderer.Quad(
                    o, 
                    o + new Vector3(10.0f * timer.GPUTime, 5.0f, 0.0f),
                    new Vector4(timer.Color, alpha)
                );
                textRenderer.TextBuffer.LowPrint(
                    o.X, 
                    o.Y - 4.0f,
                    0.0f, 
                    timer.ToString()
                );

                o.Y += 6.0f;
            }
            quadRenderer.Quad(
                new Vector3(10.0f + 159.0f, a.Y, 0.0f),
                new Vector3(10.0f + 161.0f, o.Y, 0.0f),
                new Vector4(1.0f, 1.0f, 1.0f, alpha)
            );
            quadRenderer.End();

            textRenderer.TextBuffer.EndPrint();
            renderer.Requested.Mesh     = textRenderer.TextBuffer.Mesh;
            renderer.Requested.Material = textRenderer.Material;
            renderer.Requested.Program  = textRenderer.Material.Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            //renderer.SetTexture("t_font", textRenderer.TextBuffer.FontStyle.Texture);
            renderer.SetFrame(renderer.DefaultFrame);
            renderer.RenderCurrent();

            renderer.Global.Floats("alpha").Set(0.5f);
            renderer.Global.Sync();

            renderer.Requested.Mesh     = quadRenderer.Mesh;
            // \todo
            //renderer.Requested.Material = uiMaterial; //  this has blending on  
            renderer.Requested.Program  = renderer.Programs["ColorFill"]; //colorFill;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetFrame(renderer.DefaultFrame);
            renderer.RenderCurrent();

            renderer.Pop();
        }
    }
}