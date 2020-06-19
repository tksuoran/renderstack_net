#if USE_JITTER_PHYSICS

using Jitter.LinearMath;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class PointOnPoint : Constraint
    {
        //private Vector3 anchor;
        Jitter.Dynamics.Constraints.SingleBody.PointOnPoint pointOnPoint;

        internal override Jitter.Dynamics.Constraints.Constraint constraint { get { return pointOnPoint; } }

        public PointOnPoint(RigidBody rigidBody, Vector3 position)
        {
            pointOnPoint = new Jitter.Dynamics.Constraints.SingleBody.PointOnPoint(
                rigidBody.rigidBody, 
                new JVector(position.X, position.Y, position.Z)
            );
            pointOnPoint.Softness = 0.1f;
        }

        public Vector3 Anchor { get { JVector v = pointOnPoint.Anchor; return new Vector3(v.X, v.Y, v.Z); } set { pointOnPoint.Anchor = new JVector(value.X, value.Y, value.Z); } }
    }
}

#endif