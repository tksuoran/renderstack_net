#region Using Statements
using System;
using System.Collections.Generic;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
#endregion

using RenderStack.Math;

namespace Jitter.Collision.Shapes
{
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
            this.UpdateShape();
        }

        public EllipsoidShape(float a, float b, float c)
        {
            this.axis = new JVector(a, b, c);
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            Mass = (float)(4.0 * System.Math.PI  * axis.X * axis.Y * axis.Z / 3.0);
            JMatrix inertia = JMatrix.Identity;

            float a = axis.X * axis.X;
            float b = axis.Y * axis.Y;
            float c = axis.Z * axis.Z;

            inertia.M11 = (b + c) / 5.0f;
            inertia.M22 = (c + a) / 5.0f;
            inertia.M33 = (a + b) / 5.0f;
            this.Inertia = inertia;
        }

        public override void SupportMapping(ref JVector direction, out JVector result)
        {
            float length = 1.0f / (float)Math.Sqrt(
                axis.X * axis.X * direction.X * direction.X +
                axis.Y * axis.Y * direction.Y * direction.Y +
                axis.Z * axis.Z * direction.Z * direction.Z
            );

            result.X = direction.X * axis.X * axis.X * length;
            result.Y = direction.Y * axis.Y * axis.Y * length;
            result.Z = direction.Z * axis.Z * axis.Z * length;
        }
    }
}
