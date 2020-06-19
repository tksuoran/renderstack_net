using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.UI;

using example.Renderer;

namespace example.UIComponents
{
    public class MenuList : Dock
    {
        private IRenderer   renderer;
        private Frame       backgroundFrame = new Frame();
        private NinePatch   ninePatch;

        public MenuList(IRenderer renderer, Orientation orientation) : base(orientation)
        {
            this.renderer = renderer;
            this.ninePatch = new NinePatch(Style.NinePatchStyle);
        }

        public void Update()
        {
            if(
                (ninePatch.Size.X != Rect.Size.X) ||
                (ninePatch.Size.Y != Rect.Size.Y)
            )
            {
                ninePatch.Place(
                    0.0f,
                    0.0f,
                    0.0f,
                    Rect.Size.X,
                    Rect.Size.Y
                );
            }
        }

        public override void BeginPlace(Rectangle reference, Vector2 growDirection)
        {
            base.BeginPlace(reference, growDirection);
            //  \note Bypassing UpdateHierarchical()
            //  backgroundFrame.LocalToParent.SetTranslation(
            backgroundFrame.LocalToWorld.SetTranslation(
                Rect.Min.X, 
                Rect.Min.Y, 
                0.0f
            );
        }


        public override void DrawSelf(IUIContext context)
        {
            Update();

            renderer.Push();

            renderer.Requested.Mesh     = ninePatch.Mesh;
            renderer.Requested.Program  = Style.Program;
            renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;
            renderer.SetTexture("t_ninepatch", ninePatch.NinePatchStyle.Texture);
            renderer.SetFrame(backgroundFrame);

            if(Rect.Hit(context.Mouse))
            {
                renderer.Global.Floats("add_color").Set(-0.33f, -0.33f, -0.33f);
            }
            else
            {
                renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            }
            renderer.Global.Sync();

            renderer.RenderCurrent();

            renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            renderer.Global.Sync();

            renderer.SetFrame(renderer.DefaultFrame);
            renderer.Pop();
        }
    }
}
