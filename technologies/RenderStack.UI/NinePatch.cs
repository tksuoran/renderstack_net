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
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;

namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public class NinePatch
    {
        private NinePatchStyle      style;
        private Mesh.Mesh           mesh;
        //private float             xOffset;
        private Vector2             size;

        //  Using private buffer objects limits Buffer.UpdateAll() cost
        private IBuffer             vertexBuffer;
        private IBuffer             indexBuffer;
        private VertexBufferWriter  vertexWriter;

        public NinePatchStyle       NinePatchStyle { get { return style; } }
        public Mesh.Mesh            Mesh{ get { return mesh; } }
        public Vector2              Size { get { return size; } }

        public NinePatch(NinePatchStyle style)
        {
            this.style = style;

            //mesh = new Mesh.Mesh(BufferUsageHint.DynamicDraw);
            mesh = new RenderStack.Mesh.Mesh();

            VertexFormat vertexFormat = new VertexFormat();
            vertexFormat.Add(new Attribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3));
            vertexFormat.Add(new Attribute(VertexUsage.TexCoord, VertexAttribPointerType.Float, 0, 2));

            // \todo Allocate vertex buffers form from UI BufferPool and use double buffered Buffers
            // \todo Share one index buffer among all UI components that have the same index buffer
            //Buffer vertexBuffer = BufferPool.Instance.GetVertexBuffer(vertexFormat, BufferUsageHint.DynamicDraw);
            //Buffer indexBuffer = BufferPool.Instance.GetIndexBuffer(DrawElementsType.UnsignedShort, BufferUsageHint.StaticDraw);
            vertexBuffer = BufferFactory.Create(vertexFormat, BufferUsageHint.DynamicDraw);
            indexBuffer = BufferFactory.Create(DrawElementsType.UnsignedShort, BufferUsageHint.StaticDraw);

            mesh.VertexBufferRange = vertexBuffer.CreateVertexBufferRange();
            IBufferRange indexBufferRange = mesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonFill, 
                indexBuffer,
                BeginMode.Triangles
            );
            var writer = new IndexBufferWriter(indexBufferRange);
            vertexWriter = new VertexBufferWriter(mesh.VertexBufferRange);

            //  12 13 14 15     
            //                  
            //   8  9 10 11     
            //                  
            //   4  5  6  7     
            //                  
            //   0  1  2  3     

            writer.BeginEdit();

            writer.Quad( 4,  5,  1,  0); writer.CurrentIndex += 6;
            writer.Quad( 5,  6,  2,  1); writer.CurrentIndex += 6;
            writer.Quad( 6,  7,  3,  2); writer.CurrentIndex += 6;

            writer.Quad( 8,  9,  5,  4); writer.CurrentIndex += 6;
            writer.Quad( 9, 10,  6,  5); writer.CurrentIndex += 6;
            writer.Quad(10, 11,  7,  6); writer.CurrentIndex += 6;

            writer.Quad(12, 13,  9,  8); writer.CurrentIndex += 6;
            writer.Quad(13, 14, 10,  9); writer.CurrentIndex += 6;
            writer.Quad(14, 15, 11, 10); writer.CurrentIndex += 6;

            writer.EndEdit();

            // \bug
            //indexBuffer.UpdateAll();
        }

        public void Place(
            float x0,
            float y0,
            float z0,
            float width,
            float height
        )
        {
            //xOffset = x0;
            size = new Vector2(width, height);

            IBufferRange vertexBufferRange = mesh.VertexBufferRange;

            var position = vertexBufferRange.VertexFormat.FindAttribute(VertexUsage.Position, 0);
            var texCoord = vertexBufferRange.VertexFormat.FindAttribute(VertexUsage.TexCoord, 0);

            vertexWriter.BeginEdit();

            float[] b = new float[4];
            b[0] = 0.0f;
            b[1] = style.Border;
            b[2] = 1.0f - style.Border;
            b[3] = 1.0f;

            float[] x = new float[4];
            x[0] = x0;
            x[1] = x0 + style.Border * style.Texture.Size.Width;
            x[2] = x0 + width - style.Border * style.Texture.Size.Width;
            x[3] = x0 + width;

            float[] y = new float[4];
            y[0] = y0;
            y[1] = y0 + style.Border * style.Texture.Size.Height;
            y[2] = y0 + height - style.Border * style.Texture.Size.Height;
            y[3] = y0 + height;

            for(int yi = 0; yi < 4; ++yi)
            {
                for(int xi = 0; xi < 4; ++xi)
                {
                    vertexWriter.Set(position, x[xi], y[yi], z0);
                    vertexWriter.Set(texCoord, b[xi], b[3 - yi]);
                    ++vertexWriter.CurrentIndex; 
                }
            }

            vertexWriter.EndEdit();

            //  \todo optimize
            //  \bug 
            //vertexBufferRange.Buffer.UpdateAll();
        }
    }

    public class NinePatchStyle
    {
        private TextureGL texture;
        private float   border = 0.25f;

        public TextureGL  Texture { get { return texture; } }
        public float    Border  { get { return border; } }

        //  9 patch                                     
        //                                              
        //  4 x 4 = 16 vertices                         
        //  3 x 3 =  9 quads    = 9 x 6 = 54 indices    
        //                                              
        //  b = border                                  
        //                                              
        //  m:::n   o::p       0  a (0,     0) x0, y0   
        //  ::::    ::::       1  b (b,     0) x1, y0   
        //  i:::j   k::l       2  c (1-b,   0) x2, y0   
        //      ::::           3  d (1,     0) x3, y0   
        //      ::::           4  e (0,     b) x0, y1   
        //  e   f:::g  h       5  f (b,     b) x1, y1   
        //  ::::    ::::       6  g (1-b,   b) x2, y1   
        //  ::::    ::::       7  h (1,     b) x3, y1   
        //  a:::b   c::d       8  i (0,   1-b) x0, y2   
        //                     9  j (b,   1-b)          
        //  12 13 14 15       10  k (1-b, 1-b)          
        //                    11  l (1,   1-b)          
        //   8  9 10 11       12  m (0,     1)          
        //                    13  n (b,     1)          
        //   4  5  6  7       14  o (1-b,   1)          
        //                    15  p (1,     1)          
        //   0  1  2  3                                 

        public NinePatchStyle(TextureGL texture)
        {
            this.texture = texture;
        }
    }
}
