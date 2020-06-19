#if true

#define DEBUG_RENDERER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

//using Caustic.OpenRL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Renderer
{
    public class RendererRL : RenderStack.Services.Service, IRenderer
    {
        #region Service, initialization etc.
        public override string Name
        {
            get { return "RendererRL"; }
        }

        OpenTK.GameWindow window;

        public void Connect(OpenTK.GameWindow window)
        {
            this.window = window;
        }

        protected override void InitializeService()
        {
            InitializeShaderSystem();
            InitializeAttributeMappings();
            programs = new Programs();
            //window.Resize += new EventHandler<EventArgs>(window_Resize); 
        }
        // \brief Connect shader attribute names to RenderStack vertex format.
        private void InitializeAttributeMappings()
        {
            var attributeMappings = AttributeMappings.Global;
            attributeMappings.Clear();
            attributeMappings.Add( 0, "_position",          VertexUsage.Position, 0, 3);
            attributeMappings.Add( 1, "_normal",            VertexUsage.Normal,   0, 3);
            attributeMappings.Add( 2, "_texcoord",          VertexUsage.TexCoord, 0, 2);
            attributeMappings.Add( 3, "_color",             VertexUsage.Color,    0, 4);
            attributeMappings.Add( 4, "_tangent",           VertexUsage.Tangent,  0, 3);
            attributeMappings.Add( 5, "_polygon_normal",    VertexUsage.Normal,   1, 3);
            attributeMappings.Add( 6, "_normal_smooth",     VertexUsage.Normal,   2, 3);
            attributeMappings.Add( 7, "_t",                 VertexUsage.Color,    1, 1);
            attributeMappings.Add( 8, "_edge_color",        VertexUsage.Color,    1, 4);
            attributeMappings.Add( 9, "_id_uint",           VertexUsage.Id,       0, 1);
            attributeMappings.Add(10, "_id_vec3",           VertexUsage.Id,       0, 3);
        }
        // \brief Declare uniforms and set Shader replacement strings
        private void InitializeShaderSystem()
        {
            CameraUniformBlock.NearFar              = "near_far";
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

            System.Text.StringBuilder uniforms = new System.Text.StringBuilder();
            uniforms.Append(CameraUniformBlock.UniformBlockRL.SourceRL);
            uniforms.Append(LightsUniforms.UniformBlockRL.SourceGL);

            modelsUB = new UniformBlockRL("models");
            modelsUB.AddMat4("model_to_world_matrix", Configuration.instanceCount);
            if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
            {
                modelsUB.AddUInt("id_offset_uint", Configuration.instanceCount);
                ShaderGL3.Replace("#if USE_INTEGER_POLYGON_ID", "#if 1");
            }
            else
            {
                modelsUB.AddVec4("id_offset_vec3", Configuration.instanceCount);
                ShaderGL3.Replace("#if USE_INTEGER_POLYGON_ID", "#if 0");
            }
            modelsUB.Seal();
            uniforms.Append(modelsUB.ToString());

            globalUB = new UniformBlockRL("global");
            globalUB.AddVec4("add_color");
            globalUB.AddFloat("alpha");
            globalUB.Seal();
            uniforms.Append(globalUB.ToString());

            uniforms.Append(LightsUniforms.UniformBlockRL.SourceRL);
            LightsUniforms.UniformBufferRL.Use();

            ShaderGL3.Replace("MAX_LIGHT_COUNT", Configuration.maxLightCount.ToString());

            models = UniformBufferFactory.Create(modelsUB);
            global = UniformBufferFactory.Create(globalUB);

            global.Floats("alpha").Set(1.0f);
            global.Floats("add_color").Set(0.0f, 0.0f, 0.4f);
            global.Sync();

            LightsUniforms.Exposure.Set(1.0f);
            LightsUniforms.Bias.Set(-0.002f, 0.0f, 0.002f);
            LightsUniforms.AmbientLightColor.Set(0.2f, 0.2f, 0.2f);
            LightsUniforms.UniformBufferRL.Sync();

            models.Use();
            global.Use();

            var nearestClampToEdge = new SamplerRL();
            nearestClampToEdge.MinFilter    = OpenTK.Graphics.OpenGL.TextureMinFilter.Nearest;
            nearestClampToEdge.MagFilter    = OpenTK.Graphics.OpenGL.TextureMagFilter.Nearest;
            nearestClampToEdge.Wrap         = OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge;
            nearestClampToEdge.CompareMode  = OpenTK.Graphics.OpenGL.TextureCompareMode.None;

            var bilinearClampToEdge = new SamplerRL();
            bilinearClampToEdge.MinFilter   = OpenTK.Graphics.OpenGL.TextureMinFilter.NearestMipmapLinear;
            bilinearClampToEdge.MagFilter   = OpenTK.Graphics.OpenGL.TextureMagFilter.Linear;
            bilinearClampToEdge.Wrap        = OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge;
            bilinearClampToEdge.CompareMode = OpenTK.Graphics.OpenGL.TextureCompareMode.None;

            Samplers.Global.AddSampler2D("t_font", nearestClampToEdge);
            Samplers.Global.AddSampler2D("t_ninepatch", bilinearClampToEdge).TextureUnitIndex = 1;
            Samplers.Global.AddSampler2D("t_cube", bilinearClampToEdge);
            Samplers.Global.AddSampler2D("t_left", nearestClampToEdge);
            Samplers.Global.AddSampler2D("t_right", nearestClampToEdge).TextureUnitIndex = 1;
            Samplers.Global.AddSampler2D("t_surface_color", bilinearClampToEdge);
            Samplers.Global.AddSampler2D("t_particle", bilinearClampToEdge);
            Samplers.Global.AddSampler2DArray("t_shadowmap_vis", bilinearClampToEdge);
            Samplers.Global.Seal();

            materialUB = new UniformBlockRL("material");
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
            uniforms.Append(MaterialUB.ToString());
            uniforms.Append(Samplers.Global.ToString());

            //  Have these last
            ShaderGL3.Replace("UNIFORMS;", uniforms.ToString());
            ShaderGL3.Replace("INSTANCE_COUNT", "50");
        }
        public void Unload()
        {
            Programs.Dispose();
        }
        #endregion Service initialization

        #region Data members

        private Programs                programs;
        private Stack<State>            requestStack = new Stack<State>();
        private State                   requested = new State();
        private State                   effective = new State();
        private State                   Effective       { get { return effective; } }
        private UniformBlockRL          modelsUB;
        private UniformBlockRL          globalUB;
        private UniformBlockRL          materialUB;
        private IUniformBuffer          models;
        private IUniformBuffer          global;
        private Group                   currentGroup;
        private Timers                  timers = new Timers();
        private Frame                   defaultFrame = new Frame();
        private bool                    lockMaterial = false;

        public  int Width { get { return window.Width; } }
        public  int Height { get { return window.Height; } }
        public  event EventHandler<EventArgs> Resize = delegate { };
        public  Programs                Programs        { get { return programs; } }
        public  State                   Requested       { get { return requested; } }
        public  IUniformBlock           MaterialUB      { get { return materialUB; } }
        public  IUniformBuffer          Models          { get { return models; } }
        public  IUniformBuffer          Global          { get { return global; } }
        public  bool                    LockMaterial    { get { return lockMaterial; } set { lockMaterial = value; } }
        public  Group                   CurrentGroup    { get { return currentGroup; } set { currentGroup = value; } }
        public  Timers                  Timers          { get { return timers; } }
        public  Frame                   DefaultFrame    { get { return defaultFrame; } }
        #endregion

        public void HandleResize()
        {
            if(Resize != null)
            {
                Resize(this, null);
            }
        }

        // \brief Find a sampler based on key and set it to use the specified texture
        // \note This does not change any Program state; Programs are assumed to be already associated with samplers
        public void SetTexture(string key, TextureGL texture)
        {
            throw new NotImplementedException();
        }

        public void UnbindTexture(string key, TextureGL texture)
        {
            throw new NotImplementedException();
        }

        // \brief Set a single frame to be used when rendering current state. Not used when rendering Groups.
        public void SetFrame(Frame frame)
        {
            models.Floats("model_to_world_matrix").SetI(
                0, frame.LocalToWorld.Matrix
            );
            models.Sync();
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
            Requested.Camera.UniformBufferGL.Sync();
            Requested.Camera.UniformBufferGL.Use();

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

                int i = 0;
                foreach(var model in models2)
                {
#if DEBUG_RENDERER
                    RenderStack.Graphics.Debug.WriteLine("render " + model.Name);
#endif
                    models.Floats("model_to_world_matrix").SetI(
                        i, model.Frame.LocalToWorld.Matrix
                    );
                    i++;
                    if(i > (Configuration.instanceCount - 1))
                    {
                        models.Sync();
                        RenderCurrent(i);
                        i = 0;
                    }
                }
                if(i > 0)
                {
                    models.Sync();

                    if(i > 1)
                    {
                        RenderCurrent(i);
                    }
                    else
                    {
                        RenderCurrent();
                    }
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
#if false
            foreach(var light in CurrentGroup.Lights)
            {
                light.UpdateFrame();
            }
            LightsUniforms.UniformBufferRL.Sync();
            LightsUniforms.UniformBufferRL.Use();
#endif
            RenderGroupInstances(CurrentGroup.OpaqueInstances);
            RenderGroupInstances(CurrentGroup.TransparentInstances);
#if DEBUG_RENDERER
            RenderStack.Graphics.Debug.WriteLine("----- RenderGroup End -----");
#endif
        }

        public void RenderCurrentDebug()
        {
            using(var t = new TimerScope(timers.MaterialSwitch))
            {
                if(Effective.Material != Requested.Material)
                {
                    Requested.Material.UseDebug();
                    Effective.Material = Requested.Material;
                }
            }
/*
            using(var t = new TimerScope(timers.ProgramSwitch))
            {
                if(Effective.Program != Requested.Program)
                {
                    Requested.Program.Use();
                    Effective.Program = Requested.Program;
                }
            }

            using(var t = new TimerScope(timers.AttributeSetup))
            {
                BindAttributesAndCheckForUpdates();
            }

            BufferRange indexBufferRange = Effective.Mesh.IndexBufferRange(Effective.MeshMode);

            using(var t = new TimerScope(timers.DrawCalls))
            {
                GL.DrawElementsBaseVertex(
                    indexBufferRange.BeginMode,
                    (int)indexBufferRange.Count,
                    indexBufferRange.Buffer.DrawElementsType,
                    (IntPtr)(indexBufferRange.OffsetBytes),
                    Effective.Mesh.VertexBufferRange.BaseVertex 
                );
            }*/
        }
        public void RenderCurrent()
        {
            using(var t = new TimerScope(timers.MaterialSwitch))
            {
                if(Effective.Material != Requested.Material)
                {
                    Requested.Material.Use();
                    Effective.Material = Requested.Material;
                }
            }

            using(var t = new TimerScope(timers.ProgramSwitch))
            {
                if(Effective.Program != Requested.Program)
                {
                    Requested.Program.Use(0);
                    Effective.Program = Requested.Program;
                }
            }

            using(var t = new TimerScope(timers.AttributeSetup))
            {
                BindAttributesAndCheckForUpdates();
            }

            IBufferRange indexBufferRange = Effective.Mesh.IndexBufferRange(Effective.MeshMode);

            using(var t = new TimerScope(timers.DrawCalls))
            {
                /*
                RL.DrawElementsBaseVertex(
                    indexBufferRange.BeginMode,
                    (int)indexBufferRange.Count,
                    indexBufferRange.DrawElementsType,
                    (IntPtr)(indexBufferRange.OffsetBytes),
                    Effective.Mesh.VertexBufferRange.BaseVertex 
                );*/
                RL.DrawElements(
                    (Caustic.OpenRL.BeginMode)indexBufferRange.BeginMode,
                    (int)indexBufferRange.Count,
                    (Caustic.OpenRL.DrawElementsType)indexBufferRange.DrawElementsTypeGL,
                    (int)indexBufferRange.OffsetBytes
                );
            }
        }
        public void RenderInstancedPrepare()
        {
            using(var t = new TimerScope(timers.MaterialSwitch))
            {
                if(Effective.Material != Requested.Material)
                {
                    Requested.Material.Use();
                    Effective.Material = Requested.Material;
                }
            }

            using(var t = new TimerScope(timers.ProgramSwitch))
            {
                if(Effective.Program != Requested.Program)
                {
                    Requested.Program.Use(0);
                    Effective.Program = Requested.Program;
                }
            }

            using(var t = new TimerScope(timers.AttributeSetup))
            {
                BindAttributesAndCheckForUpdates();
            }
        }
        public void RenderCurrent(int instanceCount)
        {
            IBufferRange indexBufferRange = Effective.Mesh.IndexBufferRange(Effective.MeshMode);

            using(var t = new TimerScope(timers.DrawCalls))
            {
                /*GL.DrawElementsInstancedBaseVertex(
                    indexBufferRange.BeginMode,
                    (int)indexBufferRange.Count,
                    indexBufferRange.Buffer.DrawElementsType,
                    (IntPtr)(indexBufferRange.OffsetBytes),
                    instanceCount,
                    Effective.Mesh.VertexBufferRange.BaseVertex
                );*/
                RL.DrawElements(
                    (Caustic.OpenRL.BeginMode)indexBufferRange.BeginMode,
                    (int)indexBufferRange.Count,
                    (Caustic.OpenRL.DrawElementsType)indexBufferRange.DrawElementsTypeGL,
                    (int)indexBufferRange.OffsetBytes
                );
            }
        }
        public void BindAttributesAndCheckForUpdates()
        {
            var vertexFormat = Requested.Mesh.VertexBufferRange.VertexFormat;
            Requested.VertexStream = Requested.Mesh.VertexBufferRange.VertexStreamRL(Effective.Program);
            Requested.IndexBufferRange = Requested.Mesh.IndexBufferRange(Requested.MeshMode);

            if(Effective.MeshMode != Requested.MeshMode)
            {
                Effective.MeshMode = Requested.MeshMode;
            }
            if(Effective.Mesh != Requested.Mesh)
            {
                Effective.Mesh = Requested.Mesh;
            }

            if(Effective.VertexStream != Requested.VertexStream)
            {
                DisableEffectiveAttributeBindings();
                SetupAttributeBindings();
                Effective.VertexStream = Requested.VertexStream;
            }
            CheckForBufferUploads();
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
                Requested.VertexStream.Use();

                if(Requested.VertexStream.Dirty)
                {
                    Requested.IndexBufferRange.UseRL();
                    Requested.Mesh.VertexBufferRange.UseRL();
                    Requested.VertexStream.SetupAttributePointers();
                    Requested.VertexStream.Dirty = false;
                }
            }
            else
            {
                Requested.VertexBuffer = Requested.Mesh.VertexBufferRange.BufferRL;
                Requested.IndexBuffer = Requested.IndexBufferRange.BufferRL;

                bool bothMatch = true;
                if(Effective.VertexBuffer != Requested.VertexBuffer)
                {
                    Requested.VertexBuffer.UseRL();
                    Effective.VertexBuffer = Requested.VertexBuffer;
                    bothMatch = false;
                }
                if(Effective.IndexBuffer != Requested.IndexBuffer)
                {
                    Requested.IndexBuffer.UseRL();
                    Effective.IndexBuffer = Requested.IndexBuffer;
                    bothMatch = false;
                }

                if(bothMatch == false)
                {
                    Requested.VertexStream.SetupAttributePointers();
                }
            }
        }
        private void CheckForBufferUploads()
        {
            if(Requested.IndexBufferRange.NeedsUploadRL)
            {
                Requested.IndexBufferRange.UpdateRL();
            }
            if(Requested.Mesh.VertexBufferRange.NeedsUploadRL)
            {
                Requested.Mesh.VertexBufferRange.UpdateRL();
            }
        }
        private void DisableEffectiveAttributeBindings()
        {
            if(Effective.VertexStream == null)
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
                Effective.VertexStream.DisableAttributes();
            }
        }
        #endregion Render
        public void ApplyViewport()
        {
            //  \todo Use render state for viewport
            RL.Viewport(
                (int)Requested.Viewport.X, 
                (int)Requested.Viewport.Y, 
                (int)Requested.Viewport.Width, 
                (int)Requested.Viewport.Height
            );
            Effective.Viewport = Requested.Viewport;
        }
        public void RenderCurrentClear()
        {
            RL.ClearColor(0.34f, 0.34f, 0.34f, 1.0f);
            RL.Clear(ClearBufferMask.ColorBufferBit);
        }
        public void BeginScissorMouse(int px, int py)
        {
            throw new InvalidOperationException();
        }

        public void EndScissorMouse()
        {
            throw new InvalidOperationException();
        }

        public void PartialGLStateResetToDefaults()
        {
        }
    }
}
#endif