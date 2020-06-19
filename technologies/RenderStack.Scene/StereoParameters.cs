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

using RenderStack.Graphics;

namespace RenderStack.Scene
{
    /// \note Mostly stable, somewhat experimental
    public class StereoParameters
    {
        private Floats   eyeSeparation   = new Floats(0.065f);
        private Floats   perspective     = new Floats(1.0f);
        private Floats   viewportCenter  = new Floats(0.0f, 0.0f, 4.0f);
        private Floats   eyePosition     = new Floats(0.0f, 0.0f, 0.0f);
        private Floats   viewPlaneSize   = new Floats(2, 1);

        public Floats    EyeSeparation   { get { return eyeSeparation; } }
        public Floats    Perspective     { get { return perspective; } }
        public Floats    ViewportCenter  { get { return viewportCenter; } }
        public Floats    EyePosition     { get { return eyePosition; } }

        public Floats    ViewPlaneSize   { get { return viewPlaneSize; } }
    }
}
