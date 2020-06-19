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

    /// <summary>
    /// A <see cref="Shape"/> representing a superellipsoid.
    /// </summary>
    public class SuperEllipsoidShape : Shape
    {
        private double a1;
        private double a2;
        private double a3;
        private double e1;
        private double e2;

        public double A1 { get { return a1; } set { a1 = value; UpdateShape(); } }
        public double A2 { get { return a2; } set { a2 = value; UpdateShape(); } }
        public double A3 { get { return a3; } set { a3 = value; UpdateShape(); } }
        public double E1 { get { return e1; } set { e1 = value; UpdateShape(); } }
        public double E2 { get { return e2; } set { e2 = value; UpdateShape(); } }

        public SuperEllipsoidShape(float rx, float ry, float rz, float e1, float e2)
        {
            this.a1 = rx;
            this.a2 = ry;
            this.a3 = rz;
            this.e1 = e1;
            this.e2 = e2;
            UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            JMatrix inertia = JMatrix.Identity;

            inertia.M11 = (float)(
                0.5 * a1 * a2 * a3 * e1 * e2 * (
                    a2 * a2 * MathHelper.Beta(1.5 * e2, 0.5 * e2) * MathHelper.Beta(0.5 * e1, 2.0 * e1 + 1.0) +
                    4.0 * a3 * a3 * MathHelper.Beta(0.5 * e2, 0.5 * e2 + 1) * MathHelper.Beta(1.5 * e1, e1 + 1.0)
                )
            );
            inertia.M22 = (float)(
                0.5 * a1 * a2 * a3 * e1 * e2 * (
                    a1 * a1 * MathHelper.Beta(1.5 * e2, 0.5 * e2) * MathHelper.Beta(0.5 * e1, 2.0 * e1 + 1.0) + 
                    4.0 * a3 * a3 * MathHelper.Beta(0.5 * e2, 0.5 * e2 + 1) * MathHelper.Beta(1.5 * e1, e1 + 1.0)
                )
            );
            inertia.M33 = (float)(
                0.5 * a1 * a2 * a3 * e1 * e2 * (a1 * a1 + a2 * a2) * MathHelper.Beta(1.5 * e2, 0.5 * e2) * MathHelper.Beta(0.5 * e1, 2.0 * e1 + 1.0)
            );
            this.Inertia = inertia;
        }

        private double PowAbs(double x, double p)
        {
            return System.Math.Pow(System.Math.Abs(x), p);
        }

        private double InsideOutside(double x, double y, double z)
        {
            double p1 = 2.0 / e1;
            double p2 = 2.0 / e2;
            double x0 = PowAbs(x / a1, p2);
            double y0 = PowAbs(y / a2, p2);
            double z0 = PowAbs(z / a3, p1);
            return PowAbs(x0 + y0, e2 / e1) + z0;
        }

        /// <summary>
        /// SupportMapping. Finds the point in the shape furthest away from the given direction.
        /// Imagine a plane with a normal in the search direction. Now move the plane along the normal
        /// until the plane does not intersect the shape. The last intersection point is the result.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="result">The result.</param>
        public override void SupportMapping(ref JVector direction, out JVector result)
        {
            JVector d = direction;
            double F = InsideOutside(direction.X, direction.Y, direction.Z);
            if(F == 0.0)
            {
                result.X = 0.0f;
                result.Y = 0.0f;
                result.Z = 0.0f;
                return;
            }
            //double beta = SPow(1.0 / F, e1 / 2.0);
            double beta = PowAbs(1.0 / F, e1 / 2.0);
            result.X = (float)(direction.X * beta);
            result.Y = (float)(direction.Y * beta);
            result.Z = (float)(direction.Z * beta);
            // F(x,y,z) = pow(beta, -2/e1)  <=> beta = pow(F(x,y,z), e1/-2)
            // y = x^(1/b)  <=>  y = x^(1/b)       if x > 0 and y > 0
        }
    }
}


#if false
            System.Math.Log(1.0, 2.0);
            double r = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

            if(r > 0.0f)
            {
                double theta    = Math.Acos((double)direction.Z / r);
                double phi      = Math.Atan2((double)direction.Y, (double)direction.X);

                double sinPhi   = System.Math.Sin(phi);
                double cosPhi   = System.Math.Cos(phi);

                double sinTheta = System.Math.Sin(theta);
                double cosTheta = System.Math.Cos(theta);

                double ctn1     = System.Math.Sign(cosTheta) * System.Math.Pow(System.Math.Abs(cosTheta), n1);
                double xP       = ctn1 * System.Math.Sign(cosPhi) * System.Math.Pow(System.Math.Abs(cosPhi), n2);
                double yP       = ctn1 * System.Math.Sign(sinPhi) * System.Math.Pow(System.Math.Abs(sinPhi), n2);
                double zP       = System.Math.Sign(sinTheta) * System.Math.Pow(System.Math.Abs(sinTheta), n1);

                result.X = (float)xP;
                result.Y = (float)yP;
                result.Z = (float)zP;
            }
            else
            {
                result.X = 0.0f;
                result.Y = 0.0f;
                result.Z = 0.0f;
            }
#endif
