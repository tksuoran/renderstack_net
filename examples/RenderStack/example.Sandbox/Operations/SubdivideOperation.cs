//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Sandbox
{
    public partial class Operations
    {
        public void Subdivide()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                Subdivide(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    Subdivide(model);
                }
            }
        }
    }
}