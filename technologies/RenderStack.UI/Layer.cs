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

using RenderStack.Math;

namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public class Layer : Area
    {
        //private IUIContext          context;
        private OpenTK.GameWindow   window;

        public Layer(IUIContext context, OpenTK.GameWindow window)
        {
            //this.context    = context;
            this.window     = window;

            Parent          = null;
            DrawOrdering    = AreaOrder.PostSelf;
            EventOrdering   = AreaOrder.Separate;

            Update();
        }

        public void Update()
        {
            rect    = new Rectangle(0, 0, window.Width - 1, window.Height - 1);
            size    = rect.Size;
            Place();
        }

        public void Place()
        {
            foreach(var child in Children)
            {
                child.DoSize(size);
            }
            foreach(var child in Children)
            {
                child.DoPlace(rect, Vector2.One);
            }

        }
    }
}
