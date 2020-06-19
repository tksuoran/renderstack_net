//#define DISABLE_CACHE

using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class DepthState : RenderState
    {
        public bool             Enabled     = false;
        public DepthFunction    Function    = DepthFunction.Less;
        public float            Near        = 0.0f;
        public float            Far         = 1.0f;

        private static          DepthState @default     = new DepthState(true);
        private static          DepthState disabled     = new DepthState(false);
        private static          DepthState last         = null;
        private static readonly DepthState stateCache   = new DepthState();

        public static DepthState Default    { get { return @default; } }
        public static DepthState Disabled   { get { return disabled; } }

        public DepthState()
        {
        }
        public DepthState(bool enabled)
        {
            Enabled = enabled;
        }

        public static void ResetState()
        {
            GL.DepthFunc(DepthFunction.Less);
            stateCache.Function = DepthFunction.Less;
            GL.DepthRange(0.0f, 1.0f);
            stateCache.Near = 0.0f;
            stateCache.Far  = 1.0f;
            GL.Enable(EnableCap.DepthTest);
            stateCache.Enabled = true;
            last = null;
        }
        public override void Reset()
        {
            Enabled     = true;
            Function    = DepthFunction.Less;
            Near        = 0.0f;
            Far         = 1.0f;
        }
        public override void Execute()
        {
#if !DISABLE_CACHE
            if(last == this)
            {
                return;
            }
#endif
            if(Enabled)
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == false)
#endif
                {
                    GL.Enable(EnableCap.DepthTest);
                    stateCache.Enabled = true;
                }
#if !DISABLE_CACHE
                if(stateCache.Function != Function)
#endif
                {
                    GL.DepthFunc(Function);
                    stateCache.Function = Function;
                }
#if !DISABLE_CACHE
                if(
                    (stateCache.Near != Near) ||
                    (stateCache.Far  != Far)
                )
#endif
                {
                    GL.DepthRange(Near, Far);
                    stateCache.Near = Near;
                    stateCache.Far  = Far;
                }
            }
            else
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == true)
#endif
                {
                    GL.Disable(EnableCap.DepthTest);
                    stateCache.Enabled = false;
                }
            }
            last = this;
        }
    }
}
