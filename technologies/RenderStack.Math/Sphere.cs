namespace RenderStack.Math
{
    public struct Sphere : System.IEquatable<Sphere>
    {
        public Vector3 Center;
        public float Radius;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public Sphere(float x, float y, float z, float r)
        : this(new Vector3(x, y, z), r)
        {
        }

        public static bool operator !=(Sphere a, Sphere b)
        {
            return (a.Center != b.Center) || (a.Radius != b.Radius);
        }

        public static bool operator ==(Sphere a, Sphere b)
        {
            return (a.Center == b.Center) && (a.Radius == b.Radius);
        }

        public override bool Equals(object other)
        {
            return (other is Sphere) ? this.Equals((Sphere)other) : false;
        }

        public bool Equals(Sphere other)
        {
            return (Center == other.Center) && (Radius == other.Radius);
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Radius.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{Center:{0} Radius:{1}}}", Center, Radius);
        }
    }
}
