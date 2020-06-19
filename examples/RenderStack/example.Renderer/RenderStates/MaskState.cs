//#define DISABLE_CACHE

using OpenTK.Graphics.OpenGL;

namespace example.Renderer

{
    public class MaskState : RenderState
    {
        public bool    Red      = true;
        public bool    Green    = true;
        public bool    Blue     = true;
        public bool    Alpha    = true;
        public bool    Depth    = true;

        private static MaskState @default   = new MaskState();
        private static MaskState last       = null;
        private static MaskState stateCache = new MaskState();

        public static MaskState Default { get { return @default; } }

        public static void ResetState()
        {
            GL.ColorMask(true, true, true, true);
            stateCache.Red    = true;
            stateCache.Green  = true;
            stateCache.Blue   = true;
            stateCache.Alpha  = true;
            last = null;
        }
        public override void Reset()
        {
            Red      = true;
            Green    = true;
            Blue     = true;
            Alpha    = true;
            Depth    = true;
        }
        public override void Execute()
        {
#if !DISABLE_CACHE
            if(last == this)
            {
                return;
            }
            if(
                (stateCache.Red   != Red  ) ||
                (stateCache.Green != Green) ||
                (stateCache.Blue  != Blue ) ||
                (stateCache.Alpha != Alpha)
            )
#endif
            {
                GL.ColorMask(Red, Green, Blue, Alpha);
                stateCache.Red    = Red;
                stateCache.Green  = Green;
                stateCache.Blue   = Blue;
                stateCache.Alpha  = Alpha;
            }
#if !DISABLE_CACHE
            if(stateCache.Depth != Depth)
#endif
            {
                GL.DepthMask(Depth);
                stateCache.Depth = Depth;
            }
            last = this;
        }
    }
}
