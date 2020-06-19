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

// #define DEBUG_BUFFER_OBJECTS

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UInt16  = System.UInt16;
using UInt32  = System.UInt32;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public interface IBuffer : IDisposable
    {
        BufferTarget        BufferTargetGL      { get; }
        DrawElementsType    DrawElementsTypeGL  { get; }
        VertexFormat        VertexFormat        { get; }
        //UInt32              BufferObjectGL      { get; }
        //IntPtr              BufferObjectRL      { get; }

        //VertexStreamRL VertexStreamRL(IProgram program);
        VertexStreamGL VertexStreamGL(AttributeMappings mappings);
        //void                UpdateAll();
        void                UpdateGL();
        void                UpdateRL();
        void                UseGL();
        void                UseRL();
        IBufferRange        CreateIndexBufferRange(BeginMode beginMode);
        IBufferRange        CreateVertexBufferRange();
        IBufferRange        CreateUniformBufferRange(IUniformBlock uniformBlock);
        void                BindRange(int bindingPoint, IntPtr offsetBytes, IntPtr size);
    }

    public class BufferFactory
    {
        public static IBuffer Create(VertexFormat vertexFormat, BufferUsageHint usageHint)
        {
#if false
            if(Configuration.useOpenRL)
            {
                return new BufferGLRL(vertexFormat, usageHint);
            }
            else
#endif
            {
                return new BufferGL(vertexFormat, usageHint);
            }
        }
        public static IBuffer Create(DrawElementsType indexType, BufferUsageHint usageHint)
        {
#if false
            if(Configuration.useOpenRL)
            {
                return new BufferGLRL(indexType, usageHint);
            }
            else
#endif
            {
                return new BufferGL(indexType, usageHint);
            }
        }
    }
}
