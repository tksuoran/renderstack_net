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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

using Attribute = RenderStack.Graphics.Attribute;
using Buffer = RenderStack.Graphics.Buffer;

namespace example.Scene
{
    public partial class Renderer: Service
    {
        public override string Name
        {
            get { return "Renderer"; }
        }

        public class State
        {
            public Camera               Camera;
            public Viewport             Viewport;
            public Model                Model;
            public Frame                Frame;
            public Mesh                 Mesh;
            public Material             Material;
            public Program              Program;
            public MeshMode             MeshMode = MeshMode.NotSet;
            public Buffer               VertexBuffer;
            public Buffer               IndexBuffer;
            public AttributeBindings    AttributeBindings;

            public State()
            {
            }
            public State(State old)
            {
                Camera            = old.Camera           ;
                Viewport          = old.Viewport         ;
                Model             = old.Model            ;
                Frame             = old.Frame            ;
                Mesh              = old.Mesh             ;
                Material          = old.Material         ;
                Program           = old.Program          ;
                MeshMode          = old.MeshMode         ;
                VertexBuffer      = old.VertexBuffer     ;
                IndexBuffer       = old.IndexBuffer      ;
                AttributeBindings = old.AttributeBindings;
            }
        }
        public class RendererCounters
        {
            public int VAOCacheHits;
            public int VBOCacheHits;
            public int ProgramCacheHits;
            public int AttributeBindingsCacheHits;
            public int dummy;

            public void Reset()
            {
                VAOCacheHits = 0;
                VBOCacheHits = 0;
                ProgramCacheHits = 0;
                AttributeBindingsCacheHits = 0;
                dummy = 0;
            }

            public override string ToString()
            {
                return 
                    "VAO = " + VAOCacheHits.ToString() + 
                    ", VBO = " + VBOCacheHits.ToString() + 
                    ", Program = " + ProgramCacheHits.ToString() + 
                    ", AttributeBindings = " + AttributeBindingsCacheHits.ToString() +
                    ", dummy = " + dummy.ToString();
            }
        }

        private Dictionary<string, IUniformValue>   lightingParameters  = new Dictionary<string,IUniformValue>();
        private Dictionary<string, IUniformValue>   globalParameters    = new Dictionary<string,IUniformValue>();
        private AttributeMappings                   attributeMappings   = new AttributeMappings();
        private Programs                            programs;
        private Stack<State>                        requestStack = new Stack<State>();
        private State                               requested = new State();
        private State                               effective = new State();

        protected State                             Effective { get { return effective; } }

        public State                                Requested { get { return requested; } }
        public RendererCounters                     Counters = new RendererCounters();
        public Dictionary<string, IUniformValue>    LightingParameters  { get { return lightingParameters; } }
        public Dictionary<string, IUniformValue>    GlobalParameters    { get { return globalParameters; } }
        public Group                                CurrentGroup;
        public Programs                             Programs { get { return programs; } }

        private Frame defaultFrame = new Frame();
        public Frame DefaultFrame { get { return defaultFrame; } }

        #region Service initialization
        protected override void InitializeService()
        {
            InitializeMappings();
            InitializeParameters();
            programs = new Programs();
            PartialGLStateResetToDefaults();
        }
        private void InitializeMappings()
        {
            attributeMappings.Add("_position",          VertexUsage.Position, 0, 3);
            attributeMappings.Add("_normal",            VertexUsage.Normal,   0, 3);
            attributeMappings.Add("_polygon_normal",    VertexUsage.Normal,   1, 3);
            attributeMappings.Add("_normal_smooth",     VertexUsage.Normal,   2, 3);
            attributeMappings.Add("_tangent",           VertexUsage.Tangent,  0, 3);
            attributeMappings.Add("_texcoord",          VertexUsage.TexCoord, 0, 2);
            attributeMappings.Add("_color",             VertexUsage.Color,    0, 4);

            /*                   name in shader            uniform  */ 
            UniformMappings.Add("_model_to_world_matrix",   LogicalUniform.ModelToWorld);
            UniformMappings.Add("_world_to_model_matrix",   LogicalUniform.WorldToModel);

            UniformMappings.Add("_model_to_clip_matrix",    LogicalUniform.ModelToClip);
            UniformMappings.Add("_clip_to_model_matrix",    LogicalUniform.ClipToModel);

            UniformMappings.Add("_model_to_view_matrix",    LogicalUniform.ModelToView);
            UniformMappings.Add("_view_to_model_matrix",    LogicalUniform.ViewToModel);

            UniformMappings.Add("_world_to_clip_matrix",    LogicalUniform.WorldToClip);
            UniformMappings.Add("_clip_to_world_matrix",    LogicalUniform.ClipToWorld);

            UniformMappings.Add("_view_to_world_matrix",    LogicalUniform.ViewToWorld);
            UniformMappings.Add("_world_to_view_matrix",    LogicalUniform.WorldToView);

            UniformMappings.Add("_view_to_clip_matrix",     LogicalUniform.ViewToClip);
            UniformMappings.Add("_clip_to_view_matrix",     LogicalUniform.ClipToView);

            UniformMappings.Add("_viewport",                LogicalUniform.Viewport);
            UniformMappings.Add("_near_far",                LogicalUniform.NearFar);

            /*                            name in shader                             parameter name   */ 

            /*  lighting  */ 
            UniformMappings.Add<Floats> ("_ambient_light_color",                    "ambient_light_color"   );
            UniformMappings.Add<Floats> ("_light_direction",                        "light_direction"       );
            UniformMappings.Add<Floats> ("_light_radiance",                         "light_radiance"        );
            UniformMappings.Add<Floats> ("_light_color",                            "light_color"           );
            UniformMappings.Add<Floats> ("_exposure",                               "exposure"              );

            /*  material, surface  */ 
            UniformMappings.Add<Floats> ("_surface_color",                          "surface_color"         );
            UniformMappings.Add<Floats> ("_surface_rim_color",                      "surface_rim_color"     );
            UniformMappings.Add<Floats> ("_surface_diffuse_reflectance_color",      "surface_diffuse_reflectance_color"  );
            UniformMappings.Add<Floats> ("_surface_specular_reflectance_color",     "surface_specular_reflectance_color"  );
            UniformMappings.Add<Floats> ("_surface_specular_reflectance_exponent",  "surface_specular_reflectance_exponent"  );
            UniformMappings.Add<Texture>("_texture",                                "texture"           );
            UniformMappings.Add<Floats> ("_grid_size",                              "grid_size"         );
            UniformMappings.Add<Floats> ("_surface_roughness",                      "surface_roughness" );
            UniformMappings.Add<Floats> ("_surface_isotropy",                       "surface_isotropy"  );

            UniformMappings.Add<Floats> ("_alpha",                                  "alpha"             );
            UniformMappings.Add<Floats> ("_global_add_color",                       "global_add_color"  );

            /* ui */ 
            globalParameters["slider_t"]            = new Floats(0.0f);
        }
        private void InitializeParameters()
        {
            lightingParameters["light_direction"] = new Floats(3, 3);
            (lightingParameters["light_direction"] as Floats).Set(0, Vector3.Normalize(new Vector3(  1.20f, 1.05, 1.05f)));
            (lightingParameters["light_direction"] as Floats).Set(1, Vector3.Normalize(new Vector3(  1.00f, 1.20, 1.00f)));
            (lightingParameters["light_direction"] as Floats).Set(2, Vector3.Normalize(new Vector3(  1.05f, 1.05, 1.20f)));
            lightingParameters["light_color"] = new Floats(3, 3);
            (lightingParameters["light_color"] as Floats).Set(0, 1.0f, 0.9f, 0.6f);
            (lightingParameters["light_color"] as Floats).Set(1, 0.4f, 0.6f, 1.0f);
            (lightingParameters["light_color"] as Floats).Set(2, 1.0f, 0.9f, 0.6f);
             lightingParameters["light_radiance"] = new Floats(1, 3);
            (lightingParameters["light_radiance"] as Floats).Set(0, 0.75f);
            (lightingParameters["light_radiance"] as Floats).Set(1, 1.75f);
            (lightingParameters["light_radiance"] as Floats).Set(2, 0.75f);

            globalParameters["global_add_color"]    = new Floats(0.0f, 0.0f, 0.0f, 0.0f);
            globalParameters["alpha"]               = new Floats(1.0f);
            globalParameters["exposure"]            = new Floats(1.0f);
            globalParameters["ambient_light_color"] = new Floats(0.23f, 0.43f, 0.53f);
        }
        #endregion Service initialization
        #region Bindings
        public void BindGroup()
        {
            foreach(var model in CurrentGroup.Models)
            {
                Requested.Model     = model;
                Requested.Frame     = model.Frame;
                Requested.Mesh      = model.Batch.Mesh;
                Requested.Material  = model.Batch.Material;
                Requested.Program   = Requested.Material.Program;
                Requested.MeshMode  = Requested.Material.MeshMode;
                BindCurrent();
            }
        }
        public void BindCurrent()
        {
            if(Requested.Program.AttributeMappings == null)
            {
                Requested.Program.AttributeMappings = attributeMappings;
            }
            Requested.Program.Bind(Requested.Camera);
            Requested.Program.Bind(lightingParameters);
            Requested.Program.Bind(Requested.Material.Parameters);
            Requested.Program.Bind(globalParameters);
        }
        #endregion Bindings
        public void Push()
        {
            requestStack.Push(requested);
            requested = new State(requested);
        }
        public void Pop()
        {
            requested = requestStack.Pop();
        }
        #region Render
        public void UpdateCamera()
        {
            Effective.Camera = Requested.Camera;
            Effective.Viewport = Requested.Viewport;
            Effective.Camera.UpdateCameraFrame();
            Effective.Camera.UpdateViewport(Effective.Viewport);
        }
        public void RenderGroup()
        {
            UpdateCamera();
            foreach(var model in CurrentGroup.Models)
            {
                Requested.Model     = model;
                Requested.Frame     = model.Frame;
                Requested.Mesh      = model.Batch.Mesh;
                Requested.Material  = model.Batch.Material;
                Requested.Program   = Requested.Material.Program;
                Requested.MeshMode  = Requested.Material.MeshMode;
                RenderCurrent();
            }
        }
        //  \todo improve
        public void RenderCurrent()
        {
            Requested.Program.Bind(Requested.Camera);
            if(Effective.Camera != Requested.Camera)
            {
                Effective.Camera = Requested.Camera;
            }
            if(Effective.Frame != Requested.Frame)
            {
                Effective.Frame = Requested.Frame;
            }
            Effective.Camera.UpdateModelFrame(Effective.Frame);

            if(Effective.Program != Requested.Program)
            {
                Requested.Program.Use();
                if(Requested.Program.AttributeMappings == null)
                {
                    Requested.Program.AttributeMappings = attributeMappings;
                }
                Effective.Program = Requested.Program;
            }
            else
            {
                ++Counters.ProgramCacheHits;
            }

            if(Effective.Material != Requested.Material)
            {
                Effective.Material = Requested.Material;
            }

            Effective.Program.Bind(Requested.Material.Parameters);

            Effective.Program.ApplyUniforms();


            if(
                (Effective.MeshMode != Requested.MeshMode) ||
                (Effective.Mesh != Requested.Mesh)
            )
            {
                Requested.AttributeBindings = Requested.Mesh.AttributeBindings(Requested.Program, Requested.MeshMode);
            }

            if(Effective.MeshMode != Requested.MeshMode)
            {
                Effective.MeshMode = Requested.MeshMode;
            }
            if(Effective.Mesh != Requested.Mesh)
            {
                Effective.Mesh = Requested.Mesh;
            }

            if(Effective.AttributeBindings != Requested.AttributeBindings)
            {
                DisableEffectiveAttributeBindings();
                SetupAttributeBindings();
                Effective.AttributeBindings = Requested.AttributeBindings;
            }
            else
            {
                ++Counters.AttributeBindingsCacheHits;
            }

            if(Effective.AttributeBindings.VertexBufferRange.BaseVertex != 0)
            {
                ++this.Counters.dummy;
            }
            GL.DrawElementsBaseVertex(
                Effective.AttributeBindings.IndexBufferRange.BeginMode,
                (int)Effective.AttributeBindings.IndexBufferRange.Count,
                Effective.AttributeBindings.IndexBufferRange.Buffer.DrawElementsType,
                (IntPtr)(Effective.AttributeBindings.IndexBufferRange.OffsetBytes),
                Effective.AttributeBindings.VertexBufferRange.BaseVertex
            );
            //GL.Finish();
        }
        private void SetupAttributeBindings()
        {
            if(
                RenderStack.Graphics.Configuration.mustUseVertexArrayObject ||
                (
                    RenderStack.Graphics.Configuration.canUseVertexArrayObject &&
                    RenderStack.Graphics.Configuration.useVertexArrayObject
                )
            )
            {
                int glBefore = int.MaxValue;
                GL.GetInteger(GetPName.ElementArrayBufferBinding, out glBefore);

                Requested.AttributeBindings.Use();

                if(Requested.AttributeBindings.Dirty)
                {
                    Requested.AttributeBindings.IndexBufferRange.Buffer.Use();
                    Requested.AttributeBindings.VertexBufferRange.Buffer.Use();
                    Requested.AttributeBindings.SetupAttributePointers();
                    Requested.AttributeBindings.Dirty = false;

                    int vao  = (int)Requested.AttributeBindings.VertexArrayObject;
                    int eabo = (int)Requested.AttributeBindings.IndexBufferRange.Buffer.BufferObject;

                    Debug.WriteLine("----------- Bound " + vao.ToString() + " now has eabo " + eabo.ToString());

                    int gl = int.MaxValue;
                    GL.GetInteger(GetPName.ElementArrayBufferBinding, out gl);
                    int requested = (int)Requested.AttributeBindings.IndexBufferRange.Buffer.BufferObject;
                    if(gl != requested)
                    {
                        ++Counters.dummy;
                    }
                }
                else
                {
                    int gl = int.MaxValue;
                    GL.GetInteger(GetPName.ElementArrayBufferBinding, out gl);
                    int effective = (int)Effective.AttributeBindings.IndexBufferRange.Buffer.BufferObject;
                    int requested = (int)Requested.AttributeBindings.IndexBufferRange.Buffer.BufferObject;

                    if(gl != requested)
                    {
                        int vao  = (int)Requested.AttributeBindings.VertexArrayObject;
                        int eabo = (int)Requested.AttributeBindings.IndexBufferRange.Buffer.BufferObject;
                        Debug.WriteLine("----------- VAO " + vao.ToString());
                        Debug.WriteLine("----------- EABO before VAO bind " + glBefore.ToString());
                        Debug.WriteLine("----------- EABO after VAO bind " + gl.ToString());
                        Debug.WriteLine("----------- Requested EABO " + requested.ToString());
                        Debug.WriteLine("----------- Effective EABO " + effective.ToString());
                        Requested.AttributeBindings.IndexBufferRange.Buffer.Use();
                    }
                }
            }
            else
            {
                Requested.VertexBuffer = Requested.AttributeBindings.VertexBufferRange.Buffer;
                Requested.IndexBuffer = Requested.AttributeBindings.IndexBufferRange.Buffer;

                if(Effective.VertexBuffer != Requested.VertexBuffer)
                {
                    Requested.VertexBuffer.Use();
                    Effective.VertexBuffer = Requested.VertexBuffer;
                }
                else
                {
                    ++Counters.VBOCacheHits;
                }
                if(Effective.IndexBuffer != Requested.IndexBuffer)
                {
                    Requested.IndexBuffer.Use();
                    Effective.IndexBuffer = Requested.IndexBuffer;
                }
                else
                {
                    ++Counters.VBOCacheHits;
                }

                Requested.AttributeBindings.SetupAttributePointers();
            }

            if(Requested.AttributeBindings.IndexBufferRange.NeedsUpload)
            {
                Requested.AttributeBindings.IndexBufferRange.Buffer.UpdateAll();
            }
            if(Requested.AttributeBindings.VertexBufferRange.NeedsUpload)
            {
                Requested.AttributeBindings.VertexBufferRange.Buffer.UpdateAll();
            }

        }
        private void DisableEffectiveAttributeBindings()
        {
            if(Effective.AttributeBindings == null)
            {
                return;
            }
            if(
                RenderStack.Graphics.Configuration.mustUseVertexArrayObject ||
                (
                    RenderStack.Graphics.Configuration.canUseVertexArrayObject &&
                    RenderStack.Graphics.Configuration.useVertexArrayObject
                )
            )
            {
                //  NOP
            }
            else
            {
                Effective.AttributeBindings.DisableAttributes();
            }
        }
        #endregion Render
        public void RenderCurrentClear()
        {
            if(Effective.Viewport != Requested.Viewport)
            {
                GL.Viewport(
                    (int)Requested.Viewport.X, 
                    (int)Requested.Viewport.Y, 
                    (int)Requested.Viewport.Width, 
                    (int)Requested.Viewport.Height
                );
                Effective.Viewport = Requested.Viewport;
            }

            GL.ClearColor(0.34f, 0.34f, 0.34f, 1.0f);
            GL.ClearStencil(0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        public void PartialGLStateResetToDefaults()
        {
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0.0f, 0.0f);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
        }
    }
}
