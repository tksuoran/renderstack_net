using System;

using RenderStack.Math;
using RenderStack.Geometry;
using RenderStack.Mesh;
using RenderStack.Physics;

using example.Renderer;

using Material = example.Renderer.Material;

namespace example.Sandbox
{
    class UnitType
    {
        public readonly string      Name;
        public readonly Material    Material;
        public readonly Mesh        Mesh;
        public readonly Shape       CollisionShape;
        public readonly Sphere      BoundingSphere;
        public readonly float       MaxHealth;
        public readonly float       Density;
        public readonly IAI         AI;
        public readonly Type        ControllerType;

        public UnitType(
            string      name,
            Material    material,
            Mesh        mesh,
            Shape       collisionShape,
            Sphere      boundingSphere,
            float       maxHealth,
            float       density,
            IAI         ai,
            Type        controllerType
        )
        {
            Name = name;
            Material = material;
            Mesh = mesh;
            CollisionShape = collisionShape;
            BoundingSphere = boundingSphere;
            MaxHealth = maxHealth;
            Density = density;
            AI = ai;
            ControllerType = controllerType;
        }
    }
}
