//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using System.Linq;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;
using RenderStack.UI;

using example.Brushes;
using example.Renderer;
using example.UIComponents;

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
