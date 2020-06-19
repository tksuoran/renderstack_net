using System;

namespace RenderStack.Math
{
    [Serializable]
    /*  Comment: Mostly stable. */
    /*  Needed by both Graphics and Scene.  */
    /*  TODO figure out if Math really is the best place for this class.  */
    public class Viewport : IEquatable<Viewport>
    {
        private int width;
        private int height;

        public int      Border;
        public int      X;
        public int      Y;
        public int      Width       { get { return width;  } set { width = value; ComputeAspectRatio(); } }
        public int      Height      { get { return height; } set { height = value; ComputeAspectRatio(); } } 
        public float    AspectRatio { get; set; }

        public static Viewport Default = new Viewport(1, 1);

        public static bool operator==(Viewport a, Viewport b)
        {
            if(
                (a is null) && (b is null)
            )
            {
                return true;
            }
            return (!(a is null)) && (!(b is null)) && (a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height) && (a.Border == b.Border);
        }

        public static bool operator!=(Viewport a, Viewport b)
        {
            if(
                (a is null) && (b is null)
            )
            {
                return false;
            }
            return (a is null) || (b is null) || (a.X != b.X) || (a.Y != b.Y) || (a.Width != b.Width) || (a.Height != b.Height) || (a.Border != b.Border);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode() ^ Border.GetHashCode();
        }

        bool IEquatable<Viewport>.Equals(Viewport o) => this == o;

        public override bool Equals(object obj)
        {
            if (obj is Viewport c)
            {
                return this == c;
            }
            return false;
        }
		
        public Viewport(int width, int height)
        {
            this.width = width;
            this.height = height;
            ComputeAspectRatio();
            //  Do not call virtual method here, it might do wrong things, we only
            //  want the above stuff in constructor
            //Resize(width, height);
        }

        public Viewport(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            this.width = width;
            this.height = height;
            ComputeAspectRatio();
            //  Do not call virtual method here, it might do wrong things, we only
            //  want the above stuff in constructor
            //Resize(width, height);
        }

        public virtual void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            ComputeAspectRatio();
        }

        private void ComputeAspectRatio()
        {
            AspectRatio = (height != 0) ? (float)width / (float)height : 1.0f;
        }
    }
}
