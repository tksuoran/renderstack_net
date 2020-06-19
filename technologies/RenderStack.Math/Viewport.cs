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
                ((object)a == null) && ((object)b == null)
            )
            {
                return true;
            }
            return ((object)a != null) && ((object)b != null) && (a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height) && (a.Border == b.Border);
        }

        public static bool operator!=(Viewport a, Viewport b)
        {
            if(
                ((object)a == null) && ((object)b == null)
            )
            {
                return false;
            }
            return ((object)a == null) || ((object)b == null) || (a.X != b.X) || (a.Y != b.Y) || (a.Width != b.Width) || (a.Height != b.Height) || (a.Border != b.Border);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode() ^ Border.GetHashCode();
        }

        bool System.IEquatable<Viewport>.Equals(Viewport o)
        {
            return this == o;
        }

        public override bool Equals(object o)
        {
            if(o is Viewport)
            {
                Viewport c = (Viewport)o;
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
