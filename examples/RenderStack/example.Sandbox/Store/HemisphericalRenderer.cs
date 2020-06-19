using OpenTK.Graphics.OpenGL;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public class HemisphericalRenderer : Service
    {
        public override string Name
        {
            get { return "HemisphericalRenderer"; }
        }

        // Services
        IRenderer               renderer;
        int                     size;

        private IFramebuffer    framebuffer;

        private Camera          camera = new Camera();

        public TextureGL Texture
        {
            get
            {
                return framebuffer[FramebufferAttachment.ColorAttachment0];
            }
        }

        public HemisphericalRenderer(int size)
        {
            this.size = size;
        }

        public void Connect(IRenderer renderer)
        {
            this.renderer = renderer;
        }

        protected override void InitializeService()
        {
            if(RenderStack.Graphics.Configuration.canUseFramebufferObject)
            {
                framebuffer = FramebufferFactory.Create(size, size);
                framebuffer.AttachTexture(
                    FramebufferAttachment.ColorAttachment0,
                    PixelFormat.Red,
                    PixelInternalFormat.Rgb8
                    //PixelInternalFormat.R8
                    //PixelInternalFormat.Rgb32f
                );
                framebuffer.AttachRenderBuffer(
                    FramebufferAttachment.DepthAttachment,
                    //PixelFormat.DepthComponent,
                    RenderbufferStorage.DepthComponent24,
                    0
                );
                framebuffer.Begin();
                framebuffer.Check();
                framebuffer.End();
            }

            camera.Projection.ProjectionType = ProjectionType.Other;
            camera.Projection.NearParameter.X = 0.01f;
        }

        public void Render(Matrix4 cameraTransform, Group renderGroup)
        {
            renderer.Requested.Viewport = framebuffer.Viewport;

            framebuffer.Begin();
            framebuffer.Check();

            //  \todo Use render state
            GL.Disable(EnableCap.PolygonOffsetFill);

            Vector3 positionInWorld = cameraTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));

            camera.Frame.Parent = null;
            camera.Frame.LocalToParent.Set(cameraTransform);
            camera.Frame.LocalToWorld.Set(cameraTransform);
            renderer.Requested.Camera = camera;
            //camera.Frame.UpdateHierarchical();

            GL.Viewport(
                (int)renderer.Requested.Viewport.X, 
                (int)renderer.Requested.Viewport.Y, 
                (int)renderer.Requested.Viewport.Width, 
                (int)renderer.Requested.Viewport.Height
            );
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            /*GL.ClearStencil(0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);*/
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Renderer.RenderGroupBasic(Renderer.RenderGroup, Renderer.Programs.Hemispherical, MeshMode.PolygonFill);
            renderer.CurrentGroup = renderGroup;
            renderer.Requested.Program  = null; // \todo fixme renderer.Programs["EqualArea"];
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.RenderGroup();

            framebuffer.End();

            Texture.GenerateMipmap();

            //  Area of disk, radius r :  Pi * r * r     =      edge * edge * 0.25 * Pi
            //  Area of texture        :                        edge * edge
            //  
            //  Hemispherical disc covers ~0.78 of the texture.
        }

        float ReadRedAverage()
        {
            if(RenderStack.Graphics.Configuration.canUseFramebufferObject)
            {
                int level = 0;
                int size = Texture.Size.Width;
                while(size > 1)
                {
                    size /= 2;
                    ++level;
                }
                framebuffer.AttachTextureLevel(FramebufferAttachment.ColorAttachment0, level);
                float[] pixels = new float[4];
                float[] depths = new float[4];
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                GL.ReadPixels<float>(0, 0, 1, 1, PixelFormat.Red, PixelType.Float, pixels);
                return pixels[0];
            }
            else
            {
                return 0.0f;
            }
        }
    }
}
