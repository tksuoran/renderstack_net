using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using RenderStack.Graphics;
using RenderStack.Mesh;

namespace example.Renderer
{
    [System.Serializable]
    // \brief Contains values for uniform buffer, plus some render states
    public class Material
    {
        private string                          name;
        private IUniformBuffer                  uniformBuffer;
        private Dictionary<string, TextureGL>   textures = new Dictionary<string,TextureGL>();

        public IProgram                         Program         { get; private set; }
        public Dictionary<string, TextureGL>    Textures        { get { return textures; } }
        public IUniformBuffer                   UniformBuffer   { get { return uniformBuffer; } }
        public string                           Name            { get { return name; } set { name = value; } }

        public MeshMode                     MeshMode        = MeshMode.PolygonFill;
        public BlendState                   BlendState      = BlendState.Default;
        public FaceCullState                FaceCullState   = FaceCullState.Default;
        public DepthState                   DepthState      = DepthState.Default;
        public MaskState                    MaskState       = MaskState.Default;
        public StencilState                 StencilState    = StencilState.Default;

        public bool                         Dirty = false;

        public Material(
            string          name,
            IProgram        program,
            IUniformBlock   uniformBlock
        )
        {
            this.name = name;
            Program = program;
            uniformBuffer = UniformBufferFactory.Create(uniformBlock);
            uniformBuffer.Sync();
        }

        public Floats Floats(string key)
        {
            return uniformBuffer.Floats(key);
        }
        public Ints Ints(string key)
        {
            return uniformBuffer.Ints(key);
        }
        public UInts UInts(string key)
        {
            return uniformBuffer.UInts(key);
        }
        public void Sync()
        {
            uniformBuffer.Sync();
        }

        public static bool LockBlendState       = false;
        public static bool LockFaceCullState    = false;
        public static bool LockDepthState       = false;
        public static bool LockMaskState        = false;
        public static bool LockStencilState     = false;

        public void Use()
        {
            UniformBuffer.Use();
            if(!LockBlendState)     BlendState.Execute();
            if(!LockFaceCullState)  FaceCullState.Execute();
            if(!LockDepthState)     DepthState.Execute();
            if(!LockMaskState)      MaskState.Execute();
            if(!LockStencilState)   StencilState.Execute();

            if(RenderStack.Graphics.Configuration.useGl1)
            {
                GL.Material(
                    MaterialFace.FrontAndBack, 
                    MaterialParameter.AmbientAndDiffuse, 
                    UniformBuffer.Floats("surface_diffuse_reflectance_color").Value
                );
                GL.Material(
                    MaterialFace.FrontAndBack, 
                    MaterialParameter.Specular,
                    UniformBuffer.Floats("surface_specular_reflectance_color").Value
                );
                float r = UniformBuffer.Floats("surface_roughness").Value[0];
                float shininess = (1 - r) * 128.0f;
                GL.Material(
                    MaterialFace.FrontAndBack, 
                    MaterialParameter.Shininess,
                    shininess
                );
            }
        }
        public void UseDebug()
        {
            Use();
        }

        public override string ToString()
        {
            return Name + ", P: " + Program.ToString() + ", MM: " + MeshMode.ToString();
        }
    }
}
