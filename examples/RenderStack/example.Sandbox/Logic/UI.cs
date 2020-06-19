//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using System.Linq;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;
using RenderStack.UI;

using example.Brushes;
using example.Renderer;
using example.UIComponents;

namespace example.Sandbox
{
    public class StereoMode
    {
        private IProgram program;

        public IProgram  Program     { get { return program; } }

        public StereoMode(IProgram program)
        {
            this.program = program;
        }
    }

    public partial class UserInterfaceManager : Service, IUIContext
    {
        #region Service
        public override string Name
        {
            get { return "UserInterfaceManager"; }
        }

        public void Connect(
            AmbientOcclusionManager aoManager,
            BrushManager            brushManager,
            CurveTool.CurveTool     curveTool,
            HighLevelRenderer       highLevelRenderer,
            MaterialManager         materialManager,
            Operations              operations,
            OperationStack          operationStack,
            IRenderer               renderer,
            SceneManager            sceneManager,
            SelectionManager        selectionManager,
            ShadowRenderer          shadowRenderer,
            Sounds                  sounds,
            Statistics              statistics,
            TextRenderer            textRenderer,
            OpenTK.GameWindow       window
        )
        {
            this.aoManager = aoManager;
            this.brushManager = brushManager;
            this.curveTool = curveTool;
            this.highLevelRenderer = highLevelRenderer;
            this.materialManager = materialManager;
            this.operations = operations;
            this.operationStack = operationStack;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.selectionManager = selectionManager;
            this.shadowRenderer = shadowRenderer;
            this.sounds = sounds;
            this.statistics = statistics;
            this.textRenderer = textRenderer;
            this.window = window;

            InitializationDependsOn(brushManager, materialManager, renderer, sceneManager, selectionManager, textRenderer);
            InitializationDependsOn(curveTool);

            initializeInMainThread = true;
        }
        #endregion
        protected override void InitializeService()
        {
            colorFill = renderer.Programs["ColorFill"];
            uiMaterial = new Material("UI Material", null, renderer.MaterialUB);
            uiMaterial.BlendState = new BlendState();
            uiMaterial.BlendState.Enabled = true;
            uiMaterial.BlendState.RGB.EquationMode = BlendEquationMode.FuncAdd;
            uiMaterial.BlendState.RGB.DestinationFactor = BlendingFactorDest.OneMinusSrcAlpha;
            uiMaterial.BlendState.RGB.SourceFactor = BlendingFactorSrc.One;
            uiMaterial.DepthState = DepthState.Disabled;
            uiMaterial.FaceCullState = FaceCullState.Disabled;

            sceneManager.MakeSimpleScene();
            CurrentScene = "Simple";

            //sceneManager.MakeCurveScene();
            //CurrentScene = "Curve";

            CurrentMaterial = "pearl";

            CurrentLightConfig = "Basic";
            AddLights();

            AddStereoModes();
            InstallInputEventHandlers();
        }
        private void AddLights()
        {
            sceneManager.ClearLights();
            if(CurrentLightConfig == "AO")
            {
                sceneManager.AddAOLights(13);
            }
            else if(CurrentLightConfig == "Basic")
            {
                sceneManager.AddBasicLights();
            }
        }

        private Dock active;
        private void SetActive(Area area)
        {
            bool add = active.Children.Contains(area.Link) == false;
            active.Children.Clear();
            if(add)
            {
                active.Add(area.Link);
            }
            layer.Update();
        }
        public void Reset()
        {
            CurrentPalette = "cupola";

            wheel = window.Mouse.WheelPrecise;

            layer = new Layer(this, window);
            layer.Name = "Root layer";

            if(Configuration.graphicalUserInterface == false)
            {
                return;
            }

            Dock verticalGroup = new Dock(Orientation.Vertical);
            active = new Dock(Orientation.Horizontal);
            #region bottom
            MenuList bottom = new MenuList(renderer, Orientation.Horizontal);
            bottom.Name = "test";

            bottom.Add(Operations());
            bottom.Add(Palette());
            bottom.Add(Scene());
            bottom.Add(Camera());
            bottom.Add(Lights());
            bottom.Add(Materials());
            bottom.Add(Noise());
            bottom.Add(Physics());

            //if(Configuration.physics)
            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "Action choice";
                choice.Style = Style.NullPadding;
                if(Configuration.physics)
                {
                    choice.Add(new ChoiceItem(renderer, "Drag"));
                    choice.Add(new ChoiceItem(renderer, "Select"));
                }
                else
                {
                    if(Configuration.physics)
                    {
                        choice.Add(new ChoiceItem(renderer, "Drag"));
                    }
                    choice.Add(new ChoiceItem(renderer, "Select"));
                }
                choice.Add(new ChoiceItem(renderer, "Add"));
                choice.Action = ActionChoiceAction;
                choice.Selected = choice.Items.First();
                bottom.Add(choice);
            }

            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "AddWithPhysics choice";
                choice.Style = Style.NullPadding;
                choice.Add(new ChoiceItem(renderer, "Add Static"));
                choice.Add(new ChoiceItem(renderer, "Add Dynamic"));
                choice.Action = AddModeChoiceAction;
                choice.Selected = choice.Items.First();
                bottom.Add(choice);
            }


            bottom.OffsetFreeSizeRelative = new Vector2( 0.5f, 0.0f);
            bottom.OffsetSelfSizeRelative = new Vector2(-0.5f, 0.0f);
            //bottom.OffsetPixels           = new Vector2( 4.0f, 4.0f);
            #endregion bottom
            verticalGroup.Add(bottom);
            verticalGroup.OffsetFreeSizeRelative = new Vector2( 0.5f, 0.0f);
            verticalGroup.OffsetSelfSizeRelative = new Vector2(-0.5f, 0.0f);
            verticalGroup.OffsetPixels           = new Vector2( 4.0f, 4.0f);
            verticalGroup.Add(active);

            Layer.Add(verticalGroup);

            if(CurrentScene != "Game")
            {
                ConnectUserControls();
            }
        }

        #region Data members
        AmbientOcclusionManager aoManager;
        BrushManager            brushManager;
        CurveTool.CurveTool     curveTool;
        HighLevelRenderer       highLevelRenderer;
        MaterialManager         materialManager;
        Operations              operations;
        OperationStack          operationStack;
        IRenderer               renderer;
        SceneManager            sceneManager;
        SelectionManager        selectionManager;
        ShadowRenderer          shadowRenderer;
        Sounds                  sounds;
        Statistics              statistics;
        TextRenderer            textRenderer;
        OpenTK.GameWindow       window;

        private Layer               layer;
        private Area                hoverArea;
        private float               wheel;
        private Material            uiMaterial;
        private IProgram            colorFill;

        public  Area                HoverArea       { get { return hoverArea; } }
        public  Dictionary<string, StereoMode> stereoModes = new Dictionary<string,StereoMode>();
        public  Layer               Layer           { get { return layer; } }

        public  bool                ShowShadowMaps  = false;
        public  bool                ShowHandles     = true;
        public  bool                AddWithPhysics  = true;

        Slider roughness;

        Slider noiseOctaves;
        Slider noiseOffset;
        Slider noiseFrequency;
        Slider noiseAmplitude;
        Slider noiseLacunarity;
        Slider noisePersistence;
        #endregion

        #region ChoiceActions

        public  StereoMode  CurrentStereoMode;
        public  string      CurrentScene = "Simple";
        public  string      CurrentPalette;
        public  string      CurrentMaterial;

        private void SceneChoiceAction(ChoiceItem selected)
        {
            System.Console.WriteLine("SceneChoiceAction " + selected.Label);
            if(CurrentScene == selected.Label)
            {
                return;
            }

            CurrentScene = selected.Label;

            // Reset these as game / tree scene will mess around with them..
            RuntimeConfiguration.gameTest = false;
            RuntimeConfiguration.curveTool = false;
            Configuration.idBuffer = true;
            userControls = sceneManager.CameraControls;
            sceneManager.Camera.Projection.ProjectionType = ProjectionType.PerspectiveVertical;
            CurrentStereoMode = stereoModes["Mono"];

            System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
            time.Start();
            if(selectionManager != null)
            {
                selectionManager.ClearSelection();
            }
            if(selected.Label == "Stereoscopic")
            {
                sceneManager.MakeStereoscopicScene();
                CurrentStereoMode = stereoModes["R/C"];
                sceneManager.Camera.Projection.ProjectionType = ProjectionType.StereoscopicVertical;
            }
            else if(selected.Label == "Polyhedra")
            {
                sceneManager.MakeBrushScene();
            }
            else if(selected.Label == "Game")
            {
                sceneManager.MakeGameScene();
            }
            else if(selected.Label == "Curve")
            {
                sceneManager.MakeCurveScene();
            }
            else if(selected.Label == "Stereo")
            {
                sceneManager.MakeStereoscopicScene();
            }
            else if(selected.Label == "Simple")
            {
                //sceneManager.MakeFloorOnlyScene();
                sceneManager.MakeSimpleScene();
            }
            else if(selected.Label == "Noise")
            {
                sceneManager.MakeNoiseScene();
                CurrentMaterial = "noisy";
                // Reset() will do this: ChooseMaterial();
                //sceneManager.MakeSuperEllipsoidScene();
            }
            else if(selected.Label == "Boxes")
            {
                sceneManager.MakeBoxScene();
            }
            else if(selected.Label == "Trees")
            {
                sceneManager.MakeTreeScene();
            }
            else if(selected.Label == "LightWave")
            {
                sceneManager.MakeLightWaveScene();
            }
            AddLights();
            time.Stop();
            Reset();
            System.Console.WriteLine("Scene change to " + selected.Label + " time: " + time.Elapsed.ToString());
        }
        private void PaletteChoiceAction(ChoiceItem selected)
        {
            CurrentPalette = selected.Label;
        }
        private void MaterialChoiceAction(ChoiceItem selected)
        {
            if(CurrentMaterial == selected.Label)
            {
                return;
            }

            CurrentMaterial = selected.Label;
            ChooseMaterial();
            UpdateNoise();
            try
            {
                foreach(Model model in selectionManager.Models)
                {
                    sceneManager.RemoveModel(model);
                    model.Batch.Material = materialManager[selected.Label];
                    sceneManager.AddModel(model);
                }
            }
            catch(System.Exception)
            {
            }
        }
        private void ChooseMaterial()
        {
            if(roughness != null)
            {
                roughness.Parameter = null;
                Material m = materialManager[CurrentMaterial];
                if(m != null)
                {
                    roughness.Parameter = m.Floats("surface_roughness");
                    roughness.Element = 0;
                }
            }
        }
        private void StereoModeChoiceAction(ChoiceItem selected)
        {
            if(stereoModes.ContainsKey(selected.Label))
            {
                CurrentStereoMode = stereoModes[selected.Label];
            }
        }
        private void ProjectionChoiceAction(ChoiceItem selected)
        {
            if(selected.Label == "Vertical")
            {
                sceneManager.Camera.Projection.ProjectionType = ProjectionType.PerspectiveVertical;
            }
            else if(selected.Label == "Horizontal")
            {
                sceneManager.Camera.Projection.ProjectionType = ProjectionType.PerspectiveHorizontal;
            }
            else if(selected.Label == "V. Stereo")
            {
                sceneManager.Camera.Projection.ProjectionType = ProjectionType.StereoscopicVertical;
            }
            else if(selected.Label == "H. Stereo")
            {
                sceneManager.Camera.Projection.ProjectionType = ProjectionType.StereoscopicHorizontal;
            }
        }
        private string CurrentLightConfig;
        private void LightConfigAction(ChoiceItem selected)
        {
            if(selected.Label == CurrentLightConfig)
            {
                return;
            }

            CurrentLightConfig = selected.Label;

            if(selected.Label == "AO")
            {
                sceneManager.ClearLights();
                sceneManager.AddAOLights(13);
            }
            else if(selected.Label == "Basic")
            {
                sceneManager.ClearLights();
                sceneManager.AddBasicLights();
            }
            Reset();
        }
        public bool AutoFocus = false;
        private void FocusChoiceAction(ChoiceItem selected)
        {
            if(selected.Label == "AutoFocus")
            {
                AutoFocus = true;
            }
            else
            {
                AutoFocus = false;
            }
        }
        public enum Action
        {
            Select,
            Drag,
            Add
        }
        public Action ButtonAction = Action.Drag;
        private void ActionChoiceAction(ChoiceItem selected)
        {
            if(selected.Label == "Select")
            {
                ButtonAction = Action.Select;
            }
            else if(selected.Label == "Drag")
            {
                ButtonAction = Action.Drag;
            }
            else if(selected.Label == "Add")
            {
                ButtonAction = Action.Add;
            }
        }
        private void AddModeChoiceAction(ChoiceItem selected)
        {
            if(selected.Label == "Add Static")
            {
                this.AddWithPhysics = false;
            }
            else if(selected.Label == "Add Dynamic")
            {
                this.AddWithPhysics = true;
            }

        }
        #endregion
        #region Actions
        private void HomeCamera(Area area)
        {
            Camera camera = sceneManager.Camera;

            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    sceneManager.Home,
                    Vector3.Zero,
                    Vector3.UnitY
                )
            );
            IFrameController cameraControls = sceneManager.CameraControls;
            cameraControls.SetTransform(camera.Frame.LocalToParent.Matrix);
            cameraControls.Clear();
        }
        private void ResetCamera(Area area)
        {
            Camera camera = sceneManager.Camera;
            StereoParameters stereo = camera.Projection.StereoParameters;

            camera.Projection.FarParameter.X  = 50.0f;
            camera.Projection.NearParameter.X = 0.01f;
            camera.Projection.FovXParameter.X = (float)(System.Math.PI * 0.5);
            camera.Projection.FovYParameter.X = (float)(System.Math.PI * 0.5);
            stereo.EyePosition.Set(0.0f, 0.0f, 0.0f);
            stereo.ViewportCenter.Set(0.0f, 0.0f, 4.0f);
            stereo.Perspective.X = 1.0f;
            stereo.EyeSeparation.X = 0.065f;
        }
        private void ReferenceStereo(Area area)
        {
            StereoParameters stereo = sceneManager.Camera.Projection.StereoParameters;

            stereo.EyeSeparation.X = stereo.ViewPlaneSize.X / 6.0f;
            stereo.ViewportCenter.Z = stereo.EyePosition.Z + stereo.ViewPlaneSize.X;
            //sceneManager.Camera.StereoParameters.EyeSeparation = 
            // s = w / 6;
            // v.z = e.z + w;
            //sceneManager.Camera.StereoParameters
        }
        private void OpSubdivide(Area area){ operations.Subdivide(); }
        private void OpSqrt3(Area area){ operations.Sqrt3(); }
        private void OpCatmull(Area area){ operations.CatmullClark(); }
        private void OpMerge(Area area){ operations.Merge(); }
        private void OpDelete(Area area){ operations.Delete(); }
        private void OpTriangulate(Area area){ operations.Triangulate(); }
        private void OpTruncate(Area area){ operations.Truncate(); }
        private void OpSmoothNormals(Area area){ operations.SmoothNormals(); }
        private void OpFlatNormals(Area area){ operations.FlatNormals(); }
        private void OpUndo(Area area){ operationStack.Undo(); }
        private void OpRedo(Area area){ operationStack.Redo(); }
        #endregion Actions

        private void AddStereoModes()
        {
            CurrentStereoMode = stereoModes["Mono"] = new StereoMode(null);
            if(Configuration.stereo)
            {
                stereoModes["R/C"] = new StereoMode(renderer.Programs["Anachrome"]);
                stereoModes["G/M"] = new StereoMode(renderer.Programs["triochrome"]);
                stereoModes["B/A"] = new StereoMode(renderer.Programs["ColorCode"]);
                stereoModes["Mix"] = new StereoMode(renderer.Programs["Blend"]);
            }
        }

        private Area Operations()
        {
            var list = new MenuList(renderer, Orientation.Vertical);

            list.Add(new Button(renderer, "Merge",         OpMerge));
            list.Add(new Button(renderer, "Delete",        OpDelete));
            list.Add(new Button(renderer, "Truncate",      OpTruncate));
            list.Add(new Button(renderer, "Triangulate",   OpTriangulate));
            list.Add(new Button(renderer, "Subdivide",     OpSubdivide));
            list.Add(new Button(renderer, "Sqrt(3)",       OpSqrt3));
            list.Add(new Button(renderer, "Catmull-Clark", OpCatmull));
            list.Add(new Button(renderer, "Smooth Normals",OpSmoothNormals));
            list.Add(new Button(renderer, "Flat Normals",  OpFlatNormals));
            list.Add(new Button(renderer, "Undo",          OpUndo));
            list.Add(new Button(renderer, "Redo",          OpRedo));

            var expand   = new Button(renderer, "Operations", SetActive);
            expand.Link = list;
            return expand;
        }
        private Area Palette()
        {
            if(brushManager == null)
            {
                return null;
            }

            var list = new MenuList(renderer, Orientation.Vertical);

            var palette = new Palette(brushManager, materialManager, renderer);
            palette.Name = "Brush palette";

            list.Add(palette);
            CurrentPalette = brushManager.Dictionaries.First().Key;

            var choice = new Choice(Orientation.Vertical);
            choice.Style = Style.NullPadding;
            choice.Action = PaletteChoiceAction;
            foreach(var kvp in brushManager.Dictionaries)
            {
                var item = new ChoiceItem(renderer, kvp.Key);
                choice.Add(item);
                if(kvp.Key == "cupola")
                {
                    choice.Selected = item;
                }
            }
            list.Add(choice);

            var expand = new Button(renderer, "Palette", SetActive);
            expand.Link = list;
            return expand;
        }

        private Area Materials()
        {
            if(materialManager == null)
            {
                return null;
            }

            var list = new MenuList(renderer, Orientation.Vertical);

            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            var choice = new Choice(Orientation.Vertical);
            choice.Style = Style.NullPadding;
            choice.Action = MaterialChoiceAction;
            string[] materials = { "pearl", "gold", "red", "green", "cyan", "blue", "magenta", "noisy", "pink", "EdgeLines", "transparent", "Grid"};

            foreach(var name in materials)
            {
                var item = new ChoiceItem(renderer, name);
                choice.Add(item);
                if(name == CurrentMaterial)
                {
                    choice.Selected = item;
                }
            }
            roughness = new Slider(renderer, "Roughness", null, 0, 0.0f, 1.0f, 0.0f, 1.0f);
            roughness.IsLogarithmic = true;
            ChooseMaterial();

            //var colorPicker = new ColorPicker(materialManager, renderer);
            //list.Add(colorPicker);
            list.Add(roughness);
            list.Add(choice);

            var expand = new Button(renderer, "Materials", SetActive);
            expand.Link = list;
            return expand;
        }
        private void UpdateNoise()
        {
            var m = materialManager[CurrentMaterial];
            if(m == null)
            {
                return;
            }
            noiseOctaves    .Parameter = m.Floats("octaves");
            noiseOffset     .Parameter = m.Floats("offset");
            noiseFrequency  .Parameter = m.Floats("frequency");
            noiseAmplitude  .Parameter = m.Floats("amplitude");
            noiseLacunarity .Parameter = m.Floats("lacunarity");
            noisePersistence.Parameter = m.Floats("persistence");
        }
        private Area Noise()
        {
            if(materialManager == null)
            {
                return null;
            }

            var list = new MenuList(renderer, Orientation.Vertical);

            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            noisePersistence = new Slider(renderer, "Persistence", null, 0, 0.0f, 2.0f, 0.0f, 2.0f);
            noiseLacunarity  = new Slider(renderer, "Lacunarity",  null, 0, 0.0f, 8.0f, 0.0f, 8.0f);
            noiseAmplitude   = new Slider(renderer, "Amplitude",   null, 0, 0.0f, 1.0f, 0.0f, 1.0f);
            noiseFrequency   = new Slider(renderer, "Frequency",   null, 0, 0.0f, 6.0f, 0.0f, 6.0f);
            noiseOffset      = new Slider(renderer, "Offset",      null, 0, 0.0f, 1.0f, 0.0f, 1.0f);
            noiseOctaves     = new Slider(renderer, "Octaves",     null, 0, 0.0f, 8.0f, 0.0f, 8.0f);

            list.Add(noisePersistence);
            list.Add(noiseLacunarity );
            list.Add(noiseAmplitude  );
            list.Add(noiseFrequency  );
            list.Add(noiseOffset     );
            list.Add(noiseOctaves    );
            UpdateNoise();

            var expand = new Button(renderer, "Noise", SetActive);
            expand.Link = list;
            return expand;
        }
        private Area Scene()
        {
            var list  = new MenuList(renderer, Orientation.Vertical);

            list.Name = "list";
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            /*if(Configuration.curveTool)*/
            {
                list.Add(new Slider(renderer, "Tension",    curveTool.TCB,   0,    -1.0f, 1.0f, -1.0f, 1.0f));
                list.Add(new Slider(renderer, "Continuity", curveTool.TCB,   1,    -1.0f, 1.0f, -1.0f, 1.0f));
                list.Add(new Slider(renderer, "Bias",       curveTool.TCB,   2,    -1.0f, 1.0f, -1.0f, 1.0f));
            }

            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "Scene choice";
                choice.Style = Style.NullPadding;
                choice.Action = SceneChoiceAction;
                string[] scenes = { "Simple", "Polyhedra", "Stereoscopic", "Noise", "Trees", "Game", "Curve" };
                foreach(var label in scenes)
                {
                    var choiceItem = choice.Add(new ChoiceItem(renderer, label));
                    if(label == CurrentScene)
                    {
                        choice.Selected = choiceItem;
                    }
                }
                list.Add(choice);
            }

            var expand = new Button(renderer, "Scene", SetActive);
            expand.Link = list;
            return expand;
        }
        private Area Lights()
        {
            var list = new MenuList(renderer, Orientation.Vertical);

            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            list.Add(new Slider(renderer, "Exposure", LightsUniforms.Exposure, 0, 0.0f, 10.0f, 0.0f, 10.0f));

            float r = 0.02f;
            var bias_x = new Slider(renderer, "Bias x", LightsUniforms.Bias, 0, -r, r, -r, r);
            var bias_y = new Slider(renderer, "Bias y", LightsUniforms.Bias, 1, -r, r, -r, r);
            var bias_z = new Slider(renderer, "Bias z", LightsUniforms.Bias, 2, -r, r, -r, r);
            var bias_w = new Slider(renderer, "Bias w", LightsUniforms.Bias, 3, -r, r, -r, r);
            /*bias_x.IsLogarithmic = true;
            bias_y.IsLogarithmic = true;
            bias_z.IsLogarithmic = true;
            bias_w.IsLogarithmic = true;*/
            list.Add(bias_x);
            list.Add(bias_y);
            list.Add(bias_z);
            list.Add(bias_w);

            for(int i = 0; i < sceneManager.RenderGroup.Lights.Count; ++i)
            {
                list.Add(
                    new Slider(
                        renderer, "Radiance " + (i + 1).ToString(), LightsUniforms.Color, 4 * i + 3, 0.0f, 10.0f, 0.0f, 10.0f
                    )
                );
            }
            list.Add(new Slider(renderer, "Ambient Red",    LightsUniforms.AmbientLightColor, 0, 0.0f, 4.0f, 0.0f, 4.0f));
            list.Add(new Slider(renderer, "Ambient Green",  LightsUniforms.AmbientLightColor, 1, 0.0f, 4.0f, 0.0f, 4.0f));
            list.Add(new Slider(renderer, "Ambient Blue",   LightsUniforms.AmbientLightColor, 2, 0.0f, 4.0f, 0.0f, 4.0f));

            if(Configuration.stereo)
            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "Config";
                choice.Style = Style.NullPadding;
                var ao = choice.Add(new ChoiceItem(renderer, "AO"));
                var basic = choice.Add(new ChoiceItem(renderer, "Basic"));
                choice.Action = LightConfigAction;
                if(CurrentLightConfig == "AO") choice.Selected = ao;
                if(CurrentLightConfig == "Basic") choice.Selected = basic;
                list.Add(choice);
            }

            var expand = new Button(renderer, "lights", SetActive);
            expand.Link = list;
            return expand;
        }
        private Area Camera()
        {
            var list  = new MenuList(renderer, Orientation.Vertical);

            list.Name = "list";
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            Camera camera = sceneManager.Camera;
            StereoParameters stereo = camera.Projection.StereoParameters;

            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;
            stereo.ViewportCenter.Z = 4.0f;
            list.Add(new Slider(renderer, "Fov X",              camera.Projection.FovXParameter,   0,    0.0f, (float)(System.Math.PI), 0.0f, 180.0f));
            list.Add(new Slider(renderer, "Fov Y",              camera.Projection.FovYParameter,   0,    0.0f, (float)(System.Math.PI), 0.0f, 180.0f));
            if(Configuration.stereo)
            {
                list.Add(new Slider(renderer, "Eye Separation", stereo.EyeSeparation,   0,   -1.0f,   1.0f,   -1.0f,   1.0f));
                list.Add(new Slider(renderer, "Perspective",    stereo.Perspective,     0,    0.0f,   1.0f,    0.0f,   1.0f));
                list.Add(new Slider(renderer, "Center X",       stereo.ViewportCenter,  0,   -4.0f,   4.0f,   -4.0f,   4.0f));
                list.Add(new Slider(renderer, "Center Y",       stereo.ViewportCenter,  1,   -4.0f,   4.0f,   -4.0f,   4.0f));
                list.Add(new Slider(renderer, "Center Z",       stereo.ViewportCenter,  2,    0.0f,  20.0f,    0.0f,  20.0f));
                list.Add(new Slider(renderer, "Eye X",          stereo.EyePosition,     0,   -4.0f,   4.0f,   -4.0f,   4.0f));
                list.Add(new Slider(renderer, "Eye Y",          stereo.EyePosition,     1,   -4.0f,   4.0f,   -4.0f,   4.0f));
                list.Add(new Slider(renderer, "Eye Z",          stereo.EyePosition,     2,  -20.0f,  20.0f,  -20.0f,  20.0f));
                //list.Add(new Slider(renderer, "Near",           camera.NearParameter,   0,    0.0f,  10.0f,    0.0f,  10.0f));
                //list.Add(new Slider(renderer, "Far",            camera.FarParameter,    0,    0.0f, 100.0f,    0.0f, 100.0f));
            }

            if(Configuration.stereo)
            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "Focus choice";
                choice.Style = Style.NullPadding;
                choice.Add(new ChoiceItem(renderer, "AutoFocus"));
                choice.Add(new ChoiceItem(renderer, "Manual"));
                choice.Action = FocusChoiceAction;
                choice.Selected = choice.Items.First();
                list.Add(choice);
            }

            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "FOV choice";
                choice.Style = Style.NullPadding;
                var dict = new Dictionary<string,ProjectionType>();
                dict["Vertical"]    = ProjectionType.PerspectiveVertical;
                dict["Horizontal"]  = ProjectionType.PerspectiveHorizontal;
                dict["V. Stereo"]   = ProjectionType.StereoscopicVertical;
                dict["H. Stereo"]   = ProjectionType.StereoscopicHorizontal;
                foreach(var kvp in dict)
                {
                    var choiceItem = choice.Add(new ChoiceItem(renderer, kvp.Key));
                    if(kvp.Value == sceneManager.Camera.Projection.ProjectionType)
                    {
                        choice.Selected = choiceItem;
                    }
                }
                choice.Action = ProjectionChoiceAction;
                list.Add(choice);
            }

            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "StereoMode choice";
                choice.Style = Style.NullPadding;
                foreach(var kvp in stereoModes)
                {
                    ChoiceItem choiceItem = choice.Add(new ChoiceItem(renderer, kvp.Key));
                    if(kvp.Value == CurrentStereoMode)
                    {
                        choice.Selected = choiceItem;
                    }
                }
                choice.Action = StereoModeChoiceAction;
                list.Add(choice);
            }

#if false
            // \todo stereo material
            if(Configuration.stereo)
            {
                list.Add(new Slider(renderer, "Saturation", renderer.Global.Floats("saturation"),   0,  0.0f, 1.0f, 0.0f, 1.0f));
                list.Add(new Slider(renderer, "Contrast",   renderer.Global.Floats("contrast"),     0,  0.0f, 1.0f, 0.0f, 1.0f));
                list.Add(new Slider(renderer, "Deghost",    renderer.Global.Floats("deghost"),      0,  0.0f, 1.0f, 0.0f, 1.0f));
            }
#endif

            {
                Dock dock = new Dock(Orientation.Horizontal);
                dock.Name = "Reset Home dock";
                dock.Style = Style.NullPadding;

                Button reset = new Button(renderer, "Reset");
                reset.Action = ResetCamera;
                dock.Add(reset);

                /*Button reference = new Button(renderer, "Reference");
                reference.Action = ReferenceStereo;
                dock.Add(reference);*/

                Button home = new Button(renderer, "Home");
                home.Action = HomeCamera;
                dock.Add(home);

                list.Add(dock);
            }

            var expand = new Button(renderer, "Camera", SetActive);
            expand.Link = list;
            return expand;
        }
        private Area Physics()
        {
            var list  = new MenuList(renderer, Orientation.Vertical);

            list.Name = "list";
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            list.Add(new Slider(renderer, "Restitution",    sceneManager.Restitution,       0,    0.0f,   1.0f,    0.0f,   1.0f));
            list.Add(new Slider(renderer, "S. Friction",    sceneManager.StaticFriction,    0,    0.0f,   1.0f,    0.0f,   1.0f));
            list.Add(new Slider(renderer, "D. Friction",    sceneManager.DynamicFriction,   0,    0.0f,   1.0f,    0.0f,   1.0f));
            list.Add(new Slider(renderer, "Gravity",        sceneManager.Gravity,           0,  -20.0f,  20.0f,  -20.0f,  20.0f));
            list.Add(new Slider(renderer, "Angular Damp",   sceneManager.AngularDamping,    0,    0.0f,   1.0f,    0.0f,   1.0f));
            list.Add(new Slider(renderer, "Linear Damp",    sceneManager.LinearDamping,     0,    0.0f,   1.0f,    0.0f,   1.0f));

            var expand = new Button(renderer, "Physics", SetActive);
            expand.Link = list;
            return expand;
        }
    }
}
