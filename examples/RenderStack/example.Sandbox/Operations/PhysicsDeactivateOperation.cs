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
        public void PhysicsDeactivate()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(
                (selectionManager.HoverModel != null) && 
                (selectionManager.HoverModel.Static == false)
            )
            {
                if(selectionManager.HoverModel != null)
                {
                    sceneManager.DeactivateModel(selectionManager.HoverModel);
                }
            }
        }
    }
}
