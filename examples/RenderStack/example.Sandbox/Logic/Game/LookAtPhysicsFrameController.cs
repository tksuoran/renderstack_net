using System;
using RenderStack.Math;
using RenderStack.Scene;
using example.Renderer;

namespace example.Sandbox
{
    [Serializable]
    /*  Comment: Mostly stable  */ 
    public class LookAtPhysicsFrameController : IFrameController, IPhysicsController
    {
        private IPhysicsObject  physicsObject;

        private Controller      rotateX         = new Controller();
        private Controller      rotateY         = new Controller();
        private Controller      rotateZ         = new Controller();
        private Controller      translateX      = new Controller();
        private Controller      translateY      = new Controller();
        private Controller      translateZ      = new Controller();
        private Controller      speedModifier   = new Controller();

        public IPhysicsObject PhysicsObject
        {
            get
            {
                return physicsObject;
            }
            set
            {
                if(value != physicsObject)
                {
                    physicsObject = value;
                    SetTransform(Frame.LocalToParent.Matrix);
                }
            }
        }
        public  Frame Frame 
        {
            get
            {
                return (physicsObject != null) ? physicsObject.Frame : null;
            }
            set
            {
                throw new System.InvalidOperationException();
                //frame = value;
                //SetTransform(frame.LocalToParent.Matrix);
            }
        }

        public void SetTransform(Matrix4 transform)
        {
        }

        public  Controller  RotateX         { get { return rotateX; } }
        public  Controller  RotateY         { get { return rotateY; } }
        public  Controller  RotateZ         { get { return rotateZ; } }
        public  Controller  TranslateX      { get { return translateX; } }
        public  Controller  TranslateY      { get { return translateY; } }
        public  Controller  TranslateZ      { get { return translateZ; } }
        public  Controller  SpeedModifier   { get { return speedModifier; } }

        public Vector3 Target = Vector3.Zero;

        public LookAtPhysicsFrameController()
        {
            TranslateX.Clear();
            TranslateY.Clear();
            TranslateZ.Clear();
            RotateX.Clear();
            RotateY.Clear();
            RotateZ.Clear();
            RotateX.Damp        = 0.840f;
            RotateY.Damp        = 0.840f;
            RotateZ.Damp        = 0.840f;
            RotateX.MaxDelta    = 0.002f;
            RotateY.MaxDelta    = 0.002f;
            RotateZ.MaxDelta    = 0.001f;
            TranslateX.Damp     = 0.950f;
            TranslateY.Damp     = 0.950f;
            TranslateZ.Damp     = 0.980f;
            TranslateX.MaxDelta = 0.020f;
            TranslateY.MaxDelta = 0.005f;
            TranslateZ.MaxDelta = 0.020f;
        }

        public void Clear()
        {
            TranslateX.Clear();
            TranslateY.Clear();
            TranslateZ.Clear();
            RotateX.Clear();
            RotateY.Clear();
            RotateZ.Clear();
        }

        public void UpdateOncePerFrame()
        {
        }

#if false
        private void OrientationCorrectionTorque(Matrix4 wantedOrientation, float scale)
        {
#if false
            // this is something like correction = wantedPosition - position
            JMatrix q = JMatrix.Inverse(wantedOrientation) * testBody.Orientation;
            JVector axis;

            float x = q.M32 - q.M23;
            float y = q.M13 - q.M31;
            float z = q.M21 - q.M12;

            float r = JMath.Sqrt(x * x + y * y + z * z);
            float t = q.M11 + q.M22 + q.M33;

            float angle = (float)Math.Atan2(r, t - 1);
            axis = new JVector(x, y, z) * angle;

            if (r != 0.0f) axis = axis * (1.0f / r);
            
            // 80.0f is the spring value "k"
            testBody.AddTorque(JVector.Transform(axis,JMatrix.Inverse(testBody.InverseInertiaWorld))*80.0f);
            
            // also apply some damping
            testBody.AngularVelocity *= 0.9f;
#endif
            Matrix4 q = Matrix4.Invert(wantedOrientation) * physicsObject.RigidBody.Orientation;
            float x = q._12 - q._21;
            float y = q._20 - q._02;
            float z = q._01 - q._10;

            float r = (float)Math.Sqrt(x * x + y * y + z * z);
            float t = q._00 + q._11 + q._22;

            float angle = (float)Math.Atan2(r, t - 1);
            Vector3 axis = new Vector3(x, y, z) * angle;
            if(r != 0.0f) axis *= (1.0f / r);

            Matrix4 inertiaWorld = Matrix4.Invert(physicsObject.RigidBody.InverseInertiaWorld);
            inertiaWorld = Matrix4.Transpose(inertiaWorld);
            axis = inertiaWorld.TransformDirection(axis);
            physicsObject.RigidBody.AddTorque(axis * scale);
            physicsObject.RigidBody.AngularVelocity *= 0.9f;
        }
#endif
        public void UpdateFixedStep()
        {
            TranslateX.Update();
            TranslateY.Update();
            TranslateZ.Update();
            RotateX.Update();
            RotateY.Update();
            RotateZ.Update();

            Matrix4 localToWorld = Matrix4.Identity;
            localToWorld._00 = physicsObject.RigidBody.Orientation._00;
            localToWorld._01 = physicsObject.RigidBody.Orientation._01;
            localToWorld._02 = physicsObject.RigidBody.Orientation._02;
            localToWorld._10 = physicsObject.RigidBody.Orientation._10;
            localToWorld._11 = physicsObject.RigidBody.Orientation._11;
            localToWorld._12 = physicsObject.RigidBody.Orientation._12;
            localToWorld._20 = physicsObject.RigidBody.Orientation._20;
            localToWorld._21 = physicsObject.RigidBody.Orientation._21;
            localToWorld._22 = physicsObject.RigidBody.Orientation._22;

            //  We can't use friction because sphere shape will roll,
            //  and it won't do air resistance either
            float damp = 0.98f;
            float dampY = Services.Get<UserInterfaceManager>().FlyMode ? damp : 1.0f;
            physicsObject.RigidBody.LinearVelocity = new Vector3(
                physicsObject.RigidBody.LinearVelocity.X * damp,
                physicsObject.RigidBody.LinearVelocity.Y * dampY,
                physicsObject.RigidBody.LinearVelocity.Z * damp
            );

            float scale = 2.0f;

            // N : Z  back
            // T : Y  up
            // B : X  right
            Vector3 back  = localToWorld.GetColumn3(2);
            Vector3 up0 = Vector3.UnitY;
            if(up0 == back)
            {
                up0 = back.MinAxis;
            }

            Vector3 right   = Vector3.Normalize(Vector3.Cross(up0, back));
            Vector3 up      = Vector3.Cross(back, right);

            //physicsObject.RigidBody.LinearVelocity += new Vector3(0.0f, 0.02f, 0.0f);
            if(TranslateX.CurrentValue != 0.0f)
            {
                physicsObject.RigidBody.LinearVelocity += right * TranslateX.CurrentValue;
            }
            if(TranslateY.CurrentValue != 0.0f)
            {
                physicsObject.RigidBody.LinearVelocity += scale * Vector3.UnitY * TranslateY.CurrentValue;
            }
            if(translateZ.CurrentValue != 0.0f)
            {
                physicsObject.RigidBody.LinearVelocity += back * TranslateZ.CurrentValue;
            }

            //  Compute desired orientation
            Matrix4 lookAt = Matrix4.CreateLookAt(physicsObject.RigidBody.Position, Target, Vector3.UnitY);

            //  Required rotation to get to desired orientation
            Matrix4 q  = physicsObject.RigidBody.Orientation * Matrix4.Transpose(lookAt); 

            //  Convert to axis angle
            Vector3 axis;
            float   angle;
            q.ToAxisAngle(out axis, out angle);

            //  Convert to torque
            float k = 1.0f / (1.0f + angle / (float)System.Math.PI);
            Matrix4 inertiaWorld    = Matrix4.Invert(physicsObject.RigidBody.InverseInertiaWorld);
            Vector3 torque          = inertiaWorld.TransformDirection(axis);
            Vector3 torqueScaled    = torque * 400.0f * k * k;
            physicsObject.RigidBody.AddTorque(torqueScaled);

            //  Also apply some damping
            physicsObject.RigidBody.AngularVelocity *= 0.9f;

            physicsObject.RigidBody.IsActive = true;

        }
    }
}
