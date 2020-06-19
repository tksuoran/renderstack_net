#if USE_JITTER_PHYSICS

using System.Collections.Generic;
using Jitter.LinearMath;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class ConvexHullShape : Shape
    {
        private Jitter.Collision.Shapes.ConvexHullShape convexHullShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return convexHullShape; } }

        public ConvexHullShape(List<Vector3> vertices)
        {
            List<JVector> jvertices = new List<JVector>(vertices.Count);
            for(int i = 0; i < vertices.Count; ++i)
            {
                Vector3 v = vertices[i];
                jvertices.Add(new JVector(v.X, v.Y, v.Z));
            }
            convexHullShape = new Jitter.Collision.Shapes.ConvexHullShape(jvertices);
        }
        public ConvexHullShape(List<Vector3> vertices, Matrix4 inertia, float mass)
        {
            List<JVector> jvertices = new List<JVector>(vertices.Count);
            for(int i = 0; i < vertices.Count; ++i)
            {
                Vector3 v = vertices[i];
                jvertices.Add(new JVector(v.X, v.Y, v.Z));
            }
            JMatrix jinertia = Util.ToJitter(inertia);
            convexHullShape = new ConvexHullShape2(jvertices, jinertia, mass);
        }
    }
    class ConvexHullShape2 : Jitter.Collision.Shapes.ConvexHullShape
    {
        private Jitter.LinearMath.JMatrix   customInertia;
        private float                       customMass;

        public ConvexHullShape2(
            List<Jitter.LinearMath.JVector> vertices, 
            Jitter.LinearMath.JMatrix       inertia, 
            float                           mass
        )
        :   base(vertices)
        {
            this.customInertia = inertia;
            this.customMass = mass;
            this.Inertia = inertia;
            this.Mass = mass;
        }

        public override void CalculateMassInertia()
        {
            this.Mass = customMass;
            this.Inertia = customInertia;
        }
    }
}

#endif