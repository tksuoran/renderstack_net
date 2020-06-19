//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if false
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Mesh;
//using RenderStack.Parameters;
using RenderStack.Scene;

using Buffer = RenderStack.Graphics.Buffer;

namespace Sandbox
{
    public partial class Application
    {
    }

    public class ReflectionRenderer : Service
    {
        SceneManager sceneManager;
        CubeRenderer cubeRenderer;

        public void Connect(
            SceneManager sceneManager,
            CubeRenderer cubeRenderer
        )
        {
            this.sceneManager = sceneManager;
            this.cubeRenderer = cubeRenderer;
        }

        protected override void InitializeService()
        {
        }

        private void RenderReflectionObjects()
        {
            if(sceneManager.ReflectionModel == null)
            {
                return;
            }

            GL.Disable      (EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0.0f, 0.0f);
            GL.Enable       (EnableCap.CullFace);
            GL.Enable       (EnableCap.DepthTest);
            GL.DepthFunc    (DepthFunction.Less);
            GL.DepthMask    (true);

            cubeRenderer.Render(
                sceneManager.ReflectionModel.Frame.LocalToWorld.Matrix, 
                sceneManager.RenderGroup
            );
        }
    }
}

#endif