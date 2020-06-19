#if false
using System;
using System.Collections.Generic;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;

namespace Jitter.Collision.Shapes
{
    public class TruncatedConeShape : Shape
    {
        float height;
        float fullConeHeight;
        float R;
        float r;
        float x;
        public float Height { get { return height; } set { height = value; UpdateShape(); } }
        public float BottomRadius { get { return R; } set { R = value; UpdateShape(); } }
        public float TopRadius { get { return r; } set { r = value; UpdateShape(); } }
        public TruncatedConeShape(float height, float bottomRadius, float topRadius)
        {
            this.height = height;
            this.R = bottomRadius;
            this.r = topRadius;

            this.UpdateShape();
        }

        public override void UpdateShape()
        {
            float a = R;
            float b = r;
            float a2 = a * a;
            float b2 = b * b;
            float ab = a * b;
            float h = height;
            float div = 20.0f * (float)System.Math.PI * (ab + a2 + b2);
            float div2 = 10.0f * (float)System.Math.PI * (ab + a2 + b2);

            // x = height of geometrical centroid
            x = h * (2.0f * ab + a2 + 3.0f * b2) / (4.0f * (ab + a2 + b2));
            {
                fullConeHeight = (R * height) / (R - r);
                float radius = R;
                sina = radius / (float)Math.Sqrt(radius * radius + fullConeHeight * fullConeHeight);
            }
            base.UpdateShape();
        }

        float sina = 0.0f;

        public override void CalculateMassInertia()
        {
            Mass = (1.0f / 3.0f) * JMath.Pi * (R * R + R * r + r * r) * height;

            // inertia through center of mass axis.
            JMatrix inertia = JMatrix.Identity;
            float a = R;
            float b = r;
            float a2 = a * a;
            float a3 = a * a * a;
            float a4 = a2 * a2;
            float b2 = b * b;
            float b3 = b * b * b;
            float b4 = b2 * b2;
            float ab = a * b;
            float h = height;
            float h2 = h * h;
            float div = 20.0f * (float)System.Math.PI * (ab + a2 + b2);
            float div2 = 10.0f * (float)System.Math.PI * (ab + a2 + b2);
            inertia.M11 = (2.0f * h2 * (3.0f * ab + a2 + 6.0f * b2) + 3.0f * (a3 * b + a2 * b2 + a * b3 + a4 * b4)) / div;
            inertia.M22 = 3.0f * (a3 * b + a2 * b2 + a * b3 + a4 + b4) / div2;
            inertia.M33 = (2.0f * h2 * (3.0f * ab + a2 + 6.0f * b2) + 3.0f * (a3 * b + a2 * b2 + a * b3 + a4 * b4)) / div;
            this.Inertia = inertia;
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
            float sigma = (float)Math.Sqrt((float)(direction.X * direction.X + direction.Z * direction.Z));

            if (direction.Y > direction.Length() * sina)
            {
                result.X = 0.0f;
                result.Y = (2.0f / 3.0f) * height;
                result.Z = 0.0f;
            }
            else if (sigma > 0.0f)
            {
                result.X = radius * direction.X / sigma;
                result.Y = -(1.0f / 3.0f) * height;
                result.Z = radius * direction.Z / sigma;
            }
            else
            {
                result.X = 0.0f;
                result.Y = -(1.0f / 3.0f) * height;
                result.Z = 0.0f;
            }

        }
    }
}
#endif