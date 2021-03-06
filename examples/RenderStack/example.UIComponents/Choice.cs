﻿using System;
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
