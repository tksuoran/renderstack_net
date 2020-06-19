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

using RenderStack.UI;

using example.Renderer;

namespace example.UIComponents
{
    public delegate void ChoiceActionDelegate(ChoiceItem selected);

    public class ChoiceItem
    {
        private Choice      choice;
        private PushButton  pushButton;

        public string Label { get { return pushButton.Label; } }

        internal PushButton PushButton { get { return pushButton; } }

        public ChoiceItem(IRenderer renderer, string label)
        {
            pushButton = new PushButton(renderer, label);
            pushButton.Action = Action;
        }

        private void Action(Area area)
        {
            if(choice.Selected != this)
            {
                choice.Selected = this;
            }
        }

        internal void Connect(Choice choice)
        {
            this.choice = choice;
        }
    }

    public class Choice : Dock
    {
        private ChoiceItem selected;
        private List<ChoiceItem> items = new List<ChoiceItem>();

        public List<ChoiceItem> Items { get { return items; } }

        public ChoiceItem       Selected
        {
            get
            {
                return selected;
            }
            set
            {
                if(value != selected)
                {
                    if(selected != null)
                    {
                        selected.PushButton.Pressed = false;
                    }
                    selected = value;
                    if(selected != null)
                    {
                        selected.PushButton.Pressed = true;
                    }
                    if(Action != null)
                    {
                        Action(selected);
                    }
                }
            }
        }
        public ChoiceActionDelegate Action;

        public Choice(Orientation orientation) : base(orientation)
        {
        }

        public ChoiceItem Add(ChoiceItem item)
        {
            item.Connect(this);
            switch(Orientation)
            {
                case Orientation.Horizontal: item.PushButton.LayoutStyle = AreaLayoutStyle.ExtendVertical; break;
                case Orientation.Vertical: item.PushButton.LayoutStyle = AreaLayoutStyle.ExtendHorizontal; break;
            }
            items.Add(item);
            base.Add(item.PushButton);
            return item;
        }
    }
}
