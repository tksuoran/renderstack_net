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

using System.Collections.Generic;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class Octahedron : Geometry
    {
        public Octahedron(double r)
        {
            MakePoint( 0,  r,  0);
            MakePoint( 0, -r,  0);
            MakePoint(-r,  0,  0);
            MakePoint( 0,  0, -r);
            MakePoint( r,  0,  0);
            MakePoint( 0,  0,  r);
            MakePolygon( 0, 2, 3 );
            MakePolygon( 0, 3, 4 );
            MakePolygon( 0, 4, 5 );
            MakePolygon( 0, 5, 2 );
            MakePolygon( 3, 2, 1 );
            MakePolygon( 4, 3, 1 );
            MakePolygon( 5, 4, 1 );
            MakePolygon( 2, 5, 1 );
        }
    }
}
