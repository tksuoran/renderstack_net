//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;

using RenderStack.Math;
using RenderStack.Scene;

using example.Renderer;

namespace example.Sandbox
{
    public interface IFrameController : IUpdateFixedStep, IUpdateOncePerFrame
    {
        Frame       Frame           { get; set; }
        Controller  RotateX         { get; }
        Controller  RotateY         { get; }
        Controller  RotateZ         { get; }
        Controller  TranslateX      { get; }
        Controller  TranslateY      { get; }
        Controller  TranslateZ      { get; }
        Controller  SpeedModifier   { get; } 

        void        SetTransform(Matrix4 transform);
        void        Clear();
    }
    public interface IPhysicsController
    {
        IPhysicsObject PhysicsObject { get; set; }
    }
}
