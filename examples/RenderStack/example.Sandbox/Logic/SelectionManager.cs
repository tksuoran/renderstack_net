using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;

namespace example.Sandbox
{
    public partial class SelectionManager : Service
    {
        public override string Name
        {
            get { return "SelectionManager"; }
        }

        CurveTool.CurveTool     curveTool;
        IDRenderer              idRenderer;
        LineRenderer            lineRenderer;
        ManipulatorManager      manipulatorManager;
        Operations              operations;
        PhysicsDrag             physicsDrag;
        IRenderer               renderer;
        SceneManager            sceneManager;
        Sounds                  sounds;
        UserInterfaceManager    userInterfaceManager;
        OpenTK.GameWindow       window;

        private GeometryMesh    sphereMesh;
        private Frame           boundingFrame = new Frame();
        private Material        stencilMaskWrite;
        private Material        silhouetteHidden;
        private Material        silhouetteVisible;
        private Material        edgeLinesSelected;
        //private Material        edgeLines;
        private Material        edgeLinesHidden;
        private Material        cornerPoints;
        private Material        polygonCentroids;
        private int             px;
        private int             py;

        private HashSet<Model>          models      = new HashSet<Model>();
        private HashSet<uint>           polygonIds  = new HashSet<uint>();
        private ReadOnlyHashSet<Model>  readonlyModels;

        //private GeometryMesh            selectionMesh;
        private Model                   hoverModel;
        private Group                   hoverGroup = new Group("SelectionManager.hoverGroup");
        private Polygon                 hoverPolygon;
        private Point                   HoverPoint;

        public ReadOnlyHashSet<Model>   Models          { get { return readonlyModels; } }
        public HashSet<uint>            PolygonIds      { get { return polygonIds; } }
        //public GeometryMesh             SelectionMesh   { get { return selectionMesh; } }
        public Group                    HoverGroup      { get { return hoverGroup; } }
        public Polygon                  HoverPolygon    { get { return hoverPolygon; } set { hoverPolygon = value; } }
        public float                    HoverDepth      { get; set; }
        public Vector3                  HoverPosition;
        public Model                    HoverModel
        { 
            get
            {
                return hoverModel;
            }
            set
            {
                if(value != hoverModel)
                {
                    hoverModel = value;
                    if(value == null)
                    {
                        sounds.Lo.Play();
                    }
                    else
                    {
                        sounds.Hi.Play();
                    }
                }
            }
        }

        public void Connect(
            CurveTool.CurveTool     curveTool,
            IDRenderer              idRenderer,
            LineRenderer            lineRenderer,
            ManipulatorManager      manipulatorManager,
            Operations              operations,
            PhysicsDrag             physicsDrag,
            IRenderer               renderer,
            SceneManager            sceneManager,
            Sounds                  sounds,
            UserInterfaceManager    userInterfaceManager,
            OpenTK.GameWindow       window
        )
        {
            this.curveTool = curveTool;
            this.idRenderer = idRenderer;
            this.lineRenderer = lineRenderer;
            this.manipulatorManager = manipulatorManager;
            this.operations = operations;
            this.physicsDrag = physicsDrag;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.sounds = sounds;
            this.userInterfaceManager = userInterfaceManager;
            this.window = window;

            InitializationDependsOn(manipulatorManager);
        }

        protected override void InitializeService()
        {
            models          = new HashSet<Model>();
            polygonIds      = new HashSet<uint>();
            readonlyModels  = new ReadOnlyHashSet<Model>(models);
            //selectionMesh   = new GeometryMesh();

            silhouetteHidden    = new Material("silhouetteHidden",  renderer.Programs["FatTriangle"], renderer.MaterialUB);
            silhouetteVisible   = new Material("silhouetteVisible", renderer.Programs["FatTriangle"], renderer.MaterialUB);
            edgeLinesSelected   = new Material("edgeLinesSelected", renderer.Programs["WideLineUniformColor"], renderer.MaterialUB);
            edgeLinesHidden     = new Material("edgeLinesHidden",   renderer.Programs["WideLineUniformColor"], renderer.MaterialUB);
            cornerPoints        = new Material("cornerPoints",      renderer.Programs["ColorFill"], renderer.MaterialUB);
            polygonCentroids    = new Material("polygonCentroids",  renderer.Programs["ColorFill"], renderer.MaterialUB);
            stencilMaskWrite    = new Material("stencilMaskWrite",  renderer.Programs["ColorFill"], renderer.MaterialUB);

            if(RenderStack.Graphics.Configuration.canUseGeometryShaders == false)
            {
                silhouetteHidden.MeshMode   = MeshMode.EdgeLines;
                silhouetteVisible.MeshMode  = MeshMode.EdgeLines;
            }

            var greaterDepthState = new DepthState();
            greaterDepthState.Enabled = true;
            greaterDepthState.Function = DepthFunction.Greater;

            float lineWidth = 2.0f;
            edgeLinesSelected.MeshMode  = MeshMode.EdgeLines;
            edgeLinesSelected.Floats("line_width").Set(lineWidth, lineWidth * lineWidth * 0.25f);
            edgeLinesSelected.Floats("line_color").Set(0.0f, 0.0f, 1.0f, 1.0f);
            edgeLinesSelected.Sync();

            lineWidth = 1.0f;
            edgeLinesHidden.MeshMode        = MeshMode.EdgeLines;
            edgeLinesHidden.DepthState      = greaterDepthState;
            edgeLinesHidden.MaskState       = new MaskState();
            edgeLinesHidden.MaskState.Red   = true;
            edgeLinesHidden.MaskState.Green = true;
            edgeLinesHidden.MaskState.Blue  = true;
            edgeLinesHidden.MaskState.Alpha = true;
            edgeLinesHidden.MaskState.Depth = false;
            edgeLinesHidden.Floats("line_width").Set(lineWidth, lineWidth * lineWidth * 0.25f);
            edgeLinesHidden.Floats("line_color").Set(0.0f, 0.0f, 0.5f, 1.0f);
            edgeLinesHidden.Sync();

            lineWidth = 6.0f;
            silhouetteHidden.DepthState = greaterDepthState;
            silhouetteHidden.MaskState = new MaskState();
            silhouetteHidden.MaskState.Depth = false;
            silhouetteHidden.Floats("line_width").Set(lineWidth, lineWidth * lineWidth * 0.25f);
            silhouetteHidden.Floats("line_color").Set(0.4f, 0.6f, 0.8f, 1.0f);

            //  StencilState to fail if stencil != 0, no writes
            silhouetteHidden.StencilState                       = new StencilState();
            silhouetteHidden.StencilState.Enabled               = true;
            silhouetteHidden.StencilState.Front.Function        = StencilFunction.Equal;
            silhouetteHidden.StencilState.Front.Reference       = 0;
            silhouetteHidden.StencilState.Front.TestMask        = 0xffff;
            silhouetteHidden.StencilState.Front.WriteMask       = 0xffff;
            silhouetteHidden.StencilState.Front.StencilFailOp   = StencilOp.Keep;
            silhouetteHidden.StencilState.Front.ZFailOp         = StencilOp.Keep;
            silhouetteHidden.StencilState.Front.ZPassOp         = StencilOp.Keep;

            silhouetteHidden.Sync();

            silhouetteVisible.MaskState = silhouetteHidden.MaskState;
            silhouetteVisible.StencilState = silhouetteHidden.StencilState;
            lineWidth = 6.0f;
            silhouetteVisible.Floats("line_width").Set(lineWidth, lineWidth * lineWidth * 0.25f);
            silhouetteVisible.Floats("line_color").Set(0.9f, 0.9f, 1.0f, 1.0f);
            silhouetteVisible.Sync();
            
            cornerPoints.Floats("point_color").Set(1.0f, 0.0f, 0.0f, 1.0f);
            cornerPoints.Sync();
            
            polygonCentroids.Floats("point_color").Set(0.5f, 0.0f, 1.0f, 1.0f);
            polygonCentroids.Sync();

            //  No depth test; Do not write to color or depth; Only write to stencil
            stencilMaskWrite.DepthState        = DepthState.Disabled;
            stencilMaskWrite.MaskState         = new MaskState();
            stencilMaskWrite.MaskState.Red     = false;
            stencilMaskWrite.MaskState.Green   = false;
            stencilMaskWrite.MaskState.Blue    = false;
            stencilMaskWrite.MaskState.Alpha   = false;
            stencilMaskWrite.MaskState.Depth   = false;

            //  StencilState to write 1 to stencil, always
            stencilMaskWrite.StencilState                       = new StencilState();
            stencilMaskWrite.StencilState.Enabled               = true;
            stencilMaskWrite.StencilState.Front.StencilFailOp   = StencilOp.Replace;
            stencilMaskWrite.StencilState.Front.ZFailOp         = StencilOp.Replace;
            stencilMaskWrite.StencilState.Front.ZPassOp         = StencilOp.Replace;
            stencilMaskWrite.StencilState.Front.Function        = StencilFunction.Always;
            stencilMaskWrite.StencilState.Front.Reference       = 1;
            stencilMaskWrite.StencilState.Front.TestMask        = 0xffff;
            stencilMaskWrite.StencilState.Front.WriteMask       = 0xffff;

            sphereMesh = new GeometryMesh(
                new RenderStack.Geometry.Shapes.Sphere(1.0f, 20, 12),
                NormalStyle.PointNormals
            );
        }

        public void ClearHover()
        {
            HoverModel = null;
            hoverPolygon = null;
        }
        public void ClearSelection()
        {
            foreach(var model in Models)
            {
                model.Selected = false;
            }
            models.Clear();

            //  TODO These should be tracked better
            if(manipulatorManager == null)
            {
                return;
            }

            manipulatorManager.Manipulator.ActiveAxis = Axis.None;
            manipulatorManager.Manipulator.Active = false;
        }
        public void Remove(Model model)
        {
            model.Selected = false;
            models.Remove(model);
        }
        public void Add(Model model)
        {
            if(model == null)
            {
                return;
            }
            model.Selected = true;
            models.Add(model);
        }
        public bool Contains(Model model)
        {
            return Models.Contains(model);
        }
        public void ResetHover()
        {
            HoverPoint = null;
        }
        public void ProcessIdBuffer(IDRenderer.HoverInfo hover, int px, int py)
        {
            ResetHover();

            uint compareId = 0xfffffeu;
            if(hover.Id == compareId)
            {
                HoverModel = null;
                HoverGroup.Clear();
                return;
            }

            try
            {
                Vector3 positionInWorld = sceneManager.Camera.WorldToClip.InverseMatrix.UnProject(
                    (float)px, 
                    (float)py, 
                    hover.Depth, 
                    renderer.Requested.Viewport.X,
                    renderer.Requested.Viewport.Y,
                    renderer.Requested.Viewport.Width,
                    renderer.Requested.Viewport.Height
                );

                HoverPosition = positionInWorld;

                if(userInterfaceManager.AutoFocus)
                {
                    Camera camera = sceneManager.Camera;
                    StereoParameters stereo = camera.Projection.StereoParameters;
                    float distance = camera.Frame.LocalToWorld.Matrix.GetColumn(3).Xyz.Distance(positionInWorld);
                    stereo.ViewportCenter.Z = distance;
                }

                HoverDepth = hover.Depth;

                uint modelPolygonId = 0;
                uint manipulatorPolygonId = 0;
                IDListEntry compareKey = new IDListEntry(hover.Id);

                Model renderModel = null;

                if(manipulatorManager != null)
                {
                    manipulatorManager.ManipulatorModel = manipulatorManager.ManipulatorGroup.IdTest(compareKey, out manipulatorPolygonId);
                    if(manipulatorManager.ManipulatorModel == null)
                    {
                        foreach(var group in hover.Groups)
                        {
                            renderModel = group.IdTest(compareKey, out modelPolygonId);
                            if(renderModel != null)
                            {
                                break;
                            }
                        }
                    }
                }

                UpdateHover(renderModel, modelPolygonId);
            }
            catch(System.Exception)
            {
                return;
            }

        }
        public void UpdateHover(Model renderModel, uint modelPolygonId)
        {
            HoverPolygon = null;
            if(renderModel != null)
            {
                if(HoverModel != renderModel)
                {
                    HoverModel = renderModel;
                    HoverGroup.Clear();
                    HoverGroup.Add(renderModel);
                }

                //if(Configuration.hoverDebug)
                {
                    PolygonIds.Clear();
                    PolygonIds.Add(modelPolygonId);

                    //  Temp debug code
                    IMeshSource meshSource = HoverModel.Batch.MeshSource;
                    if (meshSource is GeometryMesh original)
                    {
                        if (original.Geometry.PolygonAttributes.Contains<Vector3>("polygon_normals"))
                        {
                            if (modelPolygonId < original.Geometry.Polygons.Count)
                            {
                                HoverPolygon = original.Geometry.Polygons[(int)modelPolygonId];
#if true
                                //selectionMesh.Geometry = Geometry.Clone(original.Geometry, SelectedPolygonIds).Destination;
                                //selectionMesh.BuildMeshFromGeometry();
                                Frame hoverFrame = HoverModel.Frame;
                                Polygon polygon = HoverPolygon;
                                var polygonNormals = original.Geometry.PolygonAttributes.Find<Vector3>("polygon_normals");
                                var polygonCentroids = original.Geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");
                                Vector3 normalInModel = polygonNormals[polygon];
                                Vector3 positionInModel = hoverFrame.LocalToWorld.InverseMatrix.TransformPoint(HoverPosition);
                                var pointLocations = original.Geometry.PointAttributes.Find<Vector3>("point_locations");
                                Corner pivotCorner = original.Geometry.ClosestPolygonCorner(polygon, positionInModel);
                                Point pivotPoint = pivotCorner.Point;

                                HoverPoint = pivotPoint;

                                Edge firstEdge;
                                Edge secondEdge;
                                original.Geometry.PolygonCornerEdges(polygon, pivotCorner, out firstEdge, out secondEdge);
                                Point firstEdgeOutPoint = firstEdge.Other(pivotPoint);
                                Point secondEdgeOutPoint = secondEdge.Other(pivotPoint);
                                Vector3 pivotLocation = hoverFrame.LocalToWorld.Matrix.TransformPoint(pointLocations[pivotPoint]);
                                Vector3 firstOutLocation = hoverFrame.LocalToWorld.Matrix.TransformPoint(pointLocations[firstEdgeOutPoint]);
                                Vector3 secondOutLocation = hoverFrame.LocalToWorld.Matrix.TransformPoint(pointLocations[secondEdgeOutPoint]);
                                Vector3 normal = hoverFrame.LocalToWorld.Matrix.TransformDirection(normalInModel);
                                Vector3 firstDirection = Vector3.Normalize(firstOutLocation - pivotLocation);
                                Vector3 secondDirection = Vector3.Normalize(secondOutLocation - pivotLocation);
                                Vector3 centroid = hoverFrame.LocalToWorld.Matrix.TransformPoint(polygonCentroids[polygon]);
                                if (Configuration.hoverDebug)
                                {
                                    lineRenderer.Begin();
                                    lineRenderer.Line(HoverPosition, HoverPosition + 0.5f * normal, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                                    lineRenderer.Line(centroid, centroid + 0.25f * normal, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                                    /*LineRenderer.Line(pivotLocation, pivotLocation + normal);
                                    LineRenderer.Line(pivotLocation - firstDirection * 1.0f, pivotLocation + firstDirection  * 1.0f);
                                    LineRenderer.Line(pivotLocation - secondDirection * 1.0f, pivotLocation + secondDirection * 1.0f);*/
                                    lineRenderer.End();
                                }
                            }
#endif
                        }
                    }
                }
            }
            else
            {
                if(HoverModel != null)
                {
                    HoverGroup.Clear();
                }
                HoverModel = null;
            }
        }
        public void Update(int px, int py)
        {
            this.px = px;
            this.py = py;

            if(userInterfaceManager.LockMouse)
            {
                if(
                    (userInterfaceManager.MouseClick != null) && 
                    (userInterfaceManager.MouseClick.Button == OpenTK.Input.MouseButton.Left) &&
                    (HoverModel != null)
                )
                {
                    operations.Delete();
                    userInterfaceManager.MouseClick = null;
                }
                if(
                    (userInterfaceManager.MouseClick != null) && 
                    (userInterfaceManager.MouseClick.Button == OpenTK.Input.MouseButton.Right) &&
                    (HoverModel != null)
                )
                {
                    operations.Insert();
                    userInterfaceManager.MouseClick = null;
                }
            }

            if(Configuration.voxelTest)
            {
                if(
                    (userInterfaceManager.MouseClick != null) && 
                    (userInterfaceManager.MouseClick.Button == userInterfaceManager.ApplyButton)
                )
                {
                    userInterfaceManager.MouseClick = null;
                    var voxel = Services.Get<VoxelRenderer.VoxelEditor>();
                    if(voxel != null)
                    {
                        voxel.TryPut();
                    }
                }
            }

            if(
                (Configuration.selection == false) ||
                (userInterfaceManager == null)
            )
            {
                return;
            }

            if(
                (userInterfaceManager.HoverArea != null) &&
                (userInterfaceManager.HoverArea != userInterfaceManager.Layer)
            )
            {
                return;
            }

            if(
                (userInterfaceManager.MouseClick != null) && 
                (userInterfaceManager.MouseClick.Button == userInterfaceManager.SelectButton)
            )
            {
                userInterfaceManager.MouseClick = null;
                switch(userInterfaceManager.ButtonAction)
                {
                    case UserInterfaceManager.Action.Drag:
                    {
                        if(
                            Configuration.physics && 
                            (physicsDrag != null)
                        )
                        {
                            physicsDrag.Begin();
                        }
                        break;
                    }
                    case UserInterfaceManager.Action.Add:
                    {
                        operations.Insert();
                        break;
                    }
                    case UserInterfaceManager.Action.Select:
                    {
                        if(HoverModel != null)
                        {
                            if(Contains(hoverModel) == false)
                            {
                                if(
                                    (window.Keyboard[OpenTK.Input.Key.ShiftLeft] == false) && 
                                    (window.Keyboard[OpenTK.Input.Key.ShiftRight] == false)
                                )
                                {
                                    ClearSelection();
                                }
                                Add(hoverModel);
                            }
                            else
                            {
                                Remove(hoverModel);
                            }
                        }

                        if(RuntimeConfiguration.curveTool)
                        {
                            curveTool.MouseClick(HoverModel);
                        }

                        if(manipulatorManager != null)
                        {
                            manipulatorManager.PickAxis(HoverPosition);
                        }

                        break;
                    }
                }
            }
            userInterfaceManager.MouseClick = null;

            if(manipulatorManager == null)
            {
                return;
            }

            if(Configuration.physics)
            {
                if(userInterfaceManager.MouseButtons[(int)userInterfaceManager.MoveButton])
                {
                    physicsDrag.Update(px, py, renderer.Requested.Viewport);
                }
                else if(physicsDrag.Model != null)
                {
                    physicsDrag.End();
                }
            }

            if(models.Count > 0)
            {
                manipulatorManager.Manipulator.Active = true;
            }
            else
            {
                manipulatorManager.Manipulator.Active = false;
            }

            if(
                userInterfaceManager.MouseButtons[(int)userInterfaceManager.MoveButton] &&
                manipulatorManager.Manipulator.ActiveAxis != Axis.None
            )
            {
                // Testing world to clip matrix
                sceneManager.Camera.UpdateFrame();
                sceneManager.Camera.UpdateViewport(renderer.Requested.Viewport);

                manipulatorManager.Manipulator.UpdateAxisMove(
                    sceneManager.Camera.WorldToClip,
                    renderer.Requested.Viewport,
                    window.Mouse.X,
                    window.Height - 1 - window.Mouse.Y
                );
            }
            else
            {
                manipulatorManager.Manipulator.UpdateScale();
            }

        }
        private void RenderBoundingBox()
        {
            var     m         = Models.First();
            var     sphere    = m.Batch.BoundingSphere;
            Matrix4 model     = m.Frame.LocalToWorld.Matrix;
            Matrix4 translate = Matrix4.CreateTranslation(sphere.Center);
            Matrix4 scale     = Matrix4.CreateScale(sphere.Radius);

            boundingFrame.LocalToWorld.Set(model * translate * scale);
            renderer.SetFrame(boundingFrame);
            renderer.Requested.Mesh     = sphereMesh.GetMesh;
            renderer.Requested.Program  = renderer.Programs["ColorFill"];
            //renderer.Requested.MeshMode = MeshMode.EdgeLines;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.RenderCurrentDebug();
        }
        public void SetWideLines(bool enable)
        {
            if(
                (RenderStack.Graphics.Configuration.canUseGeometryShaders) &&
                (RenderStack.Graphics.Configuration.glslVersion >= 330)
            )
            {
                if(enable)
                {
                    GL.Enable(EnableCap.PolygonOffsetFill);
                }
                else
                {
                    GL.Disable(EnableCap.PolygonOffsetFill);
                }
            }
            else
            {
                if(enable)
                {
                    GL.LineWidth(renderer.Requested.Material.Floats("line_width").X);
                    GL.PointSize(renderer.Requested.Material.Floats("line_width").X);
                }
                else
                {
                    GL.LineWidth(1.0f);
                    GL.PointSize(1.0f);
                }
            }
        }
        public void RenderSelectionStencilMask()
        {
            renderer.Requested.Material = stencilMaskWrite;
            renderer.Requested.Program  = renderer.Requested.Material.Program;
            renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
            foreach(var model in Models)
            {
                renderer.SetFrame(model.Frame);
                renderer.Requested.Mesh = model.Batch.Mesh;
                renderer.RenderCurrent();
            }
        }
        private void RenderWideLines()
        {
            SetWideLines(true);
            renderer.RenderCurrent();
            if(RenderStack.Graphics.Configuration.useGl1)
            {
                renderer.Requested.MeshMode = MeshMode.CornerPoints;
                renderer.RenderCurrent();
            }
        }
        public void Render()
        {
            if(Models.Count == 0)
            {
                return;
            }

            //  Write stencil mask to selection
            RenderSelectionStencilMask();

            if(RuntimeConfiguration.selectionBoundingVolume/* && (hoverModel != null)*/)
            {
                RenderBoundingBox();
            }

            foreach(var model in Models)
            {
                renderer.SetFrame(model.Frame);
                renderer.Requested.Mesh = model.Batch.Mesh;

                if(renderer.Requested.Mesh.HasIndexBufferRange(MeshMode.EdgeLines) == false)
                {
                    continue;
                }

                if(RenderStack.Graphics.Configuration.useGl1 == false)
                {
                    GL.Enable(EnableCap.SampleAlphaToCoverage);
                    GL.Enable(EnableCap.SampleAlphaToOne);
                }

                if(RuntimeConfiguration.selectionSilhouette)
                {
                    //  Parts inside/intersecting other geometry
                    //  Do not render on top of selected object using stencil test

                    renderer.Requested.Material = silhouetteHidden;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
                    RenderWideLines();

                    //  Parts not intersecting/inside anything
                    renderer.Requested.Material = silhouetteVisible;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
                    RenderWideLines();
                }

                if(RuntimeConfiguration.selectionWireframeHidden)
                {
                    GL.PolygonOffset(-3.0f, 1.0f);
                    GL.Enable(EnableCap.PolygonOffsetFill);

                    renderer.Requested.Material = edgeLinesHidden;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
                    RenderWideLines();

                    GL.Disable(EnableCap.PolygonOffsetFill);
                }

                if(RuntimeConfiguration.selectionWireframeEdges)
                {
                    GL.PolygonOffset(-3.0f, 1.0f);
                    GL.Enable(EnableCap.PolygonOffsetFill);

                    renderer.Requested.Material = edgeLinesSelected;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
                    RenderWideLines();

                    GL.Disable(EnableCap.PolygonOffsetFill);
                }

                if(RuntimeConfiguration.selectionWireframePoints)
                {
                    GL.PointSize(2.0f);
                    renderer.Requested.Material = cornerPoints;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    renderer.Requested.MeshMode = renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
                    renderer.RenderCurrent();
                }

                if(RuntimeConfiguration.selectionWireframeCentroids)
                {
                    GL.PointSize(2.0f);
                    renderer.Requested.Material = polygonCentroids;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    renderer.Requested.MeshMode = renderer.Requested.MeshMode = renderer.Requested.Material.MeshMode;
                    renderer.RenderCurrent();
                }

                if(RenderStack.Graphics.Configuration.useGl1 == false)
                {
                    GL.Disable(EnableCap.SampleAlphaToCoverage);
                    GL.Disable(EnableCap.SampleAlphaToOne);
                }

                SetWideLines(false);
            }
        }

    }
}

