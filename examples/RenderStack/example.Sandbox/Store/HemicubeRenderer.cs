//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

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

namespace example.Sandbox
{
    public enum HemicubeFramebuffer
    {
        PosXNegX = 0,
        PosYNegY = 1,
        NegZ     = 2
    }
    public enum HemicubeOrientation
    {
        PosX = 0,
        NegX = 1,
        PosY = 2,
        NegY = 3,
        NegZ = 4
    }
    public partial class HemicubeRenderer : Service
    {
        public override string Name
        {
            get { return "HemicubeRenderer"; }
        }

        IRenderer                   renderer;

        private int                 size;
        private PixelFormat         pixelFormat;
        private PixelInternalFormat pixelInternalFormat;
        private IFramebuffer[]      framebuffer = new IFramebuffer[3];
        private Camera[]            camera = new Camera[5];
        private Viewport[]          viewport = new Viewport[5];
        private Matrix4[]           faceOrientation = new Matrix4[5];
        public  Material            MaterialOverride;
        public float                Near = 0.00001f;

        public  TextureGL Texture(int i)
        {
            return framebuffer[i][FramebufferAttachment.ColorAttachment0];
        }

        private int halfSize;
        private int fullSize;
        private int smallestMipmapLevel;

        public int      Size { get { return fullSize; } }
        public float    Average;

        /*  Full frustum is given by -0.5 .. 0.5 for left..right and bottom..top  */ 
        public void SetupCameraFrustum(Camera camera, float left, float right, float bottom, float top)
        {
            float tanXHalfAngle     = (float)System.Math.Tan(camera.Projection.FovXRadians * 0.5);
            float tanYHalfAngle     = (float)System.Math.Tan(camera.Projection.FovYRadians * 0.5);
            float width             =  2.0f * camera.Projection.Near * tanXHalfAngle;
            float height            =  2.0f * camera.Projection.Near * tanYHalfAngle;
            camera.Projection.FrustumLeft      =  left   * width;
            camera.Projection.FrustumRight     =  right  * width;
            camera.Projection.FrustumTop       =  top    * height;
            camera.Projection.FrustumBottom    =  bottom * height;
        }

        public HemicubeRenderer(
            int                 size, 
            PixelFormat         pixelFormat, 
            PixelInternalFormat pixelInternalFormat
        )
        {
            this.size = size;
            this.pixelFormat = pixelFormat;
            this.pixelInternalFormat = pixelInternalFormat;
        }
        public void Connect(IRenderer renderer)
        {
            this.renderer = renderer;

            InitializationDependsOn(renderer);
        }
        protected override void InitializeService()
        {
            fullSize = size;
            halfSize = size / 2;
            int i;

            smallestMipmapLevel = 0;
            int mipmapSize = size;
            while (mipmapSize > 1)
            {
                mipmapSize /= 2;
                ++smallestMipmapLevel;
            }

            for (i = 0; i < 3; ++i)
            {
                framebuffer[i] = FramebufferFactory.Create(size, size);

                framebuffer[i].AttachTexture(
                    FramebufferAttachment.ColorAttachment0,
                    pixelFormat,
                    pixelInternalFormat
                );
                framebuffer[i].AttachRenderBuffer(
                    FramebufferAttachment.DepthAttachment,
                    //PixelFormat.DepthComponent,
                    RenderbufferStorage.DepthComponent32,
                    0
                );
                framebuffer[i].Begin();
                framebuffer[i].Check();
                framebuffer[i].End();
            }

            for (i = 0; i < 5; ++i)
            {
                // posx, negx, posy, negy, negz
                camera[i] = new Camera();
                camera[i].Projection.ProjectionType = ProjectionType.GenericFrustum;
                camera[i].Projection.FovXRadians = (float)Math.PI * 0.5f;
                camera[i].Projection.FovYRadians = (float)Math.PI * 0.5f;
                camera[i].Projection.Near = Near;
                camera[i].Name = "Hemeicube camera " + i.ToString();
            }

            /*  Scale frustums                                        left   right bottom top  */
            SetupCameraFrustum(camera[(int)HemicubeOrientation.PosX], 0.0f, 0.5f, -0.5f, 0.5f);
            SetupCameraFrustum(camera[(int)HemicubeOrientation.NegX], -0.5f, 0.0f, -0.5f, 0.5f);
            SetupCameraFrustum(camera[(int)HemicubeOrientation.NegY], -0.5f, 0.5f, 0.0f, 0.5f);
            SetupCameraFrustum(camera[(int)HemicubeOrientation.PosY], -0.5f, 0.5f, -0.5f, 0.0f);
            SetupCameraFrustum(camera[(int)HemicubeOrientation.NegZ], -0.5f, 0.5f, -0.5f, 0.5f);

            //  I figured out these experimentally. I do not fully understand why
            //  X and Z is inverted from what would be expected while Y is not.
            //  Possibly this has to do something with the fact that camera is
            //  looking at the negative Z. Figure out.
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3(-1.0f,  0.0f, 0.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[0]); // posx
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 1.0f,  0.0f, 0.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[1]); // negx
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,  1.0f, 0.0f), new Vector3(0.0f,  0.0f, -1.0f), out faceOrientation[2]); // posy
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, -1.0f, 0.0f), new Vector3(0.0f,  0.0f,  1.0f), out faceOrientation[3]); // negy
            Matrix4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f, 1.0f), new Vector3(0.0f, -1.0f,  0.0f), out faceOrientation[4]); // negz


            viewport[0] = new Viewport(halfSize,    0,          halfSize,   size);      // posx
            viewport[1] = new Viewport(0,           0,          halfSize,   size);      // negx
            viewport[2] = new Viewport(0,           halfSize,   size,       halfSize);  // posy
            viewport[3] = new Viewport(0,           0,          size,       halfSize);  // negy
            viewport[4] = new Viewport(0,           0,          size,       size);      // negz
        }

        private void Render(
            HemicubeFramebuffer fbo, 
            HemicubeOrientation orientation,
            Matrix4             cameraRotation, 
            Vector3             positionInWorld
        )
        {
            //  Select and activate framebuffer
            framebuffer[(int)fbo].Begin();
            framebuffer[(int)fbo].Check();

            //  Set camera orientation
            int i = (int)orientation;
            Matrix4 localToWorld = cameraRotation * faceOrientation[i];
            localToWorld._03 = positionInWorld.X;
            localToWorld._13 = positionInWorld.Y;
            localToWorld._23 = positionInWorld.Z;
            camera[i].Frame.Parent = null;
            camera[i].Frame.LocalToParent.Set(localToWorld);
            camera[i].Frame.LocalToWorld.Set(localToWorld);
            renderer.Requested.Camera = camera[i];
            //camera[i].Frame.UpdateHierarchical();

            //  Set viewport
            renderer.Requested.Viewport = viewport[i];
#if false
            switch(i)
            {
                case (int)HemicubeOrientation.PosX: GL.ClearColor(1.0f, 0.5f, 0.5f, 1.0f); break; // red
                case (int)HemicubeOrientation.NegX: GL.ClearColor(1.0f, 1.0f, 0.5f, 1.0f); break; // yellow
                case (int)HemicubeOrientation.PosY: GL.ClearColor(0.5f, 1.0f, 0.5f, 1.0f); break; // green
                case (int)HemicubeOrientation.NegY: GL.ClearColor(0.5f, 1.0f, 1.0f, 1.0f); break; // cyan
                case (int)HemicubeOrientation.NegZ: GL.ClearColor(0.5f, 0.5f, 1.0f, 1.0f); break; // blue
            }
#endif
            GL.Scissor(
                (int)renderer.Requested.Viewport.X, 
                (int)renderer.Requested.Viewport.Y, 
                (int)renderer.Requested.Viewport.Width, 
                (int)renderer.Requested.Viewport.Height
            );
            GL.Enable(EnableCap.ScissorTest);
            GL.Viewport(
                (int)renderer.Requested.Viewport.X, 
                (int)renderer.Requested.Viewport.Y, 
                (int)renderer.Requested.Viewport.Width, 
                (int)renderer.Requested.Viewport.Height
            );

            //  Clear and render
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.ScissorTest);

            renderer.Requested.Material = MaterialOverride;
            //renderer.Requested.Program  = ProgramOverride;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.RenderGroup(); 

            framebuffer[(int)fbo].End();
        }
        public void Render(Matrix4 cameraTransform, Group renderGroup)
        {
            Matrix4 cameraRotation  = cameraTransform;
            cameraRotation._03      = 0.0f;
            cameraRotation._13      = 0.0f;
            cameraRotation._23      = 0.0f;
            cameraRotation._33      = 1.0f;
            cameraRotation          = cameraRotation * Matrix4.CreateRotation((float)Math.PI, new Vector3(0.0f, 1.0f, 0.0f));
            renderer.CurrentGroup = renderGroup;

            // \todo Use a renderstate to control this
            GL.Disable(EnableCap.PolygonOffsetFill);

            Vector3 positionInWorld = cameraTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            float sum = 0.0f;
            Render(HemicubeFramebuffer.PosXNegX, HemicubeOrientation.PosX, cameraRotation, positionInWorld);
            Render(HemicubeFramebuffer.PosXNegX, HemicubeOrientation.NegX, cameraRotation, positionInWorld);
            sum += ReadRedAverage(HemicubeFramebuffer.PosXNegX);
            Render(HemicubeFramebuffer.PosYNegY, HemicubeOrientation.PosY, cameraRotation, positionInWorld);
            Render(HemicubeFramebuffer.PosYNegY, HemicubeOrientation.NegY, cameraRotation, positionInWorld);
            sum += ReadRedAverage(HemicubeFramebuffer.PosYNegY);
            Render(HemicubeFramebuffer.NegZ,     HemicubeOrientation.NegZ, cameraRotation, positionInWorld);
            sum += ReadRedAverage(HemicubeFramebuffer.NegZ);

            Average = sum * (1.0f / 3.0f);
        }
        float ReadRedAverage(HemicubeFramebuffer fbo)
        {
            TextureGL texture = framebuffer[(int)fbo][FramebufferAttachment.ColorAttachment0];

            texture.GenerateMipmap();

            // TODO AccessViolatonException
            bool found = false;
            while(
                (found == false) && 
                (smallestMipmapLevel >= 0)
            )
            {
                try
                {
                    framebuffer[(int)fbo].AttachTextureLevel(FramebufferAttachment.ColorAttachment0, smallestMipmapLevel);
                    found = true;
                }
                catch(Exception)
                {
                    System.Diagnostics.Trace.TraceWarning("TODO");
                    --smallestMipmapLevel;
                }
                if(smallestMipmapLevel == -1)
                {
                    System.Diagnostics.Trace.TraceWarning("TODO");
                    return 0;
                }
            }
            float[] pixels = new float[4];
            float[] depths = new float[4];
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.ReadPixels<float>(0, 0, 1, 1, PixelFormat.Red, PixelType.Float, pixels);
            framebuffer[(int)fbo].AttachTextureLevel(FramebufferAttachment.ColorAttachment0, 0);

            return pixels[0];
        }
    }

}

