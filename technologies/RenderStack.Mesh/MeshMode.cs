namespace RenderStack.Mesh
{
    /// \brief Selects which Mesh representation should be used .
    /// \note Experimental.
    public enum MeshMode
    {
        NotSet             = 0,
        PolygonFill        = 1,
        EdgeLines          = 2,
        CornerPoints       = 3,
        CornerNormals      = 4,
        PolygonCentroids   = 5,
        Count              = 6
    }
}
