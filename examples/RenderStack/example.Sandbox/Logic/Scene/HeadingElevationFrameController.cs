//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;

using RenderStack.Math;
using RenderStack.Scene;

namespace example.Sandbox
{
    [Serializable]
    /*  Comment: Mostly stable  */ 
    public class HeadingElevationFrameController : IFrameController
    {
        private Frame       frame;
        private float       elevation;
        private float       heading;
        private Matrix4     headingMatrix;
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
            /*
            rotationMatrix._03 = 0.0f;
            rotationMatrix._13 = 0.0f;
            rotationMatrix._23 = 0.0f;
            rotationMatrix._33 = 1.0f;
            */
            float bank;
            rotationMatrix.ToHPB(out heading, out elevation, out bank);
            Update();
        }

        public  Controller  RotateX         { get { return rotateX; } }
        public  Controller  RotateY         { get { return rotateY; } }
        public  Controller  RotateZ         { get { return rotateZ; } }
        public  Controller  TranslateX      { get { return translateX; } }
        public  Controller  TranslateY      { get { return translateY; } }
        public  Controller  TranslateZ      { get { return translateZ; } }
        public  Controller  SpeedModifier   { get { return speedModifier; } }

        public HeadingElevationFrameController()
        {
            TranslateX.Clear();
            TranslateY.Clear();
            TranslateZ.Clear();
            RotateX.Clear();
            RotateY.Clear();
            RotateZ.Clear();
#if false // 100 fps
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
#else // 120 fps
            RotateX.Damp        = 0.700f;
            RotateY.Damp        = 0.700f;
            RotateZ.Damp        = 0.700f;
            RotateX.MaxDelta    = 0.02f;
            RotateY.MaxDelta    = 0.02f;
            RotateZ.MaxDelta    = 0.02f;
            TranslateX.Damp     = 0.92f;
            TranslateY.Damp     = 0.92f;
            TranslateZ.Damp     = 0.92f;
            TranslateX.MaxDelta = 0.004f;
            TranslateY.MaxDelta = 0.004f;
            TranslateZ.MaxDelta = 0.004f;
            SpeedModifier.MaxValue  = 3.0f;
            SpeedModifier.Damp      = 0.92f;
            SpeedModifier.MaxDelta  = 0.5f;
#endif
            elevation = 0.0f;
            heading = 0.0f;
            Update();
            /*Matrix4 elevationMatrix = Matrix4.CreateRotation(elevation, Vector3.UnitX);
            headingMatrix = Matrix4.CreateRotation(heading, Vector3.UnitY);
            rotationMatrix = headingMatrix * elevationMatrix;*/
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
  
        public void Update()
        {
            Matrix4 elevationMatrix = Matrix4.CreateRotation(elevation, Vector3.UnitX);
            headingMatrix = Matrix4.CreateRotation(heading, Vector3.UnitY);
            rotationMatrix = headingMatrix * elevationMatrix;

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

            Frame.LocalToParent.Set(localToParent, parentToLocal);
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
            SpeedModifier.Update();

            float speed = 1.0f + speedModifier.CurrentValue;

            if(TranslateX.CurrentValue != 0.0f)
            {
                /*  Right axis is column 0  */ 
                positionInParent += headingMatrix.GetColumn3(0) * TranslateX.CurrentValue * speed;
            }
            if(TranslateY.CurrentValue != 0.0f)
            {
                /*  Up axis is column 1  */ 
                positionInParent += headingMatrix.GetColumn3(1) * TranslateY.CurrentValue * speed;
            }
            if(translateZ.CurrentValue != 0.0f)
            {
                /*  View axis is column 2  */ 
                positionInParent += headingMatrix.GetColumn3(2) * TranslateZ.CurrentValue * speed;
            }
            if(
                (RotateX.CurrentValue != 0.0f) ||
                (RotateY.CurrentValue != 0.0f) 
            )
            {
                heading += RotateY.CurrentValue;
                elevation += RotateX.CurrentValue;
                Matrix4 elevationMatrix = Matrix4.CreateRotation(elevation, Vector3.UnitX);
                headingMatrix = Matrix4.CreateRotation(heading, Vector3.UnitY);
                rotationMatrix = headingMatrix * elevationMatrix;
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

            Frame.LocalToParent.Set(localToParent, parentToLocal);
        }

    }
}
