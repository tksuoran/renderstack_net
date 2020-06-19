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

using RenderStack.Math;
using RenderStack.Graphics;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Scene
{
    public class LightsUniforms
    {
        public static UniformBlockGL    UniformBlockGL;
        public static UniformBlockRL    UniformBlockRL;
        public static UniformBufferGL   UniformBufferGL;
        //public static UniformBufferRL   UniformBufferRL;

        public static void Initialize(
            string  blockName,
            int     maxLightCount
        )
        {
            UniformBlockGL = new UniformBlockGL(blockName);
            UniformBlockGL.AddInt (spec.Count);
            UniformBlockGL.AddVec4(spec.Exposure);
            UniformBlockGL.AddVec4(spec.Bias);
            UniformBlockGL.AddVec4(spec.AmbientLightColor);
            UniformBlockGL.AddVec4(spec.Color,         maxLightCount);
            UniformBlockGL.AddVec4(spec.Direction,     maxLightCount);
            UniformBlockGL.AddMat4(spec.WorldToLight,  maxLightCount);
            UniformBlockGL.AddMat4(spec.WorldToShadow, maxLightCount);
            UniformBlockGL.Seal();
            var bufferGL = UniformBufferGL = new UniformBufferGL(UniformBlockGL);

#if false
            IUniformBuffer bufferRL = null;
            if(Configuration.useOpenRL)
            {
                UniformBlockRL = new UniformBlockRL(blockName);
                UniformBlockRL.AddInt (spec.Count);
                UniformBlockRL.AddVec4(spec.Exposure);
                UniformBlockRL.AddVec4(spec.Bias);
                UniformBlockRL.AddVec4(spec.AmbientLightColor);
                UniformBlockRL.AddVec4(spec.Color,         maxLightCount);
                UniformBlockRL.AddVec4(spec.Direction,     maxLightCount);
                UniformBlockRL.AddMat4(spec.WorldToLight,  maxLightCount);
                UniformBlockRL.AddMat4(spec.WorldToShadow, maxLightCount);
                UniformBlockRL.Seal();
                UniformBufferRL = new UniformBufferRL(UniformBlockRL);
            }
#endif

            Count               = new MultiInts  (spec.Count            , new Ints(1, 1),                bufferGL /*bufferRL*/);
            Exposure            = new MultiFloats(spec.Exposure         , new Floats( 4, 1),             bufferGL /*bufferRL*/);
            Bias                = new MultiFloats(spec.Bias             , new Floats( 4, 1),             bufferGL /*bufferRL*/);
            AmbientLightColor   = new MultiFloats(spec.AmbientLightColor, new Floats( 4, 1),             bufferGL /*bufferRL*/);
            WorldToLight        = new MultiFloats(spec.WorldToLight     , new Floats(16, maxLightCount), bufferGL /*bufferRL*/);
            WorldToShadow       = new MultiFloats(spec.WorldToShadow    , new Floats(16, maxLightCount), bufferGL /*bufferRL*/);
            Direction           = new MultiFloats(spec.Direction        , new Floats( 4, maxLightCount), bufferGL /*bufferRL*/);
            Color               = new MultiFloats(spec.Color            , new Floats( 4, maxLightCount), bufferGL /*bufferRL*/);
        }

        public struct Spec
        {
            public string Count;
            public string Exposure;
            public string Bias;
            public string AmbientLightColor;
            public string WorldToLight;
            public string WorldToShadow;
            public string Direction;
            public string Color;
        };

        public static Spec      spec;

        public static MultiInts     Count;
        public static MultiFloats   Exposure;               
        public static MultiFloats   Bias;                   
        public static MultiFloats   AmbientLightColor;      
        public static MultiFloats   WorldToLight;           
        public static MultiFloats   WorldToShadow;          
        public static MultiFloats   Direction;              
        public static MultiFloats   Color;                  
    };

    [Serializable]
    /// \note Experimental
    public class Light
    {
        private Camera          camera;
        public  Camera          Camera      { get { return camera; } }

        public  string          Name        { get { return Camera.Name; } set { Camera.Name = value; } }
        public  Frame           Frame       { get { return Camera.Frame; } }
        public Projection       Projection  { get { return Camera.Projection; } }

        private static Matrix4  Texture = new Matrix4(
            0.5f, 0.0f, 0.0f, 0.5f,
            0.0f, 0.5f, 0.0f, 0.5f,
            0.0f, 0.0f, 0.5f, 0.5f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
        private static Matrix4  TextureInverse = new Matrix4(
            2.0f, 0.0f, 0.0f, -1.0f,
            0.0f, 2.0f, 0.0f, -1.0f,
            0.0f, 0.0f, 2.0f, -1.0f,
            0.0f, 0.0f, 0.0f,  1.0f
        );

        private Transform   worldToShadow = new Transform(Matrix4.Identity, Matrix4.Identity);
        public Transform    WorldToShadow { get { return worldToShadow; } }
        public int          LightIndex;

        public Light(int lightIndex)
        {
            LightIndex = lightIndex;
            camera = new Camera();
        }

        public void UpdateFrame()
        {
            Camera.UpdateFrame();
            //parameters.LightToWorld.Set(ViewToWorld.Matrix);
            LightsUniforms.WorldToLight.Set(Frame.LocalToWorld.InverseMatrix);

            /*parameters.ViewPositionInWorld.Set(
                ViewToWorld.Matrix._03,
                ViewToWorld.Matrix._13,
                ViewToWorld.Matrix._23
            );*/
            WorldToShadow.Set(
                Texture * Camera.WorldToClip.Matrix,
                Camera.WorldToClip.InverseMatrix * TextureInverse
            );
            LightsUniforms.WorldToShadow.SetI(LightIndex, WorldToShadow.Matrix);
        }

        /// Viewport has been modified, updates transformations.
        public void UpdateViewport(Viewport viewport)
        {
            Camera.UpdateViewport(viewport);
        }
    }
}