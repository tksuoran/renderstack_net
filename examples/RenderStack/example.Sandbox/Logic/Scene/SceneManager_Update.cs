using System.Collections.Generic;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public partial class SceneManager : Service, ISceneManager
    {
        public ulong UpdateSerial { get; private set; } = 1;

        private List<IUpdateOncePerFrame>   updateOncePerFrame = new List<IUpdateOncePerFrame>();
        private List<IUpdateFixedStep>      updateFixedStep    = new List<IUpdateFixedStep>();

        public void NextUpdateSerial()
        {
            ++UpdateSerial;
        }

        public void Add(IUpdateOncePerFrame update)
        {
            if(update == null)
            {
                return;
            }
            updateOncePerFrame.Add(update);
        }
        public void Add(IUpdateFixedStep update)
        {
            if(update == null)
            {
                return;
            }
            updateFixedStep.Add(update);
        }

        public void UpdateOncePerFrame()
        {
            NextUpdateSerial();

            if(Configuration.physics == true)
            {
                FetchPhysics();
            }

            foreach(var update in updateOncePerFrame)
            {
                update.UpdateOncePerFrame();
                UpdateShadowMap = true;
            }

            camera.Frame.UpdateHierarchical(UpdateSerial);
            foreach(var group in RenderGroups)
            {
                UpdateOncePerFrame(group);
            }
        }
        public void UpdateOncePerFrame(Group group)
        {
            foreach(var light in group.Lights)
            {
                light.Camera.Frame.UpdateHierarchical(UpdateSerial);
            }
            foreach(var model in group.Models)
            {
                model.Frame.UpdateHierarchical(UpdateSerial);
            }
        }
        public void UpdateFixed()
        {
            foreach(var update in updateFixedStep)
            {
                update.UpdateFixedStep();
            }
        }
    }
}