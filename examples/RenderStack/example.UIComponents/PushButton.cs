using RenderStack.UI;

using example.Renderer;

namespace example.UIComponents
{
    public class PushButton : Button
    {
        public bool Pressed = false;

        public PushButton(IRenderer renderer, string label):base(renderer,label)
        {
            this.Style = Style.Foreground;
            Name = label;
        }

        public override void DrawSelf(IUIContext context)
        {
            Renderer.Push();

            //  First draw ninepatch
            Renderer.Requested.Program  = Style.Background.Program;
            Renderer.Requested.Mesh     = NinePatch.Mesh;
            Renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;
            Renderer.SetTexture("t_ninepatch", NinePatch.NinePatchStyle.Texture);
            Renderer.SetFrame(BackgroundFrame);

            if(Rect.Hit(context.Mouse))
            {
                if(context.MouseButtons[(int)(OpenTK.Input.MouseButton.Left)])
                {
                    if(Pressed)
                    {
                        Renderer.Global.Floats("add_color").Set(0.3f, 0.3f, 0.7f);
                    }
                    else
                    {
                        Renderer.Global.Floats("add_color").Set(0.6f, 0.6f, 0.8f);
                    }
                    Trigger = true;
                }
                else
                {
                    if(Trigger)
                    {
                        if(Action != null)
                        {
                            Action(this);
                        }
                        else
                        {
                            Pressed = !Pressed;
                        }
                        Trigger = false;
                    }
                    if(Pressed)
                    {
                        Renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 1.0f);
                    }
                    else
                    {
                        Renderer.Global.Floats("add_color").Set(0.72f, 0.72f, 0.72f);
                    }
                }
            }
            else
            {
                Trigger = false;
                if(Pressed)
                {
                    Renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 1.0f);
                }
                else
                {
                    Renderer.Global.Floats("add_color").Set(0.5f, 0.5f, 0.5f);
                }
            }
            Renderer.Global.Sync();
            Renderer.RenderCurrent();
            Renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);

            //  Then draw text
            if(Style.Font != null)
            {
                Renderer.Requested.Program  = Style.Program;
                Renderer.Requested.Mesh     = TextBuffer.Mesh;
                Renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;
                Renderer.SetFrame(TextFrame);
                Renderer.RenderCurrent();
            }

            Renderer.SetFrame(Renderer.DefaultFrame);
            Renderer.Pop();
        }
    }
}
