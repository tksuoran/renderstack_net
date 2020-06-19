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
using System.Collections.Generic;

using RenderStack.Graphics;
using RenderStack.Scene;
using RenderStack.UI;

namespace example.UI
{
    public class PushButton : Button
    {
        public bool Pressed = false;

        public PushButton(Renderer renderer, string label):base(renderer,label)
        {
            this.Style = Style.Foreground;
            Name = label;
        }

        public override void DrawSelf(IUIContext context)
        {
            Renderer.Push();

            Renderer.Requested.Frame = BackgroundFrame;

            /*  First draw ninepatch  */ 
            Style.Material.Parameters["texture"] = NinePatch.NinePatchStyle.Texture;
            Renderer.Requested.Program  = Style.Material.Program;
            Renderer.Requested.Material = Style.Material;
            Renderer.Requested.Mesh     = NinePatch.Mesh;
            Renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;

            if(Rect.Hit(context.Mouse))
            {
                if(context.MouseButtons[(int)(OpenTK.Input.MouseButton.Left)])
                {
                    if(Pressed)
                    {
                        (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.3f, 0.3f, 0.7f);
                    }
                    else
                    {
                        (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.6f, 0.6f, 0.8f);
                    }
                    Trigger = true;
                }
                else
                {
                    if(Trigger)
                    {
                        if(Action != null)
                        {
                            Action(this);
                        }
                        else
                        {
                            Pressed = !Pressed;
                        }
                        Trigger = false;
                    }
                    if(Pressed)
                    {
                        (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 1.0f);
                    }
                    else
                    {
                        (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.72f, 0.72f, 0.72f);
                    }
                }
            }
            else
            {
                Trigger = false;
                if(Pressed)
                {
                    (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 1.0f);
                }
                else
                {
                    (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.5f, 0.5f, 0.5f);
                }
            }
            Renderer.RenderCurrent();

            /*  Then draw text  */ 
            Renderer.Requested.Mesh = TextBuffer.Mesh;
            Renderer.Requested.Frame = TextFrame;
            Style.Material.Parameters["texture"]  = TextBuffer.FontStyle.Texture;
            (Renderer.GlobalParameters["global_add_color"] as Floats).Set(0.0f, 0.0f, 0.0f);
            Renderer.RenderCurrent();

            Renderer.Pop();
        }
    }
}
