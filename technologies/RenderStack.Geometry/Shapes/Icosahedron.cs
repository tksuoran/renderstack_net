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

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Icosahedron : Geometry
    {
        public Icosahedron(double r)
        {
            double sq5 = System.Math.Sqrt(5.0);
            double a = 2.0 / (1.0 + sq5);
            double b = System.Math.Sqrt((3.0 + sq5) / (1.0 + sq5));
            a /= b;

            MakePoint(      0,  r * a,  r / b );
            MakePoint(      0,  r * a, -r / b );
            MakePoint(      0, -r * a,  r / b );
            MakePoint(      0, -r * a, -r / b );
            MakePoint(  r * a,  r / b,      0 );
            MakePoint(  r * a, -r / b,      0 );
            MakePoint( -r * a,  r / b,      0 );
            MakePoint( -r * a, -r / b,      0 );
            MakePoint(  r / b,      0,  r * a );
            MakePoint(  r / b,      0, -r * a );
            MakePoint( -r / b,      0,  r * a );
            MakePoint( -r / b,      0, -r * a );

            MakePolygon( 1,  4,  6 );
            MakePolygon( 0,  6,  4 );
            MakePolygon( 0,  2, 10 );
            MakePolygon( 0,  8,  2 );
            MakePolygon( 1,  3,  9 );
            MakePolygon( 1, 11,  3 );
            MakePolygon( 2,  5,  7 );
            MakePolygon( 3,  7,  5 );
            MakePolygon( 6, 10, 11 );
            MakePolygon( 7, 11, 10 );
            MakePolygon( 4,  9,  8 );
            MakePolygon( 5,  8,  9 );
            MakePolygon( 0, 10,  6 );
            MakePolygon( 0,  4,  8 );
            MakePolygon( 1,  6, 11 );
            MakePolygon( 1,  9,  4 );
            MakePolygon( 3, 11,  7 );
            MakePolygon( 3,  5,  9 );
            MakePolygon( 2,  7, 10 );
            MakePolygon( 2,  8,  5 );
        }
    }
}
