using RenderStack.Math;

namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public class Dock : Area
    {
        private Vector2 cursorStart;
        private Vector2 cursorEnd;
        private Vector2 growDirection;

        public Orientation Orientation { get; set; }
        public AreaLayoutStyle  ChildLayoutStyle = AreaLayoutStyle.Normal;

        public Dock(Orientation orientation)
        {
            Orientation = orientation;
            EventOrdering = AreaOrder.Separate;
        }

        public override void BeginSize(Vector2 freeSizeReference)
        {
            Size = Style.Padding;
        }

        public override void  CallSize(Area area)
        {
            // dmsg( M_WML, "%s->Dock::callSize( %s ) = %...", this->getName().c_str(), a->getName().c_str() );
            // Vector2 sub_size  = a->doSize( size );
            //Debug.Print(Name + ".CallSize(" + area.Name + ") = ...");
            Vector2 subSize = area.DoSize(size);

            // dmsg( M_WML, "  %s->Dock::callSize( %s ) = %f x %f", this->getName().c_str(), a->getName().c_str(), sub_size[0], sub_size[1] );
            //Debug.Print(Name + ".CallSize(" + area.Name + ") = " + subSize.ToString());
            // size[axis_sum]   += sub_size[axis_sum] + style->padding[axis_sum];
            // size[axis_max]    = MAX( size[axis_max], sub_size[axis_max] );
            switch(Orientation)
            {
                case Orientation.Horizontal:
                {
                    SizeX += subSize.X + Style.InnerPadding.X;
                    SizeY = System.Math.Max(SizeY, subSize.Y);
                    break;
                }
                case Orientation.Vertical:
                {
                    SizeY += subSize.Y + Style.InnerPadding.Y;
                    SizeX = System.Math.Max(SizeX, subSize.X);
                    break;
                }
            }
            // dmsg( M_WML, "  %s->size = %f x %f", this->getName().c_str(), a->getName().c_str(), size[0], size[1] );
            //Debug.Print("  " + Name + ".Size = " + size.ToString());
        }

        public override Area Add(Area area)
        {
            if(area == null)
            {
                return null;
            }
            area.LayoutStyle = ChildLayoutStyle;

            return base.Add(area);
        }

        public override void EndSize()
        {
            // size[axis_max] += 2 * style->padding[axis_max];
            // dmsg( M_WML, "%s->Dock::endSize( %f x %f )", this->getName().c_str(), size[0], size[1] );
            switch(Orientation)
            {
                case Orientation.Horizontal:
                {
                    SizeX -= growDirection.X * Style.InnerPadding.X;
                    SizeX += growDirection.X * Style.Padding.X;
                    SizeY += growDirection.Y * 2.0f * Style.Padding.Y; 
                    break;
                }
                case Orientation.Vertical:
                {
                    SizeY -= growDirection.Y * Style.InnerPadding.Y;
                    SizeY += growDirection.Y * Style.Padding.Y;
                    SizeX += growDirection.Y * 2.0f * Style.Padding.X; 
                    break;
                }
            }
            //Debug.Print(Name + ".Dock.EndSize() size = " + size.ToString());
        }

        public override void BeginPlace(Rectangle reference, Vector2 containerGrowDirection)
        {
            // dmsg( M_WML, "%s->Dock::beginPlace()", getName().c_str() );
            // rect.min = ref.min  + offset_pixels + ref.getSize() * offset_free_size_relative + size * offset_self_size_relative;
            // rect.max = rect.min + size;
            // 
            // if( isEnabled(Area::USE_CLIP_TO_REFERENCE) ){
            //     rect.intersect( ref );
            // }
            // 
            // in_rect = rect.shrink( style->padding );
            // 
            // cursor_start = rect.min + style->padding;
            // cursor_end   = rect.max - style->padding;

            //Debug.Print(Name + ".Dock.BeginPlace()");

            rect.Min = reference.Min + OffsetPixels + reference.Size * OffsetFreeSizeRelative + Size * OffsetSelfSizeRelative;
            rect.Max = rect.Min + Size;

            if(ClipToReference)
            {
                rect.Intersect(reference);
            }

            inRect = rect.Shrink(Style.Padding);

            switch(LayoutXOrder)
            {
                case AreaLayoutOrder.Increasing:
                {
                    growDirection.X = 1.0f;
                    cursorStart.X = rect.Min.X + Style.Padding.X;
                    cursorEnd.X   = rect.Max.X - Style.Padding.X;
                    break;
                }
                case AreaLayoutOrder.Decreasing:
                {
                    growDirection.X = -1.0f;
                    cursorStart.X = rect.Max.X - Style.Padding.X;
                    cursorEnd.X   = rect.Min.X + Style.Padding.X;
                    break;
                }
            }
            switch(LayoutYOrder)
            {
                case AreaLayoutOrder.Increasing:
                {
                    growDirection.Y = 1.0f;
                    cursorStart.Y = rect.Min.Y + Style.Padding.Y;
                    cursorEnd.Y   = rect.Max.Y - Style.Padding.Y;
                    break;
                }
                case AreaLayoutOrder.Decreasing:
                {
                    growDirection.Y = -1.0f;
                    cursorStart.Y = rect.Max.Y - Style.Padding.Y;
                    cursorEnd.Y   = rect.Min.Y + Style.Padding.Y;
                    break;
                }
            }
        }

        public override void CallPlace(Area area)
        {
            // dmsg( M_WML, "%s->Dock::callPlace( %s )", this->getName().c_str(), a->getName().c_str() );
            // dmsg( M_WML, "  cursor_start = %f, %f", cursor_start[0], cursor_start[1] );
            // Vector2 sub_size = a->doPlace( Rect(cursor_start,cursor_end) );
            // cursor_start[axis_sum] += sub_size[axis_sum] + style->padding[axis_sum];
            // dmsg( M_WML, "  cursor_start = %f, %f", cursor_start[0], cursor_start[1] );

            //Debug.Print(Name + ".Dock.CallPlace(" + area.Name + ")");
            //Debug.Print("  cursorStart = " + cursorStart.ToString());

            Rectangle reference = new Rectangle(cursorStart, cursorEnd);

            Vector2 subSize = area.DoPlace(reference, growDirection);

            switch(Orientation)
            {
                case Orientation.Horizontal:    cursorStart.X += growDirection.X * (subSize.X + Style.InnerPadding.X); break;
                case Orientation.Vertical:      cursorStart.Y += growDirection.Y * (subSize.Y + Style.InnerPadding.Y); break;
            }
            //Debug.Print("  cursorStart = " + cursorStart.ToString() + " subSize was " + subSize.ToString());
        }
    }
}
