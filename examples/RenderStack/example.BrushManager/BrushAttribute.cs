using System;
using System.Collections.Generic;
using System.Text;

namespace example.Brushes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    /*  Comment: Highly experimental  */ 
    public class BrushAttribute
    :   System.Attribute
    {
    }
}
