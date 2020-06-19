using RenderStack.Math;

using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class Clear : RenderState
    {
        // TODO Add integer clears
        public ClearBufferMask  ClearBufferMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit;
        public Vector4          ClearColor4     = Vector4.Zero;
        public float            ClearDepth      = 1.0f;

        private static readonly Clear stateCache = new Clear();

        public static void ResetState()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            stateCache.ClearColor4 = Vector4.Zero;
            GL.ClearDepth(1.0f);
            stateCache.ClearDepth = 1.0f;
        }
        public override void Reset()
        {
            ClearBufferMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit;
            ClearColor4     = Vector4.Zero;
            ClearDepth      = 1.0f;
        }
        public override void Execute()
        {
            if(ClearBufferMask == 0)
            {
                return;
            }
            if(stateCache.ClearColor4 != ClearColor4)
            {
                GL.ClearColor(
                    ClearColor4.X,
                    ClearColor4.Y,
                    ClearColor4.Z,
                    ClearColor4.W
                );
                stateCache.ClearColor4 = ClearColor4;
            }
            if(stateCache.ClearDepth != ClearDepth)
            {
                GL.ClearDepth(ClearDepth);
                stateCache.ClearDepth = ClearDepth;
            }
            GL.Clear(ClearBufferMask);
        }
    }
}
