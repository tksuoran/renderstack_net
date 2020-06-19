#if USE_JITTER_PHYSICS

using RenderStack.Math;

namespace RenderStack.Physics
{
    public abstract class Shape
    {
        internal abstract Jitter.Collision.Shapes.Shape shape { get; }

        public float Mass { get { return shape.Mass; } /*set { shape.Mass = value; }*/ }

        public Matrix4 Inertia
        {
            get
            {
                return Util.FromJitter(shape.Inertia);
            }
            /*set
            {
                JMatrix jm;
                jm.M11 = value._00; jm.M12 = value._10; jm.M13 = value._20;
                jm.M21 = value._01; jm.M22 = value._11; jm.M23 = value._21;
                jm.M31 = value._02; jm.M32 = value._12; jm.M33 = value._22;
                shape.Inertia = jm;
            }*/
        }
    }
}

#endif