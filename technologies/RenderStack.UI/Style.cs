using System;
using RenderStack.Graphics;
using RenderStack.Math;

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