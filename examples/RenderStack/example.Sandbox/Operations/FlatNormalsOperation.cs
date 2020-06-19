using System;
using RenderStack.Geometry;
using RenderStack.Mesh;

using example.Renderer;

namespace example.Sandbox
{
    public partial class Operations
    {
        public void FlatNormals()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                FlatNormals(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    FlatNormals(model);
                }
            }
        }

        public void FlatNormals(Model model)
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

            Geometry newGeometry = new CloneGeometryOperation(mesh.Geometry, null).Destination;
            newGeometry.ComputePolygonCentroids();
            newGeometry.ComputePolygonNormals();
            //newGeometry.ComputeCornerNormals(0.0f/*2.0f * (float)Math.PI*/);
            newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (0.0f * (float)Math.PI));
            newGeometry.BuildEdges();

            model.Batch.MeshSource = new GeometryMesh(newGeometry, NormalStyle.PolygonNormals);
            model.Name = "Flat(" + model.Name + ")";
        }

        public void SmoothNormals()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                SmoothNormals(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    SmoothNormals(model);
                }
            }
        }

        public void SmoothNormals(Model model)
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

            Geometry newGeometry = new CloneGeometryOperation(mesh.Geometry, null).Destination;
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
                    "Smooth(" + model.Name + ")", 
                    new Batch(
                        new GeometryMesh(newGeometry, NormalStyle.PointNormals),
                        model.Batch.Material
                    )
                )
            );

            operationStack.Do(op);
        }
    }
}
