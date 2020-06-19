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
    public class Bender : IUpdateOncePerFrame // using once per frame update for visual effect
    {
        private Frame   frame;
        private Matrix4 rotationMatrix;
        private Vector3 positionInParent;
        private Vector3 axis;
        private float   maxAngle;
        private float   cycleTime;

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

        public Bender(Vector3 axis, float maxAngle, float cycleTime)
        {
            this.axis = axis;
            this.maxAngle = maxAngle;
            this.cycleTime = cycleTime;
        }
        public void UpdateFixedStep()
        {
        }
        public void UpdateOncePerFrame()
        {
            if(frame == null)
            {
                return;
            }
            float phase = Time.Now / cycleTime;
            float angle = maxAngle * (float)System.Math.Sin(phase);
            Matrix4.CreateRotation(angle, axis, out rotationMatrix);

            Matrix4 localToParent = rotationMatrix;
            Matrix4 parentToLocal;

            Matrix4.Transpose(localToParent, out parentToLocal);


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
