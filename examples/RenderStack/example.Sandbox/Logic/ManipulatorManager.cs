using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using RenderStack.Geometry.Shapes;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    public enum Axis
    {
        X = 0,
        Y,
        Z,
        W,
        None
    }
    public class Manipulator
    {
        private ManipulatorManager manager;

        private Frame transform = new Frame();
        private Model XCylinder;
        private Model XCone;
        private Model YCylinder;
        private Model YCone;
        private Model ZCylinder;
        private Model ZCone;           
        private Model parent;

        public Model Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
                transform.Parent = parent.Frame;
            }
        }

        private Transform   initialLocalToParent;
        private Transform   initialLocalToWorld;

        private bool active;

        public  bool Active 
        {
            get
            {
                return active;
            }
            set
            {
                if(value != active)
                {
                    SceneManager sceneManager = manager.SceneManager;
                    sceneManager.UpdateShadowMap = true;
                    active = value;
                    if(active)
                    {
                        if(sceneManager.IdGroups.Contains(manager.ManipulatorGroup) == false)
                        {
                            sceneManager.IdGroups.Add(manager.ManipulatorGroup);
                        }
                    }
                    else
                    {
                        if(sceneManager.IdGroups.Contains(manager.ManipulatorGroup) == true)
                        {
                            sceneManager.IdGroups.Remove(manager.ManipulatorGroup);
                        }
                    }
                }
            } 
        }
        public  Ray         AxisRayInWorld          { get; set; }
        //public  float       InitialOffsetOnAxis     { get; set; }
        public  Vector3     InitialPositionInWorld  { get; set; }
        public  Axis        ActiveAxis              { get; set; }
        public  Frame       ReferenceFrame          { get; set; }
        public  string      ReferenceLabel          { get; set; }

        public  Axis        PickActiveAxis(Model @object)
        {
            if(@object == XCylinder || @object == XCone)
            {
                ActiveAxis = Axis.X;
            }
            else if(@object == YCylinder || @object == YCone)
            {
                ActiveAxis = Axis.Y;
            }
            else if(@object == ZCylinder || @object == ZCone)
            {
                ActiveAxis = Axis.Z;
            }
            else
            {
                ActiveAxis = Axis.None;
            }
            return ActiveAxis;
        }

        public void UpdateScale()
        {
            // \todo Which camera to use here?
            Matrix4 viewToWorld     = manager.SceneManager.Camera.Frame.LocalToWorld.Matrix;
            Matrix4 modelToWorld    = transform.LocalToWorld.Matrix;
            Vector3 cameraPosition  = new Vector3(viewToWorld._03, viewToWorld._13, viewToWorld._23);
            Vector3 modelPosition   = new Vector3(modelToWorld._03, modelToWorld._13, modelToWorld._23);
            float distance = modelPosition.Distance(cameraPosition);
            transform.LocalToParent.SetScale(2.0f * distance);
        }
        public void BeginAxisMove(Vector3 initialPositionInWorld)
        {
            if(Parent == null)
            {
                Services.Get<TextRenderer>().DebugLine("BeginAxisMove: Parent == null");
                return;
            }

            Parent.Frame.Updated = false;
            //Parent.Frame.UpdateHierarchical();

            initialLocalToWorld = new Transform(Parent.Frame.LocalToWorld);
            initialLocalToParent = new Transform(Parent.Frame.LocalToParent);
            InitialPositionInWorld = initialPositionInWorld;

            //  TODO Compute initial offset on axis
            //InitialPositionInLocal = Parent.Frame.LocalToWorld.InverseMatrix.TransformPoint(initialPositionInWorld);

            Vector3 localAxis = Vector3.UnitX;
            switch(ActiveAxis)
            {
                case Axis.X: localAxis = Vector3.UnitX; break;
                case Axis.Y: localAxis = Vector3.UnitY; break;
                case Axis.Z: localAxis = Vector3.UnitZ; break;
            }
            AxisRayInWorld = new Ray(
                InitialPositionInWorld,
                initialLocalToWorld.Matrix.TransformDirection(localAxis)
            );

            Parent.Frame.LocalToParent.Set(
                initialLocalToParent.Matrix,
                initialLocalToParent.InverseMatrix
            );
            Parent.Frame.Updated = false;
            //Parent.Frame.UpdateHierarchical();
        }
        public void UpdateAxisMove(Transform worldToClip, Viewport viewport, int px, int py)
        {
            var textRenderer = Services.Get<TextRenderer>();
            if(Parent == null)
            {
                textRenderer.DebugLine("UpdateAxisMove: Parent == null");
                return;
            }

            float z0;
            float z1;

            //  P1 is the start position in screen space
            Vector2 P1  = worldToClip.Matrix.ProjectToScreenSpace(
                InitialPositionInWorld, 
                viewport.X, 
                viewport.Y, 
                viewport.Width,
                viewport.Height,
                out z0
            );
            //  P2 is point along the axis direction in screen space
            Vector2 P2  = worldToClip.Matrix.ProjectToScreenSpace(
                InitialPositionInWorld + AxisRayInWorld.Direction, 
                viewport.X, 
                viewport.Y, 
                viewport.Width,
                viewport.Height,
                out z1
            );

            //  Normalize P1P2 in screen space
            //  Vector2 P1P2 = Vector2.Normalize(P2 - P1);
            //  P2 = P1 + P1P2;

            //  P3 is mouse position in screen space
            Vector2 P3  = new Vector2((float)px, (float)py);

            //  Determine point on line P1..P2 closest to P3 - in screen space
            float n = (P3.X - P1.X) * (P2.X - P1.X) + (P3.Y - P1.Y) * (P2.Y - P1.Y);
            float d = P1.DistanceSquared(P2);
            float u = n / d;

            //  If points are very close, don't update
            //  If start position is outside view frustum, don't update
            //manager.Renderer.DebugLine("UpdateAxisMove: d = " + d.ToString("#.000") + " z0 = " + z0.ToString("#.000") + " z1 = " + z1.ToString("#.000"));
            if(d < 0.001f || z0 < 0.0f || z1 > 1.0f)
            {
                return;
            }

            Vector2 screenSpaceClosest = P1 + u * (P2 - P1);

            //  Create a 3D line that goes from near to far plane
            //  through the closest point in screenspace.
            Vector3 root    = worldToClip.InverseMatrix.UnProject(
                screenSpaceClosest.X, 
                screenSpaceClosest.Y, 
                0.0f, 
                viewport.X,
                viewport.Y,
                viewport.Width,
                viewport.Height
            );
            Vector3 tip     = worldToClip.InverseMatrix.UnProject(
                screenSpaceClosest.X, 
                screenSpaceClosest.Y, 
                1.0f, 
                viewport.X,
                viewport.Y,
                viewport.Width,
                viewport.Height
            );

            //  Form a pick ray from the above
            Vector3 pickDirection   = Vector3.Normalize(tip - root);
            Ray     pickRay         = new Ray(root, pickDirection);
            float   pickRayDistance;
            float   axisRayDistance;

            //  Find closest point along the axis to the picking ray
            bool    exists = pickRay.FindClosestPoints(
                AxisRayInWorld, 
                out pickRayDistance, 
                out axisRayDistance
            );

            if(exists && pickRayDistance > 0.0f)
            {
                Vector3 closestOnPickRay    = root + pickDirection * pickRayDistance;
                Vector3 closestOnAxisRay    = InitialPositionInWorld + AxisRayInWorld.Direction * axisRayDistance;

                float distance = closestOnAxisRay.Distance(closestOnPickRay);
                textRenderer.DebugLine(distance.ToString("#.000"));

                distance = closestOnAxisRay.Distance(InitialPositionInWorld);
                textRenderer.DebugLine("closestOnAxisRay.Distance(InitialPositionInWorld) = " + distance.ToString("#.000"));

                Vector3 delta               = AxisRayInWorld.Direction * axisRayDistance;
                Matrix4 translation;
                Matrix4 inverseTranslation;
                Matrix4.CreateTranslation( delta.X,  delta.Y,  delta.Z, out translation);
                Matrix4.CreateTranslation(-delta.X, -delta.Y, -delta.Z, out inverseTranslation);

                Matrix4 newLocalToWorld = translation * initialLocalToWorld.Matrix;
                Matrix4 newWorldToLocal = initialLocalToWorld.InverseMatrix * inverseTranslation;

                Vector3 newPositionInWorld = newLocalToWorld.GetColumn3(3);
                textRenderer.DebugLine("newPositionInWorld = " + newPositionInWorld.ToString());

                if(Parent.Frame.Parent != null)
                {
                    Matrix4 oldWorldToParent = Parent.Frame.Parent.LocalToWorld.InverseMatrix;
                    Matrix4 newLocalToParent = oldWorldToParent * newLocalToWorld;
                    Parent.Frame.LocalToParent.Set(newLocalToParent);
                }
                else
                {
                    Parent.Frame.LocalToParent.Set(newLocalToWorld);
                }

                Parent.Frame.Updated = false;  //  TODO better tracking, add deactivation kind of stuff

                manager.SceneManager.MovePhysicsObject(Parent, newLocalToWorld);
            }
        }

        public Manipulator(ManipulatorManager manager)
        {
            this.manager = manager;

            Active      = false;
            ActiveAxis  = Axis.None;

            var redMaterial   = manager.MaterialManager["ManipulatorRed"];
            var greenMaterial = manager.MaterialManager["ManipulatorGreen"];
            var blueMaterial  = manager.MaterialManager["ManipulatorBlue"];

            var renderer = manager.Renderer;

            float cylinderWidth = 0.02f;
            float coneStart = 0.4f;
            float coneWidth = 0.2f;
            float scale     = 0.15f;
            //float scale     = 1.0f;

            var axisCylinder = new GeometryMesh( 
                new Cylinder(0.0, coneStart * scale, cylinderWidth * scale, 20),
                NormalStyle.CornerNormals
            );
            var axisCone = new GeometryMesh(
                new Cone(coneStart * scale, 1.0 * scale, coneWidth * scale, 0.0, true, false, 20, 4),
                NormalStyle.CornerNormals
            );

            Matrix4 translation = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            Matrix4 toYAxis = Matrix4.CreateRotation((float)Math.PI * 0.5f, new Vector3(0.0f, 0.0f, 1.0f));
            Matrix4 toZAxis = Matrix4.CreateRotation(-(float)Math.PI * 0.5f, new Vector3(0.0f, 1.0f, 0.0f));

            XCylinder = new Model("X-Axis Cylinder",  axisCylinder, redMaterial);
            XCone     = new Model("X-Axis Cone",      axisCone,     redMaterial);
            YCylinder = new Model("Y-Axis Cylinder",  axisCylinder, greenMaterial,  toYAxis);
            YCone     = new Model("Y-Axis Cone",      axisCone,     greenMaterial,  toYAxis);
            ZCylinder = new Model("Z-Axis Cylinder",  axisCylinder, blueMaterial,   toZAxis);
            ZCone     = new Model("Z-Axis Cone",      axisCone,     blueMaterial,   toZAxis);

            XCylinder.Frame.Parent  = transform;
            XCone.Frame.Parent      = transform;
            YCylinder.Frame.Parent  = transform;
            YCone.Frame.Parent      = transform;
            ZCylinder.Frame.Parent  = transform;
            ZCone.Frame.Parent      = transform;

            manager.ManipulatorGroup.Add(XCylinder);
            manager.ManipulatorGroup.Add(XCone);
            manager.ManipulatorGroup.Add(YCylinder);
            manager.ManipulatorGroup.Add(YCone);
            manager.ManipulatorGroup.Add(ZCylinder);
            manager.ManipulatorGroup.Add(ZCone);
        }
    }

    public class ManipulatorManager : Service
    {
        public override string Name
        {
            get { return "ManipulatorManager"; }
        }

        MaterialManager         materialManager;
        IRenderer               renderer;
        LineRenderer            lineRenderer;
        SceneManager            sceneManager;
        SelectionManager        selectionManager;

        private Manipulator     manipulator;
        private Group           manipulatorGroup = new Group("ManipulatorManager.manipulatorGroup");

        public Model            ManipulatorModel    { get; set; }

        public MaterialManager  MaterialManager     { get { return materialManager; } }
        public IRenderer        Renderer            { get { return renderer; } }
        public SceneManager     SceneManager        { get { return sceneManager; } }
        public Manipulator      Manipulator         { get { return manipulator; } }
        public Group            ManipulatorGroup    { get { return manipulatorGroup; } }

        public void Connect(
            LineRenderer        lineRenderer,
            MaterialManager     materialManager,
            IRenderer           renderer,
            SceneManager        sceneManager,
            SelectionManager    selectionManager
        )
        {
            this.lineRenderer = lineRenderer;
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.selectionManager = selectionManager;

            InitializationDependsOn(materialManager);
            InitializationDependsOn(renderer);
            InitializationDependsOn(sceneManager);
        }

        private DepthState      depthReplace = new DepthState();
        private DepthState      depthGreater = new DepthState();
        private DepthState      depthLEqual = new DepthState();
        private DepthState      depthAlways = new DepthState();
        private BlendState      blendHidden = new BlendState();
        private StencilState    stencilSetOne = new StencilState();
        private StencilState    stencilSetTwo = new StencilState();
        private StencilState    stencilRequireOne = new StencilState();
        private StencilState    stencilRequireTwo = new StencilState();
        private MaskState       maskNoRGBADepth = new MaskState();
        private MaskState       maskDepthOnly = new MaskState();
        private MaskState       maskNoDepth = new MaskState();
        protected override void InitializeService()
        {
            //  Use this to only write to stencil
            maskNoRGBADepth.Red     = false;
            maskNoRGBADepth.Green   = false;
            maskNoRGBADepth.Blue    = false;
            maskNoRGBADepth.Alpha   = false;
            maskNoRGBADepth.Depth   = false;

            //  Use this to only write to depth
            maskDepthOnly.Red     = false;
            maskDepthOnly.Green   = false;
            maskDepthOnly.Blue    = false;
            maskDepthOnly.Alpha   = false;
            maskDepthOnly.Depth   = true;

            maskNoDepth.Red     = true;
            maskNoDepth.Green   = true;
            maskNoDepth.Blue    = true;
            maskNoDepth.Alpha   = true;
            maskNoDepth.Depth   = false;

            //  Use this to set stencil one where depth test passes
            stencilSetOne.Enabled               = true;
            stencilSetOne.Separate              = false;
            stencilSetOne.Front.Function        = StencilFunction.Always;
            stencilSetOne.Front.Reference       = 1;
            stencilSetOne.Front.TestMask        = 0xffff;
            stencilSetOne.Front.WriteMask       = 0xffff;
            stencilSetOne.Front.StencilFailOp   = StencilOp.Keep;  // never fails with stencilfunc always
            stencilSetOne.Front.ZFailOp         = StencilOp.Keep;
            stencilSetOne.Front.ZPassOp         = StencilOp.Replace;

            //  Use this to require stencil to equal 1 to pass
            stencilRequireOne.Enabled               = true;
            stencilRequireOne.Separate              = false;
            stencilRequireOne.Front.Function        = StencilFunction.Equal;
            stencilRequireOne.Front.Reference       = 1;
            stencilRequireOne.Front.TestMask        = 0xffff;
            stencilRequireOne.Front.WriteMask       = 0xffff;
            stencilRequireOne.Front.StencilFailOp   = StencilOp.Keep;
            stencilRequireOne.Front.ZFailOp         = StencilOp.Keep;
            stencilRequireOne.Front.ZPassOp         = StencilOp.Keep;

            //  Use this to set stencil two where depth test passes
            stencilSetTwo.Enabled               = true;
            stencilSetTwo.Separate              = false;
            stencilSetTwo.Front.Function        = StencilFunction.Always;
            stencilSetTwo.Front.Reference       = 2;
            stencilSetTwo.Front.TestMask        = 0xffff;
            stencilSetTwo.Front.WriteMask       = 0xffff;
            stencilSetTwo.Front.StencilFailOp   = StencilOp.Keep;   // never fails with stencilfunc always
            stencilSetTwo.Front.ZFailOp         = StencilOp.Keep;
            stencilSetTwo.Front.ZPassOp         = StencilOp.Replace;

            //  Use this to require stencil to equal 2 to pass
            stencilRequireTwo.Enabled               = true;
            stencilRequireTwo.Separate              = false;
            stencilRequireTwo.Front.Function        = StencilFunction.Equal;
            stencilRequireTwo.Front.Reference       = 2;
            stencilRequireTwo.Front.TestMask        = 0xffff;
            stencilRequireTwo.Front.WriteMask       = 0xffff;
            stencilRequireTwo.Front.StencilFailOp   = StencilOp.Keep;
            stencilRequireTwo.Front.ZFailOp         = StencilOp.Keep;
            stencilRequireTwo.Front.ZPassOp         = StencilOp.Keep;

            //  Use this to reset depth values to 1.0 where renderer
            depthReplace.Enabled = true;
            depthReplace.Function = DepthFunction.Always;
            depthReplace.Near = 1.0f;
            depthReplace.Far = 1.0f;

            depthLEqual.Enabled = true;
            depthLEqual.Function = DepthFunction.Lequal;

            depthAlways.Enabled = true;
            depthAlways.Function = DepthFunction.Always;

            //  Use this for greater depth function
            depthGreater.Function = DepthFunction.Gequal;
            depthGreater.Enabled = true;

            blendHidden.Enabled = true;
            blendHidden.Color = new Vector4(0.4f, 0.4f, 0.4f, 0.8f);
            blendHidden.RGB.EquationMode = BlendEquationMode.FuncAdd;
            blendHidden.RGB.SourceFactor = BlendingFactorSrc.ConstantColor;
            blendHidden.RGB.DestinationFactor = BlendingFactorDest.ConstantAlpha;
            
            var redMaterial = materialManager.MakeMaterial("ManipulatorRed", "manipulator");
            redMaterial.Floats("surface_diffuse_reflectance_color").Set(1.0f, 0.0f, 0.0f);
            redMaterial.Sync();
            var greenMaterial = materialManager.MakeMaterial("ManipulatorGreen", "manipulator");
            greenMaterial.Floats("surface_diffuse_reflectance_color").Set(0.0f, 1.0f, 0.0f);
            greenMaterial.Sync();
            var blueMaterial = materialManager.MakeMaterial("ManipulatorBlue", "manipulator");
            blueMaterial.Floats("surface_diffuse_reflectance_color").Set(0.0f, 0.0f, 1.0f);
            blueMaterial.Sync();

            manipulator = new Manipulator(this);
        }

        public void SetReferenceModel(Model referenceModel)
        {
            if(referenceModel != null)
            {
                manipulator.ReferenceLabel = referenceModel.Name;
                manipulator.ReferenceFrame = referenceModel.Frame;
            }
            else
            {
                manipulator.ReferenceLabel = null;
                manipulator.ReferenceFrame = null;
            }
        }
        public void PickAxis(Vector3 position)
        {
            manipulator.PickActiveAxis(ManipulatorModel);
            if(manipulator.ActiveAxis != Axis.None)
            {
                manipulator.BeginAxisMove(position);
            }
        }
        public void Render()
        {
            if(manipulator.Active == false)
            {
                return;
            }

            Model model = selectionManager.Models.First();
            manipulator.Parent = model;

            //  ManipulatorGroup is not updated by SceneManager.Update()
            sceneManager.UpdateOncePerFrame(ManipulatorGroup);

            renderer.CurrentGroup = ManipulatorGroup;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;

            //  Step 0: Lock depth, stencil and mask states
            Material.LockDepthState = true;
            Material.LockStencilState = true;
            Material.LockMaskState = true;
            Material.LockBlendState = true;

            //  Hidden manipulator set stencil 1
            maskNoRGBADepth.Execute();
            depthGreater.Execute();
            stencilSetOne.Execute();
            BlendState.Default.Execute();
            renderer.RenderGroup();

            //  Visible manipulator set stencil 2
            maskNoRGBADepth.Execute();
            depthLEqual.Execute();
            stencilSetTwo.Execute();
            renderer.RenderGroup();

            //  Clear depth for manipulator
            maskDepthOnly.Execute();
            depthReplace.Execute();
            StencilState.Default.Execute();
            renderer.RenderGroup();

            //  Set depth for manipulator
            maskDepthOnly.Execute();
            depthLEqual.Execute();
            renderer.RenderGroup();

            //  Step 4: Render visible parts
            MaskState.Default.Execute();
            depthLEqual.Execute();
            stencilRequireTwo.Execute();
            BlendState.Default.Execute();
            renderer.RenderGroup();

            //  Step 5: Render with hidden depth and blend states
            maskNoDepth.Execute();
            depthLEqual.Execute();
            stencilRequireOne.Execute();
            blendHidden.Execute();
            renderer.RenderGroup();

            //  Unlock depth and blend states
            Material.LockDepthState = false;
            Material.LockStencilState = false;
            Material.LockMaskState = false;
            Material.LockBlendState = false;

            Vector3 o = model.Frame.LocalToWorld.Matrix.GetColumn3(3);
            Vector3 xStart = o; xStart.X -= 20.0f;
            Vector3 yStart = o;
            Vector3 zStart = o; zStart.Z -= 20.0f;
            Vector3 xEnd = o; xEnd.X += 20.0f;
            Vector3 yEnd = o; yEnd.Y  = 0.0f;
            Vector3 zEnd = o; zEnd.Z += 20.0f;
            lineRenderer.Begin();
            lineRenderer.Line(xStart, xEnd, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            lineRenderer.Line(yStart, yEnd, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            lineRenderer.Line(zStart, zEnd, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            lineRenderer.End();
        }
    }
}
