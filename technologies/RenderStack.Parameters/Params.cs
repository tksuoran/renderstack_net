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

#if false
using System;
using System.Runtime.InteropServices;

using RenderStack.Math;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Parameters
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]

    public class Params<T>
    {
        public T[]  Value;
        public int  Elements;
        public int  Dimension;
        public T    X { get { return Value[0]; } set { Value[0] = value; } }
        public T    Y { get { return Value[1]; } set { Value[1] = value; } }
        public T    Z { get { return Value[2]; } set { Value[2] = value; } }
        public T    W { get { return Value[3]; } set { Value[3] = value; } }

        public T this[int index, int component]
        {
            get
            {
                return Value[index * Elements + component];
            }
            set
            {
                Value[index * Elements + component] = value;
            }
        }
        public T this[int index]
        {
            get { return Value[index]; }
            set { Value[index] = value; }
        }

        public Params()
        {
            Value = new T[1 * 1];
            Elements = 1;
            Dimension = 1;
        }

        public Params(int elements, int dimension)
        {
            Value = new T[dimension * elements];
            Elements = elements;
            Dimension = dimension;
        }
        public Params(T x)
        {
            Value = new T[1 * 1];
            Elements = 1;
            Dimension = 1;
            X = x;
        }
        public Params(T x, T y)
        {
            Value = new T[1 * 2];
            Elements = 2;
            Dimension = 1;
            X = x;
            Y = y;
        }
        public Params(T x, T y, T z)
        {
            Value = new T[1 * 3];
            Elements = 3;
            Dimension = 1;
            X = x;
            Y = y;
            Z = z;
        }
        public Params(T x, T y, T z, T w)
        {
            Value = new T[1 * 4];
            Elements = 4;
            Dimension = 1;
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Set(T x)
        {
            X = x;
        }
        public void Set(T x, T y)
        {
            X = x;
            Y = y;
        }
        public void Set(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public void Set(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public void Set(int index, T x)
        {
            Value[index * Elements + 0] = x;
        }
        public void Set(int index, T x, T y)
        {
            Value[index * Elements + 0] = x;
            Value[index * Elements + 1] = y;
        }
        public void Set(int index, T x, T y, T z)
        {
            Value[index * Elements + 0] = x;
            Value[index * Elements + 1] = y;
            Value[index * Elements + 2] = z;
        }
        public void Set(int index, T x, T y, T z, T w)
        {
            Value[index * Elements + 0] = x;
            Value[index * Elements + 1] = y;
            Value[index * Elements + 2] = z;
            Value[index * Elements + 3] = w;
        }
    }

    public class Ints : Params<int>
    {
        public Ints():base(1,1){}
        public Ints(int elements, int dimension):base(elements, dimension){}
        public Ints(int x):base(x){}
        public void Apply(int slot)
        {
            unsafe
            {
                fixed(int* ptr = &Value[0])
                {
                    switch(Elements)
                    {
                        case  1: GL.Uniform1(slot, Dimension, ptr); break;
                        case  2: GL.Uniform2(slot, Dimension, ptr); break;
                        case  3: GL.Uniform3(slot, Dimension, ptr); break;
                        case  4: GL.Uniform4(slot, Dimension, ptr); break;
                    }
                }
            }
        }
    }
    public class UInts : Params<uint>
    {
        public UInts():base(1,1){}
        public UInts(int elements, int dimension):base(elements, dimension){}
        public UInts(uint x):base(x){}
        public void Apply(int slot)
        {
            unsafe
            {
                fixed(uint* ptr = &Value[0])
                {
                    switch(Elements)
                    {
                        case  1: GL.Uniform1(slot, Dimension, ptr); break;
                        case  2: GL.Uniform2(slot, Dimension, ptr); break;
                        case  3: GL.Uniform3(slot, Dimension, ptr); break;
                        case  4: GL.Uniform4(slot, Dimension, ptr); break;
                    }
                }
            }
        }
    }
    public class Floats : Params<float>
    {
        public Floats():base(1,1){}
        public Floats(int elements, int dimension):base(elements, dimension){}
        public Floats(float x):base(x){}
        public Floats(float x, float y):base(x,y){}
        public Floats(float x, float y, float z):base(x,y,z){}
        public Floats(float x, float y, float z, float w):base(x,y,z,w){}
        public void Set(Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }
        public void Set(Vector3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }
        public void Set(Vector4 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }
        public void Set(int index, Vector2 v)
        {
            Value[index * Elements + 0] = v.X;
            Value[index * Elements + 1] = v.Y;
        }
        public void Set(int index, Vector3 v)
        {
            Value[index * Elements + 0] = v.X;
            Value[index * Elements + 1] = v.Y;
            Value[index * Elements + 2] = v.Z;
        }
        public void Set(int index, Vector4 v)
        {
            Value[index * Elements + 0] = v.X;
            Value[index * Elements + 1] = v.Y;
            Value[index * Elements + 2] = v.Z;
            Value[index * Elements + 3] = v.W;
        }
        public void Apply(int slot)
        {
            unsafe
            {
                fixed(float* ptr = &Value[0])
                {
                    switch(Elements)
                    {
                        case  1: GL.Uniform1        (slot, Dimension, ptr); break;
                        case  2: GL.Uniform2        (slot, Dimension, ptr); break;
                        case  3: GL.Uniform3        (slot, Dimension, ptr); break;
                        case  4: GL.Uniform4        (slot, Dimension, ptr); break;
                        case 16: GL.UniformMatrix4  (slot, Dimension, false, ptr); break;
                    }
                    
                }
            }
        }
        public void Set(Matrix4 m)
        {
            Set(0, m);
        }
        public void Set(int index, Matrix4 m)
        {
            if(Elements != 16)
            {
                throw new ArgumentException("Set(Matrix4) with Elements != 16");
            }

            Value[index * Elements +  0] = m._00;
            Value[index * Elements +  1] = m._01;
            Value[index * Elements +  2] = m._02;
            Value[index * Elements +  3] = m._03;
            Value[index * Elements +  4] = m._10;
            Value[index * Elements +  5] = m._11;
            Value[index * Elements +  6] = m._12;
            Value[index * Elements +  7] = m._13;
            Value[index * Elements +  8] = m._20;
            Value[index * Elements +  9] = m._21;
            Value[index * Elements + 10] = m._22;
            Value[index * Elements + 11] = m._23;
            Value[index * Elements + 12] = m._30;
            Value[index * Elements + 13] = m._33;
            Value[index * Elements + 14] = m._32;
            Value[index * Elements + 15] = m._33;
        }
    }
}
#endif