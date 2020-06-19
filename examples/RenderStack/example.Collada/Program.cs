using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Parameters;
using RenderStack.Scene;

using Matrix4 = RenderStack.Math.Matrix4;
using Vector3 = RenderStack.Math.Vector3;

namespace examples
{
    public class Application : GameWindow
    {
        #region shaders
        string vs = 
@"#version 330                                                          
                                                                        
in vec3 _position;                                                      
in vec3 _normal;                                                        
                                                                        
uniform mat4 _model_to_clip_matrix;                                     
uniform mat4 _model_to_world_matrix;                                    
                                                                        
out vec3 v_normal;                                                      
                                                                        
void main()                                                             
{                                                                       
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);         
    v_normal    = vec3(_model_to_world_matrix * vec4(_normal, 0.0));    
}                                                                       ";

        string fs = 
@"#version 330                                                          
                                                                        
uniform vec3  _light_direction;                                         
uniform vec3  _light_color;                                             
uniform vec3  _surface_color;                                           
                                                                        
in vec3 v_normal;                                                       
                                                                        
out vec4 out_color;                                                     
                                                                        
void main(void)                                                         
{                                                                       
    vec3  N          = normalize(v_normal);                             
    vec3  L          = _light_direction.xyz;                            
    float ln         = dot(L, N);                                       
    float ln_clamped = max(ln, 0.0);                                    
                                                                        
    out_color.rgb = _surface_color * _light_color * ln_clamped;         
    out_color.a   = 1.0;                                                
}                                                                       ";
        #endregion shaders

        Mesh                mesh;
        Program             program;
        Viewport            viewport;
        MeshMode            meshMode = MeshMode.PolygonFill;

        Camera              camera              = new Camera();
        Frame               modelFrame          = new Frame();
        Dictionary<string, IUniformValue>  
                            parameters          = new Dictionary<string,IUniformValue>();
        UniformMappings     uniformMappings     = new UniformMappings();
        AttributeMappings   attributeMappings   = new AttributeMappings();

        public Application(OpenTK.DisplayDevice display)
        :   base(
            640, 
            480,
            new GraphicsMode(), 
            "RenderStack", 
            0,
            display, 
            2, 
            1,
            GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug
        )
        {
        }

        protected override void OnLoad(System.EventArgs e)
        {
            mesh = new GeometryMesh(
                new ColladaGeometry("cube.dae"),
                NormalStyle.CornerNormals
            ).GetMesh;

            viewport = new Viewport(base.Width, base.Height);

            modelFrame.LocalToParent.Set(
                Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f)
            );

            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    new Vector3(0.0f, 0.0f, -4.0f),     //  camera location
                    new Vector3(0.0f, 0.0f,  0.0f),     //  aim point
                    new Vector3(0.0f, 1.0f,  0.0f)      //  up vector
                )
            );

            camera.FovYRadians      = RenderStack.Math.Conversions.DegreesToRadians(60.0f);
            camera.ProjectionType   = ProjectionType.PerspectiveVertical;

            program = new Program(vs, fs);

            attributeMappings.Add("_position", VertexUsage.Position, 0, 3);
            attributeMappings.Add("_normal",   VertexUsage.Normal,   0, 3);

            program.AttributeMappings = attributeMappings;

            /*                   name in shader            uniform  */ 
            UniformMappings.Add("_model_to_world_matrix",  LogicalUniform.ModelToWorld);
            UniformMappings.Add("_world_to_model_matrix",  LogicalUniform.WorldToModel);
            UniformMappings.Add("_model_to_clip_matrix",   LogicalUniform.ModelToClip);
            UniformMappings.Add("_clip_to_model_matrix",   LogicalUniform.ClipToModel);
            UniformMappings.Add("_world_to_clip_matrix",   LogicalUniform.WorldToClip);
            UniformMappings.Add("_clip_to_world_matrix",   LogicalUniform.ClipToWorld);
            UniformMappings.Add("_view_position_in_world", LogicalUniform.ViewPositionInWorld);

            /*                  type     name in shader      parameter name   */ 
            UniformMappings.Add<Floats>("_light_direction", "light_direction");
            UniformMappings.Add<Floats>("_light_color",     "light_color"    );
            UniformMappings.Add<Floats>("_surface_color",   "surface_color"  );

            parameters["surface_color"]   = new Floats(1.0f, 1.0f, 1.0f);
            parameters["light_direction"] = new Floats(0.0f, 1.0f, 0.0f);
            parameters["light_color"]     = new Floats(1.0f, 1.0f, 1.0f);

            program.Bind(camera);
            program.Bind(parameters);

            //  Setup initial GL state
            {
                //  Since we only use one program, we don't
                //  need to touch this after.
                program.Use();
                program.ApplyUniforms();

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);

                GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);

                //  Since we only use one mesh, we don't 
                //  need to change this after
                mesh.ApplyAttributes(program, meshMode);
            }

            Resize += new EventHandler<EventArgs>(Application_Resize);
            Unload += new EventHandler<EventArgs>(Application_Unload);
        }

        void Application_Unload(object sender, EventArgs e)
        {
            if(mesh != null)
            {
                mesh.Dispose();
                mesh = null;
            }
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
            camera.UpdateViewport(viewport);
            camera.UpdateModelFrame(modelFrame);

            //  Is is also necessary to upload changed uniforms
            program.ApplyUniforms();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.DrawElements(
                mesh.IndexBuffer(meshMode).BeginMode,
                (int)(mesh.IndexBuffer(meshMode).Count),
                mesh.IndexBuffer(meshMode).DrawElementsType,
                0
            );

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