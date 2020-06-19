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

using RenderStack.Math;

namespace RenderStack.Scene
{
    [Serializable]
    /// \brief Maintains a generic 3D transformation and its inverse.
    /// 
    /// \note Mostly stable
    public class Transform
    {
        private Matrix4 matrix;
        private Matrix4 inverseMatrix;

        public Matrix4 Matrix        { get { return matrix; } }
        public Matrix4 InverseMatrix { get { return inverseMatrix; } } 

        public Matrix4 GetMatrix(){ return matrix; }

        public void SetTranslation(Vector3 v)
        {
            Matrix4.CreateTranslation(
                v.X, v.Y, v.Z, 
                out matrix
            );
            Matrix4.CreateTranslation(
                -v.X, -v.Y, -v.Z, 
                out inverseMatrix
            );
            //Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void FixInverse()
        {
            inverseMatrix = Matrix4.Invert(matrix);
        }
        public void SetTranslation(float x, float y, float z)
        {
            Matrix4.CreateTranslation(
                x, y, z, 
                out matrix
            );
            Matrix4.CreateTranslation(
                -x, -y, -z, 
                out inverseMatrix
            );
            //Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetRotation(float angleRadians, Vector3 axis)
        {
            Matrix4.CreateRotation(angleRadians, axis, out matrix);
            Matrix4.CreateRotation(-angleRadians, axis, out inverseMatrix);
        }
        public void SetScale(float x)
        {
            Matrix4.CreateScale(x, out matrix);
            Matrix4.CreateScale(1.0f / x, out inverseMatrix);
        }
        public void SetScale(float x, float y, float z)
        {
            Matrix4.CreateScale(x, y, z, out matrix);
            Matrix4.CreateScale(1.0f / x, 1.0f / y, 1.0f / z, out inverseMatrix);
        }

        public void SetProjection(
            float s,                //  Stereo-scopic 3D eye separation
            float p,                //  Perspective (0 == parallel, 1 == perspective)
            float n, float f,       //  Near and far z clip depths
            float w, float h,       //  Width and height of viewport (at depth vz)
            Vector3 v,              //  Center of viewport
            Vector3 e               //  Center of projection (eye position)
        )
        {
            Matrix4.CreateProjection(s, p, n, f, w, h, v, e, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetOrthographic(float left, float right, float bottom, float top, float near, float far)
        {
            Matrix4.CreateOrthographic(left, right, bottom, top, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetOrthographicCentered(float width, float height, float near, float far)
        {
            Matrix4.CreateOrthographicCentered(width, height, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetFrustum(float left, float right, float bottom, float top, float near, float far)
        {
            Matrix4.CreateFrustum(left, right, bottom, top, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetFrustumSimple(float width, float height, float near, float far)
        {
            Matrix4.CreateFrustumSimple(width, height, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetPerspective(float fovXRadians, float fovYRadians, float near, float far)
        {
            Matrix4.CreatePerspective(fovXRadians, fovYRadians, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetPerspectiveVertical(float fovYRadians, float aspectRatio, float near, float far)
        {
            Matrix4.CreatePerspectiveVertical(fovYRadians, aspectRatio, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        public void SetPerspectiveHorizontal(float fovXRadians, float aspectRatio, float near, float far)
        {
            Matrix4.CreatePerspectiveHorizontal(fovXRadians, aspectRatio, near, far, out matrix);
            Matrix4.Invert(matrix, out inverseMatrix);
        }

        public Transform(Transform t)
        {
            matrix        = t.Matrix;
            inverseMatrix = t.inverseMatrix;
        }
        public Transform()
        {
            matrix        = Matrix4.Identity;
            inverseMatrix = Matrix4.Identity;
        }
        public Transform(Matrix4 matrix)
        {
            this.matrix        = matrix;
            this.inverseMatrix = Matrix4.Invert(matrix);
        }
        public Transform(Matrix4 matrix, Matrix4 inverseMatrix)
        {
            this.matrix        = matrix;
            this.inverseMatrix = inverseMatrix;
        }
        public void Set(Matrix4 matrix)
        {
            this.matrix         = matrix;
            this.inverseMatrix  = Matrix4.Invert(matrix);
        }
        public void Set(Matrix4 matrix, Matrix4 inverseMatrix)
        {
            this.matrix         = matrix;
            this.inverseMatrix  = inverseMatrix;
        }

        public void Catenate(Matrix4 t)
        {
            matrix = matrix * t;
            Matrix4.Invert(matrix, out inverseMatrix);
        }
        /*public void Inverse(Transform transform, Transform result)
        {
            result.matrix           = transform.InverseMatrix;
            result.inverseMatrix    = transform.Matrix;
        }*/

        /*  Operations to transform these:  */ 
        /*  Point      */
        /*  Vector3    */ 
        /*  Normal     */ 
        /*  Ray        */ 
        /*  BBox       */ 
        /*  Transform  */

    }
}
