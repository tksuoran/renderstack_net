using System;
using RenderStack.Geometry;
using RenderStack.Mesh;
using example.Renderer;

namespace example.Sandbox
{
    public class MeshModified : IOperation
    {
        public class State
        {
            public string   Name;
            public Batch    Batch;

            public State(string name, Batch batch)
            {
                Name = name;
                Batch = batch;
            }
        }
        private Model   model;
        private State   before;
        private State   after;

        public MeshModified(
            Model   model,
            State   before,
            State   after
        )
        {
            this.model = model;
            this.before = before;
            this.after = after;
        }

        void IOperation.Execute(Application sandbox)
        {
            SceneManager sceneManager = RenderStack.Services.BaseServices.Get<SceneManager>();
            sceneManager.RemoveModel(model);
            model.Name = after.Name;
            model.Batch = after.Batch;
            sceneManager.AddModel(model);
            sceneManager.UpdateShadowMap = true;
        }

        void IOperation.Undo(Application sandbox)
        {
            SceneManager sceneManager = RenderStack.Services.BaseServices.Get<SceneManager>();
            sceneManager.RemoveModel(model);
            model.Name = before.Name;
            model.Batch = before.Batch;
            sceneManager.AddModel(model);
            sceneManager.UpdateShadowMap = true;
        }
    }

    public partial class Operations
    {
        public void CatmullClark()
        {
            SceneManager sceneManager = RenderStack.Services.BaseServices.Get<SceneManager>();
            SelectionManager selectionManager = RenderStack.Services.BaseServices.Get<SelectionManager>();
            if (selectionManager == null)
            {
                return;
            }
            if(selectionManager.Models.Count == 0)
            {
                CatmullClark(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    CatmullClark(model);
                }
            }
        }

        public void CatmullClark(Model model)
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
            Geometry newGeometry = new CatmullClarkGeometryOperation(mesh.Geometry).Destination;
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
                    "Catmull-Clark(" + model.Name + ")", 
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
