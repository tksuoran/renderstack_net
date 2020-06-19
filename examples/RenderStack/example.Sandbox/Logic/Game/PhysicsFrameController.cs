//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;

using RenderStack.Math;
using RenderStack.Scene;

using example.Renderer;

namespace example.Sandbox
{
    [Serializable]
    /*  Comment: Mostly stable  */ 
    public class PhysicsFrameController : IFrameController, IPhysicsController
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

        public PhysicsFrameController()
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
            TranslateX.Damp     = 0.900f;
            TranslateY.Damp     = 0.900f;
            TranslateZ.Damp     = 0.900f;
            TranslateX.MaxDelta = 0.020f;
            TranslateY.MaxDelta = 0.020f;
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
            back.Y = 0.0f;
            back = Vector3.Normalize(back);
            Vector3 up0 = Vector3.UnitY;
            if(up0 == back)
            {
                up0 = back.MinAxis;
            }

            Vector3 right   = Vector3.Normalize(Vector3.Cross(up0, back));
            Vector3 up      = Vector3.Cross(back, right);


            physicsObject.RigidBody.IsActive = true;
            //physicsObject.RigidBody.LinearVelocity += new Vector3(0.0f, 0.02f, 0.0f);
            if(TranslateX.CurrentValue != 0.0f)
            {
                /*  Right axis is column 0  */ 
                physicsObject.RigidBody.IsActive = true;
                //physicsObject.RigidBody.LinearVelocity += scale * localToWorld.GetColumn3(0) * TranslateX.CurrentValue;
                physicsObject.RigidBody.LinearVelocity += scale * right * TranslateX.CurrentValue;
            }
            if(TranslateY.CurrentValue != 0.0f)
            {
                /*  Up axis is column 1  */ 
                physicsObject.RigidBody.IsActive = true;
                //physicsObject.RigidBody.LinearVelocity += scale * localToWorld.GetColumn3(1) * TranslateY.CurrentValue;
                physicsObject.RigidBody.LinearVelocity += scale * Vector3.UnitY * TranslateY.CurrentValue;
            }
            if(translateZ.CurrentValue != 0.0f)
            {
                /*  Back axis is column 2  */ 
                physicsObject.RigidBody.IsActive = true;
                physicsObject.RigidBody.LinearVelocity += scale * back/*localToWorld.GetColumn3(2)*/ * TranslateZ.CurrentValue;
            }
            if(
                (RotateX.CurrentValue != 0.0f) ||
                (RotateY.CurrentValue != 0.0f) 
            )
            {
                physicsObject.RigidBody.IsActive = true;
                float v = RotateY.CurrentValue;
                float a = (float)System.Math.Abs(v);
                float s = (float)System.Math.Sign(v);
                float r = s * (float)System.Math.Pow(a, 0.2f);
                //float r = RotateY.CurrentValue;
                physicsObject.RigidBody.AngularVelocity += new Vector3(0.0f, r, 0.0f);
                physicsObject.RigidBody.AngularVelocity *= 0.94f;

                //Services.Instance.TextRenderer.DebugLine("Heading : " + heading + " Elevation : " + elevation);
                //Services.Instance.TextRenderer.DebugLine("newLocalToWorld : " + newLocalToWorld);
                //Services.Instance.TextRenderer.DebugLine("rotationMatrix : " + rotationMatrix);
            }
            physicsObject.RigidBody.IsActive = true;
        }
    }
}
