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

namespace RenderStack.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    /*  Comment: Mostly stable. */
    public struct Vector4 : ILinear, IEquatable<Vector4>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public static Vector4 Zero = new Vector4(0, 0, 0, 0);
        public static readonly Vector4 One = new Vector4(1, 1, 1, 1);
        public static Vector4 UnitX = new Vector4(1, 0, 0, 0);
        public static Vector4 UnitY = new Vector4(0, 1, 0, 0);
        public static Vector4 UnitZ = new Vector4(0, 0, 1, 0);
        public static Vector4 UnitW = new Vector4(0, 0, 0, 1);

        public Vector4(float s)
        {
            X = s;
            Y = s;
            Z = s;
            W = s;
        }
        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Vector4(Vector3 v, float w)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = w;
        }
        public Vector4(Vector2 xy, Vector2 zw)
        {
            X = xy.X;
            Y = xy.Y;
            Z = zw.X;
            W = zw.Y;
        }
        public Vector4(Vector2 v, float z, float w)
        {
            X = v.X;
            Y = v.Y;
            Z = z;
            W = w;
        }

        public Vector3 Xxx  { get { return new Vector3(X, X, X); } }
        public Vector4 Xxyy { get { return new Vector4(X, X, Y, Y); } }
        public Vector2 Xy   { get { return new Vector2(X, Y); } set { X = value.X; Y = value.Y; } }
        public Vector3 Xyz  { get { return new Vector3(X, Y, Z); } set { X = value.X; Y = value.Y; Z = value.Z; } }
        public Vector3 Xzx  { get { return new Vector3(X, Z, X); } }
        public Vector4 Xzyw { get { return new Vector4(X, Z, Y, W); } }
        public Vector3 Yyy  { get { return new Vector3(Y, Y, Y); } }
        public Vector4 Yyyy { get { return new Vector4(Y, Y, Y, Y); } }
        public Vector3 Zzz  { get { return new Vector3(Z, Z, Z); } }
        public Vector4 Zzww { get { return new Vector4(Z, Z, W, W); } }
        public Vector2 Zw   { get { return new Vector2(Z, W); } }
        public Vector3 Wyz  { get { return new Vector3(W, Y, Z); } }

        public float Length
        {
            get
            {
                return (float)System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
            }
        }
        public float LengthSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z + W * W;
            }
        }

        public static Vector4 operator -(Vector4 vec)
        {
            return new Vector4(-vec.X, -vec.Y, -vec.Z, -vec.W);
        }
        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }
        public static Vector4 operator *(float scale, Vector4 vec)
        {
            return new Vector4(scale * vec.X, scale * vec.Y, scale * vec.Z, scale * vec.W);
        }
        public static Vector4 operator *(Vector4 vec, float scale)
        {
            return new Vector4(scale * vec.X, scale * vec.Y, scale * vec.Z, scale * vec.W);
        }
        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }
        public static Vector4 operator /(Vector4 vec, float scale)
        {
            return new Vector4(vec.X / scale, vec.Y / scale, vec.Z / scale, vec.W / scale);
        }
        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        public static Vector4 Min(Vector4 a, Vector4 b)
        {
            return new Vector4(
                a.X < b.X ? a.X : b.X,
                a.Y < b.Y ? a.Y : b.Y,
                a.Z < b.Z ? a.Z : b.Z,
                a.W < b.W ? a.W : b.W
            );
        }
        public static Vector4 Max(Vector4 a, Vector4 b)
        {
            return new Vector4(
                a.X > b.X ? a.X : b.X,
                a.Y > b.Y ? a.Y : b.Y,
                a.Z > b.Z ? a.Z : b.Z,
                a.W > b.W ? a.W : b.W
            );
        }
        public static Vector4 Abs(Vector4 a)
        {
            return new Vector4(
                System.Math.Abs(a.X),
                System.Math.Abs(a.Y),
                System.Math.Abs(a.Z),
                System.Math.Abs(a.W)
            );
        }
        public static Vector4 Mix(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(
                t * (b.X - a.X) + a.X,
                t * (b.Y - a.Y) + a.Y,
                t * (b.Z - a.Z) + a.Z,
                t * (b.W - a.W) + a.W
            );
        }
        public static Vector4 BaryCentric(Vector4 a, Vector4 b, Vector4 c, float u, float v)
        {
            return a + u * (b - a) + v * (c - a);
        }
        public static Vector4 Clamp(Vector4 v, float min, float max)
        {
            if(v.X < min) v.X = min;
            if(v.X > max) v.X = max;
            if(v.Y < min) v.Y = min;
            if(v.Y > max) v.Y = max;
            if(v.Z < min) v.Z = min;
            if(v.Z > max) v.Z = max;
            if(v.W < min) v.W = min;
            if(v.W > max) v.W = max;
            return v;
        }
        public static Vector4 Clamp(Vector4 vec, Vector4 min, Vector4 max)
        {
            vec.X = vec.X < min.X ? min.X : vec.X > max.X ? max.X : vec.X;
            vec.Y = vec.Y < min.Y ? min.Y : vec.Y > max.Y ? max.Y : vec.Y;
            vec.Z = vec.Z < min.Z ? min.Z : vec.Z > max.Z ? max.Z : vec.Z;
            vec.W = vec.W < min.W ? min.W : vec.W > max.W ? max.W : vec.W;
            return vec;
        }
        public static Vector4 Step(Vector4 edge, Vector4 x)
        {
            return new Vector4(
                x.X < edge.X ? 0.0f : 1.0f,
                x.Y < edge.Y ? 0.0f : 1.0f,
                x.Z < edge.Z ? 0.0f : 1.0f,
                x.W < edge.W ? 0.0f : 1.0f
            );
        }
        public static Vector4 Floor(Vector4 v)
        {
            return new Vector4(
                (float)System.Math.Floor(v.X),
                (float)System.Math.Floor(v.Y),
                (float)System.Math.Floor(v.Z),
                (float)System.Math.Floor(v.W)
            );
        }
        /*public static float Length(Vector4 v)
        {
            return (float)(Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z + v.W * v.W));
        }
        public static float LengthSquared(Vector4 v)
        {
            return (float)(Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z + v.W * v.W));
        }*/
        public static Vector4 Normalize(Vector4 v)
        {
            float length = v.Length;
            return new Vector4(v.X / length, v.Y / length, v.Z / length, v.W / length);
        }
        public static Vector3 Homogenize(Vector4 v)
        {
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }
        public static float Dot(Vector4 left, Vector4 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        }
        public static Vector4 Cross(Vector4 x, Vector4 y, Vector4 z)
        {
            return new Vector4(
                x.Y * y.Z * z.W + x.Z * y.W * z.Y + x.W * y.Y * z.Z - x.Y * y.W * z.Z - x.Z * y.Y * z.W - x.W * y.Z * z.Y,
                x.X * y.W * z.Z + x.Z * y.X * z.W + x.W * y.Z * z.X - x.X * y.Z * z.W - x.Z * y.W * z.X - x.W * y.X * z.Z,
                x.X * y.Y * z.W + x.Y * y.W * z.X + x.W * y.X * z.Y - x.X * y.W * z.Y - x.Y * y.X * z.W - x.W * y.Y * z.X,
                x.X * y.Z * z.Y + x.Y * y.X * z.Z + x.Z * y.Y * z.X - x.X * y.Y * z.Z - x.Y * y.Z * z.X - x.Z * y.X * z.Y
            );
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", X, Y, Z, W);
        }

        public ILinear PlusWeightTimesOther(float weight, ILinear other)
        {
            this += weight * (Vector4)other;
            return this;
        }

        public static bool operator==(Vector4 a, Vector4 b)
        {
            return
                (a.X == b.X) &&
                (a.Y == b.Y) &&
                (a.Z == b.Z) &&
                (a.W == b.W);
        }
        public static bool operator!=(Vector4 a, Vector4 b)
        {
            return
                (a.X != b.X) ||
                (a.Y != b.Y) ||
                (a.Z != b.Z) ||
                (a.W != b.W);
        }
        public bool Equals(Vector4 other)
        {
            return
                (X == other.X) &&
                (Y == other.Y) &&
                (Z == other.Z) &&
                (W == other.W);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        }
        public override bool Equals(object o)
        {
            if(o is Vector4)
            {
                Vector4 c = (Vector4)o;
                return this == c;
            }
            return false;
        }
		
    }
}
