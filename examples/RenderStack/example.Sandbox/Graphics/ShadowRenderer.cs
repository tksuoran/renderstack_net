//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;

namespace example.Sandbox
{
    public class ShadowRenderer : Service
    {
        public override string Name
        {
            get { return "ShadowRenderer"; }
        }

        MaterialManager materialManager;
        QuadRenderer    quadRenderer;
        IRenderer       renderer;
        SceneManager    sceneManager;

        private BlendState              ShadowBlendState;
        private FaceCullState           ShadowFaceCullState;
        private DepthState              ShadowDepthState;
        private IProgram                visualizeShadowMap;
        private IFramebuffer            shadow;
        private FramebufferAttachment   shadowAttachment;

        public  TextureGL               ShadowMap { get { return shadow[shadowAttachment]; } }

        protected override void InitializeService()
        {
            //  Listen to changes in light count
            sceneManager.LightCountSet += new SceneManager.LightCountSetDelegate(SetLightCount);

            ShadowBlendState    = BlendState.Default;
            ShadowFaceCullState = FaceCullState.Default;
            ShadowDepthState    = DepthState.Default;

            quadRenderer        = new QuadRenderer(renderer);
            visualizeShadowMap  = renderer.Programs["VisualizeShadowMap"];

            if(example.Renderer.Configuration.hardwareShadowPCF)
            {
                materialManager.MakeMaterial("Shadow","ShadowCaster2");
            }
            else
            {
                materialManager.MakeMaterial("Shadow","ShadowCaster");
            }

            {
                var m = materialManager.MakeMaterial("VisualizeShadowMap");
                m.DepthState = DepthState.Disabled;
                m.FaceCullState = FaceCullState.Disabled;
            }

            renderer.Resize += new EventHandler<EventArgs>(renderer_Resize);
        }

        void renderer_Resize(object sender, EventArgs e)
        {
            UpdateQuads();
        }

        private int currentLightCount = 0;
        public void SetLightCount(int n)
        {
            if(n == currentLightCount)
            {
                return;
            }

            currentLightCount = n;

            // \todo if n < currentLightCount ?

            if(RenderStack.Graphics.Configuration.useGl1 == true)
            {
                return;
            }

            if(shadow != null)
            {
                shadow.Dispose();
            }
            shadow = FramebufferFactory.Create(Configuration.shadowResolution, Configuration.shadowResolution);

            if(example.Renderer.Configuration.hardwareShadowPCF)
            {
                shadowAttachment = FramebufferAttachment.DepthAttachment;
                var texture = shadow.AttachTextureArray(
                    FramebufferAttachment.DepthAttachment, 
                    PixelFormat.DepthComponent,
                    PixelInternalFormat.DepthComponent,
                    n
                );
            }
            else
            {
                shadowAttachment = FramebufferAttachment.ColorAttachment0;
                if(
                    (RenderStack.Graphics.Configuration.canUseTextureArrays) &&
                    (RenderStack.Graphics.Configuration.glslVersion >= 330)
                )
                {
                    shadow.AttachTextureArray(
                        FramebufferAttachment.ColorAttachment0, 
                        PixelFormat.Red,
                        //PixelInternalFormat.R16f,
                        PixelInternalFormat.R32f,
                        n
                    );
                }
                else
                {
                    shadow.AttachTexture(
                        FramebufferAttachment.ColorAttachment0, 
                        PixelFormat.Red,
                        //PixelInternalFormat.R16f,
                        PixelInternalFormat.R32f
                    );
                }
                shadow.AttachRenderBuffer(
                    FramebufferAttachment.DepthAttachment, 
                    //PixelFormat.DepthComponent, 
                    RenderbufferStorage.DepthComponent32,
                    0
                );
            }
            shadow.Begin();
            shadow.Check();
            shadow.End();

            var noShadow = materialManager.Textures["NoShadow"] = new TextureGL(
                1, 1, PixelFormat.Red, PixelInternalFormat.R16f, n
            );
            System.Single[] whiteData = new System.Single[n];
            for(int i = 0; i < n; ++i)
            {
                whiteData[i] = 1.0f;
            }
            noShadow.Upload(whiteData, 0);
        }

        public void UpdateQuads()
        {
            int n = currentLightCount;
            float gap = 5.0f;
            float size = (renderer.Width / (n + 1)) - gap;
            float x = gap;
            quadRenderer.Begin();
            for(int i = 0; i < n; ++i)
            {
                quadRenderer.Quad(
                    new Vector3(       x, gap,        (float)(i)),
                    new Vector3(x + size, gap + size, (float)(i))
                );
                x += size + gap;
            }
            quadRenderer.End();
        }

        public void Connect(
            MaterialManager materialManager,
            IRenderer       renderer,
            SceneManager    sceneManager
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;

            InitializationDependsOn(renderer);
            InitializationDependsOn(materialManager);
        }

        public void UpdateShadowMaps()
        {
            RenderStack.Graphics.Debug.WriteLine("=== UpdateShadowMaps");

            renderer.SetTexture("t_shadowmap", ShadowMap);

            foreach(var light in sceneManager.RenderGroup.Lights)
            {
                light.Frame.UpdateHierarchical(sceneManager.UpdateSerial);
                light.UpdateViewport(shadow.Viewport);
                light.UpdateFrame();
            }

            foreach(var light in sceneManager.RenderGroup.Lights)
            {
                RenderShadow(light);
            }

            sceneManager.UpdateShadowMap = false;
        }

        private void ClearShadow(int lightIndex)
        {
            if(
                (RenderStack.Graphics.Configuration.canUseTextureArrays == false) &&
                (lightIndex > 0)
            )
            {
                return;
            }

            IFramebuffer framebuffer    = shadow;
            renderer.Requested.Viewport = framebuffer.Viewport;

            var shadowAttachment = example.Renderer.Configuration.hardwareShadowPCF 
                ? FramebufferAttachment.DepthAttachment 
                : FramebufferAttachment.ColorAttachment0;

            framebuffer.Begin();
            if(
                (RenderStack.Graphics.Configuration.canUseTextureArrays) &&
                (RenderStack.Graphics.Configuration.glslVersion >= 330)
            )
            {
                framebuffer.AttachTextureLayer(shadowAttachment, 0, lightIndex);
            }

            GL.Viewport(
                renderer.Requested.Viewport.X,
                renderer.Requested.Viewport.Y,
                renderer.Requested.Viewport.Width,
                renderer.Requested.Viewport.Height
            );

            renderer.PartialGLStateResetToDefaults();

            GL.Disable      (EnableCap.ScissorTest);
            GL.ClearColor   (1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear        (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        private void RenderShadow(Light light)
        {
            ClearShadow(light.LightIndex);

            renderer.Requested.Camera = light.Camera;
            renderer.UpdateCamera();

            foreach(var group in sceneManager.ShadowCasterGroups)
            {
                if(group.Visible == false)
                {
                    continue;
                }
                renderer.CurrentGroup = group;
                RenderGroupShadow(group);
            }
            shadow.End();
        }
        private void RenderGroupShadow(Group group)
        {
            renderer.Requested.Material = materialManager["Shadow"];
            renderer.Requested.Program  = materialManager["Shadow"].Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;

            int renderedModelCount = 0;
            if(
                RenderStack.Graphics.Configuration.canUseInstancing
                /* && RenderStack.Graphics.Configuration.canUseBaseVertex */
            )
            {
                //  Instanced code path
                //  \todo What about transparent instances?
                foreach(var kvp in group.OpaqueInstances.Collection)
                {
                    var tuple = kvp.Key;
                    var models2 = kvp.Value;
                    renderer.Requested.Mesh = tuple.Item1;
                    renderer.RenderInstancedPrepare();

                    int i = 0;
                    foreach(var model in models2)
                    {
                        renderer.Models.Floats("model_to_world_matrix").SetI(
                            i, model.Frame.LocalToWorld.Matrix
                        );
                        ++i;
                        ++renderedModelCount;
                        if(i > 49)
                        {
                            renderer.Models.Sync();
                            renderer.RenderCurrent(i);
                            i = 0;
                        }
                    }
                    if(i > 0)
                    {
                        renderer.Models.Sync();

                        if(i > 1)
                        {
                            renderer.RenderCurrent(i);
                        }
                        else
                        {
                            renderer.RenderCurrent();
                        }
                    }
                }
            }
            else
            {
                //  Non-instanced code path
                foreach(var model in group.Models)
                {
                    renderer.Requested.Mesh = model.Batch.Mesh;
                    renderer.SetFrame(model.Frame);
                    renderer.RenderCurrent();
                    ++renderedModelCount;
                }
            }
        }
        public void VisualizeShadowmaps()
        {
            if(quadRenderer == null)
            {
                return;
            }

            renderer.SetFrame(renderer.DefaultFrame);
            renderer.Requested.Mesh     = quadRenderer.Mesh;
            renderer.Requested.Material = materialManager["VisualizeShadowMap"];
            renderer.Requested.Program  = visualizeShadowMap;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetTexture("t_shadowmap_vis", ShadowMap);
            renderer.RenderCurrent();
        }

        public void AnimateLights()
        {
            float now = Time.Now;
            float floorSize = RuntimeConfiguration.curveTool ? 07.0f : 20.0f;;

            float scale = Configuration.slow ? 1.0f : 2.0f;
            double count = (double)(sceneManager.RenderGroup.Lights.Count);
            foreach(var light in sceneManager.RenderGroup.Lights)
            {
                int i = light.LightIndex;
                double theta = now + 2.0 * Math.PI * (double)(i) / count;
                Vector3 d = new Vector3(
                    2.0 + Math.Cos(theta),
                    1.0,
                    2.0 + Math.Sin(theta)
                );
                d = Vector3.Normalize(d);

                LightsUniforms.Direction.SetI(i, d);
                Vector3 direction = new Vector3(
                    LightsUniforms.Direction[i, 0],
                    LightsUniforms.Direction[i, 1],
                    LightsUniforms.Direction[i, 2]
                );

                light.Projection.OrthoLeft      = -floorSize * 0.6f * scale;
                light.Projection.OrthoTop       =  floorSize * 0.6f * scale;
                light.Projection.OrthoWidth     =  floorSize * 1.2f * scale;
                light.Projection.OrthoHeight    =  floorSize * 1.2f * scale;

                light.Frame.LocalToParent.Set(
                    Matrix4.CreateLookAt(
                        floorSize * 0.5f * direction * scale,
                        new Vector3(0.0f, 0.0f, 0.0f),
                        Vector3.UnitY
                    )
                );
                light.Frame.UpdateHierarchical(sceneManager.UpdateSerial);
                light.UpdateFrame();
                light.UpdateViewport(shadow.Viewport);
            }
        }

    }
}
