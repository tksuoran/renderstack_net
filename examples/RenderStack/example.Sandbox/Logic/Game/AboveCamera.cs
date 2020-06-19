using System;
using System.Collections.Generic;

using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;
using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;

using example.Renderer;

namespace example.Sandbox
{
    //  A simple camera updater that is always exactly above player
    class AboveCamera : ICameraUpdate
    {
        private Camera      camera;
        private Unit        unit;

        public Camera Camera { get { return camera; } set { if(camera != value) { camera = value; Reset(); } } }
        public Unit   Unit   { get { return unit; }   set { if(unit != value) { unit = value; Reset(); } } }

        public AboveCamera(Camera camera, Unit unit)
        {
            this.camera = camera;
            this.unit = unit;

            Reset();
        }

        public void Reset()
        {
            Update();
        }
        public void Update()
        {
            Matrix4     currentCamera = camera.Frame.LocalToWorld.Matrix;
            Vector3     playerPosition = unit.Model.RigidBody.Position;
            Vector3     newPos = playerPosition;
            newPos.Y = 30.0f;

            Matrix4     newCamera = Matrix4.CreateLookAt(newPos, playerPosition, -Vector3.UnitZ);

            newCamera.SetColumn3(3, newPos);
            camera.Frame.LocalToParent.Set(newCamera);
            camera.Frame.UpdateHierarchicalNoCache();
        }
    }
}