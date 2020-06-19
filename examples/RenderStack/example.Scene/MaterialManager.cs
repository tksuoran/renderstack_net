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

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Scene
{
    public class MaterialManager : Service
    {
        public override string Name
        {
            get { return "MaterialManager"; }
        }

        Renderer            renderer;

        Material            textured;
        Material            @default;
        Material            schlick;
        Material            grid;
        Material[]          cubeMaterials = new Material[6];

        public Material     Default         { get { return @default; } }
        public Material     Schlick         { get { return schlick; } }
        public Material     GridMaterial    { get { return grid; } }
        public Material     Textured        { get { return textured; } }

        private readonly Dictionary<string, Material> materials = new Dictionary<string,Material>();
        public Dictionary<string, Material> Materials 
        {
            get
            {
                return materials;
            }
        }
        public Material this[string name]
        {
            get
            {
                if(materials.ContainsKey(name))
                {
                    return materials[name];
                }
                return Default;
            }
        }

        public Material MakeSimpleMaterial(float r, float g, float b)
        {
            return MakeSimpleMaterial(r, g, b, 0.5f, 1.0f, 0.02f);
        }
        public Material MakeSimpleMaterial(float r, float g, float b, float diffuse, float specular, float roughness)
        {
            Material material = new Material(renderer.Programs["Schlick"], MeshMode.PolygonFill);
            material.Parameters["surface_color"]                        = new Floats(diffuse * r, diffuse * g, diffuse * b);
            material.Parameters["surface_diffuse_reflectance_color"]    = new Floats(diffuse * r, diffuse * g, diffuse * b);
            material.Parameters["surface_specular_reflectance_color"]   = new Floats(specular * r, specular * r, specular *r);
            material.Parameters["surface_roughness"]                    = new Floats(roughness);
            return material;
        }

        public void Connect(
            Renderer        renderer,
            TextRenderer    textRenderer
        )
        {
            this.renderer = renderer;

            InitializationDependsOn(renderer);
            InitializationDependsOn(textRenderer);
        }

        protected override void InitializeService()
        {
            textured = new Material(renderer.Programs["Textured"], MeshMode.PolygonFill);

            textured.Parameters["texture"] = null;

            schlick = new Material(renderer.Programs["Schlick"], MeshMode.PolygonFill);
            schlick.Parameters["surface_color"]                          = new Floats(0.5f, 0.5f, 0.5f);
            schlick.Parameters["surface_diffuse_reflectance_color"]      = new Floats(0.44f, 0.44f, 0.44f);
            schlick.Parameters["surface_specular_reflectance_color"]     = new Floats(1.0f, 1.0f, 1.0f);
            schlick.Parameters["surface_specular_reflectance_exponent"]  = new Floats(80.0f);
            schlick.Parameters["surface_roughness"]                      = new Floats(0.02f);

            grid = new Material(renderer.Programs["Grid"], MeshMode.PolygonFill);
            grid.Parameters["grid_size"]                                = new Floats(1.0f, 1.0f);
            grid.Parameters["surface_color"]                            = new Floats(0.22f, 0.22f, 0.22f);
            grid.Parameters["surface_rim_color"]                        = new Floats(0.0f, 0.0f, 0.0f);
            grid.Parameters["surface_diffuse_reflectance_color"]        = new Floats(0.44f, 0.44f, 0.44f);
            grid.Parameters["surface_specular_reflectance_color"]       = new Floats(1.0f, 1.0f, 1.0f);
            grid.Parameters["surface_specular_reflectance_exponent"]    = new Floats(100.0f);
            grid.Parameters["surface_roughness"]                        = new Floats(0.02f);

            materials["pearl"]      = MakeSimpleMaterial(1.00f, 0.80f, 0.80f, 0.8f, 0.2f, 0.90f);
            materials["gold"]       = MakeSimpleMaterial(1.00f, 0.60f, 0.01f, 0.6f, 0.4f, 0.03f);
            materials["red"]        = MakeSimpleMaterial(1.00f, 0.05f, 0.00f, 0.4f, 0.4f, 0.04f);
            materials["green"]      = MakeSimpleMaterial(0.05f, 1.00f, 0.15f, 0.05f, 0.3f, 0.005f);
            materials["cyan"]       = MakeSimpleMaterial(0.05f, 0.80f, 1.00f, 0.2f, 0.4f, 0.80f);
            materials["blue"]       = MakeSimpleMaterial(0.15f, 0.20f, 0.80f, 0.4f, 1.0f, 0.01f);
            materials["magenta"]    = MakeSimpleMaterial(1.00f, 0.05f, 1.00f, 0.4f, 1.0f, 0.02f);
            materials["pink"]       = MakeSimpleMaterial(1.00f, 0.33f, 0.66f, 0.6f, 0.1f, 0.01f);
            materials["grid"]       = grid;
            materials["schlick"]    = schlick;

            @default = schlick;
        }
    }
}