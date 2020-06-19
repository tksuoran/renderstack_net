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

namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public class Popup : Dock
    {
        private bool isOpen;
        private Area closed;
        private Area open;

        public Area Current { get { return isOpen ? Open : Closed; } }
        public Area Closed  { get { return closed; }    set { if(closed != value){ Toggle(); closed = value; Toggle(); } } } 
        public Area Open    { get { return open; }      set { if(open != value){ Toggle(); open = value; Toggle(); } } }
        public bool IsOpen  { get { return isOpen; } }

        public Popup(Area closed, Area open) : base(Orientation.Horizontal)
        {
            Style   = Style.NullPadding;
            Closed  = closed;
            Open    = open;
            isOpen  = false;
            Add(closed);
        }

        public void Toggle()
        {
            Set(!isOpen);
        }

        //  Set(true) to open
        //  Set(false) to close
        public void Set(bool open)
        {
            if(open)
            {
                if(!isOpen)
                {
                    isOpen = true;
                    if(Closed != null)
                    {
                        Remove(Closed);
                    }
                    if(Open != null)
                    {
                        Add(Open);
                    }
                    if(Parent != null)
                    {
                        // window_manager->update();
                    }
                }
            }
            else
            {
                if(isOpen)
                {
                    isOpen = false;
                    if(Open != null)
                    {
                        Remove(Open);
                    }
                    if(Closed != null)
                    {
                        Add(Closed);
                    }
                    if(Parent != null)
                    {
                        //window_manager->update();
                    }
                }
            }
        }
    }
}
