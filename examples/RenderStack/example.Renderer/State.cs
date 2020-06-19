using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;

namespace example.Renderer
{
    public class State
    {
        private Camera camera;
        public Camera           Camera { get { return camera; } set { camera = value; } } 
        public Viewport         Viewport;
        public Mesh             Mesh;
        public Material         Material;
        public MeshMode         MeshMode = MeshMode.NotSet;
        public IProgram         Program;
        public IBuffer          VertexBuffer;
        public IBuffer          IndexBuffer;
        public IBufferRange     IndexBufferRange;
        public IVertexStream    VertexStream;

        public State()
        {
        }
        public State(State old)
        {
            Camera              = old.Camera           ;
            Viewport            = old.Viewport         ;
            Mesh                = old.Mesh             ;
            Material            = old.Material         ;
            MeshMode            = old.MeshMode         ;
            Program             = old.Program          ;
            VertexBuffer        = old.VertexBuffer     ;
            IndexBuffer         = old.IndexBuffer      ;
            IndexBufferRange    = old.IndexBufferRange ;
            VertexStream        = old.VertexStream;
        }
    }
}
