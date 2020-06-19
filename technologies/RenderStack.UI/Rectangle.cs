﻿//  Copyright (C) 2011 by Timo Suoranta                                            
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

using RenderStack.Math;

namespace RenderStack.UI
{
    [Serializable]
    /*  Comment: Experimental. Used by the user interface.  */
    public class Rectangle
    {
        public Vector2 Min;
        public Vector2 Max;

        public Vector2 Size
        {
            get
            {
                return new Vector2(
                    Max.X - Min.X + 1.0f,
                    Max.Y - Min.Y + 1.0f
                );
            }
        }

        public Rectangle()
        {
        }
        public void CopyFrom(Rectangle a)
        {
            Min.X = a.Min.X;
            Min.Y = a.Min.Y;
            Max.X = a.Max.X;
            Max.Y = a.Max.Y;
        }
        public Rectangle(float minX, float minY, float maxX, float maxY)
        {
            Min.X = minX;
            Min.Y = minY;
            Max.X = maxX;
            Max.Y = maxY;
        }
        public void ResetForGrow()
        {
            Min.X = float.MaxValue;
            Min.Y = float.MaxValue;
            Max.X = float.MinValue;
            Max.Y = float.MinValue;
        }

        public Rectangle(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }
        public bool Hit(Vector2 hitPosition)
        {
            if(
                (hitPosition.X < Min.X) || 
                (hitPosition.Y < Min.Y) || 
                (hitPosition.X > Max.X) ||
                (hitPosition.Y > Max.Y)
            )
            {
                return false;
            }
            return true;
        }
        public void Extend(float x, float y)
        {
            if(x < Min.X) Min.X = x;
            if(x > Max.X) Max.X = x;
            if(y < Min.Y) Min.Y = y;
            if(y > Max.Y) Max.Y = y;
        }
        public void ClipTo(Rectangle reference)
        {
            Min.X = System.Math.Max(Min.X, reference.Min.X);
            Min.Y = System.Math.Max(Min.Y, reference.Min.Y);
            Max.X = System.Math.Min(Max.X, reference.Max.X);
            Max.Y = System.Math.Min(Max.Y, reference.Max.Y);
        }
        public Rectangle Shrink(Vector2 padding)
        {
            return new Rectangle(
                Min + padding,
                Max - padding
            );
        }
        public void GrowBy(float paddingX, float paddingY)
        {
            Min.X -= paddingX;
            Min.Y -= paddingY;
            Max.X += paddingX;
            Max.Y += paddingY;
        }
        public void Intersect(Rectangle other)
        {
            Min.X = System.Math.Max(Min.X, other.Min.X);
            Min.Y = System.Math.Max(Min.Y, other.Min.Y);
            Max.X = System.Math.Min(Max.X, other.Max.X);
            Max.Y = System.Math.Min(Max.Y, other.Max.Y);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1} .. {2}, {3}  size {4})", Min.X, Min.Y, Max.X, Max.Y, Size.ToString());
        }
    }
}
