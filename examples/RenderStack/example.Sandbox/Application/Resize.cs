//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;

using example.Loading;
using example.Renderer;

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Sandbox
{
    public partial class Application
    {
        private void Application_Resize(object sender, EventArgs e)
        {
            Services.Get<HighLevelRenderer>().Resize(this.Width, this.Height);
        }
    }
}