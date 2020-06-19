using System;
using System.Runtime.InteropServices;

namespace RenderStack.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    /// \note Somewhat experimental
    public struct IVector3 : IEquatable<IVector3>
    {
        public int X;
        public int Y;
        public int Z;

        public static IVector3 Zero  = new IVector3(0, 0, 0);
        public static IVector3 One   = new IVector3(1, 1, 1);
        public static IVector3 UnitX = new IVector3(1, 0, 0);
        public static IVector3 UnitY = new IVector3(0, 1, 0);
        public static IVector3 UnitZ = new IVector3(0, 0, 1);
        public static IVector3 MinValue = new IVector3(int.MinValue, int.MinValue, int.MinValue);
        public static IVector3 MaxValue = new IVector3(int.MaxValue, int.MaxValue, int.MaxValue);

        public IVector3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public IVector3(IVector3 p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        /*public IVector3(IVector2 xy, int z)
        {
            X = xy.X;
            Y = xy.Y;
            Z = z;
        }*/

        public Vector2 Xy
        {
            get
            {
                return new Vector2(X, Y);
            }
        }

        public int LengthSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public int DistanceSquared(IVector3 v)
        {
            IVector3 d = v - this;
            return d.LengthSquared;
        }
        public static IVector3 operator -(IVector3 vec)
        {
            return new IVector3(-vec.X, -vec.Y, -vec.Z);
        }
        public static IVector3 operator -(IVector3 left, IVector3 right)
        {
            return new IVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static IVector3 operator *(int scale, IVector3 vec)
        {
            return new IVector3(scale * vec.X, scale * vec.Y, scale * vec.Z);
        }
        public static IVector3 operator *(IVector3 vec, int scale)
        {
            return new IVector3(scale * vec.X, scale * vec.Y, scale * vec.Z);
        }
        public static IVector3 operator *(IVector3 left, IVector3 right)
        {
            return new IVector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        public static IVector3 operator /(IVector3 vec, int scale)
        {
            return new IVector3(vec.X / scale, vec.Y / scale, vec.Z / scale);
        }
        public static IVector3 operator +(IVector3 left, IVector3 right)
        {
            return new IVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static IVector3 Abs(IVector3 a)
        {
            return new IVector3(
                System.Math.Abs(a.X),
                System.Math.Abs(a.Y),
                System.Math.Abs(a.Z)
            );
        }
        public static IVector3 Sign(IVector3 a)
        {
            return new IVector3(
                System.Math.Sign(a.X),
                System.Math.Sign(a.Y),
                System.Math.Sign(a.Z)
            );
        }
        public static IVector3 Min(IVector3 a, IVector3 b)
        {
            return new IVector3(
                a.X < b.X ? a.X : b.X,
                a.Y < b.Y ? a.Y : b.Y,
                a.Z < b.Z ? a.Z : b.Z
            );
        }
        public static IVector3 Max(IVector3 a, IVector3 b)
        {
            return new IVector3(
                a.X > b.X ? a.X : b.X,
                a.Y > b.Y ? a.Y : b.Y,
                a.Z > b.Z ? a.Z : b.Z
            );
        }
        public static IVector3 Mix(IVector3 a, IVector3 b, int t)
        {
            return new IVector3(
                t * (b.X - a.X) + a.X,
                t * (b.Y - a.Y) + a.Y,
                t * (b.Z - a.Z) + a.Z
            );
        }
        public static IVector3 Clamp(IVector3 v, int min, int max)
        {
            if(v.X < min) v.X = min;
            if(v.X > max) v.X = max;
            if(v.Y < min) v.Y = min;
            if(v.Y > max) v.Y = max;
            if(v.Z < min) v.Z = min;
            if(v.Z > max) v.Z = max;
            return v;
        }
        public static IVector3 Clamp(IVector3 vec, IVector3 min, IVector3 max)
        {
            vec.X = vec.X < min.X ? min.X : vec.X > max.X ? max.X : vec.X;
            vec.Y = vec.Y < min.Y ? min.Y : vec.Y > max.Y ? max.Y : vec.Y;
            vec.Z = vec.Z < min.Z ? min.Z : vec.Z > max.Z ? max.Z : vec.Z;
            return vec;
        }
        public static int Dot(IVector3 left, IVector3 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }
        public static IVector3 Cross(IVector3 left, IVector3 right)
        {
            return new IVector3(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X
            );
        }
        public IVector3 MaxAxis
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
        public IVector3 MinAxis
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
            return String.Format("({0}, {1}, {2})", X, Y, Z);
        }

        public static bool operator==(IVector3 a, IVector3 b)
        {
            return (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z);
        }

        public static bool operator!=(IVector3 a, IVector3 b)
        {
            return (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        bool IEquatable<IVector3>.Equals(IVector3 o)
        {
            return this == o;
        }

        public override bool Equals(object obj)
        {
            if (obj is IVector3 c)
            {
                return this == c;
            }
            return false;
        }

    }
}
