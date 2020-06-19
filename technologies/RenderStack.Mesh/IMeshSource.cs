namespace RenderStack.Mesh
{
    /// \note Somewhat experimental
    public interface IMeshSource
    {
        string  Name        { get; set; }
        Mesh    GetMesh     { get; }
    }
}
