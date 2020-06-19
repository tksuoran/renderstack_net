using System;

using RenderStack.Geometry;
using RenderStack.Mesh;

using example.Renderer;

namespace example.Sandbox
{
    public partial class Operations
    {
        public void Triangulate()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                Triangulate(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    Triangulate(model);
                }
            }
        }

        public void Triangulate(Model model)
        {
            if(model == null)
            {
                return;
            }
            GeometryMesh mesh = model.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }
            Geometry newGeometry = new TriangulateGeometryOperation(mesh.Geometry).Destination;
            newGeometry.ComputePolygonCentroids();
            newGeometry.ComputePolygonNormals();
            //newGeometry.ComputeCornerNormals(2.0f * (float)Math.PI);
            newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)Math.PI));
            newGeometry.BuildEdges();

            MeshModified op = new MeshModified(
                model,
                new MeshModified.State(
                    model.Name, 
                    model.Batch
                ),
                new MeshModified.State(
                    "Triangulate(" + model.Name + ")", 
                    new Batch(
                        new GeometryMesh(newGeometry, NormalStyle.PolygonNormals),
                        model.Batch.Material
                    )
                )
            );

            operationStack.Do(op);

            //model.MeshSource = new GeometryMesh(newGeometry);
            //model.Name = "Triangulate(" + model.Name + ")";
        }
    }
}