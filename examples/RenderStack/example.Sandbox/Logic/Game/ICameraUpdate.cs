using RenderStack.Scene;

namespace example.Sandbox
{
    interface ICameraUpdate
    {
        Camera Camera { get; set; }
        Unit   Unit   { get; set; }
        void Reset();
        void Update();
    }
}