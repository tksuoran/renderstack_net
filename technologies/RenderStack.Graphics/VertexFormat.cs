using System;
using System.Collections.Generic;

namespace RenderStack.Graphics
{
    [Serializable]
    /// \note Mostly stable.
    public class VertexFormat : IEquatable<VertexFormat>
    {
        private List<Attribute> attributes = new List<Attribute>();
        private int stride;

        public int Stride { get { return stride; } private set { stride = value; } }

        public override string ToString()
        {
            var s = new System.Text.StringBuilder();
            s.Append("stride = ");
            s.Append(stride.ToString());
            s.Append(" ");
            foreach(Attribute attribute in attributes)
            {
                s.Append(attribute.UsageString);
                s.Append(" ");
                s.Append(attribute.Dimension);
                s.Append(" ");
            }
            return s.ToString();
        }

        private void prepareAlignment()
        {
            while((Stride & (1 + 2 + 4)) != 0)
            {
                ++Stride;
            }
        }

        public void Clear()
        {
            attributes.Clear();
            Stride = 0;
        }
        public Attribute Add(Attribute vertexAttribute)
        {
            vertexAttribute.Offset = Stride;
            Stride += vertexAttribute.Stride();
            attributes.Add(vertexAttribute);

            prepareAlignment();

            return vertexAttribute;
        }
        public bool HasAttribute(VertexUsage usage, int index)
        {
            foreach(Attribute attribute in attributes)
            {
                if(
                    ((attribute.Usage & usage) == usage) &&
                    (attribute.Index == index)
                )
                {
                    return true;
                }
            }
            return false;
        }
        public Attribute FindAttribute(VertexUsage usage, int index)
        {
            foreach(Attribute attribute in attributes)
            {
                if(
                    ((attribute.Usage & usage) == usage) &&
                    (attribute.Index == index)
                )
                {
                    return attribute;
                }
            }
            return null;
        }

        public static bool operator==(VertexFormat a, VertexFormat b)
        {
            if(object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if(object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            if(a.attributes.Count != b.attributes.Count)
            {
                return false;
            }
            for(int i = 0; i < a.attributes.Count; ++i)
            {
                if(a.attributes[i] != b.attributes[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator!=(VertexFormat a, VertexFormat b)
        {
            if(object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if(object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            if(a.attributes.Count != b.attributes.Count)
            {
                return true;
            }
            for(int i = 0; i < a.attributes.Count; ++i)
            {
                if(a.attributes[i] != b.attributes[i])
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int a = 0;
            foreach(var attribute in attributes)
            {
                a ^= attribute.GetHashCode();
            }
            return a;
        }

        bool System.IEquatable<VertexFormat>.Equals(VertexFormat o)
        {
            return this == o;
        }

        public override bool Equals(object o)
        {
            if(o is VertexFormat)
            {
                VertexFormat c = (VertexFormat)o;
                return this == c;
            }
            return false;
        }
    }
}
