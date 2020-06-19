using System;

using RenderStack.Math;
using RenderStack.Scene;

using example.Renderer;

namespace example.Sandbox
{
    internal interface IAI
    {
        void Update(Unit unit);
    }

    class SeekerAI : IAI
    {
        public void Update(Unit unit)
        {
            var game = Services.Get<Game>();
            var target = game.Player.Model.RigidBody.Position;
            var controller = unit.Controller as LookAtPhysicsFrameController;
            float distance = unit.Model.RigidBody.Position.Distance(target);
            if(controller == null)
            {
                return;
            }
            //if(distance < 20.0f)
            {
                controller.Target = target;
            }

            //  Advance if nearby but not too close
            controller.TranslateZ.Less = (distance > 4.0f) && (distance < 30.0f);
            controller.TranslateZ.More = (distance < 1.0f);
            float speed = unit.Model.RigidBody.LinearVelocity.LengthSquared;
            if(speed > 10.0f)
            {
                controller.TranslateZ.Less = false;
                controller.TranslateZ.More = false;
            }
        }
    }
}
