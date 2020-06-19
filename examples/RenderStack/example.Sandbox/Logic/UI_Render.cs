using System.Linq;
using RenderStack.Math;
using RenderStack.Mesh;
using example.Renderer;

namespace example.Sandbox
{
    public partial class UserInterfaceManager
    {
        public float InterFrameTime;
        public float UpdateTime;
        public float PhysicsTime;
        public int FixedUpdates;
        public int FixedUpdatesLess;
        public int FixedUpdatesMore;

        public void Render()
        {
            highLevelRenderer.Use2DCamera();

            Render2D();

            renderer.SetTexture("t_font", textRenderer.TextBuffer.FontStyle.Texture);
            mouse.X = window.Mouse.X;
            mouse.Y = window.Height - 1 - window.Mouse.Y;
            renderer.Requested.Material = uiMaterial;
            layer.Update();
            layer.Draw(this);
            hoverArea = layer.GetHit(new Vector2(mouse.X, mouse.Y));
            if(hoverArea == layer)
            {
                System.Diagnostics.Debugger.Break();
                hoverArea = layer.GetHit(new Vector2(mouse.X, mouse.Y));
            }
            renderer.Global.Floats("add_color").Set(0.0f, 0.0f, 0.0f);
            renderer.Global.Sync();
        }

        private void Render2D()
        {
            RenderStack.Graphics.Debug.WriteLine("=== Render2D()");

            if(ShowShadowMaps && (shadowRenderer != null))
            {
                shadowRenderer.VisualizeShadowmaps();
            }

#if false
            if(RenderStack.Graphics.Configuration.useOpenRL)
            {
                var rl = Services.Get<OpenRLRenderer>();
                rl.Update();
                rl.Visualize();

                rl.Update();
                rl.Visualize();
            }
#endif

            if(textRenderer != null)
            {
                Render2DTexts();
            }
        }

        private void RenderExtraInfo()
        {
            /*var depthStencilVisualizer = Services.Get<DepthStencilVisualizer>();
            if(depthStencilVisualizer != null)
            {
                depthStencilVisualizer.VisualizeDepth();
            }*/

            //Services.Instance.TextRenderer.DebugLine("HoverPosition = " + selectionManager.HoverPosition);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            //sb.Append(statistics.FrameTime);
            sb.Append((window.RenderTime * 1000.0f).ToString("0.00"));
            sb.Append(" ");
            sb.Append(InterFrameTime.ToString("0.00"));
            textRenderer.DebugLine(sb.ToString());
            sb = new System.Text.StringBuilder();

            {
                var mon = renderer.Timers.AMDHardwareMonitor;
                mon.Update();
                if(mon.Temperature != null)
                {
                    for(int i = 0; i < mon.Temperature.Length; ++i)
                    {
                        var t = mon.Temperature[i];
                        textRenderer.DebugLine("AMD core " + i + ": " + t.ToString("0.00"));
#if false
                        if(t > 74.0f)
                        {
                            window.TargetRenderFrequency = 30.0f;
                        }
                        else
                        {
                            window.TargetRenderFrequency = 60.0f;
                        }
#endif
                    }
                }
            }

            if(Configuration.physics)
            {
                sb.Append(FixedUpdates);
                sb.Append(" fixed updated (");
                sb.Append(FixedUpdatesLess);
                sb.Append(" less, ");
                sb.Append(FixedUpdatesMore);
                sb.Append(" more) ");
                sb.Append(sceneManager.PhysicsObjects.Count);
                sb.Append(" physics objects");
            }
            if(RuntimeConfiguration.curveTool)
            {
                if(curveTool.LockEditHandle)
                {
                    sb.Append(" locked");
                }
            }

            if(selectionManager.HoverGroup.Models.Count > 0)
            {
                Model m = selectionManager.HoverGroup.Models.First();
                sb.Append(" " + m.Name);
            }

            if(selectionManager.HoverPolygon != null)
            {
                sb.Append(" " + selectionManager.HoverPolygon.Corners.Count.ToString() + "-gon");
            }

            textRenderer.DebugLine(sb.ToString());
            if(Renderer.Configuration.AMDGPUPerf)
            {
                textRenderer.DebugLine("GPUTime: " + AMDGPUPerf.Instance["GPUTime"]);
                textRenderer.DebugLine("GPUBusy: " + AMDGPUPerf.Instance["GPUBusy"]);
                textRenderer.DebugLine("ShaderBusy: " + AMDGPUPerf.Instance["ShaderBusy"]);
                textRenderer.DebugLine("ShaderBusyVS: " + AMDGPUPerf.Instance["ShaderBusyVS"]);
                textRenderer.DebugLine("ShaderBusyPS: " + AMDGPUPerf.Instance["ShaderBusyPS"]);
                textRenderer.DebugLine("PSTexBusy: " + AMDGPUPerf.Instance["PSTexBusy"]);
                textRenderer.DebugLine("PSExportStalls: " + AMDGPUPerf.Instance["PSExportStalls"]);
                textRenderer.DebugLine("PSTexInstCount: " + AMDGPUPerf.Instance["PSTexInstCount"]);
                textRenderer.DebugLine("PSPixelsIn: " + AMDGPUPerf.Instance["PSPixelsIn"]);
                textRenderer.DebugLine("PSPixelsOut: " + AMDGPUPerf.Instance["PSPixelsOut"]);
                textRenderer.DebugLine("HiZQuadsCulled: " + AMDGPUPerf.Instance["HiZQuadsCulled"]);
                textRenderer.DebugLine("ZUnitStalled: " + AMDGPUPerf.Instance["ZUnitStalled"]);
            }
            var timerRenderer = Services.Get<TimerRenderer>();
            if(timerRenderer != null)
            {
                timerRenderer.Render();
            }
        }

        private void RenderHover()
        {
            //  Render hover name right next to mouse
            if(
                (selectionManager.HoverModel != null) &&
                !string.IsNullOrEmpty(selectionManager.HoverModel.Name)
            )
            {
                // \todo improve
                int px; int py;
                highLevelRenderer.MouseInScreen(out px, out py);
                textRenderer.TextBuffer.Print(
                    px + 20, 
                    py, 
                    0.0f, 
                    selectionManager.HoverModel.Name
                );

                renderer.Requested.Mesh = textRenderer.TextBuffer.Mesh;
                renderer.RenderCurrent();
            }
        }

        private void Render2DTexts()
        {
            renderer.Requested.Mesh     = textRenderer.TextBuffer.Mesh;
            renderer.Requested.Material = textRenderer.Material;
            renderer.Requested.Program  = textRenderer.Material.Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetTexture("t_font", textRenderer.TextBuffer.FontStyle.Texture);
            renderer.SetFrame(renderer.DefaultFrame);

            if(FlyMode)
            {
                textRenderer.DebugLine("Flying");
            }
            if(LockMouse)
            {
                textRenderer.DebugLine("Mouse Look Mode - Press ESC to unlock");
                textRenderer.DebugLine("Use WASD and QE to move, space to jump - F to toggle fly mode");
                textRenderer.DebugLine("Left mouse button: Remove - Right mouse button: Add");
                textRenderer.DebugLine("");
            }
            else
            {
                textRenderer.DebugLine("Free Mouse Mode - Press ESC to lock mouse look");
                textRenderer.DebugLine("Use WASD and QE to move, space to jump - F to toggle fly mode");
                textRenderer.DebugLine("");
            }

            if(RuntimeConfiguration.guiExtraInfo)
            {
                RenderExtraInfo();
            }

            if(
                RuntimeConfiguration.hoverModelName && 
                (selectionManager != null)
            )
            {
                RenderHover();
            }

            //renderer.DebugLine("Location: " + HoverPosition.X + ", " + HoverPosition.Y + ", " + HoverPosition.Z);

            textRenderer.DrawDebugLines();
        }
    }
}

#if false
        private void VisualizeHemicube(HemicubeRenderer hemicubeRenderer)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            renderer.Requested.Frame    = renderer.DefaultFrame;
            renderer.Requested.Mesh     = quadRenderer.Mesh;
            renderer.Requested.Material = materialManager["Textured"];
            renderer.Requested.Program  = renderer.Programs["Textured"];
            renderer.Requested.MeshMode = MeshMode.PolygonFill;

            quadRenderer.Begin();
            quadRenderer.HemiCubeCrossX(hemicubeRenderer);
            quadRenderer.End();
            materialManager["Textured"].Parameters["texture"] = hemicubeRenderer.Texture(0);
            renderer.RenderCurrent();

            quadRenderer.Begin();
            quadRenderer.HemiCubeCrossY(hemicubeRenderer);
            quadRenderer.End();
            materialManager["Textured"].Parameters["texture"] = hemicubeRenderer.Texture(1);
            renderer.RenderCurrent();

            quadRenderer.Begin();
            quadRenderer.HemiCubeCrossZ(hemicubeRenderer);
            quadRenderer.End();
            materialManager["Textured"].Parameters["texture"] = hemicubeRenderer.Texture(2);
            renderer.RenderCurrent();
        }
#endif
