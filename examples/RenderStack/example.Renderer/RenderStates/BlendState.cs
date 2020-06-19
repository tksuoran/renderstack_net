// #define DISABLE_CACHE

using RenderStack.Math;

using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class BlendStateComponent
    {
        public BlendEquationMode   EquationMode         = BlendEquationMode.FuncAdd;
        public BlendingFactorSrc   SourceFactor         = BlendingFactorSrc.One;
        public BlendingFactorDest  DestinationFactor    = BlendingFactorDest.One;

        public void Reset()
        {
            EquationMode         = BlendEquationMode.FuncAdd;
            SourceFactor         = BlendingFactorSrc.One;
            DestinationFactor    = BlendingFactorDest.One;
        }
    }
    public class BlendState : RenderState
    {
        public bool                 Enabled = false;
        public BlendStateComponent  RGB     = new BlendStateComponent();
        public BlendStateComponent  Alpha   = new BlendStateComponent();
        public Vector4              Color   = Vector4.Zero;

        private static          BlendState  last        = null;
        private static          BlendState  @default    = new BlendState();
        private static readonly BlendState  stateCache  = new BlendState();

        public static BlendState Default { get { return @default; } }

        public static void ResetState()
        {
            GL.BlendColor(0.0f, 0.0f, 0.0f, 0.0f);
            stateCache.Color = Vector4.Zero;
            GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            stateCache.RGB.EquationMode = BlendEquationMode.FuncAdd;
            stateCache.Alpha.EquationMode = BlendEquationMode.FuncAdd;
            GL.BlendFuncSeparate(
                BlendingFactorSrc.One,
                BlendingFactorDest.One,
                BlendingFactorSrc.One,
                BlendingFactorDest.One
            );
            stateCache.RGB.SourceFactor        = BlendingFactorSrc.One;
            stateCache.RGB.DestinationFactor   = BlendingFactorDest.One;
            stateCache.Alpha.SourceFactor      = BlendingFactorSrc.One;
            stateCache.Alpha.DestinationFactor = BlendingFactorDest.One;
            GL.Disable(EnableCap.Blend);
            stateCache.Enabled = false;
            last = null;
        }
        public override void Reset()
        {
            Enabled = false;
            RGB     = new BlendStateComponent();
            Alpha   = new BlendStateComponent();
            Color   = Vector4.Zero;
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
                    GL.Enable(EnableCap.Blend);
                    stateCache.Enabled = true;
                }
#if !DISABLE_CACHE
                if(stateCache.Color != Color)
#endif
                {
                    GL.BlendColor(
                        Color.X, 
                        Color.Y, 
                        Color.Z, 
                        Color.W
                    );
                    stateCache.Color = Color;
                }
#if !DISABLE_CACHE
                if(
                    (stateCache.RGB.EquationMode != RGB.EquationMode) ||
                    (stateCache.Alpha.EquationMode != Alpha.EquationMode)
                )
#endif
                {
                    GL.BlendEquationSeparate(RGB.EquationMode, Alpha.EquationMode);
                    stateCache.RGB.EquationMode = RGB.EquationMode;
                    stateCache.Alpha.EquationMode = Alpha.EquationMode;
                }
#if !DISABLE_CACHE
                if(
                    (stateCache.RGB.SourceFactor         != RGB.SourceFactor) ||
                    (stateCache.RGB.DestinationFactor    != RGB.DestinationFactor) ||
                    (stateCache.Alpha.SourceFactor       != Alpha.SourceFactor) ||
                    (stateCache.Alpha.DestinationFactor  != Alpha.DestinationFactor)
                )
#endif
                {
                    GL.BlendFuncSeparate(
                        RGB.SourceFactor, 
                        RGB.DestinationFactor,
                        Alpha.SourceFactor,
                        Alpha.DestinationFactor
                    );
                    stateCache.RGB.SourceFactor         = RGB.SourceFactor;
                    stateCache.RGB.DestinationFactor    = RGB.DestinationFactor;
                    stateCache.Alpha.SourceFactor       = Alpha.SourceFactor;
                    stateCache.Alpha.DestinationFactor  = Alpha.DestinationFactor;
                }
            }
            else
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == true)
#endif
                {
                    GL.Disable(EnableCap.Blend);
                    stateCache.Enabled = false;
                }
            }
            last = this;
        }
    }
}
