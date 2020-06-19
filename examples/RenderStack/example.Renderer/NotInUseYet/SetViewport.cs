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
    public class SetViewport : RenderState
    {
        public Viewport Viewport = null;

        private static          SetViewport last = null;
        private static readonly SetViewport stateCache = new SetViewport(null);

        public SetViewport(Viewport viewport)
        {
            Viewport = viewport;
        }

        public static void ResetState()
        {
            last = null;
            stateCache.Viewport = null;
            //  No meaningfull GL.Viewport() call to make;
            //  Will be set to something anyway when something is drawn.
        }
        public override void Reset()
        {
            Viewport = null;
        }
        public override void Execute()
        {
            if(last == this)
            {
                return;
            }
            if(stateCache.Viewport != Viewport)
            {
                GL.Viewport(
                    Viewport.X, 
                    Viewport.Y, 
                    Viewport.Width,
                    Viewport.Height
                );
                stateCache.Viewport = Viewport;
            }
            last = this;
        }
    }
}
