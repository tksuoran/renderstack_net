using System;
using RenderStack.Geometry;
using RenderStack.Mesh;

using example.Renderer;

namespace example.Sandbox
{
    public partial class Operations
    {
        public void Truncate()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                Truncate(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    Truncate(model);
                }
            }
        }

        public void Truncate(Model model)
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

            Geometry newGeometry = new TruncateGeometryOperation(mesh.Geometry, true).Destination;
            newGeometry.ComputePolygonCentroids();
            newGeometry.ComputePolygonNormals();
            newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)Math.PI));
            newGeometry.BuildEdges();

            MeshModified op = new MeshModified(
                model,
                new MeshModified.State(
                    model.Name, 
                    model.Batch
                ),
                new MeshModified.State(
                    "Truncate(" + model.Name + ")", 
                    new Batch(
                        new GeometryMesh(
                            newGeometry, 
                            NormalStyle.PolygonNormals
                        ),
                        model.Batch.Material
                    )
                )
            );

            operationStack.Do(op);
        }
    }
}
