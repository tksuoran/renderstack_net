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
    public class FontStyle
    {
        public class FontInfo
        {
            private int[] padding = new int[4];
            private int[] spacing = new int[2];
            public string   Face            { get; private set; }
            public int      Size            { get; private set; }
            public bool     Bold            { get; private set; }
            public bool     Italic          { get; private set; }
            public string   Charset         { get; private set; }
            public bool     Unicode         { get; private set; }
            public int      StretchH        { get; private set; }
            public int      Smooth          { get; private set; }
            public int      Aa              { get; private set; }
            public int[]    Padding         { get { return padding; } }
            public int[]    Spacing         { get { return spacing; } }
            public int      Outline         { get; private set; }

            public FontInfo(XmlNode node)
            {
                Face        = node.Attributes["face"].Value;
                Size        = int.Parse(node.Attributes["size"].Value);
                Bold        = int.Parse(node.Attributes["bold"].Value) != 0;
                Italic      = int.Parse(node.Attributes["italic"].Value) != 0;
                Charset     = node.Attributes["charset"].Value;
                Unicode     = int.Parse(node.Attributes["unicode"].Value) != 0;
                StretchH    = int.Parse(node.Attributes["stretchH"].Value);
                Smooth      = int.Parse(node.Attributes["smooth"].Value);
                Aa          = int.Parse(node.Attributes["aa"].Value);
                string[] paddingStr = node.Attributes["padding"].Value.Split(",".ToCharArray());
                for(int i = 0; i < 4; ++i)
                    Padding[i] = int.Parse(paddingStr[i]);
                string[] spacingStr = node.Attributes["spacing"].Value.Split(",".ToCharArray());
                for(int i = 0; i < 2; ++i)
                    Spacing[i] = int.Parse(spacingStr[i]);
                Outline     = int.Parse(node.Attributes["outline"].Value);
            }
        }
        public class FontCommon
        {
            public float    LineHeight;
            public float    Base;
            public float    ScaleW;
            public float    ScaleH;
            public int      Pages;
            public bool     Packed;
            public int      AlphaChnl;
            public int      RedChnl;
            public int      GreenChnl;
            public int      BlueChnl;

            public FontCommon(XmlNode node)
            {
                LineHeight  = float.Parse(node.Attributes["lineHeight"].Value);
                Base        = float.Parse(node.Attributes["base"].Value);
                ScaleW      = float.Parse(node.Attributes["scaleW"].Value);
                ScaleH      = float.Parse(node.Attributes["scaleH"].Value);
                Pages       = int.Parse(node.Attributes["pages"].Value);
                Packed      = int.Parse(node.Attributes["packed"].Value) != 0;
                AlphaChnl   = int.Parse(node.Attributes["alphaChnl"].Value);
                RedChnl     = int.Parse(node.Attributes["redChnl"].Value);
                GreenChnl   = int.Parse(node.Attributes["greenChnl"].Value);
                BlueChnl    = int.Parse(node.Attributes["blueChnl"].Value);
            }
        }
        public class FontChar
        {
            public float U        { get; private set; }
            public float V        { get; private set; }
            public float U2       { get; private set; }
            public float V2       { get; private set; }
            public float Width    { get; private set; }
            public float Height   { get; private set; }
            public float XOffset  { get; private set; }
            public float YOffset  { get; private set; }
            public float XAdvance { get; private set; }

            private List<FontKerning>   kernings;
            public List<FontKerning>    Kernings { get { return kernings; } }

            public FontChar()
            {
            }
            public FontChar(XmlNode node, FontCommon common)
            {
                kernings = new List<FontKerning>();
                float x     = float.Parse(node.Attributes["x"].Value);
                float y     = float.Parse(node.Attributes["y"].Value);
                Width       = float.Parse(node.Attributes["width"].Value);
                Height      = float.Parse(node.Attributes["height"].Value);
                XOffset     = float.Parse(node.Attributes["xoffset"].Value);
                YOffset     = float.Parse(node.Attributes["yoffset"].Value);
                XAdvance    = float.Parse(node.Attributes["xadvance"].Value);
                //Page        = short.Parse(node.Attributes["page"].Value);
                //Chnl        = short.Parse(node.Attributes["chnl"].Value);
                U           = x / common.ScaleW;
                V           = y / common.ScaleH;
                U2          = U + Width / common.ScaleW;
                V2          = V + Height / common.ScaleH;
            }
        }
        public class FontKerning : IEquatable<FontKerning>, IComparable<FontKerning>
        {
            public short Second { get; private set; }
            public short Amount;

            /*  User for comparing  */ 
            public FontKerning(short second)
            {
                Second = second;
            }

            public FontKerning(short second, short amount)
            {
                Second = second;
                Amount = amount;
            }

            public bool Equals(FontKerning other)
            {
                return Second == other.Second;
            }

            public int CompareTo(FontKerning other)
            {
                return Second.CompareTo(other.Second);
            }
        }

        private TextureGL   texture;
        private FontCommon  common;
        private FontChar[]  chars = new FontChar[256];

        public TextureGL    Texture     { get { return texture; } }
        public float        LineHeight  { get { return common.LineHeight; } }

        public FontStyle(string filename)
        {
            XmlDocument spec = new XmlDocument();
            spec.Load(filename);

            //info = new FontInfo(spec.SelectSingleNode("font/info"));
            common = new FontCommon(spec.SelectSingleNode("font/common"));

            XmlNode pagesNode   = spec.SelectSingleNode("font/pages");
            XmlNode pageNode    = pagesNode.ChildNodes.Item(0);
            /*int     pageId      = */int.Parse(pageNode.Attributes["id"].Value);
            string  file        = pageNode.Attributes["file"].Value;
            var     image       = new RenderStack.Graphics.Image("res/images/" + file);

            texture = new TextureGL(image, false);

            XmlNode charsNode = spec.SelectSingleNode("font/chars");
            foreach(XmlNode charNode in charsNode.ChildNodes)
            {
                int charId = int.Parse(charNode.Attributes["id"].Value);
                chars[charId] = new FontChar(charNode, common);
            }
            XmlNode kerningsNode = spec.SelectSingleNode("font/kernings");
            if(kerningsNode != null)
            {
                foreach(XmlNode kerningNode in kerningsNode.ChildNodes)
                {
                    short first   = short.Parse(kerningNode.Attributes["first"].Value);
                    short second  = short.Parse(kerningNode.Attributes["second"].Value);
                    if(first >= 0 && second >= 0 && first < 256 && second < 256)
                    {
                        short amount  = short.Parse(kerningNode.Attributes["amount"].Value);
                        bool modifiedExisting = false;
                        foreach(FontKerning existingKerning in chars[first].Kernings)
                        {
                            /*  This part is overly paranoid - if unicode wasn't used,  */ 
                            /*  kerning pairs are broken...  */ 
                            if(existingKerning.Second == second)
                            {
                                if(
                                    (existingKerning.Amount == 0) ||
                                    (System.Math.Sign(existingKerning.Amount) != System.Math.Sign(amount))
                                )
                                {
                                    existingKerning.Amount = 0;
                                    break;
                                }
                                else if(amount < 0 && amount < existingKerning.Amount)
                                {
                                    existingKerning.Amount = amount;
                                    modifiedExisting = true;
                                    break;
                                }
                                else if(amount > 0 && amount > existingKerning.Amount)
                                {
                                    existingKerning.Amount = amount;
                                    modifiedExisting = true;
                                    break;
                                }
                            }
                        }
                        if(modifiedExisting == false)
                        {
                            chars[first].Kernings.Add(new FontKerning(second, amount));
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Trace.TraceWarning("No kerning info"); 
            }
            foreach(FontChar @char in chars)
            {
                if(@char != null)
                {
                    if(@char.Kernings != null)
                    {
                        @char.Kernings.Sort();
                    }
                }
            }
        }

        Attribute           position;
        Attribute           texCoord;
        Attribute           color;
        Rectangle           bounds = new Rectangle();
        public Rectangle   Bounds { get { return bounds; } }

        public void BeginPrint(Mesh.Mesh mesh)
        {
            bounds.ResetForGrow();

            var vertexFormat = mesh.VertexBufferRange.VertexFormat;
            position    = vertexFormat.FindAttribute(VertexUsage.Position, 0);
            texCoord    = vertexFormat.FindAttribute(VertexUsage.TexCoord, 0);
            color       = vertexFormat.FindAttribute(VertexUsage.Color,    0);
        }

        public void LowPrint(
            VertexBufferWriter vertexWriter, 
            IndexBufferWriter indexWriter, 
            float x, 
            float y, 
            float z, 
            string text
        )
        {
            if(string.IsNullOrEmpty(text) == true)
            {
                return;
            }
            y += common.Base;
            FontChar fontChar = null;
            for(int i = 0; i < text.Length; ++i)
            {
                char c = text[i];

                fontChar = chars[c];
                if(fontChar == null)
                {
                    continue;
                }

                float a     = fontChar.XAdvance;
                float w     = fontChar.Width;
                float h     = fontChar.Height;
                float ox    = fontChar.XOffset;
                float oy    = fontChar.YOffset;

                indexWriter.Quad(
                    vertexWriter.CurrentIndex, 
                    vertexWriter.CurrentIndex + 1, 
                    vertexWriter.CurrentIndex + 2, 
                    vertexWriter.CurrentIndex + 3
                );
                indexWriter.CurrentIndex += 6;
                vertexWriter.Set(position,  x +     ox, y -     oy, z);
                vertexWriter.Set(texCoord,  fontChar.U, fontChar.V);
                vertexWriter.Set(color,     1.0f, 1.0f, 1.0f);
                ++vertexWriter.CurrentIndex; 
                bounds.Extend(x +     ox, y -     oy);

                vertexWriter.Set(position, x + w + ox, y -     oy, z);
                vertexWriter.Set(texCoord, fontChar.U2, fontChar.V);
                vertexWriter.Set(color,     1.0f, 1.0f, 1.0f);
                ++vertexWriter.CurrentIndex; 
                bounds.Extend(x + w + ox, y -     oy);

                vertexWriter.Set(position, x + w + ox, y - h - oy, z);
                vertexWriter.Set(texCoord, fontChar.U2, fontChar.V2);
                vertexWriter.Set(color,     1.0f, 1.0f, 1.0f);
                ++vertexWriter.CurrentIndex; 
                bounds.Extend(x + w + ox, y - h - oy);

                vertexWriter.Set(position, x +     ox, y - h - oy, z);
                vertexWriter.Set(texCoord, fontChar.U, fontChar.V2);
                vertexWriter.Set(color,     1.0f, 1.0f, 1.0f);
                ++vertexWriter.CurrentIndex; 
                bounds.Extend(x +     ox, y - h - oy);

                x += a;

                if(
                    (i + 1 < text.Length - 1) &&
                    (fontChar.Kernings != null)
                )
                {
                    char next = text[i + 1];

                    FontKerning compare = new FontKerning((short)next);

                    int index = fontChar.Kernings.BinarySearch(compare);

                    if(index >= 0)
                    {
                        short amount = fontChar.Kernings[index].Amount;
                        x += (float)(amount);
                    }
                }
            }
        }
    }
}
