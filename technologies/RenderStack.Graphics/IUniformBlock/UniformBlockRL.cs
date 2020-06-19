using System;
using System.Collections.Generic;
using System.Text;

namespace RenderStack.Graphics
{
    public class UniformBlockRL : IUniformBlock
    {
        private string          name;
        private List<Uniform>   uniforms = new List<Uniform>();
        private int             offset;
        private Callback        changeDelegate;
        private IUniformBuffer  uniformBuffer; 

        public string           Name            { get { return name; } set { name = value; } } 
        public List<Uniform>    Uniforms        { get { return uniforms; } }
        public int              Size            { get { return offset; } }
        public int              BindingPointGL  { get { throw new InvalidOperationException(); } }
        public string           SourceGL        { get { throw new InvalidOperationException(); } }
        public Callback         ChangeDelegate  { get { return changeDelegate; } set { changeDelegate = value; } }
        public IUniformBuffer   UniformBuffer
        {
            get
            {
                return uniformBuffer;
            }
            set
            {
                if(value != uniformBuffer)
                {
                    uniformBuffer = value;
                    if(changeDelegate != null)
                    {
                        changeDelegate();
                    }
                }
            }
        }

        private static Dictionary<string, UniformBlockRL> instances = new Dictionary<string,UniformBlockRL>();
        public static Dictionary<string, UniformBlockRL> Instances { get { return instances; } }

        public override string ToString()
        {
            throw new InvalidOperationException();
        }

        public string TypeStr(OpenTK.Graphics.OpenGL.ActiveUniformType type)
        {
            switch(type)
            {
                case OpenTK.Graphics.OpenGL.ActiveUniformType.Int:          return "int  ";
                case OpenTK.Graphics.OpenGL.ActiveUniformType.UnsignedInt:  return "uint ";
                case OpenTK.Graphics.OpenGL.ActiveUniformType.Float:        return "float";
                case OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec2:    return "vec2 ";
                case OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec3:    return "vec3 ";
                case OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec4:    return "vec4 ";
                // \todo !!! XXX FIXME HACK type (mat4) not implemented in uniform blocks
                case OpenTK.Graphics.OpenGL.ActiveUniformType.FloatMat4:    return "vec4 ";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public string SourceRL
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append("uniformblock ").Append(Name).Append("\n{\n");
                foreach(var uniform in uniforms)
                {
                    var name = uniform.Name;
                    sb.Append("    ").Append(TypeStr(uniform.Type)).Append(" ").Append(name);
                    if(uniform.IsArray)
                    {
                        sb.Append("[").Append(uniform.Count).Append("]");
                    }
                    if(uniform.Type == OpenTK.Graphics.OpenGL.ActiveUniformType.FloatMat4)
                    {
                        sb.Append("[4]");
                    }
                    sb.Append(";\n");
                }

                sb.Append("};\n\n");
                return sb.ToString();
            }
        }

        public UniformBlockRL(string name)
        {
            Name = name;
            if(instances.ContainsKey(Name))
            {
                throw new System.ArgumentException("UniformBlockRL " + Name + " already exists");
            }
            Instances[Name] = this;
        }

        public void Seal()
        {
            while((offset % Configuration.UniformBufferOffsetAlignment) != 0)
            {
                ++offset;
            }
        }

        public Uniform AddFloat(string name)
        {
            while((offset % 4) != 0) ++offset; // align by 4 bytes
            int uniformOffset = offset;
            offset += 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.Float, 
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddFloat(string name, int dimension)
        {
            while((offset % 4) != 0) ++offset; // align by 4 bytes
            int uniformOffset = offset;
            offset += 4 * dimension;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension,
                OpenTK.Graphics.OpenGL.ActiveUniformType.Float,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddVec2(string name)
        {
            while((offset % (2 * 4)) != 0) ++offset; // align by 2 * 4 bytes
            int uniformOffset = offset;
            offset += 2 * 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec2,
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddVec2(string name, int dimension)
        {
            while((offset % (2 * 4)) != 0) ++offset; // align by 2 * 2 bytes
            int uniformOffset = offset;
            offset += dimension * 2 * 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension, 
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec2,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddVec3(string name)
        {
            while((offset % (4 * 4)) != 0) ++offset; // align by 4 * 4 bytes
            int uniformOffset = offset;
            offset += 4 * 4; // std140 layout
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec3,
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddVec3(string name, int dimension)
        {
            while((offset % (4 * 4)) != 0) ++offset; // align by 4 * 4 bytes
            int uniformOffset = offset;
            offset += dimension * 4 * 4; // std140 layout
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec3,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddVec4(string name)
        {
            while((offset % (4 * 4)) != 0) ++offset; // align by 4 * 4 bytes
            int uniformOffset = offset;
            offset += 4 * 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec4,
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddVec4(string name, int dimension)
        {
            while((offset % (4 * 4)) != 0) ++offset;
            int uniformOffset = offset;
            offset += dimension * 4 * 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatVec4,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }

        public Uniform AddMat4(string name)
        {
            while((offset % (4 * 4)) != 0) ++offset; // align by 4 * 4 bytes
            int uniformOffset = offset;
            offset += 16 * 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatMat4,
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddMat4(string name, int dimension)
        {
            while((offset % (4 * 4)) != 0) ++offset; // align by 4 * 4 bytes
            int uniformOffset = offset;
            offset += dimension * 16 * 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension,
                OpenTK.Graphics.OpenGL.ActiveUniformType.FloatMat4,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }

        public Uniform AddInt(string name)
        {
            while((offset % 4) != 0) ++offset; // align by 4 bytes
            int uniformOffset = offset;
            offset += 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.Int,
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddInt(string name, int dimension)
        {
            while((offset % 4) != 0) ++offset; // align by 4 bytes
            int uniformOffset = offset;
            offset += 4 * dimension;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension,
                OpenTK.Graphics.OpenGL.ActiveUniformType.Int,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }

        public Uniform AddUInt(string name)
        {
            while((offset % 4) != 0) ++offset; // align by 4 bytes
            int uniformOffset = offset;
            offset += 4;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                1,
                OpenTK.Graphics.OpenGL.ActiveUniformType.Int,
                uniformOffset
            );
            uniforms.Add(uniform);
            return uniform;
        }
        public Uniform AddUInt(string name, int dimension)
        {
            while((offset % 4) != 0) ++offset; // align by 4 bytes
            int uniformOffset = offset;
            offset += 4 * dimension;
            var uniform = new Uniform(
                name,
                uniforms.Count,
                dimension,
                OpenTK.Graphics.OpenGL.ActiveUniformType.Int,
                uniformOffset
            );
            uniform.IsArray = true;
            uniforms.Add(uniform);
            return uniform;
        }
    }
}
