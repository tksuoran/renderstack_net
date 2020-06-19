using OpenTK.Graphics.OpenGL;
using RenderStack.Graphics;
using RenderStack.Mesh;

using Attribute = RenderStack.Graphics.Attribute;

namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public class TextBuffer
    {
        private Mesh.Mesh           mesh;
        private FontStyle           fontStyle;

        //  Using private buffer objects limits Buffer.UpdateAll() cost
        private IBuffer             vertexBuffer;
        private IBuffer             indexBuffer;
        private VertexBufferWriter  vertexWriter;
        private IndexBufferWriter   indexWriter;

        public Mesh.Mesh            Mesh        { get { return mesh; } }
        public FontStyle            FontStyle   { get { return fontStyle; } }

        public TextBuffer(FontStyle fontStyle)
        {
            this.fontStyle = fontStyle;

            mesh = new Mesh.Mesh();

            var vertexFormat = new VertexFormat();
            vertexFormat.Add(new Attribute(VertexUsage.Position,  VertexAttribPointerType.Float, 0, 3));
            vertexFormat.Add(new Attribute(VertexUsage.TexCoord,  VertexAttribPointerType.Float, 0, 2));
            vertexFormat.Add(new Attribute(VertexUsage.Color,     VertexAttribPointerType.Float, 0, 3));

            vertexBuffer = BufferFactory.Create(vertexFormat, BufferUsageHint.DynamicDraw);
            indexBuffer = BufferFactory.Create(DrawElementsType.UnsignedInt, BufferUsageHint.StaticDraw);

            mesh.VertexBufferRange = vertexBuffer.CreateVertexBufferRange();
            var indexBufferRange = mesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonFill, 
                indexBuffer,
                BeginMode.Triangles
            );

            vertexWriter = new VertexBufferWriter(mesh.VertexBufferRange);
            indexWriter = new IndexBufferWriter(indexBufferRange);
        }
        public void BeginPrint()
        {
            fontStyle.BeginPrint(mesh);
            vertexWriter.BeginEdit();
            indexWriter.BeginEdit();
        }
        public void EndPrint()
        {
            vertexWriter.EndEdit();
            indexWriter.EndEdit();
        }
        public void LowPrint(
            float       x, 
            float       y, 
            float       z, 
            string      text
        )
        {
            fontStyle.LowPrint(
                vertexWriter,
                indexWriter,
                x, 
                y, 
                z, 
                text
            );
        }

        public void Print(
            float   x, 
            float   y, 
            float   z, 
            string  text
        )
        {
            BeginPrint();
            LowPrint(x, y, z, text);
            EndPrint();
        }
    }
}
