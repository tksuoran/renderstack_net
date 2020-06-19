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
    [Serializable]
    public class BufferRangeGL : IBufferRange
    {
        private string              name = "";
        private byte[]              data;
        private bool                needsUpload = false;
        [NonSerialized]
        private IUniformBlock       uniformBlock;
        private BufferGL            buffer;
        private UInt32              count = 0;
        private BeginMode           beginMode;          /*  mode  */ 
        private long                size = 0;
        private long                offsetBytes = 0;    /*  Offset in Buffer  */
        private int                 baseVertex = -1;    /*  for use with DrawElementsBaseVertex()  */

        public  string              Name                { get { return name; } set { name = value; } }
        public  DrawElementsType    DrawElementsTypeGL  { get { return buffer.DrawElementsTypeGL; } }
        public  BufferTarget        BufferTargetGL      { get { return buffer.BufferTargetGL; } }
        public  VertexFormat        VertexFormat        { get { return buffer.VertexFormat; } }
        public  BufferGL            BufferGL            { get { return buffer; } }
        public  UInt32              Count               { get { return count; } }
        public  BeginMode           BeginMode           { get { return beginMode; } }
        public  long                Size                { get { return size; } }
        public  IUniformBlock       UniformBlock        { get { return uniformBlock; } }
        public  long                OffsetBytes         { get { return offsetBytes; } }
        public  int                 BaseVertex          { get { return baseVertex; } }
        public  bool                NeedsUploadGL       { get { return needsUpload; } }
        public  bool                NeedsUploadRL       { get { return false; } }

#if false
        public VertexStreamRL VertexStreamRL(IProgram program)
        {
            throw new InvalidOperationException();
        }
#endif
        public VertexStreamGL VertexStreamGL(AttributeMappings mappings)
        {
            return buffer.VertexStreamGL(mappings);
        }

        public void UpdateAll()
        {
            buffer.UpdateAll();
        }
        public void UpdateGL()
        {
            buffer.UpdateGL();
        }
#if false
        public void UpdateRL()
        {
            buffer.UpdateRL();
        }
#endif
        public bool Match(IBufferRange other)
        {
            //  This ensures that vertex format / index type match
            if(BufferTargetGL != other.BufferTargetGL)
            {
                return false;
            }
            switch(BufferTargetGL)
            {
                //  Vertex Buffer
                case BufferTarget.ArrayBuffer:
                {
                    if(VertexFormat != other.VertexFormat)
                    {
                        return false;
                    }
                    break;
                }
                //  Index Buffer
                case BufferTarget.ElementArrayBuffer:
                {
                    if(DrawElementsTypeGL != other.DrawElementsTypeGL)
                    {
                        return false;
                    }
                    break;
                }
                //  UniformBuffer
                case BufferTarget.UniformBuffer:
                {
                    if(UniformBlock != null)
                    {
                        if(UniformBlock != other.UniformBlock)
                        {
                            return false;
                        }
                    }
                    break;
                }
            }
            return true;
        }
        public void UseGL()
        {
            buffer.UseGL();
        }
        public void UseRL()
        {
            throw new InvalidOperationException();
        }
        //public BufferGL BufferGL { get { return buffer; } }
#if false
        public BufferRL BufferRL { get { throw new InvalidOperationException(); } }
#endif

        //  Index buffer, with valid begin mode
        internal BufferRangeGL(BufferGL buffer, BeginMode beginMode)
        {
            this.buffer = buffer;
            this.beginMode = beginMode;
        }

        //  Vertex Buffer
        public BufferRangeGL(BufferGL buffer)
        {
            this.buffer = buffer;
            unchecked
            {
                // Vertex buffers do not have a valid begin mode value 
                this.beginMode = (BeginMode)(uint.MaxValue); 
            }
        }

        public void Allocate(int size)
        {
            this.size = size;
            data = new byte[size];
        }

        //  Uniform Buffer
        public BufferRangeGL(BufferGL buffer, IUniformBlock uniformBlock)
        {
#if DEBUG
            if(Configuration.useGl1)
            {
                throw new InvalidOperationException();
            }
#endif
            this.uniformBlock = uniformBlock as UniformBlockGL;
            if(this.uniformBlock == null)
            {
                throw new InvalidOperationException();
            }
            this.buffer = buffer;
            unchecked
            {
                this.beginMode = (BeginMode)(uint.MaxValue); // uniform buffers do not have a valid begin mode value 
            }

            //  Pre-initialize uniform buffer as we do not use BeginEdit() / EndEdit()
            size = uniformBlock.Size;
            data = new byte[size];
        }
        public void UseUniformBufferGL()
        {
#if DEBUG
            if(Configuration.useGl1)
            {
                throw new InvalidOperationException();
            }
#endif
#if DEBUG_UNIFORM_BUFFER
            System.Diagnostics.Debug.WriteLine(
                "GL.BindBufferRange(BufferTarget.UniformBuffer, " + 
                uniformBlock.BindingPoint + ", " +
                (int)Buffer.BufferObject + ", " +
                (IntPtr)offsetBytes + ", " +
                (IntPtr)size +
                "); " + Name + " block " + uniformBlock.Name
            );
#endif
            BufferGL.BindRange(
                uniformBlock.BindingPointGL,
                (IntPtr)offsetBytes, 
                (IntPtr)size
            );
        }
#if false
        public void UseUniformBufferRL()
        {
            throw new InvalidOperationException();
        }
#endif

        public unsafe void WriteTo(long offset, bool force)
        {
            if((needsUpload == false) && (force == false))
            {
                return;
            }
            this.offsetBytes = offset;
            if(buffer.BufferTargetGL == BufferTarget.ArrayBuffer)
            {
                long baseVertex = offset / (long)buffer.VertexFormat.Stride;
                if(baseVertex * buffer.VertexFormat.Stride == offset)
                {
                    this.baseVertex = (int)baseVertex;
                }
                else
                {
                    throw new Exception("Base Vertex not aligned");
                }
            }
            if(buffer.BufferTargetGL == BufferTarget.ArrayBuffer)
            {
                if(baseVertex == -1)
                {
                    throw new Exception();
                }
            }
            if(data == null)
            {
                //throw new Exception("data == null");
                return;
            }
            if(data.Length == 0)
            {
                //throw new Exception("data.Length == 0");
                return;
            }
            if(data.Length != size)
            {
                throw new Exception("data.Length != size");
            }
            fixed(byte* data_ptr = &data[0])
            {
#if DEBUG_BUFFER_OBJECTS
                if(GL.IsBuffer(BufferGL.BufferObjectGL) == false)
                {
                    throw new Exception();
                }
                int abb;
                int vab;
                //int vabb;
                int glSize;
                GL.GetInteger(GetPName.ArrayBufferBinding, out abb);
                GL.GetInteger(GetPName.VertexArrayBinding, out vab);
                //GL.GetInteger(GetPName.VertexArrayBufferBinding, out vabb);
                GL.GetBufferParameter(BufferGL.BufferTargetGL, BufferParameterName.BufferSize, out glSize);
#endif
                GL.BufferSubData(BufferGL.BufferTargetGL, (IntPtr)offset, (IntPtr)data.Length, (IntPtr)(data_ptr));
#if DEBUG_BUFFER_OBJECTS
                System.Diagnostics.Debug.WriteLine(
                    "GL.BufferSubData(" + 
                    BufferGL.BufferTargetGL + ", " + 
                    (IntPtr)offset + ", " + 
                    (IntPtr)data.Length + ", data_ptr"+ 
                    //(IntPtr)(data_ptr) +
                    "); " + Name
                );
#endif

#if DEBUG_UNIFORM_BUFFER
                if(this.UniformBlock != null)
                {
                    int byteOffset = 0;
                    int bytesOnLine = 0;
                    //System.Diagnostics.Debug.WriteLine("    : 00010203 04050607 08090a0b 0c0d0e0f 10111213 14151617 18191a1b 1c1d1e1f");
                    //System.Diagnostics.Debug.WriteLine("----+ -------- -------- -------- -------- -------- -------- -------- --------");
                    System.Diagnostics.Debug.WriteLine("    :  0 1 2 3  4 5 6 7  8 9 a b  c d e f");
                    System.Diagnostics.Debug.WriteLine("----+------------------------------------");
                    System.Text.StringBuilder sb = null;
                    while((byteOffset < data.Length) && (byteOffset < 256))
                    {
                        if(sb == null)
                        {
                            sb = new System.Text.StringBuilder();
                            sb.Append(String.Format("{0:x4}", offset + byteOffset)).Append(": ");
                        }
                        byte b = data[byteOffset];
                        string hex = String.Format("{0:x2}", b);
                        sb.Append(hex);
                        if(((bytesOnLine + 1) % 4) == 0)
                        {
                            sb.Append(" ");
                        }
                        ++byteOffset;
                        ++bytesOnLine;
                        if(bytesOnLine >= 16)
                        {
                            System.Diagnostics.Debug.WriteLine(sb.ToString());
                            sb = null;
                            bytesOnLine = 0;
                        }
                    }
                    if(sb != null)
                    {
                        System.Diagnostics.Debug.WriteLine(sb.ToString());
                    }
                }
#endif
            }

#if DEBUG_BUFFER_OBJECTS
            if(BufferGL.BufferTargetGL == BufferTarget.UniformBuffer)
            {
                System.Diagnostics.Debug.WriteLine(
                    Name + 
                    ".WriteTo()" + 
                    " offset " + offset + 
                    " length " + data.Length
                );
            }
            if(BufferGL.BufferTargetGL == BufferTarget.ArrayBuffer)
            {
                System.Diagnostics.Debug.WriteLine(
                    Name + 
                    ".WriteTo()" + 
                    " offset " + offset + 
                    " length " + data.Length +
                    " count " + count + 
                    //" stride " + stride + 
                    " baseVertex " + this.baseVertex
                );
            }
#endif
            //data = null;
            needsUpload = false;
        }
        public void Touch(uint count, byte[] data)
        {
            needsUpload = true;
            this.count = count;
            this.size = data.Length;
            this.data = data;
        }

        public unsafe void Floats(int offset, Floats param)
        {
            //System.Diagnostics.Debug.WriteLine("Offset " + offset);
            fixed(byte* bptr = &data[offset])
            {
                float* ptr = (float*)bptr;
                for(int i = 0; i < param.Value.Length; ++i)
                {
                    float v = param.Value[i];
                    ptr[i] = v;
                    //System.Diagnostics.Debug.WriteLine("index " + i + " value " + v);
                }
            }
            needsUpload = true;
        }
        public unsafe void Ints(int offset, Ints param)
        {
            fixed(byte* bptr = &data[offset])
            {
                int* ptr = (int*)bptr;
                for(int i = 0; i < param.Value.Length; ++i)
                {
                    ptr[i] = param.Value[i];
                }
            }
            needsUpload = true;
        }
        public unsafe void UInts(int offset, UInts param)
        {
            fixed(byte* bptr = &data[offset])
            {
                uint* ptr = (uint*)bptr;
                for(int i = 0; i < param.Value.Length; ++i)
                {
                    ptr[i] = param.Value[i];
                }
            }
            needsUpload = true;
        }
    }
}
