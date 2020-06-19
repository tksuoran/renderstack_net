#if USE_BEPU_PHYSICS

using System.Collections.Generic;
using BEPUphysics;
using XVector3 = Microsoft.Xna.Framework.Vector3;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class World
    {
        private Space space;
        private List<RigidBody> rigidBodies = new List<RigidBody>();
        public List<RigidBody> RigidBodies { get { return rigidBodies; } }
        private List<Constraint> constraints = new List<Constraint>();

        public Vector3 Gravity { get { XVector3 g = space.ForceUpdater.Gravity; return new Vector3(g.X, g.Y, g.Z); } set { space.ForceUpdater.Gravity = new XVector3(value.X, value.Y, value.Z); } }

        public World()
        {
            space = new Space();
            space.ForceUpdater.Gravity = new XVector3(0.0f, -10.0f, 0.0f);

            if(System.Environment.ProcessorCount > 1)
            {
                for(int i = 0; i < System.Environment.ProcessorCount; i++)
                {
                    space.ThreadManager.AddThread();
                }
            }

        }

        public void SetDampingFactors(float angular, float linear)
        {
            //world.SetDampingFactors(angular, linear);
        }

        public void Step(float step, bool multithread)
        {
            //world.Step(step, multithread);
            space.Update(step);
        }

        public void Clear()
        {
            foreach(var body in rigidBodies)
            {
                space.Remove(body.entity);
            }
            rigidBodies.Clear();
            foreach(var constraint in constraints)
            {
                space.Remove(constraint.updateable);
            }
            constraints.Clear();
        }
        public void AddBody(RigidBody body)
        {
            rigidBodies.Add(body);
            space.Add(body.entity);
        }
        public void RemoveBody(RigidBody body)
        {
            rigidBodies.Remove(body);
            space.Remove(body.entity);
        }
        public void AddConstraint(Constraint constraint)
        {
            constraints.Add(constraint);
            space.Add(constraint.updateable);
        }
        public void RemoveConstraint(Constraint constraint)
        {
            constraints.Remove(constraint);
            space.Remove(constraint.updateable);
        }
    }
}

#endif