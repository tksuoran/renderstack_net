// #define DISABLE_CACHE

using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class FaceCullState : RenderState
    {
        public bool                 Enabled             = true;
        public CullFaceMode         CullFaceMode        = CullFaceMode.Back;
        public FrontFaceDirection   FrontFaceDirection  = FrontFaceDirection.Ccw;

        private static          FaceCullState @default   = new FaceCullState();
        private static          FaceCullState disabled   = new FaceCullState(false);
        private static          FaceCullState last       = null;
        private static readonly FaceCullState stateCache = new FaceCullState();

        public static FaceCullState Default     { get { return @default; } }
        public static FaceCullState Disabled    { get { return disabled; } }

        public FaceCullState()
        {
        }
        public FaceCullState(bool enabled)
        {
            Enabled = enabled;
        }

        public static void ResetState()
        {
            GL.CullFace(CullFaceMode.Back);
            stateCache.CullFaceMode = CullFaceMode.Back;
            GL.FrontFace(FrontFaceDirection.Ccw);
            stateCache.FrontFaceDirection = FrontFaceDirection.Ccw;
            GL.Disable(EnableCap.CullFace);
            stateCache.Enabled = false;
            last = null;
        }
        public override void Reset()
        {
            Enabled             = true;
            CullFaceMode        = CullFaceMode.Back;
            FrontFaceDirection  = FrontFaceDirection.Ccw;
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
                    GL.Enable(EnableCap.CullFace);
                    stateCache.Enabled = true;
                }
#if !DISABLE_CACHE
                if(stateCache.CullFaceMode != CullFaceMode)
#endif
                {
                    GL.CullFace(CullFaceMode);
                    stateCache.CullFaceMode = CullFaceMode;
                }
#if !DISABLE_CACHE
                if(stateCache.FrontFaceDirection != FrontFaceDirection)
#endif
                {
                    GL.FrontFace(FrontFaceDirection);
                    stateCache.FrontFaceDirection = FrontFaceDirection;
                }
            }
            else
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == true)
#endif
                {
                    GL.Disable(EnableCap.CullFace);
                    stateCache.Enabled = false;
                }
            }
            last = this;
        }
    }
}
