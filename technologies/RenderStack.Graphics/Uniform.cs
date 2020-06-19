using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Abstraction for OpenGL uniform
    /// 
    /// \note Mostly stable.
    public class Uniform
    {
        private string              name;
        private int                 count;
        private ActiveUniformType   type;
        private int                 index = -1;
        //  Used for uniforms in non-default uniform block
        private int                 block = -1;
        private int                 offset;
        private ISampler            sampler;

        public  bool                IsArray = false;

        public int                  Index               { get { return index; } }
        public int                  TextureUnitIndex;   // used for sampler uniforms only
        public int                  Count               { get { return count; } }
        public ActiveUniformType    Type                { get { return type; } }
        public string               Name                { get { return name; } }
        public int                  Block               { get { return block; } }
        public int                  Offset              { get { return offset; } }
        public IUniformValue        Default             { get; set; }
        public ISampler             Sampler             { get { return sampler; } set { sampler = value; } }

        //  Uniform in a non-default uniform block
        public Uniform(string name, int index, int count, ActiveUniformType type, int offset)
        {
            this.name = name;
            this.index = index;
            this.count = count;
            this.type = type;
            this.offset = offset;
        }

        //  Uniform in the default uniform block
        public Uniform(string name, int index, int count, ActiveUniformType type)
        {
            this.name  = name;
            this.index = index;
            this.count = count;
            this.type  = type;
        }

        public override string ToString()
        {
            if(block != -1)
            {
                return type.ToString() + " " + name + " block = " + block + ", index = " + index + ", offset = " + offset;
            }
            else if(index != -1)
            {
                return type.ToString() + " " + name + " index = " + index;
            }
            else
            {
                return type.ToString() + " " + name;
            }
        }
    }
}
