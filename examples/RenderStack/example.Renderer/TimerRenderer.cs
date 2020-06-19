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
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.UI;
using RenderStack.Services;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;

namespace example.Sandbox
{
    public class TimerRenderer : Service
    {
        public override string Name
        {
            get { return "TimerRenderer"; }
        }

        QuadRenderer        quadRenderer;
        Renderer.IRenderer  renderer;
        TextRenderer        textRenderer;
        OpenTK.GameWindow   window;

        public void Connect(
            Renderer.IRenderer  renderer,
            TextRenderer        textRenderer,
            OpenTK.GameWindow   window
        )
        {
            this.renderer = renderer;
            this.textRenderer = textRenderer;
            this.window = window;
        }
        protected override void InitializeService()
        {
            quadRenderer = new QuadRenderer(renderer);
        }
        public void Render()
        {
            renderer.Push();

            quadRenderer.Begin();
            Vector3 o = new Vector3(10.0f, 64, 0.0f);
            Vector3 a = o;
            float alpha = 0.50f;

            textRenderer.TextBuffer.BeginPrint();

            foreach(var timer in Timer.Timers)
            {
                quadRenderer.Quad(
                    o, 
                    o + new Vector3(10.0f * timer.CPUTime, 5.0f, 0.0f),
                    new Vector4(timer.Color, alpha)
                );
                o.Y += 6.0f;
                quadRenderer.Quad(
                    o, 
                    o + new Vector3(10.0f * timer.GPUTime, 5.0f, 0.0f),
                    new Vector4(timer.Color, alpha)
                );
                textRenderer.TextBuffer.LowPrint(
                    o.X, 
                    o.Y - 4.0f,
                    0.0f, 
                    timer.ToString()
                );

                o.Y += 6.0f;
            }
            quadRenderer.Quad(
                new Vector3(10.0f + 159.0f, a.Y, 0.0f),
                new Vector3(10.0f + 161.0f, o.Y, 0.0f),
                new Vector4(1.0f, 1.0f, 1.0f, alpha)
            );
            quadRenderer.End();

            textRenderer.TextBuffer.EndPrint();
            renderer.Requested.Mesh     = textRenderer.TextBuffer.Mesh;
            renderer.Requested.Material = textRenderer.Material;
            renderer.Requested.Program  = textRenderer.Material.Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            //renderer.SetTexture("t_font", textRenderer.TextBuffer.FontStyle.Texture);
            renderer.SetFrame(renderer.DefaultFrame);
            renderer.RenderCurrent();

            renderer.Global.Floats("alpha").Set(0.5f);
            renderer.Global.Sync();

            renderer.Requested.Mesh     = quadRenderer.Mesh;
            // \todo
            //renderer.Requested.Material = uiMaterial; //  this has blending on  
            renderer.Requested.Program  = renderer.Programs["ColorFill"]; //colorFill;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetFrame(renderer.DefaultFrame);
            renderer.RenderCurrent();

            renderer.Pop();
        }
    }
}