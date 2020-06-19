//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

#if USE_BEPU_PHYSICS

using System.Collections.Generic;
using BEPUphysics.CollisionShapes;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class ConvexHullShape : Shape
    {
        private BEPUphysics.CollisionShapes.ConvexShapes.ConvexHullShape convexHullShape;

        internal override BEPUphysics.CollisionShapes.EntityShape shape { get { return convexHullShape; } }

        public ConvexHullShape(List<Vector3> vertices)
        {
            List<Microsoft.Xna.Framework.Vector3> xvertices = new List<Microsoft.Xna.Framework.Vector3>(vertices.Count);
            for(int i = 0; i < vertices.Count; ++i)
            {
                Vector3 v = vertices[i];
                xvertices.Add(new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z));
            }
            convexHullShape = new BEPUphysics.CollisionShapes.ConvexShapes.ConvexHullShape(xvertices);
            ComputeMassProperties();
        }
        public ConvexHullShape(List<Vector3> vertices, Matrix4 inertia, float mass)
        {
            List<Microsoft.Xna.Framework.Vector3> xvertices = new List<Microsoft.Xna.Framework.Vector3>(vertices.Count);
            for(int i = 0; i < vertices.Count; ++i)
            {
                Vector3 v = vertices[i];
                xvertices.Add(new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z));
            }
            convexHullShape = new BEPUphysics.CollisionShapes.ConvexShapes.ConvexHullShape(xvertices);
            Mass = mass;
            Inertia = inertia;
        }
    }
}

#endif