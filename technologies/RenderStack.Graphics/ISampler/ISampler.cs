using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public interface ISampler
    {
        TextureMinFilter    MinFilter   { get; set; }
        TextureMagFilter    MagFilter   { get; set; }
        TextureWrapMode     Wrap        { get; set; }
        TextureCompareMode  CompareMode { get; set; }
        DepthFunction       CompareFunc { get; set; }

        void Apply(int textureUnit, TextureTarget bindTarget);
    }
    public class SamplerFactory
    {
        public static ISampler Create()
        {
            if(Configuration.useGl1)
            {
                return (ISampler)new SamplerGL1();
            }
            return (ISampler)new SamplerGL3();
        }
    }
}