#if false
using System;

using Jitter;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Jitter.Dynamics.Joints;

public class EllipsoidShape : Shape
{
    private JVector axis;

    public JVector Axis
    {
        get { return axis; }
        set { axis = value; base.UpdateShape(); }
    }

    public EllipsoidShape(JVector axis)
    {
        this.axis = axis;
        this.UpdateShape();  // !!
    }

    public EllipsoidShape(float a, float b, float c)
    {
        this.axis = new JVector(a, b, c);
        this.UpdateShape();  // !!
    }

    public override void SupportMapping(ref JVector direction, out JVector result)
    {
        float length = 1.0f / (float)Math.Sqrt(axis.X * axis.X * direction.X * direction.X +
                                     axis.Y * axis.Y * direction.Y * direction.Y +
                                     axis.Z * axis.Z * direction.Z * direction.Z);

        result.X = direction.X * axis.X * axis.X * length;
        result.Y = direction.Y * axis.Y * axis.Y * length;
        result.Z = direction.Z * axis.Z * axis.Z * length;
    }

    protected override float CalculateMassInertia(
        out JMatrix inertia, 
        out JVector center
    )
    {
        center = JVector.Zero;
        float mass = 4.18879f * this.axis.X * this.axis.Y * this.axis.Z;
        inertia = new JMatrix();
        inertia.M11 = 0.2f * mass * (axis.Y * axis.Y + axis.Z * axis.Z);
        inertia.M22 = 0.2f * mass * (axis.X * axis.X + axis.Z * axis.Z);
        inertia.M33 = 0.2f * mass * (axis.X * axis.X + axis.Y * axis.Y);
        return mass;
    }
}
#endif