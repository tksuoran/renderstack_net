﻿// #define DEBUG_UNIFORM_BUFFER

using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public class UniformBufferData
    {
        private string name;
        public string Name { get { return name; } set { name = value; } }

        private Dictionary<string, IUniformValue>   parameters = new Dictionary<string,IUniformValue>();
        private IUniformValue[]                     parameterArray;
        private IUniformBlock                       uniformBlock;

        public Dictionary<string, IUniformValue>    Parameters      { get { return parameters; } }
        public IUniformValue[]                      ParameterArray  { get { return parameterArray; } }
        public IUniformBlock                        UniformBlock    { get { return uniformBlock; } }

        public IUniformValue this[string key]
        {
            get
            {
                return parameters[key];
            }
            set
            {
                parameters[key] = value;
            }
        }

        public Floats Floats(string key)
        {
            return (Floats)parameters[key];
        }
        public Ints Ints(string key)
        {
            return (Ints)parameters[key];
        }
        public UInts UInts(string key)
        {
            return (UInts)parameters[key];
        }

        public UniformBufferData(IUniformBlock uniformBlock/*, IBufferRange bufferRange*/)
        {
            this.uniformBlock = uniformBlock;
            //this.uniformBufferRange = bufferRange;
            parameterArray = new IUniformValue[uniformBlock.Uniforms.Count];

            int i = 0;
            foreach(var uniform in uniformBlock.Uniforms)
            {
#if DEBUG_UNIFORM_BUFFER
                System.Diagnostics.Debug.WriteLine(
                    uniform.Type.ToString() + " " + uniform.Name + 
                    ", index = " + uniform.Index + 
                    ", i = " + i + 
                    ", offset = " + uniform.Offset +
                    ", stride = " + uniform.Stride
                );
#endif
                switch(uniform.Type)
                {
                    case ActiveUniformType.Float:       parameterArray[i] = new Floats(1, uniform.Count); break;
                    case ActiveUniformType.FloatVec2:   parameterArray[i] = new Floats(2, uniform.Count); break;
                    case ActiveUniformType.FloatVec3:   parameterArray[i] = new Floats(3, uniform.Count); break;
                    case ActiveUniformType.FloatVec4:   parameterArray[i] = new Floats(4, uniform.Count); break;
                    case ActiveUniformType.FloatMat4:   parameterArray[i] = new Floats(16, uniform.Count); break;
                    case ActiveUniformType.Int:         parameterArray[i] = new Ints(1, uniform.Count); break;
                    case ActiveUniformType.UnsignedInt: parameterArray[i] = new UInts(1, uniform.Count); break;
                    case ActiveUniformType.Sampler1D: break;
                    case ActiveUniformType.Sampler1DArray: break;
                    case ActiveUniformType.Sampler1DArrayShadow: break;
                    case ActiveUniformType.Sampler1DShadow: break;
                    case ActiveUniformType.Sampler2D: break;
                    case ActiveUniformType.Sampler2DArray: break;
                    case ActiveUniformType.Sampler2DArrayShadow: break;
                    case ActiveUniformType.Sampler2DMultisample: break;
                    case ActiveUniformType.Sampler2DMultisampleArray: break;
                    case ActiveUniformType.Sampler2DRect: break;
                    case ActiveUniformType.Sampler2DRectShadow: break;
                    case ActiveUniformType.Sampler2DShadow: break;
                    case ActiveUniformType.Sampler3D: break;
                    case ActiveUniformType.SamplerBuffer: break;
                    case ActiveUniformType.SamplerCube: break;
                    case ActiveUniformType.SamplerCubeMapArray: break;
                    case ActiveUniformType.SamplerCubeMapArrayShadow: break;
                    case ActiveUniformType.SamplerCubeShadow: break;
                    default: 
                    {
                        throw new System.Exception("Unsupported uniform type");
                    }
                }
                if(parameterArray[i] != null)
                {
                    parameterArray[i].Index = i;//uniform.Index;
                    parameters[uniform.Name] = parameterArray[i];
                    if(uniform.Default != null)
                    {
                        parameterArray[i].CopyFrom(uniform.Default);
                    }
                }
                i++;
            }
        }

        public void Sync(IBufferRange bufferRange)
        {
            bool dirty = false;
            foreach(var uniformValue in parameterArray)
            {
                if(
                    (uniformValue == null) || 
                    (uniformValue.Index == -1) || 
                    (uniformValue.Dirty == false)
                )
                {
                    continue;
                }
                dirty = true;
                var uniform = uniformBlock.Uniforms[uniformValue.Index];
                /*System.Diagnostics.Debug.WriteLine(
                    "Sync " + uniform.Type + " " + uniform.Name + " at offset " + uniform.Offset +
                    " index " + uniformValue.Index
                );*/
                switch(uniform.Type)
                {
                    case ActiveUniformType.Float:       bufferRange.Floats(uniform.Offset, (Floats)uniformValue); break;
                    case ActiveUniformType.FloatVec2:   bufferRange.Floats(uniform.Offset, (Floats)uniformValue); break;
                    case ActiveUniformType.FloatVec3:   bufferRange.Floats(uniform.Offset, (Floats)uniformValue); break;
                    case ActiveUniformType.FloatVec4:   bufferRange.Floats(uniform.Offset, (Floats)uniformValue); break;
                    case ActiveUniformType.FloatMat4:   bufferRange.Floats(uniform.Offset, (Floats)uniformValue); break;
                    case ActiveUniformType.Int:         bufferRange.Ints(uniform.Offset, (Ints)uniformValue); break;
                    case ActiveUniformType.UnsignedInt: bufferRange.UInts(uniform.Offset, (UInts)uniformValue); break;
                    default: break;
                }
            }

            if(dirty == false)
            {
                return;
            }

            if(bufferRange is BufferRangeGL)
            {
                bufferRange.UpdateGL();
            }
#if false
            else if(bufferRange is BufferRangeRL)
            {
                bufferRange.UpdateRL();
            }
#endif
            else
            {
                throw new System.ArgumentException();
            }
        }
    }
}
