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

//#define DISABLE_CACHE

using System.Collections.Generic;

using RenderStack.Math;
using RenderStack.Graphics;
using RenderStack.Scene;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Renderer

{
    public class MaskState : RenderState
    {
        public bool    Red      = true;
        public bool    Green    = true;
        public bool    Blue     = true;
        public bool    Alpha    = true;
        public bool    Depth    = true;

        private static MaskState @default   = new MaskState();
        private static MaskState last       = null;
        private static MaskState stateCache = new MaskState();

        public static MaskState Default { get { return @default; } }

        public static void ResetState()
        {
            GL.ColorMask(true, true, true, true);
            stateCache.Red    = true;
            stateCache.Green  = true;
            stateCache.Blue   = true;
            stateCache.Alpha  = true;
            last = null;
        }
        public override void Reset()
        {
            Red      = true;
            Green    = true;
            Blue     = true;
            Alpha    = true;
            Depth    = true;
        }
        public override void Execute()
        {
#if !DISABLE_CACHE
            if(last == this)
            {
                return;
            }
            if(
                (stateCache.Red   != Red  ) ||
                (stateCache.Green != Green) ||
                (stateCache.Blue  != Blue ) ||
                (stateCache.Alpha != Alpha)
            )
#endif
            {
                GL.ColorMask(Red, Green, Blue, Alpha);
                stateCache.Red    = Red;
                stateCache.Green  = Green;
                stateCache.Blue   = Blue;
                stateCache.Alpha  = Alpha;
            }
#if !DISABLE_CACHE
            if(stateCache.Depth != Depth)
#endif
            {
                GL.DepthMask(Depth);
                stateCache.Depth = Depth;
            }
            last = this;
        }
    }
}
