using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class IndexBufferWriter
    {
        private BufferWriter    BufferWriter;
        //private IBuffer         Buffer          { get { return BufferWriter.Buffer; } }
        private MemoryStream    memoryStream    { get { return BufferWriter.memoryStream; } }
        private BinaryWriter    writer          { get { return BufferWriter.writer; } }
        public  uint            CurrentIndex    { get { return BufferWriter.CurrentIndex; } set { BufferWriter.CurrentIndex = value; } }

        public IndexBufferWriter(IBufferRange bufferRange)
        {
            this.BufferWriter = new BufferWriter(bufferRange);
        }

        public void BeginEdit()
        {
            BufferWriter.BeginEdit();
        }
        public void EndEdit()
        {
            BufferWriter.EndEdit();
        }

        private void SetIndex(uint indexPosition, UInt32 indexValue)
        {
            switch(BufferWriter.bufferRange.DrawElementsTypeGL)
            {
                case DrawElementsType.UnsignedByte:
                {
                    if(indexValue > byte.MaxValue)
                    {
                        throw new System.ArgumentOutOfRangeException();
                    }
                    byte value = (byte)indexValue;
                    writer.Seek((int)indexPosition, System.IO.SeekOrigin.Begin);
                    writer.Write(value);
                    break;
                }
                case DrawElementsType.UnsignedInt:
                {
                    if(indexValue > UInt32.MaxValue)
                    {
                        throw new System.ArgumentOutOfRangeException();
                    }
                    UInt32 value = (UInt32)indexValue;
                    writer.Seek((int)indexPosition * sizeof(uint), System.IO.SeekOrigin.Begin);
                    writer.Write(value);
                    break;
                }
                case DrawElementsType.UnsignedShort:
                {
                    if(indexValue > UInt16.MaxValue)
                    {
                        throw new System.ArgumentOutOfRangeException();
                    }
                    UInt16 value = (UInt16)indexValue;
                    writer.Seek((int)indexPosition * sizeof(ushort), System.IO.SeekOrigin.Begin);
                    writer.Write(value);
                    break;
                }
                default:
                {
                    throw new System.ArgumentOutOfRangeException();
                }
            }
        }
        public void Point(UInt32 index0)
        {
            SetIndex(CurrentIndex, index0);
        }
        public void Line(UInt32 index0, UInt32 index1)
        {
            SetIndex(CurrentIndex + 0, index0);
            SetIndex(CurrentIndex + 1, index1);
        }
        public void Triangle(UInt32 index0, UInt32 index1, UInt32 index2)
        {
            SetIndex(CurrentIndex + 0, index0);
            SetIndex(CurrentIndex + 1, index1);
            SetIndex(CurrentIndex + 2, index2);
        }
        public void Quad(UInt32 index0, UInt32 index1, UInt32 index2, UInt32 index3)
        {
            SetIndex(CurrentIndex + 0, index0);
            SetIndex(CurrentIndex + 1, index1);
            SetIndex(CurrentIndex + 2, index2);
            SetIndex(CurrentIndex + 3, index0);
            SetIndex(CurrentIndex + 4, index2);
            SetIndex(CurrentIndex + 5, index3);
        }
    }
}