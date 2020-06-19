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

using RenderStack.Math;

namespace example.Sandbox
{
    public partial class Operations
    {
        public override string Name
        {
            get { return "Operations"; }
        }
        public bool PhysicsOk()
        {
            if(
                (Configuration.physics == false) ||
                (selectionManager == null) ||
                (selectionManager.HoverModel == null) ||
                (selectionManager.HoverModel.RigidBody == null) ||
                (selectionManager.HoverModel.Static == true)
            )
            {
                return false;
            }
            return true;
        }
    }
}
