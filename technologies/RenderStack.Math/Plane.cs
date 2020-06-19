namespace RenderStack.Math
{
    public struct Plane : System.IEquatable<Plane>
    {
        public Vector3 Normal;
        public float D;

        public Plane(Vector4 value)
        : this(new Vector3(value.X, value.Y, value.Z), value.W)
        {
        }

        public Plane(Vector3 normal, float d)
        {
            Normal = normal;
            D = d;
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;

            Vector3 cross = Vector3.Cross(ab, ac);
            Normal = Vector3.Normalize(cross);
            D = -(Vector3.Dot(cross, a));
        }

        public Plane(float a, float b, float c, float d)
        : this(new Vector3(a, b, c), d)
        {
        }

        public float Dot(Vector4 value) => (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z) + (D * value.W);

        public float DotCoordinate(Vector3 value) => (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z) + D;

        public void DotCoordinate(ref Vector3 value, out float result) => result = (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z) + D;

        public float DotNormal(Vector3 value) => (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z);

        public void DotNormal(ref Vector3 value, out float result) => result = (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z);

        public void Normalize()
        {
            float factor;
            Vector3 normal = Normal;
            Normal = Vector3.Normalize(Normal);
            factor = Normal.X / normal.X;
            D = D * factor;
        }

        public static Plane Normalize(Plane value)
        {
            Plane ret;
            Normalize(ref value, out ret);
            return ret;
        }

        public static void Normalize(ref Plane value, out Plane result)
        {
            float factor;
            result.Normal = Vector3.Normalize(value.Normal);
            factor = result.Normal.X / value.Normal.X;
            result.D = value.D * factor;
        }

        public static bool operator !=(Plane plane1, Plane plane2)
        {
            return !plane1.Equals(plane2);
        }

        public static bool operator ==(Plane plane1, Plane plane2)
        {
            return plane1.Equals(plane2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Plane) && Equals((Plane)obj);
        }

        public bool Equals(Plane other)
        {
            return ((Normal == other.Normal) && (D == other.D));
        }

        public override int GetHashCode()
        {
            return Normal.GetHashCode() ^ D.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{Normal:{0} D:{1}}}", Normal, D);
        }
    }
}
