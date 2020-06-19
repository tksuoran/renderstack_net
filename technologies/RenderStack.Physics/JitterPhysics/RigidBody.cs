#if USE_JITTER_PHYSICS

using Jitter.LinearMath;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class RigidBody
    {
        internal Jitter.Dynamics.RigidBody  rigidBody;
        private Material                    material;
        private bool                        restrictRotations;
        private float                       mass;
        private JMatrix                     inertia;
        private Shape                       shape;

        public bool     RestrictRotations
        {
            get
            {
                return restrictRotations;
            }
            set
            {
                if(restrictRotations != value)
                {
                    restrictRotations = value;
                    UpdateMass(mass);
                }
            }
        }
        public Vector3  Position            { get { return Util.FromJitter(rigidBody.Position);            } set { rigidBody.Position        = Util.ToJitter(value); } }
        public Vector3  LinearVelocity      { get { return Util.FromJitter(rigidBody.LinearVelocity);      } set { rigidBody.LinearVelocity  = Util.ToJitter(value); } }
        public Vector3  AngularVelocity     { get { return Util.FromJitter(rigidBody.AngularVelocity);     } set { rigidBody.AngularVelocity = Util.ToJitter(value); } }
        public Matrix4  InverseInertiaWorld { get { return Util.FromJitter(rigidBody.InverseInertiaWorld); } }
        public Matrix4  Orientation         { get { return Util.FromJitter(rigidBody.Orientation);         } set { rigidBody.Orientation     = Util.ToJitter(value); } }
        public bool     IsActive            { get { return rigidBody.IsActive;          } set { rigidBody.IsActive = value; } }
        public bool     IsStatic            { get { return rigidBody.IsStatic;          } set { rigidBody.IsStatic = value; } }
        public bool     AffectedByGravity   { get { return rigidBody.AffectedByGravity; } set { rigidBody.AffectedByGravity = value; } }
        public bool     AllowDeactivation   { get { return rigidBody.AllowDeactivation; } set { rigidBody.AllowDeactivation = value; } }
        public object   Tag                 { get { return rigidBody.Tag;               } set { rigidBody.Tag = value; } }
        public float    Mass                { get { return mass; /*rigidBody.Mass;*/ } set { UpdateMass(value); } }
        public Material Material            { get { return material; } }
        public Shape    Shape               { get { return shape; } }

        public void UpdateMass(float mass)
        {
            this.mass = mass;
            if(restrictRotations == true)
            {
                rigidBody.SetMassProperties(JMatrix.Zero, 1.0f / mass, true);
            }
            else
            {
                rigidBody.SetMassProperties(inertia, mass, false);
            }
        }

        public RigidBody(Shape shape)
        {
            this.shape = shape;
            rigidBody = new Jitter.Dynamics.RigidBody(shape.shape);
            material = new Material(rigidBody.Material);
            inertia = rigidBody.Inertia;
            UpdateMass(rigidBody.Mass);
        }

        public void AddForce(Vector3 force)
        {
            rigidBody.AddForce(Util.ToJitter(force));
        }
        public void AddTorque(Vector3 torque)
        {
            rigidBody.AddTorque(Util.ToJitter(torque));
        }
        public void ApplyImpulse(Vector3 impulse)
        {
            rigidBody.ApplyImpulse(Util.ToJitter(impulse));
        }
    }
}

#endif