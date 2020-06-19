using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;

using Matrix4 = RenderStack.Math.Matrix4;
using Vector2 = RenderStack.Math.Vector2;
using Vector3 = RenderStack.Math.Vector3;
using Sphere = RenderStack.Geometry.Shapes.Sphere;

namespace example.Simple
{
    public class Application : GameWindow
    {
        #region shaders
        string vs = 
@"//  Schlick.vs

#version 150

in vec3 _position;
in vec3 _normal;
in vec3 _color;

out vec3 v_normal;
out vec3 v_view_direction;
out vec3 v_color;

UNIFORMS;

void main()
{
    mat4 model_to_world_matrix = models.model_to_world_matrix[gl_InstanceID];
    mat4 model_to_clip_matrix = camera.world_to_clip_matrix * models.model_to_world_matrix[gl_InstanceID];
    mat4 model_to_view_matrix = camera.world_to_view_matrix * models.model_to_world_matrix[gl_InstanceID];
    mat4 view_to_world_matrix = camera.view_to_world_matrix;
    vec3 view_position_in_world = camera.view_position_in_world.xyz;

    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    v_normal    = vec3(model_to_world_matrix * vec4(_normal, 0.0));

    vec4 position = model_to_world_matrix * vec4(_position, 1.0);

    v_view_direction = view_position_in_world.xyz - position.xyz;
    v_color = _color;
}
";

        string fs = 
@"//  Schlick.fs

#version 150

in  vec3    v_view_direction;
in  vec3    v_color;
in  vec3    v_normal;

out vec4    out_color;

UNIFORMS;

void main(void)
{
    float r             = material.surface_roughness;
    float one_minus_r   = 1.0 - r;

    vec3  N             = normalize(v_normal);
    vec3  V             = normalize(v_view_direction);

    float vn            = dot(V, N);
    float vn_clamped    = max(vn, 0.0);
    float GvnDenom      = r + one_minus_r * vn_clamped;     //  <=>  r + vn - (r * vn)  <=>  r - (r * vn) + vn

    vec3  surface_diffuse_reflectance  = material.surface_diffuse_reflectance_color.rgb;
    vec3  surface_specular_reflectance = material.surface_specular_reflectance_color.rgb;

    vec3  surface_diffuse_reflectance_linear    = surface_diffuse_reflectance * surface_diffuse_reflectance;
    vec3  surface_specular_reflectance_linear   = surface_specular_reflectance * surface_specular_reflectance;

    // S(HV) = C + (1 - C)(1 - VN)^5
    vec3 C = surface_diffuse_reflectance.rgb;
    vec3 white_minus_C = vec3(1.0, 1.0, 1.0) - C;
    vec3 S = C + white_minus_C * pow(1.0 - vn_clamped, 5.0);

    vec3 exitant_radiance = vec3(0.0);

    vec3    L   = lights.direction[0].xyz;
    float   ln  = dot(L, N);

    for(int i = 0; i < 3; ++i)
    {
        vec3    L   = lights.direction[i].xyz;
        float   ln  = dot(L, N);

        if(ln > 0.0)
        {
            float ln_clamped    = max(ln, 0.0);
            vec3  H             = normalize(L + V);
            float hn            = dot(H, N);
            float hv            = dot(H, V);
            float hn_clamped    = max(hn, 0.0);
            float hv_clamped    = max(hv, 0.0);
            float hn2           = hn * hn;

            float GlnDenom      = (r   + one_minus_r * ln);
            float Zhn0          = (1.0 - one_minus_r * hn * hn);
            float D             = max((ln * r) / (GvnDenom * GlnDenom * Zhn0 * Zhn0), 0.0);

            float light_visibility      = 1.0;
            vec3  light_color_linear    = lights.color[i].rgb * lights.color[i].rgb;
            vec3  light_radiance        = light_color_linear * lights.color[i].a;
            vec3  incident_radiance     = light_radiance * light_visibility;

            exitant_radiance += incident_radiance * surface_diffuse_reflectance_linear * ln_clamped;   //  diffuse
            exitant_radiance += incident_radiance * S * D;          //  specular
        }
    }

    vec3    ambient_light_color_linear = lights.ambient_light_color.rgb * lights.ambient_light_color.rgb;
    vec3    ambient_factor             = surface_diffuse_reflectance;
    float   ambient_visibility         = min(v_color.r, 1.0 + min(N.y, 0.0));
    
    exitant_radiance                  += ambient_factor * ambient_light_color_linear * ambient_visibility;

    out_color.rgb = vec3(1.0) - exp(-exitant_radiance * lights.exposure.x);
    //out_color.rgb = exitant_radiance;

    //out_color.rgb = sqrt(out_color.rgb);
    out_color.rgb = pow(out_color.rgb, vec3(1.0 / 2.2));

    out_color.rgb  *= global.alpha;
    //out_color.rgb  *= global.alpha;
    //out_color.rgb  *= 0.01;
    //out_color.rgb  += lights.color[0].rgb * vec3(ln);
    //out_color.a     = global.alpha;
    //out_color.rgba += global.add_color;
}
";
        #endregion shaders
        public Application(OpenTK.DisplayDevice display)
        :   base(
            640, 
            480,
            new GraphicsMode(), 
            "RenderStack", 
            0,
            display, 
            3, 2, GraphicsContextFlags.Default
            //  Test forward compatible during development, but never ship with it set
            //3, 3, GraphicsContextFlags.ForwardCompatible
        )
        {
        }

        private UniformBlockGL      ModelsUB;
        private UniformBlockGL      GlobalUB;
        private UniformBlockGL      MaterialUB;
        private UniformBlockGL      LightsUB;
        private IUniformBuffer[]    Materials = new IUniformBuffer[10];
        private IUniformBuffer      global;
        private IUniformBuffer      lights;
        private IUniformBuffer      models;
        private Mesh                mesh;
        private IProgram            program;
        private Viewport            viewport;
        private Camera              camera;
        private Frame[]             modelFrame;

        private float   radius              = 1.4f;
        private int     maxInstanceCount    = 500;       //  How many instances can be drawn with one draw call
        private int     lightCount          = 3;
        private float   poissonRadius       = 20.0f;
        private float   poissonMinDistance  = 1.5f;

        private ulong   serial = 1;
        private int     modelCount;                     //  How many models in scene

        protected override void OnLoad(System.EventArgs e)
        {
            //  Check GL features
            RenderStack.Graphics.Configuration.Initialize();
            if(RenderStack.Graphics.Configuration.glslVersion < 150)
            {
                throw new System.PlatformNotSupportedException("You need at least GL 3.2");
            }

            AttributeMappings.Global.Add(0, "_position", VertexUsage.Position, 0, 3);
            AttributeMappings.Global.Add(1, "_normal",   VertexUsage.Normal,   0, 3);

            //  Build uniform blocks
            {
                System.Text.StringBuilder uniforms = new System.Text.StringBuilder();
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
                uniforms.Append(CameraUniformBlock.UniformBlockGL.SourceGL);

                ModelsUB = new UniformBlockGL("models");
                ModelsUB.AddMat4("model_to_world_matrix", maxInstanceCount);
                ModelsUB.Seal();
                uniforms.Append(ModelsUB.SourceGL);

                GlobalUB = new UniformBlockGL("global");
                GlobalUB.AddVec4("add_color");
                GlobalUB.AddFloat("alpha");
                GlobalUB.Seal();
                uniforms.Append(GlobalUB.SourceGL);

                MaterialUB = new UniformBlockGL("material");
                MaterialUB.AddVec4("surface_diffuse_reflectance_color");
                MaterialUB.AddVec4("surface_specular_reflectance_color");
                MaterialUB.AddFloat("surface_roughness");
                MaterialUB.Seal();
                uniforms.Append(MaterialUB.SourceGL);

                LightsUB = new UniformBlockGL("lights");
                LightsUB.AddVec4("exposure");
                LightsUB.AddVec4("color", lightCount);
                LightsUB.AddVec4("ambient_light_color");
                LightsUB.AddVec4("direction", lightCount);
                LightsUB.AddMat4("world_to_shadow_matrix", lightCount);
                LightsUB.Seal();
                uniforms.Append(LightsUB.SourceGL);

                ShaderGL3.Replace("UNIFORMS;", uniforms.ToString());
            }

            //  Models
            List<Vector2> positions = UniformPoissonDiskSampler.SampleCircle(Vector2.Zero, poissonRadius, poissonMinDistance);
            modelCount = positions.Count;
            System.Diagnostics.Debug.WriteLine("Model count: " + modelCount);
            modelFrame = new Frame[modelCount];
            for(int i = 0; i < modelCount; ++i)
            {
                modelFrame[i] = new Frame();
                modelFrame[i].LocalToParent.Set(
                    Matrix4.CreateTranslation(positions[i].X, positions[i].Y, -40.0f)
                );
            }

            //  Setup geometry and mesh
            Geometry geometry = new Sphere(radius, 20, 8);
            //Matrix4 translate = Matrix4.CreateTranslation(0.0f, 0.0f, -4.0f);
            //geometry.Transform(translate);
            mesh = new GeometryMesh(geometry, NormalStyle.PointNormals).GetMesh;
            //  \todo Get rid of this
            //mesh.VertexBufferRange.Buffer.VertexFormat.Seal(AttributeMappings.Global);

            //  This is where we collect models transformations
            models = UniformBufferFactory.Create(ModelsUB);

            global = UniformBufferFactory.Create(GlobalUB);
            global.Floats("alpha").Set(1.0f);
            global.Floats("add_color").Set(0.0f, 0.0f, 0.4f);
            global.Sync();

            //  Setup lights
            lights = UniformBufferFactory.Create(LightsUB);
            lights.Floats("exposure").Set(1.0f);
            lights.Floats("ambient_light_color").Set(1.0f, 1.0f, 1.0f);
            for(int i = 0; i < lightCount; ++i)
            {
                lights.Floats("direction").SetI(i, 0.0f, 1.0f, 0.0f);
                lights.Floats("color").SetI(i, 1.0f, 1.0f, 1.0f, 1.0f / (float)lightCount);
            }
            lights.Sync();

            global.Use();
            models.Use();
            lights.Use();

            //  Setup viewport and camera
            viewport = new Viewport(base.Width, base.Height);
            camera = new Camera();
            camera.Projection.Near = 0.1f;
            camera.Projection.Far = 300.0f;
            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    new Vector3(0.0f, 0.0f,  4.0f),     //  camera location
                    new Vector3(0.0f, 0.0f,  0.0f),     //  aim point
                    new Vector3(0.0f, 1.0f,  0.0f)      //  up vector
                )
            );

            camera.Projection.FovYRadians      = RenderStack.Math.Conversions.DegreesToRadians(60.0f);
            camera.Projection.ProjectionType   = ProjectionType.PerspectiveVertical;
            camera.UniformBufferGL.Use();

            //  Setup shader program
            program = (IProgram)new ProgramGL3(vs, fs);

            //  Setup material
            for(int i = 0; i < Materials.Length; ++i)
            {
                var material = UniformBufferFactory.Create(MaterialUB);
                float h = (float)(i) / (float)(Materials.Length - 1);
                float r, g, b;
                Conversions.HSVtoRGB(360.0f * h, 1.0f, 1.0f, out r, out g, out b);
                float diffuse = 0.2f;
                float specular = 0.8f;
                float roughness = 0.1f;
                material.Floats("surface_diffuse_reflectance_color"    ).Set(diffuse * r, diffuse * g, diffuse * b);
                material.Floats("surface_specular_reflectance_color"   ).Set(specular * r, specular * r, specular *r);
                material.Floats("surface_roughness"                    ).Set(roughness);
                material.Sync();
                Materials[i] = material;
            }

            //  Update scene. There is no animation in this scene
            //  so we only need to do it once.
            camera.Frame.UpdateHierarchical(serial);
            foreach(var frame in modelFrame)
            {
                frame.UpdateHierarchical(serial);
            }

            //  Setup initial GL state
            {
                //  Since we only use one program, we don't need to touch this after.
                camera.UpdateFrame();
                camera.UpdateViewport(viewport);
                camera.UniformBufferGL.Sync();

                //  Bind program
                program.Use(0);

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);

                float gammaHalf = (float)Math.Pow(0.5, 1.0 / 2.2);
                GL.ClearColor(gammaHalf, gammaHalf, gammaHalf, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                SwapBuffers();

                Visible = true;

                //  Since we only use one mesh, we don't need to change this after
                SetupAttributeBindings();

                //  The mesh has not yet been uploaded to GL, do it now
                IBufferRange vertexBufferRange = mesh.VertexBufferRange;
                IBufferRange indexBufferRange = mesh.IndexBufferRange(MeshMode.PolygonFill);
                if(vertexBufferRange.NeedsUploadGL)
                {
                    vertexBufferRange.UpdateGL();
                }
                if(indexBufferRange.NeedsUploadGL)
                {
                    indexBufferRange.UpdateGL();
                }
            }

            Resize += new EventHandler<EventArgs>(Application_Resize);
            Unload += new EventHandler<EventArgs>(Application_Unload);
        }

        private void SetupAttributeBindings()
        {
            var vertexFormat = mesh.VertexBufferRange.VertexFormat;
            VertexStreamGL attributeBindings = mesh.VertexBufferRange.VertexStreamGL(program.AttributeMappings);
            if(
                RenderStack.Graphics.Configuration.mustUseVertexArrayObject ||
                (
                    RenderStack.Graphics.Configuration.canUseVertexArrayObject &&
                    RenderStack.Graphics.Configuration.useVertexArrayObject
                )
            )
            {
                attributeBindings.Use();
                if(attributeBindings.Dirty)
                {
                    mesh.VertexBufferRange.UseGL();
                    mesh.IndexBufferRange(MeshMode.PolygonFill).UseGL();
                    attributeBindings.SetupAttributePointers();
                    attributeBindings.Dirty = false;
                }
            }
            else
            {
                mesh.VertexBufferRange.UseGL();
                mesh.IndexBufferRange(MeshMode.PolygonFill).UseGL();
                attributeBindings.SetupAttributePointers();
            }
        }

        void Application_Unload(object sender, EventArgs e)
        {
            BufferPool.Instance.Dispose();
            if(program != null)
            {
                program.Dispose();
                program = null;
            }
        }

        void Application_Resize(object sender, EventArgs e)
        {
            //  When window size is changed, we need to update
            //  GL viewport and projection matrix and any
            //  other matrices that depend on it.
            viewport.Resize(this.Width, this.Height);

            GL.Viewport(
                (int)viewport.X, 
                (int)viewport.Y, 
                (int)viewport.Width, 
                (int)viewport.Height
            );

            //  These change camera uniforms
            camera.UpdateViewport(viewport);

            //  Is is also necessary to upload changed uniforms to uniform buffer
            camera.UniformBufferGL.Sync();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        static long start = Environment.TickCount;

        public static float Now 
        {
            get
            {
                long ticks = Environment.TickCount - start;
                long ms = ticks;
                float s = (float)ms / 1000.0f;
                return s;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            ++serial;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            float z = 20.0f * (float)(System.Math.Sin(2.0f * Now));
            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    new Vector3(0.0f, 0.0f,      z),      //  camera location
                    new Vector3(0.0f, 0.0f, -100.0f),     //  aim point
                    new Vector3(0.0f, 1.0f,    0.0f)      //  up vector
                )
            );
            camera.Frame.UpdateHierarchical(serial);
            camera.UpdateFrame();
            camera.UpdateViewport(viewport);
            camera.UniformBufferGL.Sync();

            int baseVertex = mesh.VertexBufferRange.BaseVertex;
            IBufferRange indexBufferRange = mesh.IndexBufferRange(MeshMode.PolygonFill);

            if(RenderStack.Graphics.Configuration.canUseInstancing)
            {
                //  Instancing code path
                int i = 0;  //  index to models uniform block
                int j = 0;  //  index to material (changed for each draw call)
                for(int mi = 0; mi < modelCount; ++mi)
                {
                    models.Floats("model_to_world_matrix").SetI(
                        i, modelFrame[mi].LocalToWorld.Matrix
                    );
                    i++;
                    if(
                        (i == maxInstanceCount) || 
                        (mi == modelCount - 1)
                    )
                    {
                        models.Sync();
                        Materials[j].Use();
                        GL.DrawElementsInstanced(
                            indexBufferRange.BeginMode, 
                            (int)(indexBufferRange.Count), 
                            indexBufferRange.DrawElementsTypeGL, 
                            (IntPtr)0, 
                            i
                        );
                        i = 0;
                        ++j;
                        if(j == Materials.Length)
                        {
                            j = 0;
                        }
                    }
                }
            }
            else
            {
                int j = 0;  //  index to material (changed for each draw call)
                for(int i = 0; i < modelCount; ++i)
                {
                    Materials[j].Use();
                    models.Floats("model_to_world_matrix").SetI(
                        0, modelFrame[i].LocalToWorld.Matrix
                    );
                    models.Sync();
                    GL.DrawElements(
                        indexBufferRange.BeginMode, 
                        (int)(indexBufferRange.Count), 
                        indexBufferRange.DrawElementsTypeGL, 
                        (IntPtr)0
                    );
                    ++j;
                    if(j == Materials.Length)
                    {
                        j = 0;
                    }
                }
            }

            SwapBuffers();
        }

        [STAThread]
        public static void Main()
        {
            OpenTK.DisplayDevice chosenDisplay = OpenTK.DisplayDevice.Default;

            using (var application = new Application(chosenDisplay))
            {
                application.Title = "example.Simple";

                application.Run(30);
            }
        }
    }
}