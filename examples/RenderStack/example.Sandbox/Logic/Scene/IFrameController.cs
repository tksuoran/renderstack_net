using RenderStack.Math;
using RenderStack.Scene;

using example.Renderer;

namespace example.Sandbox
{
    public interface IFrameController : IUpdateFixedStep, IUpdateOncePerFrame
    {
        Frame       Frame           { get; set; }
        Controller  RotateX         { get; }
        Controller  RotateY         { get; }
        Controller  RotateZ         { get; }
        Controller  TranslateX      { get; }
        Controller  TranslateY      { get; }
        Controller  TranslateZ      { get; }
        Controller  SpeedModifier   { get; } 

        void        SetTransform(Matrix4 transform);
        void        Clear();
    }
    public interface IPhysicsController
    {
        IPhysicsObject PhysicsObject { get; set; }
    }
}
