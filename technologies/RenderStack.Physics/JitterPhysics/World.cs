//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_JITTER_PHYSICS

using System.Collections.Generic;
using Jitter.LinearMath;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class World
    {
        private Jitter.World world;
        private Dictionary<Jitter.Dynamics.RigidBody, RigidBody> map = new Dictionary<Jitter.Dynamics.RigidBody,RigidBody>();
        private List<RigidBody> rigidBodies = new List<RigidBody>();
        public List<RigidBody> RigidBodies { get { return rigidBodies; } }

        public Vector3 Gravity
        {
            get
            {
                return Util.FromJitter(world.Gravity);
            } 
            set 
            {
                world.Gravity = Util.ToJitter(value);
            } 
        }

        public World()
        {
            Jitter.Collision.CollisionSystem collision = new Jitter.Collision.CollisionSystemPersistentSAP();
            world = new Jitter.World(collision);
            world.Gravity.Set(0.0f, -10.0f, 0.0f);
        }

        public void SetDampingFactors(float angular, float linear)
        {
            world.SetDampingFactors(angular, linear);
        }

        public void Step(float step, bool multithread)
        {
            world.Step(step, multithread);
        }

        public void Clear()
        {
            world.Clear();
            rigidBodies.Clear();
            map.Clear();
        }
        public void AddBody(RigidBody body)
        {
            rigidBodies.Add(body);
            map[body.rigidBody] = body;
            world.AddBody(body.rigidBody);
        }
        public void RemoveBody(RigidBody body)
        {
            rigidBodies.Remove(body);
            world.RemoveBody(body.rigidBody);
            map.Remove(body.rigidBody);
        }
        public void AddConstraint(Constraint constraint)
        {
            world.AddConstraint(constraint.constraint);
        }
        public void RemoveConstraint(Constraint constraint)
        {
            world.RemoveConstraint(constraint.constraint);
        }
        private bool raycastCallback(RigidBody body,JVector normal, float fraction)
        {
            return true;
        }
        public bool Raycast(RigidBody body, Vector3 origin, Vector3 direction, out Vector3 normal, out float fraction)
        {
            JVector jnormal;
            JVector jorigin = Util.ToJitter(origin);
            JVector jdirection = Util.ToJitter(direction);
            Jitter.Collision.CollisionSystem system = world.CollisionSystem;
            bool res = system.Raycast(body.rigidBody, jorigin, jdirection, out jnormal, out fraction);
            normal = Util.FromJitter(jnormal);
            return res;
        }
        public bool Raycast(Vector3 origin, Vector3 direction, out RigidBody body, out Vector3 normal, out float fraction)
        {
            Jitter.Dynamics.RigidBody jrigidbody;
            JVector jnormal;
            JVector jorigin = Util.ToJitter(origin);
            JVector jdirection = Util.ToJitter(direction);
            Jitter.Collision.CollisionSystem system = world.CollisionSystem;
            bool res = system.Raycast(
                jorigin,
                jdirection,
                null, // (Jitter.Collision.RaycastCallback)null,
                out jrigidbody,
                out jnormal,
                out fraction
            );
            normal = Util.FromJitter(jnormal);
            if(jrigidbody != null)
            {
                body = map[jrigidbody];
            }
            else
            {
                body = null;
                return false;
            }
            return res;
        }
    }
}

#endif