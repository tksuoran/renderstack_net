using System.Diagnostics;

using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public class Timers
    {
        public Timer Shadow     = new Timer("Shadow",   1.0, 0.0, 0.0, true);
        public Timer ID         = new Timer("ID",       1.0, 0.5, 0.0, true);
        public Timer Render3D   = new Timer("Render",   0.0, 1.0, 0.0, true);
        public Timer GUI        = new Timer("GUI",      0.0, 1.0, 1.0, true);
    }
}
