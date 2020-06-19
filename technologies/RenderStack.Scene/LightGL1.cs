#if false
using System;
using System.Diagnostics;

using RenderStack.Math;
using RenderStack.Graphics;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Scene
{
    public class LightsUniforms
    {
        public static IUniformBuffer    UniformBuffer;
        public static UniformBlockGL    UniformBlock;
        public static Ints              Count;
        public static Floats            Exposure;
        public static Floats            Bias;
        public static Floats            AmbientLightColor;
        public static Floats            WorldToLight;
        public static Floats            WorldToShadow;
        public static Floats            Direction;
        public static Floats            Color;
    };

    [Serializable]
    /// \note Experimental
    public class Light
    {
        private Camera      camera;
        public  Camera      Camera      { get { return camera; } }

        public  string      Name        { get { return Camera.Name; } set { Camera.Name = value; } }
        public  Frame       Frame       { get { return Camera.Frame; } }
        public  Projection  Projection  { get { return Camera.Projection; } }

        private static Matrix4  Texture = new Matrix4(
            0.5f, 0.0f, 0.0f, 0.5f,
            0.0f, 0.5f, 0.0f, 0.5f,
            0.0f, 0.0f, 0.5f, 0.5f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
        private static Matrix4  TextureInverse = new Matrix4(
            2.0f, 0.0f, 0.0f, -1.0f,
            0.0f, 2.0f, 0.0f, -1.0f,
            0.0f, 0.0f, 2.0f, -1.0f,
            0.0f, 0.0f, 0.0f,  1.0f
        );

        private Transform   worldToShadow = new Transform(Matrix4.Identity, Matrix4.Identity);
        public Transform    WorldToShadow { get { return worldToShadow; } }
        public int          LightIndex;

        public Light(int lightIndex)
        {
            LightIndex = lightIndex;
            camera = new Camera();
        }

        public void UpdateFrame()
        {
            Camera.UpdateFrame();

            //parameters.LightToWorld.Set(ViewToWorld.Matrix);
            LightsUniforms.WorldToLight.Set(Frame.LocalToWorld.InverseMatrix);

            /*parameters.ViewPositionInWorld.Set(
                ViewToWorld.Matrix._03,
                ViewToWorld.Matrix._13,
                ViewToWorld.Matrix._23
            );*/
            WorldToShadow.Set(
                Texture * Camera.WorldToClip.Matrix,
                Camera.WorldToClip.InverseMatrix * TextureInverse
            );
            LightsUniforms.WorldToShadow.Set(LightIndex, WorldToShadow.Matrix);
        }

        /// Viewport has been modified, updates transformations.
        public void UpdateViewport(Viewport viewport)
        {
            Camera.UpdateViewport(viewport);
        }
    }
}
#endif