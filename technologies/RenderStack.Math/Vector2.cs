using System;
using System.Runtime.InteropServices;

namespace RenderStack.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    /*  Comment: Mostly stable. */
    public struct Vector2 : ILinear
    {
        public float X;
        public float Y;

        public static Vector2 Zero  = new Vector2(0, 0);
        public static Vector2 One   = new Vector2(1, 1);
        public static Vector2 UnitX = new Vector2(1, 0);
        public static Vector2 UnitY = new Vector2(0, 1);

        public Vector2(float s)
        {
            X = s;
            Y = s;
        }
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vector2(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }

        public Vector3 Xxx { get { return new Vector3(X, X, X); } }
        public Vector3 Yyy { get { return new Vector3(Y, Y, Y); } }

        public float Length
        {
            get
            {
                return (float)System.Math.Sqrt(X * X + Y * Y);
            }
        }
        public float LengthSquared
        {
            get
            {
                return X * X + Y * Y;
            }
        }

        public float Distance(Vector2 v)
        {
            Vector2 d = v - this;
            return d.Length;
        }
        public float DistanceSquared(Vector2 v)
        {
            Vector2 d = v - this;
            return d.LengthSquared;
        }
        public static Vector2 operator -(Vector2 vec)
        {
            return new Vector2(-vec.X, -vec.Y);
        }
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X - right.X, left.Y - right.Y);
        }
        public static Vector2 operator *(float scale, Vector2 vec)
        {
            return new Vector2(scale * vec.X, scale * vec.Y);
        }
        public static Vector2 operator *(Vector2 vec, float scale)
        {
            return new Vector2(scale * vec.X, scale * vec.Y);
        }
        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X * right.X, left.Y * right.Y);
        }
        public static Vector2 operator /(Vector2 vec, float scale)
        {
            return new Vector2(vec.X / scale, vec.Y / scale);
        }
        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.X < b.X ? a.X : b.X,
                a.Y < b.Y ? a.Y : b.Y
            );
        }
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.X > b.X ? a.X : b.X,
                a.Y > b.Y ? a.Y : b.Y
            );
        }
        public static Vector2 Mix(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(
                t * (b.X - a.X) + a.X,
                t * (b.Y - a.Y) + a.Y
            );
        }
        public static Vector2 Clamp(Vector2 v, float min, float max)
        {
            if(v.X < min) v.X = min;
            if(v.X > max) v.X = max;
            if(v.Y < min) v.Y = min;
            if(v.Y > max) v.Y = max;
            return v;
        }
        public static Vector2 Clamp(Vector2 vec, Vector2 min, Vector2 max)
        {
            vec.X = vec.X < min.X ? min.X : vec.X > max.X ? max.X : vec.X;
            vec.Y = vec.Y < min.Y ? min.Y : vec.Y > max.Y ? max.Y : vec.Y;
            return vec;
        }
        public static Vector2 Step(Vector2 edge, Vector2 x)
        {
            return new Vector2(
                x.X < edge.X ? 0.0f : 1.0f,
                x.Y < edge.Y ? 0.0f : 1.0f
            );
        }
        public static Vector2 Floor(Vector2 v)
        {
            return new Vector2(
                (float)System.Math.Floor(v.X),
                (float)System.Math.Floor(v.Y)
            );
        }
        public static Vector2 Normalize(Vector2 v)
        {
            float length = v.Length;
            return new Vector2(v.X / length, v.Y / length);
        }
        public static float Dot(Vector2 left, Vector2 right)
        {
            return left.X * right.X + left.Y * right.Y;
        }
        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public ILinear PlusWeightTimesOther(float weight, ILinear other)
        {
            this += weight * (Vector2)other;
            return this;
        }
    }
}
