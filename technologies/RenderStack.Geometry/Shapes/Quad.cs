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
    public class QuadXY : Geometry
    {
        public QuadXY(
            double x, double y, double z
        )
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint(-x,  y,  z, 0, 1);  /*  0  */ 
            MakePoint(-x, -y,  z, 0, 0);  /*  4  */ 
            MakePoint( x, -y,  z, 1, 0);  /*  5  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 

            MakePolygon(  0, 1, 2, 3  );
        }
    }
    [System.Serializable]
    public class QuadXZ : Geometry
    {
        public QuadXZ(
            double x, double y, double z
        )
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint(-x,  y,  z, 0, 1);  /*  0  */ 
            MakePoint(-x,  y, -z, 0, 0);  /*  4  */ 
            MakePoint( x,  y, -z, 1, 0);  /*  5  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 

            MakePolygon(  0, 1, 2, 3  );
        }
    }
    [System.Serializable]
    public class QuadYZ : Geometry
    {
        public QuadYZ(
            double x, double y, double z
        )
        {
            x = x / 2;
            y = y / 2;
            z = z / 2;

            MakePoint( x, -y,  z, 0, 1);  /*  0  */ 
            MakePoint( x, -y, -z, 0, 0);  /*  4  */ 
            MakePoint( x,  y, -z, 1, 0);  /*  5  */ 
            MakePoint( x,  y,  z, 1, 1);  /*  1  */ 

            MakePolygon(  0, 1, 2, 3  );
        }
    }
}
