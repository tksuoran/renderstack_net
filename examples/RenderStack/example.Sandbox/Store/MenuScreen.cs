//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.Graphics;
//using RenderStack.Parameters;
using RenderStack.UI;

using OpenTK.Graphics.OpenGL;

namespace Game
{
    public class MenuScreen : Sandbox.Service
    {
        public override string Name
        {
            get { return "MenuScreen"; }
        }

        Sandbox.Renderer        renderer;
        Sandbox.TextRenderer    textRenderer;
        FontStyle               fontStyle;

        public void Connect(
            Sandbox.Renderer        renderer,
            Sandbox.TextRenderer    textRenderer
        )
        {
            this.renderer           = renderer;
            this.textRenderer       = textRenderer;

            InitializationDependsOn(renderer);
            InitializationDependsOn(textRenderer);
        }

        protected override void InitializeService()
        {
            fontStyle  = new FontStyle("res/fonts/large.fnt"); 
        }

        public void Render()
        {
            GL.ClearColor(0.0f, 0.5f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            textRenderer.Begin();
            renderer.CurrentMaterial.Parameters["texture"] = fontStyle.Texture;

            Rectangle bounds;

            fontStyle.Print(textRenderer.TextBuffer.Mesh, 0.0f, 0.0f, 0.0f, "Snake", out bounds);

            textRenderer.Frame.LocalToParent.SetTranslation(
                (float)(int)((renderer.CurrentViewport.Width  / 2) - (bounds.Size.X / 2)), //(float)(int)((renderer.CurrentViewport.Width  / 2) - (bounds.Size.X / 2)),
                (float)(int)((renderer.CurrentViewport.Height / 2)), //0.0f, //(float)(int)((renderer.CurrentViewport.Height / 2) - (bounds.Size.Y / 2)),
                0.0f
            );
            textRenderer.Camera.UpdateModelFrame(textRenderer.Frame);
            renderer.ForceRebindUniforms = true;
            renderer.RenderCurrent();
            renderer.ForceRebindUniforms = false;

        }
    }
}
