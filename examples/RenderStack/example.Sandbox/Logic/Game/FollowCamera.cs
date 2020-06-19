using RenderStack.Math;
using RenderStack.Scene;

namespace example.Sandbox
{
    //  A third person camera which follows behind the player
    class FollowCamera : ICameraUpdate
    {
        private Camera      camera;
        private Unit        unit;
        private Vector3     lastTargetSmooth        = Vector3.Zero;
        private Vector3     lastDesiredPosSmooth    = Vector3.Zero;
        private Quaternion  lastDesiredOrientation  = Quaternion.Identity;
        private Vector3     cameraOffset            = new Vector3(0.0f, 3.0f, -5.0f);
        public Camera Camera { get { return camera; } set { if(camera != value) { camera = value; Reset(); } } }
        public Unit   Unit   { get { return unit; }   set { if(unit != value) { unit = value; Reset(); } } }

        public FollowCamera(Camera camera, Unit unit)
        {
            this.camera = camera;
            this.unit = unit;

            Reset();
        }

        public void Reset()
        {
            Matrix4     currentCamera = camera.Frame.LocalToWorld.Matrix;
            Vector3     cameraPos = currentCamera.GetColumn3(3);
            Vector3     playerPosition = unit.Model.RigidBody.Position;
            Vector3     playerBack = unit.Model.RigidBody.Orientation.GetColumn3(2);
            Vector3     playerBackPlanar = playerBack;
            playerBackPlanar.Y = 0.0f;
            playerBackPlanar = Vector3.Normalize(playerBackPlanar);

            lastDesiredPosSmooth = playerPosition - cameraOffset.Z * playerBackPlanar;
            lastDesiredPosSmooth.Y += cameraOffset.Y;

            lastTargetSmooth = unit.Model.RigidBody.Position - 3.0f * playerBackPlanar;
            Matrix4     lookAt = Matrix4.CreateLookAt(cameraPos, lastTargetSmooth, Vector3.UnitY);
            lastDesiredOrientation = Quaternion.CreateFromRotationMatrix(lookAt);
        }
        public void Update()
        {
            //  STEP 0: Get current state of camera and player
            Matrix4     currentCamera = camera.Frame.LocalToWorld.Matrix;
            Vector3     cameraPos = currentCamera.GetColumn3(3);
            Vector3     playerPosition = unit.Model.RigidBody.Position;

            //  STEP 1: Compute desired camera position, temporal smooth

            //  The desired camera position is 5.0 units behind the player,
            //  on XZ plane instead of player coordinate system.
            Vector3     playerBack = unit.Model.RigidBody.Orientation.GetColumn3(2);
            Vector3     playerBackPlanar = playerBack;
            playerBackPlanar.Y = 0.0f;
            playerBackPlanar = Vector3.Normalize(playerBackPlanar);

            Vector3     desiredPos = playerPosition - cameraOffset.Z * playerBackPlanar;

            //  And 2.5 units above player
            desiredPos.Y += cameraOffset.Y;

            //  Desired position is smoothed temporally
            Vector3     desiredPosSmooth = Vector3.Mix(lastDesiredPosSmooth, desiredPos, 0.1f);
            float distance = lastDesiredPosSmooth.Distance(desiredPos);

            //  If camera is not high enough, give more weight to desired camera height
            float dy = cameraPos.Y - playerPosition.Y;
            if(dy < 5.0f)
            {
                desiredPosSmooth.Y = 0.5f * lastDesiredPosSmooth.Y + 0.5f * desiredPos.Y;
            }

            lastDesiredPosSmooth = desiredPosSmooth;

            //  STEP 2: Compute updated camera position, temporal smooth
            //  Updated camera position is also smoothed temporally
            Vector3     newPos = 0.95f * cameraPos + 0.05f * desiredPosSmooth;

            //  If camera is not high enough, give more weight to desired camera height
            if(dy < 2.5f)
            {
                newPos.Y = 0.8f * cameraPos.Y + 0.2f * desiredPosSmooth.Y;
            }

            //  STEP 3: Compute desired camera orientation, temporal smooth 
            //  Compute desired orientation
            //  Camera should look at 3.0 units ahead of the player
            Vector3     target = playerPosition - 3.0f * playerBackPlanar;
            lastTargetSmooth = 0.95f * lastTargetSmooth + 0.05f * target;
            Matrix4     lookAt = Matrix4.CreateLookAt(newPos, lastTargetSmooth, Vector3.UnitY);

            Quaternion  currentOrientation = Quaternion.CreateFromRotationMatrix(currentCamera);
            Quaternion  desiredOrientation = Quaternion.CreateFromRotationMatrix(lookAt);
            desiredOrientation = Quaternion.Lerp(lastDesiredOrientation, desiredOrientation, 0.05f);
            desiredOrientation = Quaternion.Normalize(desiredOrientation);
            lastDesiredOrientation = desiredOrientation;

            //  STEP 4: Compute updated camera orientation
            //Quaternion  newOrientation = Quaternion.Slerp(currentOrientation, desiredOrientation, 0.5f);
            Quaternion  newOrientation = desiredOrientation;
            Matrix4     newCamera = Matrix4.CreateFromQuaternion(newOrientation);

            newCamera.SetColumn3(3, newPos);
            camera.Frame.LocalToParent.Set(newCamera);
            camera.Frame.UpdateHierarchicalNoCache();
        }
    }
}