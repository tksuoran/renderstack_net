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
using System.Runtime.InteropServices;

namespace RenderStack.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    /// \note Somewhat experimental
    public struct BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Size     { get { return Max - Min; } }
        public Vector3 HalfSize { get { return Size / 2; } }
        public Vector3 Center   { get { return Min + HalfSize; } }

        public void Clear()
        {
            Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        }

        public void ExtendBy(Vector3 point)
        {
            if(point.X < Min.X) Min.X = point.X;
            if(point.X > Max.X) Max.X = point.X;
            if(point.Y < Min.Y) Min.Y = point.Y;
            if(point.Y > Max.Y) Max.Y = point.Y;
            if(point.Z < Min.Z) Min.Z = point.Z;
            if(point.Z > Max.Z) Max.Z = point.Z;
        }

        public void ExtendBy(float x, float y, float z)
        {
            if(x < Min.X) Min.X = x;
            if(x > Max.X) Max.X = x;
            if(y < Min.Y) Min.Y = y;
            if(y > Max.Y) Max.Y = y;
            if(z < Min.Z) Min.Z = z;
            if(z > Max.Z) Max.Z = z;
        }

        public override string ToString()
        {
            return Min.ToString() + " .. " + Max.ToString();
        }
    }
}
