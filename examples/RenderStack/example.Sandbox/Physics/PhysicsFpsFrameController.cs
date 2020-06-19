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
    public class PhysicsFpsFrameController : IFrameController, IPhysicsController
    {
        private IPhysicsObject  physicsObject;
        //private Frame           frame;
        private float           elevation;
        private float           heading;
        private Matrix4         headingMatrix;
        private Matrix4         rotationMatrix;

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
            rotationMatrix = transform;
            //positionInParent = rotationMatrix.GetColumn3(3);

            /*  Remove translation  */ 
            /*  TODO should really decompose instead..  */ 
            rotationMatrix._03 = 0.0f;
            rotationMatrix._13 = 0.0f;
            rotationMatrix._23 = 0.0f;
            rotationMatrix._33 = 1.0f;
        }

        public  Controller  RotateX         { get { return rotateX; } }
        public  Controller  RotateY         { get { return rotateY; } }
        public  Controller  RotateZ         { get { return rotateZ; } }
        public  Controller  TranslateX      { get { return translateX; } }
        public  Controller  TranslateY      { get { return translateY; } }
        public  Controller  TranslateZ      { get { return translateZ; } }
        public  Controller  SpeedModifier   { get { return speedModifier; } }

        public PhysicsFpsFrameController()
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

            elevation = 0.0f;
            heading = 0.0f;

            UpdateTransform();
        }

        public void UpdateTransform()
        {
            if(physicsObject == null)
            {
                return;
            }
            //physicsObject.Frame.UpdateHierarchical();

            Matrix4 elevationMatrix = Matrix4.CreateRotation(elevation, Vector3.UnitX);
            headingMatrix = Matrix4.CreateRotation(heading, Vector3.UnitY);
            rotationMatrix = headingMatrix * elevationMatrix;

            /*
            physicsObject.RigidBody.IsActive = true;
            physicsObject.RigidBody.Orientation = new Jitter.LinearMath.JMatrix(
                rotationMatrix._00, rotationMatrix._10, rotationMatrix._20,
                rotationMatrix._01, rotationMatrix._11, rotationMatrix._21,
                rotationMatrix._02, rotationMatrix._12, rotationMatrix._22
            );
            */

            Matrix4 newLocalToWorld = physicsObject.Frame.LocalToWorld.Matrix;
            Matrix4 newWorldToLocal = physicsObject.Frame.LocalToWorld.InverseMatrix;
            newLocalToWorld._00 = rotationMatrix._00; newLocalToWorld._01 = rotationMatrix._01; newLocalToWorld._02 = rotationMatrix._02;
            newLocalToWorld._10 = rotationMatrix._10; newLocalToWorld._11 = rotationMatrix._11; newLocalToWorld._12 = rotationMatrix._12;
            newLocalToWorld._20 = rotationMatrix._20; newLocalToWorld._21 = rotationMatrix._21; newLocalToWorld._22 = rotationMatrix._22;

            if(physicsObject.Frame.Parent != null)
            {
                Matrix4 oldWorldToParent = physicsObject.Frame.Parent.LocalToWorld.InverseMatrix;
                Matrix4 newLocalToParent = oldWorldToParent * newLocalToWorld;
                physicsObject.Frame.LocalToParent.Set(newLocalToParent);
            }
            else
            {
                physicsObject.Frame.LocalToParent.Set(newLocalToWorld);
            }

            //  Also update local to world properly
            physicsObject.Frame.UpdateHierarchicalNoCache();
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

        public void UpdateFixedStep()
        {
        }

        public void UpdateOncePerFrame()
        {
            if(physicsObject == null)
            {
                return;
            }
            TranslateX.Update();
            TranslateY.Update();
            TranslateZ.Update();
            RotateX.Update();
            RotateY.Update();
            RotateZ.Update();

            //  We can't use friction because sphere shape will roll,
            //  and it won't do air resistance either
            float damp = 0.88f;
            float dampY = Services.Get<UserInterfaceManager>().FlyMode ? damp : 1.0f;
            physicsObject.RigidBody.LinearVelocity = new Vector3(
                physicsObject.RigidBody.LinearVelocity.X * damp,
                physicsObject.RigidBody.LinearVelocity.Y * dampY,
                physicsObject.RigidBody.LinearVelocity.Z * damp
            );

            float scale = 2.0f;

            if(TranslateX.CurrentValue != 0.0f)
            {
                /*  Right axis is column 0  */ 
                //positionInParent += headingMatrix.GetColumn3(0) * TranslateX.CurrentValue;
                physicsObject.RigidBody.IsActive = true;
                physicsObject.RigidBody.LinearVelocity += scale * headingMatrix.GetColumn3(0) * TranslateX.CurrentValue;
            }
            if(TranslateY.CurrentValue != 0.0f)
            {
                /*  Up axis is column 1  */ 
                //positionInParent += headingMatrix.GetColumn3(1) * TranslateY.CurrentValue;
                physicsObject.RigidBody.IsActive = true;
                physicsObject.RigidBody.LinearVelocity += scale * headingMatrix.GetColumn3(1) * TranslateY.CurrentValue;
            }
            if(translateZ.CurrentValue != 0.0f)
            {
                /*  View axis is column 2  */ 
                //positionInParent += headingMatrix.GetColumn3(2) * TranslateZ.CurrentValue;
                physicsObject.RigidBody.IsActive = true;
                physicsObject.RigidBody.LinearVelocity += scale * headingMatrix.GetColumn3(2) * TranslateZ.CurrentValue;
            }
            if(
                (RotateX.CurrentValue != 0.0f) ||
                (RotateY.CurrentValue != 0.0f) 
            )
            {
                heading += RotateY.CurrentValue;
                elevation += RotateX.CurrentValue;

                UpdateTransform();

                //Services.Instance.TextRenderer.DebugLine("Heading : " + heading + " Elevation : " + elevation);
                //Services.Instance.TextRenderer.DebugLine("newLocalToWorld : " + newLocalToWorld);
                //Services.Instance.TextRenderer.DebugLine("rotationMatrix : " + rotationMatrix);
            }
        }

        public void RenderUpdate()
        {
            //  Physics will move the object
        }

    }
}
