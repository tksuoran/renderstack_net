using System;

namespace RenderStack.Scene
{
    [Serializable]
    /// \note Mostly stable, somewhat experimental
    public enum ProjectionType
    {
        Other = 0,              //  Projection is done by shader in unusual way - hemispherical for example
        PerspectiveHorizontal,
        PerspectiveVertical,
        Perspective,            //  Uses both horizontal and vertical fov and ignores aspect ratio
        OrthogonalHorizontal,
        OrthogonalVertical,
        Orthogonal,             //  Uses both horizontal and vertical size and ignores aspect ratio, O-centered
        OrthogonalRectangle,    //  Like above, not O-centered, uses X and Y as corner
        GenericFrustum,         //  Generic frustum
        StereoscopicHorizontal,
        StereoscopicVertical
    };
}
