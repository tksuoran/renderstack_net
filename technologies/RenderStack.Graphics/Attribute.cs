using System;
using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    [Serializable]
    /// \brief Attribute for use in VertexFormat. Attributes should always be made to be part of one VertexFormat.
    /// 
    /// \note Mostly stable.
    public class Attribute : IEquatable<Attribute>
    {
        public static int SizeOfType(VertexAttribPointerType type)
        {
            switch(type)
            {
                case VertexAttribPointerType.Byte:           return 1;
                case VertexAttribPointerType.Double:         return 8;
                case VertexAttribPointerType.Float:          return 4;
                case VertexAttribPointerType.HalfFloat:      return 2;
                case VertexAttribPointerType.Int:            return 4;
                case VertexAttribPointerType.Short:          return 2;
                case VertexAttribPointerType.UnsignedByte:   return 1;
                case VertexAttribPointerType.UnsignedInt:    return 4;
                case VertexAttribPointerType.UnsignedShort:  return 2;
                default: throw new System.Exception("Invalid typeCode");
            }
        }

        private VertexUsage              usage;
        private VertexAttribPointerType  type;
        private int                      index;
        private int                      dimension;
        private int                      offset;
        private bool                     normalized;

        public VertexUsage              Usage       { get { return usage; }         private set { usage = value; } }
        public VertexAttribPointerType  Type        { get { return type; }          private set { type = value; } }
        public int                      Index       { get { return index; }         private set { index = value; } }
        public int                      Dimension   { get { return dimension; }     private set { dimension = value; } }
        public int                      Offset      { get { return offset; }        set { offset = value; } }
        public bool                     Normalized  { get { return normalized; }    set { normalized = value; } }

        public string UsageString
        {
            get
            {
                //  TODO test bits and build correct string
                switch(Usage)
                {
                    case VertexUsage.Position:        return "Position";
                    case VertexUsage.Normal:          return "Normal";
                    case VertexUsage.Tangent:         return "Tangent";
                    case VertexUsage.Bitangent:       return "Bitangent";
                    case VertexUsage.Color:           return "Color";
                    case VertexUsage.Weights:         return "Weights";
                    case VertexUsage.MatrixIndices:   return "MatrixIndices";
                    case VertexUsage.TexCoord:        return "TexCoord";
                    case VertexUsage.Id:              return "Id";
                    default: return "";
                }
            }
        }

        public Attribute()
        {
        }

        public Attribute(
            VertexUsage             usage,
            VertexAttribPointerType type,
            int                     index,
            int                     dimension
        )
        {
            Usage      = usage;
            Type       = type;
            Index      = index;
            Dimension  = dimension;
            Offset     = -1; // Set by VertexFormat when attached to it
            Normalized = false;
        }

        public Attribute(
            VertexUsage             usage,
            VertexAttribPointerType type,
            int                     index,
            int                     dimension,
            bool                    normalized
        )
        {
            Usage      = usage;
            Type       = type;
            Index      = index;
            Dimension  = dimension;
            Offset     = -1; // Set by VertexFormat when attached to it
            Normalized = normalized;
        }

        public int Stride()
        {
            return Dimension * SizeOfType(Type);
        }

        public static bool operator==(Attribute a, Attribute b)
        {
            if(object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if(object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return
                (a.usage == b.usage) &&
                (a.type == b.type) &&
                (a.dimension == b.dimension) &&
                (a.offset == b.offset) &&
                (a.normalized == b.normalized);
        }

        public static bool operator!=(Attribute a, Attribute b)
        {
            if(object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if(object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return
                (a.usage != b.usage) ||
                (a.type != b.type) ||
                (a.dimension != b.dimension) ||
                (a.offset != b.offset) ||
                (a.normalized != b.normalized);
        }

        public override int GetHashCode()
        {
            return usage.GetHashCode() ^ type.GetHashCode() ^ dimension.GetHashCode() ^ offset.GetHashCode() ^ normalized.GetHashCode();
        }

        bool System.IEquatable<Attribute>.Equals(Attribute o)
        {
            return this == o;
        }

        public override bool Equals(object o)
        {
            if(o is Attribute)
            {
                Attribute c = (Attribute)o;
                return this == c;
            }
            return false;
        }

    }
}
