using System;

namespace RenderStack.Geometry
{
    [Serializable]
    /// \brief Connection between two points.
    /// \note Edges A to B and B to A are considered the same edge.
    /// 
    /// Edge stores only connectivity information.
    /// All edge attributes such as smoothing angle
    /// are stored externally in attribute maps such as
    /// Dictionary<Edge, float>
    /// \note Experimental.
    public struct Edge : IEquatable<Edge>
    {
        private Point a;
        private Point b;

        public Point A { get { return a; } private set { a = value; } }
        public Point B { get { return b; } private set { b = value; } }

        public Point this[int index] { get { return index == 0 ? A : B; } }

        public Point Other(Point first)
        {
            return (A == first) ? B : A;
        }

        /// \brief Constructs a new edge sharing points a and b.
        public Edge(Point a, Point b)
        {
            this.a = a;
            this.b = b;
        }

        /// \brief Equality test for edges. Edge sharing the same points are
        /// considered equal regardless of the order of the points.
        bool IEquatable<Edge>.Equals(Edge other)
        {
            bool equals = 
                ((A == other.A) && (B == other.B)) ||
                ((A == other.B) && (B == other.A));

            /*if(equals == true)
            {
                Logger.Log("!");
            }*/

            return equals;
        }

        public override bool Equals(object obj)
        {
            if(obj is Edge)
            {
                Edge other = (Edge)obj;
                bool equals = 
                    ((A == other.A) && (B == other.B)) ||
                    ((A == other.B) && (B == other.A));

                return equals;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode();
        }
    }
}
