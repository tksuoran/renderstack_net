﻿using System.Collections.Generic;
using System.Text;

namespace RenderStack.Graphics
{
    public class Samplers
    {
        public static Samplers              Global = new Samplers();

        private Dictionary<string, Uniform> samplerDictionary = new Dictionary<string, Uniform>();
        private StringBuilder               sb = new StringBuilder();
        private List<Uniform>               samplers = new List<Uniform>();
        private TextureGL[]                 textures;

        //public Texture[]                    Textures { get { return textures; } }   //  Currently bound textures
        public List<Uniform>                SamplerUniforms { get { return samplers; } }

        public Uniform Sampler(string key)
        {
            return samplerDictionary[key];
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public void Seal()
        {
            textures = new TextureGL[samplerDictionary.Count];
        }

        private Uniform Add(string typeString, string name, OpenTK.Graphics.OpenGL.ActiveUniformType type, ISampler sampler)
        {
            var uniform = new Uniform(name, -1, 1, type);
            uniform.Sampler = sampler;
            sb.Append("uniform ").Append(typeString).Append(" ").Append(name).Append(";\n");
            uniform.TextureUnitIndex = samplers.Count;
            samplers.Add(uniform);
            samplerDictionary[name] = uniform;
            return uniform;
        }
        public Uniform AddSampler1D(string name, ISampler sampler)
        {
            return Add("sampler1D", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler1D, sampler);
        }
        public Uniform AddSampler1DArray(string name, ISampler sampler)
        {
            return Add("sampler1DArray", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler1DArray, sampler);
        }
        public Uniform AddSampler1DArrayShadow(string name, ISampler sampler)
        {
            return Add("sampler1DArrayShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler1DArrayShadow, sampler);
        }
        public Uniform AddSampler1DShadow(string name, ISampler sampler)
        {
            return Add("sampler1DShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler1DShadow, sampler);
        }
        public Uniform AddSampler2D(string name, ISampler sampler)
        {
            return Add("sampler2D", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2D, sampler);
        }
        public Uniform AddSampler2DArray(string name, ISampler sampler)
        {
            return Add("sampler2DArray", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DArray, sampler);
        }
        public Uniform AddSampler2DArrayShadow(string name, ISampler sampler)
        {
            return Add("sampler2DArrayShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DArrayShadow, sampler);
        }
        public Uniform AddSampler2DMultisample(string name, ISampler sampler)
        {
            return Add("sampler2DMultisample", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DMultisample, sampler);
        }
        public Uniform AddSampler2DMultisampleArray(string name, ISampler sampler)
        {
            return Add("sampler2DMultisampleArray", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DMultisampleArray, sampler);
        }
        public Uniform AddSampler2DRect(string name, ISampler sampler)
        {
            return Add("sampler2DRect", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DRect, sampler);
        }
        public Uniform AddSampler2DRectShadow(string name, ISampler sampler)
        {
            return Add("sampler2DRectShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DRectShadow, sampler);
        }
        public Uniform AddSampler2DShadow(string name, ISampler sampler)
        {
            return Add("sampler2DShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler2DShadow, sampler);
        }
        public Uniform AddSampler3D(string name, ISampler sampler)
        {
            return Add("sampler3D", name, OpenTK.Graphics.OpenGL.ActiveUniformType.Sampler3D, sampler);
        }
        public Uniform AddSamplerBuffer(string name, ISampler sampler)
        {
            return Add("samplerBuffer", name, OpenTK.Graphics.OpenGL.ActiveUniformType.SamplerBuffer, sampler);
        }
        public Uniform AddSamplerCube(string name, ISampler sampler)
        {
            return Add("samplerCube", name, OpenTK.Graphics.OpenGL.ActiveUniformType.SamplerCube, sampler);
        }
        public Uniform AddSamplerCubeMapArray(string name, ISampler sampler)
        {
            return Add("samplerCubeMapArray", name, OpenTK.Graphics.OpenGL.ActiveUniformType.SamplerCubeMapArray, sampler);
        }
        public Uniform AddSamplerCubeMapArrayShadow(string name, ISampler sampler)
        {
            return Add("samplerCubeArrayShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.SamplerCubeMapArrayShadow, sampler);
        }
        public Uniform AddSamplerCubeShadow(string name, ISampler sampler)
        {
            return Add("samplerCubeShadow", name, OpenTK.Graphics.OpenGL.ActiveUniformType.SamplerCubeShadow, sampler);
        }
    }
}