using OpenTK.Graphics.OpenGL;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

using example.Renderer;

namespace example.UIComponents
{
    public class ColorPicker : Area
    {
        // Services
        //FramebufferManager  framebufferManager;
        MaterialManager     materialManager;
        IRenderer           renderer;

        private Model       disc;
        private Model       triangle;
        private NinePatch   ninePatch;
        private float       aSize   = 80.0f;
        private Camera      camera  = new Camera();
        private Group       group   = new Group("ColorPicker");
        private Material    hsv;
        private Material    hsv2;

        public ColorPicker(
            //FramebufferManager framebufferManager,
            MaterialManager materialManager,
            IRenderer       renderer
        )
        {
            GeometryMesh discMesh = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Disc(1.0, 0.8, 32, 2),
                NormalStyle.PolygonNormals
            );
            GeometryMesh triangleMesh = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Triangle(0.8f / 0.57735027f),
                NormalStyle.PolygonNormals
            );

            hsv  = materialManager["HSVColorFill"];
            hsv2 = materialManager["HSVColorFill2"];

            disc     = new Model("disc",     discMesh,     hsv,  0.0f, 0.0f, 0.0f);
            triangle = new Model("triangle", triangleMesh, hsv2, 0.0f, 0.0f, 0.0f);

            group.Add(disc);
            group.Add(triangle);

            //this.framebufferManager = framebufferManager;
            this.materialManager = materialManager;
            this.renderer = renderer;

            this.ninePatch = new NinePatch(Style.NinePatchStyle);

            FillBasePixels = new Vector2(aSize, aSize);
        }

#if false
        public void Animate()
        {
            Vector2 cellSize    = new Vector2(aSize, aSize);
            Vector2 halfCell    = cellSize * 0.5f;
            Vector2 origo       = new Vector2(Rect.Min.X, Rect.Min.Y) + halfCell;
            float   scale       = (aSize - 10.0f) / 2.0f;

            float t = 0.1f * Time.Now;
            float h = t - (float)System.Math.Floor(t);
            float angle = (h - 0.25f) * 2.0f * (float)System.Math.PI;
            Vector3 center = new Vector3(origo, 0.0f);
            disc.Frame.LocalToParent.Set(
                Matrix4.CreateTranslation(center.X, center.Y, center.Z) 
                * Matrix4.CreateScale(scale)
                * Matrix4.CreateRotation(angle, -Vector3.UnitZ)
            );
            triangle.Frame.LocalToParent.Set(
                Matrix4.CreateTranslation(center.X, center.Y, center.Z) 
                * Matrix4.CreateScale(scale)
                * Matrix4.CreateRotation(0.5f * (float)System.Math.PI, Vector3.UnitZ)
            );

            hsv.Floats("t").Set(h);
            hsv.Sync();
        }
#endif

        public override void DrawSelf(IUIContext context)
        {
            /*  First draw ninepatch  */ 
#if false
            (Style.Material.Parameters.Values["texture"] as IParameterValue<Texture>).Value = ninePatch.NinePatchStyle.Texture;
            ninePatch.Place(Rect.Min.X, Rect.Min.Y, 0.0f, Rect.Max.X - Rect.Min.X, Rect.Max.Y - Rect.Min.Y);
            renderer.CurrentMesh = ninePatch.Mesh;
            renderer.RenderCurrent();

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
#endif
            renderer.Push();

            if(Rect.Hit(context.Mouse))
            {
                if(context.MouseButtons[(int)(OpenTK.Input.MouseButton.Left)])
                {
                    //
                }
            }

            //Animate();

            renderer.CurrentGroup = group;

            GL.Disable(EnableCap.Blend);
            GL.Scissor(
                (int)(Rect.Min.X), 
                (int)(Rect.Min.Y), 
                (int)(Rect.Size.X), 
                (int)(Rect.Size.Y)
            );
            GL.Enable(EnableCap.ScissorTest);

            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.RenderGroup();

            GL.Disable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.Blend);

            renderer.Pop();
        }

    }
}
