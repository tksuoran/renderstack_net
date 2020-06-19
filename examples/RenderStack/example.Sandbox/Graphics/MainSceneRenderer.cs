using OpenTK.Graphics.OpenGL;
using RenderStack.Mesh;
using RenderStack.Services;
using example.Renderer;

namespace example.Sandbox
{
    public class MainSceneRenderer : Service
    {
        public override string  Name
        {
            get { return "MainSceneRenderer"; }
        }

        LineRenderer            lineRenderer;
        ManipulatorManager      manipulatorManager;
        IRenderer               renderer;
        SceneManager            sceneManager;
        SelectionManager        selectionManager;

        public void Connect(
            LineRenderer        lineRenderer,
            ManipulatorManager  manipulatorManager,
            IRenderer           renderer,
            SceneManager        sceneManager,
            SelectionManager    selectionManager
        )
        {
            this.lineRenderer = lineRenderer;
            this.manipulatorManager = manipulatorManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.selectionManager = selectionManager;
        }

        protected override void InitializeService()
        {
        }

        public void Render(int viewIndex)
        {
            renderer.RenderCurrentClear();
            renderer.ApplyViewport();

            if(RuntimeConfiguration.doubleSided)
            {
                FaceCullState.Disabled.Execute();
                Material.LockFaceCullState = true;
            }

            Group hoverGroup        = (selectionManager   != null) ? selectionManager.HoverGroup : null;
            Group manipulatorGroup  = (manipulatorManager != null) ? manipulatorManager.ManipulatorGroup : null;
            var   selectedModels    = (selectionManager   != null) ? selectionManager.Models : null;

            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            foreach(var group in sceneManager.RenderGroups)
            {
                if(group.Visible == false)
                {
                    continue;
                }
                renderer.CurrentGroup = group;
                renderer.RenderGroup();
            }

            GL.Disable(EnableCap.PolygonOffsetFill);
            renderer.PartialGLStateResetToDefaults();

            if(selectionManager != null)
            {
                selectionManager.Render();
            }

            if(manipulatorManager != null)
            {
                manipulatorManager.Render();
            }

            if(RuntimeConfiguration.debugInfo && (selectionManager != null))
            {
                var physicsDrag = Services.Get<PhysicsDrag>();
                renderer.PartialGLStateResetToDefaults();
                if(
                    (selectionManager.HoverModel != null) || 
                    (selectedModels.Count > 0) ||
                    (
                        (physicsDrag != null) && 
                        (physicsDrag.Model != null)
                    )
                )
                {
                    lineRenderer.Render(renderer.DefaultFrame);
                }
            }

            if(RuntimeConfiguration.doubleSided)
            {
                Material.LockFaceCullState = false;
            }

            // \todo fixme renderer.RenderLineRenderer(sceneManager.DebugLineRenderer, sceneManager.DebugFrame);
        }

    }
}
