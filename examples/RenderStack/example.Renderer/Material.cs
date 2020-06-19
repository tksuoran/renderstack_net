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
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
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
