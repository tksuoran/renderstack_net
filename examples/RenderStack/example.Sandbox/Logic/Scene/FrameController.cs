﻿using System;

using RenderStack.Math;
using RenderStack.Scene;

namespace example.Sandbox
{
    [Serializable]
    /*  Comment: Mostly stable  */ 
    public class FrameController : IFrameController
    {
        private Frame       frame;
        private Matrix4     rotationMatrix;
        private Vector3     positionInParent;
        private Controller  rotateX      = new Controller();
        private Controller  rotateY      = new Controller();
        private Controller  rotateZ      = new Controller();
        private Controller  translateX   = new Controller();
        private Controller  translateY   = new Controller();
        private Controller  translateZ   = new Controller();
        private Controller  speedModifier = new Controller();

        public  Frame Frame 
        {
            get
            {
                return frame;
            }
            set
            {
                if(value == frame)
                {
                    return;
                }

                frame = value;
                SetTransform(frame.LocalToParent.Matrix);
            }
        }

        public void SetTransform(Matrix4 transform)
        {
            rotationMatrix = transform;
            positionInParent = rotationMatrix.GetColumn3(3);

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

        public FrameController()
        {
            TranslateX.Clear();
            TranslateY.Clear();
            TranslateZ.Clear();
            RotateX.Clear();
            RotateY.Clear();
            RotateZ.Clear();
#if false // 100 fps updates
            RotateX.Damp        = 0.950f;
            RotateY.Damp        = 0.950f;
            RotateZ.Damp        = 0.950f;
            RotateX.MaxDelta    = 0.002f;
            RotateY.MaxDelta    = 0.002f;
            RotateZ.MaxDelta    = 0.001f;
            TranslateX.Damp     = 0.950f;
            TranslateY.Damp     = 0.950f;
            TranslateZ.Damp     = 0.950f;
            TranslateX.MaxDelta = 0.003f;
            TranslateY.MaxDelta = 0.003f;
            TranslateZ.MaxDelta = 0.003f;
#else
            RotateX.Damp        = 0.950f;
            RotateY.Damp        = 0.950f;
            RotateZ.Damp        = 0.950f;
            RotateX.MaxDelta    = 0.003f;
            RotateY.MaxDelta    = 0.003f;
            RotateZ.MaxDelta    = 0.002f;
            TranslateX.Damp     = 0.950f;
            TranslateY.Damp     = 0.950f;
            TranslateZ.Damp     = 0.950f;
            TranslateX.MaxDelta = 0.010f;
            TranslateY.MaxDelta = 0.010f;
            TranslateZ.MaxDelta = 0.010f;
#endif
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
            //  Since this is not updated with physics
        }
        public void UpdateFixedStep()
        {
            TranslateX.Update();
            TranslateY.Update();
            TranslateZ.Update();
            RotateX.Update();
            RotateY.Update();
            RotateZ.Update();

            if(TranslateX.CurrentValue != 0.0f)
            {
                /*  Right axis is column 0  */ 
                positionInParent += rotationMatrix.GetColumn3(0) * TranslateX.CurrentValue;
            }
            if(TranslateY.CurrentValue != 0.0f)
            {
                /*  Up axis is column 1  */ 
                positionInParent += rotationMatrix.GetColumn3(1) * TranslateY.CurrentValue;
            }
            if(translateZ.CurrentValue != 0.0f)
            {
                /*  View axis is column 2  */ 
                positionInParent += rotationMatrix.GetColumn3(2) * TranslateZ.CurrentValue;
            }
            if(RotateX.CurrentValue != 0.0f)
            {
                Matrix4 rotation = Matrix4.CreateRotation(RotateX.CurrentValue, rotationMatrix.GetColumn3(0));
                rotationMatrix = rotation * rotationMatrix;
            }
            if(RotateY.CurrentValue != 0.0f)
            {
                Matrix4 rotation = Matrix4.CreateRotation(RotateY.CurrentValue, rotationMatrix.GetColumn3(1));
                rotationMatrix = rotation * rotationMatrix;
            }
            if(RotateZ.CurrentValue != 0.0f)
            {
                Matrix4 rotation = Matrix4.CreateRotation(RotateZ.CurrentValue, rotationMatrix.GetColumn3(2));
                rotationMatrix = Matrix4.CreateRotation(RotateZ.CurrentValue, rotationMatrix.GetColumn3(2)) * rotationMatrix;
            }
            if(frame == null)
            {
                return;
            }

            Matrix4 localToParent = rotationMatrix;
            Matrix4 parentToLocal;

            Matrix4.Transpose(localToParent, out parentToLocal);

            // HACK
            if(positionInParent.Y < 0.03f)
            {
                positionInParent.Y = 0.03f;
            }

            /*  Put translation to column 3  */ 
            localToParent._03 = positionInParent.X;
            localToParent._13 = positionInParent.Y;
            localToParent._23 = positionInParent.Z;
            localToParent._33 = 1.0f;

            /*  Put inverse translation to column 3 */ 
            parentToLocal._03 = parentToLocal._00 * -positionInParent.X + parentToLocal._01 * -positionInParent.Y + parentToLocal._02 * - positionInParent.Z;
            parentToLocal._13 = parentToLocal._10 * -positionInParent.X + parentToLocal._11 * -positionInParent.Y + parentToLocal._12 * - positionInParent.Z;
            parentToLocal._23 = parentToLocal._20 * -positionInParent.X + parentToLocal._21 * -positionInParent.Y + parentToLocal._22 * - positionInParent.Z;
            parentToLocal._33 = 1.0f;

            //rame.LocalToParent.Set(localToParent);
            Frame.LocalToParent.Set(localToParent, parentToLocal);
        }
        public void RenderUpdate()
        {
        }

    }
}
