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
using System.Diagnostics;

using RenderStack.Math;

namespace RenderStack.UI
{
    /*  Comment: Mostly stable, somewhat experimental  */ 
    public class Area
    {
        public      string      Name;
        public      bool        Hidden                  = false;
        public      Vector2     OffsetPixels            = new Vector2();
        public      Vector2     OffsetSelfSizeRelative  = new Vector2();
        public      Vector2     OffsetFreeSizeRelative  = new Vector2();
        public      Vector2     FillBasePixels          = new Vector2();
        public      Vector2     FillFreeSizeRelative    = new Vector2();
        protected   Area        parent;
        private     List<Area>  children = new List<Area>();
        protected   AreaOrder   DrawOrdering;
        protected   AreaOrder   EventOrdering;
        protected   Vector2     size = new Vector2();
        protected   Rectangle   rect = new Rectangle();
        protected   Rectangle   inRect = new Rectangle();

        public      Vector2     Size    { get { return size; } set { size = value; } }
        public      float       SizeX   { get { return size.X; } set { size.X = value; } }
        public      float       SizeY   { get { return size.Y; } set { size.Y = value; } }
        public      Rectangle   Rect    { get { return rect; } }
        protected   Rectangle   InRect  { get { return inRect; } }
        public      List<Area>  Children { get { return children; } }

        public      bool        ClipToReference;

        public      Area        Link;
        public      Area        Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if(parent != value)
                {
                    parent = value;
                }
            }
        }
        public      Style       Style;

        public Area()
        {
            DrawOrdering = AreaOrder.SelfFirst;
            Style = Style.Default;
        }

        private bool inDraw = false;
        private List<Area> addList = new List<Area>();
        private List<Area> removeList = new List<Area>();

        public virtual Area Add(Area area)
        {
            if(inDraw)
            {
                addList.Add(area);
            }
            else
            {
                Children.Add(area);
                area.Parent = this;
            }
            return area;
        }
        public Area Remove(Area area)
        {
            if(inDraw)
            {
                removeList.Add(area);
            }
            else
            {
                Children.Remove(area);
                area.Parent = null;
            }
            return area;
        }
        public Area GetHit(Vector2 hitPosition)
        {
            if(Hidden)
            {
                return null;
            }
            if(EventOrdering == AreaOrder.SelfFirst)
            {
                if(Rect.Hit(hitPosition))
                {
                    return this;
                }
            }

            foreach(var child in Children)
            {
                Area hit = child.GetHit(hitPosition);
                if(hit != null)
                {
                    return hit;
                }
            }

            if(EventOrdering == AreaOrder.PostSelf)
            {
                if(Rect.Hit(hitPosition))
                {
                    return this;
                }
            }

            return null;
        }

        public virtual void DrawSelf(IUIContext context)
        {
        }
        public void Draw(IUIContext context)
        {
            if(Hidden)
            {
                return;
            }

            inDraw = true;
            if(DrawOrdering == AreaOrder.SelfFirst)
            {
                DrawSelf(context);
            }

            foreach(var child in Children)
            {
                child.Draw(context);
            }

            if(DrawOrdering == AreaOrder.PostSelf)
            {
                DrawSelf(context);
            }
            inDraw = false;

            foreach(Area area in addList)
            {
                Add(area);
            }
            foreach(Area area in removeList)
            {
                Remove(area);
            }
            addList.Clear();
            removeList.Clear();
        }

        #region Layout
        public virtual void BeginSize(Vector2 freeSizeReference)
        {
            //Debug.Print(Name + ".Area.BeginSize() reference = " + freeSizeReference.ToString());
            size.X = FillBasePixels.X + freeSizeReference.X * FillFreeSizeRelative.X;
            size.Y = FillBasePixels.Y + freeSizeReference.Y * FillFreeSizeRelative.Y;
            //Debug.Print(Name + ".size = " + size.ToString());
        }
        public virtual void CallSize(Area area)
        {
            //Debug.Print(Name + ".Area.CallSize(" + area.Name + ")");
            area.DoSize(size);
        }
        public virtual void EndSize()
        {
            //Debug.Print(Name + ".Area.EndSize() size = " + size.ToString());
        }
        // Do not make this virtual.
        // Derived classes should override BeginSize() instead
        public Vector2 DoSize(Vector2 freeSizeReference)
        {
            //Debug.Print(Name + ".Area.DoSize() reference = " + freeSizeReference.ToString());
            BeginSize(freeSizeReference);
            foreach(var area in Children)
            {
                CallSize(area);
            }
            EndSize();
            //Debug.Print(Name + ".size = " + size.ToString());
            return size;
        }
        public enum AreaLayoutStyle
        {
            Normal,
            ExtendHorizontal,
            ExtendVertical
        }
        public enum AreaLayoutOrder
        {
            Increasing,
            Decreasing
        }
        public AreaLayoutStyle LayoutStyle = AreaLayoutStyle.Normal;
        public AreaLayoutOrder LayoutXOrder = AreaLayoutOrder.Increasing;
        public AreaLayoutOrder LayoutYOrder = AreaLayoutOrder.Increasing;
        public virtual void BeginPlace(Rectangle reference, Vector2 containerGrowDirection)
        {
            //Debug.Print(Name + ".Area.BeginPlace() reference = " + reference.ToString());
            //Debug.Print("  " + Name + ".size = " + size.ToString());
            switch(LayoutStyle)
            {
                case AreaLayoutStyle.Normal:
                {
                    break;
                }
                case AreaLayoutStyle.ExtendHorizontal:
                {
                    size.X = reference.Size.X;
                    break;
                }
                case AreaLayoutStyle.ExtendVertical:
                {
                    size.Y = reference.Size.Y;
                    break;
                }
            }
            //Debug.Print("  " + Name + ".size = " + size.ToString() + " after LayoutStyle");

            rect.Min.X = 
                reference.Min.X + 
                OffsetPixels.X + 
                reference.Size.X * OffsetFreeSizeRelative.X + 
                Size.X * OffsetSelfSizeRelative.X;

            rect.Min.Y = 
                reference.Min.Y + 
                OffsetPixels.Y + 
                reference.Size.Y * OffsetFreeSizeRelative.Y + 
                Size.Y * OffsetSelfSizeRelative.Y;

            rect.Max = rect.Min + Size;

            if(ClipToReference)
            {
                rect.ClipTo(reference);
            }
            inRect = rect.Shrink(Style.Padding);
        }
        public virtual void CallPlace(Area area)
        {
            //Debug.Print(Name + ".Area.CallPlace(" + area.Name + ")");
            area.DoPlace(rect, Vector2.One);
        }
        public virtual void EndPlace()
        {
        }
        // Do not make this virtual.
        // Derived classes should override BeginPlace() instead
        public Vector2 DoPlace(Rectangle referenceLocation, Vector2 growDirection)
        {
            //Debug.Print(Name + ".Area.DoPlace() reference = " + referenceLocation.ToString());
            //Debug.Print("  " + Name + ".size = " + size.ToString());
            BeginPlace(referenceLocation, growDirection);
            foreach(var area in Children)
            {
                CallPlace(area);
                //string name = area.Name != null ? area.Name : "";
            }
            EndPlace();
            return Size;
        }
        #endregion Layout
    }
}