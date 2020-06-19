namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public class Popup : Dock
    {
        private Area closed;
        private Area open;

        public Area Current { get { return IsOpen ? Open : Closed; } }
        public Area Closed  { get { return closed; }    set { if(closed != value){ Toggle(); closed = value; Toggle(); } } } 
        public Area Open    { get { return open; }      set { if(open != value){ Toggle(); open = value; Toggle(); } } }
        public bool IsOpen  { get; private set; }

        public Popup(Area closed, Area open) : base(Orientation.Horizontal)
        {
            Style   = Style.NullPadding;
            Closed  = closed;
            Open    = open;
            IsOpen = false;
            Add(closed);
        }

        public void Toggle()
        {
            Set(!IsOpen);
        }

        //  Set(true) to open
        //  Set(false) to close
        public void Set(bool open)
        {
            if(open)
            {
                if(!IsOpen)
                {
                    IsOpen = true;
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
                if(IsOpen)
                {
                    IsOpen = false;
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
