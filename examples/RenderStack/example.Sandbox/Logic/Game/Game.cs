using System;
using System.Collections.Generic;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;
using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;

using example.Renderer;

namespace example.Sandbox
{
    class Game : Service, IUpdateOncePerFrame, IUpdateFixedStep
    {
        public override string Name { get { return "Game"; } }

        HighLevelRenderer       highLevelRenderer;
        MaterialManager         materialManager;
        IRenderer               renderer;
        SceneManager            sceneManager;

        private SeekerAI        seeker = new SeekerAI();

        //  Suggested naming:
        //  Friendly or goodies: greek alphabet - alpha, beta, gamma..
        //  Baddies: elements: Hydrogen, Helium, 
        private UnitType        alpha;
        private UnitType        helium;

        private List<Unit>      units = new List<Unit>();
        private Unit            player;

        private ICameraUpdate   cameraUpdate;
        public Vector3          SpawnPosition;
        public Unit             Player { get { return player; } }

        public void Connect(
            HighLevelRenderer   highLevelRenderer,
            MaterialManager     materialManager,
            IRenderer           renderer,
            SceneManager        sceneManager
        )
        {
            this.highLevelRenderer = highLevelRenderer;
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;

            InitializationDependsOn(materialManager, renderer, sceneManager);
        }

        public static RenderStack.Physics.Shape MakeShape(Geometry geometry)
        {
            var             pointLocations = geometry.PointAttributes.Find<Vector3>("point_locations");
            List<Vector3>   positions = new List<Vector3>();
            BoundingBox     box = new BoundingBox();
            for(int i = 0; i < geometry.Points.Count; ++i)
            {
                Point point = geometry.Points[i];
                Vector3 p = pointLocations[point];
                positions.Add(p);
                box.ExtendBy(p);
            }
            return new RenderStack.Physics.ConvexHullShape(positions);
        }

        private System.Random random = new Random(1);

        protected override void  InitializeService()
        {
            SpawnPosition = new Vector3(0.0f, 2.0f, 0.0f);

            //  "alpha" is used as player ship
            {
                float sq3 = (float)Math.Sqrt(3.0f);

                Geometry gLow = new CustomTetrahedron(1.0f, sq3 / 2.0f, 2.0f);
                gLow.BuildEdges();
                Geometry g = new SubdivideGeometryOperation(gLow).Destination;


                //g.Transform(Matrix4.CreateScale(1.6f));
                //g = new CatmullClarkGeometryOperation(g).Destination;
                //g = new CatmullClarkGeometryOperation(g).Destination;
                var gm = new GeometryMesh(
                    g,
                    NormalStyle.PolygonNormals
                );
                var m = gm.GetMesh;
                alpha = new UnitType(
                    "Alpha", 
                    materialManager["magenta"],
                    m,
                    MakeShape(gLow),
                    g.ComputeBoundingSphere(),
                    10.0f,  // max health
                    1.0f,   // density
                    null,
                    //typeof(PhysicsFrameController)
                    typeof(LookAtPhysicsFrameController)
                );
            }

            {
                Geometry g = new Cuboctahedron(1.0f);
                helium = new UnitType(
                    "Helium",
                    materialManager["red"],
                    new GeometryMesh(g, NormalStyle.PolygonNormals).GetMesh,
                    MakeShape(g),
                    g.ComputeBoundingSphere(),
                    5.0f,
                    1.0f,
                    seeker as IAI,
                    typeof(LookAtPhysicsFrameController)
                );
            }
        }

        public void Reset()
        {
            player = Spawn(alpha, SpawnPosition.X, SpawnPosition.Z);
            //cameraUpdate = new FollowCamera(sceneManager.Camera, player);
            cameraUpdate = new AboveCamera(sceneManager.Camera, player);

            for(int i = 0; i < 2; ++i)
            {
                Spawn(helium, Random(10.0f), Random(10.0f));
            }
        }

        public float Random(float range)
        {
            return 2.0f * range * (float)random.NextDouble() - range;
        }

        public float YAt(float x, float z)
        {
            if(sceneManager.FloorModel == null)
            {
                return 0.0f;
            }
            Vector3 root = new Vector3(x, 1000.0f, z);
            Vector3 normal;
            Vector3 direction = -Vector3.UnitY;
            float fraction;
            bool ok = sceneManager.World.Raycast(
                sceneManager.FloorModel.RigidBody, 
                root, 
                direction, 
                out normal, 
                out fraction
            );
            Vector3 intersection = root + fraction * direction;
            return intersection.Y;
        }
        public Vector3 Place(float x, float z, UnitType type)
        {
            float y = YAt(x, z) + type.BoundingSphere.Radius;
            return new Vector3(x, y, z);
        }

        public Unit Spawn(UnitType type, float x, float z)
        {
            float y = YAt(x, z) + type.BoundingSphere.Radius;
            Vector3 position = new Vector3(x, y, z);
            var unit = new Unit(type, position);
            units.Add(unit);
            sceneManager.AddModel(unit.Model, true, true);
            //unit.Model.RigidBody.Mass                     *= type.Density;
            unit.Model.RigidBody.Material.Restitution     = 0.99f;
            unit.Model.RigidBody.Material.StaticFriction  = 0.99f;
            //unit.Model.RigidBody.Material.DynamicFriction = 0.70f;
            unit.Model.RigidBody.AllowDeactivation        = false;
            //unit.Controller = new LookAtPhysicsFrameController(unit.Model);
            if(type.ControllerType != null)
            {
                unit.Controller = (IFrameController)System.Activator.CreateInstance(type.ControllerType);
                IPhysicsController physicsController = unit.Controller as IPhysicsController;
                if(physicsController != null)
                {
                    physicsController.PhysicsObject = unit.Model;
                }
            }

            return unit;
        }

        public void UpdateOncePerFrame()
        {
            foreach(var unit in units)
            {
                unit.UpdateOncePerFrame();
            }
        }

        public void TargetMouseHoverPosition()
        {
            var controller = player.Controller as LookAtPhysicsFrameController;
            if(controller == null)
            {
                return;
            }

            Matrix4     clipToWorld = sceneManager.Camera.WorldToClip.InverseMatrix;
            Viewport    viewport = highLevelRenderer.WindowViewport;
            int px;
            int py;
            highLevelRenderer.MouseInScreen(out px, out py);

            Vector3 target;
#if true
            Vector3 root = clipToWorld.UnProject(px, py, 0.0f, viewport.X, viewport.Y, viewport.Width, viewport.Height);
            Vector3 tip  = clipToWorld.UnProject(px, py, 1.0f, viewport.X, viewport.Y, viewport.Width, viewport.Height);
            Vector3 direction = tip - root;

            //  Raycast
            Vector3                         normal;
            //RenderStack.Physics.RigidBody   body;
            float                           fraction;
            if(sceneManager.FloorModel == null)
            {
                return;
            }
            bool ok = sceneManager.World.Raycast(sceneManager.FloorModel.RigidBody, root, direction, out normal, out fraction);
            if(ok)
            {
                Vector3 terrain = root + fraction * direction;
                target = terrain + new Vector3(0.0f, 0.5f, 0.0f);
            }
            else
            {
                return;
            }
#else
            target = clipToWorld.UnProject(px, py, 0.5f, viewport.X, viewport.Y, viewport.Width, viewport.Height);
            target.Y = player.Model.RigidBody.Position.Y;
#endif
            Vector3 delta = target - player.Model.RigidBody.Position;
            if(delta.LengthSquared > 1.0f)
            {
                delta.Y = 0.0f;
                delta = Vector3.Normalize(delta);
                controller.Target = player.Model.RigidBody.Position + 5.0f * delta;
                //controller.Target = target;
            }
        }

        public void UpdateFixedStep()
        {
            cameraUpdate.Update();
            TargetMouseHoverPosition();
            foreach(var unit in units)
            {
                unit.UpdateFixedStep();
            }
        }
    }
}

