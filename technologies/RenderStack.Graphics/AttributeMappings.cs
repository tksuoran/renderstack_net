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

//#define LOG_BINDINGS

using System;
using System.Collections.Generic;
using GL = OpenTK.Graphics.OpenGL.GL;
using OpenGL = OpenTK.Graphics.OpenGL;
using Int16   = System.Int16;
using Int32   = System.Int32;
using UInt16  = System.UInt16;
using UInt32  = System.UInt32;
using Single  = System.Single;
using Double  = System.Double;
using VertexAttribPointerType   = OpenTK.Graphics.OpenGL.VertexAttribPointerType;
using VertexAttribIPointerType  = OpenTK.Graphics.OpenGL.VertexAttribIPointerType;

namespace RenderStack.Graphics
{
    [Serializable]
    /// \brief Contains information how to map shader attributes to a vertex usage, index and dimension.
    /// 
    /// \note Mostly stable.
    public class AttributeMapping
    {
        public int          Slot        { get; private set; }
        public string       Name        { get; private set; }
        public VertexUsage  SrcUsage    { get; private set; }
        public VertexUsage  DstUsage    { get; private set; }
        public int          SrcIndex    { get; private set; } // index in vertex format
        public int          DstIndex    { get; private set; } // index in shader, or texture unit for fixed function
        public int          Dimension   { get; private set; }

        public AttributeMapping(
            int         slot,
            string      name,
            VertexUsage usage,
            int         index,
            int         dimension
        )
        {
            Slot        = slot;
            Name        = name;
            SrcUsage    = usage;
            DstUsage    = usage;
            SrcIndex    = index;
            DstIndex    = index;
            Dimension   = dimension;
        }
        public AttributeMapping(
            int         slot,
            string      name,
            VertexUsage srcUsage,
            int         srcIndex,
            VertexUsage dstUsage,
            int         dstIndex,
            int         dimension
        )
        {
            Slot        = slot;
            Name        = name;
            SrcUsage    = srcUsage;
            SrcIndex    = srcIndex;
            DstUsage    = dstUsage;
            DstIndex    = dstIndex;
            Dimension   = dimension;
        }
    }
    /**  Collection of attribute mappings  */
    public class AttributeMappings
    {
        //  Each instance is given unique index - used in vertex format
        private static int instanceIndex = 0;
        public readonly int InstanceIndex = instanceIndex++;

        private static Dictionary<string, AttributeMappings> pool = new Dictionary<string,AttributeMappings>();
        public static Dictionary<string, AttributeMappings> Pool { get { return pool; } }

        public AttributeMappings()
        {
            if(InstanceIndex >= Configuration.maxAttributeMappings)
            {
                throw new System.InsufficientMemoryException("Too many AttributeMappings");
            }
        }

        public static AttributeMappings Global = new AttributeMappings();

        private List<AttributeMapping> mappings = new List<AttributeMapping>();
        public List<AttributeMapping> Mappings { get { return mappings; } }

        public void Clear()
        {
            mappings.Clear();
        }

        public void Add(
            int         slot,
            string      name,
            VertexUsage usage,
            int         index,
            int         dimension
        )
        {
            var mapping = new AttributeMapping(slot, name, usage, index, dimension);
            mappings.Add(mapping);
        }

        public void Add(
            int         slot,
            string      name,
            VertexUsage srcUsage,
            int         srcIndex,
            VertexUsage dstUsage,
            int         dstIndex,
            int         dimension
        )
        {
            var mapping = new AttributeMapping(slot, name, srcUsage, srcIndex, dstUsage, dstIndex, dimension);
            mappings.Add(mapping);
        }

#if false
        public void BindAttributes(VertexStreamRL vertexStream, IProgram program, VertexFormat vertexFormat)
        {
            foreach(var mapping in mappings)
            {
                var programAttribute = program.Attribute(mapping.Name);
                if(programAttribute == null)
                {
                    continue;
                }
                var vertexFormatAttribute = vertexFormat.FindAttribute(mapping.SrcUsage, mapping.SrcIndex);
                if(vertexFormatAttribute == null)
                {
                    continue;
                }

                vertexStream.Add(
                    mapping,
                    vertexFormatAttribute,
                    vertexFormat.Stride,
                    programAttribute.Slot
                );
            }
        }
#endif

        public void BindAttributes(IVertexStream vertexStream, VertexFormat vertexFormat)
        {
            foreach(var mapping in mappings)
            {
                if(vertexFormat.HasAttribute(mapping.SrcUsage, mapping.SrcIndex))
                {
                    var attribute = vertexFormat.FindAttribute(mapping.SrcUsage, mapping.SrcIndex);

                    vertexStream.Add(
                        mapping,
                        attribute,
                        vertexFormat.Stride
                    );
                }
            }
        }

    }
}
