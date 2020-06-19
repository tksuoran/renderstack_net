// #define DEBUG_BUFFER_OBJECTS
// #define DEBUG_UNIFORM_BUFFER

using UInt32  = System.UInt32;
using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
#if false
    public interface IVertexBufferRange
    {
        //void Set(Attribute attribute, params byte[] values);
        //void Set(Attribute attribute, params int[] values);
        //void Set(Attribute attribute, params uint[] values);
        //void Set(Attribute attribute, params float[] values);
        void Set(Attribute attribute, Vector2 value);
        void Set(Attribute attribute, Vector3 value);
        void Set(Attribute attribute, Vector4 value);
        void Position(Vector3 value);
        void Position(Vector3 value, int index);
        void Normal(Vector3 value);
        void Normal(Vector3 value, int index);
        void TexCoord(Vector2 value);
        void TexCoord(Vector2 value, int index);
    }
    public interface IIndexBufferRange
    {
        void Point(UInt32 index0);
        void Line(UInt32 index0, UInt32 index1);
        void Triangle(UInt32 index0, UInt32 index1, UInt32 index2);
        void Quad(UInt32 index0, UInt32 index1, UInt32 index2, UInt32 index3);
    }
#endif
    public interface IBufferRange
    {
        string              Name                { get; set; }
        BufferGL            BufferGL            { get; }
        //BufferRL            BufferRL            { get; }
        DrawElementsType    DrawElementsTypeGL  { get; }
        BufferTarget        BufferTargetGL      { get; }
        VertexFormat        VertexFormat        { get; }
        UInt32              Count               { get; }
        BeginMode           BeginMode           { get; }
        long                Size                { get; }
        IUniformBlock       UniformBlock        { get; }
        long                OffsetBytes         { get; }
        int                 BaseVertex          { get; }
        bool                NeedsUploadGL       { get; }
        bool                NeedsUploadRL       { get; }

        //VertexStreamRL      VertexStreamRL(IProgram program);
        VertexStreamGL      VertexStreamGL(AttributeMappings mappings);

        bool Match              (IBufferRange other);
        void Allocate           (int size);
        void UseUniformBufferGL ();
        //void UseUniformBufferRL ();
        void WriteTo            (long offset, bool force);
        void Touch              (uint count, byte[] data);
        void UpdateGL           ();
        //void UpdateRL           ();
        void UseGL              ();
        void UseRL              ();
        void Floats             (int offset, Floats param);
        void Ints               (int offset, Ints param);
        void UInts              (int offset, UInts param);
    }
}
