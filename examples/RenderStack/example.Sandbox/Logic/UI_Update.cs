using OpenTK.Input;
using RenderStack.Services;
using RenderStack.UI;

namespace example.Sandbox
{
    public partial class UserInterfaceManager : Service, IUIContext
    {
        public void ConnectUserControls()
        {
            userControls = sceneManager.CameraControls;
        }
  
        bool mouseReset = true;
        MouseState current;
        MouseState previous;        
        public void ResetMouse()
        {
            mouseReset = true;
            previous = current = OpenTK.Input.Mouse.GetState();
        }
        public void UpdateFixedStep()
        {
            if(RuntimeConfiguration.gameTest == false)
            {
                IFrameController cameraControls = sceneManager.CameraControls;
                if(userControls != null)
                {
                    cameraControls.UpdateFixedStep();
                }
            }
        }
        public void UpdateOncePerFrame()
        {
            if(RuntimeConfiguration.gameTest == false)
            {
                IFrameController cameraControls = sceneManager.CameraControls;

                //if(ignore != false)
                {
                    current = OpenTK.Input.Mouse.GetState();
                    if(mouseReset == false && (current != previous))
                    {
                        if(lockMouse && hasMouse && window.Focused)
                        {
                            int xdelta = current.X - previous.X;
                            int ydelta = current.Y - previous.Y;
                            float wheelDelta = current.WheelPrecise - previous.WheelPrecise;
                            if((xdelta > 0) || (ydelta > 0) || (xdelta < 0) || (ydelta < 0))
                            {
                                userControls.RotateY.Adjust(-xdelta / 1024.0f);
                                userControls.RotateX.Adjust(-ydelta / 1024.0f);
                                CenterMouse();
                                //System.Console.WriteLine("Mouse delta: " + xdelta + ", " + ydelta);
                            }
                        }

                    }
                    previous = current;
                }
                cameraControls.UpdateOncePerFrame();
            }

            //float wheelNow = window.Mouse.WheelPrecise;
            //float wheelDelta = wheel - wheelNow;
            //wheel = wheelNow;

            var m = materialManager[CurrentMaterial];
            {
                if(m != null)
                {
                    m.Sync();
                }
            }
        }
    }
}
