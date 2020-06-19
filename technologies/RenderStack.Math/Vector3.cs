using System;
using System.Runtime.InteropServices;

namespace RenderStack.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    /*  Comment: Mostly stable. */
    public struct Vector3 : ILinear, IEquatable<Vector3>
    {
        public float X;
        public float Y;
        public float Z;

        public static Vector3 Zero  = new Vector3(0, 0, 0);
        public static Vector3 One   = new Vector3(1, 1, 1);
        public static Vector3 UnitX = new Vector3(1, 0, 0);
        public static Vector3 UnitY = new Vector3(0, 1, 0);
        public static Vector3 UnitZ = new Vector3(0, 0, 1);
        public static Vector3 MinValue = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
        public static Vector3 MaxValue = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);

        public Vector3(float s)
        {
            X = s;
            Y = s;
            Z = s;
        }
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3(double x, double y, double z)
        {
            X = (float)(x);
            Y = (float)(y);
            Z = (float)(z);
        }
        public Vector3(Vector3 p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }
        public Vector3(Vector2 xy, float z)
        {
            X = xy.X;
            Y = xy.Y;
            Z = z;
        }

        public Vector2 Xy   { get { return new Vector2(X, Y); } }
        public Vector3 Xyz  { get { return this; } }
        public Vector3 Xxx  { get { return new Vector3(X, X, X); } }
        public Vector3 Yyy  { get { return new Vector3(Y, Y, Y); } }
        public Vector4 Yyyy { get { return new Vector4(Y, Y, Y, Y); } }
        public Vector3 Yzx  { get { return new Vector3(Y, Z, X); } }
        public Vector3 Zxy  { get { return new Vector3(Z, X, Y); } }
        public Vector3 Zzz  { get { return new Vector3(Z, Z, Z); } }

        public float Length
        {
            get
            {
                return (float)System.Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }
        public float LengthSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public static Vector3 Vector3FromUint(uint i)
        {
            uint r = (i >> 16) & 0xff;
            uint g = (i >>  8) & 0xff;
            uint b = (i >>  0) & 0xff;

            return new Vector3(
                r / 255.0f,
                g / 255.0f,
                b / 255.0f
            );
        }

        public static uint UintFromVector3(Vector3 v)
        {
            float   rf  = v.X * 255.0f;
            float   gf  = v.Y * 255.0f;
            float   bf  = v.Z * 255.0f;
            uint    r   = (uint)System.Math.Round(rf) << 16;
            uint    g   = (uint)System.Math.Round(gf) <<  8;
            uint    b   = (uint)System.Math.Round(bf) <<  0;
            uint    i   = r | g | b;

            return i;
        }

        public float Distance(Vector3 v)
        {
            Vector3 d = v - this;
            return d.Length;
        }
        public float DistanceSquared(Vector3 v)
        {
            Vector3 d = v - this;
            return d.LengthSquared;
        }
        public static float PointLineDistance(Vector3 x0, Vector3 x1, Vector3 x2)
        {
            Vector3 x1_x0   = x1 - x0;
            Vector3 x2_x1   = x2 - x1;

            float a   = x1_x0.LengthSquared * x2_x1.LengthSquared;
            float dot = Dot(x1_x0, x2_x1);
            return (a - dot * dot) / x2_x1.LengthSquared;
        }
        public static Vector3 operator -(Vector3 vec)
        {
            return new Vector3(-vec.X, -vec.Y, -vec.Z);
        }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3 operator *(float scale, Vector3 vec)
        {
            return new Vector3(scale * vec.X, scale * vec.Y, scale * vec.Z);
        }
        public static Vector3 operator *(Vector3 vec, float scale)
        {
            return new Vector3(scale * vec.X, scale * vec.Y, scale * vec.Z);
        }
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        public static Vector3 operator /(Vector3 vec, float scale)
        {
            return new Vector3(vec.X / scale, vec.Y / scale, vec.Z / scale);
        }
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.X < b.X ? a.X : b.X,
                a.Y < b.Y ? a.Y : b.Y,
                a.Z < b.Z ? a.Z : b.Z
            );
        }
        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.X > b.X ? a.X : b.X,
                a.Y > b.Y ? a.Y : b.Y,
                a.Z > b.Z ? a.Z : b.Z
            );
        }
        public static Vector3 Mix(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                t * (b.X - a.X) + a.X,
                t * (b.Y - a.Y) + a.Y,
                t * (b.Z - a.Z) + a.Z
            );
        }
        public static Vector3 Clamp(Vector3 v, float min, float max)
        {
            if(v.X < min) v.X = min;
            if(v.X > max) v.X = max;
            if(v.Y < min) v.Y = min;
            if(v.Y > max) v.Y = max;
            if(v.Z < min) v.Z = min;
            if(v.Z > max) v.Z = max;
            return v;
        }
        public static Vector3 Clamp(Vector3 vec, Vector3 min, Vector3 max)
        {
            vec.X = vec.X < min.X ? min.X : vec.X > max.X ? max.X : vec.X;
            vec.Y = vec.Y < min.Y ? min.Y : vec.Y > max.Y ? max.Y : vec.Y;
            vec.Z = vec.Z < min.Z ? min.Z : vec.Z > max.Z ? max.Z : vec.Z;
            return vec;
        }
        public static Vector3 Step(Vector3 edge, Vector3 x)
        {
            return new Vector3(
                x.X < edge.X ? 0.0f : 1.0f,
                x.Y < edge.Y ? 0.0f : 1.0f,
                x.Z < edge.Z ? 0.0f : 1.0f
            );
        }
        public static Vector3 Floor(Vector3 v)
        {
            return new Vector3(
                (float)System.Math.Floor(v.X),
                (float)System.Math.Floor(v.Y),
                (float)System.Math.Floor(v.Z)
            );
        }
        public static IVector3 IFloor(Vector3 v)
        {
            return new IVector3(
                v.X < 0 ? ((int)v.X - 1) : ((int)v.X),
                v.Y < 0 ? ((int)v.Y - 1) : ((int)v.Y),
                v.Z < 0 ? ((int)v.Z - 1) : ((int)v.Z)
            );
        }
        public static Vector3 Normalize(Vector3 v)
        {
            float length = v.Length;
            if(length < float.Epsilon)
            {
                length = 1.0f;
            }
            return new Vector3(v.X / length, v.Y / length, v.Z / length);
        }
        public static float Dot(Vector3 left, Vector3 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }
        public static void Dot(ref Vector3 left, ref Vector3 right, out float res)
        {
            res = left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }
        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            return new Vector3(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X
            );
        }
        public Vector3 MaxAxis
        {
            get
            {
                if(System.Math.Abs(X) >= System.Math.Abs(Y) && System.Math.Abs(X) >= System.Math.Abs(Z))
                {
                    return UnitX;
                }
                if(System.Math.Abs(Y) >= System.Math.Abs(X) && System.Math.Abs(Y) >= System.Math.Abs(Z))
                {
                    return UnitY;
                }
                return UnitZ;
            }
        }
        public Vector3 MinAxis
        {
            get
            {
                if(System.Math.Abs(X) <= System.Math.Abs(Y) && System.Math.Abs(X) <= System.Math.Abs(Z))
                {
                    return UnitX;
                }
                if(System.Math.Abs(Y) <= System.Math.Abs(X) && System.Math.Abs(Y) <= System.Math.Abs(Z))
                {
                    return UnitY;
                }
                return UnitZ;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        public ILinear PlusWeightTimesOther(float weight, ILinear other)
        {
            this += weight * (Vector3)other;
            return this;
        }

        public static bool operator==(Vector3 a, Vector3 b)
        {
            return (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z);
        }

        public static bool operator!=(Vector3 a, Vector3 b)
        {
            return (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        bool IEquatable<Vector3>.Equals(Vector3 o) => this == o;

        public override bool Equals(object obj)
        {
            if (obj is Vector3 c)
            {
                return this == c;
            }
            return false;
        }

    }
}
