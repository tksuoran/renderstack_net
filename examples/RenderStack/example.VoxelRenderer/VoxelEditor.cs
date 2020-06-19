using System;

using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

namespace example.VoxelRenderer
{
    public class VoxelEditor : Service
    {
        public override string Name
        {
            get { return "CubeEditor"; }
        }

        IRenderer   renderer;
        Map         map;

        public void Connect(IRenderer renderer, Map map)
        {
            this.renderer = renderer;
            this.map = map;
        }
        protected override void InitializeService()
        {
        }

        public void RenderCubes(int px, int py, Camera camera)
        {
            map.RenderChunks(camera);

            //Vector3     start       =  sceneManager.Camera.Frame.LocalToWorld.Matrix.TransformPoint(Vector3.Zero);
            //Vector3     viewVector  = -sceneManager.Camera.Frame.LocalToWorld.Matrix.GetColumn3(2);
            //Vector3     end         = start + viewVector * 10.0f;
            //Vector3     end         = sceneManager.Camera.Frame.LocalToWorld.Matrix.TransformPoint(new Vector3(0,0,-2000));

            Vector3 root = camera.WorldToClip.InverseMatrix.UnProject(
                px, 
                py, 
                0.0f, 
                renderer.Requested.Viewport.X,
                renderer.Requested.Viewport.Y,
                renderer.Requested.Viewport.Width,
                renderer.Requested.Viewport.Height
            );
            Vector3 tip = camera.WorldToClip.InverseMatrix.UnProject(
                px, 
                py, 
                1.0f, 
                renderer.Requested.Viewport.X,
                renderer.Requested.Viewport.Y,
                renderer.Requested.Viewport.Width,
                renderer.Requested.Viewport.Height
            );

            IVector3 pos;
            IVector3 facing;
            EditPos.Y = 255;
            bool intersect = map.Intersect(root, tip, out pos, out facing);
            if(intersect)
            {
                byte block = map[pos.X, (byte)pos.Y, pos.Z];
                var textRenderer = BaseServices.Get<Renderer.TextRenderer>();
                if(textRenderer != null)
                {
                    textRenderer.DebugLine(
                        "Block ahead: " + pos.ToString() + BlockType.Name(block) + " facing " + facing.ToString()
                    );
                }
                EditPos = pos + facing;
            }
        }

        public void TryPut()
        {
            if(EditPos.Y != 255)
            {
                map.Put(EditPos.X, (byte)EditPos.Y, EditPos.Z, BlockType.Stone);
            }
        }

        public IVector3 EditPos;
        public const float ZeroTolerance = 1e-6f;

        public static bool RayIntersectsPlane(
            ref Vector3 start, 
            ref Vector3 direction, 
            ref Plane plane, 
            out float distance
        )
        {
            float direction_;
            Vector3.Dot(ref plane.Normal, ref direction, out direction_);

            if(Math.Abs(direction_) < ZeroTolerance)
            {
                distance = 0f;
                return false;
            }

            float position;
            Vector3.Dot(ref plane.Normal, ref start, out position);
            distance = (plane.D - position) / direction_;

            if(distance < 0f)
            {
                if(distance < -ZeroTolerance)
                {
                    distance = 0;
                    return false;
                }

                distance = 0f;
            }

            return true;
        }
    }
}
