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

// #define DISABLE_CACHE

using System.Collections.Generic;

using RenderStack.Math;
using RenderStack.Graphics;
using RenderStack.Scene;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public class FaceCullState : RenderState
    {
        public bool                 Enabled             = true;
        public CullFaceMode         CullFaceMode        = CullFaceMode.Back;
        public FrontFaceDirection   FrontFaceDirection  = FrontFaceDirection.Ccw;

        private static          FaceCullState @default   = new FaceCullState();
        private static          FaceCullState disabled   = new FaceCullState(false);
        private static          FaceCullState last       = null;
        private static readonly FaceCullState stateCache = new FaceCullState();

        public static FaceCullState Default     { get { return @default; } }
        public static FaceCullState Disabled    { get { return disabled; } }

        public FaceCullState()
        {
        }
        public FaceCullState(bool enabled)
        {
            Enabled = enabled;
        }

        public static void ResetState()
        {
            GL.CullFace(CullFaceMode.Back);
            stateCache.CullFaceMode = CullFaceMode.Back;
            GL.FrontFace(FrontFaceDirection.Ccw);
            stateCache.FrontFaceDirection = FrontFaceDirection.Ccw;
            GL.Disable(EnableCap.CullFace);
            stateCache.Enabled = false;
            last = null;
        }
        public override void Reset()
        {
            Enabled             = true;
            CullFaceMode        = CullFaceMode.Back;
            FrontFaceDirection  = FrontFaceDirection.Ccw;
        }
        public override void Execute()
        {
#if !DISABLE_CACHE
            if(last == this)
            {
                return;
            }
#endif
            if(Enabled)
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == false)
#endif
                {
                    GL.Enable(EnableCap.CullFace);
                    stateCache.Enabled = true;
                }
#if !DISABLE_CACHE
                if(stateCache.CullFaceMode != CullFaceMode)
#endif
                {
                    GL.CullFace(CullFaceMode);
                    stateCache.CullFaceMode = CullFaceMode;
                }
#if !DISABLE_CACHE
                if(stateCache.FrontFaceDirection != FrontFaceDirection)
#endif
                {
                    GL.FrontFace(FrontFaceDirection);
                    stateCache.FrontFaceDirection = FrontFaceDirection;
                }
            }
            else
            {
#if !DISABLE_CACHE
                if(stateCache.Enabled == true)
#endif
                {
                    GL.Disable(EnableCap.CullFace);
                    stateCache.Enabled = false;
                }
            }
            last = this;
        }
    }
}
