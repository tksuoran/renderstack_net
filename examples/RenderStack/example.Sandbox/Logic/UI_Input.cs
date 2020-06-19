//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Management;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace example.Sandbox
{
    public partial class UserInterfaceManager
    {
        public bool[]                   mouseButtons = new bool[(int)OpenTK.Input.MouseButton.LastButton];
        public OpenTK.Input.MouseButton SelectButton = OpenTK.Input.MouseButton.Left;
        public OpenTK.Input.MouseButton MoveButton   = OpenTK.Input.MouseButton.Left;
        public OpenTK.Input.MouseButton LookButton   = OpenTK.Input.MouseButton.Right;
        public OpenTK.Input.MouseButton ApplyButton  = OpenTK.Input.MouseButton.Left;

        //private int                                 mouseXDelta;
        //private int                                 mouseYDelta;
        private OpenTK.Input.MouseButtonEventArgs   mouseClick = null;
        public int ButtonsDown;

        public bool[]                                       MouseButtons { get { return mouseButtons; } }
        //public int MouseXDelta                              { get { return mouseXDelta; } }
        //public int MouseYDelta                              { get { return mouseYDelta; } }
        public OpenTK.Input.MouseButtonEventArgs MouseClick { get { return mouseClick; } set { mouseClick = value; } }

        private Vector2 mouse;
        public Vector2 Mouse { get { return mouse; } }

        public bool FlyMode = false;
        private IFrameController userControls;
        public IFrameController UserControls { get { return userControls; } set { userControls = value; } }

        public void ToggleController()
        {
            if(userControls == sceneManager.CameraControls)
            {
                if(RuntimeConfiguration.gameTest)
                {
                    var game = Services.Get<Game>();
                    if(game != null)
                    {
                        var player = game.Player;
                        if(player != null)
                        {
                            userControls = player.Controller;
                        }
                    }
                }
            }
            else
            {
                userControls = sceneManager.CameraControls;
            }
        }
        private void InstallInputEventHandlers()
        {
            window.FocusedChanged += new EventHandler<EventArgs>(window_FocusedChanged);
            window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            window.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
            window.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            window.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            window.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            window.MouseLeave += new EventHandler<EventArgs>(Mouse_MouseLeave);
            window.MouseEnter += new EventHandler<EventArgs>(window_MouseEnter);
            //window.WindowState = OpenTK.WindowState.Fullscreen;
            //window.CursorVisible = false;
        }

        void window_FocusedChanged(object sender, EventArgs e)
        {
            userControls.TranslateX.Inhibit   = !window.Focused;
            userControls.TranslateY.Inhibit   = !window.Focused;
            userControls.TranslateZ.Inhibit   = !window.Focused;
            userControls.RotateX.Inhibit      = !window.Focused;
            userControls.RotateY.Inhibit      = !window.Focused;
            userControls.RotateZ.Inhibit      = !window.Focused;
        }

        #region Mouse
        private bool lockMouse = false;
        public bool LockMouse
        {
            get
            {
                return lockMouse;
            }
            set
            {
                lockMouse = value;
                if(value)
                {
                    CenterMouse();
                }
                else
                {
                    ignore = false;
                }
            }
        }
        bool ignore = false;
        private void CenterMouse()
        {
            System.Drawing.Point centerInWindow = new System.Drawing.Point(window.Width / 2, window.Height / 2);
            System.Drawing.Point centerInScreen = window.PointToScreen(centerInWindow);
            ignore = true;
            OpenTK.Input.Mouse.SetPosition(centerInScreen.X, centerInScreen.Y);
        }
        public int mouseXDelta = 0;
        public int mouseYDelta = 0;
        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if(ignore == true)
            {
                mouseReset = false;
            }
#if false
            if(ignore == false)
            {
                if(lockMouse)
                {
                    mouseXDelta += e.XDelta;
                    mouseYDelta += e.YDelta;

                    //IFrameController cameraControls = sceneManager.CameraControls;
                    //cameraControls.RotateY.Adjust(-e.XDelta / 4096.0f);
                    //cameraControls.RotateX.Adjust(-e.YDelta / 4096.0f);
                    //cameraControls.TranslateZ.Adjust(wheelDelta * 0.2f);
                    //mouseXDelta = 0;
                    //mouseYDelta = 0;

                    CenterMouse();
                }
            }
            else
            {
                ignore = false;
            }
#endif
        }
        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            --ButtonsDown;
            if(MouseButtons[(int)e.Button] != e.IsPressed)
            {
                MouseButtons[(int)e.Button] = e.IsPressed;
            }
        }
        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            ++ButtonsDown;
            if(MouseButtons[(int)e.Button] != e.IsPressed)
            {
                MouseButtons[(int)e.Button] = e.IsPressed;
                if(
                    (HoverArea == null) || 
                    (HoverArea == Layer)
                )
                {
                    mouseClick = e;
                }
            }
        }
        private bool hasMouse = false;
        void Mouse_MouseLeave(object sender, EventArgs e)
        {
            hasMouse = false;
        }
        void window_MouseEnter(object sender, EventArgs e)
        {
            hasMouse = true;
            if(lockMouse)
            {
                ResetMouse();
            }
        }
        #endregion

        #region Keyboards
        public void ToggleFlyMode()
        {
            FlyMode = !FlyMode;
            if(
                (sceneManager != null) && 
                (sceneManager.PhysicsCamera != null) &&
                (sceneManager.PhysicsCamera.RigidBody != null)
            )
            {
                sceneManager.PhysicsCamera.RigidBody.AffectedByGravity = !FlyMode;
            }
        }
        public void Jump()
        {
            var physicsFrameController = userControls as PhysicsFrameController;
            if(physicsFrameController != null)
            {
                physicsFrameController.PhysicsObject.RigidBody.LinearVelocity += new Vector3(0.0f, 4.0f, 0.0f);
                return;
            }

            if(
                (Configuration.physics == true) && 
                (sceneManager.PhysicsCamera != null) &&
                (sceneManager.PhysicsCamera.RigidBody != null)
            )
            {
                sceneManager.PhysicsCamera.RigidBody.LinearVelocity += new Vector3(0.0f, 4.0f, 0.0f);
            }
        }
        public void ToggleCurveToolLock()
        {
            if(RuntimeConfiguration.curveTool && (curveTool != null))
            {
                curveTool.LockEditHandle = !curveTool.LockEditHandle;
            }
        }
        public void ToggleLockMouse()
        {
            ResetMouse();
            LockMouse = !LockMouse;
        }
        private int altDown = 0;
        private int f4 = 0;
        private void IncAlt()
        {
            ++altDown;
            CheckAltF4();
        }
        private void IncF4()
        {
            ++f4;
            CheckAltF4();
        }
        private void DecAlt()
        {
            --altDown;
            if(altDown < 0)
            {
                altDown = 0;
            }
        }
        private void DecF4()
        {
            --f4;
            if(f4 < 0)
            {
                f4 = 0;
            }
        }
        private void CheckAltF4()
        {
            if(altDown > 0 && f4 > 0)
            {
                window.Exit();
            }
        }
        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch(e.Key)
            {
                case OpenTK.Input.Key.AltLeft: DecAlt(); break;
                case OpenTK.Input.Key.AltRight: DecAlt(); break;
                case OpenTK.Input.Key.F4: DecF4(); break;
                case OpenTK.Input.Key.A: userControls.TranslateX.Less = false; break;
                case OpenTK.Input.Key.D: userControls.TranslateX.More = false; break;
                case OpenTK.Input.Key.Q: userControls.TranslateY.Less = false; break;
                case OpenTK.Input.Key.E: userControls.TranslateY.More = false; break;
                case OpenTK.Input.Key.W: userControls.TranslateZ.Less = false; break;
                case OpenTK.Input.Key.S: userControls.TranslateZ.More = false; break;
                case OpenTK.Input.Key.Z: userControls.RotateX.Less = false; break;
                case OpenTK.Input.Key.X: userControls.RotateX.More = false; break;
                //case OpenTK.Input.Key.D: userControls.RotateY.Less = false; break;
                //case OpenTK.Input.Key.A: userControls.RotateY.More = false; break;
                default: break;
            }
        }
        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if(userControls != null)
            {
                switch(e.Key)
                {
                    case OpenTK.Input.Key.AltLeft: IncAlt(); break;
                    case OpenTK.Input.Key.AltRight: IncAlt(); break;
                    case OpenTK.Input.Key.F4: IncF4(); break;
                    case OpenTK.Input.Key.ShiftLeft:    userControls.SpeedModifier.More = !userControls.SpeedModifier.More; break;
                    case OpenTK.Input.Key.A:        userControls.TranslateX.Less = true; break;
                    case OpenTK.Input.Key.D:        userControls.TranslateX.More = true; break;
                    case OpenTK.Input.Key.Q:        userControls.TranslateY.Less = true; break;
                    case OpenTK.Input.Key.E:        userControls.TranslateY.More = true; break;
                    case OpenTK.Input.Key.W:        userControls.TranslateZ.Less = true; break;
                    case OpenTK.Input.Key.S:        userControls.TranslateZ.More = true; break;
                    case OpenTK.Input.Key.Z:        userControls.RotateX.Less = true; break;
                    case OpenTK.Input.Key.X:        userControls.RotateX.More = true; break;
                    //case OpenTK.Input.Key.D:        userControls.RotateY.Less = true; break;
                    //case OpenTK.Input.Key.A:        userControls.RotateY.More = true; break;
                    case OpenTK.Input.Key.Escape:   ToggleLockMouse(); break;
                    case OpenTK.Input.Key.F:        ToggleFlyMode(); break;
                    case OpenTK.Input.Key.Space:    Jump(); break;
                    case OpenTK.Input.Key.Tab:      ToggleCurveToolLock(); break;
                    default: break;
                }
            }

            if(Configuration.keyUserInterface)
            {
                if(operations != null)
                {
                    #region sqrt3 - 1
                    if(e.Key == OpenTK.Input.Key.Number1)
                    {
                        operations.Sqrt3();
                    }
                    #endregion sqrt3
                    #region subdivide - 2
                    if(e.Key == OpenTK.Input.Key.Number2)
                    {
                        operations.Subdivide();
                    }
                    #endregion subdivide
                    #region triangulate - 4
                    if(e.Key == OpenTK.Input.Key.Number4)
                    {
                        operations.Triangulate();
                    }
                    #endregion triangulate
                    #region catmull-clark - 3
                    if(e.Key == OpenTK.Input.Key.Number3)
                    {
                        operations.CatmullClark();
                    }
                    #endregion catmull-clark
                    #region uniform truncate - 5
                    if(e.Key == OpenTK.Input.Key.Number5)
                    {
                        operations.Truncate();
                    }
                    #endregion uniform truncate
                    #region flat normals - 6
                    if(e.Key == OpenTK.Input.Key.Number6)
                    {
                        operations.FlatNormals();
                    }
                    #endregion flat normals
                    #region smooth normals - 7
                    if(e.Key == OpenTK.Input.Key.Number7)
                    {
                        operations.SmoothNormals();
                    }
                    #endregion flat normals
                }
                #region delete - delete
                if(e.Key == OpenTK.Input.Key.Delete)
                {
                    operations.Delete();
                    sounds.Down.Play();
                }
                #endregion delete
                #region insert 
                if(e.Key == OpenTK.Input.Key.Insert)
                {
                    operations.Insert();
                }
                #endregion insert
                #region configuration - function keys
                if(e.Key == OpenTK.Input.Key.F1)
                {
                    RuntimeConfiguration.debugInfo = !RuntimeConfiguration.debugInfo;
                }
                if(e.Key == OpenTK.Input.Key.F2)
                {
                    // \todo save screenshot
                    ShowShadowMaps = !ShowShadowMaps;
                }
                if(e.Key == OpenTK.Input.Key.F3)
                {
                    RuntimeConfiguration.guiExtraInfo = !RuntimeConfiguration.guiExtraInfo;
                }
                if(e.Key == OpenTK.Input.Key.F4)
                {
                    RuntimeConfiguration.selectionSilhouette = !RuntimeConfiguration.selectionSilhouette;
                }
                if(e.Key == OpenTK.Input.Key.F5)
                {
                    ToggleController();
                    RuntimeConfiguration.selectionWireframeEdges = !RuntimeConfiguration.selectionWireframeEdges;
                }
                if(e.Key == OpenTK.Input.Key.F6)
                {
                    RuntimeConfiguration.selectionWireframeHidden = !RuntimeConfiguration.selectionWireframeHidden;
                }
                if(e.Key == OpenTK.Input.Key.F7)
                {
                    RuntimeConfiguration.doubleSided = !RuntimeConfiguration.doubleSided;
                    //RuntimeConfiguration.manipulator = !RuntimeConfiguration.manipulator;
                }
                if(e.Key == OpenTK.Input.Key.F8)
                {
                    RuntimeConfiguration.selectionWireframeCentroids = !RuntimeConfiguration.selectionWireframeCentroids;
                }
                if(e.Key == OpenTK.Input.Key.F9)
                {
                    RuntimeConfiguration.selectionWireframePoints = !RuntimeConfiguration.selectionWireframePoints;
                    /*ShowHandles = !ShowHandles;
                    if(curveTool != null)
                    {
                        curveTool.ShowHandles = ShowHandles;
                    } */
                }
                if(e.Key == OpenTK.Input.Key.F10)
                {
                    RuntimeConfiguration.disableReadPixels = !RuntimeConfiguration.disableReadPixels;
                }
                if(e.Key == OpenTK.Input.Key.F10)
                {
                    RuntimeConfiguration.allWireframe = !RuntimeConfiguration.allWireframe;
                }
                #endregion configuration - function keys
                #region merge - Home
                if (e.Key == OpenTK.Input.Key.Home)
                {
                    operations.Merge();
                    sounds.Up.Play();
                }
                #endregion merge
                #region update ambient occlusion - tab
                if(
                    (e.Key == OpenTK.Input.Key.Tab) &&
                    (aoManager != null)
                )
                {
    #if false
                    Matrix4 oldLocalToParent = sceneManager.Camera.Frame.LocalToParent.Matrix;
                    Matrix4 newLocalToParent = Matrix4.Identity;

                    Vector3 eye     = oldLocalToParent.TransformPoint(Vector3.Zero);
                    Vector3 back    = oldLocalToParent.TransformDirection(Vector3.UnitZ);
                    Vector3 up      = oldLocalToParent.TransformDirection(Vector3.UnitY);
                    newLocalToParent.SetLookAt(eye, eye - back, up);
                    cameraControls.SetTransform(newLocalToParent);
    #endif
                    using(var scope = new Time.Scope("ao update"))
                    {
                        if(selectionManager.HoverModel != null)
                        {
                            aoManager.UpdateAmbientOcclusionModel(selectionManager.HoverModel, sceneManager.AoOccluders);
                        }
                        else
                        {
                            aoManager.UpdateAmbientOcclusion(sceneManager.AoReceivers, sceneManager.AoOccluders);
                        }
                    }
                }
                #endregion update ambient occlusion - tab

                #region physics
                if(Configuration.physics)
                {
                    if(e.Key == OpenTK.Input.Key.PageDown)
                    {
                        operations.PhysicsDeactivate();
                    }
                    if(
                        (e.Key == OpenTK.Input.Key.End) && 
                        (selectionManager.HoverModel != null) && 
                        (selectionManager.HoverModel.Static == false)
                    )
                    {
                        sceneManager.UpdateModelPhysics(selectionManager.HoverModel);
                    }
                }
                #endregion physics
#if true
                /*if (e.Key == OpenTK.Input.Key.Period)
                {
                    manipulatorManager.SetReferenceModel(selectionManager.HoverModel);
                }*/
#if false 
                // \todo fixme
                if(e.Key == OpenTK.Input.Key.Number0)
                {
                    if(selectionManager.HoverModel != null)
                    {
                        if(selectionManager.HoverModel.Batch.Material != null)
                        {
                            selectionManager.HoverModel.Batch.Material.Program = materialManager.GetNextProgram(
                                selectionManager.HoverModel.Batch.Material.Program
                            );
                        }
                    }
                }
#endif
#endif
            }
        }

        #endregion
    }
}
