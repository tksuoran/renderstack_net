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
    public class CameraUniformBlock
    {
        public static void Initialize(string blockName)
        {
            UniformBlockGL = new UniformBlockGL(blockName);
            UniformBlockGL.AddVec4(NearFar);
            UniformBlockGL.AddMat4(ViewToClip);
            UniformBlockGL.AddMat4(WorldToClip);
            UniformBlockGL.AddMat4(WorldToView);
            UniformBlockGL.AddMat4(ViewToWorld);
            UniformBlockGL.AddVec4(Viewport);
            UniformBlockGL.AddVec4(ViewPositionInWorld);
            UniformBlockGL.Seal();

            UniformBlockRL = new UniformBlockRL(blockName);
            UniformBlockRL.AddVec4(FovXFovYAspect);
            UniformBlockRL.AddMat4(ViewToClip);
            UniformBlockRL.AddMat4(WorldToClip);
            UniformBlockRL.AddMat4(WorldToView);
            UniformBlockRL.AddMat4(ViewToWorld);
            UniformBlockRL.AddVec4(Viewport);
            UniformBlockRL.AddVec4(ViewPositionInWorld);
            UniformBlockRL.Seal();
        }
        public static UniformBlockGL    UniformBlockGL;
        public static UniformBlockRL    UniformBlockRL;
        public static string            NearFar;
        public static string            FovXFovYAspect;
        public static string            WorldToClip;   //  world <-> clip
        public static string            ClipToWorld;
        public static string            ViewToWorld;   //  view <-> world
        public static string            WorldToView;
        public static string            ViewToClip;    //  view <-> clip
        public static string            ClipToView;
        public static string            Viewport;
        public static string            ViewPositionInWorld;
    };

    public class CameraParameters
    {
        public CameraParameters(IUniformBuffer bufferGL/*, IUniformBuffer bufferRL*/)
        {
            NearFar              = new MultiFloats(CameraUniformBlock.NearFar            , new Floats( 4, 1), bufferGL/*, bufferRL*/);
            FovXFovYAspect       = new MultiFloats(CameraUniformBlock.FovXFovYAspect     , new Floats( 4, 1), bufferGL/*, bufferRL*/);
            Viewport             = new MultiFloats(CameraUniformBlock.Viewport           , new Floats( 4, 1), bufferGL/*, bufferRL*/);
            WorldToClip          = new MultiFloats(CameraUniformBlock.WorldToClip        , new Floats(16, 1), bufferGL/*, bufferRL*/);
            ClipToWorld          = new MultiFloats(CameraUniformBlock.ClipToWorld        , new Floats(16, 1), bufferGL/*, bufferRL*/);
            ViewToWorld          = new MultiFloats(CameraUniformBlock.ViewToWorld        , new Floats(16, 1), bufferGL/*, bufferRL*/);
            WorldToView          = new MultiFloats(CameraUniformBlock.WorldToView        , new Floats(16, 1), bufferGL/*, bufferRL*/);
            ViewToClip           = new MultiFloats(CameraUniformBlock.ViewToClip         , new Floats(16, 1), bufferGL/*, bufferRL*/);
            ClipToView           = new MultiFloats(CameraUniformBlock.ClipToView         , new Floats(16, 1), bufferGL/*, bufferRL*/);
            ViewPositionInWorld  = new MultiFloats(CameraUniformBlock.ViewPositionInWorld, new Floats( 4, 1), bufferGL/*, bufferRL*/);
        }
        public MultiFloats NearFar;
        public MultiFloats FovXFovYAspect;
        public MultiFloats Viewport;
        public MultiFloats WorldToClip;
        public MultiFloats ClipToWorld;
        public MultiFloats ViewToWorld;
        public MultiFloats WorldToView;
        public MultiFloats ViewToClip;
        public MultiFloats ClipToView;
        public MultiFloats ViewPositionInWorld;
    }

    [Serializable]
    /// \brief Manages transformations used for cameras
    /// 
    /// \note Recently refactored
    public class Camera
    {
        private string              name;
        private Frame               frame = new Frame();
        private UniformBufferGL     uniformBufferGL;
        //private UniformBufferRL     uniformBufferRL;
        private Projection          projection = new Projection();
        private Transform           viewToClip  = new Transform(Matrix4.Identity, Matrix4.Identity);    /*  also known as the projection matrix  */ 
        private Transform           worldToClip = new Transform(Matrix4.Identity, Matrix4.Identity);    /*  transform world space to clip space  */ 
        private CameraParameters    parameters;

        public  string              Name            { get { return name; } set { name = value; } }
        public  Frame               Frame           { get { return frame; } }
        public  UniformBufferGL     UniformBufferGL { get { return uniformBufferGL; } }
        //public  UniformBufferRL     UniformBufferRL { get { return uniformBufferRL; } }
        public  CameraParameters    Parameters      { get { return parameters; } }
        public  Projection          Projection      { get { return projection; } }
        public  Transform           ViewToClip      { get { return viewToClip;  } }
        public  Transform           WorldToClip     { get { return worldToClip; } }

        public Camera()
        {
            uniformBufferGL = new UniformBufferGL(CameraUniformBlock.UniformBlockGL);
#if false
            if(Configuration.useOpenRL)
            {
                uniformBufferRL = new UniformBufferRL(CameraUniformBlock.UniformBlockRL);
            }
#endif
            parameters = new CameraParameters(uniformBufferGL/*uniformBufferRL*/);
        }

        // \todo UpdateProjection()
        /// Viewport has been modified, updates transformations.
        public void UpdateViewport(Viewport viewport)
        {
            parameters.FovXFovYAspect.Set(Projection.FovXRadians, Projection.FovYRadians, viewport.AspectRatio);
            parameters.NearFar.Set(Projection.Near, Projection.Far);
            parameters.Viewport.Set(
                (float)viewport.X, 
                (float)viewport.Y, 
                (float)viewport.Width, 
                (float)viewport.Height
            );

            try
            {
                projection.Update(ViewToClip, viewport);
            }
            catch(Exception e)
            {
                Trace.TraceError("UpdateViewport exception: " + e.ToString());
            }

            //  world to (view) local  =>  view to clip
            WorldToClip.Set(
                ViewToClip.Matrix * frame.LocalToWorld.InverseMatrix,
                Frame.LocalToWorld.Matrix * ViewToClip.InverseMatrix 
            );

            parameters.ViewToClip.Set(ViewToClip.Matrix);
            parameters.ClipToView.Set(ViewToClip.InverseMatrix);
            parameters.WorldToClip.Set(WorldToClip.Matrix);
            parameters.ClipToWorld.Set(WorldToClip.InverseMatrix);
        }

        /// Camera Frame has been modified, update transformations
        public void UpdateFrame()
        {
            parameters.ViewToWorld.Set(frame.LocalToWorld.Matrix);
            parameters.WorldToView.Set(frame.LocalToWorld.InverseMatrix);

            parameters.ViewPositionInWorld.Set(
                frame.LocalToWorld.Matrix._03,
                frame.LocalToWorld.Matrix._13,
                frame.LocalToWorld.Matrix._23,
                1.0f
            );
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(Name);
            sb.Append(" ");
            sb.Append(Projection.ToString());
            return sb.ToString();
        }
    }
}







#if false
    public enum ModelsUniform
    {
        ModelToWorld,  //  model <-> world
        WorldToModel,
    };

    //  Models and Camera combined
    public enum ModelsCameraUniform
    {
        ModelToView,   //  model <-> view
        ViewToModel,
        ModelToClip,   //  model <-> clip
        ClipToModel
    }
        public Floats ModelToWorld     = new Floats(16,1);
        public Floats WorldToModel     = new Floats(16,1);
        public Floats ModelToClip      = new Floats(16,1);
        public Floats ClipToModel      = new Floats(16,1);
        public Floats ModelToView      = new Floats(16,1);
        public Floats ViewToModel      = new Floats(16,1);
        public Floats ModelToShadow    = new Floats(16,1);

    //  Models and Lights combined
    public enum ModelsLightsUniform
    {
        ModelToShadow
    }

        private Frame           model;
        public Frame            Model   { get { return model; } set { model = value; } }
        private Transform       modelToClip     = new Transform(Matrix4.Identity, Matrix4.Identity);    /*  transform current frame to clip space    */ 
        private Transform       modelToView     = new Transform(Matrix4.Identity, Matrix4.Identity);    /*  transform current frame to camera space  */ 
        private Transform[]     modelToShadow;
        public Transform        ModelToClip     { get { return modelToClip; } }
        public Transform        ModelToView     { get { return modelToView; } }
        public Transform[]      ModelToShadow   { get { return modelToShadow; } }

        /// Model Frame (or Model) has been modified, update transformations.
        /// 
        public void UpdateModelFrame(Frame modelFrame)
        {
            Model = modelFrame;

            //  (model) local to world  =>  world to clip
            ModelToClip.Set(
                WorldToClip.Matrix * Model.LocalToWorld.Matrix,
                Model.LocalToWorld.InverseMatrix * WorldToClip.InverseMatrix
            );
            //  (model) local to world  =>  world to (view) local
            ModelToView.Set(
                Frame.LocalToWorld.InverseMatrix * Model.LocalToWorld.Matrix,
                Model.LocalToWorld.InverseMatrix * Frame.LocalToWorld.Matrix
            );

            parameters.ModelToClip.Set(ModelToClip.Matrix);
            parameters.ClipToModel.Set(ModelToClip.InverseMatrix);
            parameters.ModelToView.Set(ModelToView.Matrix);
            parameters.ViewToModel.Set(ModelToView.InverseMatrix);
            parameters.ModelToWorld.Set(Model.LocalToWorld.Matrix);
            parameters.WorldToModel.Set(Model.LocalToWorld.InverseMatrix);

            if(Shadow != null)
            {
                if(modelToShadow == null || modelToShadow.Length != Shadow.Length)
                {
                    modelToShadow = new Transform[Shadow.Length];
                    for(int i = 0; i < Shadow.Length; ++i)
                    {
                        modelToShadow[i] = new Transform(Matrix4.Identity, Matrix4.Identity);
                    }
                    parameters.ModelToShadow = new Floats(16, Shadow.Length);
                }
                for(int i = 0; i < Shadow.Length; ++i)
                {
                    ModelToShadow[i].Set(
                        Texture * Shadow[i].WorldToClip.Matrix * Model.LocalToWorld.Matrix,
                        Model.LocalToWorld.InverseMatrix * Shadow[i].WorldToClip.InverseMatrix * TextureInverse
                    );
                    parameters.ModelToShadow.Set(i, ModelToShadow[i].Matrix);
                }
            }
        }
#endif

#if false
        public void UpdateClipPlanes()
        {
            // Left clipping plane

            p_planes[0].a = comboMatrix._41 + comboMatrix._11;
            p_planes[0].b = comboMatrix._42 + comboMatrix._12;
            p_planes[0].c = comboMatrix._43 + comboMatrix._13;
            p_planes[0].d = comboMatrix._44 + comboMatrix._14;
            // Right clipping plane
            p_planes[1].a = comboMatrix._41 - comboMatrix._11;
            p_planes[1].b = comboMatrix._42 - comboMatrix._12;
            p_planes[1].c = comboMatrix._43 - comboMatrix._13;
            p_planes[1].d = comboMatrix._44 - comboMatrix._14;
            // Top clipping plane
            p_planes[2].a = comboMatrix._41 - comboMatrix._21;
            p_planes[2].b = comboMatrix._42 - comboMatrix._22;
            p_planes[2].c = comboMatrix._43 - comboMatrix._23;
            p_planes[2].d = comboMatrix._44 - comboMatrix._24;
            // Bottom clipping plane
            p_planes[3].a = comboMatrix._41 + comboMatrix._21;
            p_planes[3].b = comboMatrix._42 + comboMatrix._22;
            p_planes[3].c = comboMatrix._43 + comboMatrix._23;
            p_planes[3].d = comboMatrix._44 + comboMatrix._24;
            // Near clipping plane
            p_planes[4].a = comboMatrix._41 + comboMatrix._31;
            p_planes[4].b = comboMatrix._42 + comboMatrix._32;
            p_planes[4].c = comboMatrix._43 + comboMatrix._33;
            p_planes[4].d = comboMatrix._44 + comboMatrix._34;
            // Far clipping plane
            p_planes[5].a = comboMatrix._41 - comboMatrix._31;
            p_planes[5].b = comboMatrix._42 - comboMatrix._32;
            p_planes[5].c = comboMatrix._43 - comboMatrix._33;
            p_planes[5].d = comboMatrix._44 - comboMatrix._34;
            // Normalize the plane equations, if requested
            if (normalize == true)
            {
            NormalizePlane(p_planes[0]);
            NormalizePlane(p_planes[1]);
            NormalizePlane(p_planes[2]);
            NormalizePlane(p_planes[3]);
            NormalizePlane(p_planes[4]);
            NormalizePlane(p_planes[5]);
            }
        }
#endif
