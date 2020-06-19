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
using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.UI;

using example.Renderer;

namespace example.UIComponents
{
    public delegate void ActionDelegate(Area context);

    public class Button : Area
    {
        private     IRenderer       renderer;
        private     readonly Frame  textFrame = new Frame();
        private     readonly Frame  backgroundFrame = new Frame();
        private     TextBuffer      textBuffer;
        private     NinePatch       ninePatch;
        private     Rectangle       bounds = new Rectangle();
        private     string          label;
        private     bool            dirty   = true;
        private     bool            trigger = false;

        protected   IRenderer       Renderer        { get { return renderer; } }
        protected   Frame           TextFrame       { get { return textFrame; } }
        protected   Frame           BackgroundFrame { get { return backgroundFrame; } }
        protected   NinePatch       NinePatch       { get { return ninePatch; } }
        protected   bool            Trigger         { get { return trigger; } set { trigger = value; } }
        protected   TextBuffer      TextBuffer      { get { return textBuffer; } }

        public ActionDelegate   Action;

        public string           Label
        { 
            get
            {
                return label;
            }
            set
            {
                if(value.CompareTo(label) != 0)
                {
                    dirty = true;
                    label = value;
                }
            }
        }

        public Button(IRenderer renderer, string label)
        : this(renderer, label, Style.Foreground)
        {
        }

        public Button(IRenderer renderer, string label, ActionDelegate action)
        : this(renderer, label, Style.Foreground)
        {
            this.Action = action;
        }
        public Button(IRenderer renderer, string label, Style style)
        {
            this.Style      = style;
            this.renderer   = renderer;
            this.textBuffer = new TextBuffer(Style.Font);
            this.ninePatch  = new NinePatch(Style.NinePatchStyle);
            Name = label;
            Label = label;
        }

        private void UpdateSize()
        {
            if(dirty)
            {
                if(Style.Font != null)
                {
                    textBuffer.BeginPrint();
                    textBuffer.LowPrint(0.0f, 0.0f, 0.0f, Label);
                    textBuffer.EndPrint();
                    bounds.CopyFrom(textBuffer.FontStyle.Bounds);

                    FillBasePixels = bounds.Max + 2.0f * Style.Padding;
                }
                else
                {
                    FillBasePixels.X = 30;
                    FillBasePixels.Y = 10;
                }

                ninePatch.Place(0.0f, 0.0f, 0.0f, FillBasePixels.X, FillBasePixels.Y);
                dirty = false;
            }
        }

        private void UpdatePlace()
        {
            if(Size.X != bounds.Max.X + 2.0f * Style.Padding.X)
            {
                ninePatch.Place(0.0f, 0.0f, 0.0f, Size.X, Size.Y);
            }
        }

        public override void BeginSize(Vector2 freeSizeReference)
        {
            UpdateSize();
            base.BeginSize(freeSizeReference);
        }

        public override void BeginPlace(Rectangle reference, Vector2 growDirection)
        {
            base.BeginPlace(reference, growDirection);
            UpdatePlace();
            //  \note Bypassing UpdateHierarchical()
            //  textFrame.LocalToParent.SetTranslation(
            //  backgroundFrame.LocalToParent.SetTranslation(
            textFrame.LocalToWorld.SetTranslation(
                Rect.Min.X + Style.Padding.X, 
                Rect.Min.Y + Style.Padding.Y, 
                0.0f
            );
            backgroundFrame.LocalToWorld.SetTranslation(
                Rect.Min.X, 
                Rect.Min.Y, 
                0.0f
            );
        }

        public override void DrawSelf(IUIContext context)
        {
            renderer.Push();

            /*  First draw ninepatch  */ 
            renderer.Requested.Program  = Style.Background.Program;
            renderer.Requested.Mesh     = ninePatch.Mesh;
            renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;
            renderer.SetTexture("t_ninepatch", ninePatch.NinePatchStyle.Texture);
            renderer.SetFrame(backgroundFrame);

            if(Rect.Hit(context.Mouse))
            {
                if(context.MouseButtons[(int)(OpenTK.Input.MouseButton.Left)])
                {
                    renderer.Global.Floats("add_color").Set(0.2f, 0.35f, 0.55f);
                    trigger = true;
                }
                else
                {
                    if(trigger)
                    {
                        if(Action != null)
                        {
                            Action(this);
                        }
                        trigger = false;
                    }
                    renderer.Global.Floats("add_color").Set(0.1f, 0.2f, 0.45f);
                }
            }
            else
            {
                trigger = false;
                renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            }

            renderer.Global.Sync();

            renderer.RenderCurrent();

            renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            renderer.Global.Sync();

            /*  Then draw text  */ 
            if(Style.Font != null)
            {
                renderer.Requested.Program  = Style.Program;
                renderer.Requested.Mesh     = textBuffer.Mesh;
                renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;
                //  Assume font texture is always set so no need to set it here

                renderer.SetFrame(textFrame);

                renderer.RenderCurrent();
            }

            renderer.SetFrame(renderer.DefaultFrame);
            renderer.Pop();
        }
    }
}
