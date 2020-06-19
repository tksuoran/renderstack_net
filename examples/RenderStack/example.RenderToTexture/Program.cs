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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;

using Matrix4 = RenderStack.Math.Matrix4;
using Vector3 = RenderStack.Math.Vector3;

namespace example.RenderToTexture
{
    public class Application : GameWindow
    {
        //  The region below is for backwards compatibility only
        #region shaders120
        string vsDiffuse120 = 
@"#version 120                                                          
                                                                        
attribute vec3 _position;                                               
attribute vec3 _normal;                                                 
                                                                        
uniform mat4 _model_to_clip_matrix;                                     
uniform mat4 _model_to_world_matrix;                                    
                                                                        
varying vec3 v_normal;                                                  
                                                                        
void main()                                                             
{                                                                       
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);         
    v_normal    = vec3(_model_to_world_matrix * vec4(_normal, 0.0));    
}                                                                       ";

        string fsDiffuse120 = 
@"#version 120                                                          
                                                                        
uniform vec3  _light_direction;                                         
uniform vec3  _light_color;                                             
uniform vec3  _surface_color;                                           
                                                                        
varying vec3 v_normal;                                                  
                                                                        
void main(void)                                                         
{                                                                       
    vec3  N              = normalize(v_normal);                         
    vec3  L              = _light_direction.xyz;                        
    float ln             = dot(L, N);                                   
    float lnClamped      = max(ln, 0.0);                                
    vec3  linearRadiance = _surface_color * _light_color * lnClamped;   
    gl_FragData[0].rgb = pow(linearRadiance, vec3(1.0/2.2));            
    gl_FragData[0].a   = 1.0;                                           
}                                                                       ";
        string vsTextured120 =
@"#version 120                                                          
                                                                        
attribute vec3 _position;                                               
attribute vec2 _texcoord;                                               
                                                                        
uniform mat4 _model_to_clip_matrix;                                     
                                                                        
varying vec2 v_texcoord;                                                
varying vec4 v_color;                                                   
                                                                        
void main()                                                             
{                                                                       
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);         
    v_texcoord  = _texcoord.xy;                                         
}                                                                       ";
        string fsTextured120 =
@"#version 120                                                          
                                                                        
uniform sampler2D   _texture;                                           
                                                                        
uniform float       _t;                                                 
                                                                        
varying vec2 v_texcoord;                                                
                                                                                                                              
void main(void)                                                         
{                                                                       
    vec2  texcoord  = vec2(v_texcoord.x, v_texcoord.y);                 
    vec4  a         = texture2D(_texture, texcoord);                    
                                                                        
    gl_FragData[0] = _t * a;                                            
}                                                                       ";
        #endregion shaders120
        //  Forward compatible contexts must use newer GLSL version
        #region shaders330
        string vsDiffuse330 = 
@"#version 330                                                          
                                                                        
in vec3 _position;                                                      
in vec3 _normal;                                                        
                                                                        
out vec3 v_normal;                                                      
                                                                        
uniform mat4 _model_to_clip_matrix;                                     
uniform mat4 _model_to_world_matrix;                                    
                                                                        
void main()                                                             
{                                                                       
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);         
    v_normal    = vec3(_model_to_world_matrix * vec4(_normal, 0.0));    
}                                                                       ";

        string fsDiffuse330 = 
@"#version 330                                                          
                                                                        
in vec3 v_normal;                                                       
                                                                        
out vec4 out_color;                                                     
                                                                        
uniform vec3  _light_direction;                                         
uniform vec3  _light_color;                                             
uniform vec3  _surface_color;                                           
                                                                        
void main(void)                                                         
{                                                                       
    vec3  N              = normalize(v_normal);                         
    vec3  L              = _light_direction.xyz;                        
    float ln             = dot(L, N);                                   
    float lnClamped      = max(ln, 0.0);                                
    vec3  linearRadiance = _surface_color * _light_color * lnClamped;   
    out_color.rgb = pow(linearRadiance, vec3(1.0/2.2));                 
    out_color.a   = 1.0;                                                
}                                                                       ";
        string vsTextured330 =
@"#version 330                                                          
                                                                        
in vec3 _position;                                                      
in vec2 _texcoord;                                                      
                                                                        
out vec2 v_texcoord;                                                    
out vec4 v_color;                                                       
                                                                        
uniform mat4 _model_to_clip_matrix;                                     
                                                                        
void main()                                                             
{                                                                       
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);         
    v_texcoord  = _texcoord.xy;                                         
}                                                                       ";
        string fsTextured330 =
@"#version 330                                                          
                                                                        
in vec2 v_texcoord;                                                     
                                                                        
out vec4 out_color;                                                     
                                                                        
uniform sampler2D   _texture;                                           
uniform float       _t;                                                 
                                                                        
void main(void)                                                         
{                                                                       
    vec2  texcoord  = vec2(v_texcoord.x, v_texcoord.y);                 
    vec4  a         = texture2D(_texture, texcoord);                    
                                                                        
    out_color = _t * a;                                                 
}                                                                       ";
        #endregion shaders330

        Mesh                                sphereMesh;
        Mesh                                quadMesh;
        ProgramDeprecated                             diffuseProgram;
        ProgramDeprecated                             texturedProgram;
        Viewport                            windowViewport;
        Framebuffer                         framebuffer;
        Camera                              cameraOne           = new Camera();
        Camera                              cameraTwo           = new Camera();
        Frame                               sphereFrame         = new Frame();
        Frame                               quadFrame           = new Frame();
        Dictionary<string, IUniformValue>   parameters          = new Dictionary<string,IUniformValue>();
        AttributeMappings                   attributeMappings   = new AttributeMappings();

        public Application(OpenTK.DisplayDevice display)
        :   base(
            640, 
            480,
            new GraphicsMode(), 
            "RenderStack", 
            0,
            display, 
            2, 1, GraphicsContextFlags.Default
            //  In releases, ask for 3.3 only if you really need it.
            //  Test forward compatible during development, but never ship with it set
            //3, 3, GraphicsContextFlags.ForwardCompatible
        )
        {
        }

        protected override void OnLoad(System.EventArgs e)
        {
            //  Check GL version and feature capabilities
            RenderStack.Graphics.Configuration.Initialize();
            if(RenderStack.Graphics.Configuration.canUseFramebufferObject == false)
            {
                throw new System.PlatformNotSupportedException(
                    "GL version 3.0 or GL_ARB_framebuffer_object extension is needed. Neither was found."
                );
            }
            if(RenderStack.Graphics.Configuration.canUseBaseVertex == false)
            {
                throw new System.PlatformNotSupportedException(
                    "GL version 3.2 or GL_ARB_draw_elements_base_vertex extension is needed. Neither was found."
                );
            }

            sphereMesh  = new GeometryMesh(new Sphere(2.00f, 20, 20), NormalStyle.PointNormals).GetMesh;
            quadMesh    = new GeometryMesh(new QuadXY(1.0f, 1.0f, 0.0f), NormalStyle.PolygonNormals).GetMesh;

            //  Initialize shared resources
            attributeMappings.Add("_position",  VertexUsage.Position, 0, 3);
            attributeMappings.Add("_normal",    VertexUsage.Normal,   0, 3);
            attributeMappings.Add("_texcoord",  VertexUsage.TexCoord, 0, 2);

            UniformMappings.Add("_model_to_world_matrix",  LogicalUniform.ModelToWorld);
            UniformMappings.Add("_world_to_model_matrix",  LogicalUniform.WorldToModel);
            UniformMappings.Add("_model_to_clip_matrix",   LogicalUniform.ModelToClip);
            UniformMappings.Add("_clip_to_model_matrix",   LogicalUniform.ClipToModel);
            UniformMappings.Add("_world_to_clip_matrix",   LogicalUniform.WorldToClip);
            UniformMappings.Add("_clip_to_world_matrix",   LogicalUniform.ClipToWorld);
            UniformMappings.Add("_view_position_in_world", LogicalUniform.ViewPositionInWorld);

            UniformMappings.Add<Floats>("_light_direction", "light_direction"   );
            UniformMappings.Add<Floats>("_light_color",     "light_color"       );
            UniformMappings.Add<Floats>("_surface_color",   "surface_color"     );

            UniformMappings.Add<Texture>("_texture",        "texture"           );
            UniformMappings.Add<Floats> ("_t",              "t"                 );

            parameters["surface_color"]     = new Floats(1.0f, 1.0f, 1.0f);
            parameters["light_direction"]   = new Floats(0.0f, 1.0f, 0.0f);
            parameters["light_color"]       = new Floats(1.0f, 1.0f, 1.0f);
            parameters["t"]                 = new Floats(0.5f);

            //  Initialize resources used by the first pass (render to texture)
            framebuffer = new Framebuffer(128, 128);
            framebuffer.AttachTexture(FramebufferAttachment.ColorAttachment0, PixelFormat.Rgb, PixelInternalFormat.Rgb8);
            framebuffer.AttachRenderBuffer(FramebufferAttachment.DepthAttachment, PixelFormat.DepthComponent, RenderbufferStorage.DepthComponent32, 0);
            framebuffer.Begin();
            framebuffer.Check();
            framebuffer.End();

            cameraOne.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    new Vector3(0.0f, 0.0f, -4.0f),
                    new Vector3(0.0f, 0.0f,  0.0f),
                    new Vector3(0.0f, 1.0f,  0.0f)
                )
            );
            cameraOne.FovYRadians = RenderStack.Math.Conversions.DegreesToRadians(60.0f);
            cameraOne.ProjectionType = ProjectionType.PerspectiveVertical;

            ulong updateSerial = 1;
            cameraOne.Frame.UpdateHierarchical(updateSerial);
            quadFrame.UpdateHierarchical(updateSerial);

            cameraOne.UpdateFrame();
            cameraOne.UpdateViewport(framebuffer);
            cameraOne.UpdateModelFrame(quadFrame);

            if(RenderStack.Graphics.Configuration.glslVersion >= 330)
            {
                diffuseProgram = new ProgramDeprecated(vsDiffuse330, fsDiffuse330);
            }
            else
            {
                diffuseProgram = new ProgramDeprecated(vsDiffuse120, fsDiffuse120);
            }
            diffuseProgram.AttributeMappings = attributeMappings;

            diffuseProgram.Bind(cameraOne);
            diffuseProgram.Bind(parameters);

            //  Initialize resources used by the second pass (render to screen, using texture)
            windowViewport = new Viewport(base.Width, base.Height);

            cameraTwo.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    new Vector3(0.0f, 0.0f, -2.0f),
                    new Vector3(0.0f, 0.0f,  0.0f),
                    new Vector3(0.0f, 1.0f,  0.0f)
                )
            );

            cameraTwo.FovYRadians = RenderStack.Math.Conversions.DegreesToRadians(60.0f);
            cameraTwo.ProjectionType = ProjectionType.PerspectiveVertical;

            cameraTwo.Frame.UpdateHierarchical(updateSerial);
            sphereFrame.UpdateHierarchical(updateSerial);
            cameraTwo.UpdateFrame();
            cameraTwo.UpdateViewport(windowViewport);
            cameraTwo.UpdateModelFrame(sphereFrame);

            parameters["texture"] = framebuffer[FramebufferAttachment.ColorAttachment0];

            if(RenderStack.Graphics.Configuration.glslVersion >= 330)
            {
                texturedProgram = new ProgramDeprecated(vsTextured330, fsTextured330);
            }
            else
            {
                texturedProgram = new ProgramDeprecated(vsTextured120, fsTextured120);
            }
            texturedProgram.AttributeMappings = attributeMappings;

            texturedProgram.Bind(cameraTwo);
            texturedProgram.Bind(parameters);

            diffuseProgram.Use();
            diffuseProgram.ApplyUniforms();

            texturedProgram.Use();
            texturedProgram.ApplyUniforms();

            EnsureUploaded(sphereMesh);
            EnsureUploaded(quadMesh);

            //  Setup some GL state
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            Resize += new EventHandler<EventArgs>(Application_Resize);
            Unload += new EventHandler<EventArgs>(Application_Unload);

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            SwapBuffers();
            Visible = true;
        }

        private void EnsureUploaded(Mesh mesh)
        {
            //  The mesh has not yet been uploaded to GL, do it now
            BufferRange vertexBufferRange = mesh.VertexBufferRange;
            BufferRange indexBufferRange = mesh.IndexBufferRange(MeshMode.PolygonFill);
            if(vertexBufferRange.NeedsUpload)
            {
                vertexBufferRange.Buffer.UpdateAll();
            }
            if(indexBufferRange.NeedsUpload)
            {
                indexBufferRange.Buffer.UpdateAll();
            }
        }

        void Application_Unload(object sender, EventArgs e)
        {
            BufferPool.Instance.Dispose();
            if(diffuseProgram != null)
            {
                diffuseProgram.Dispose();
                diffuseProgram = null;
            }
            if(texturedProgram != null)
            {
                texturedProgram.Dispose();
                texturedProgram = null;
            }
        }

        void Application_Resize(object sender, EventArgs e)
        {
            windowViewport.Resize(base.Width, base.Height);
            cameraOne.UpdateViewport(framebuffer);
            cameraOne.UpdateModelFrame(quadFrame);
            cameraTwo.UpdateViewport(windowViewport);
            cameraTwo.UpdateModelFrame(sphereFrame);
            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if(Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        void UseViewport(Viewport vp)
        {
            GL.Viewport(
                (int)vp.X, 
                (int)vp.Y, 
                (int)vp.Width, 
                (int)vp.Height
            );
        }

        void RenderToTexture()
        {
            Mesh        mesh        = sphereMesh;
            ProgramDeprecated     program     = diffuseProgram;
            MeshMode    meshMode    = MeshMode.PolygonFill;

            framebuffer.Begin();
            GL.ClearColor(0.72f, 0.72f, 0.72f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UseViewport(framebuffer);
            program.Use();

            //  We do not change any uniforms for this program so no need to reapply them
            //bindings.Apply(program);

            var attributeBindings = mesh.AttributeBindings(program, meshMode);
            SetupAttributeBindings(attributeBindings);
            EnsureUploaded(mesh);

            BufferRange vertexBufferRange = mesh.VertexBufferRange;
            BufferRange indexBufferRange = mesh.IndexBufferRange(MeshMode.PolygonFill);

            GL.DrawElementsBaseVertex(
                indexBufferRange.BeginMode,
                (int)indexBufferRange.Count,
                indexBufferRange.Buffer.DrawElementsType,
                (IntPtr)(indexBufferRange.OffsetBytes),
                vertexBufferRange.BaseVertex
            );

            DisableAttributeBindings(attributeBindings);

            framebuffer.End();
        }

        void RenderToScreen()
        {
            Mesh        mesh        = quadMesh;
            ProgramDeprecated     program     = texturedProgram;
            MeshMode    meshMode    = MeshMode.PolygonFill;

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            UseViewport(windowViewport);
            program.Use();

            //  We animate one uniform every frame, so we also need to upload uniforms
            //  This covers changes to camera uniforms caused by resizes
            double  time = (double)(System.Environment.TickCount);
            float   t   = (float)(System.Math.Sin(time * 0.01) * 0.5f + 0.5f);
            (parameters["t"] as Floats).Set(t);

            program.ApplyUniforms();

            var attributeBindings = mesh.AttributeBindings(program, meshMode);
            SetupAttributeBindings(attributeBindings);
            EnsureUploaded(mesh);

            BufferRange vertexBufferRange = mesh.VertexBufferRange;
            BufferRange indexBufferRange = mesh.IndexBufferRange(MeshMode.PolygonFill);

            GL.DrawElementsBaseVertex(
                indexBufferRange.BeginMode,
                (int)indexBufferRange.Count,
                indexBufferRange.Buffer.DrawElementsType,
                (IntPtr)(indexBufferRange.OffsetBytes),
                vertexBufferRange.BaseVertex
            );

            DisableAttributeBindings(attributeBindings);
        }

        private void SetupAttributeBindings(AttributeBindings attributeBindings)
        {
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
                    attributeBindings.VertexBufferRange.Buffer.Use();
                    attributeBindings.IndexBufferRange.Buffer.Use();
                    attributeBindings.SetupAttributePointers();
                    attributeBindings.Dirty = false;
                }
            }
            else
            {
                attributeBindings.VertexBufferRange.Buffer.Use();
                attributeBindings.IndexBufferRange.Buffer.Use();
                attributeBindings.SetupAttributePointers();
            }
        }
        private void DisableAttributeBindings(AttributeBindings attributeBindings)
        {
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
                attributeBindings.DisableAttributes();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            RenderToTexture();
            RenderToScreen();

            SwapBuffers();
        }

        [STAThread]
        public static void Main()
        {
            OpenTK.DisplayDevice chosenDisplay = OpenTK.DisplayDevice.Default;

            using (var application = new Application(chosenDisplay))
            {
                application.Title = "example.RenderToTexture";

                application.Run(30);
            }
        }
    }
}
 