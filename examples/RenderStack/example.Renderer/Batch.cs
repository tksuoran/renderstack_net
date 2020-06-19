using System;
using RenderStack.Math;
using RenderStack.Mesh;

namespace example.Renderer
{
    /// \brief Rendering unit that stores Mesh and Material and also supports culling
    public class Batch : IDisposable
    {
        private bool disposed = false;
        ~Batch()
        {
            Dispose();
        }
        public void Dispose()
        {
            disposed = true;
        }
        public IMeshSource  MeshSource      { get; set; }
        public Mesh         Mesh            { get { return MeshSource.GetMesh; } }
        public Material     Material        { get; set; }
        public BoundingBox  BoundingBox;
        public Sphere       BoundingSphere;

        public Batch(IMeshSource meshSource)
        {
            MeshSource = meshSource;
            GeometryMesh g = meshSource as GeometryMesh;
            if(g != null)
            {
                //BoundingBox = g.Geometry.BoundingBox();  
                BoundingSphere = g.Geometry.ComputeBoundingSphere();
            }
            else
            {
                BoundingSphere.Center = Vector3.Zero;
                BoundingSphere.Radius = float.MaxValue;
            }
        }
        public Batch(IMeshSource meshSource, Material material)
        :this(meshSource)
        {
            Material = material;
        }
    }
}
