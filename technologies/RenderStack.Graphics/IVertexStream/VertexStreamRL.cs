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

//#define LOG_BINDINGS

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using VertexAttribPointerType   = OpenTK.Graphics.OpenGL.VertexAttribPointerType;
using VertexAttribIPointerType  = OpenTK.Graphics.OpenGL.VertexAttribIPointerType;

namespace RenderStack.Graphics
{
    public class VertexStreamRL : IVertexStream, IDisposable
    {
        private List<AttributeBinding>  bindings = new List<AttributeBinding>();
        private bool                    dirty    = true;

        public List<AttributeBinding>   Bindings { get { return Bindings; } }
        public bool                     Dirty    { get { return dirty; } set { dirty = value; } }

        public void Dispose()
        {
        }

        public void Use()
        {
            throw new NotImplementedException();
        }
        public void SetupAttributePointers()
        {
            foreach(AttributeBinding binding in bindings)
            {
                RL.VertexAttribBuffer(
                    binding.Slot,
                    binding.Size,
                    (Caustic.OpenRL.VertexAttribType)binding.Type,
                    binding.Normalized,
                    binding.Stride,
                    binding.Offset
                );
            }
        }
        public void DisableAttributes()
        {
        }
        public AttributeBinding Add(AttributeMapping mapping, Attribute attribute, int stride)
        {
            var binding = new AttributeBinding(mapping, attribute, stride);
            bindings.Add(binding);
            return binding;
        }
        public AttributeBinding Add(AttributeMapping mapping, Attribute attribute, int stride, int slot)
        {
            var binding = new AttributeBinding(mapping, attribute, stride, slot);
            bindings.Add(binding);
            return binding;
        }
        public void Clear()
        {
            bindings.Clear();
        }
    }

}
