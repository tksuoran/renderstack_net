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
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace RenderStack.Graphics
{
    /// \brief Abstraction for OpenGL renderbuffer.
    /// 
    /// \todo [Serializable]
    /// \note Mostly stable.
    public class RenderBufferGL1 : IRenderBuffer
    {
        private readonly RenderStack.Math.Viewport  viewport;
        private RenderbufferStorage                 internalFormat;
        private int                                 sampleCount;

        public RenderStack.Math.Viewport    Viewport        { get { return viewport; } }
        public RenderbufferStorage          InternalFormat  { get { return internalFormat; } }
        public int                          SampleCount     { get { return sampleCount; } }

        private void SetSize()
        {
            // \todo
        }

        public void Resize(int width, int height)
        {
            viewport.Resize(width, height);
            SetSize();
        }

        public void Dispose()
        {
        }

        public RenderBufferGL1(int width, int height, RenderbufferStorage internalFormat, int sampleCount)
        {
            viewport = new RenderStack.Math.Viewport(width, height);
            this.internalFormat = internalFormat;
            this.sampleCount = sampleCount;

            SetSize();
        }
    }
}
