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
// #define DEBUG_UNIFORM_BUFFER

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
#if false
    public interface IVertexBufferRange
    {
        //void Set(Attribute attribute, params byte[] values);
        //void Set(Attribute attribute, params int[] values);
        //void Set(Attribute attribute, params uint[] values);
        //void Set(Attribute attribute, params float[] values);
        void Set(Attribute attribute, Vector2 value);
        void Set(Attribute attribute, Vector3 value);
        void Set(Attribute attribute, Vector4 value);
        void Position(Vector3 value);
        void Position(Vector3 value, int index);
        void Normal(Vector3 value);
        void Normal(Vector3 value, int index);
        void TexCoord(Vector2 value);
        void TexCoord(Vector2 value, int index);
    }
    public interface IIndexBufferRange
    {
        void Point(UInt32 index0);
        void Line(UInt32 index0, UInt32 index1);
        void Triangle(UInt32 index0, UInt32 index1, UInt32 index2);
        void Quad(UInt32 index0, UInt32 index1, UInt32 index2, UInt32 index3);
    }
#endif
    public interface IBufferRange
    {
        string              Name                { get; set; }
        BufferGL            BufferGL            { get; }
        //BufferRL            BufferRL            { get; }
        DrawElementsType    DrawElementsTypeGL  { get; }
        BufferTarget        BufferTargetGL      { get; }
        VertexFormat        VertexFormat        { get; }
        UInt32              Count               { get; }
        BeginMode           BeginMode           { get; }
        long                Size                { get; }
        IUniformBlock       UniformBlock        { get; }
        long                OffsetBytes         { get; }
        int                 BaseVertex          { get; }
        bool                NeedsUploadGL       { get; }
        bool                NeedsUploadRL       { get; }

        //VertexStreamRL      VertexStreamRL(IProgram program);
        VertexStreamGL      VertexStreamGL(AttributeMappings mappings);

        bool Match              (IBufferRange other);
        void Allocate           (int size);
        void UseUniformBufferGL ();
        //void UseUniformBufferRL ();
        void WriteTo            (long offset, bool force);
        void Touch              (uint count, byte[] data);
        void UpdateGL           ();
        //void UpdateRL           ();
        void UseGL              ();
        void UseRL              ();
        void Floats             (int offset, Floats param);
        void Ints               (int offset, Ints param);
        void UInts              (int offset, UInts param);
    }
}
