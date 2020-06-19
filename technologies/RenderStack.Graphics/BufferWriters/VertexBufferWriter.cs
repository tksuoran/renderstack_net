using System.IO;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class VertexBufferWriter
    {
        private BufferWriter BufferWriter;
        //private IBuffer         Buffer          { get { return BufferWriter.Buffer; } }
        private MemoryStream MemoryStream => BufferWriter.memoryStream;
        private BinaryWriter Writer => BufferWriter.writer;
        public  uint         CurrentIndex    { get { return BufferWriter.CurrentIndex; } set { BufferWriter.CurrentIndex = value; } }

        public VertexBufferWriter(IBufferRange bufferRange)
        {
            BufferWriter = new BufferWriter(bufferRange);
        }

        public void BeginEdit()
        {
            BufferWriter.BeginEdit();
        }
        public void EndEdit()
        {
            BufferWriter.EndEdit();
        }

#if true
        public void Set(Attribute attribute, params byte[] values)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            foreach(byte value in values)
            {
                Writer.Write(value);
            }
        }
        public void Set(Attribute attribute, params int[] values)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            foreach(int value in values)
            {
                Writer.Write(value);
            }
        }
        public void Set(Attribute attribute, params uint[] values)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            foreach(uint value in values)
            {
                Writer.Write(value);
            }
        }
        public void Set(Attribute attribute, params float[] values)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            foreach(float value in values)
            {
                Writer.Write(value);
            }
        }
#else
        public void Set(Attribute attribute, byte value)
        {
            writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            writer.Write(value);
        }
        public void Set(Attribute attribute, int value)
        {
            writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            writer.Write(value);
        }
        public void Set(Attribute attribute, uint value)
        {
            writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            writer.Write(value);
        }
        public void Set(Attribute attribute, float value)
        {
            writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, System.IO.SeekOrigin.Begin);
            writer.Write(value);
        }
#endif
        public void Set(Attribute attribute, Vector2 value)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, SeekOrigin.Begin);
            Writer.Write(value.X);
            Writer.Write(value.Y);
        }
        public void Set(Attribute attribute, Vector3 value)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, SeekOrigin.Begin);
            Writer.Write(value.X);
            Writer.Write(value.Y);
            Writer.Write(value.Z);
        }
        public void Set(Attribute attribute, Vector4 value)
        {
            Writer.Seek((int)CurrentIndex * BufferWriter.bufferRange.VertexFormat.Stride + attribute.Offset, SeekOrigin.Begin);
            Writer.Write(value.X);
            Writer.Write(value.Y);
            Writer.Write(value.Z);
            Writer.Write(value.W);
        }
        public void Position(Vector3 value)
        {
            var attribute = BufferWriter.bufferRange.VertexFormat.FindAttribute(VertexUsage.Position, 0);
            Set(attribute, value);
        }
        public void Position(Vector3 value, int index)
        {
            var attribute = BufferWriter.bufferRange.VertexFormat.FindAttribute(VertexUsage.Position, index);
            Set(attribute, value);
        }
        public void Normal(Vector3 value)
        {
            var attribute = BufferWriter.bufferRange.VertexFormat.FindAttribute(VertexUsage.Normal, 0);
            Set(attribute, value);
        }
        public void Normal(Vector3 value, int index)
        {
            var attribute = BufferWriter.bufferRange.VertexFormat.FindAttribute(VertexUsage.Normal, index);
            Set(attribute, value);
        }
        public void TexCoord(Vector2 value)
        {
            var attribute = BufferWriter.bufferRange.VertexFormat.FindAttribute(VertexUsage.TexCoord, 0);
            Set(attribute, value);
        }
        public void TexCoord(Vector2 value, int index)
        {
            var attribute = BufferWriter.bufferRange.VertexFormat.FindAttribute(VertexUsage.TexCoord, index);
            Set(attribute, value);
        }
    }
}