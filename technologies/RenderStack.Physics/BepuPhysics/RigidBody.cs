#if USE_BEPU_PHYSICS

using BEPUphysics.MathExtensions;
using XVector3 = Microsoft.Xna.Framework.Vector3;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class Helper
    {
        public static void Check(Matrix3X3 m)
        {
            if(
                float.IsNaN(m.M11) ||
                float.IsNaN(m.M12) ||
                float.IsNaN(m.M13) ||
                float.IsNaN(m.M21) ||
                float.IsNaN(m.M22) ||
                float.IsNaN(m.M23) ||
                float.IsNaN(m.M31) ||
                float.IsNaN(m.M32) ||
                float.IsNaN(m.M33)
            )
            {
                throw new System.ArithmeticException();
            }
        }
    }
    public class RigidBody
    {
        internal BEPUphysics.Entities.Entity entity;

        private float       mass;
        private Shape       shape;
        private Material    material;

        public Vector3      Position        { get { XVector3 p = entity.Position; return new Vector3(p.X, p.Y, p.Z); } set { entity.Position = new XVector3(value.X, value.Y, value.Z); /*Check();*/ } }
        public Vector3      LinearVelocity  { get { XVector3 v = entity.LinearVelocity; return new Vector3(v.X, v.Y, v.Z); } set { entity.LinearVelocity = new XVector3(value.X, value.Y, value.Z); /*Check();*/ } }
        public Vector3      AngularVelocity  { get { XVector3 v = entity.AngularVelocity; return new Vector3(v.X, v.Y, v.Z); } set { entity.AngularVelocity = new XVector3(value.X, value.Y, value.Z); /*Check();*/ } }
        public Matrix4      Orientation
        {
            get
            {
                Check();
                Matrix4 m = Matrix4.Identity;
                Matrix3X3 bm = entity.OrientationMatrix;
                m._00 = bm.M11; m._10 = bm.M12; m._20 = bm.M13;
                m._01 = bm.M21; m._11 = bm.M22; m._21 = bm.M23;
                m._02 = bm.M31; m._12 = bm.M32; m._22 = bm.M33;
                return m;
            }
            set
            {
                entity.OrientationMatrix = new Matrix3X3(
                    value._00, value._01, value._02,
                    value._10, value._11, value._12,
                    value._20, value._21, value._22
                );
                Check();
            }
        }

        public bool         IsActive            { get { return entity.IsActive; } set { entity.IsActive = value; /*Check();*/ } }
        public bool         IsStatic
        {
            get
            {
                return !entity.IsDynamic;
            }
            set
            {
                //Check();
                if(value)
                {
                    entity.BecomeKinematic();
                    //Check();
                }
                else
                {
                    UpdateMassProperties(mass);
                    //Check();
                }
            }
        }
        public bool     AffectedByGravity   { get { return entity.IsAffectedByGravity; } set { entity.IsAffectedByGravity = value; /*Check();*/ } }
        public bool     AllowDeactivation   { get { return !entity.IsAlwaysActive; } set { entity.IsAlwaysActive = !value; /*Check();*/ } }
        public object   Tag                 { get { return entity.Tag; } set { entity.Tag = value; /*Check();*/ } }
        public float Mass
        {
            get
            {
                return mass;
            }
            set
            {
                UpdateMassProperties(mass);
            }
        }
        private void UpdateMassProperties(float mass)
        {
            //Check();
            this.mass = mass;
            if(mass == 0.0f)
            {
                mass = 1.0f;
            }
            if (mass <= 0 || float.IsNaN(mass) || float.IsInfinity(mass))
            {
                entity.BecomeKinematic();
            }
            else
            {
                Matrix3X3 inertiaTensor;
                Matrix3X3.Multiply(
                    ref shape.massDistribution, 
                    mass * BEPUphysics.CollisionShapes.ConvexShapes.InertiaHelper.InertiaTensorScale, 
                    out inertiaTensor
                );
                entity.BecomeDynamic(mass, inertiaTensor);
            }
            //Check();
        }
        public Material Material            { get { return material; } }

        public void Check()
        {
            Helper.Check(entity.OrientationMatrix);
            Helper.Check(entity.InertiaTensor);
            Helper.Check(entity.InertiaTensorInverse);
            Helper.Check(entity.LocalInertiaTensor);
            Helper.Check(entity.LocalInertiaTensorInverse);
            entity.AngularMomentum.Check();
            entity.AngularVelocity.Check();
            entity.LinearMomentum.Check();
            entity.LinearVelocity.Check();
        }

        public RigidBody(Shape shape)
        {
            entity = new BEPUphysics.Entities.Entity(shape.shape);
            this.shape = shape;
            UpdateMassProperties(shape.Mass);

            //Helper.Check(shape.massDistribution);

            entity.BecomeDynamic(Mass, shape.massDistribution);
            
            //Check();

            material = new Material(entity.Material);
        }

        public void AddForce(Vector3 force)
        {
            //Check();
            //World world;
            //entity. AddForce(new JVector(force.X, force.Y, force.Z));
            float scale = 0.1f;
            entity.ApplyImpulse(XVector3.Zero, new XVector3(force.X * scale, force.Y * scale, force.Z * scale));
            //Check();
        }
        public bool RestrictRotations;
        public void ApplyImpulse(Vector3 impulse)
        {
            //Check();
            entity.ApplyImpulse(XVector3.Zero, new XVector3(impulse.X, impulse.Y, impulse.Z));
            //Check();
        }
    }
}

#endif