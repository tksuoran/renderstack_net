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
using System.Runtime.InteropServices;

using RenderStack.Math;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// Interface for uniform values that can be fed to OpenGL uniforms
    /// 
    /// \note Somewhat experimental.
    public interface IUniformValue : IComparable
    {
        //! Creates a clone of this IUniformValue, returning the same type but does not copy values
        IUniformValue GetUninitialized();

        //! Copies values from another IUniformValue. It should have the same type
        bool    IsCompatibleWith(object o);
        void    CopyFrom(IUniformValue cache);

        bool    Dirty   { get; set; }
        int     Index   { get; set; }
    }
}
