//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using RenderStack.Math;

using RenderStack.Graphics;

namespace example.CurveTool
{
    public class ControlPoint
    {
        public ControlPoint(Vector3 position, Floats parameters)
        {
            Position = position;
            Parameters = parameters;
        }
        public Vector3 Position;
        public Floats  Parameters;
    }
    /*  Comment: Experimental  */ 
    public interface ICurve : RenderStack.Geometry.Shapes.IParametricCurve
    {
        ControlPoint this[int index]{ get; set; }
        int Count { get; }
        bool Dirty { get; set; }
    }
}