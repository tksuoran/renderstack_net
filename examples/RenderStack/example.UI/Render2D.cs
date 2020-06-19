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

using Buffer = RenderStack.Graphics.Buffer;

namespace example.UI
{
    public partial class Application
    {
        private void Use2DCamera()
        {
            sceneManager.Camera2D.ProjectionType = ProjectionType.OrthogonalRectangle;
            sceneManager.Camera2D.OrthoLeft      = 0;
            sceneManager.Camera2D.OrthoTop       = 0;
            sceneManager.Camera2D.OrthoWidth     = Width;
            sceneManager.Camera2D.OrthoHeight    = Height;
            sceneManager.Camera2D.Near           = -1000.0f;
            sceneManager.Camera2D.Far            =  1000.0f;
            sceneManager.Camera2D.UpdateFrame();
            sceneManager.Camera2D.UpdateViewport(windowViewport);
            renderer.Requested.Camera = sceneManager.Camera2D;
            renderer.UpdateCamera();
        }
        private void Set2DRenderStates()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }

        private void Render2DTexts()
        {
            renderer.Requested.Model    = null;
            renderer.Requested.Mesh     = textRenderer.TextBuffer.Mesh;
            renderer.Requested.Frame    = renderer.DefaultFrame;
            renderer.Requested.Material = materialManager["Textured"];
            renderer.Requested.Program  = renderer.Programs["Textured"];
            renderer.Requested.MeshMode = MeshMode.PolygonFill;

            materialManager["Textured"].Parameters["texture"] = textRenderer.TextBuffer.FontStyle.Texture;

            textRenderer.DebugLine(frameTime);
            textRenderer.DebugLine("Drag with right mouse button to look, use wheel to move. Keys: WASD and RF to move ");
            textRenderer.DebugLine(renderer.Counters.ToString());
            renderer.Counters.Reset();

            textRenderer.DrawDebugLines();
        }
        private void Render2D()
        {
            RenderStack.Graphics.Debug.WriteLine("----- Render2D Begin-----");

            Use2DCamera();
            Set2DRenderStates();

            if(textRenderer != null)
            {
                Render2DTexts();
            }

            if(userInterfaceManager != null)
            {
                userInterfaceManager.Render();
            }

            RenderStack.Graphics.Debug.WriteLine("----- Render2D End -----");
        }
    }
}
