#if false // gl3
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
using System.IO;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Renderer
{
    /// \brief Service to render scene to a cube map
    /// \note Not tested for a while
    public partial class CubeRenderer : Service
    {
        #region Service
        public override string Name
        {
            get { return "CubeRenderer"; }
        }

        Renderer renderer;

        public void Connect(Renderer renderer)
        {
            this.renderer = renderer;

            InitializationDependsOn(renderer);
        }

        protected override void InitializeService()
        {
            framebuffer = new Framebuffer(size, size);
            framebuffer.AttachCubeTexture(
                FramebufferAttachment.ColorAttachment0,
                PixelFormat.Rgb,
                PixelInternalFormat.Rgba8
                //PixelInternalFormat.Rgb32f
            );
            framebuffer.AttachRenderBuffer(
                FramebufferAttachment.DepthAttachment,
                PixelFormat.DepthComponent,
                RenderbufferStorage.DepthComponent32,
                0
            );
            framebuffer.Begin();
            framebuffer.Check();
            framebuffer.End();

            for(int i = 0; i < 6; ++i)
            {
                // posx, negx, posy, negy, posz, negz
                camera[i] = new Camera();
                camera[i].Projection.ProjectionType = ProjectionType.Perspective;
                camera[i].Projection.FovXRadians = (float)System.Math.PI * 0.5f;
                camera[i].Projection.FovYRadians = (float)System.Math.PI * 0.5f;
                camera[i].Name = "CubeMap camera " + i.ToString();
            }

            //  I figured out these experimentally. I do not fully understand why
            //  X and Z is inverted from what would be expected while Y is not.
            //  Possibly this has to do something with the fact that camera is
            //  looking at the negative Z. Figure out.
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3(-1.0f,  0.0f,  0.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[0]); // posx
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 1.0f,  0.0f,  0.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[1]); // negx
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,  1.0f,  0.0f), new Vector3(0.0f,  0.0f, -1.0f), out faceOrientation[2]); // posy
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, -1.0f,  0.0f), new Vector3(0.0f,  0.0f,  1.0f), out faceOrientation[3]); // negy
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f, -1.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[4]); // posz
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f,  1.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[5]); // negz
        }

        #endregion

        private int         size;
        private Framebuffer framebuffer;
        private Camera[]    camera = new Camera[6];
        private Matrix4[]   faceOrientation = new Matrix4[6];

        //  http://www.opengl.org/wiki/GL_EXT_framebuffer_object#Quick_example.2C_render_to_texture_.28Cubemap.29
        public Texture Texture
        {
            get
            {
                return framebuffer[FramebufferAttachment.ColorAttachment0];
            }
        }

        public CubeRenderer(int size)
        {
            this.size = size;
        }

        public void Render(Matrix4 cameraTransform, Group renderGroup)
        {
            Matrix4 cameraRotation = cameraTransform;
            cameraRotation._03 = 0.0f;
            cameraRotation._13 = 0.0f;
            cameraRotation._23 = 0.0f;
            cameraRotation._33 = 1.0f;
            cameraRotation = cameraRotation * Matrix4.CreateRotation(
                (float)System.Math.PI, 
                Vector3.UnitY
            );

            renderer.CurrentGroup = renderGroup;
            renderer.Requested.Viewport = framebuffer;

            Vector3 positionInWorld = cameraTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));

            for(int i = 0; i < 6; ++i)
            {
                framebuffer.AttachCubeFace(
                    FramebufferAttachment.ColorAttachment0, 
                    TextureTarget.TextureCubeMapPositiveX + i, 
                    0
                );
                framebuffer.Begin();
                framebuffer.Check();

                Matrix4 localToWorld = cameraRotation * faceOrientation[i];
                localToWorld._03 = positionInWorld.X;
                localToWorld._13 = positionInWorld.Y;
                localToWorld._23 = positionInWorld.Z;
                camera[i].Frame.Parent = null;
                camera[i].Frame.LocalToParent.Set(localToWorld);
                camera[i].Frame.LocalToWorld.Set(localToWorld);
                renderer.Requested.Camera = camera[i];
                //camera[i].Frame.UpdateHierarchical(1);

                GL.Viewport(
                    (int)renderer.Requested.Viewport.X, 
                    (int)renderer.Requested.Viewport.Y, 
                    (int)renderer.Requested.Viewport.Width, 
                    (int)renderer.Requested.Viewport.Height
                );
                GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderer.CurrentGroup = renderGroup;
                renderer.Requested.MeshMode = MeshMode.PolygonFill;
                renderer.RenderGroup();//null, null, MeshMode.NotSet); 

                framebuffer.End();
            }
            //framebuffer.UnbindTexture(FramebufferAttachment.ColorAttachment0);

            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, Texture.TextureObject);
                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
                GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            }
        }
    }
}

//  This is from the OpenGL spec about how to lookup cube textures:
//  
//  s = 0.5 + 0.5 * sc/|ma|
//  t = 0.5 + 0.5 * tc/|ma|
//         sc  tc ma
//  posx  -rz -ry rx
//  negx   rz -ry rx
//  posy   rx  rz ry
//  negy   rx -rz ry
//  posz   rx -ry rz
//  negz  -rx -ry rz
#if false
    switch(i)
    {
        case 0: GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f); break;
        case 1: GL.ClearColor(0.5f, 0.0f, 0.0f, 1.0f); break;
        case 2: GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f); break;
        case 3: GL.ClearColor(0.0f, 0.5f, 0.0f, 1.0f); break;
        case 4: GL.ClearColor(0.0f, 0.0f, 1.0f, 1.0f); break;
        case 5: GL.ClearColor(0.0f, 0.0f, 0.5f, 1.0f); break;
    }
#endif

#endif