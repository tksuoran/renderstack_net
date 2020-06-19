using RenderStack.Math;
using example.Renderer;

namespace example.Sandbox
{
    internal class Unit : IUpdateFixedStep, IUpdateOncePerFrame
    {
        public readonly UnitType    Type;
        public float                Health;
        public Model                Model;
        public IFrameController     Controller;

        public Unit(UnitType type, Vector3 position)
        {
            this.Type = type;
            this.Health = type.MaxHealth;
            this.Model = new Model(type.Name, type.Mesh, type.Material);
            Model.PhysicsShape = type.CollisionShape;
        }

        public void UpdateFixedStep()
        {
            if(Type.AI != null)
            {
                Type.AI.Update(this);
            }
            if(Controller != null)
            {
                Controller.UpdateFixedStep();
            }
        }

        public void UpdateOncePerFrame()
        {
            if(Controller != null)
            {
                Controller.UpdateOncePerFrame();
            }
        }
    }
}
