using System;
using System.Collections.Generic;
using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;
using RenderStack.Physics;
using example.Brushes;
using example.Renderer;

namespace example.Sandbox
{
    public partial class SceneManager : Service, ISceneManager
    {
        #region service
        public override string Name
        {
            get { return "SceneManager"; }
        }

        Brushes.BrushManager    brushManager;
        MaterialManager         materialManager;
        IRenderer               renderer;
        SelectionManager        selectionManager;
        Sounds                  sounds;
        TextRenderer            textRenderer;
        UserInterfaceManager    userInterfaceManager;

        public void Connect(
            BrushManager            brushManager,
            MaterialManager         materialManager,
            IRenderer               renderer,
            SelectionManager        selectionManager,
            Sounds                  sounds,
            TextRenderer            textRenderer,
            UserInterfaceManager    userInterfaceManager,
            OpenTK.GameWindow       window
        )
        {
            this.brushManager           = brushManager;
            this.materialManager        = materialManager;
            this.renderer               = renderer;
            this.selectionManager       = selectionManager;
            this.sounds                 = sounds;
            this.textRenderer           = textRenderer;
            this.userInterfaceManager   = userInterfaceManager;
            this.window                 = window;

            InitializationDependsOn(brushManager, materialManager, renderer);

            //debugLineRenderer.Initialize();
        }

        private void Message(string message)
        {
#if false
            System.Diagnostics.Trace.WriteLine(message);
            if(textRenderer != null && textRenderer.IsInitialized)
            {
                textRenderer.Message(message);
            }
#endif
        }
        protected override void InitializeService()
        {
            //float size = 15.0f;

            if(Configuration.physics)
            {
                Message("SceneManager: InitializePhysics...");
                InitializePhysics();
            }
        }

        public void Reset()
        {
            Clear();

            var curveTool = Services.Get<CurveTool.CurveTool>();
            if(curveTool != null)
            {
                curveTool.Enabled = false;
            }
            RenderGroups.Add(renderGroup);
            shadowCasterGroups.Add(shadowCasterGroup);
            IdGroups.Add(renderGroup);
        }
        #endregion
        #region data members
        private float               floorSize = 0.0f;

        private OpenTK.GameWindow   window;

        private Camera              camera;
        private Group               renderGroup         = new Group("renderGroup");
        private Group               shadowCasterGroup   = new Group("shadowCasterGroup");

        private Group               reflectedGroup      = new Group("reflectedGroup");
        private Group               aoReceivers         = new Group("aoReceivers");
        private Group               aoOccluders         = new Group("aoOccluders");

        private Model               floorModel;
        //private LineRenderer        debugLineRenderer   = new LineRenderer();

        private List<Group>         renderGroups        = new List<Group>();
        private List<Group>         shadowCasterGroups  = new List<Group>();
        private List<Group>         idGroups            = new List<Group>();

        private PhysicsCamera       physicsCamera;
        public Camera               Camera              { get { return camera; } }
        public PhysicsCamera        PhysicsCamera       { get { return physicsCamera; } }
        //public Camera[]             ShadowCamera        { get { return shadowCamera; } }
        public Group                RenderGroup         { get { return renderGroup; } }
        public Group                ReflectedGroup      { get { return reflectedGroup; } }
        public Group                AoReceivers         { get { return aoReceivers; } }
        public Group                AoOccluders         { get { return aoOccluders; } }
        public Model                FloorModel          { get { return floorModel; } }
        public Model                ReflectionModel;

        public List<Group>          RenderGroups        { get { return renderGroups; } }
        public List<Group>          IdGroups            { get { return idGroups; } }
        public List<Group>          ShadowCasterGroups  { get { return shadowCasterGroups; } }

//        public LineRenderer         DebugLineRenderer   { get { return debugLineRenderer; } }
        public Frame                DebugFrame;

        public bool                 UpdateShadowMap = true;
        #endregion data members

        struct ModelInfo
        {
            public Brushes.Brush    brush;
            public float            x;
            public float            z;
        }
        #region Model Activate, Deactivate, Clear, RemoveModel, AddModel, UnparentModel
        public void ActivateModel(Model model)
        {
            if(Configuration.physics)
            {
                model.RigidBody.IsActive = true;
                if(model != FloorModel)
                {
                    model.RigidBody.IsStatic = false;
                }
                model.Frame.Updated = false;
            }
        }
        public void DeactivateModel(Model model)
        {
            if(Configuration.physics)
            {
                model.RigidBody.IsActive = false;
                model.RigidBody.IsStatic = true;
            }
        }
        public void Clear()
        {
            //System.Console.WriteLine("\n------ SceneManager.Clear() -------------------------------------------------");

            if(Configuration.physics)
            {
                ResetPhysics();
            }
            updateOncePerFrame.Clear();
            updateFixedStep.Clear();
            renderGroup.Clear();
            renderGroups.Clear();
            reflectedGroup.Clear();
            aoReceivers.Clear();
            aoOccluders.Clear();
            shadowCasterGroup.Clear();

            renderGroups.Clear();
            shadowCasterGroups.Clear();
            IdGroups.Clear();

            floorModel = null;
            UpdateShadowMap = true;
        }
        public void RemoveModel(Model model)
        {
            if(Configuration.physics)
            {
                RemoveModelPhysics(model);
            }
            renderGroup.Remove(model);
            reflectedGroup.Remove(model);
            aoReceivers.Remove(model);
            aoOccluders.Remove(model);
            if(shadowCasterGroup.Models.Contains(model))
            {
                shadowCasterGroup.Remove(model);
                UpdateShadowMap = true;
            }
            /*if(model == floorModel)
            {
                floorModel = null;
            }*/
        }
        public Model AddModel(Model model)
        {
            return AddModel(model, true, true);
        }
        public Model AddModel(Model model, bool physics)
        {
            return AddModel(model, true, physics);
        }
        public Model AddModel(Model model, bool shadowCaster, bool physics)
        {
            if(model == null)
            {
                throw new System.ArgumentNullException();
            }
            if(Configuration.physics && physics)
            {
                AddModelPhysics(model);
            }

            if(model.Batch.Material == null)
            {
                model.Batch.Material = materialManager.Materials["Schlick"];
            }

            renderGroup.Add(model);
            if(shadowCaster)
            {
                shadowCasterGroup.Add(model);
                UpdateShadowMap = true;
            }
            reflectedGroup.Add(model);
            aoReceivers.Add(model);
            aoOccluders.Add(model);

            return model;
        }
        public Model AddModel(Model model, Shape shape)
        {
            model.PhysicsShape = shape;
            model.Static = shape == null;
            AddModel(model, shape != null);
            if(Configuration.physics && shape != null)
            {
                model.RigidBody.IsStatic = false;
            }
            return model;
        }
        public void UnparentModel(Model model)
        {
            NextUpdateSerial();
            model.Frame.UpdateHierarchical(UpdateSerial);
            Matrix4 localToWorld = model.Frame.LocalToWorld.Matrix;
            model.Frame.Parent = null;
            model.Frame.LocalToParent.Set(localToWorld);
            model.Frame.Updated = false;
        }
        #endregion Model operations
        #region Add stuff
        private void AddHemisphericalObject()
        {
            /*Model hemisphericalModel = Model.Create(
                "Hemispherical View",
                cube4,
                texturedMaterial,
                Matrix4.CreateTranslation(  0.0f,   3.0f,   0.0f)
            );
            hemisphericalModel.Parameters.Add<Texture>("texture").Value = defaultTexture;
            renderGroup.Models.Add(hemisphericalModel);*/
        }

#if false
        public void MakeSuperEllipsoidScene()
        {
            ResetModels();
            AddUserControlledCamera();

            float scale = 1.0f;
            //sceneManager.AddFloor(22.0f * scale, 0, -1.0f);
            AddFloor(30.0f * scale, 5, -0.5f);

            float gap = 10.0f;

            Material magenta = materialManager["magenta"];
            for(double n1 = 0.5; n1 <= 2.0; n1 += 0.5)
            //double n1 = 0.25;
            {
                for(double n2 = 0.5; n2 <= 2.0; n2 += 0.5)
                //double n2 = 0.25;
                {
                    GeometryMesh mesh = new GeometryMesh(
                        new RenderStack.Geometry.Shapes.SuperEllipsoid(
                            new Vector3(1.0f, 1.0f, 1.0f), n1, n2, 8 * 4, 8 * 4
                        ),
                        NormalStyle.CornerNormals
                    );
                    Shape shape = new SuperEllipsoidShape(1.0f, 1.0f, 1.0f, (float)n1, (float)n2);
                    AddModel(
                        new Model(
                            "SuperEllipsoid(" + n1 +", " + n2 + ")",
                            mesh,
                            magenta,
                            -10.0f + (float)(n1 * gap),
                            0.5f,
                            -10.0f + (float)(n2 * gap)
                        ), 
                        shape
                    );
                }
            }
        }
#endif
        private void AddReflectionTestObjects()
        {
            GeometryMesh cube4 = new GeometryMesh(new Cube(2.0f), NormalStyle.PointNormals);
            Geometry newGeometry = new CatmullClarkGeometryOperation(cube4.Geometry).Destination;
            newGeometry = new CatmullClarkGeometryOperation(newGeometry).Destination;
            newGeometry.ComputePolygonCentroids();
            newGeometry.ComputePolygonNormals();
            //newGeometry.ComputeCornerNormals(0.0f);
            newGeometry.SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)Math.PI));
            newGeometry.BuildEdges();

            float a = 4.0f;

            GeometryMesh cube4s = new GeometryMesh(newGeometry, NormalStyle.PointNormals);
            AddModel(new Model("PosX", cube4, materialManager["CubeMaterials[0]"],    a,   0.0f,   0.0f));
            AddModel(new Model("NegX", cube4, materialManager["CubeMaterials[1]"],   -a,   0.0f,   0.0f));
            AddModel(new Model("PosY", cube4, materialManager["CubeMaterials[2]"], 0.0f,      a,   0.0f));
            AddModel(new Model("NegY", cube4, materialManager["CubeMaterials[3]"], 0.0f,     -a,   0.0f));
            AddModel(new Model("PosZ", cube4, materialManager["CubeMaterials[4]"], 0.0f,   0.0f,      a));
            AddModel(new Model("NegZ", cube4, materialManager["CubeMaterials[5]"], 0.0f,   0.0f,     -a));
        }
        #endregion Add stuff
        #region cameras
        public Vector3 Home = new Vector3(0.0f, 1.5f, 10.0f);

        private IFrameController    cameraControls;
        public  IFrameController    CameraControls  { get { return cameraControls; } }
        public void AddCameraUserControls()
        {
            if(Configuration.physicsCamera)
            {
                physicsCamera = new PhysicsCamera(camera); 
                AddPhysicsObject(physicsCamera);
                MovePhysicsObject(physicsCamera, physicsCamera.Frame.LocalToWorld.Matrix);
                physicsCamera.RigidBody.AffectedByGravity = userInterfaceManager.FlyMode ? false : true;
                cameraControls = new PhysicsFpsFrameController();
                (cameraControls as PhysicsFpsFrameController).PhysicsObject = physicsCamera;
            }
            else
            {
                if(Configuration.lockUpVector)
                {
                    cameraControls = new HeadingElevationFrameController();
                }
                else
                {
                    cameraControls = new FrameController();
                }
                cameraControls.Frame = camera.Frame;
            }
            userInterfaceManager.ConnectUserControls();
        }
        private void    AddCameras()
        {
            AddCameras(1.0f);
        }
        private void    AddCameras(float scale)
        {
            if(floorSize == 0.0f)
            {
                floorSize = 30.0f;
                //throw new System.InvalidOperationException("You need to add floor before you add cameras");
            }
            camera = new Camera(); 
            camera.Name = "camera";

            camera.Projection.FovYRadians      = RenderStack.Math.Conversions.DegreesToRadians(50.0f);
            camera.Projection.ProjectionType   = ProjectionType.PerspectiveVertical;
            camera.Projection.Near             = 0.1f;
            camera.Projection.Far              = 100.0f;

            if(Configuration.stereo == false)
            {
                Home.Z *= 1.5f;
                Home.Y *= 2.5f;
            }

            if(RuntimeConfiguration.gameTest)
            {
                Home.Y += 5.0f;
            }

            Home *= scale;

            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    Home,
                    Vector3.Zero,
                    Vector3.UnitY
                )
            );

        }

        #endregion

        ///  Returns world coordinates under mouse
        public Vector3 MouseToWorld(float depth, int x, int y)
        {
            int px = x;
            int py = window.Height - 1 - y;

            Vector3 positionInWorld = camera.WorldToClip.InverseMatrix.UnProject(
                (float)px, 
                (float)py, 
                depth, 
                renderer.Requested.Viewport.X,
                renderer.Requested.Viewport.Y,
                renderer.Requested.Viewport.Width,
                renderer.Requested.Viewport.Height
            );

            return positionInWorld;
        }

    }
}
