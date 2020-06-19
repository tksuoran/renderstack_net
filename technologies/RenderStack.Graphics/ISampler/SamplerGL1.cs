using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class SamplerGL1 : ISampler
    {
        private TextureMinFilter    minFilter   = TextureMinFilter.Nearest;
        private TextureMagFilter    magFilter   = TextureMagFilter.Nearest;
        private TextureWrapMode     wrap        = TextureWrapMode.ClampToEdge;
        private TextureCompareMode  compareMode = TextureCompareMode.None;
        private DepthFunction       compareFunc = DepthFunction.Less;

        public TextureMinFilter     MinFilter   { get { return minFilter; } set { if(minFilter != value){ minFilter = value; } } }
        public TextureMagFilter     MagFilter   { get { return magFilter; } set { if(magFilter != value){ magFilter = value; } } }
        public TextureWrapMode      Wrap        { get { return wrap; } set { if(wrap != value){ wrap = value; } } }
        public TextureCompareMode   CompareMode { get { return compareMode; } set { if(compareMode != value){ compareMode = value; } } }
        public DepthFunction        CompareFunc { get { return compareFunc; } set { if(compareFunc != value){ compareFunc = value; } } }

        public void Apply()
        {
        }
        public void Apply(int textureUnit, TextureTarget bindTarget)
        {
            GL.TexParameter(bindTarget, TextureParameterName.TextureMinFilter,  (int)(MinFilter));
            GL.TexParameter(bindTarget, TextureParameterName.TextureMagFilter,  (int)(MagFilter));
            GL.TexParameter(bindTarget, TextureParameterName.TextureWrapS,      (int)(Wrap));
            GL.TexParameter(bindTarget, TextureParameterName.TextureWrapT,      (int)(Wrap));
            GL.TexParameter(bindTarget, TextureParameterName.TextureWrapR,      (int)(Wrap));
        }
    }
}