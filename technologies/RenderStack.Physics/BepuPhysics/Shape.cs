#if USE_BEPU_PHYSICS

using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.MathExtensions;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public abstract class Shape
    {
        internal Matrix3X3 massDistribution;
        private float mass;
        public float Mass
        {
            get
            { 
                return mass;
            } 
            set
            {
                mass = value;
            }
        }
        public Matrix4 Inertia
        {
            get
            {
                Matrix4 m = Matrix4.Identity;
                m._00 = massDistribution.M11; m._10 = massDistribution.M12; m._20 = massDistribution.M13;
                m._01 = massDistribution.M21; m._11 = massDistribution.M22; m._21 = massDistribution.M23;
                m._02 = massDistribution.M31; m._12 = massDistribution.M32; m._22 = massDistribution.M33;
                return m;
            }
            set
            {
                massDistribution.M11 = value._00; massDistribution.M12 = value._10; massDistribution.M13 = value._20;
                massDistribution.M21 = value._01; massDistribution.M22 = value._11; massDistribution.M23 = value._21;
                massDistribution.M31 = value._02; massDistribution.M32 = value._12; massDistribution.M33 = value._22;
            }
        }

        public void ComputeMassProperties()
        {
            ShapeDistributionInformation shapeInfo;
            shape.ComputeDistributionInformation(out shapeInfo);
            mass = shapeInfo.Volume;
            Matrix3X3.Multiply(ref shapeInfo.VolumeDistribution, mass * InertiaHelper.InertiaTensorScale, out massDistribution);
        }

        internal abstract BEPUphysics.CollisionShapes.EntityShape shape { get; }
    }
}

#endif