using System;
using System.Diagnostics;
using System.Collections.Generic;

#if ASSET_MONITOR
using RenderStack.Graphics.AssetMonitor;
#endif
using RenderStack.Services;

using example.Brushes;
using example.Renderer;
using example.Loading;
using example.VoxelRenderer;

namespace example.Sandbox
{
    public class Services : BaseServices
    {
        public static readonly Services Instance = new Services();

        private LoadingScreenManager    loading;

        public void Cleanup()
        {
            ClearServices();
        }
        public void Message(string message)
        {
            if(loading != null) loading.Message(message);
        }
        public void Initialize(Application application)
        {
            loading = application.Loader;

            //  Essential services
#if ASSET_MONITOR
            var assetMonitor            = new AssetMonitor();
#endif
            var depthStencilVisualizer  = new DepthStencilVisualizer();
            var game                    = new Game();
            var highLevelRenderer       = new HighLevelRenderer();
            var idRenderer              = Configuration.idBuffer ? new IDRenderer() : null;
            var lineRenderer            = new LineRenderer();
            var manipulatorManager      = new ManipulatorManager();
            var map                     = Configuration.voxelTest ? new Map() : null;
            var voxelEditor             = Configuration.voxelTest ? new VoxelEditor() : null;
#if false
            var openRLRenderer          = RenderStack.Graphics.Configuration.useOpenRL ? new OpenRLRenderer() : null;
#endif
            var mainSceneRenderer       = new MainSceneRenderer();
            var materialManager         = new MaterialManager();        //  Cannot start without
            var renderer                = RendererFactory.Create();     //  Cannot start without
            var sceneManager            = new SceneManager();           //  Cannot start without
            var selectionManager        = Configuration.selection ? new SelectionManager() : null;
            var shadowRenderer          = (RenderStack.Graphics.Configuration.useGl1 == false) ? new ShadowRenderer() : null;
            var sounds                  = new Sounds();  //  Cannot start without - always created, even if disabled
            var statistics              = new Statistics();
            var stereoscopicRenderer    = Configuration.stereo ? new StereoscopicRenderer() : null;
            var styleManager            = Configuration.graphicalUserInterface ? new StyleManager() : null;

            var textRenderer            = new TextRenderer();
            var timerRenderer           = new TimerRenderer();
            var userInterfaceManager    = new UserInterfaceManager();
            var updateManager           = new UpdateManager();

            var curveTool               = new CurveTool.CurveTool();
#if false
            aoHemicubeRenderer          = new HemicubeRenderer(32, PixelFormat.Red, PixelInternalFormat.R8);
            cubeRenderer                = new CubeRenderer(512);
            hemisphericalRenderer       = new HemisphericalRenderer(256);
            pointViewHemicubeRenderer   = new HemicubeRenderer(512, PixelFormat.Rgba, PixelInternalFormat.Rgba8);
#endif
            var aoManager       = new AmbientOcclusionManager();
            var brushManager    = Configuration.brushes ? new BrushManager() : null;
            var operations      = new Operations();
            var operationStack  = new OperationStack();

            var physicsDrag = Configuration.physics ? new PhysicsDrag() : null;

            //  Sometimes objective C would be useful..
            //if(aoHemicubeRenderer != null)          aoHemicubeRenderer.Connect(renderer);
            if(aoManager != null)                   aoManager.Connect(null, renderer, sceneManager);
            if(brushManager != null)                brushManager.Connect(materialManager, textRenderer);
            //if(cubeRenderer != null)                cubeRenderer.Connect(renderer);
            if(curveTool != null)                   curveTool.Connect(materialManager, renderer, sceneManager);
            if(depthStencilVisualizer != null)      depthStencilVisualizer.Connect(materialManager, renderer);
            if(game != null)                        game.Connect(highLevelRenderer, materialManager, renderer, sceneManager);
            if(highLevelRenderer != null)           highLevelRenderer.Connect(
                curveTool, 
                idRenderer, 
                lineRenderer, 
                mainSceneRenderer,
                manipulatorManager, 
                map, 
                materialManager,
                renderer, 
                sceneManager, 
                selectionManager, 
                shadowRenderer, 
                stereoscopicRenderer,
                userInterfaceManager, 
                voxelEditor,
                application as OpenTK.GameWindow
            );
            if(idRenderer != null)                  idRenderer.Connect(application, materialManager, renderer, sceneManager, textRenderer, userInterfaceManager);
            if(lineRenderer != null)                lineRenderer.Connect(renderer);
            if(mainSceneRenderer != null)           mainSceneRenderer.Connect(lineRenderer, manipulatorManager, renderer, sceneManager, selectionManager);
            if(manipulatorManager != null)          manipulatorManager.Connect(lineRenderer, materialManager, renderer, sceneManager, selectionManager);
            if(map != null)                         map.Connect(materialManager, renderer);
            if(materialManager != null)             materialManager.Connect(renderer);
#if false
            if(openRLRenderer != null)              openRLRenderer.Connect(materialManager, renderer, sceneManager);
#endif
            if(operations != null)                  operations.Connect(brushManager, materialManager, operationStack, sceneManager, selectionManager, sounds, userInterfaceManager);
            if(operationStack != null)              operationStack.Connect(application);
            if(physicsDrag != null)                 physicsDrag.Connect(lineRenderer, selectionManager, sceneManager, application);
            //if(pointViewHemicubeRenderer != null)   pointViewHemicubeRenderer.Connect(renderer);
            if(renderer != null)                    renderer.Connect(application as OpenTK.GameWindow);
            if(sceneManager != null)                sceneManager.Connect(brushManager, materialManager, renderer, selectionManager, sounds, textRenderer, userInterfaceManager, application as OpenTK.GameWindow);
            if(shadowRenderer != null)              shadowRenderer.Connect(materialManager, renderer, sceneManager);
            if(selectionManager != null)            selectionManager.Connect(curveTool, idRenderer, lineRenderer, manipulatorManager, operations, physicsDrag, renderer, sceneManager, sounds, userInterfaceManager, application);
            if(stereoscopicRenderer != null)        stereoscopicRenderer.Connect(highLevelRenderer, mainSceneRenderer, materialManager, renderer, sceneManager, application as OpenTK.GameWindow);
            if(styleManager != null)                styleManager.Connect(materialManager, renderer, textRenderer);
            if(textRenderer != null)                textRenderer.Connect(application, renderer);
            if(timerRenderer != null)               timerRenderer.Connect(renderer, textRenderer, application as OpenTK.GameWindow);
            if(updateManager != null)               updateManager.Connect(sceneManager, sounds, userInterfaceManager);
            if(userInterfaceManager != null)        userInterfaceManager.Connect(aoManager, brushManager, curveTool, highLevelRenderer, materialManager, operations, operationStack, renderer, sceneManager, selectionManager, shadowRenderer, sounds, statistics, textRenderer, application);
            if(voxelEditor != null)                 voxelEditor.Connect(renderer, map);

            //  This helps us get text rendering capability earlier and thus we can
            //  display loading progress
            //if(framebufferManager != null)          framebufferManager.InitializationDependsOn(textRenderer);
            if(lineRenderer != null)                lineRenderer.InitializationDependsOn(textRenderer);
            if(operationStack != null)              operationStack.InitializationDependsOn(textRenderer);
            if(selectionManager != null)            selectionManager.InitializationDependsOn(textRenderer);
            if(sounds != null)                      sounds.InitializationDependsOn(textRenderer);

#if ASSET_MONITOR
            if(renderer != null)                    renderer.InitializationDependsOn(assetMonitor);
#endif

            Action<object> initializeService = new Action<object>(InitializeService);
            HashSet<IService> uninitialized = new HashSet<IService>(ServicesSet);
            HashSet<IService> removeSet = new HashSet<IService>();
            int count = uninitialized.Count;
            var stopwatch = new Stopwatch();
            while(uninitialized.Count > 0)
            {
                Trace.TraceInformation("====== Service initialization pass");
                removeSet.Clear();              
                foreach(var service in uninitialized)
                {
                    if(service.Dependencies.Count == 0)
                    {
                        string message = "Initializing " + service.Name + " (" + count + " left)";
                        Trace.TraceInformation(message);
                        if(
                            (Configuration.loadingWindow == false) &&
                            (textRenderer != null) && 
                            textRenderer.IsInitialized
                        )
                        {
                            // \todo this is broken, there is no camera
                            //textRenderer.Message(message);
                        }
                        if(loading != null) loading.Message(message);
                        if(service.InitializeInMainThread || true)
                        {
                            service.Initialize();
                            if(loading != null) loading.Step();

                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();

                        }
                        else
                        {
                            ThreadManager2.Instance.AddTask(initializeService, service);
                        }
                        removeSet.Add(service);
                        --count;
                    }
                }
                ThreadManager2.Instance.Execute();
                if(removeSet.Count == 0)
                {
                    Trace.TraceError("Circular Service Dependencies Detected");
                    foreach(var service in uninitialized)
                    {
                        Trace.TraceInformation(service.ToString());
                    }
                    Debugger.Break();
                }
                uninitialized.ExceptWith(removeSet);
                foreach(var other in uninitialized)
                {
                    other.Dependencies.ExceptWith(removeSet);
                }
            }
        }
        protected override void InitializeService(object obj)
        {
            Service service = (Service)obj;
            service.Initialize();
            if(loading != null) loading.Step();
        }
    }

}
