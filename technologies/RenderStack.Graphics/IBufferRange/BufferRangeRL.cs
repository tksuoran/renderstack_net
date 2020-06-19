// #define DEBUG_BUFFER_OBJECTS
// #define DEBUG_UNIFORM_BUFFER

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UInt16  = System.UInt16;
using UInt32  = System.UInt32;

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
    [Serializable]
    public class BufferRangeRL : IBufferRange
    {
        private string          name = "";
        public  string          Name { get { return name; } set { name = value; } }
        private byte[]          data;
        private bool            needsUpload = false;
        [NonSerialized]
        private IUniformBlock   uniformBlock;       // uniform buffers are not serialized
        private BufferRL        buffer;
        private UInt32          count = 0;
        private BeginMode       beginMode;          /*  mode  */ 
        private long            size = 0;
        private long            offsetBytes = 0;    /*  Offset in Buffer  */
        private int             baseVertex = -1;    /*  for use with DrawElementsBaseVertex()  */

        public  OpenTK.Graphics.OpenGL.DrawElementsType DrawElementsTypeGL  { get { return buffer.DrawElementsTypeGL; } }
        public  OpenTK.Graphics.OpenGL.BufferTarget     BufferTargetGL      { get { return buffer.BufferTargetGL; } }
        public  OpenTK.Graphics.OpenGL.BeginMode        BeginMode           { get { return (OpenTK.Graphics.OpenGL.BeginMode)beginMode; } }
        public  VertexFormat    VertexFormat    { get { return buffer.VertexFormat; } }
        public  UInt32          Count           { get { return count; } }
        public  long            Size            { get { return size; } }
        public  IUniformBlock   UniformBlock    { get { return uniformBlock; } }
        public  long            OffsetBytes     { get { return offsetBytes; } }
        public  int             BaseVertex      { get { return baseVertex; } }
        public  bool            NeedsUploadGL   { get { return false; } }
        public  bool            NeedsUploadRL   { get { return needsUpload; } }

        public VertexStreamRL VertexStreamRL(IProgram program)
        {
            return buffer.VertexStreamRL(program);
        }

        public VertexStreamGL VertexStreamGL(AttributeMappings mappings)
        {
            throw new InvalidOperationException();
        }

        public static int SizeOfType(DrawElementsType type)
        {
            switch(type)
            {
                case DrawElementsType.UnsignedByte:   return 1;
                case DrawElementsType.UnsignedInt:    return 4;
                case DrawElementsType.UnsignedShort:  return 2;
                default: throw new System.Exception("Invalid typeCode");
            }
        }

        public void UpdateAll()
        {
            buffer.UpdateAll();
        }
        public void UpdateGL()
        {
            buffer.UpdateGL();
        }
        public void UpdateRL()
        {
            buffer.UpdateRL();
        }
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
                case OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer:
                {
                    if(VertexFormat != other.VertexFormat)
                    {
                        return false;
                    }
                    break;
                }
                //  Index Buffer
                case OpenTK.Graphics.OpenGL.BufferTarget.ElementArrayBuffer:
                {
                    if(DrawElementsTypeGL != other.DrawElementsTypeGL)
                    {
                        return false;
                    }
                    break;
                }
                //  UniformBuffer
                case OpenTK.Graphics.OpenGL.BufferTarget.UniformBuffer:
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
            throw new InvalidOperationException();
        }
        public void UseRL()
        {
            buffer.Use();
        }
        public BufferGL BufferGL { get { throw new InvalidOperationException(); } }
        public BufferRL BufferRL { get { return buffer; } }

        //  Index buffer, with valid begin mode
        internal BufferRangeRL(BufferRL buffer, BeginMode beginMode)
        {
            this.buffer = buffer;
            this.beginMode = beginMode;
            buffer.Add(this);
        }
        //  Vertex Buffer
        internal BufferRangeRL(BufferRL buffer)
        {
            this.buffer = buffer;
            unchecked
            {
                this.beginMode = (BeginMode)(uint.MaxValue); // vertex buffers do not have a valid begin mode value 
            }
            buffer.Add(this);
        }
        //  Uniform Buffer
        internal BufferRangeRL(BufferRL buffer, IUniformBlock uniformBlock)
        {
            this.uniformBlock = uniformBlock;
            this.buffer = buffer;
            unchecked
            {
                this.beginMode = (BeginMode)(uint.MaxValue); // uniform buffers do not have a valid begin mode value 
            }
            buffer.Add(this);

            //  Pre-initialize uniform buffer as we do not use BeginEdit() / EndEdit()
            size = uniformBlock.Size;
            data = new byte[size];
        }
        public void Allocate(int size)
        {
            this.size = size;
            data = new byte[size];
        }
        public void UseUniformBufferGL()
        {
            throw new InvalidOperationException();
        }
        public void UseUniformBufferRL()
        {
            throw new InvalidOperationException();
        }

        public unsafe void WriteTo(long offset, bool force)
        {
            if((needsUpload == false) && (force == false))
            {
                return;
            }
            this.offsetBytes = offset;
            if(BufferTargetGL == OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer)
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
            if(BufferTargetGL == OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer)
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
            {
#if DEBUG_BUFFER_OBJECTS
                if(GL.IsBuffer(Buffer.BufferObject) == false)
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
                GL.GetBufferParameter(Buffer.BufferTarget, BufferParameterName.BufferSize, out glSize);
#endif
                RL.BufferSubData(
                    (BufferTarget)BufferTargetGL, 
                    (int)offset, 
                    data.Length, 
                    data
                );
#if DEBUG_UNIFORM_BUFFER
                System.Console.WriteLine(
                    "RL.BufferSubData(" + 
                    (BufferTarget)BufferTargetGL + ", " + 
                    (IntPtr)offset + ", " + 
                    (IntPtr)data.Length + ", " + 
                    data +
                    "); " + Name + " (offsetBytes = " + this.offsetBytes + ")"
                );

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
            if(buffer.BufferTarget == BufferTarget.UniformBuffer)
            {
                System.Diagnostics.Debug.WriteLine(
                    Name + 
                    ".WriteTo()" + 
                    " offset " + offset + 
                    " length " + data.Length
                );
            }
            if(buffer.BufferTarget == BufferTarget.ArrayBuffer)
            {
                System.Diagnostics.Debug.WriteLine(
                    Name + 
                    ".WriteTo()" + 
                    " offset " + offset + 
                    " length " + data.Length +
                    " count " + count + 
                    " stride " + stride + 
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
