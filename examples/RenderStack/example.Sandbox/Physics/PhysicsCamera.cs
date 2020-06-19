using RenderStack.Scene;
using RenderStack.Physics;

using example.Renderer;

namespace example.Sandbox
{
    public class PhysicsCamera : IPhysicsObject
    {
        public Camera       Camera          { get; set; }
        public Shape        PhysicsShape    { get; set; } 
        public RigidBody    RigidBody       { get; set; }
        public bool         Static          { get; set; }
        public bool         ShadowCaster    { get { return false; } }
        public bool         UsePosition     { get { return true; } }
        public bool         UseRotation     { get { return false; } }
        public Frame        Frame           { get { return Camera.Frame; } }
        public string       Name            { get { return Camera.Name; } }

        public PhysicsCamera(Camera camera)
        {
            Camera = camera;

            // If shape is sphere it will roll and friction won't work
            PhysicsShape = new SphereShape(1.0f);
            //PhysicsShape = new CapsuleShape(1.0f, 0.5f);
            //PhysicsShape = new BoxShape(0.5f, 2.0f, 0.5f);

            RigidBody = new RigidBody(PhysicsShape);

            // Initially objects are unmovable and non-active
            //@object.RigidBody.Mass                     *= 5.0f;
            RigidBody.Material.Restitution      = 0.00f;
            RigidBody.Material.StaticFriction   = 0.01f;
            RigidBody.Material.DynamicFriction  = 0.20f;
            RigidBody.RestrictRotations         = true;
            RigidBody.AllowDeactivation         = true;
            RigidBody.IsActive                  = true;
            RigidBody.IsStatic                  = false;
            RigidBody.Tag                       = this;
            RigidBody.Mass                  *= 0.1f;

            Static = false;
        }
    }
}
