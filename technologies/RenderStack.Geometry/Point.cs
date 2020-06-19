using System;
using System.Collections.Generic;

namespace RenderStack.Geometry
{
    /// Point is the basic building block for geometries.
    /// Points are needed to build Polygons.
    /// When a Point is connected to a Polygon, a Corner is
    /// created in the middle to serve as the connection.
    /// \note Stable.
    [Serializable]
    public class Point
    {
        public List<Corner> Corners { get; } = new List<Corner>();
    }
}
