using System;
using System.IO;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public class BufferWriter
    {
        internal    IBufferRange    bufferRange;
        //public      IBuffer         Buffer  { get { return bufferRange.Buffer; } }
        internal    MemoryStream    memoryStream;
        internal    BinaryWriter    writer;
        internal    int             stride = 0;
        public      uint            CurrentIndex = 0;
        private     byte[]          data;

        private void UpdateData()
        {
            long currentSize = CurrentIndex * stride;

            if(currentSize == 0)
            {
                return;
            }

            if(
                (bufferRange.NeedsUploadGL == true) ||
                (bufferRange.NeedsUploadRL == true)
            )
            {
                //  \todo inefficient use by app
            }

            long padding = currentSize - memoryStream.Position;
            if(padding > 0)
            {
                memoryStream.Seek(currentSize - 1, SeekOrigin.Begin);
                memoryStream.WriteByte(0);
#if DEBUG_BUFFER_OBJECTS
                Debug.WriteLine("adding padding " + padding.ToString());
#endif
            }

            data = memoryStream.ToArray();
            if(data.Length == 0)
            {
                return;
            }
            if(data.Length != currentSize)
            {
                throw new Exception();
            }

            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            memoryStream.SetLength(0);

#if DEBUG_BUFFER_OBJECTS
            Debug.WriteLine((Name ?? "?") + ".EndEdit() size = " + size +  ", count = " + count + ", stride = " + stride);
#endif
        }
        private bool Match(IBufferRange targetBufferRange)
        {
            return bufferRange.Match(targetBufferRange);
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

        public BufferWriter(IBufferRange bufferRange)
        {
            this.bufferRange = bufferRange;
        }
        public long StreamPosition { get { return writer.BaseStream.Position; } }
        public void BeginEdit()
        {
            if(bufferRange.BufferTargetGL == BufferTarget.ArrayBuffer)
            {
                stride = bufferRange.VertexFormat.Stride;
            }
            else if(bufferRange.BufferTargetGL == BufferTarget.ElementArrayBuffer)
            {
                stride = SizeOfType(bufferRange.DrawElementsTypeGL);
            }
            else if(bufferRange.BufferTargetGL == BufferTarget.UniformBuffer)
            {
                stride = 0; // not used
            }
            else
            {
                throw new System.Exception("Invalid buffer stride");
            }

            CurrentIndex = 0;

            memoryStream = memoryStream ?? new MemoryStream();
            writer = writer ?? new BinaryWriter(memoryStream);

            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            memoryStream.SetLength(0);
        }
        public void ExportTo(IBufferRange targetBufferRange)
        {
            if(bufferRange.Match(targetBufferRange) == false)
            {
                throw new InvalidOperationException("Target IBufferRange does not match with BufferWriter IBufferRange");
            }
            if(data != null)
            {
                bufferRange.Touch(CurrentIndex, data);
            }
        }
        public void EndEdit()
        {
            UpdateData();
            if(data != null)
            {
                ExportTo(bufferRange);
            }
        }
    }
}
