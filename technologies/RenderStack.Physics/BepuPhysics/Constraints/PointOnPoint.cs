//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_BEPU_PHYSICS

using RenderStack.Math;
using XVector3 = Microsoft.Xna.Framework.Vector3;
using BEPUphysicsDemos.SampleCode;

namespace RenderStack.Physics
{
    public class PointOnPoint : Constraint
    {
        //private Vector3 anchor;
        protected MotorizedGrabSpring grabber;

        internal override BEPUphysics.UpdateableSystems.Updateable updateable { get { return grabber; } }

        public PointOnPoint(RigidBody rigidBody, Vector3 position)
        {
            Vector3 positionInLocal = position + rigidBody.Position;
            grabber = new MotorizedGrabSpring();
            grabber.Setup(rigidBody.entity, new XVector3(positionInLocal.X, positionInLocal.Y, positionInLocal.Z));
            grabber.LocalOffset = new XVector3(position.X, position.Y, position.Z);
        }

        public Vector3 Anchor { get { XVector3 v = grabber.GoalPosition; return new Vector3(v.X, v.Y, v.Z); } set { grabber.GoalPosition = new XVector3(value.X, value.Y, value.Z); } }
    }
}

#endif