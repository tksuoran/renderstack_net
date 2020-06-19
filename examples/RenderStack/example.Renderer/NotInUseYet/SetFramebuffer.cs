#if false // gl3
//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

using RenderStack.Math;
using RenderStack.Graphics;
using RenderStack.Scene;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class SetFramebuffer : RenderState
    {
        public Framebuffer Framebuffer = null;

        private static          SetFramebuffer last = null;
        private static readonly SetFramebuffer stateCache = new SetFramebuffer(null);

        public SetFramebuffer(Framebuffer framebuffer)
        {
            Framebuffer = framebuffer;
        }

        public static void ResetState()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            stateCache.Framebuffer = null;
            last = null;
        }
        public override void Reset()
        {
            Framebuffer = null;
        }
        public override void Execute()
        {
            if(last == this)
            {
                return;
            }
            if(stateCache.Framebuffer != Framebuffer)
            {
                Framebuffer.Begin();
                stateCache.Framebuffer = Framebuffer;
            }
            last = this;
        }
    }
}

#endif