//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;
using Sphere = RenderStack.Geometry.Shapes.Sphere;

//  TODO:
//      http://www.niksula.hut.fi/~hkankaan/Homepages/bezierfast.html
namespace example.CurveTool
{
    /*  Comment: Experimental  */ 
    public class CurveHandle
    {
        public Model model;

        public Floats Parameters = new Floats(0.0f, 0.0f, 0.0f);

        public Vector3 Position
        {
            get
            {
                //model.Frame.UpdateHierarchical();
                return new Vector3(
                    model.Frame.LocalToWorld.Matrix._03,
                    model.Frame.LocalToWorld.Matrix._13,
                    model.Frame.LocalToWorld.Matrix._23
                );
            }
            set
            {
                //  TODO take into account parent transform!
                //model.Frame.UpdateHierarchical();
                model.Frame.LocalToParent.SetTranslation(value);
                model.Frame.LocalToWorld.SetTranslation(value);
                //model.Frame.UpdateHierarchical();
            }
        }
    }
}
