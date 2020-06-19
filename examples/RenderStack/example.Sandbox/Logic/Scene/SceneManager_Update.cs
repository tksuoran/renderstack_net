using System.Collections.Generic;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public partial class SceneManager : Service, ISceneManager
    {
        private ulong                       updateSerial = 1; // must start greater than 0
        public ulong                        UpdateSerial { get { return updateSerial; } }

        private List<IUpdateOncePerFrame>   updateOncePerFrame = new List<IUpdateOncePerFrame>();
        private List<IUpdateFixedStep>      updateFixedStep    = new List<IUpdateFixedStep>();

        public void NextUpdateSerial()
        {
            ++updateSerial;
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

            camera.Frame.UpdateHierarchical(updateSerial);
            foreach(var group in RenderGroups)
            {
                UpdateOncePerFrame(group);
            }
        }
        public void UpdateOncePerFrame(Group group)
        {
            foreach(var light in group.Lights)
            {
                light.Camera.Frame.UpdateHierarchical(updateSerial);
            }
            foreach(var model in group.Models)
            {
                model.Frame.UpdateHierarchical(updateSerial);
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