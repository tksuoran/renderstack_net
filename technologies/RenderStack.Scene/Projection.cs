using System;
using System.Diagnostics;
using RenderStack.Math;
using RenderStack.Graphics;

namespace RenderStack.Scene
{
    public class Projection
    {
        private ProjectionType  projectionType;
        private Floats          near = new Floats(1, 1);
        private Floats          far  = new Floats(1, 1);
        private Floats          fovX = new Floats(1, 1);
        private Floats          fovY = new Floats(1, 1);

        public Floats           NearParameter   { get { return near; } }
        public Floats           FarParameter    { get { return far; } }
        public Floats           FovXParameter   { get { return fovX; } }
        public Floats           FovYParameter   { get { return fovY; } }
        public float            Near            { get { return near.X; } set { near.X = value; } }
        public float            Far             { get { return far.X; } set { far.X = value; } }
        public ProjectionType   ProjectionType  { get { return projectionType; } set { projectionType = value; } }
        public float            FovXRadians     { get { return fovX.X; } set { fovX.X = value; } }
        public float            FovYRadians     { get { return fovY.X; } set { fovY.X = value; } }
        public float            OrthoLeft       { get; set; }
        public float            OrthoTop        { get; set; }
        public float            OrthoWidth      { get; set; }
        public float            OrthoHeight     { get; set; }
        public float            FrustumLeft     { get; set; }
        public float            FrustumRight    { get; set; }
        public float            FrustumBottom   { get; set; }
        public float            FrustumTop      { get; set; }

        private StereoParameters stereoParameters = new StereoParameters();
        public StereoParameters StereoParameters { get { return stereoParameters; } }

        public Projection()
        {
            Near            = 1.0f;
            Far             = 100.0f;
            ProjectionType  = ProjectionType.PerspectiveVertical;
            FovXRadians     = (float)(0.5 * System.Math.PI);
            FovYRadians     = (float)(0.5 * System.Math.PI);
            OrthoWidth      = 1.0f;
            OrthoHeight     = 1.0f;
        }

        public void Update(Transform transform, Viewport viewport)
        {
            switch(ProjectionType)
            {
                case ProjectionType.Perspective:
                {
                    transform.SetPerspective(
                        FovXRadians,
                        FovYRadians,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.PerspectiveHorizontal:
                {
                    transform.SetPerspectiveHorizontal(
                        FovXRadians,
                        viewport.AspectRatio,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.PerspectiveVertical:
                {
                    transform.SetPerspectiveVertical(
                        FovYRadians,
                        viewport.AspectRatio,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.OrthogonalHorizontal:
                {
                    transform.SetOrthographic(
                        -0.5f * OrthoWidth,
                         0.5f * OrthoWidth,
                        -0.5f * OrthoWidth / viewport.AspectRatio,
                         0.5f * OrthoWidth / viewport.AspectRatio,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.OrthogonalVertical:
                {
                    transform.SetOrthographic(
                        -0.5f * OrthoHeight * viewport.AspectRatio,
                         0.5f * OrthoHeight * viewport.AspectRatio,
                        -0.5f * OrthoHeight,
                         0.5f * OrthoHeight,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.Orthogonal:
                {
                    transform.SetOrthographic(
                        -0.5f * OrthoWidth,
                         0.5f * OrthoWidth,
                        -0.5f * OrthoHeight,
                         0.5f * OrthoHeight,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.OrthogonalRectangle:
                {
                    transform.SetOrthographic(
                        OrthoLeft,
                        OrthoLeft + OrthoWidth,
                        OrthoTop,
                        OrthoTop + OrthoHeight,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.GenericFrustum:
                {
                    transform.SetFrustum(
                        FrustumLeft,
                        FrustumRight,
                        FrustumBottom,
                        FrustumTop,
                        Near,
                        Far
                    );
                    break;
                }
                case ProjectionType.StereoscopicHorizontal:
                {
                    float fov = FovXRadians;
                    fov = System.Math.Max(fov, 0.01f);
                    fov = System.Math.Min(fov, (float)(System.Math.PI * 0.99));

                    float z = System.Math.Max(stereoParameters.ViewportCenter.Z, 0.01f);

                    float w = 2.0f * z * (float)(System.Math.Tan(fov * 0.5f));
                    float h = w / viewport.AspectRatio;

                    StereoParameters.ViewPlaneSize.Set(w, h);

                    transform.SetProjection(
                        StereoParameters.EyeSeparation.X,
                        StereoParameters.Perspective.X,
                        Near,
                        Far,
                        w,
                        h,
                        new Vector3(stereoParameters.ViewportCenter.X, stereoParameters.ViewportCenter.Y, stereoParameters.ViewportCenter.Z),
                        new Vector3(stereoParameters.EyePosition.X, stereoParameters.EyePosition.Y, stereoParameters.EyePosition.Z)
                    );
                    break;
                }
                case ProjectionType.StereoscopicVertical:
                {
                    float fov = FovYRadians;
                    fov = System.Math.Max(fov, 0.01f);
                    fov = System.Math.Min(fov, (float)(System.Math.PI * 0.99));

                    float z = System.Math.Max(stereoParameters.ViewportCenter.Z, 0.01f);

                    float h = 2.0f * z * (float)(System.Math.Tan(fov * 0.5f));
                    float w = h * viewport.AspectRatio;

                    StereoParameters.ViewPlaneSize.Set(w, h);

                    transform.SetProjection(
                        StereoParameters.EyeSeparation.X,
                        StereoParameters.Perspective.X,
                        Near,
                        Far,
                        w,
                        h,
                        new Vector3(stereoParameters.ViewportCenter.X, stereoParameters.ViewportCenter.Y, stereoParameters.ViewportCenter.Z),
                        new Vector3(stereoParameters.EyePosition.X, stereoParameters.EyePosition.Y, stereoParameters.EyePosition.Z)
                    );
                    break;
                }
            }
        }
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            sb.Append("Near: ");
            sb.Append(Near);
            sb.Append(" Far: ");
            sb.Append(Far);
            sb.Append(" Projection: ");
            sb.Append(ProjectionType);
            sb.Append(" ");
            switch(ProjectionType)
            {
                case ProjectionType.Orthogonal:
                {
                    sb.Append(" OrthoWidth: ");
                    sb.Append(OrthoWidth);
                    sb.Append(" OrthoHeight: ");
                    sb.Append(OrthoHeight);
                    break;
                }
                case ProjectionType.OrthogonalHorizontal:
                {
                    sb.Append(" OrthoWidth: ");
                    sb.Append(OrthoWidth);
                    break;
                }
                case ProjectionType.OrthogonalRectangle:
                {
                    sb.Append(" OrthoLeft: ");
                    sb.Append(OrthoLeft);
                    sb.Append(" OrthoTop: ");
                    sb.Append(OrthoTop);
                    sb.Append(" OrthoWidth: ");
                    sb.Append(OrthoWidth);
                    sb.Append(" OrthoHeight: ");
                    sb.Append(OrthoHeight);
                    break;
                }
                case ProjectionType.OrthogonalVertical:
                {
                    sb.Append(" OrthoHeight: ");
                    sb.Append(OrthoHeight);
                    break;
                }
                case ProjectionType.Other:
                {
                    break;
                }
                case ProjectionType.Perspective:
                {
                    sb.Append(" FovXRadians: ");
                    sb.Append(FovXRadians);
                    sb.Append(" FovYRadians: ");
                    sb.Append(FovYRadians);
                    break;
                }
                case ProjectionType.PerspectiveHorizontal:
                {
                    sb.Append(" FovXRadians: ");
                    sb.Append(FovXRadians);
                    break;
                }
                case ProjectionType.PerspectiveVertical:
                {
                    sb.Append(" FovYRadians: ");
                    sb.Append(FovYRadians);
                    break;
                }
                case ProjectionType.StereoscopicHorizontal:
                {
                    sb.Append(" FovXRadians: ");
                    sb.Append(FovXRadians);
                    sb.Append(" EyeSeparation: ");
                    sb.Append(StereoParameters.EyeSeparation.X);
                    sb.Append(" ViewportCenter.Z: ");
                    sb.Append(stereoParameters.ViewportCenter.Z);
                    break;
                }
                case ProjectionType.StereoscopicVertical:
                {
                    sb.Append(" FovYRadians: ");
                    sb.Append(FovYRadians);
                    sb.Append(" EyeSeparation: ");
                    sb.Append(StereoParameters.EyeSeparation.X);
                    sb.Append(" ViewportCenter.Z: ");
                    sb.Append(stereoParameters.ViewportCenter.Z);
                    break;
                }
                case ProjectionType.GenericFrustum:
                {
                    sb.Append(" FrustumLeft: ");
                    sb.Append(FrustumLeft);
                    sb.Append(" FrustumRight: ");
                    sb.Append(FrustumRight);
                    sb.Append(" FrustumBottom: ");
                    sb.Append(FrustumBottom);
                    sb.Append(" FrustumTop: ");
                    sb.Append(FrustumTop);
                    break;
                }
                default:
                {
                    break;
                }
            }
            return sb.ToString();
        }
    }
}