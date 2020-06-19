//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using RenderStack.Math;
using RenderStack.UI;

namespace example.UI
{
    public partial class UserInterfaceManager : Service, IUIContext
    {
        public bool[]                   mouseButtons = new bool[(int)MouseButton.LastButton];
        public MouseButton              SelectButton = MouseButton.Left;
        public MouseButton              MoveButton   = MouseButton.Left;
        public MouseButton              LookButton   = MouseButton.Right;

        private int                     mouseXDelta;
        private int                     mouseYDelta;
        private float                   wheel;
        private MouseButtonEventArgs    mouseClick = null;

        public bool[]                   MouseButtons    { get { return mouseButtons; } }
        public int                      MouseXDelta     { get { return mouseXDelta; } }
        public int                      MouseYDelta     { get { return mouseYDelta; } }
        public MouseButtonEventArgs     MouseClick      { get { return mouseClick; } set { mouseClick = value; } }

        private Vector2 mouse;
        public Vector2 Mouse { get { return mouse; } }

        public void InstallInputEventHandlers()
        {
            window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
            window.Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);
            window.MouseLeave += new EventHandler<EventArgs>(Mouse_MouseLeave);
            OpenTK.Input.Mouse.SetPosition(0,0);
        }

        void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if(MouseButtons[(int)LookButton] == true)
            {
                mouseXDelta += e.XDelta;
                mouseYDelta += e.YDelta;
            }
        }
        void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(MouseButtons[(int)e.Button] != e.IsPressed)
            {
                MouseButtons[(int)e.Button] = e.IsPressed;
            }
        }
        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(MouseButtons[(int)e.Button] != e.IsPressed)
            {
                MouseButtons[(int)e.Button] = e.IsPressed;
                mouseClick = e;
            }
        }
        void Mouse_MouseLeave(object sender, EventArgs e)
        {
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if(e.Key == OpenTK.Input.Key.Number1)
            {
            }
        }
    }
}
