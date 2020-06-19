//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;
using System.Linq;

using RenderStack.Graphics;
using RenderStack.Geometry;
using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.Mesh;
using RenderStack.Physics;

using example.Renderer;

namespace example.Sandbox
{
    // TODO Move to PhysicsManager?
    /*  Comment: Highly experimental  */ 
    public partial class SceneManager
    {
        public World World { private set; get; }

        private List<IPhysicsObject>    physicsObjects = new List<IPhysicsObject>();
        public List<IPhysicsObject>     PhysicsObjects { get { return physicsObjects; } }

        public void ResetPhysics()
        {
            World.Clear();
            physicsObjects.Clear();
        }

        public void InitializePhysics()
        {
#if false // \todo fix
            if(Configuration.sounds)
            {
                collision.CollisionDetected += new CollisionDetectedHandler(collision_CollisionDetected);
            }
#endif
            World = new World(); 

            //World.SetIterations(12, 5);
        }

#if false
        float lastCollisionTime = 0.0f;
        float soundThreshold = 1.0f;
        void collision_CollisionDetected(RigidBody body1, RigidBody body2, JVector point1, JVector point2, JVector normal, float penetration)
        {
            if(Configuration.sounds)
            {
                float now           = Time.Now;
                float timeEpsilon   = now - lastCollisionTime;

                float speed = body1.LinearVelocity.LengthSquared() + body2.LinearVelocity.LengthSquared();
                if(speed > 10.0f)
                {
                    soundThreshold *= 2.0f;
                    lastCollisionTime = now;
                    sounds.Queue(sounds.Collision);
                }
            }
        }
#endif

        public void AddPhysicsObject(IPhysicsObject @object)
        {
            @object.Frame.UpdateHierarchical(updateSerial);

            if(@object.RigidBody == null)
            {
                Shape initialShape = @object.PhysicsShape;
                if(@object.PhysicsShape == null)
                {
                    throw new System.ArgumentException("Physics object has no Shape");
                }

                @object.RigidBody = new RigidBody(@object.PhysicsShape);

                MovePhysicsObject(@object, @object.Frame.LocalToWorld.Matrix);

                // Initially objects are unmovable and non-active
                //@object.RigidBody.Mass                     *= 5.0f;
                @object.RigidBody.Material.Restitution     = 0.00f;
                @object.RigidBody.Material.StaticFriction  = 0.80f;
                @object.RigidBody.Material.DynamicFriction = 0.70f;
                @object.RigidBody.AllowDeactivation        = true;
                @object.RigidBody.IsActive                 = false;
                @object.RigidBody.IsStatic                 = true;
                @object.RigidBody.Tag = @object;
                @object.RigidBody.IsActive = true;

                if(@object.UseRotation == false)
                {
                    @object.RigidBody.RestrictRotations = true;
                }
            }

            World.AddBody(@object.RigidBody);

            physicsObjects.Add(@object);
        }
        public void AddModelPhysics(Model model)
        {
            model.Frame.UpdateHierarchical(updateSerial);

            Shape initialShape = model.PhysicsShape;
            if(model.PhysicsShape == null)
            {
                MakeModelPhysicsConvexHull(model);
            }

            model.RigidBody = new RigidBody(model.PhysicsShape);

            MovePhysicsObject(model, model.Frame.LocalToWorld.Matrix);

            // Initially objects are unmovable and non-active
            model.RigidBody.Mass                     *= 2.0f;
            model.RigidBody.Material.Restitution     = 0.00f;
            model.RigidBody.Material.StaticFriction  = 0.80f;
            model.RigidBody.Material.DynamicFriction = 0.70f;
            model.RigidBody.AllowDeactivation        = true;
            model.RigidBody.IsActive                 = false;
            model.RigidBody.IsStatic                 = true;
            model.RigidBody.Tag = model;

            if(model.Static == false)
            {
                model.RigidBody.IsActive = true;
                model.RigidBody.IsStatic = false;
            }

            World.AddBody(model.RigidBody);

            physicsObjects.Add(model);
        }
        public void MakeModelPhysicsConvexHull(Model model)
        {
            GeometryMesh    mesh = model.Batch.MeshSource as GeometryMesh;
            Geometry        geometry = mesh.Geometry;
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
            ConvexHullShape shape = new ConvexHullShape(positions);
            model.PhysicsShape = shape;
            /*System.Diagnostics.Trace.TraceInformation(
                "ConvexHull" 
                + " Shift = " + shape.Shift.ToString()
                + " BoundingBox = " + box.ToString()
            );*/
        }
#if false // \todo fix
        public void MakeModelPhysicsGeneric(Model model)
        {
            model.Frame.UpdateHierarchical();

            model.UpdateOctree();

            var shape = new TriangleMeshShape(model.Octree);
            model.PhysicsShape = shape;
        }
#endif
        public void RemoveModelPhysics(Model model)
        {
            physicsObjects.Remove(model);
            if(
                (model.RigidBody != null)
            )
            {
                World.RemoveBody(model.RigidBody);
                model.RigidBody = null;
            }
        }
        public void UpdateModelPhysics(Model model)
        {
            if(model.RigidBody != null)
            {
                RemoveModelPhysics(model);
            }
            AddModelPhysics(model);
        }
        public void MakeRigidBodiesForModels()
        {
            foreach(Model model in RenderGroup.Models)
            {
                AddModelPhysics(model);
            }
        }
        public void MovePhysicsObject(IPhysicsObject @object, Matrix4 matrix)
        {
            if(@object.ShadowCaster)
            {
                UpdateShadowMap = true;
            }

            if(@object.RigidBody == null)
            {
                return;
            }

            Vector3 position = matrix.TransformPoint(Vector3.Zero);
            @object.RigidBody.Position    = position;
            @object.RigidBody.Orientation = matrix;
        }

        // .X = requested
        // .Y = effective
        private Floats restitution      = new Floats(0.5f, 0.5f);
        private Floats staticFriction   = new Floats(0.5f, 0.5f);
        private Floats dynamicFriction  = new Floats(0.5f, 0.5f);
        private Floats gravity          = new Floats(-9.8f, -9.8f);
        private Floats angularDamping   = new Floats(0.85f, 0.85f);
        private Floats linearDamping    = new Floats(0.85f, 0.85f);

        public Floats Restitution       { get { return restitution; } }
        public Floats StaticFriction    { get { return staticFriction; } }
        public Floats DynamicFriction   { get { return dynamicFriction; } }
        public Floats Gravity           { get { return gravity; } }
        public Floats AngularDamping    { get { return angularDamping; } }
        public Floats LinearDamping     { get { return linearDamping; } }

        public void RefreshParameters()
        {
            if(
                (restitution.X != restitution.Y) ||
                (staticFriction.X != staticFriction.Y) ||
                (dynamicFriction.X != dynamicFriction.Y)
            )
            {
                //  This would need that we update our values based on selection
#if false
                if(selectionManager.Models.Count > 0)
                {
                    foreach(var model in selectionManager.Models)
                    {
                        if(model.RigidBody != null)
                        {
                            model.RigidBody.Restitution = Restitution.X;
                            model.RigidBody.StaticFriction = StaticFriction.X;
                            model.RigidBody.DynamicFriction = DynamicFriction.X;
                        }
                    }
                }
                else
#endif
                {
                    foreach(RigidBody body in World.RigidBodies)
                    {
                        body.Material.Restitution = Restitution.X;
                        body.Material.StaticFriction = StaticFriction.X;
                        body.Material.DynamicFriction = DynamicFriction.X;
                    }
                }
                Restitution.Y = Restitution.X;
                StaticFriction.Y = StaticFriction.X;
                DynamicFriction.Y = DynamicFriction.X;
            }

            if(Gravity.X != Gravity.Y)
            {
                Vector3 g = World.Gravity;
                g.Y = Gravity.X;
                World.Gravity = g;
                Gravity.Y = Gravity.X;
            }
            if(
                (AngularDamping.X != AngularDamping.Y) ||
                (LinearDamping.X != LinearDamping.Y)
            )
            {
                World.SetDampingFactors(angularDamping.X, linearDamping.X);
                AngularDamping.Y = AngularDamping.X;
                LinearDamping.Y = LinearDamping.X;
            }
        }

        public void UpdatePhysics()
        {
            if(World == null)
            {
                return;
            }

#if false // \todo fix
            if(soundThreshold > 1.0f)
            {
                soundThreshold *= 0.99f;
            }
#endif

            RefreshParameters();

            float   step        = 1.0f / 120.0f;
            bool    multithread = true;

            World.Step(step, multithread);
        }

        public void FetchPhysics()
        {
            foreach(IPhysicsObject @object in physicsObjects)
            {
                if(@object.RigidBody == null)
                {
                    continue;
                }

                if(@object.RigidBody.IsActive == false)
                {
                    continue;
                }

                if(@object.ShadowCaster)
                {
                    UpdateShadowMap = true;
                }

                Matrix4 newLocalToWorld = @object.Frame.LocalToWorld.Matrix;
                Matrix4 newWorldToLocal = @object.Frame.LocalToWorld.InverseMatrix;

                // TODO Add proper handling for objects going outside sane world
                if(@object.Static == false)
                {
                    if(@object.RigidBody.Position.Y < -5.0f)
                    {
                        @object.RigidBody.Position = new Vector3(
                            0.0f,
                            20.0f,
                            0.0f
                        );
                        @object.RigidBody.LinearVelocity *= 0.2f;
                        @object.RigidBody.AngularVelocity *= 0.1f;
                    }
                }

                if(@object.UseRotation)
                {
                    newLocalToWorld._00 = @object.RigidBody.Orientation._00;
                    newLocalToWorld._01 = @object.RigidBody.Orientation._01;
                    newLocalToWorld._02 = @object.RigidBody.Orientation._02;
                    newLocalToWorld._10 = @object.RigidBody.Orientation._10;
                    newLocalToWorld._11 = @object.RigidBody.Orientation._11;
                    newLocalToWorld._12 = @object.RigidBody.Orientation._12;
                    newLocalToWorld._20 = @object.RigidBody.Orientation._20;
                    newLocalToWorld._21 = @object.RigidBody.Orientation._21;
                    newLocalToWorld._22 = @object.RigidBody.Orientation._22;
                }

                if(@object.UsePosition)
                {
                    newLocalToWorld._03 = @object.RigidBody.Position.X;
                    newLocalToWorld._13 = @object.RigidBody.Position.Y;
                    newLocalToWorld._23 = @object.RigidBody.Position.Z;
                }

                if(@object.Frame.Parent != null)
                {
                    Matrix4 oldWorldToParent = @object.Frame.Parent.LocalToWorld.InverseMatrix;
                    Matrix4 newLocalToParent = oldWorldToParent * newLocalToWorld;
                    @object.Frame.LocalToParent.Set(newLocalToParent);
                }
                else
                {
                    @object.Frame.LocalToParent.Set(newLocalToWorld);
                }
            }
        }
    }
}
