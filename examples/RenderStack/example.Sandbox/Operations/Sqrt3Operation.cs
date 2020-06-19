using System;
using RenderStack.Geometry;
using RenderStack.Mesh;

using example.Renderer;

namespace example.Sandbox
{
    public partial class Operations
    {
        public void Sqrt3()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                Sqrt3(selectionManager.HoverModel);
            }
            else
            {
                // TODO Fix
                foreach(var model in selectionManager.Models)
                {
                    Sqrt3(model);
                }
            }
        }
        public void Sqrt3(Model model)
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

            Geometry newGeometry = new Sqrt3GeometryOperation(mesh.Geometry).Destination;
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
                    "Sqrt3(" + model.Name + ")", 
                    new Batch(
                        new GeometryMesh(
                            newGeometry,
                            NormalStyle.PointNormals
                        ),
                        model.Batch.Material
                    )
                )
            );

            operationStack.Do(op);
        }
    }
}