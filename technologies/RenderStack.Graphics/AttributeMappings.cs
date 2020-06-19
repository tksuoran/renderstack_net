//#define LOG_BINDINGS

using System;
using System.Collections.Generic;

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
