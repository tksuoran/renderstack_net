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
using System.Collections.Generic;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

namespace RenderStack.UI
{
    [Serializable]
    /*  Comment: Experimental  */ 
    public class Style
    {
        public Vector2          Padding;
        public Vector2          InnerPadding;
        public IProgram         Program;
        public FontStyle        Font;
        public NinePatchStyle   NinePatchStyle;

        public static Style Default;
        public static Style Background;
        public static Style Foreground;
        public static Style NullPadding;

        public Style(
            Vector2         padding, 
            Vector2         innerpadding,
            FontStyle       fontStyle,
            NinePatchStyle  ninePatchStyle,
            IProgram        program
        )
        {
            Padding         = padding;
            InnerPadding    = innerpadding;
            Font            = fontStyle;
            NinePatchStyle  = ninePatchStyle;
            Program         = program;
        }
    }

}