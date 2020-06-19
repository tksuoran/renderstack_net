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

namespace RenderStack.Scene
{
    [Serializable]
    /// \note Mostly stable, somewhat experimental
    public enum ProjectionType
    {
        Other = 0,              //  Projection is done by shader in unusual way - hemispherical for example
        PerspectiveHorizontal,
        PerspectiveVertical,
        Perspective,            //  Uses both horizontal and vertical fov and ignores aspect ratio
        OrthogonalHorizontal,
        OrthogonalVertical,
        Orthogonal,             //  Uses both horizontal and vertical size and ignores aspect ratio, O-centered
        OrthogonalRectangle,    //  Like above, not O-centered, uses X and Y as corner
        GenericFrustum,         //  Generic frustum
        StereoscopicHorizontal,
        StereoscopicVertical
    };
}
