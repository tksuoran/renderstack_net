using VertexAttribPointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType;

namespace RenderStack.Graphics
{
    /// \brief Contains information necessary to feed uniform data to GL.
    /// 
    /// \note Mostly stable.
    public class AttributeBinding
    {
        public AttributeMapping         AttributeMapping    { get; private set; }
        public Attribute                Attribute           { get; private set; }
        public int                      Stride              { get; private set; }
        public int                      Slot                { get; private set; }

        public int                      Size                { get { return Attribute.Dimension; } }
        public VertexAttribPointerType  Type                { get { return Attribute.Type; } }
        public bool                     Normalized          { get { return Attribute.Normalized; } }
        public int                      Offset              { get { return Attribute.Offset; } }

        public AttributeBinding(
            AttributeMapping    mapping,
            Attribute           attribute,
            int                 stride
        )
        {
            AttributeMapping = mapping;
            Attribute = attribute;
            Stride = stride;
        }
        public AttributeBinding(
            AttributeMapping    mapping,
            Attribute           attribute,
            int                 stride,
            int                 slot
        )
        {
            AttributeMapping = mapping;
            Attribute = attribute;
            Stride = stride;
            Slot = slot;
        }
    }
}