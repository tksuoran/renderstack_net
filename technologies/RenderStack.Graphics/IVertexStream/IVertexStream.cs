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
using GL = OpenTK.Graphics.OpenGL.GL;
using OpenGL = OpenTK.Graphics.OpenGL;
using Int16   = System.Int16;
using Int32   = System.Int32;
using UInt16  = System.UInt16;
using UInt32  = System.UInt32;
using Single  = System.Single;
using Double  = System.Double;
using VertexAttribPointerType   = OpenTK.Graphics.OpenGL.VertexAttribPointerType;
using VertexAttribIPointerType  = OpenTK.Graphics.OpenGL.VertexAttribIPointerType;

namespace RenderStack.Graphics
{

    public interface IVertexStream : IDisposable
    {
        //Int32                    VertexArrayObject  { get; }
        List<AttributeBinding>   Bindings           { get; }
        bool                     Dirty              { get; set; }

        void                Use();
        void                SetupAttributePointers();
        void                DisableAttributes();
        AttributeBinding    Add(AttributeMapping mapping, Attribute attribute, int stride);
        void                Clear();
    }

}
