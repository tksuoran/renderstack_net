using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class SamplerGL3 : ISampler
    {
        private int                 samplerObject;
        private TextureMinFilter    minFilter   = TextureMinFilter.Nearest;
        private TextureMagFilter    magFilter   = TextureMagFilter.Nearest;
        private TextureWrapMode     wrap        = TextureWrapMode.ClampToEdge;
        private TextureCompareMode  compareMode = TextureCompareMode.None;
        private DepthFunction       compareFunc = DepthFunction.Less;

        public TextureMinFilter     MinFilter       { get { return minFilter; } set { if(minFilter != value){ minFilter = value; } } }
        public TextureMagFilter     MagFilter       { get { return magFilter; } set { if(magFilter != value){ magFilter = value; } } }
        public TextureWrapMode      Wrap            { get { return wrap; } set { if(wrap != value){ wrap = value; } } }
        public TextureCompareMode   CompareMode     { get { return compareMode; } set { if(compareMode != value){ compareMode = value; } } }
        public DepthFunction        CompareFunc     { get { return compareFunc; } set { if(compareFunc != value){ compareFunc = value; } } }

        public SamplerGL3()
        {
            GL.GenSamplers(1, out samplerObject);
        }
        // \todo IDisposable, SamplerGhost, ~SamplerGL3

        public void Apply()
        {
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureMinFilter, (int)(MinFilter));
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureMagFilter, (int)(MagFilter));
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureCompareMode, (int)(compareMode));
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureCompareFunc, (int)(compareFunc));
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureWrapS, (int)(Wrap));
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureWrapT, (int)(Wrap));
            GL.SamplerParameter(samplerObject, SamplerParameter.TextureWrapR, (int)(Wrap));
        }
        public void Apply(int textureUnit, TextureTarget bindTarget)
        {
            //  Workarounds for AMD driver bug
            //  http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Number=298749#Post298749
            Apply();
            //GL.BindTexture(BindTarget, TextureObject);
            //GL.TexParameter(bindTarget, TextureParameterName.TextureMagFilter, (int)(MagFilter));
            GL.TexParameter(bindTarget, TextureParameterName.TextureMinFilter, (int)(MinFilter));
            //GL.TexParameter(bindTarget, TextureParameterName.TextureCompareMode, (int)(compareMode));
            //GL.TexParameter(bindTarget, TextureParameterName.TextureWrapS,     (int)(Wrap));
            //GL.TexParameter(bindTarget, TextureParameterName.TextureWrapT,     (int)(Wrap));
            //GL.TexParameter(bindTarget, TextureParameterName.TextureWrapR,     (int)(Wrap));

            //  \todo Apply() needs to be called somewhere anyway :P

            GL.BindSampler(textureUnit, samplerObject);
        }
    }
}