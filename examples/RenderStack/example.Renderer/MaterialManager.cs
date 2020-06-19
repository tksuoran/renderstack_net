using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using RenderStack.Graphics;
using RenderStack.Services;

namespace example.Renderer
{
    // \brief Maintains a centralized collections of materials and textures to avoid duplicates
    public class MaterialManager : Service
    {
        public override string Name
        {
            get { return "MaterialManager"; }
        }

        IRenderer       renderer;

        /* This is used for material palette */
        List<IProgram>  programs = new List<IProgram>();

        private readonly Dictionary<string, Material>   materials = new Dictionary<string,Material>();
        private readonly Dictionary<string, TextureGL>  textures = new Dictionary<string,TextureGL>();
        public Dictionary<string, Material> Materials 
        {
            get
            {
                return materials;
            }
        }
        public Dictionary<string, TextureGL> Textures
        {
            get
            {
                return textures;
            }
        }
        public TextureGL Texture(string path, bool generateMipmaps)
        {
            if(materials.ContainsKey(path))
            {
                return textures[path];
            }
            var texture = new TextureGL(new Image(path), true);
            textures[path] = texture;
            return texture;
        }
        public Material this[string name]
        {
            get
            {
                if(materials.ContainsKey(name))
                {
                    return materials[name];
                }
                return null;
            }
        }


        public void Connect(IRenderer renderer)
        {
            this.renderer = renderer;

            InitializationDependsOn(renderer);

            initializeInMainThread = true;
        }

        private void EnableSeamlessCubemaps()
        {
            //  Never mind if this fails, it just improves quality
            if(RenderStack.Graphics.Configuration.canUseSeamlessCubeMap)
            {
                GL.Enable(EnableCap.TextureCubeMapSeamless);
            }
        }

        public Material MakeMaterial(string name)
        {
            var material = materials[name] = new Material(name, renderer.Programs[name], renderer.MaterialUB);
            return material;
        }
        public Material MakeMaterial(string name, string program)
        {
            var material = materials[name] = new Material(name, renderer.Programs[program], renderer.MaterialUB);
            return material;
        }

        protected override void InitializeService()
        {
            {
                Image whiteImage = new Image(16, 16, 1.0f, 1.0f, 1.0f, 1.0f);
                var white = textures["White"] = new TextureGL(whiteImage, true);
            }

            int lightCount = example.Renderer.Configuration.maxLightCount;
            if(RenderStack.Graphics.Configuration.useGl1 == false)
            {
                System.Single[] whiteData = new System.Single[lightCount];
                for(int i = 0; i < lightCount; ++i)
                {
                    whiteData[i] = 1.0f;
                }
                var noShadow = textures["NoShadow"] = new TextureGL(1, 1, PixelFormat.Red, PixelInternalFormat.R16f, lightCount);
                noShadow.Upload(whiteData, 0);
            }

            EnableSeamlessCubemaps();
        }
    }
}
