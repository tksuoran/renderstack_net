//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Math;
using RenderStack.Mesh;

using example.Brushes;
using example.Renderer;

namespace example.Sandbox
{
    public class Insert : IOperation
    {
        private Model model;
        // \todo consider add mode (with or without physics)

        public Insert(Model model)
        {
            this.model = model;
        }

        public void Execute(Application sandbox)
        {
            Services.Get<SceneManager>().AddModel(model);
            var sounds = Services.Get<Sounds>();
            if(sounds != null) sounds.Insert.Play();
        }
        public void Undo(Application sandbox)
        {
            Services.Get<SceneManager>().RemoveModel(model);
        }
    }

    public partial class Operations
    {
        public void Insert()
        {
            var selectionManager = Services.Get<SelectionManager>();
            var brushManager = Services.Get<BrushManager>();
            if(
                (selectionManager == null) || 
                (brushManager == null) ||
                (selectionManager.HoverModel == null) || 
                (selectionManager.HoverPolygon == null)
            )
            {
                return;
            }

            GeometryMesh mesh = selectionManager.HoverModel.Batch.MeshSource as GeometryMesh;
            if(mesh == null)
            {
                return;
            }

            int cornerCount = selectionManager.HoverPolygon.Corners.Count;
            Brush brush = brushManager.CurrentBrush;
            if(
                (brush == null) || 
                (brush.PolygonDictionary.ContainsKey(cornerCount) == false)
            )
            {
                Dictionary<int, Brush> dictionary = brushManager.Dictionary(userInterfaceManager.CurrentPalette);
                if(dictionary == null)
                {
                    return;
                }

                if(dictionary.ContainsKey(cornerCount))
                {
                    brush = dictionary[cornerCount];
                }
                else
                {
                    return;
                }
            }

            GeometryMesh    brushPolyMesh   = brush.Model.Batch.MeshSource as GeometryMesh;
            
            Polygon         brushPolygon    = brush.PolygonDictionary[cornerCount];
            ReferenceFrame  hoverFrame      = Operations.MakePolygonReference(mesh.Geometry, selectionManager.HoverPolygon);
            ReferenceFrame  brushFrame      = Operations.MakePolygonReference(brushPolyMesh.Geometry, brushPolygon);

            float scale = hoverFrame.Scale / brushFrame.Scale;

            if(scale != 1.0f)
            {
                //Geometry newGeometry = new CloneGeometryOperation(brushPolyMesh.Geometry, null).Destination;
                Matrix4  scaleTransform;
                Matrix4.CreateScale(scale, out scaleTransform);
                //newGeometry.Transform(scaleTransform);
                //newGeometry.ComputePolygonCentroids();
                //newGeometry.ComputePolygonNormals();
                ////newGeometry.ComputeCornerNormals(0.0f);
                //newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (0.0f * (float)Math.PI));
                //newGeometry.BuildEdges();

                brushFrame.Transform(scaleTransform);

                brushPolyMesh = brush.ScaledMesh(scale); //new GeometryMesh(newGeometry, NormalStyle.PolygonNormals);
            }

            //  Flip target normal..
            hoverFrame.Normal *= -1.0f;

            Matrix4 hoverTransform  = hoverFrame.GetTransform();
            Matrix4 brushTransform  = brushFrame.GetTransform();
            Matrix4 inverseBrush    = Matrix4.Invert(brushTransform);
            Matrix4 align           = hoverTransform * inverseBrush;

            Material material = materialManager[userInterfaceManager.CurrentMaterial];

            Model newModel = new Model(
                brush.Model.Name,
                brushPolyMesh,
                material,
                align
            );

            if(Configuration.physics)
            {
                newModel.PhysicsShape = brush.ScaledShape(scale);
            }

            newModel.Frame.Updated = false;
            newModel.Frame.Parent = selectionManager.HoverModel.Frame;
            newModel.Frame.UpdateHierarchicalNoCache();
            sceneManager.UnparentModel(newModel);
            //newModel.Frame.Updated = true;

            //  Added objects are not static in physics sence
            newModel.Static = !userInterfaceManager.AddWithPhysics;

            Insert op = new Insert(newModel);
            operationStack.Do(op);

#if false
            newModel.Frame.Debug(0);

            newModel.Selected = true;
            selectionManager.Models.Add(newModel);
            selectionManager.HoverModel.Selected = true;
            selectionManager.Models.Add(selectionManager.HoverModel);
#endif
        }
    }
}
