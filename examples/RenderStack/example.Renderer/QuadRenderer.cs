using OpenTK.Graphics.OpenGL;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Renderer
{
    // \brief A service to render Quads
    public class QuadRenderer
    {
        private Mesh                mesh;
        private IBuffer             vertexBuffer;
        private IBuffer             indexBuffer;
        private IBufferRange        vertexBufferRange;
        private IBufferRange        indexBufferRange;
        private VertexBufferWriter  vertexWriter;
        private IndexBufferWriter   indexWriter;
        private Attribute           position;
        private Attribute           texcoord;
        private Attribute           color;

        public Mesh                 Mesh            { get { return mesh; } }
        public bool                 NotEmpty        { get { return indexBufferRange.Count > 0; } }

        public QuadRenderer(IRenderer renderer)
        {
            mesh = new Mesh();

            var vertexFormat = new VertexFormat();
            position = vertexFormat.Add(new Attribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3));
            texcoord = vertexFormat.Add(new Attribute(VertexUsage.TexCoord, VertexAttribPointerType.Float, 0, 2));
            color = vertexFormat.Add(new Attribute(VertexUsage.Color, VertexAttribPointerType.Float, 0, 4));
            // Some gfx cards fail to show last vertex right if texcoord is set to have 3 components

            vertexBuffer = BufferFactory.Create(vertexFormat, BufferUsageHint.StaticDraw);
            indexBuffer = BufferFactory.Create(DrawElementsType.UnsignedInt, BufferUsageHint.StaticDraw);

            mesh.VertexBufferRange = vertexBufferRange = vertexBuffer.CreateVertexBufferRange();

            indexBufferRange = mesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonFill,
                indexBuffer,
                BeginMode.Triangles
            );

            vertexWriter = new VertexBufferWriter(mesh.VertexBufferRange);
            indexWriter = new IndexBufferWriter(indexBufferRange);
        }

        public void Begin()
        {
            vertexWriter.BeginEdit();
            indexWriter.BeginEdit();
        }

        public void Quad(TextureGL texture)
        {
            float z = 10.0f;
            Vector3 bottomLeft = new Vector3(4.0f, 4.0f, z);
            Vector3 topRight   = bottomLeft + new Vector3(texture.Size.Width, texture.Size.Height, z);
            Quad(bottomLeft, topRight);
        }

        //   b-----c
        //   |\    |
        //   |  \  |
        //   |    \|
        //   a-----d
        public void Quad(Vector3 bottomLeft, Vector3 topRight, Vector4 rgba)
        {
            indexWriter.Quad(
                vertexWriter.CurrentIndex, 
                vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,
                vertexWriter.CurrentIndex + 3 
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color,     rgba.X, rgba.Y, rgba.Z, rgba.W);
            vertexWriter.Set(texcoord,  0.0f,           0.0f);
            vertexWriter.Set(position,  bottomLeft.X,   bottomLeft.Y,   bottomLeft.Z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color,     rgba.X, rgba.Y, rgba.Z, rgba.W);
            vertexWriter.Set(texcoord,  0.0f,           1.0f);
            vertexWriter.Set(position,  bottomLeft.X,   topRight.Y,     bottomLeft.Z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color,     rgba.X, rgba.Y, rgba.Z, rgba.W);
            vertexWriter.Set(texcoord,  1.0f,           1.0f);
            vertexWriter.Set(position,  topRight.X,     topRight.Y,     bottomLeft.Z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color,     rgba.X, rgba.Y, rgba.Z, rgba.W);
            vertexWriter.Set(texcoord,  1.0f,           0.0f);
            vertexWriter.Set(position,  topRight.X,     bottomLeft.Y,   bottomLeft.Z);   ++vertexWriter.CurrentIndex;
        }
        public void Quad(Vector3 bottomLeft, Vector3 topRight)
        {
            Quad(bottomLeft, topRight, Vector4.One);
        }

        public void CubeCrossPosZCenter(TextureGL texture)
        {
            float z = 10.0f;
            //float a = texture.Width;
            float a = 128.0f;
            Vector2 O = new Vector2(4.0f, 30.0f);
            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 D;

            //       B    C                 
            //        posy                  
            //       A    D                 
            // B    CB    CB    CB    C     
            //  negx  posz  posx  negz      
            // A    DA    DA    DA    D     
            //       B    C                 
            //        negy                  
            //       A    D                 

            //  negx   rz -ry rx
            A = O + new Vector2(0.0f, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f, -1.0f,  -1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f,  1.0f,  -1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f, -1.0f,   1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  posz   rx -ry rz
            A = O + new Vector2(a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f, -1.0f,   1.0f); 
            vertexWriter.Set(position,  A.X,    A.Y,    z);   
            ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  1.0f,   1.0f); 
            vertexWriter.Set(position,  B.X,    B.Y,    z);   
            ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f,   1.0f); 
            vertexWriter.Set(position,  C.X,    C.Y,    z);   
            ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f, -1.0f,   1.0f); 
            vertexWriter.Set(position,  D.X,    D.Y,    z);   
            ++vertexWriter.CurrentIndex;

            //  posy   rx  rz ry
            A = O + new Vector2(a, 2.0f * a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  1.0f,  -1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f,  -1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  negy   rx -rz ry
            A = O + new Vector2(a, 0.0f);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  -1.0f,   1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,   1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  posx  -rz -ry rx
            A = O + new Vector2(2.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,   1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f,   1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f,  -1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  negz  -rx -ry rz
            A = O + new Vector2(3.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f,  -1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,   1.0f,  -1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;
        }
        public void CubeCrossNegZCenter(TextureGL texture)
        {
            float z = 10.0f;
            //float a = texture.Width;
            float a = 128.0f;
            Vector2 O = new Vector2(4.0f, 30.0f);
            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 D;

            //       B    C                 
            //        posy                  
            //       A    D                 
            // B    CB    CB    CB    C     
            //  negx  negz  posx  posz      
            // A    DA    DA    DA    D     
            //       B    C                 
            //        negy                  
            //       A    D                 

            //  negx   rz -ry rx  --  flipped third texcoord
            A = O + new Vector2(0.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f, -1.0f,   1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f,  1.0f,  -1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  -1.0f, -1.0f,  -1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  posz   rx -ry rz  --  flipped first texcoord
            A = O + new Vector2(3.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f, -1.0f,   1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f, -1.0f,   1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  posy   rx  rz ry  -- flipped third texcoord
            A = O + new Vector2(a, 2.0f * a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  1.0f,  -1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f,   1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f,  -1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  negy   rx -rz ry  --  flipped third texcoord
            A = O + new Vector2(a, 0.0f);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  -1.0f,   1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,   1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  posx  -rz -ry rx -- flipped third texcoord
            A = O + new Vector2(2.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f,  -1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f,   1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,   1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  negz  -rx -ry rz  --  flipped first texcoord
            A = O + new Vector2(1.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(a, a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord, -1.0f,   1.0f,  -1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f,  -1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  -1.0f,  -1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;
        }

        public float EdgeLength = 50.0f;

#if false
        public void HemiCubeCrossX(HemicubeRenderer renderer)
        {
            float z = 10.0f;
            //float a = texture.Width;
            float a = EdgeLength;
            Vector2 O = new Vector2(4.0f, 30.0f);
            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 D;

            //       B        C         
            //          posy            
            //       A   1    D         
            // B    CB        CB    C   
            //                          
            //                          
            //  negx    negz    posx    
            //   0       2       0      
            //                          
            // A    DA        DA    D   
            //       B        C         
            //          negy            
            //       A   1    D         

            //  negx   rz -ry rx  --  flipped third texcoord  --  half width AB
            A = O + new Vector2(0.5f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(0.5f * a, a);
            D = A + new Vector2(0.5f * a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.5f,  1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.5f,  0.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,  0.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,  1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  posx  -rz -ry rx -- flipped third texcoord -- half width CD
            A = O + new Vector2(2.0f * a, a);
            B = A + new Vector2(0.0f, a);
            C = A + new Vector2(0.5f * a, a);
            D = A + new Vector2(0.5f * a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  0.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.5f,  0.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.5f,  1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;
        }
        public void HemiCubeCrossY(HemicubeRenderer renderer)
        {
            float z = 10.0f;
            //float a = texture.Width;
            float a = EdgeLength;
            Vector2 O = new Vector2(4.0f, 30.0f);
            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 D;

            //       B        C         
            //          posy            
            //       A   1    D         
            // B    CB        CB    C   
            //                          
            //                          
            //  negx    negz    posx    
            //   0       2       0      
            //                          
            // A    DA        DA    D   
            //       B        C         
            //          negy            
            //       A   1    D         

            //  posy   rx  rz ry  -- flipped third texcoord -- half height BC
            A = O + new Vector2(a, 2.0f * a);
            B = A + new Vector2(0.0f, 0.5f * a);
            C = A + new Vector2(a, 0.5f * a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,   0.5f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,   1.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   1.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,   0.5f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;

            //  negy   rx -rz ry  --  flipped third texcoord -- half height AD
            A = O + new Vector2(a, 0.5f * a);
            B = A + new Vector2(0.0f, 0.5f * a);
            C = A + new Vector2(a, 0.5f * a);
            D = A + new Vector2(a, 0);
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,  0.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,  0.5f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  0.5f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  0.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;
        }
        public void HemiCubeCrossZ(HemicubeRenderer renderer)
        {
            float z = 10.0f;
            //float a = texture.Width;
            float a = EdgeLength;
            Vector2 O = new Vector2(4.0f, 30.0f);
            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 D;

            //       B        C         
            //          posy            
            //       A   1    D         
            // B    CB        CB    C   
            //                          
            //                          
            //  negx    negz    posx    
            //   0       2       0      
            //                          
            // A    DA        DA    D   
            //       B        C         
            //          negy            
            //       A   1    D         

            //  negz  -rx -ry rz  --  flipped first texcoord
            A = O + new Vector2(1.0f * a, a);                
            B = A + new Vector2(0.0f, a);                    
            C = A + new Vector2(a, a);                       
            D = A + new Vector2(a, 0);                       
            indexWriter.Quad(                                
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  1.0f); vertexWriter.Set(position,  A.X,    A.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  1.0f,  0.0f); vertexWriter.Set(position,  B.X,    B.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,  0.0f); vertexWriter.Set(position,  C.X,    C.Y,    z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
            vertexWriter.Set(texcoord,  0.0f,  1.0f); vertexWriter.Set(position,  D.X,    D.Y,    z);   ++vertexWriter.CurrentIndex;
        }
#endif

        public void End()
        {
            vertexWriter.EndEdit();
            indexWriter.EndEdit();
        }
    }
}
