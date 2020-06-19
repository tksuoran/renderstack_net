
using System;
using System.ComponentModel;
using System.Text;

namespace RenderStack.Math
{
    [Serializable]
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector3 Xyz { get { return new Vector3(X, Y, Z); } set { X = value.X; Y = value.Y; Z = value.Z; } }

        private static Quaternion identity = new Quaternion(0, 0, 0, 1);
        public static Quaternion Identity { get{ return identity; } }

        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
        public Quaternion(Vector3 v, float s)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
            this.W = s;
        }

        public static Quaternion Add(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z, q1.W + q2.W);
        }
        public static void Add(ref Quaternion q1, ref Quaternion q2, out Quaternion result)
        {
            result.W = q1.W + q2.W;
            result.X = q1.X + q2.X;
            result.Y = q1.Y + q2.Y;
            result.Z = q1.Z + q2.Z;
        }

        public static Quaternion Concatenate(Quaternion q1, Quaternion q2)
        {
            Quaternion quaternion;
            quaternion.X = ((q2.X * q1.W) + (q1.X * q2.W)) + (q2.Y * q1.Z) - (q2.Z * q1.Y);
            quaternion.Y = ((q2.Y * q1.W) + (q1.Y * q2.W)) + (q2.Z * q1.X) - (q2.X * q1.Z);
            quaternion.Z = ((q2.Z * q1.W) + (q1.Z * q2.W)) + (q2.X * q1.Y) - (q2.Y * q1.X);
            quaternion.W = (q2.W * q1.W) - ((q2.X * q1.X) + (q2.Y * q1.Y)) + (q2.Z * q1.Z);
            return quaternion;
        }
        public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
        {
            result.X = ((value2.X * value1.W) + (value1.X * value2.W)) + (value2.Y * value1.Z) - (value2.Z * value1.Y);
            result.Y = ((value2.Y * value1.W) + (value1.Y * value2.W)) + (value2.Z * value1.X) - (value2.X * value1.Z);
            result.Z = ((value2.Z * value1.W) + (value1.Z * value2.W)) + (value2.X * value1.Y) - (value2.Y * value1.X);
            result.W = (value2.W * value1.W) - ((value2.X * value1.X) + (value2.Y * value1.Y)) + (value2.Z * value1.Z);
        }

        public void ToAxisAngle(out Vector3 axis, out float angle)
        {
            Vector4 result = ToAxisAngle();
            axis = result.Xyz;
            angle = result.W;
        }
        public Vector4 ToAxisAngle()
        {
            Quaternion q = this;
            if(System.Math.Abs(q.W) > 1.0f)
            {
                q.Normalize();
            }

            Vector4 result = new Vector4();

            result.W = 2.0f * (float)System.Math.Acos(q.W);  // angle
            float den = (float)System.Math.Sqrt(1.0 - q.W * q.W);
            if(den > 0.0001f)
            {
                result.Xyz = q.Xyz / den;
            }
            else
            {
                result.Xyz = Vector3.UnitX;
            }

            return result;
        }


        public void Conjugate()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }
        public static Quaternion Conjugate(Quaternion value)
        {
            Quaternion q;
            q.X = -value.X;
            q.Y = -value.Y;
            q.Z = -value.Z;
            q.W = value.W;
            return q;
        }
        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
        }

        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion quaternion;
            quaternion.X = (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f))) + (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f)));
            quaternion.Y = (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f))) - (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f)));
            quaternion.Z = (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f))) - (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f)));
            quaternion.W = (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f))) + (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f)));
            return quaternion;
        }
        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            result.X = (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f))) + (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f)));
            result.Y = (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f))) - (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f)));
            result.Z = (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f))) - (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f)));
            result.W = (((float)System.Math.Cos((double)(yaw * 0.5f)) * (float)System.Math.Cos((double)(pitch * 0.5f))) * (float)System.Math.Cos((double)(roll * 0.5f))) + (((float)System.Math.Sin((double)(yaw * 0.5f)) * (float)System.Math.Sin((double)(pitch * 0.5f))) * (float)System.Math.Sin((double)(roll * 0.5f)));
        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float sinHalfAngle = (float)System.Math.Sin(angle / 2.0f);
            return new Quaternion(axis.X * sinHalfAngle, axis.Y * sinHalfAngle, axis.Z * sinHalfAngle, (float)System.Math.Cos(angle / 2.0f));
        }
        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            float sinHalfAngle = (float)System.Math.Sin(angle / 2.0f);
            result.X = axis.X * sinHalfAngle;
            result.Y = axis.Y * sinHalfAngle;
            result.Z = axis.Z * sinHalfAngle;
            result.W = (float)System.Math.Cos(angle / 2.0f);
        }

        public static Quaternion CreateFromRotationMatrix(Matrix4 matrix)
        {
            Quaternion result;
            if((matrix._00 + matrix._11 + matrix._22) > 0.0F)
            {
                float M1 = (float)System.Math.Sqrt((double)(matrix._00 + matrix._11 + matrix._22 + 1.0F));
                result.W = M1 * 0.5F;
                M1 = 0.5F / M1;
                result.X = (matrix._12 - matrix._21) * M1;
                result.Y = (matrix._20 - matrix._02) * M1;
                result.Z = (matrix._01 - matrix._10) * M1;
                return result;
            }
            if((matrix._00 >= matrix._11) && (matrix._00 >= matrix._22))
            {
                float M2 = (float)System.Math.Sqrt((double)(1.0F + matrix._00 - matrix._11 - matrix._22));
                float M3 = 0.5F / M2;
                result.X = 0.5F * M2;
                result.Y = (matrix._01 + matrix._10) * M3;
                result.Z = (matrix._02 + matrix._20) * M3;
                result.W = (matrix._12 - matrix._21) * M3;
                return result;
            }
            if(matrix._11 > matrix._22)
            {
                float M4 = (float)System.Math.Sqrt((double)(1.0F + matrix._11 - matrix._00 - matrix._22));
                float M5 = 0.5F / M4;
                result.X = (matrix._10 + matrix._01) * M5;
                result.Y = 0.5F * M4;
                result.Z = (matrix._21 + matrix._12) * M5;
                result.W = (matrix._20 - matrix._02) * M5;
                return result;
            }
            float M6 = (float)System.Math.Sqrt((double)(1.0F + matrix._22 - matrix._00 - matrix._11));
            float M7 = 0.5F / M6;
            result.X = (matrix._20 + matrix._02) * M7;
            result.Y = (matrix._21 + matrix._12) * M7;
            result.Z = 0.5F * M6;
            result.W = (matrix._01 - matrix._10) * M7;
            return result;
        }
        public static void CreateFromRotationMatrix(ref Matrix4 m, out Quaternion result)
        {
            if((m._00 + m._11 + m._22) > 0.0F)
            {
                float M1 = (float)System.Math.Sqrt((double)(m._00 + m._11 + m._22 + 1.0F));
                result.W = M1 * 0.5F;
                M1 = 0.5F / M1;
                result.X = (m._12 - m._21) * M1;
                result.Y = (m._20 - m._02) * M1;
                result.Z = (m._01 - m._10) * M1;
                return;
            }
            if((m._00 >= m._11) && (m._00 >= m._22))
            {
                float M2 = (float)System.Math.Sqrt((double)(1.0F + m._00 - m._11 - m._22));
                float M3 = 0.5F / M2;
                result.X = 0.5F * M2;
                result.Y = (m._01 + m._10) * M3;
                result.Z = (m._02 + m._20) * M3;
                result.W = (m._12 - m._21) * M3;
                return;
            }
            if(m._11 > m._22)
            {
                float M4 = (float)System.Math.Sqrt((double)(1.0F + m._11 - m._00 - m._22));
                float M5 = 0.5F / M4;
                result.X = (m._10 + m._01) * M5;
                result.Y = 0.5F * M4;
                result.Z = (m._21 + m._12) * M5;
                result.W = (m._20 - m._02) * M5;
                return;
            }
            float M6 = (float)System.Math.Sqrt((double)(1.0F + m._22 - m._00 - m._11));
            float M7 = 0.5F / M6;
            result.X = (m._20 + m._02) * M7;
            result.Y = (m._21 + m._12) * M7;
            result.Z = 0.5F * M6;
            result.W = (m._01 - m._10) * M7;
        }

        public static Quaternion Divide(Quaternion q1, Quaternion q2)
        {
            Quaternion result;

            float w5 = 1.0F / ((q2.X * q2.X) + (q2.Y * q2.Y) + (q2.Z * q2.Z) + (q2.W * q2.W));
            float w4 = -q2.X * w5;
            float w3 = -q2.Y * w5;
            float w2 = -q2.Z * w5;
            float w1 =  q2.W * w5;

            result.X = (q1.X * w1) + (w4 * q1.W) + ((q1.Y * w2) - (q1.Z * w3));
            result.Y = (q1.Y * w1) + (w3 * q1.W) + ((q1.Z * w4) - (q1.X * w2));
            result.Z = (q1.Z * w1) + (w2 * q1.W) + ((q1.X * w3) - (q1.Y * w4));
            result.W = (q1.W * q2.W * w5) - ((q1.X * w4) + (q1.Y * w3) + (q1.Z * w2));
            return result;
        }
        public static void Divide(ref Quaternion q1, ref Quaternion q2, out Quaternion result)
        {
            float w5 = 1.0F / ((q2.X * q2.X) + (q2.Y * q2.Y) + (q2.Z * q2.Z) + (q2.W * q2.W));
            float w4 = -q2.X * w5;
            float w3 = -q2.Y * w5;
            float w2 = -q2.Z * w5;
            float w1 =  q2.W * w5;

            result.X = (q1.X * w1) + (w4 * q1.W) + ((q1.Y * w2) - (q1.Z * w3));
            result.Y = (q1.Y * w1) + (w3 * q1.W) + ((q1.Z * w4) - (q1.X * w2));
            result.Z = (q1.Z * w1) + (w2 * q1.W) + ((q1.X * w3) - (q1.Y * w4));
            result.W = (q1.W * q2.W * w5) - ((q1.X * w4) + (q1.Y * w3) + (q1.Z * w2));
        }

        public static float Dot(Quaternion q1, Quaternion q2)
        {
            return (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
        }
        public static void Dot(ref Quaternion q1, ref Quaternion q2, out float result)
        {
           result = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
        }

        public override bool Equals(object obj)
        {
            return (obj is Quaternion) ? this == (Quaternion)obj : false;
        }
        public bool Equals(Quaternion other)
        {
            if((X == other.X) && (Y == other.Y) && (Z == other.Z))
            {
                return W == other.W;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        public static Quaternion Inverse(Quaternion quaternion)
        {
            Quaternion result;
            float m1 = 1.0F / ((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W));
            result.X = -quaternion.X * m1;
            result.Y = -quaternion.Y * m1;
            result.Z = -quaternion.Z * m1;
            result.W = quaternion.W * m1;
            return result;
        }
        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            float m1 = 1.0F / ((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W));
            result.X = -quaternion.X * m1;
            result.Y = -quaternion.Y * m1;
            result.Z = -quaternion.Z * m1;
            result.W = quaternion.W * m1;
        }

        public float Length()
        {
            return (float)System.Math.Sqrt((double)((X * X) + (Y * Y) + (Z * Z) + (W * W)));
        }
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        public static Quaternion Lerp(Quaternion q1, Quaternion q2, float amount)
        {
            Quaternion result;
            float f2 = 1.0F - amount;
            if(((q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W)) >= 0.0F)
            {
                result.X = (f2 * q1.X) + (amount * q2.X);
                result.Y = (f2 * q1.Y) + (amount * q2.Y);
                result.Z = (f2 * q1.Z) + (amount * q2.Z);
                result.W = (f2 * q1.W) + (amount * q2.W);
            }
            else
            {
                result.X = (f2 * q1.X) - (amount * q2.X);
                result.Y = (f2 * q1.Y) - (amount * q2.Y);
                result.Z = (f2 * q1.Z) - (amount * q2.Z);
                result.W = (f2 * q1.W) - (amount * q2.W);
            }
            float f4 = (result.X * result.X) + (result.Y * result.Y) + (result.Z * result.Z) + (result.W * result.W);
            float f3 = 1.0F / (float)System.Math.Sqrt((double)f4);
            result.X *= f3;
            result.Y *= f3;
            result.Z *= f3;
            result.W *= f3;
            return result;
        }
        public static void Lerp(ref Quaternion q1, ref Quaternion q2, float amount, out Quaternion result)
        {
            float m2 = 1.0F - amount;
            if(((q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W)) >= 0.0F)
            {
                result.X = (m2 * q1.X) + (amount * q2.X);
                result.Y = (m2 * q1.Y) + (amount * q2.Y);
                result.Z = (m2 * q1.Z) + (amount * q2.Z);
                result.W = (m2 * q1.W) + (amount * q2.W);
            }
            else
            {
                result.X = (m2 * q1.X) - (amount * q2.X);
                result.Y = (m2 * q1.Y) - (amount * q2.Y);
                result.Z = (m2 * q1.Z) - (amount * q2.Z);
                result.W = (m2 * q1.W) - (amount * q2.W);
            }
            float m4 = (result.X * result.X) + (result.Y * result.Y) + (result.Z * result.Z) + (result.W * result.W);
            float m3 = 1.0F / (float)System.Math.Sqrt((double)m4);
            result.X *= m3;
            result.Y *= m3;
            result.Z *= m3;
            result.W *= m3;
        }

        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float amount)
        {
            Quaternion result;
            float t1;
            float t2;

            float q4 = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
            bool flag = false;
            if(q4 < 0.0F)
            {
                flag = true;
                q4 = -q4;
            }
            if(q4 > 0.999999F)
            {
                t1 = 1.0F - amount;
                t2 = flag ? -amount : amount;
            }
            else
            {
                float q5 = (float)System.Math.Acos((double)q4);
                float q6 = (float)(1.0 / System.Math.Sin((double)q5));
                t1 = (float)System.Math.Sin((double)((1.0F - amount) * q5)) * q6;
                t2 = flag ? (float)-System.Math.Sin((double)(amount * q5)) * q6 : (float)System.Math.Sin((double)(amount * q5)) * q6;
            }
            result.X = (t1 * q1.X) + (t2 * q2.X);
            result.Y = (t1 * q1.Y) + (t2 * q2.Y);
            result.Z = (t1 * q1.Z) + (t2 * q2.Z);
            result.W = (t1 * q1.W) + (t2 * q2.W);
            return result;
        }
        public static void Slerp(ref Quaternion q1, ref Quaternion q2, float amount, out Quaternion result)
        {
            float t1;
            float t2;

            float q4 = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
            bool flag = false;
            if(q4 < 0.0F)
            {
                flag = true;
                q4 = -q4;
            }
            if(q4 > 0.999999F)
            {
                t1 = 1.0F - amount;
                t2 = flag ? -amount : amount;
            }
            else
            {
                float q5 = (float)System.Math.Acos((double)q4);
                float q6 = (float)(1.0 / System.Math.Sin((double)q5));
                t1 = (float)System.Math.Sin((double)((1.0F - amount) * q5)) * q6;
                t2 = flag ? (float)-System.Math.Sin((double)(amount * q5)) * q6 : (float)System.Math.Sin((double)(amount * q5)) * q6;
            }
            result.X = (t1 * q1.X) + (t2 * q2.X);
            result.Y = (t1 * q1.Y) + (t2 * q2.Y);
            result.Z = (t1 * q1.Z) + (t2 * q2.Z);
            result.W = (t1 * q1.W) + (t2 * q2.W);
        }

        public static Quaternion Subtract(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z, q1.W - q2.W);
        }
        public static void Subtract(ref Quaternion q1, ref Quaternion q2, out Quaternion result)
        {
            result.X = q1.X - q2.X;
            result.Y = q1.Y - q2.Y;
            result.Z = q1.Z - q2.Z;
            result.W = q1.W - q2.W;
        }

        public static Quaternion Multiply(Quaternion q1, Quaternion q2)
        {
            Quaternion result;
            float f12 = (q1.Y * q2.Z) - (q1.Z * q2.Y);
            float f11 = (q1.Z * q2.X) - (q1.X * q2.Z);
            float f10 = (q1.X * q2.Y) - (q1.Y * q2.X);
            float f9 = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z);
            result.X = (q1.X * q2.W) + (q2.X * q1.W) + f12;
            result.Y = (q1.Y * q2.W) + (q2.Y * q1.W) + f11;
            result.Z = (q1.Z * q2.W) + (q2.Z * q1.W) + f10;
            result.W = (q1.W * q2.W) - f9;
            return result;
        }
        public static Quaternion Multiply(Quaternion q1, float scaleFactor)
        {
            Quaternion result;
            result.X = q1.X * scaleFactor;
            result.Y = q1.Y * scaleFactor;
            result.Z = q1.Z * scaleFactor;
            result.W = q1.W * scaleFactor;
            return result;
        }
        public static void Multiply(ref Quaternion q1, float scaleFactor, out Quaternion result)
        {
            result.X = q1.X * scaleFactor;
            result.Y = q1.Y * scaleFactor;
            result.Z = q1.Z * scaleFactor;
            result.W = q1.W * scaleFactor;
        }
        public static void Multiply(ref Quaternion q1, ref Quaternion q2, out Quaternion result)
        {
            float f12 = (q1.Y * q2.Z) - (q1.Z * q2.Y);
            float f11 = (q1.Z * q2.X) - (q1.X * q2.Z);
            float f10 = (q1.X * q2.Y) - (q1.Y * q2.X);
            float f9 = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z);
            result.X = (q1.X * q2.W) + (q2.X * q1.W) + f12;
            result.Y = (q1.Y * q2.W) + (q2.Y * q1.W) + f11;
            result.Z = (q1.Z * q2.W) + (q2.Z * q1.W) + f10;
            result.W = (q1.W * q2.W) - f9;
        }

        public static Quaternion Negate(Quaternion quaternion)
        {
            Quaternion result;
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
            return result;
        }
        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
        }

        public void Normalize()
        {
            float f1 = 1.0F / (float)System.Math.Sqrt((double)((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W)));
            this.X *= f1;
            this.Y *= f1;
            this.Z *= f1;
            this.W *= f1;
        }
        public static Quaternion Normalize(Quaternion quaternion)
        {
            Quaternion result;
            float f1 = 1.0F / (float)System.Math.Sqrt((double)((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W)));
            result.X = quaternion.X * f1;
            result.Y = quaternion.Y * f1;
            result.Z = quaternion.Z * f1;
            result.W = quaternion.W * f1;
            return result;
        }
        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            float f1 = 1.0F / (float)System.Math.Sqrt((double)((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W)));
            result.X = quaternion.X * f1;
            result.Y = quaternion.Y * f1;
            result.Z = quaternion.Z * f1;
            result.W = quaternion.W * f1;
        }

        public static Quaternion operator+(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z, q1.W + q2.W);
        }
        public static Quaternion operator/(Quaternion q1, Quaternion q2)
        {
            Quaternion result;

            float w5 = 1.0F / ((q2.X * q2.X) + (q2.Y * q2.Y) + (q2.Z * q2.Z) + (q2.W * q2.W));
            float w4 = -q2.X * w5;
            float w3 = -q2.Y * w5;
            float w2 = -q2.Z * w5;
            float w1 = q2.W * w5;

            result.X = (q1.X * w1) + (w4 * q1.W) + ((q1.Y * w2) - (q1.Z * w3));
            result.Y = (q1.Y * w1) + (w3 * q1.W) + ((q1.Z * w4) - (q1.X * w2));
            result.Z = (q1.Z * w1) + (w2 * q1.W) + ((q1.X * w3) - (q1.Y * w4));
            result.W = (q1.W * q2.W * w5) - ((q1.X * w4) + (q1.Y * w3) + (q1.Z * w2));
            return result;
        }
        public static bool operator==(Quaternion q1, Quaternion q2)
        {
            return 
                (q1.X == q2.X) && 
                (q1.Y == q2.Y) &&
                (q1.Z == q2.Z) &&
                (q1.W == q2.W);
        }
        public static bool operator!=(Quaternion q1, Quaternion q2)
        {
            return 
                (q1.X != q2.X) ||
                (q1.Y != q2.Y) ||
                (q1.Z != q2.Z) ||
                (q1.W != q2.W);
        }
        public static Quaternion operator*(Quaternion q1, Quaternion q2)
        {
            Quaternion result;
            float f12 = (q1.Y * q2.Z) - (q1.Z * q2.Y);
            float f11 = (q1.Z * q2.X) - (q1.X * q2.Z);
            float f10 = (q1.X * q2.Y) - (q1.Y * q2.X);
            float f9 = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z);
            result.X = (q1.X * q2.W) + (q2.X * q1.W) + f12;
            result.Y = (q1.Y * q2.W) + (q2.Y * q1.W) + f11;
            result.Z = (q1.Z * q2.W) + (q2.Z * q1.W) + f10;
            result.W = (q1.W * q2.W) - f9;
            return result;
        }
        public static Quaternion operator*(Quaternion q1, float s)
        {
            Quaternion result;
            result.X = q1.X * s;
            result.Y = q1.Y * s;
            result.Z = q1.Z * s;
            result.W = q1.W * s;
            return result;
        }
        public static Quaternion operator-(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z, q1.W - q2.W);
        }
        public static Quaternion operator-(Quaternion quaternion)
        {
            Quaternion q1;
            q1.X = -quaternion.X;
            q1.Y = -quaternion.Y;
            q1.Z = -quaternion.Z;
            q1.W = -quaternion.W;
            return q1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(32);
            sb.Append("(");
            sb.Append(X);
            sb.Append(", ");
            sb.Append(Y);
            sb.Append(", ");
            sb.Append(Z);
            sb.Append(", ");
            sb.Append(W);
            sb.Append(")");
            return sb.ToString();
        }

    }
}
