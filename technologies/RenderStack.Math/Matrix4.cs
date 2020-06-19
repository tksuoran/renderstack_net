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
using System.Runtime.InteropServices;

/*                                                                      */ 
/*  m[0][0] m[1][0] m[2][0] m[3][0]    sx       tx                      */ 
/*  m[0][1] m[1][1] m[2][1] m[3][1]       sy    ty                      */ 
/*  m[0][2] m[1][2] m[2][2] m[3][2]          sz tz                      */ 
/*  m[0][3] m[1][3] m[2][3] m[3][3]              1                      */ 
/*                                                                      */ 
/*     c0      c1      c2      c3      r  u  b  p                       */ 
/*                                     i  p  a  o                       */ 
/*    _00     _01     _02     _03      g     c  s                       */ 
/*    _10     _11     _12     _13      h     k                          */ 
/*    _20     _21     _22     _23      t                                */ 
/*    _30     _31     _32     _33                                       */ 
/*                                                                      */ 
/*  Matrix Quaternion FAQ    OpenGL              Matrix3                */ 
/*                                                                      */ 
/*  |  0  1  2  3 |          | 0  4   8 12 |    | _00 _01 _02 _03 |     */ 
/*  |             |          |             |    |                 |     */ 
/*  |  4  5  6  7 |          | 1  5   9 13 |    | _10 _11 _12 _13 |     */ 
/*  |             |          |             |    |                 |     */ 
/*  |  8  9 10 11 |          | 2  6  10 14 |    | _20 _21 _22 _23 |     */ 
/*  |             |          |             |    |                 |     */ 
/*  | 12 13 14 15 |          | 3  7  11 15 |    | _30 _31 _32 _33 |     */ 
/*                                                                      */ 
/*  MQF  Matrix4                                                        */ 
/*                                                                      */ 
/*   0 = _00                                                            */ 
/*   1 = _01                                                            */ 
/*   2 = _02                                                            */ 
/*   3 = _03                                                            */ 
/*   4 = _10                                                            */ 
/*   5 = _11                                                            */ 
/*   6 = _12                                                            */ 
/*   7 = _13                                                            */ 
/*   8 = _20                                                            */ 
/*   9 = _21                                                            */ 
/*  10 = _22                                                            */ 
/*  11 = _23                                                            */ 
/*  12 = _30                                                            */ 
/*  13 = _31                                                            */ 
/*  14 = _32                                                            */ 
/*  15 = _33                                                            */ 

/*  http://www.opengl.org/resources/faq/technical/transformations.htm#tran0005
  
9.005 Are OpenGL matrices column-major or row-major?

For programming purposes, OpenGL matrices are 16-value arrays 
with base vectors laid out contiguously in memory. 

The translation components occupy the 13th, 14th, and 15th elements 
of the 16-element matrix, where indices are numbered from 1 to 16 
as described in section 2.11.2 of the OpenGL 2.1 Specification.

Column-major versus row-major is purely a notational convention.
Note that post-multiplying with column-major matrices produces
the same result as pre-multiplying with row-major matrices. 
The OpenGL Specification and the OpenGL Reference Manual both
use column-major notation. You can use any notation, as long
as it's clearly stated.

Sadly, the use of column-major format in the spec and blue book
has resulted in endless confusion in the OpenGL programming community. 
Column-major notation suggests that matrices are not laid out in
memory as a programmer would expect.

 
 */


namespace RenderStack.Math
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    /// \brief Generic 4 by 4 matrix class - compatible with OpenGL conventions
    ///   - Scenario #4 from http://and-what-happened.blogspot.com/2011/05/row-major-and-column-major-matrices.html
    ///   - Column major memory layout as in OpenGL        
    ///   - Right handed                                   
    ///   - Element names are _rc, _, row, column
    ///   - Column 0 = right axis (_00, _10, _20)
    ///   - Column 1 = up axis (_01, _11, _21)
    ///   - Column 2 = back axis (right handed coordinate system!) (_02, _12, _13)
    ///   - Column 3 = position, aka. translation (_03, _13, _23)
    ///   - Base vectors are columns                       
    ///   - Translation is in the last column              
    ///   - Column vectors                                 
    ///   - Row matrices                                   
    ///   - 4x4 * 4x1 -> 4x1                               
    ///   - matrix * column vector                         
    ///   - matrix operations are applied on the left      
    ///   - M2 * M1 * v                                    
    ///   - BAx                                            
    ///   - world_to_clip * local_to_world * v             
    ///   - M * v : multiply the element in                   
    ///     the vector with every element in the row                 
    ///   - Transpose of column matrix is row matrix                 
    ///   - Transpose of row matrix is column matrix                 
    ///   - Default field layout for structs is sequential           
    ///   - Structs cannot have default constructors,                
    ///     they have built-in default constructors                  
    ///   - You cannot pass partially initialized struct anywhere    
    ///   - Instances of classes are only stored on the GC heap      
    ///   - Instances of structs and other value types only          
    ///     on the stack or inside objects                           
    ///   - A(BC)  = (AB)C       associative  
    ///   - A(B+C) =  AB + AC    distributive 
    ///   - (A+B)C =  AC + BC                 
    ///   - c(AB)  = (cA)B                    
    ///   - (Ac)B  = A(cB)                    
    ///   - (AB)c  = A(Bc)                    
    ///   - AB     = (BT AT)T                 
    ///  
    /// \note Mostly stable
    public struct Matrix4 : IEquatable<Matrix4>
    {
        #region Matrix Elements, Memory Layoyt, Basic constructor, indexer [,]
        /*  First column - base vector X - right axis */ 
        public float _00;  /*  a1  OpenGL  */
        public float _10;  /*  a2          */ 
        public float _20;  /*  a3          */ 
        public float _30;  /*  a4          */ 

        /*  Second column - base vector Y - up axis */
            public float _01;  /*  a5  */ 
            public float _11;  /*  a6  */ 
            public float _21;  /*  a7  */ 
            public float _31;  /*  a8  */ 

        /*  Third column - base vector Z - back vector */
                public float _02;  /*  a9   */ 
                public float _12;  /*  a10  */ 
                public float _22;  /*  a11  */ 
                public float _32;  /*  a12  */

        /*  Fourth column - translation  */ 
                    public float _03;  /*  a13 OpenGL 13th  */
                    public float _13;  /*  a14 OpenGL 14th  */
                    public float _23;  /*  a15 OpenGL 15th  */
                    public float _33;  /*  a16              */

        /*  _rc is row column  */ 
        public Matrix4(
            /* col 1st       2nd        3rd        4th                   */ 
            float _00, float _01, float _02, float _03,  /*  First  row  */ 
            float _10, float _11, float _12, float _13,  /*  Second row  */ 
            float _20, float _21, float _22, float _23,  /*  Third  row  */ 
            float _30, float _31, float _32, float _33   /*  Fourth row  */ 
        )
        {
            /*  Set the first column  */
            this._00 = _00;
            this._10 = _10;
            this._20 = _20;
            this._30 = _30;

            /*  Set the second column  */
            this._01 = _01;
            this._11 = _11;
            this._21 = _21;
            this._31 = _31;

            /*  Set the third column  */
            this._02 = _02;
            this._12 = _12;
            this._22 = _22;
            this._32 = _32;

            /*  Set the fourth column  */
            this._03 = _03;
            this._13 = _13;
            this._23 = _23;
            this._33 = _33;
        }

        public void Set(Matrix4 m)
        {
            _00 = m._00;
            _10 = m._10;
            _20 = m._20;
            _30 = m._30;
            _01 = m._01;
            _11 = m._11;
            _21 = m._21;
            _31 = m._31;
            _02 = m._02;
            _12 = m._12;
            _22 = m._22;
            _32 = m._32;
            _03 = m._03;
            _13 = m._13;
            _23 = m._23;
            _33 = m._33;
        }

        private static readonly float[] gl1m = new Single[16];
        public float[] GL1Matrix()
        {
            gl1m[ 0] = _00; gl1m[ 1] = _01; gl1m[ 2] = _02; gl1m[ 3] = _03;
            gl1m[ 4] = _10; gl1m[ 5] = _11; gl1m[ 6] = _12; gl1m[ 7] = _13;
            gl1m[ 8] = _20; gl1m[ 9] = _21; gl1m[10] = _22; gl1m[11] = _23;
            gl1m[12] = _30; gl1m[13] = _31; gl1m[14] = _32; gl1m[15] = _33;
            return gl1m;
        }
        public float[] RLMatrix()
        {
            gl1m[ 0] = _00; gl1m[ 1] = _10; gl1m[ 2] = _20; gl1m[ 3] = _30;
            gl1m[ 4] = _01; gl1m[ 5] = _11; gl1m[ 6] = _21; gl1m[ 7] = _31;
            gl1m[ 8] = _02; gl1m[ 9] = _12; gl1m[10] = _22; gl1m[11] = _32;
            gl1m[12] = _03; gl1m[13] = _13; gl1m[14] = _23; gl1m[15] = _33;
            return gl1m;
        }


        //  x=cross(y,z), y=cross(z,x), z=cross(x,y)
        //  for left handed view = center - eye
        //  for right handed view = eye -center
        public static Matrix4 CreateLookAt(
            Vector3 eye,
            Vector3 center,
            Vector3 up
        )
        {
            Matrix4 matrix;
            CreateLookAt(eye, center, up, out matrix);
            return matrix;
        }

        public static void CreateLookAt(
            Vector3 eye,
            Vector3 center,
            Vector3 up0,
            out Matrix4 result
        )
        {
            if(eye == center)
            {
                // \todo respect requested up vector
                result = identity;
                return;
            }
            Vector3 back    = Vector3.Normalize(eye - center);
            if(up0 == back)
            {
                up0 = back.MinAxis;
            }

            Vector3 right   = Vector3.Normalize(Vector3.Cross(up0, back));
            Vector3 up      = Vector3.Cross(back, right);

            /*  right axis = column0  */ 
            result._00 = right.X;
            result._10 = right.Y;
            result._20 = right.Z;

            /*  up axis = column1  */ 
            result._01 = up.X;
            result._11 = up.Y;
            result._21 = up.Z;

            /*  back axis = column2 */ 
            result._02 = back.X;
            result._12 = back.Y;
            result._22 = back.Z;

            result._03 = eye.X;
            result._13 = eye.Y;
            result._23 = eye.Z;

            result._30 = 0.0f;
            result._31 = 0.0f;
            result._32 = 0.0f;
            result._33 = 1.0f;
        }

        public void SetLookAtBroken(Vector3 eye, Vector3 center, Vector3 up0)
        {
            Vector3 view    = Vector3.Normalize(center - eye);
            Vector3 right   = Vector3.Normalize(Vector3.Cross(up0, view));
            Vector3 up      = Vector3.Cross(view, right);

            /*  right axis = row0  */ 
            _00 = right.X;
            _01 = right.Y;
            _02 = right.Z;
            _03 = -Vector3.Dot(right, eye);

            /*  up axis = row1  */ 
            _10 = up.X;
            _11 = up.Y;
            _12 = up.Z;
            _13 = -Vector3.Dot(up, eye);

            /*  view axis = row2 */ 
            _20 = view.X;
            _21 = view.Y;
            _22 = view.Z;
            _23 = -Vector3.Dot(view, eye);

            _30 = 0.0f;
            _31 = 0.0f;
            _32 = 0.0f;
            _33 = 1.0f;
        }

        /*  index = r + c * N  N = 4  */
        public float this[int row, int column]
        {
            get
            {
                switch(column * 4 + row)
                {
                    /*  Column major  */
                    /*  First Column  */
                    case 0 + 0 * 4: return _00;
                    case 1 + 0 * 4: return _10;
                    case 2 + 0 * 4: return _20;
                    case 3 + 0 * 4: return _30;

                    /*  Second Column  */
                    case 0 + 1 * 4: return _01;
                    case 1 + 1 * 4: return _11;
                    case 2 + 1 * 4: return _21;
                    case 3 + 1 * 4: return _31;

                    /*  Third Column  */
                    case 0 + 2 * 4: return _02;
                    case 1 + 2 * 4: return _12;
                    case 2 + 2 * 4: return _22;
                    case 3 + 2 * 4: return _32;

                    /*  Fourth Column  */
                    case 0 + 3 * 4: return _03;
                    case 1 + 3 * 4: return _13;
                    case 2 + 3 * 4: return _23;
                    case 3 + 3 * 4: return _33;
                    default: return 0.0f;
                }
            }
        }

        public Vector3 GetRow3(int rowI)
        {
            Vector3 row;

            switch(rowI)
            {
                case 0: row.X = _00; row.Y = _01; row.Z = _02; break;
                case 1: row.X = _10; row.Y = _11; row.Z = _12; break;
                case 2: row.X = _20; row.Y = _21; row.Z = _22; break;
                case 3: row.X = _30; row.Y = _31; row.Z = _32; break;
                default: row.X = 0; row.Y = 0; row.Z = 0; break;
            }
            return row;
        }

        public Vector4 GetRow(int rowI)
        {
            Vector4 row;

            switch(rowI)
            {
                case 0: row.X = _00; row.Y = _01; row.Z = _02; row.W = _03; break;
                case 1: row.X = _10; row.Y = _11; row.Z = _12; row.W = _13; break;
                case 2: row.X = _20; row.Y = _21; row.Z = _22; row.W = _23; break;
                case 3: row.X = _30; row.Y = _31; row.Z = _32; row.W = _33; break;
                default: row.X = 0; row.Y = 0; row.Z = 0; row.W = 0; break;
            }
            return row;
        }

        public Vector3 GetColumn3(int columnI)
        {
            Vector3 column;

            switch(columnI)
            {
                case 0: column.X = _00; column.Y = _10; column.Z = _20; break;
                case 1: column.X = _01; column.Y = _11; column.Z = _21; break;
                case 2: column.X = _02; column.Y = _12; column.Z = _22; break;
                case 3: column.X = _03; column.Y = _13; column.Z = _23; break;
                default: column.X = 0; column.Y = 0; column.Z = 0; break;
            }
            return column;
        }

        public void SetColumn3(int columnI, Vector3 v)
        {
            switch(columnI)
            {
                case 0: _00 = v.X; _10 = v.Y; _20 = v.Z; break;
                case 1: _01 = v.X; _11 = v.Y; _21 = v.Z; break;
                case 2: _02 = v.X; _12 = v.Y; _22 = v.Z; break;
                case 3: _03 = v.X; _13 = v.Y; _23 = v.Z; break;
                default: break;
            }
        }

        public Vector4 GetColumn(int columnI)
        {
            Vector4 column;

            switch(columnI)
            {
                case 0: column.X = _00; column.Y = _10; column.Z = _20; column.W = _30; break;
                case 1: column.X = _01; column.Y = _11; column.Z = _21; column.W = _31; break;
                case 2: column.X = _02; column.Y = _12; column.Z = _22; column.W = _32; break;
                case 3: column.X = _03; column.Y = _13; column.Z = _23; column.W = _33; break;
                default: column.X = 0; column.Y = 0; column.Z = 0; column.W = 0; break;
            }
            return column;
        }

        private static Matrix4 identity = new Matrix4(
            1f, 0f, 0f, 0f, 
            0f, 1f, 0f, 0f, 
            0f, 0f, 1f, 0f, 
            0f, 0f, 0f, 1f
        );

        public static Matrix4 Identity
        {
            get { return identity; }
        }
        #endregion

#if false
        /*  NOT TESTED  */ 
        //  Matrix from Quaternion
        public Matrix4(
            Neure.Quaternion q
        )
        {
            float x2 = 2.0f * q.X;
            float y2 = 2.0f * q.Y;
            float z2 = 2.0f * q.Z;
            float xx = q.X * x2;
            float xy = q.X * y2;
            float xz = q.X * z2;
            float yy = q.Y * y2;
            float yz = q.Y * z2;
            float zz = q.Z * z2;
            float wx = q.W * x2;
            float wy = q.W * y2;
            float wz = q.W * z2;

            _03 =
            _13 =
            _23 =
            _30 = _31 = _32 = 0.0f;
            _33 = 1.0f;

            _00 = 1.0f - yy - zz;
            _10 = xy - wz;
            _20 = xz + wy;

            _01 = xy + wz;
            _11 = 1.0f - xx - zz;
            _21 = yz - wx;

            _02 = xz - wy;
            _12 = yz + wx;
            _22 = 1.0f - xx - yy;
        }
#endif

        public float Trace3
        {
            get
            {
                return _00 + _11 + _22;
            }
        }
        public float Trace
        {
            get
            {
                return _00 + _11 + _22 + _33;
            }
        }


        #region Projections
        /*  http://and-what-happened.blogspot.com/p/just-formulas.html  */ 
        /*  The projection produced by this formula has x, y and z extents of -1:+1.  */
        /*  The perspective control value p is not restricted to integer values.  */ 
        /*  The view plane is defined by z.  */ 
        /*  Objects on the view plane will have a homogeneous w value of 1.0 after the transform. */ 

        public static void CreateProjection(
            float s,                //  Stereo-scopic 3D eye separation
            float p,                //  Perspective (0 == parallel, 1 == perspective)
            float n, float f,       //  Near and far z clip depths
            float w, float h,       //  Width and height of viewport (at depth vz)
            Vector3 v,              //  Center of viewport
            Vector3 e,              //  Center of projection (eye position)
            out Matrix4 result
        )
        {
            result._00 =  2.0f / w;
            result._01 =  0.0f;
            result._02 = (2.0f * (e.X - v.X) + s) / (w * (v.Z - e.Z));
            result._03 = (2.0f * ((v.X * e.Z) - (e.X * v.Z)) - s * v.Z) / (w * (v.Z - e.Z));

            result._10 =  0.0f;
            result._11 =  2.0f / h;
            result._12 =  2.0f * (e.Y - v.Y) / (h * (v.Z - e.Z));
            result._13 =  2.0f * ((v.Y * e.Z) - (e.Y * v.Z)) / (h * (v.Z - e.Z));

            result._20 =  0.0f;
            result._21 =  0.0f;
            result._22 = (2.0f * (v.Z * (1.0f - p) - e.Z) + p * (f + n)) / ((f - n) * (v.Z - e.Z));
            result._23 = -((v.Z * (1.0f - p) - e.Z) * (f + n) + 2.0f * f * n * p) / ((f - n) * (v.Z - e.Z));

            result._30 =  0.0f;
            result._31 =  0.0f;
            result._32 = p / (v.Z - e.Z);
            result._33 = (v.Z * (1.0f - p) - e.Z) / (v.Z - e.Z);

            /*  Changes handedness  */
            result._02 = -result._02;
            result._12 = -result._12;
            result._22 = -result._22;
            result._32 = -result._32;
        }

        public static void CreateOrthographic(float left, float right, float bottom, float top, float near, float far, out Matrix4 result)
        {
            float width  = right - left;
            float height = top - bottom;
            float depth  = far - near;
            float x     =  2.0f / width;
            float y     =  2.0f / height;
            float z     = -2.0f / depth;
            float xt    = -(right + left) / width;
            float yt    = -(top + bottom) / height;
            float zt    = -(far + near) / depth;

            result._00 = x;    result._01 = 0.0f; result._02 = 0.0f; result._03 = xt;
            result._10 = 0.0f; result._11 = y;    result._12 = 0.0f; result._13 = yt;
            result._20 = 0.0f; result._21 = 0.0f; result._22 = z;    result._23 = zt;
            result._30 = 0.0f; result._31 = 0.0f; result._32 = 0.0f; result._33 = 1.0f;
        }
        public static void CreateOrthographicCentered(float width, float height, float near, float far, out Matrix4 result)
        {
            float depth = far - near;
            float faddn = far + near;
            float x     =  2.0f / width;
            float y     =  2.0f / height;
            float z     = -2.0f / depth;
            float zt    = -faddn / depth;

            result._00 = x;    result._01 = 0.0f; result._02 = 0.0f; result._03 = 0.0f;
            result._10 = 0.0f; result._11 = y;    result._12 = 0.0f; result._13 = 0.0f;
            result._20 = 0.0f; result._21 = 0.0f; result._22 = z;    result._23 = zt;
            result._30 = 0.0f; result._31 = 0.0f; result._32 = 0.0f; result._33 = 1.0f;
        }
        public static void CreateFrustum(float left, float right, float bottom, float top, float near, float far, out Matrix4 result)
        {
            float x, y, a, b, c, d;

            //  TODO Do we need to do something about potential division by zero?
            x =  (2.0f  * near)   / (right - left);
            y =  (2.0f  * near)   / (top   - bottom);
            a =  (right + left)   / (right - left);
            b =  (top   + bottom) / (top   - bottom);
            c = -(far   + near)   / (far   - near);
            d = -(2.0f * far * near) / (far - near);

            result._00 = x;     result._01 = 0.0f;  result._02 = a;      result._03 = 0.0f;
            result._10 = 0.0f;  result._11 = y;     result._12 = b;      result._13 = 0.0f;
            result._20 = 0.0f;  result._21 = 0.0f;  result._22 = c;      result._23 = d;
            result._30 = 0.0f;  result._31 = 0.0f;  result._32 = -1.0f;  result._33 = 0.0f;
        }
        public static void CreateFrustumSimple(float width, float height, float near, float far, out Matrix4 result)
        {
            float x;
            float y;
            float c;
            float d;
            float near2;
            float far_plus__near;
            float far_minus_near;
            float far_times_near;

            near2          = 2.0f * near;
            far_plus__near = far + near;
            far_minus_near = far - near;
            far_times_near = far * near;

            x = near2 / width;
            y = near2 / height;
            c = -       far_plus__near / far_minus_near;
            d = -2.0f * far_times_near / far_minus_near;

            result._00 = x; result._01 = 0; result._02 = 0;     result._03 = 0;
            result._10 = 0; result._11 = y; result._12 = 0;     result._13 = 0;
            result._20 = 0; result._21 = 0; result._22 = c;     result._23 = d;
            result._30 = 0; result._31 = 0; result._32 = -1.0f; result._33 = 0;
        }
        public static void CreatePerspective(float fovXRadians, float fovYRadians, float near, float far, out Matrix4 result)
        {
            fovYRadians = System.Math.Max(fovYRadians, 0.01f);
            fovYRadians = System.Math.Min(fovYRadians, (float)(System.Math.PI * 0.99));
            fovXRadians = System.Math.Max(fovXRadians, 0.01f);
            fovXRadians = System.Math.Min(fovXRadians, (float)(System.Math.PI * 0.99));
            float tanXHalfAngle = (float)System.Math.Tan(fovXRadians * 0.5);
            float tanYHalfAngle = (float)System.Math.Tan(fovYRadians * 0.5);
            float width         = 2.0f * near * tanXHalfAngle;
            float height        = 2.0f * near * tanYHalfAngle;
            CreateFrustumSimple(width, height, near, far, out result);
        }
        public static void CreatePerspectiveVertical(float fovYRadians, float aspectRatio, float near, float far, out Matrix4 result)
        {
            fovYRadians = System.Math.Max(fovYRadians, 0.01f);
            fovYRadians = System.Math.Min(fovYRadians, (float)(System.Math.PI * 0.99));
            float tanHalfAngle = (float)System.Math.Tan(fovYRadians * 0.5);
            float height       = 2.0f * near * tanHalfAngle;
            float width        = height * aspectRatio;

            CreateFrustumSimple(width, height, near, far, out result);
        }
        public static void CreatePerspectiveHorizontal(float fovXRadians, float aspectRatio, float near, float far, out Matrix4 result)
        {
            fovXRadians = System.Math.Max(fovXRadians, 0.01f);
            fovXRadians = System.Math.Min(fovXRadians, (float)(System.Math.PI * 0.99));
            float tanHalfAngle = (float)System.Math.Tan(fovXRadians * 0.5);
            float width        = 2.0f * near * tanHalfAngle;
            float height       = width / aspectRatio;

            CreateFrustumSimple(width, height, near, far, out result);
        }
        #endregion
        #region Translation, Rotation, Scale
        public static Matrix4 CreateTranslation(Vector3 v)
        {
            Matrix4 matrix;
            CreateTranslation(v.X, v.Y, v.Z, out matrix);
            return matrix;
        }
        public static Matrix4 CreateTranslation(float x, float y, float z)
        {
            Matrix4 matrix;
            CreateTranslation(x, y, z, out matrix);
            return matrix;
        }
        public static void CreateTranslation(float x, float y, float z, out Matrix4 result)
        {
            /*  Translation goes to fourth column  */

            /*  Set the first column  */
            result._00 = 1.0f;
            result._10 = 0.0f;
            result._20 = 0.0f;
            result._30 = 0.0f;

            /*  Set the second column  */
            result._01 = 0.0f;
            result._11 = 1.0f;
            result._21 = 0.0f;
            result._31 = 0.0f;

            /*  Set the third column  */
            result._02 = 0.0f;
            result._12 = 0.0f;
            result._22 = 1.0f;
            result._32 = 0.0f;    /*  Opengl 15th  */

            /*  Set the fourth column  */
            result._03 = x;
            result._13 = y;
            result._23 = z;
            result._33 = 1.0f;
        }
        public static Matrix4 CreateRotation(float angleRadians, Vector3 axis)
        {
            Matrix4 matrix;
            CreateRotation(angleRadians, axis, out matrix);
            return matrix;
        }
        public static void CreateRotation(float angleRadians, Vector3 axis, out Matrix4 result)
        {
            float rsin = (float)System.Math.Sin(angleRadians);
            float rcos = (float)System.Math.Cos(angleRadians);

            float u = axis.X;
            float v = axis.Y;
            float w = axis.Z;

            /*  Set the first row  */
            result._00 =      rcos + u * u * (1 - rcos);
            result._01 = -w * rsin + v * u * (1 - rcos);
            result._02 =  v * rsin + w * u * (1 - rcos);
            result._03 =                              0;

            /*  Set the second row  */
            result._10 =  w * rsin + u * v * (1 - rcos);
            result._11 =      rcos + v * v * (1 - rcos);
            result._12 = -u * rsin + w * v * (1 - rcos);
            result._13 =                              0;

            /*  Set the third row  */
            result._20 = -v * rsin + u * w * (1 - rcos);
            result._21 =  u * rsin + v * w * (1 - rcos);
            result._22 =      rcos + w * w * (1 - rcos);
            result._23 =                              0;

            /*  Set the fourth row  */
            result._30 = 0.0f;
            result._31 = 0.0f;
            result._32 = 0.0f;
            result._33 = 1.0f;
        }
        public static Matrix4 CreateScale(float x)
        {
            Matrix4 matrix;
            CreateScale(x, out matrix);
            return matrix;
        }
        public static void CreateScale(float s, out Matrix4 result)
        {
            /*  Set the first column  */
            result._00 = s;
            result._10 = 0.0f;
            result._20 = 0.0f;
            result._30 = 0.0f;

            /*  Set the second column  */
            result._01 = 0.0f;
            result._11 = s;
            result._21 = 0.0f;
            result._31 = 0.0f;

            /*  Set the third column  */
            result._02 = 0.0f;
            result._12 = 0.0f;
            result._22 = s;
            result._32 = 0.0f;

            /*  Set the fourth column  */
            result._03 = 0.0f;  /*  X translation  */
            result._13 = 0.0f;  /*  Y translation  */
            result._23 = 0.0f;  /*  Z translation  */
            result._33 = 1.0f;
        }
        public static Matrix4 CreateScale(float x, float y, float z)
        {
            Matrix4 matrix;
            CreateScale(x, y, z, out matrix);
            return matrix;
        }
        public static void CreateScale(float x, float y, float z, out Matrix4 result)
        {
            /*  Set the first column  */
            result._00 = x;
            result._10 = 0.0f;
            result._20 = 0.0f;
            result._30 = 0.0f;

            /*  Set the second column  */
            result._01 = 0.0f;
            result._11 = y;
            result._21 = 0.0f;
            result._31 = 0.0f;

            /*  Set the third column  */
            result._02 = 0.0f;
            result._12 = 0.0f;
            result._22 = z;
            result._32 = 0.0f;

            /*  Set the fourth column  */
            result._03 = 0.0f;  /*  X translation  */
            result._13 = 0.0f;  /*  Y translation  */
            result._23 = 0.0f;  /*  Z translation  */
            result._33 = 1.0f;
        }
        public static Matrix4 CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix4 ret;
            CreateFromQuaternion(ref quaternion, out ret);
            return ret;
        }
        public static void CreateFromQuaternion(ref Quaternion q, out Matrix4 result)
        {
            result = Matrix4.Identity;

            result._00 = 1.0f - 2.0f * (q.Y * q.Y + q.Z * q.Z);
            result._01 =        2.0f * (q.X * q.Y + q.W * q.Z); // !
            result._02 =        2.0f * (q.X * q.Z - q.W * q.Y); // !
            result._03 = 0.0f;
            result._10 =        2.0f * (q.X * q.Y - q.W * q.Z); // !
            result._11 = 1.0f - 2.0f * (q.X * q.X + q.Z * q.Z);
            result._12 =        2.0f * (q.Y * q.Z + q.W * q.X); // !
            result._13 = 0.0f;
            result._20 =        2.0f * (q.X * q.Z + q.W * q.Y);
            result._21 =        2.0f * (q.Y * q.Z - q.W * q.X);
            result._22 = 1.0f - 2.0f * (q.X * q.X + q.Y * q.Y);
            result._23 = 0.0f;
            result._30 = 0.0f;
            result._31 = 0.0f;
            result._32 = 0.0f;
            result._33 = 1.0f;

#if false
            Matrix4 result2;
            float x2 = 2.0f * q.X;
            float y2 = 2.0f * q.Y;
            float z2 = 2.0f * q.Z;
            float xx = q.X * x2;
            float xy = q.X * y2;
            float xz = q.X * z2;
            float yy = q.Y * y2;
            float yz = q.Y * z2;
            float zz = q.Z * z2;
            float wx = q.W * x2;
            float wy = q.W * y2;
            float wz = q.W * z2;

            //  Setup rotation and inverse rotation
            result2._00 = 1.0f - yy - zz;
            result2._10 =        xy + wz;
            result2._20 =        xz - wy;
            result2._30 = 0.0f;

            result2._01 =        xy - wz;
            result2._11 = 1.0f - xx - zz;
            result2._21 =        yz + wx;
            result2._31 = 0.0f;

            result2._02 =        xz + wy;
            result2._12 =        yz - wx;
            result2._22 = 1.0f - xx - yy;
            result2._32 = 0.0f;

            //  Apply translation
            result2._03 = 0.0f;
            result2._13 = 0.0f;
            result2._23 = 0.0f;
            result2._33 = 1.0f;
#endif
        }
        public static Matrix4 CreateFromEulerAngles(float h, float p, float b)
        {
            Matrix4 ret;
            CreateFromEulerAngles(h, p, b, out ret);
            return ret;
        }
        public static void CreateFromEulerAngles(float h, float p, float b, out Matrix4 result)
        {
            float A = (float)System.Math.Cos(p); /*  x-axis  */
            float B = (float)System.Math.Sin(p);
            float C = (float)System.Math.Cos(h); /*  y-axis  */
            float D = (float)System.Math.Sin(h);
            float E = (float)System.Math.Cos(b); /*  z-axis  */
            float F = (float)System.Math.Sin(b);

            float DB = D * B;
            float CB = C * B;
            result._00 =   C *  E  +  -DB * F;
            result._01 =   C * -F  +  -DB * E;
            result._02 =  -D * A;
            result._03 =  0;
                    
            result._10 =  A * F;
            result._11 =  A * E;
            result._12 = -B;
            result._13 = 0.0f;
                    
            result._20 = D *  E + CB * F;
            result._21 = D * -F + CB * E;
            result._22 = C *  A;
            result._23 = 0.0f;
                    
            result._30 = 0.0f;
            result._31 = 0.0f;
            result._32 = 0.0f;
            result._33 = 1.0f;
        }
        #endregion
#if false
        /*  NOT TESTED  */ 
        #region Create with Inverse Transform
        public static void CreateWithInverseFromQuaternionAndTranslation(
            Neure.Quaternion    rotation,
            Neure.Vector3       translation,
            out Neure.Matrix4x4 transform,
            out Neure.Matrix4x4 inverseTransform
        )
        {
            float x2 = 2.0f * rotation.X;
            float y2 = 2.0f * rotation.Y;
            float z2 = 2.0f * rotation.Z;
            float xx = rotation.X * x2;
            float xy = rotation.X * y2;
            float xz = rotation.X * z2;
            float yy = rotation.Y * y2;
            float yz = rotation.Y * z2;
            float zz = rotation.Z * z2;
            float wx = rotation.W * x2;
            float wy = rotation.W * y2;
            float wz = rotation.W * z2;

            //  Setup rotation and inverse rotation
            inverseTransform._00 = transform._00 = 1.0f - yy - zz;
            inverseTransform._01 = transform._10 =        xy + wz;
            inverseTransform._02 = transform._20 =        xz - wy;
            inverseTransform._30 = transform._30 = 0.0f;  //  Not needed ?

            inverseTransform._10 = transform._01 =        xy - wz;
            inverseTransform._11 = transform._11 = 1.0f - xx - zz;
            inverseTransform._12 = transform._21 =        yz + wx;
            inverseTransform._31 = transform._31 = 0.0f;  //  Not needed ?

            inverseTransform._20 = transform._02 =        xz + wy;
            inverseTransform._21 = transform._12 =        yz - wx;
            inverseTransform._22 = transform._22 = 1.0f - xx - yy;
            inverseTransform._32 = transform._32 = 0.0f;  //  Not needed ?

            //  Apply translation
            transform._03 = translation.X;
            transform._13 = translation.Y;
            transform._23 = translation.Z;
            inverseTransform._33 = transform._33 = 1.0f;  //  Not needed ?

            //  Apply inverse translation
            inverseTransform._03 = (inverseTransform._00 * -translation.X) + (inverseTransform._01 * -translation.Y) + (inverseTransform._02 * -translation.Z);
            inverseTransform._13 = (inverseTransform._10 * -translation.X) + (inverseTransform._11 * -translation.Y) + (inverseTransform._12 * -translation.Z);
            inverseTransform._23 = (inverseTransform._20 * -translation.X) + (inverseTransform._21 * -translation.Y) + (inverseTransform._22 * -translation.Z);
        }

        public static void CreateWithInverseFromQuaternionAndTranslationAndScale(
            Neure.Quaternion    rotation,
            Neure.Vector3       translation,
            Neure.Vector3       scale,
            out Neure.Matrix4x4 transform,
            out Neure.Matrix4x4 inverseTransform
        )
        {
            float x2 = 2.0f * rotation.X;
            float y2 = 2.0f * rotation.Y;
            float z2 = 2.0f * rotation.Z;
            float xx = rotation.X * x2;
            float xy = rotation.X * y2;
            float xz = rotation.X * z2;
            float yy = rotation.Y * y2;
            float yz = rotation.Y * z2;
            float zz = rotation.Z * z2;
            float wx = rotation.W * x2;
            float wy = rotation.W * y2;
            float wz = rotation.W * z2;

            //  Transform First Column - InverseTransform First Row
            inverseTransform._00 = transform._00 = 1.0f - yy - zz;
            inverseTransform._01 = transform._10 =        xy + wz;
            inverseTransform._02 = transform._20 =        xz - wy;
            inverseTransform._30 = transform._30 = 0.0f;  //  Not needed ?

            //  Transform Second Column - InverseTransform Second Row
            inverseTransform._10 = transform._01 =        xy - wz;
            inverseTransform._11 = transform._11 = 1.0f - xx - zz;
            inverseTransform._12 = transform._21 =        yz + wx;
            inverseTransform._31 = transform._31 = 0.0f;  //  Not needed ?

            inverseTransform._20 = transform._02 =        xz + wy;
            inverseTransform._21 = transform._12 =        yz - wx;
            inverseTransform._22 = transform._22 = 1.0f - xx - yy;
            inverseTransform._32 = transform._32 = 0.0f;  //  Not needed ?

            //  Apply translation
                                   transform._03 = translation.X;
                                   transform._13 = translation.Y;
                                   transform._23 = translation.Z;
            inverseTransform._33 = transform._33 = 1.0f;  //  Not needed ?

            //  Apply inverse translation
            inverseTransform._03 = (inverseTransform._00 * -translation.X) + (inverseTransform._01 * -translation.Y) + (inverseTransform._02 * -translation.Z);
            inverseTransform._13 = (inverseTransform._10 * -translation.X) + (inverseTransform._11 * -translation.Y) + (inverseTransform._12 * -translation.Z);
            inverseTransform._23 = (inverseTransform._20 * -translation.X) + (inverseTransform._21 * -translation.Y) + (inverseTransform._22 * -translation.Z);

            //  Apply scale to transform
            transform._00 = transform._00 * scale.X;
            transform._10 = transform._10 * scale.Y;
            transform._20 = transform._20 * scale.Z;

            transform._01 = transform._01 * scale.X;
            transform._11 = transform._11 * scale.Y;
            transform._21 = transform._21 * scale.Z;

            transform._02 = transform._02 * scale.X;
            transform._12 = transform._12 * scale.Y;
            transform._22 = transform._22 * scale.Z;

            //  Apply inverse scale to inverse transform
            {
                float isx = 1.0f / scale.X;  //  TODO Division by zero?
                float isy = 1.0f / scale.Y;  //  TODO Division by zero?
                float isz = 1.0f / scale.Z;  //  TODO Division by zero?

                inverseTransform._00 *= isx;
                inverseTransform._10 *= isx;
                inverseTransform._20 *= isx;

                inverseTransform._01 *= isy;
                inverseTransform._11 *= isy;
                inverseTransform._21 *= isy;

                inverseTransform._02 *= isz;
                inverseTransform._12 *= isz;
                inverseTransform._22 *= isz;

                inverseTransform._03 *= isz;
                inverseTransform._13 *= isz;
                inverseTransform._23 *= isz;
            }
        }
        #endregion
#endif
        // \note http://en.wikipedia.org/wiki/Rotation_matrix#Axis_and_angle
        public void ToAxisAngle(out Vector3 axis, out float angle)
        {
            float x = _12 - _21;
            float y = _20 - _02;
            float z = _01 - _10;
            float r = (float)System.Math.Sqrt(x * x + y * y + z * z);
            float t = Trace3;

            angle = (float)System.Math.Atan2(r, t - 1);
            axis  = new Vector3(x, y, z) * angle;

            if(r != 0.0f)
            {
                axis *= (1.0f / r);
            }
        }
        public void ToHPB(out float h, out float p, out float b)
        {
            float cy = (float)System.Math.Sqrt(_11 * _11 + _10 * _10);
            if(cy > float.Epsilon)
            {
                h = (float)System.Math.Atan2( _02, _22);
                p = (float)System.Math.Atan2(-_12,  cy);
                b = (float)System.Math.Atan2( _10, _11);
            }
            else 
            {
                h = (float)System.Math.Atan2(-_20, _00);
                p = (float)System.Math.Atan2(-_12, cy );
                b = 0.0f;
            }
        }

        #region Operators
        public static Vector4 operator*(Matrix4 l, Vector4 r)
        {

            Vector4 p;
            p.X = l._00 * r.X + l._01 * r.Y + l._02 * r.Z + l._03 * r.W;
            p.Y = l._10 * r.X + l._11 * r.Y + l._12 * r.Z + l._13 * r.W;
            p.Z = l._20 * r.X + l._21 * r.Y + l._22 * r.Z + l._23 * r.W;
            p.W = l._30 * r.X + l._31 * r.Y + l._32 * r.Z + l._33 * r.W;
            return p;
        }
        public static Vector3 operator*(Matrix4 l, Vector3 r)
        {
            Vector3 p;
            p.X = l._00 * r.X + l._01 * r.Y + l._02 * r.Z + l._03;
            p.Y = l._10 * r.X + l._11 * r.Y + l._12 * r.Z + l._13;
            p.Z = l._20 * r.X + l._21 * r.Y + l._22 * r.Z + l._23;
            return p;
        }
        public static Matrix4 operator *(Matrix4 l, Matrix4 r)
        {
            Matrix4 p;

            /* _rw  row column  */
            /*  left: rows  */
            /*  right: columns */ 

            /*  First column  */ 

            p._00 = l._00 * r._00  +  l._01 * r._10  +  l._02 * r._20  +  l._03 * r._30;
            p._10 = l._10 * r._00  +  l._11 * r._10  +  l._12 * r._20  +  l._13 * r._30;
            p._20 = l._20 * r._00  +  l._21 * r._10  +  l._22 * r._20  +  l._23 * r._30;
            p._30 = l._30 * r._00  +  l._31 * r._10  +  l._32 * r._20  +  l._33 * r._30;

            /*  Second column  */
            p._01 = l._00 * r._01  +  l._01 * r._11  +  l._02 * r._21  +  l._03 * r._31;
            p._11 = l._10 * r._01  +  l._11 * r._11  +  l._12 * r._21  +  l._13 * r._31;
            p._21 = l._20 * r._01  +  l._21 * r._11  +  l._22 * r._21  +  l._23 * r._31;
            p._31 = l._30 * r._01  +  l._31 * r._11  +  l._32 * r._21  +  l._33 * r._31;

            /*  Third column  */
            p._02 = l._00 * r._02  +  l._01 * r._12  +  l._02 * r._22  +  l._03 * r._32;
            p._12 = l._10 * r._02  +  l._11 * r._12  +  l._12 * r._22  +  l._13 * r._32;
            p._22 = l._20 * r._02  +  l._21 * r._12  +  l._22 * r._22  +  l._23 * r._32;
            p._32 = l._30 * r._02  +  l._31 * r._12  +  l._32 * r._22  +  l._33 * r._32;

            /*  Fourth column  */
            p._03 = l._00 * r._03  +  l._01 * r._13  +  l._02 * r._23  +  l._03 * r._33;
            p._13 = l._10 * r._03  +  l._11 * r._13  +  l._12 * r._23  +  l._13 * r._33;
            p._23 = l._20 * r._03  +  l._21 * r._13  +  l._22 * r._23  +  l._23 * r._33;
            p._33 = l._30 * r._03  +  l._31 * r._13  +  l._32 * r._23  +  l._33 * r._33;
            return p;
        }
        public static Matrix4 operator *(Matrix4 l, float s)
        {
            Matrix4 p;

            p._00 = l._00 * s;
            p._01 = l._01 * s;
            p._02 = l._02 * s;
            p._03 = l._03 * s;

            p._10 = l._10 * s;
            p._11 = l._11 * s;
            p._12 = l._12 * s;
            p._13 = l._13 * s;

            p._20 = l._20 * s;
            p._21 = l._21 * s;
            p._22 = l._22 * s;
            p._23 = l._23 * s;

            p._30 = l._30 * s;
            p._31 = l._31 * s;
            p._32 = l._32 * s;
            p._33 = l._33 * s;
            return p;
        }

        #endregion

        public Vector3 TransformPoint(Vector3 p)
        {
            Vector3 tp;
            tp.X = _00 * p.X + _01 * p.Y + _02 * p.Z + _03;
            tp.Y = _10 * p.X + _11 * p.Y + _12 * p.Z + _13;
            tp.Z = _20 * p.X + _21 * p.Y + _22 * p.Z + _23;
            return tp;
        }
        public Vector3 TransformDirection(Vector3 d)
        {
            Vector3 tp;
            tp.X = _00 * d.X + _01 * d.Y + _02 * d.Z;
            tp.Y = _10 * d.X + _11 * d.Y + _12 * d.Z;
            tp.Z = _20 * d.X + _21 * d.Y + _22 * d.Z;
            return tp;
        }

        public Vector3 UnProject(
            float       winx, 
            float       winy, 
            float       winz, 
            float       viewportX,
            float       viewportY,
            float       viewportWidth,
            float       viewportHeight
        )
        {
            Vector4 @in;
            Vector4 @out;

            @in.X = (winx - viewportX + 0.5f) / viewportWidth  * 2.0f - 1.0f;
            @in.Y = (winy - viewportY + 0.5f) / viewportHeight * 2.0f - 1.0f;
            @in.Z = 2.0f * winz - 1.0f;
            @in.W = 1.0f;

            //  Objects coordinates
            @out = this * @in;
            if(@out.W == 0.0f)
            {
                throw new ArgumentException();
            }

            return new Vector3(@out.X / @out.W, @out.Y / @out.W, @out.Z / @out.W);
        }

        // This is assuming this contains the desired projection matrix
        public Vector2 ProjectToScreenSpace(
            Vector3     positionInWorld, 
            float       viewportX,
            float       viewportY,
            float       viewportWidth,
            float       viewportHeight,
            out float   depthInClip
        )
        {
            //  Apply projection
            Vector4 positionInClip = this * new Vector4(positionInWorld, 1.0f);

            // depthInClip = positionInClip.Z;
            depthInClip = positionInClip.Z / positionInClip.W;

            //  Normalized device coordinate
            Vector3 positionInNDC = Vector4.Homogenize(positionInClip);

            //  window coordinate
            Vector2 positionInScreen = new Vector2(
                (0.5f + (positionInNDC.X * 0.5f)) * viewportWidth  + viewportX,
                (0.5f + (positionInNDC.Y * 0.5f)) * viewportHeight + viewportY
            );

            return positionInScreen;
        }

        public override string ToString()
        {
#if true
            return String.Format(
                "\t{0,3}, {1,3}, {2,3}, {3,3} \n" +
                "\t{4,3}, {5,3}, {6,3}, {7,3} \n" + 
                "\t{8,3}, {9,3}, {10,3}, {11,3} \n" + 
                "\t{12,3}, {13,3}, {14,3}, {15,3} \n",
                _00, _01, _02, _03,
                _10, _11, _12, _13,
                _20, _21, _22, _23,
                _30, _31, _32, _33
            );
#else
            return String.Format(
                "\t{0,3}, {3,3}, {6,3} \n" +
                "\t{1,3}, {4,3}, {7,3} \n" + 
                "\t{2,3}, {5,3}, {8,3} \n",
                _00, _01, _02,
                _10, _11, _12,
                _20, _21, _22
            );
#endif
        }

        private static int[] colIdx = new int[4];
        private static int[] rowIdx = new int[4];
        private static int[] pivotIdx = new int[4];
        private static float[,] inverse = new float[4,4];
        public static void Invert(Matrix4 mat, out Matrix4 result)
        {
            colIdx[0] = 0; colIdx[1] = 0; colIdx[2] = 0; colIdx[3] = 0;
            rowIdx[0] = 0; rowIdx[1] = 0; rowIdx[2] = 0; rowIdx[3] = 0;
            pivotIdx[0] = -1; pivotIdx[1] = -1; pivotIdx[2] = -1; pivotIdx[3] = -1;

            // convert the matrix to an array for easy looping
#if true
            inverse[0,0] = mat._00; inverse[0,1] = mat._01; inverse[0,2] = mat._02; inverse[0,3] = mat._03;
            inverse[1,0] = mat._10; inverse[1,1] = mat._11; inverse[1,2] = mat._12; inverse[1,3] = mat._13;
            inverse[2,0] = mat._20; inverse[2,1] = mat._21; inverse[2,2] = mat._22; inverse[2,3] = mat._23;
            inverse[3,0] = mat._30; inverse[3,1] = mat._31; inverse[3,2] = mat._32; inverse[3,3] = mat._33;
#else
            inverse[0,0] = mat._00; inverse[1,0] = mat._01; inverse[2,0] = mat._02; inverse[3,0] = mat._03;
            inverse[0,1] = mat._00; inverse[1,1] = mat._01; inverse[2,1] = mat._02; inverse[3,1] = mat._03;
            inverse[0,2] = mat._00; inverse[1,2] = mat._01; inverse[2,2] = mat._02; inverse[3,2] = mat._03;
            inverse[0,3] = mat._00; inverse[1,3] = mat._01; inverse[2,3] = mat._02; inverse[3,3] = mat._03;
#endif
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                float maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                float absVal = System.Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                result._00 = mat._00; result._01 = mat._01; result._02 = mat._02; result._03 = mat._03;
                                result._10 = mat._10; result._11 = mat._11; result._12 = mat._12; result._13 = mat._13;
                                result._20 = mat._20; result._21 = mat._21; result._22 = mat._22; result._23 = mat._23;
                                result._30 = mat._30; result._31 = mat._31; result._32 = mat._32; result._33 = mat._33;
                                return;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        float f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                    //return mat;
                }

                // Scale row so it has a unit diagonal
                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    float f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            result._00 = inverse[0, 0]; result._01 = inverse[0, 1]; result._02 = inverse[0, 2]; result._03 = inverse[0, 3];
            result._10 = inverse[1, 0]; result._11 = inverse[1, 1]; result._12 = inverse[1, 2]; result._13 = inverse[1, 3];
            result._20 = inverse[2, 0]; result._21 = inverse[2, 1]; result._22 = inverse[2, 2]; result._23 = inverse[2, 3];
            result._30 = inverse[3, 0]; result._31 = inverse[3, 1]; result._32 = inverse[3, 2]; result._33 = inverse[3, 3];
        }
        public static Matrix4 Invert(Matrix4 mat)
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            // convert the matrix to an array for easy looping
            float[,] inverse = {{mat._00, mat._01, mat._02, mat._03}, 
                                {mat._10, mat._11, mat._12, mat._13}, 
                                {mat._20, mat._21, mat._22, mat._23}, 
                                {mat._30, mat._31, mat._32, mat._33} };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                float maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                float absVal = System.Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                return mat;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        float f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                    //return mat;
                }

                // Scale row so it has a unit diagonal
                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    float f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            mat._00 = inverse[0, 0]; mat._01 = inverse[0, 1]; mat._02 = inverse[0, 2]; mat._03 = inverse[0, 3];
            mat._10 = inverse[1, 0]; mat._11 = inverse[1, 1]; mat._12 = inverse[1, 2]; mat._13 = inverse[1, 3];
            mat._20 = inverse[2, 0]; mat._21 = inverse[2, 1]; mat._22 = inverse[2, 2]; mat._23 = inverse[2, 3];
            mat._30 = inverse[3, 0]; mat._31 = inverse[3, 1]; mat._32 = inverse[3, 2]; mat._33 = inverse[3, 3];
            return mat;
        }
        public static Matrix4 Transpose(Matrix4 mat)
        {
            Matrix4 matrix;
            Transpose(mat, out matrix);
            return matrix;
        }
        public static void Transpose(Matrix4 mat, out Matrix4 result)
        {
            result._00 = mat._00;
            result._10 = mat._01;
            result._20 = mat._02;
            result._30 = mat._03;
            result._01 = mat._10;
            result._11 = mat._11;
            result._21 = mat._12;
            result._31 = mat._13;
            result._02 = mat._20;
            result._12 = mat._21;
            result._22 = mat._22;
            result._32 = mat._23;
            result._03 = mat._30;
            result._13 = mat._31;
            result._23 = mat._32;
            result._33 = mat._33;
        }

        public bool Equals(Matrix4 other)
        {
            return 
                other._00 == _00 &&
                other._10 == _10 &&
                other._20 == _20 &&
                other._30 == _30 &&
                other._01 == _01 &&
                other._11 == _11 &&
                other._21 == _21 &&
                other._31 == _31 &&
                other._02 == _02 &&
                other._12 == _12 &&
                other._22 == _22 &&
                other._32 == _32 &&
                other._03 == _03 &&
                other._13 == _13 &&
                other._23 == _23 &&
                other._33 == _33;
        }
    }
}
