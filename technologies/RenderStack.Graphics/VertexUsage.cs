using System;

namespace RenderStack.Graphics
{
    [Serializable]
    /// \note Mostly stable.
    public enum VertexUsage
    {
        None            =   0,
        Position        =   1,
        Tangent         =   2,
        Normal          =   4,
        Bitangent       =   8,
        Color           =  16,
        Weights         =  32,
        MatrixIndices   =  64,
        TexCoord        = 128,
        Id              = 256
    }
}
