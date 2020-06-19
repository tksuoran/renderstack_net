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
    public delegate void ValueChangedDelegate(Area context, float value);

    public class Slider : Area
    {
        private IRenderer               renderer;
        private Frame                   textFrame = new Frame();
        private Frame                   backgroundFrame = new Frame();
        private TextBuffer              textBuffer;
        private NinePatch               ninePatch;
        private Rectangle               bounds = new Rectangle();
        private string                  label;
        private bool                    labelDirty = true;
        private bool                    valueDirty = true;
        private float                   currentRelativeValue;
        private float                   min;
        private float                   max;
        private float                   displayMin;
        private float                   displayMax;
        private ValueChangedDelegate    valueChanged;
        private IParams<float>          parameter;
        private int                     element;

        // \todo Does not mean logarithmic really, only non-linear
        public bool IsLogarithmic = false;

        public ValueChangedDelegate ValueChanged
        {
            get
            {
                return valueChanged;
            }
            set
            {
                if(value != valueChanged)
                {
                    valueChanged = value;
                    if(valueChanged != null)
                    {
                        valueChanged(this, CurrentValue);
                    }
                }
            }
        }
        public string Label
        { 
            get
            {
                return label;
            }
            set
            {
                if(value.CompareTo(label) != 0)
                {
                    labelDirty = true;
                    label = value;
                }
            }
        }

        public float CurrentDisplayValue
        {
            get
            {
                return DisplayMin + RelativeValue * (DisplayMax - DisplayMin);
            }
            set
            {
                RelativeValue = (value - DisplayMin) / (DisplayMax - DisplayMin);
            }
        }
        public float CurrentValue
        {
            get
            {
                return Min + RelativeValue * (Max - Min);
            }
            set
            {
                RelativeValue = (value - Min) / (Max - Min);
            }
        }
        public float SliderRelativeValue
        {
            get
            {
                if(IsLogarithmic)
                {
                    return LogarithmicRelativeValue;
                }
                return RelativeValue;
            }
            set
            {
                if(IsLogarithmic)
                {
                    LogarithmicRelativeValue = value;
                }
                else
                {
                    RelativeValue = value;
                }
            }
        }
        public float LogarithmicRelativeValue
        {
            get
            {
                return (float)Math.Pow(RelativeValue, 1.0 / 4.0);
            }
            set
            {
                RelativeValue = (float)Math.Pow(value, 4.0);
            }
        }
        public float RelativeValue
        {
            get
            {
                return currentRelativeValue;
            }
            set
            {
                if(value != currentRelativeValue)
                {
                    currentRelativeValue = value;
                    if(ValueChanged != null)
                    {
                        ValueChanged(this, CurrentValue);
                    }
                    if(parameter != null)
                    {
                        parameter[element] = CurrentValue;
                    }
                    valueDirty = true;
                }
            }
        }
        public float            Min         { get { return min; } set { min = value; } }
        public float            Max         { get { return max; } set { max = value; } }
        public float            DisplayMin  { get { return displayMin; } set { displayMin = value; } }
        public float            DisplayMax  { get { return displayMax; } set { displayMax = value; } }
        public IParams<float>   Parameter   { get { return parameter; } set { parameter = value; } }
        public int              Element     { get { return element; } set { element = value; } }

        public int Token;

        private IUniformBuffer uniformBuffer;
        public IUniformBuffer UniformBuffer { get { return uniformBuffer; } set { UniformBuffer = value; } }

        public Slider(
            IRenderer       renderer, 
            string          label, 
            IParams<float>  parameter, 
            int             element,
            float           min, 
            float           max,
            float           displayMin, 
            float           displayMax
        )
            :this(renderer,label,parameter,element,min,max,displayMin,displayMax,null)
        {
        }

        public Slider(
            IRenderer       renderer, 
            string          label, 
            IParams<float>  parameter, 
            int             element,
            float           min, 
            float           max,
            float           displayMin, 
            float           displayMax,
            IUniformBuffer  uniformbuffer
        )
        {
            this.renderer = renderer;
            this.parameter = parameter;
            this.element = element;
            Name = label;

            Style = Style.Foreground;
            textBuffer = new TextBuffer(Style.Font);
            ninePatch = new NinePatch(Style.NinePatchStyle);

            Min = min;
            Max = max;
            DisplayMin = displayMin;
            DisplayMax = displayMax;

            if(parameter != null)
            {
                CurrentValue = parameter[element];
            }
            Label = label;

            this.uniformBuffer = uniformbuffer;
        }

        private void UpdateSize()
        {
            if(labelDirty)
            {
                if(Style.Font != null)
                {
                    textBuffer.BeginPrint();
                    textBuffer.LowPrint(0.0f, 0.0f, 0.0f, Label + ": 180.99");
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
                labelDirty = false;
                valueDirty = true;
            }
            if(valueDirty && Style.Font != null)
            {
                textBuffer.BeginPrint();
                textBuffer.LowPrint(0.0f, 0.0f, 0.0f, Label + ": " + String.Format("{0:0.000}", CurrentDisplayValue));
                textBuffer.EndPrint();
            }
        }

        private void UpdatePlace()
        {
            if(Size.X != bounds.Max.X + 2.0f * Style.Padding.X)
            {
                ninePatch.Place(
                    0.0f,
                    0.0f,
                    0.0f,
                    Size.X,
                    Size.Y
                );
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
            //  textFrame.LocalToParent.Set(
            //  backgroundFrame.LocalToParent.Set(
            textFrame.LocalToWorld.Set(
                Matrix4.CreateTranslation(
                    Rect.Min.X + Style.Padding.X, 
                    Rect.Min.Y + Style.Padding.Y, 
                    0.0f
                )
            );
            backgroundFrame.LocalToWorld.Set(
                Matrix4.CreateTranslation(
                    Rect.Min.X, 
                    Rect.Min.Y, 
                    0.0f
                )
            );
        }

        private bool trigger = false;
        public override void DrawSelf(IUIContext context)
        {
            if(
                (parameter != null) &&
                (CurrentValue != parameter[element])
            )
            {
                CurrentValue = parameter[element];
            }

            renderer.Push();

            //  First draw ninepatch
            renderer.Requested.Mesh     = ninePatch.Mesh;
            renderer.Requested.Program  = renderer.Programs["Slider"];
            renderer.Requested.MeshMode = RenderStack.Mesh.MeshMode.PolygonFill;
            renderer.SetTexture("t_ninepatch", ninePatch.NinePatchStyle.Texture);
            renderer.SetFrame(backgroundFrame);

            Rectangle testArea = new Rectangle();
            testArea = Rect;

            if(testArea.Hit(context.Mouse))
            {
                if(context.MouseButtons[(int)(OpenTK.Input.MouseButton.Left)])
                {
                    float x = context.Mouse.X - testArea.Min.X;
                    SliderRelativeValue = x / (testArea.Size.X);

                    renderer.Global.Floats("add_color").Set(0.3f, 0.4f, 0.5f);
                    trigger = true;
                }
                else
                {
                    if(trigger)
                    {
                        trigger = false;
                    }
                    renderer.Global.Floats("add_color").Set(0.2f, 0.3f, 0.4f);
                }
            }
            else
            {
                trigger = false;
                renderer.Global.Floats("add_color").Set(0.1f, 0.2f, 0.3f);
            }
            renderer.Global.Sync();

            float t = SliderRelativeValue;
            float pixelX = testArea.Min.X + t * testArea.Size.X;

            renderer.Requested.Material.Floats("t").Set(pixelX);
            renderer.Requested.Material.Sync();

            renderer.RenderCurrent();

            renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            renderer.Global.Sync();

            //  Then draw text
            if(Style.Font != null)
            {
                renderer.Requested.Program  = renderer.Programs["Font"];
                renderer.Requested.Mesh = textBuffer.Mesh;
                renderer.SetFrame(textFrame);
                //  Assume font texture is always set so no need to set it here
                renderer.RenderCurrent();
            }

            renderer.SetFrame(renderer.DefaultFrame);
            renderer.Pop();
        }
    }
}
