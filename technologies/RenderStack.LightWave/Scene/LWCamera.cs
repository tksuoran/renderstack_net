using RenderStack.Math;

namespace RenderStack.LightWave
{
    public class LWCamera : LWItem
    {
        public int              TargetObject;
        public SFloatEnvelope   ZoomFactor = new SFloatEnvelope();
        public int              MotionBlur;
        public SFloatEnvelope   BlurLength = new SFloatEnvelope();
        public int              DepthOfField;
        public SFloatEnvelope   FocalDistance = new SFloatEnvelope();
        public SFloatEnvelope   LensFStop = new SFloatEnvelope();
        public double           ResolutionMultiplier;
        public Vector2          FrameSize;
        public Vector2          CustomSize;
        public int              Resolution;
        public int              FilmSize;
        public int              NTSCWidescreen;
        public double           PixelAspect;
        public double           ApertureHeight;
    }
}