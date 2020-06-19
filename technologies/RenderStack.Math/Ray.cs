using System;

namespace RenderStack.Math
{
    [Serializable]
    /*  Comment: Experimental. */
    public struct Ray
    {
        public Vector3 Direction;
        public Vector3 Position;

        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }

        public bool FindClosestPoints(Ray ray2, out float ray1Distance, out float ray2Distance)
        {
            float dist1; // ray1 is a function of dist1 -> Ray1(dist1) = ray1.Position + dist1 * line1.Direction
            float dist2; // ray2 is a function of dist2

            Vector3 Diff = ray2.Position - Position;

            Vector3 DCross = Vector3.Cross(Direction, ray2.Direction);
            float denominator = DCross.LengthSquared;

            if(denominator < float.Epsilon)
            {
                ray1Distance = 0.0f;
                ray2Distance = 0.0f;
                return false;
            }

            Vector3 a = Diff;
            Vector3 b = ray2.Direction;
            Vector3 c = DCross;
            float det1 = a.X * b.Y * c.Z + a.Z * b.X * c.Y + a.Y * b.Z * c.X
                       - a.Z * b.Y * c.X - a.X * b.Z * c.Y - a.Y * b.X * c.Z;

            b = Direction;
            float det2 = a.X * b.Y * c.Z + a.Z * b.X * c.Y + a.Y * b.Z * c.X
                       - a.Z * b.Y * c.X - a.X * b.Z * c.Y - a.Y * b.X * c.Z;


            dist1 = det1 / denominator;
            dist2 = det2 / denominator;

            ray1Distance = dist1;
            ray2Distance = dist2;
            return true;
        }

    }
}
