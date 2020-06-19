//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Linq;

using RenderStack.Graphics;
using RenderStack.Mesh;
using RenderStack.Math;
using RenderStack.Scene;
using RenderStack.UI;

namespace example.UI
{
    public class StereoMode
    {
        private ProgramDeprecated program;

        public ProgramDeprecated Program { get { return program; } }

        public StereoMode(ProgramDeprecated program)
        {
            this.program = program;
        }
    }

    public partial class UserInterfaceManager : Service, IUIContext
    {
        public override string Name
        {
            get { return "UserInterfaceManager"; }
        }

        #region Data members
        // Services
        MaterialManager         materialManager;
        Renderer                renderer;
        SceneManager            sceneManager;
        OpenTK.GameWindow       window;

        private FrameController cameraControls = new FrameController();
        private Layer           layer;
        private Area            hoverArea;
        private Style           menuStyle;
        private ProgramDeprecated         textured;

        public  Area            HoverArea       { get { return hoverArea; } }
        public  FrameController CameraControls  { get { return cameraControls; } }
        public  Layer           Layer           { get { return layer; } }
        public  Dictionary<string, StereoMode> stereoModes = new Dictionary<string,StereoMode>();
        public  StereoMode      CurrentStereoMode;
        #endregion

        public void Connect(
            MaterialManager         materialManager,
            Renderer                renderer,
            SceneManager            sceneManager,
            TextRenderer            textRenderer,
            OpenTK.GameWindow       window
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.sceneManager = sceneManager;
            this.window = window;

            InitializationDependsOn(materialManager);
            InitializationDependsOn(renderer);
            InitializationDependsOn(sceneManager);
            InitializationDependsOn(textRenderer);
        }

        #region ChoiceActions
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
                sceneManager.Camera.ProjectionType = ProjectionType.PerspectiveVertical;
            }
            else if(selected.Label == "Horizontal")
            {
                sceneManager.Camera.ProjectionType = ProjectionType.PerspectiveHorizontal;
            }
            else if(selected.Label == "V. Stereo")
            {
                sceneManager.Camera.ProjectionType = ProjectionType.StereoscopicVertical;
            }
            else if(selected.Label == "H. Stereo")
            {
                sceneManager.Camera.ProjectionType = ProjectionType.StereoscopicHorizontal;
            }
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
        #endregion
        #region Actions
        private void ResetCamera(Area area)
        {
            Camera camera = sceneManager.Camera;

            camera.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    sceneManager.Home,
                    Vector3.Zero,
                    Vector3.UnitY
                )
            );
            cameraControls.SetTransform(camera.Frame.LocalToParent.Matrix);
            cameraControls.Clear();
            StereoParameters stereo = camera.StereoParameters;

            camera.FarParameter.X  = 50.0f;
            camera.NearParameter.X = 0.01f;
            camera.FovXParameter.X = (float)(System.Math.PI * 0.5);
            camera.FovYParameter.X = (float)(System.Math.PI * 0.5);
            stereo.EyePosition.Set(0.0f, 0.0f, 0.0f);
            stereo.ViewportCenter.Set(0.0f, 0.0f, 4.0f);
            stereo.Perspective.X = 1.0f;
            stereo.EyeSeparation.X = 0.065f;
        }
        private void ReferenceStereo(Area area)
        {
            StereoParameters stereo = sceneManager.Camera.StereoParameters;

            stereo.EyeSeparation.X = stereo.ViewPlaneSize.X / 6.0f;
            stereo.ViewportCenter.Z = stereo.EyePosition.Z + stereo.ViewPlaneSize.X;
            //sceneManager.Camera.StereoParameters.EyeSeparation = 
            // s = w / 6;
            // v.z = e.z + w;
            //sceneManager.Camera.StereoParameters
        }
        private void TogglePopup(Area area)
        {
            Popup popup = area.Link as Popup;
            if(popup == null)
            {
                return;
            }
            popup.Set(!popup.IsOpen);
            layer.Update();
        }

        #endregion Actions

        private void AddStereoModes()
        {
            stereoModes["Mono"] = new StereoMode(null);
            stereoModes["R/C"] = new StereoMode(renderer.Programs["Anachrome"]);
            stereoModes["G/M"] = new StereoMode(renderer.Programs["Triochrome"]);
            stereoModes["B/A"] = new StereoMode(renderer.Programs["ColorCode"]);
            stereoModes["Mix"] = new StereoMode(renderer.Programs["Blend"]);
        }

        private Area Operations()
        {
            Button expand   = new Button(renderer, "Operations");
            expand.Action   = TogglePopup;

            MenuList    list    = new MenuList(renderer, Orientation.Vertical);
            Popup       popup   = new Popup(expand, list);

            Button      collapse = new Button(renderer, "Operations");
            collapse.Action      = TogglePopup;
            collapse.Link        = popup;

            Button  merge       = new Button(renderer, "Merge");
            Button  delete      = new Button(renderer, "Delete");
            Button  truncate    = new Button(renderer, "Truncate");
            Button  triangulate = new Button(renderer, "Triangulate");
            Button  subdivide   = new Button(renderer, "Subdivide");
            Button  sqrt3       = new Button(renderer, "Sqrt(3)");
            Button  catmull     = new Button(renderer, "Catmull-Clark");
            Button  smooth      = new Button(renderer, "Smooth Normals");
            Button  flat        = new Button(renderer, "Flat Normals");
            Button  undo        = new Button(renderer, "Undo");
            Button  redo        = new Button(renderer, "Redo");

            list.Add(collapse   );
            list.Add(undo       );
            list.Add(redo       );
            list.Add(merge      );
            list.Add(delete     );
            list.Add(smooth     );
            list.Add(flat       );
            list.Add(truncate   );
            list.Add(triangulate);
            list.Add(subdivide  );
            list.Add(sqrt3      );
            list.Add(catmull    );

            expand.Link = popup;
            return popup;
        }
        private Area Materials()
        {
            if(materialManager == null)
            {
                return null;
            }

            Button expand   = new Button(renderer, "Materials");
            expand.Action   = TogglePopup;

            MenuList    list     = new MenuList(renderer, Orientation.Vertical);
            Popup       popup    = new Popup(expand, list);

            Button      collapse = new Button(renderer, "Materials");
            collapse.Action      = TogglePopup;
            collapse.Link        = popup;

            list.Add(collapse);
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            Choice choice = new Choice(Orientation.Vertical);
            choice.Style = Style.NullPadding;
            //choice.Action = MaterialChoiceAction;
            string[] materials = { "pearl", "gold", "red", "green", "cyan", "blue", "magenta", "pink", "EdgeLines", "Grid"};

            foreach(var name in materials)
            {
                ChoiceItem item = new ChoiceItem(renderer, name);
                choice.Add(item);
            }
            choice.Selected = choice.Items.First();
            list.Add(choice);

            expand.Link = popup;
            return popup;
        }
        private Area Scene()
        {
            Button expand = new Button(renderer, "Scene");
            expand.Name = "expand";
            expand.Action = TogglePopup;

            MenuList    list  = new MenuList(renderer, Orientation.Vertical);
            Popup       popup = new Popup(expand, list);

            list.Name = "list";
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;
            popup.Name = "popup";

            Button collapse = new Button(renderer, "Scene");
            collapse.Action = TogglePopup;
            collapse.Link   = popup;
            collapse.Name   = "collapse";
            list.Add(collapse);

            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;

            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "Scene choice";
                choice.Style = Style.NullPadding;
                choice.Add(new ChoiceItem(renderer, "Simple"));
                choice.Add(new ChoiceItem(renderer, "Polyhedra"));
                choice.Add(new ChoiceItem(renderer, "Spheres"));
                //choice.Action = SceneChoiceAction;
                choice.Selected = choice.Items.First();
                list.Add(choice);
            }

            expand.Link = popup;
            popup.Set(false);

            return popup;
        }
        private Area Lights()
        {
            Button expand = new Button(renderer, "Lights");
            expand.Name = "expand";
            expand.Action = TogglePopup;

            MenuList    list  = new MenuList(renderer, Orientation.Vertical);
            Popup       popup = new Popup(expand, list);

            list.Name = "list";
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;
            popup.Name = "popup";

            Button collapse = new Button(renderer, "Lights");
            collapse.Action = TogglePopup;
            collapse.Link   = popup;
            collapse.Name   = "collapse";
            list.Add(collapse);

            list.Add(new Slider(renderer, "Exposure",       (renderer.GlobalParameters["exposure"] as Floats), 0, 0.0f, 10.0f, 0.0f, 10.0f));

            list.Add(new Slider(renderer, "Radiance 1",     (renderer.LightingParameters["light_radiance"] as Floats), 0, 0.0f, 10.0f, 0.0f, 10.0f));
            list.Add(new Slider(renderer, "Radiance 2",     (renderer.LightingParameters["light_radiance"] as Floats), 1, 0.0f, 10.0f, 0.0f, 10.0f));
            list.Add(new Slider(renderer, "Radiance 3",     (renderer.LightingParameters["light_radiance"] as Floats), 2, 0.0f, 10.0f, 0.0f, 10.0f));
            list.Add(new Slider(renderer, "Ambient Red",    (renderer.GlobalParameters["ambient_light_color"] as Floats), 0, 0.0f, 4.0f, 0.0f, 4.0f));
            list.Add(new Slider(renderer, "Ambient Green",  (renderer.GlobalParameters["ambient_light_color"] as Floats), 1, 0.0f, 4.0f, 0.0f, 4.0f));
            list.Add(new Slider(renderer, "Ambient Blue",   (renderer.GlobalParameters["ambient_light_color"] as Floats), 2, 0.0f, 4.0f, 0.0f, 4.0f));

            expand.Link = popup;
            popup.Set(false);

            return popup;
        }
        private Area Camera()
        {
            Button expand = new Button(renderer, "Camera");
            expand.Name = "expand";
            expand.Action = TogglePopup;

            MenuList    list  = new MenuList(renderer, Orientation.Vertical);
            Popup       popup = new Popup(expand, list);

            list.Name = "list";
            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;
            popup.Name = "popup";

            Button collapse = new Button(renderer, "Camera");
            collapse.Action = TogglePopup;
            collapse.Link   = popup;
            collapse.Name   = "collapse";
            list.Add(collapse);

            Camera camera = sceneManager.Camera;
            StereoParameters stereo = camera.StereoParameters;

            list.ChildLayoutStyle = Area.AreaLayoutStyle.ExtendHorizontal;
            stereo.ViewportCenter.Z = 4.0f;
            list.Add(new Slider(renderer, "Fov X",          camera.FovXParameter,   0,    0.0f, (float)(System.Math.PI), 0.0f, 180.0f));
            list.Add(new Slider(renderer, "Fov Y",          camera.FovYParameter,   0,    0.0f, (float)(System.Math.PI), 0.0f, 180.0f));

#if false
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
#endif

            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "FOV choice";
                choice.Style = Style.NullPadding;
                choice.Add(new ChoiceItem(renderer, "Vertical"));
                choice.Add(new ChoiceItem(renderer, "Horizontal"));

#if false
                {
                    choice.Add(new ChoiceItem(renderer, "V. Stereo"));
                    choice.Add(new ChoiceItem(renderer, "H. Stereo"));
                }
#endif
                choice.Action = ProjectionChoiceAction;
                choice.Selected = choice.Items.First();
                list.Add(choice);
            }

#if false
            {
                Choice choice = new Choice(Orientation.Horizontal);
                choice.Name = "StereoMode choice";
                choice.Style = Style.NullPadding;
                foreach(var kvp in stereoModes)
                {
                    choice.Add(new ChoiceItem(renderer, kvp.Key));
                }
                choice.Action = StereoModeChoiceAction;
                choice.Selected = choice.Items.First();
                list.Add(choice);
            }

            {
                list.Add(new Slider(renderer, "Saturation",     (renderer.GlobalParameters["saturation"]    as Floats),  0,  0.0f, 1.0f, 0.0f, 1.0f));
                list.Add(new Slider(renderer, "Contrast",       (renderer.GlobalParameters["contrast"]      as Floats),  0,  0.0f, 1.0f, 0.0f, 1.0f));
                list.Add(new Slider(renderer, "Deghost",        (renderer.GlobalParameters["deghost"]       as Floats),  0,  0.0f, 1.0f, 0.0f, 1.0f));
            }
#endif

            {
                Dock dock = new Dock(Orientation.Horizontal);
                dock.Name = "Reset Home dock";
                dock.Style = Style.NullPadding;

                Button reset = new Button(renderer, "Reset");
                reset.Action = ResetCamera;
                dock.Add(reset);

                list.Add(dock);
            }

            expand.Link = popup;
            popup.Set(false);

            return popup;
        }
        private Area Menu()
        {
            Material        material            = new Material("menu", renderer.Programs["Textured"], MeshMode.PolygonFill);
            FontStyle       fontStyle           = new FontStyle("res/fonts/large.fnt"); 
            Vector2         padding             = new Vector2(6.0f, 6.0f); 
            Vector2         innerPadding        = new Vector2(2.0f, 2.0f);
            NinePatchStyle  ninePatchStyle      = new NinePatchStyle("ninepatch8.png");

            menuStyle = new Style(
                padding, 
                innerPadding, 
                fontStyle, 
                ninePatchStyle, 
                material
            );

            Dock dock = new Dock(Orientation.Vertical);
            dock.OffsetFreeSizeRelative = new Vector2(0.5f, 0.5f);
            dock.OffsetSelfSizeRelative = new Vector2(-0.5f, -0.5f);
            var b1 = new Button(renderer, "New Game", menuStyle);
            var b2 = new Button(renderer, "Load", menuStyle);
            var b3 = new Button(renderer, "Options", menuStyle);
            var b4 = new Button(renderer, "Quit", menuStyle);
            b1.OffsetFreeSizeRelative = new Vector2(0.5f, 0.0f);
            b2.OffsetFreeSizeRelative = new Vector2(0.5f, 0.0f);
            b3.OffsetFreeSizeRelative = new Vector2(0.5f, 0.0f);
            b4.OffsetFreeSizeRelative = new Vector2(0.5f, 0.0f);
            b1.OffsetSelfSizeRelative = new Vector2(-0.5f, 0.0f);
            b2.OffsetSelfSizeRelative = new Vector2(-0.5f, 0.0f);
            b3.OffsetSelfSizeRelative = new Vector2(-0.5f, 0.0f);
            b4.OffsetSelfSizeRelative = new Vector2(-0.5f, 0.0f);
            dock.Add(b1);
            dock.Add(b2);
            dock.Add(b3);
            dock.Add(b4);
            return dock;
        }
        protected override void InitializeService()
        {
            wheel = window.Mouse.WheelPrecise;

            textured = new ProgramDeprecated(renderer.Programs["Textured"]);

            layer = new Layer(this, window);
            layer.Name = "Root layer";
#if true
            //AddStereoModes();

            MenuList test = new MenuList(renderer, Orientation.Horizontal);
            test.Name = "test";

            test.Add(Operations());
            test.Add(Scene());
            test.Add(Camera());
            test.Add(Lights());
            test.Add(Materials());

            test.OffsetFreeSizeRelative = new Vector2(0.0f, 0.0f);
            test.OffsetSelfSizeRelative = new Vector2(0.0f, 0.0f);
            test.OffsetPixels           = new Vector2(4.0f, 4.0f);

            Layer.Add(test);
#endif

            //  Not enabled yet - work in progress. This needs layout order support (top down instead of bottom up)
            //  Layer.Add(Menu());

            InstallInputEventHandlers();
        }

        public void Render()
        {
            //  TODO Figure out what to do with uniform binding stuff
            mouse.X = window.Mouse.X;
            mouse.Y = window.Height - 1 - window.Mouse.Y;
            layer.Update();
            layer.Draw(this);
            hoverArea = layer.GetHit(new Vector2(mouse.X, mouse.Y));
            if(hoverArea == layer)
            {
                System.Diagnostics.Debugger.Break();
                hoverArea = layer.GetHit(new Vector2(mouse.X, mouse.Y));
            }
        }

        public void UpdateControls()
        {
            float wheelNow = window.Mouse.WheelPrecise;
            float wheelDelta = wheel - wheelNow;
            wheel = wheelNow;
            FrameController cameraControls = sceneManager.CameraControls;
            cameraControls.RotateY.Adjust(-MouseXDelta / 2000.0f);
            cameraControls.RotateX.Adjust(-MouseYDelta / 2000.0f);
            cameraControls.TranslateZ.Adjust(wheelDelta * 0.2f);
            mouseXDelta = 0;
            mouseYDelta = 0;

            cameraControls.TranslateX.Inhibit   = !window.Focused;
            cameraControls.TranslateY.Inhibit   = !window.Focused;
            cameraControls.TranslateZ.Inhibit   = !window.Focused;
            cameraControls.RotateX.Inhibit      = !window.Focused;
            cameraControls.RotateY.Inhibit      = !window.Focused;
            cameraControls.RotateZ.Inhibit      = !window.Focused;
            cameraControls.TranslateX.Less      = window.Keyboard[OpenTK.Input.Key.A];
            cameraControls.TranslateX.More      = window.Keyboard[OpenTK.Input.Key.D];
            cameraControls.TranslateY.Less      = window.Keyboard[OpenTK.Input.Key.F];
            cameraControls.TranslateY.More      = window.Keyboard[OpenTK.Input.Key.R];
            cameraControls.TranslateZ.Less      = window.Keyboard[OpenTK.Input.Key.W];
            cameraControls.TranslateZ.More      = window.Keyboard[OpenTK.Input.Key.S];
            cameraControls.RotateX.Less         = window.Keyboard[OpenTK.Input.Key.Z];
            cameraControls.RotateX.More         = window.Keyboard[OpenTK.Input.Key.X];
            cameraControls.RotateY.Less         = window.Keyboard[OpenTK.Input.Key.V];
            cameraControls.RotateY.More         = window.Keyboard[OpenTK.Input.Key.C];
            cameraControls.RotateZ.Less         = window.Keyboard[OpenTK.Input.Key.E];
            cameraControls.RotateZ.More         = window.Keyboard[OpenTK.Input.Key.Q];

            cameraControls.FixedUpdate();
            cameraControls.AfterFixedUpdates();
        }
    }
}
