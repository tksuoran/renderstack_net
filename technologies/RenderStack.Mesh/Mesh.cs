using System;
using RenderStack.Graphics;

using BeginMode         = OpenTK.Graphics.OpenGL.BeginMode;

namespace RenderStack.Mesh
{
    [Serializable]
    /// \brief Basic renderable shape, vertex buffer and index buffer. Can also have multiple representations, see MeshMode.
    /// \note Mostly stable, somewhat experimental.
    public class Mesh : IMeshSource
    {
        Mesh    IMeshSource.GetMesh { get { return this; } }

        private System.Diagnostics.StackTrace constructorStackTrace;

        public Mesh()
        {
            constructorStackTrace = new System.Diagnostics.StackTrace(true);
        }

        private string          name;
        private IBufferRange[]  indexBufferRanges = new IBufferRange[(int)(MeshMode.Count)];

        public  string          Name                { get { return name; } set { name = value; } }
        public  IBufferRange    VertexBufferRange   { get; set; } // \todo use constructor to set VertexBuffer?

        public IBufferRange IndexBufferRange(MeshMode meshMode)
        {
            return indexBufferRanges[(int)meshMode];
        }
        public bool HasIndexBufferRange(MeshMode meshMode)
        {
            if(meshMode == MeshMode.NotSet) return false;
            return indexBufferRanges[(int)meshMode] != null;
        }
        public IBufferRange FindOrCreateIndexBufferRange(
            MeshMode    meshMode, 
            IBuffer     buffer, 
            BeginMode   beginMode
        )
        {
            if(HasIndexBufferRange(meshMode) == false)
            {
                var indexBufferRange = buffer.CreateIndexBufferRange(beginMode);
                indexBufferRanges[(int)meshMode] = indexBufferRange;

                return indexBufferRange;
            }
            else
            {
                var indexBufferRange = indexBufferRanges[(int)meshMode];
                return indexBufferRanges[(int)meshMode];
            }
        }

    }
}
