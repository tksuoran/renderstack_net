using System;

using RenderStack.Graphics;
using RenderStack.Scene;

namespace example.Renderer
{
    public interface IRenderer : RenderStack.Services.IService
    {
        void Connect(OpenTK.GameWindow window);

        int             Width           { get; }
        int             Height          { get; }
        event EventHandler<EventArgs> Resize;

        State           Requested       { get; }
        Group           CurrentGroup    { get; set; }

        Programs        Programs        { get; }
        Frame           DefaultFrame    { get; }
        IUniformBlock   MaterialUB      { get; }
        IUniformBuffer  Global          { get; }
        IUniformBuffer  Models          { get; }
        Timers          Timers          { get; }
        bool            LockMaterial    { get; set; }

        void HandleResize();
        void Unload();
        void BindAttributesAndCheckForUpdates();
        void SetTexture(string samplerName, TextureGL texture);
        void UnbindTexture(string samplerName, TextureGL texture);
        void SetFrame(Frame frame);
        void Push();
        void Pop();
        void ApplyViewport();
        void UpdateCamera();
        void RenderGroupInstances(Instances instances);
        void RenderGroup();
        void RenderCurrentDebug();
        void RenderCurrent();
        void RenderCurrent(int instanceCount);
        void RenderInstancedPrepare();
        void RenderCurrentClear();
        void BeginScissorMouse(int px, int py);
        void EndScissorMouse();
        void PartialGLStateResetToDefaults();
    }
    public class RendererFactory
    {
        public static IRenderer Create()
        {
#if true
            if(RenderStack.Graphics.Configuration.useGl1)
            {
                return new RendererGL1() as IRenderer;
            }
            return new RendererGL3() as IRenderer;
#else
            return new RendererRL() as IRenderer;
#endif
        }
    }
}
