//#define DISABLE_CACHE

using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class StencilStateComponent
    {
        public StencilOp        StencilFailOp   = StencilOp.Keep;
        public StencilOp        ZFailOp         = StencilOp.Keep;
        public StencilOp        ZPassOp         = StencilOp.Keep;
        public StencilFunction  Function        = StencilFunction.Always;
        public int              Reference       = 0;
        public int              TestMask        = 0xffff;
        public int              WriteMask       = 0xffff;

        public void Apply(StencilFace face, StencilStateComponent cache)
        {
#if !DISABLE_CACHE
            if(
                (cache.StencilFailOp != StencilFailOp) ||
                (cache.ZFailOp       != ZFailOp) ||
                (cache.ZPassOp       != ZPassOp)
            )
#endif
            {
                GL.StencilOpSeparate(face, StencilFailOp, ZFailOp, ZPassOp);
                cache.StencilFailOp = StencilFailOp;
                cache.ZFailOp       = ZFailOp;
                cache.ZPassOp       = ZPassOp;
            }
#if !DISABLE_CACHE
            if(cache.WriteMask != WriteMask)
#endif
            {
                GL.StencilMaskSeparate(face, WriteMask);
                cache.WriteMask = WriteMask;
            }
#if !DISABLE_CACHE
            if(
                (cache.Function  != Function)  ||
                (cache.Reference != Reference) ||
                (cache.TestMask  != TestMask) 
            )
#endif
            {
                GL.StencilFuncSeparate((Version20)face, Function, Reference, TestMask);
                cache.Function  = Function;
                cache.Reference = Reference;
                cache.TestMask  = TestMask;
            }
        }
        public void ApplyShared(StencilState cache)
        {
#if !DISABLE_CACHE
            if(
                (cache.Front.StencilFailOp != StencilFailOp)  ||
                (cache.Front.ZFailOp       != ZFailOp) ||
                (cache.Front.ZPassOp       != ZPassOp) ||
                (cache.Back.StencilFailOp  != StencilFailOp)  ||
                (cache.Back.ZFailOp        != ZFailOp) ||
                (cache.Back.ZPassOp        != ZPassOp)
            )
#endif
            {
                GL.StencilOp(StencilFailOp, ZFailOp, ZPassOp);
                cache.Front.StencilFailOp   = cache.Back.StencilFailOp  = StencilFailOp;
                cache.Front.ZFailOp         = cache.Back.ZFailOp        = ZFailOp;
                cache.Front.ZPassOp         = cache.Back.ZPassOp        = ZPassOp;
            }

#if !DISABLE_CACHE
            if(
                (cache.Front.WriteMask != WriteMask) ||
                (cache.Back.WriteMask  != WriteMask)
            )
#endif
            {
                GL.StencilMask(WriteMask);
                cache.Front.WriteMask = cache.Back.WriteMask = WriteMask;
            }

#if !DISABLE_CACHE
            if(
                (cache.Front.Function  != Function)  ||
                (cache.Front.Reference != Reference) ||
                (cache.Front.TestMask  != TestMask)  ||
                (cache.Back.Function   != Function)  ||
                (cache.Back.Reference  != Reference) ||
                (cache.Back.TestMask   != TestMask)
            )
#endif
            {
                GL.StencilFunc(Function, Reference, TestMask);
                cache.Front.Function  = cache.Back.Function  = Function;
                cache.Front.Reference = cache.Back.Reference = Reference;
                cache.Front.TestMask  = cache.Back.TestMask  = TestMask;
            }
        }
    }
    public class StencilState : RenderState
    {
        public bool                  Enabled  = false;
        public bool                  Separate = false;
        public StencilStateComponent Front    = new StencilStateComponent();
        public StencilStateComponent Back     = new StencilStateComponent();

        private static          StencilState last       = null;
        private static          StencilState @default   = new StencilState();
        private static readonly StencilState stateCache = new StencilState();

        public static StencilState Default { get { return @default; } }

        public static void ResetState()
        {
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
            stateCache.Front.StencilFailOp  = stateCache.Back.StencilFailOp = StencilOp.Keep;
            stateCache.Front.ZFailOp        = stateCache.Back.ZFailOp       = StencilOp.Keep;
            stateCache.Front.ZPassOp        = stateCache.Back.ZPassOp       = StencilOp.Keep;

            GL.StencilMask(0xffff);
            stateCache.Front.WriteMask = stateCache.Back.WriteMask = 0xffff;

            GL.StencilFunc(StencilFunction.Always, 0, 0xffff);
            stateCache.Front.Function  = stateCache.Back.Function  = StencilFunction.Always;
            stateCache.Front.Reference = stateCache.Back.Reference = 0;
            stateCache.Front.TestMask  = stateCache.Back.TestMask  = 0xffff;

            last = null;
        }
        public override void Reset()
        {
            Separate            = false;
            Enabled             = false;

            Front.StencilFailOp = Back.StencilFailOp = StencilOp.Keep;
            Front.ZFailOp       = Back.ZFailOp       = StencilOp.Keep;
            Front.ZPassOp       = Back.ZPassOp       = StencilOp.Keep;

            Front.WriteMask = Back.WriteMask = 0xffff;

            Front.Function  = Back.Function  = StencilFunction.Always;
            Front.Reference = Back.Reference = 0;
            Front.TestMask  = Back.TestMask  = 0xffff;
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
                    GL.Enable(EnableCap.StencilTest);
                    stateCache.Enabled = true;
                }
                if(Separate)
                {
                    Front.Apply(StencilFace.Front, stateCache.Front);
                    Back.Apply(StencilFace.Back, stateCache.Back);
                    stateCache.Separate = true;
                }
                else
                {
#if !DISABLE_CACHE
                    if(stateCache.Separate == false)
                    {
                        //  Cache already in shared state
                        Front.Apply(StencilFace.FrontAndBack, stateCache.Front);
                    }
                    else
#endif
                    {
                        //  Cache not yet in shared state - make it shared
                        Front.ApplyShared(stateCache);
                        stateCache.Separate = false;
                    }
                }
            }
            else
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == true)
#endif
                {
                    GL.Disable(EnableCap.StencilTest);
                    stateCache.Enabled = false;
                }
            }
            last = this;
        }
    }
}
