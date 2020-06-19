//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;
using RenderStack.UI;

using example.Renderer;

using Attribute = RenderStack.Graphics.Attribute;

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
