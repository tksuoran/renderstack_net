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
using System.Diagnostics;
using System.Runtime.InteropServices;

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class SamplerRL : ISampler
    {
        private TextureMinFilter    minFilter   = TextureMinFilter.Nearest;
        private TextureMagFilter    magFilter   = TextureMagFilter.Nearest;
        private TextureWrapMode     wrap        = TextureWrapMode.ClampToEdge;

        public OpenTK.Graphics.OpenGL.TextureMinFilter      MinFilter   { get { return (OpenTK.Graphics.OpenGL.TextureMinFilter)minFilter; } set { var v = (TextureMinFilter)value; if(minFilter != v){ minFilter = v; } } }
        public OpenTK.Graphics.OpenGL.TextureMagFilter      MagFilter   { get { return (OpenTK.Graphics.OpenGL.TextureMagFilter)magFilter; } set { var v = (TextureMagFilter)value; if(magFilter != v){ magFilter = v; } } }
        public OpenTK.Graphics.OpenGL.TextureWrapMode       Wrap        { get { return (OpenTK.Graphics.OpenGL.TextureWrapMode)wrap; } set { var v = (TextureWrapMode)value; if(wrap != v){ wrap = v; } } }
        public OpenTK.Graphics.OpenGL.TextureCompareMode    CompareMode { get { return OpenTK.Graphics.OpenGL.TextureCompareMode.None; } set {} }
        public OpenTK.Graphics.OpenGL.DepthFunction         CompareFunc { get { return OpenTK.Graphics.OpenGL.DepthFunction.Always; } set {} }

        public void Apply(int textureUnit, OpenTK.Graphics.OpenGL.TextureTarget bindTarget)
        {
            RL.TexParameter((TextureTarget)bindTarget, TexParameter.TextureMagFilter, (int)(MagFilter));
            RL.TexParameter((TextureTarget)bindTarget, TexParameter.TextureMinFilter, (int)(MinFilter));
            RL.TexParameter((TextureTarget)bindTarget, TexParameter.TextureWrapS, (int)(Wrap));
            RL.TexParameter((TextureTarget)bindTarget, TexParameter.TextureWrapT, (int)(Wrap));
            RL.TexParameter((TextureTarget)bindTarget, TexParameter.TextureWrapR, (int)(Wrap));
        }

    }
}
