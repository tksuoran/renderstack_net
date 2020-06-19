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
using System.Xml;
using System.IO;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Mesh;

using Attribute = RenderStack.Graphics.Attribute;
using Buffer = RenderStack.Graphics.BufferGL;

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
