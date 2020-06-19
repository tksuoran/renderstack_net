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
