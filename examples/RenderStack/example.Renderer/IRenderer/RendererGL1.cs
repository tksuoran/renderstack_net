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

// #define DISABLE_CACHE
#define DEBUG_RENDERER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

using Attribute = RenderStack.Graphics.Attribute;
using Buffer = RenderStack.Graphics.BufferGL;

namespace example.Renderer
{
    // \brief Service to render either a Group or the current requested state. Supports instanced rendering.
    // \note Non-instanced rendering is not currently supported
    // \note You need to Sync() materials' (and global) uniform buffers manually after you have modified them
    // \note Camera and light uniform buffers are synced when rendering groups, otherwise Sync() them manually
    // \note Model uniform blocks are updated when rendering a group or by using SetFrame()
    public partial class RendererGL1 : RenderStack.Services.Service, IRenderer
    {
        #region Service
        public override string Name
        {
            get { return "Renderer"; }
        }

        public override System.Type ServiceType
        {
            get
            {
                return typeof(IRenderer); 
            }
        }

        OpenTK.GameWindow window;

        public void Connect(OpenTK.GameWindow window)
        {
            this.window = window;
        }

        protected override void InitializeService()
        {
            InitializeTextureUnits();
            InitializeShaderSystem();
            InitializeAttributeMappings();
            programs = new Programs();
            PartialGLStateResetToDefaults();
        }
        private void InitializeTextureUnits()
        {
            //  Bind a dummy texture to all texture units
            byte[] whiteData = new byte[4];
            whiteData[0] = 255;
            whiteData[1] = 255;
            whiteData[2] = 255;
            whiteData[3] = 255;
            dummyTexture = new TextureGL(1, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba);
            dummyTexture.Upload(whiteData, 0);
            nearestSampler = new SamplerGL1();
            nearestSampler.MinFilter = TextureMinFilter.Nearest;
            nearestSampler.MagFilter = TextureMagFilter.Nearest;
            nearestSampler.Wrap = TextureWrapMode.ClampToEdge;
            int textureUnits = 4;
            try
            {
                GL.GetInteger(GetPName.MaxTextureUnits, out textureUnits);
            }
            catch(Exception)
            {
            }
            for(int i = 0; i < textureUnits; ++i)
            {
                try
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    dummyTexture.Apply();
                    nearestSampler.Apply(i, TextureTarget.Texture2D);
                }
                catch(Exception)
                {
                }
            }

            float[] zeros = { 0.0f, 0.0f, 0.0f, 0.0f };
            for(int i = 0; i < 8; ++i)
            {
                GL.Light(LightName.Light0 + i, LightParameter.Ambient, zeros);
            }

        }
        private bool modelsDirty = false;
        private void ModelsDirty()
        {
            modelsDirty = true;
        }
        private void ForceProgramUse()
        {
            effective.Program = null;
        }
        private void InitializeShaderSystem()
        {
            //InitializeOpenRL();

            //GL.Enable(EnableCap.Normalize);
            var dualTexture = AttributeMappings.Pool["dualtexture"] = new AttributeMappings();
            dualTexture.Add( 0, "_position",  VertexUsage.Position, 0,    3);
            dualTexture.Add( 1, "_texcoord",  VertexUsage.TexCoord, 0, VertexUsage.TexCoord, 0, 2);
            dualTexture.Add( 2, "_texcoord",  VertexUsage.TexCoord, 0, VertexUsage.TexCoord, 1, 2);
            dualTexture.Add( 3, "_color",     VertexUsage.Color,    0,    4);

            var idToColor = AttributeMappings.Pool["idToColor"] = new AttributeMappings();
            idToColor.Add(0, "_position",  VertexUsage.Position, 0, 3);
            idToColor.Add(1, "_id_vec3",   VertexUsage.Id,       0, VertexUsage.Color, 0, 3);

            CameraUniformBlock.NearFar              = "near_far";
            CameraUniformBlock.FovXFovYAspect       = "fovx_fovy_aspect";
            CameraUniformBlock.ViewToClip           = "view_to_clip_matrix";
            CameraUniformBlock.ClipToView           = "view_to_clip_matrix";
            CameraUniformBlock.WorldToClip          = "world_to_clip_matrix";
            CameraUniformBlock.ClipToWorld          = "clip_to_world_matrix";
            CameraUniformBlock.WorldToView          = "world_to_view_matrix";
            CameraUniformBlock.ViewToWorld          = "view_to_world_matrix";
            CameraUniformBlock.Viewport             = "viewport";
            CameraUniformBlock.ViewPositionInWorld  = "view_position_in_world";
            CameraUniformBlock.Initialize("camera");

            LightsUniforms.spec.Count               = "count";
            LightsUniforms.spec.Exposure            = "exposure";
            LightsUniforms.spec.Bias                = "bias";
            LightsUniforms.spec.AmbientLightColor   = "ambient_light_color";
            LightsUniforms.spec.WorldToLight        = "world_to_light_matrix";
            LightsUniforms.spec.WorldToShadow       = "world_to_shadow_matrix";
            LightsUniforms.spec.Direction           = "direction";
            LightsUniforms.spec.Color               = "color";
            LightsUniforms.Initialize("lights", Configuration.maxLightCount);

            globalUB = new UniformBlockGL("global");
            globalUB.AddVec4("add_color");
            globalUB.AddFloat("alpha");
            globalUB.Seal();

            modelsUB = new UniformBlockGL("models");
            modelsUB.AddMat4("model_to_world_matrix", Configuration.instanceCount);
            modelsUB.AddVec4("id_offset_vec3", Configuration.instanceCount);
            ShaderGL3.Replace("#if USE_INTEGER_POLYGON_ID", "#if 0");
            modelsUB.Seal();

            models = UniformBufferFactory.Create(modelsUB);
            global = UniformBufferFactory.Create(globalUB);
            global.Floats("alpha").Set(1.0f);
            global.Floats("add_color").Set(0.0f, 0.0f, 0.4f);
            global.SyncDelegate = ForceProgramUse;

            models.SyncDelegate = ModelsDirty;
            models.Use();
            global.Use();

            LightsUniforms.Exposure.Set(1.0f);
            LightsUniforms.Bias.Set(-0.002f, 0.0f, 0.002f);
            LightsUniforms.AmbientLightColor.Set(0.2f, 0.2f, 0.2f);
            LightsUniforms.UniformBufferGL.Sync();
            LightsUniforms.UniformBufferGL.Use();

            var nearestClampToEdge = new SamplerGL1();
            nearestClampToEdge.MinFilter    = TextureMinFilter.Nearest;
            nearestClampToEdge.MagFilter    = TextureMagFilter.Nearest;
            nearestClampToEdge.Wrap         = TextureWrapMode.ClampToEdge;
            nearestClampToEdge.CompareMode  = TextureCompareMode.None;

            var bilinearClampToEdge = new SamplerGL1();
            bilinearClampToEdge.MinFilter   = TextureMinFilter.NearestMipmapLinear;
            bilinearClampToEdge.MagFilter   = TextureMagFilter.Linear;
            bilinearClampToEdge.Wrap        = TextureWrapMode.ClampToEdge;
            bilinearClampToEdge.CompareMode = TextureCompareMode.None;

            Samplers.Global.AddSampler2D("t_font", nearestClampToEdge);
            Samplers.Global.AddSampler2D("t_ninepatch", bilinearClampToEdge).TextureUnitIndex = 1;
            Samplers.Global.AddSampler2D("t_cube", bilinearClampToEdge);
            Samplers.Global.AddSampler2D("t_left", nearestClampToEdge);
            Samplers.Global.AddSampler2D("t_right", nearestClampToEdge).TextureUnitIndex = 1;
            Samplers.Global.AddSampler2D("t_surface_color", bilinearClampToEdge);
            Samplers.Global.AddSampler2D("t_particle", bilinearClampToEdge);
            Samplers.Global.AddSampler2DArray("t_shadowmap_vis", bilinearClampToEdge);
            Samplers.Global.Seal();

            materialUB = new UniformBlockGL("material");
            materialUB.AddVec4("surface_diffuse_reflectance_color");
            materialUB.AddVec4("surface_specular_reflectance_color");
            materialUB.AddFloat("surface_specular_reflectance_exponent");
            materialUB.AddVec2("surface_rim_parameters");
            materialUB.AddVec4("grid_size");
            materialUB.AddFloat("surface_roughness");
            materialUB.AddFloat("surface_isotropy");
            materialUB.AddFloat("contrast").Default = new Floats(1.0f);
            materialUB.AddFloat("deghost").Default = new Floats(0.0f);
            materialUB.AddFloat("saturation").Default = new Floats(1.0f);
            materialUB.AddFloat("t");
            materialUB.AddFloat("slider_t");
            materialUB.AddVec4("fill_color");
            materialUB.AddVec4("point_color");
            materialUB.AddFloat("point_z_offset");
            materialUB.AddVec4("line_color");
            materialUB.AddVec2("line_width").Default = new Floats(1.0f, 0.25f); // \todo check how to compute .y
            materialUB.AddFloat("bias_units");
            materialUB.AddFloat("bias_factor");
            materialUB.AddFloat("octaves").Default = new Floats(4.0f);
            materialUB.AddFloat("offset").Default = new Floats(0.0f);
            materialUB.AddFloat("frequency").Default = new Floats(2.2f);
            materialUB.AddFloat("amplitude").Default = new Floats(0.2f);
            materialUB.AddFloat("lacunarity").Default = new Floats(3.3f);
            materialUB.AddFloat("persistence").Default = new Floats(0.25f);
            materialUB.Seal();
            materialUB.ChangeDelegate = ForceProgramUse;
        }
        private void InitializeAttributeMappings()
        {
            // \todo would need to have per program attribute mappings for fixed function
            var attributeMappings = AttributeMappings.Global;
            attributeMappings.Add( 0, "_position",          VertexUsage.Position, 0, 3);
            //attributeMappings.Add( 1, "_normal",            VertexUsage.Normal,   0, 3);
            attributeMappings.Add( 2, "_texcoord",          VertexUsage.TexCoord, 0, 2);
            attributeMappings.Add( 3, "_color",             VertexUsage.Color,    0, 4);
            attributeMappings.Add( 4, "_tangent",           VertexUsage.Tangent,  0, 3);
            //attributeMappings.Add( 5, "_polygon_normal",    VertexUsage.Normal,   1, 3);
            attributeMappings.Add( 5, "_polygon_normal",    VertexUsage.Normal,   0, 3);
            //attributeMappings.Add( 6, "_normal_smooth",     VertexUsage.Normal,   2, 3);
            attributeMappings.Add( 7, "_t",                 VertexUsage.Color,    1, 1);
            attributeMappings.Add( 8, "_edge_color",        VertexUsage.Color,    1, 4);
            attributeMappings.Add( 9, "_id_uint",           VertexUsage.Id,       0, 1);
            attributeMappings.Add(10, "_id_vec3",           VertexUsage.Id,       0, 3);
        }
        #endregion
        public void Unload()
        {
        }
        public void HandleResize()
        {
            if(Resize != null)
            {
                Resize(this, null);
            }
        }

        public int Width { get { return window.Width; } }
        public int Height { get { return window.Height; } }

        #region Data members
        private TextureGL               dummyTexture;
        private SamplerGL1              nearestSampler;
        private Programs                programs;
        private Stack<State>            requestStack = new Stack<State>();
        private State                   requested = new State();
        private State                   effective = new State();
        private State                   Effective       { get { return effective; } }
        private UniformBlockGL          modelsUB;
        private UniformBlockGL          globalUB;
        private UniformBlockGL          materialUB;
        private IUniformBuffer          models;
        private IUniformBuffer          global;
        private List<UniformBlockGL>    uniformBlocks = new List<UniformBlockGL>();

        public  event EventHandler<EventArgs> Resize = delegate { };
        public  Programs                Programs        { get { return programs; } }
        public  State                   Requested       { get { return requested; } }
        public  IUniformBlock           MaterialUB      { get { return materialUB; } }
        public  IUniformBuffer          Models          { get { return models; } }
        public  IUniformBuffer          Global          { get { return global; } }
        public  List<UniformBlockGL>    UniformBlocks   { get { return uniformBlocks; } }

        // \brief When enabled, prevents group rendering to changing material and program
        public bool lockMaterial = false;
        public bool LockMaterial { get { return lockMaterial; } set { lockMaterial = value; } }

        private Group               currentGroup;
        public Group                CurrentGroup { get { return currentGroup; } set { currentGroup = value; } }

        private Timers              timers = new Timers();
        public Timers               Timers { get { return timers; } }

        private Frame defaultFrame = new Frame();
        public Frame DefaultFrame { get { return defaultFrame; } }
        #endregion

        // \brief Find a sampler based on key and set it to use the specified texture
        // \note This does not change any Program state; Programs are assumed to be already associated with samplers
        public void SetTexture(string key, TextureGL texture)
        {
            var samplerUniform = Samplers.Global.Sampler(key);
            if(samplerUniform == null)
            {
                return;
            }

            int textureUnitIndex = samplerUniform.TextureUnitIndex;
            if(textureUnitIndex == -1)
            {
                return;
            }

            GL.ActiveTexture(TextureUnit.Texture0 + textureUnitIndex);
            texture.Apply();
            samplerUniform.Sampler.Apply(textureUnitIndex, texture.BindTarget);

            // \todo cache textures? textures[key] = texture; 
        }
        public void UnbindTexture(string key, TextureGL texture)
        {
        }

        private readonly static float[] f = new float[16];
        private void LoadMatrix(Matrix4 m)
        {
            f[ 0] = m._00;
            f[ 1] = m._10;
            f[ 2] = m._20;
            f[ 3] = m._30;
            f[ 4] = m._01;
            f[ 5] = m._11;
            f[ 6] = m._21;
            f[ 7] = m._31;
            f[ 8] = m._02;
            f[ 9] = m._12;
            f[10] = m._22;
            f[11] = m._32;
            f[12] = m._03;
            f[13] = m._13;
            f[14] = m._23;
            f[15] = m._33;
            GL.LoadMatrix(f);
        }
        // \brief Set a single frame to be used when rendering current state. Not used when rendering Groups.
        public void SetFrame(Frame frame)
        {
            Matrix4 model = frame.LocalToWorld.Matrix;
            Matrix4 view = effective.Camera.Frame.LocalToWorld.InverseMatrix;
            Matrix4 modelView = view * model;
            GL.MatrixMode(MatrixMode.Modelview);
            LoadMatrix(modelView);
        }
        public void SetFrame(int i)
        {
            Matrix4 model = Models.Floats("model_to_world_matrix").Get(i);
            Matrix4 view = effective.Camera.Parameters.WorldToView.Get(0);
            Matrix4 modelView = view * model;
            GL.MatrixMode(MatrixMode.Modelview);
            LoadMatrix(modelView);
        }

        // \brief Push current request state to stack
        public void Push()
        {
            requestStack.Push(requested);
            requested = new State(requested);
        }
        // \brief Pop current request state from stack
        public void Pop()
        {
            requested = requestStack.Pop();
            //  \todo Investigate why this is needed (otherwise errors in render; scene with only floor has no shadows correct)?
            requested.VertexStream = null;
        }
        #region Render
        // \brief Apply changes to requested camera
        public void UpdateCamera()
        {
            Requested.Camera.UpdateFrame();
            Requested.Camera.UpdateViewport(Requested.Viewport);

            Matrix4 projection = Requested.Camera.ViewToClip.Matrix;
            GL.MatrixMode(MatrixMode.Projection);
            LoadMatrix(projection);

            Effective.Camera = Requested.Camera;
            Effective.Viewport = Requested.Viewport;
        }

        // \brief Render a set of instances from current group group with the current requested state, optionally updating material and program
        // \note A set of instances can be for example all opaque instances or all transparent instances
        public void RenderGroupInstances(Instances instances)
        {
#if true
            foreach(var kvp in instances.Collection)
            {
                var tuple = kvp.Key;
                var models2 = kvp.Value;
                Requested.Mesh = tuple.Item1;
                if(LockMaterial == false)
                {
                    Requested.Material  = tuple.Item2;
                    Requested.Program   = Requested.Material.Program;
                    Requested.MeshMode  = Requested.Material.MeshMode;
                }
                RenderInstancedPrepare();

                foreach(var model in models2)
                {
#if DEBUG_RENDERER
                    RenderStack.Graphics.Debug.WriteLine("render " + model.Name);
#endif
                    SetFrame(model.Frame);
                    RenderCurrent();
                }
            }
#else
            foreach(var model in CurrentGroup.Models)
            {
#if DEBUG_RENDERER
                RenderStack.Graphics.Debug.WriteLine("render " + model.Name);
#endif
                Requested.Mesh      = model.Batch.Mesh;
                Requested.Material  = model.Batch.Material;
                Requested.Program   = Requested.Material.Program;

                SetFrame(model.Frame);
                /*models.Floats("model_to_world_matrix").Set(
                    0, model.Frame.LocalToWorld.Matrix
                );
                models.Sync();*/

                RenderCurrent();
            }
#endif
        }
        // \brief Render current group, also update current camera and lights
        // \note All instances are rendered in two passes, first pass for all opaque, second pass for all transparent instances
        // \todo Add filter for which instances to render, so all opaque instances from multiple groups can be rendered before all transparent instances
        public void RenderGroup()
        {
#if DEBUG_RENDERER
            RenderStack.Graphics.Debug.WriteLine("----- RenderGroup Begin -----");
#endif
            UpdateCamera();
            int index = 0;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 worldToView = Requested.Camera.Frame.LocalToWorld.InverseMatrix;
            LoadMatrix(worldToView);
            foreach(var light in CurrentGroup.Lights)
            {
                light.UpdateFrame();
                GL.Enable(EnableCap.Light0 + index);
                //Vector4 pos
                unsafe
                {
                    fixed(float* ptr = &LightsUniforms.Direction.Value[index * 4])
                    {
                        GL.Light(LightName.Light0 + index, LightParameter.Position, ptr);
                    }
                    fixed(float* ptr = &LightsUniforms.Color.Value[index * 4])
                    {
                        GL.Light(LightName.Light0 + index, LightParameter.Diffuse, ptr);
                        GL.Light(LightName.Light0 + index, LightParameter.Specular, ptr);
                    }
                    if(index == 0)
                    {
                        fixed(float* ptr = &LightsUniforms.AmbientLightColor.Value[index * 4])
                        {
                            GL.Light(LightName.Light0, LightParameter.Ambient, ptr);
                        }
                    }
                }
                ++index;
            }
            RenderGroupInstances(CurrentGroup.OpaqueInstances);
            RenderGroupInstances(CurrentGroup.TransparentInstances);
#if DEBUG_RENDERER
            RenderStack.Graphics.Debug.WriteLine("----- RenderGroup End -----");
#endif
        }

        public void UpdateMaterialProgramVertexStream(int instance)
        {
            Timers.MaterialSwitch.Begin();
            if(Effective.Material != Requested.Material)
            {
                Requested.Material.Use();
                Effective.Material = Requested.Material;
            }
            Timers.MaterialSwitch.End();

            Timers.ProgramSwitch.Begin();
            if(
                (Effective.Program != Requested.Program) ||
                modelsDirty
            )
            {
                ProgramGL1 program = (ProgramGL1)Requested.Program;

                Requested.Program.Use(instance);
                Effective.Program = Requested.Program;
                modelsDirty = false;
            }
            Timers.ProgramSwitch.End();

            Timers.AttributeSetup.Begin();
            BindAttributesAndCheckForUpdates();
            Timers.AttributeSetup.End();
        }
        public void RenderCurrentDebug()
        {
            RenderCurrent();
        }
        public void RenderCurrent()
        {
            UpdateMaterialProgramVertexStream(0);

            IBufferRange indexBufferRange = Effective.Mesh.IndexBufferRange(Effective.MeshMode);

            Timers.DrawCalls.Begin();
            GL.DrawElements(
                indexBufferRange.BeginMode,
                (int)indexBufferRange.Count,
                indexBufferRange.DrawElementsTypeGL,
                (IntPtr)(indexBufferRange.OffsetBytes)
            );
            Timers.DrawCalls.End();
        }
        public void RenderCurrent(int instanceCount)
        {
            IBufferRange    indexBufferRange        = Effective.Mesh.IndexBufferRange(Effective.MeshMode);
            ProgramGL1      requested_ProgramGL1    = (ProgramGL1)Requested.Program;
            bool            usePerInstance          = requested_ProgramGL1.FixedFunctionProgram.UsePerInstance;

            Timers.DrawCalls.Begin();
            for(int i = 0; i < instanceCount; ++i)
            {
                SetFrame(i);

                if(usePerInstance == true)
                {
                    Timers.ProgramSwitch.Begin();
                    Effective.Program.Use(i);
                    Timers.ProgramSwitch.End();
                }

                GL.DrawElements(
                    indexBufferRange.BeginMode,
                    (int)indexBufferRange.Count,
                    indexBufferRange.DrawElementsTypeGL,
                    (IntPtr)(indexBufferRange.OffsetBytes)
                );
            }
            Timers.DrawCalls.End();
        }
        public void RenderInstancedPrepare()
        {
            Timers.MaterialSwitch.Begin();
            if(Effective.Material != Requested.Material)
            {
                Requested.Material.Use();
                Effective.Material = Requested.Material;
            }
            Timers.MaterialSwitch.End();

            Timers.ProgramSwitch.Begin();
            if(Effective.Program != Requested.Program)
            {
                ProgramGL1 requested_ProgramGL1 = (ProgramGL1)Requested.Program;
                if(requested_ProgramGL1.FixedFunctionProgram.UsePerInstance == false)
                {
                    Requested.Program.Use(0);
                    Effective.Program = Requested.Program;
                }
            }
            Timers.ProgramSwitch.End();

            Timers.AttributeSetup.Begin();
            BindAttributesAndCheckForUpdates();
            Timers.AttributeSetup.End();
        }
        public void BindAttributesAndCheckForUpdates()
        {
            Requested.VertexStream = Requested.Mesh.VertexBufferRange.VertexStreamGL(Effective.Program.AttributeMappings);
            Requested.IndexBufferRange = Requested.Mesh.IndexBufferRange(Requested.MeshMode);

            if(Effective.MeshMode != Requested.MeshMode)
            {
                Effective.MeshMode = Requested.MeshMode;
            }
            if(Effective.Mesh != Requested.Mesh)
            {
                Effective.Mesh = Requested.Mesh;
            }

            // \todo there is room for optimizations here
#if true // !DISABLE_CACHE
            if(
                (Effective.VertexStream != Requested.VertexStream) ||
                (Effective.VertexBuffer != Requested.VertexBuffer) ||
                (Effective.IndexBuffer != Requested.IndexBuffer) ||
                (Effective.IndexBufferRange != Requested.IndexBufferRange)
            )
#endif
            {
                DisableEffectiveAttributeBindings();
                SetupAttributeBindings();
                Effective.VertexStream = Requested.VertexStream;
                Effective.IndexBufferRange = Requested.IndexBufferRange;
            }
            CheckForBufferUploads();
        }
        private void SetupAttributeBindings()
        {
            Requested.VertexBuffer = Requested.Mesh.VertexBufferRange.BufferGL;
            Requested.IndexBuffer = Requested.IndexBufferRange.BufferGL;

            bool match = 
                (Effective.VertexStream == Requested.VertexStream) &&
                (Effective.IndexBufferRange == Requested.IndexBufferRange);
#if !DISABLE_CACHE
            if(Effective.VertexBuffer != Requested.VertexBuffer)
#endif
            {
                Requested.VertexBuffer.UseGL();
                Effective.VertexBuffer = Requested.VertexBuffer;
                match = false;
            }
#if !DISABLE_CACHE
            if(Effective.IndexBuffer != Requested.IndexBuffer)
#endif
            {
                Requested.IndexBuffer.UseGL();
                Effective.IndexBuffer = Requested.IndexBuffer;
                match = false;
            }

#if !DISABLE_CACHE
            if(match == false)
#endif
            {
                Requested.VertexStream.SetupAttributePointers();
            }
        }
        private void CheckForBufferUploads()
        {
            if(Requested.IndexBufferRange.NeedsUploadGL)
            {
                Requested.IndexBufferRange.BufferGL.UpdateAll();
            }
            if(Requested.Mesh.VertexBufferRange.NeedsUploadGL)
            {
                Requested.Mesh.VertexBufferRange.BufferGL.UpdateAll();
            }
        }
        private void DisableEffectiveAttributeBindings()
        {
            if(Effective.VertexStream == null)
            {
                return;
            }
            Effective.VertexStream.DisableAttributes();
        }
        #endregion Render
        public void ApplyViewport()
        {
            //  \todo Use render state for viewport
            GL.Viewport(
                (int)Requested.Viewport.X, 
                (int)Requested.Viewport.Y, 
                (int)Requested.Viewport.Width, 
                (int)Requested.Viewport.Height
            );
            Effective.Viewport = Requested.Viewport;
        }
        public void RenderCurrentClear()
        {
            GL.ClearColor(0.34f, 0.34f, 0.34f, 1.0f);
            GL.ClearStencil(0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }
        public void BeginScissorMouse(int px, int py)
        {
            GL.Scissor(px - 1, py - 1, 3, 3);
            GL.Enable(EnableCap.ScissorTest);
        }

        public void EndScissorMouse()
        {
            GL.Scissor(
                Requested.Viewport.X, 
                Requested.Viewport.Y, 
                Requested.Viewport.Width, 
                Requested.Viewport.Height
            );
            GL.Disable(EnableCap.ScissorTest);
        }

        public void PartialGLStateResetToDefaults()
        {
#if DEBUG_RENDERER
            RenderStack.Graphics.Debug.WriteLine("PartialGLStateResetToDefaults()...");
#endif
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0.0f, 0.0f);
        }
    }
}




#if false
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
#endif
