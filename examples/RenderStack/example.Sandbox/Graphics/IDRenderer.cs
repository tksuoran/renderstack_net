using System;
using System.Diagnostics;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

namespace example.Sandbox
{
    /*  Comment: Highly experimental  */ 
    public class IDRenderer : Service
    {
        Application                     application;
        Renderer.MaterialManager        materialManager;
        IRenderer                       renderer;
        SceneManager                    sceneManager;
        Renderer.TextRenderer           textRenderer;
        UserInterfaceManager            userInterfaceManager;

        private bool                    useFramebuffer;
        private IFramebuffer            framebuffer;
        private Material                resetDepth;
        private Material                idMaterial;
        private uint                    idOffset;
        private List<Renderer.Group>    idGroupsRendered = new List<Group>();

        public override string Name
        {
            get { return "IDRenderer"; }
        }

        public void Connect(
            Application                 application,
            Renderer.MaterialManager    materialManager,
            IRenderer                   renderer,
            SceneManager                sceneManager,
            Renderer.TextRenderer       textRenderer,
            UserInterfaceManager        userInterfaceManager
        )
        {
            this.application            = application;
            this.materialManager        = materialManager;
            this.renderer               = renderer;
            this.sceneManager           = sceneManager;
            this.textRenderer           = textRenderer;
            this.userInterfaceManager   = userInterfaceManager;

            InitializationDependsOn(materialManager, renderer);
        }

        protected override void InitializeService()
        {
            //  \todo Check which Id program to use?
            string idProgram = RenderStack.Graphics.Configuration.useIntegerPolygonIDs ? "IdUInt" : "IdVec3";

            idMaterial = materialManager.MakeMaterial("Id", idProgram);

            {
                //  This material renders everything to the far plane with a custom depth state
                resetDepth = materialManager.MakeMaterial("IdResetDepth", idProgram);
                resetDepth.DepthState            = new DepthState();
                resetDepth.DepthState.Function   = DepthFunction.Always;
                resetDepth.DepthState.Near       = 1.0f;
                resetDepth.DepthState.Far        = 1.0f;
            }

            useFramebuffer = 
                (RenderStack.Graphics.Configuration.useGl1 == false) && 
                (RenderStack.Graphics.Configuration.canUseFramebufferObject == true);
                // \todo and if using multisample on default framebuffer
            //useFramebuffer = false;
            if(useFramebuffer == true)
            {
                CreateFramebuffers();
            }
            renderer.Resize += new EventHandler<EventArgs>(window_Resize);
        }

        private void DestroyFramebuffers()
        {
            if(framebuffer != null)
            {
                framebuffer.Dispose();
                framebuffer = null;
            }
        }

        private void CreateFramebuffers()
        {
            if(useFramebuffer == false)
            {
                return;
            }

            framebuffer = FramebufferFactory.Create(renderer.Width, renderer.Height);
            if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
            {
                framebuffer.AttachRenderBuffer(
                    FramebufferAttachment.ColorAttachment0, 
                    //PixelFormat.RedInteger, 
                    RenderbufferStorage.R32ui,
                    0
                );
            }
            else
            {
                framebuffer.AttachRenderBuffer(
                    FramebufferAttachment.ColorAttachment0, 
                    //PixelFormat.Rgb, 
                    RenderbufferStorage.Rgb8,
                    0
                );
            }
            framebuffer.AttachRenderBuffer(
                FramebufferAttachment.DepthAttachment, 
                //PixelFormat.DepthComponent, 
                RenderbufferStorage.DepthComponent32,
                0
            );
            framebuffer.Begin();
            framebuffer.Check();
            framebuffer.End();
        }

        void window_Resize(object sender, EventArgs e)
        {
            if(useFramebuffer)
            {
                DestroyFramebuffers();
                CreateFramebuffers();
            }
        }

        public void Render(int px, int py)
        {
            this.px = px;
            this.py = py;

            RenderStack.Graphics.Debug.WriteLine("=== RenderId");

            bool        first   = true;
            Camera      camera  = sceneManager.Camera;
            List<Group> groups  = sceneManager.IdGroups;

            if(useFramebuffer && framebuffer != null)
            {
                renderer.Requested.Viewport = framebuffer.Viewport;
                framebuffer.Begin();
            }

            //SetMultisample(false);
            //renderer.BeginScissorMouse(px, py);
            RenderIdBegin();

            renderer.Requested.Camera   = camera;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            foreach(var group in groups)
            {
                if(group.Visible == false)
                {
                    continue;
                }

                renderer.CurrentGroup = group;

                if(first)
                {
                    //  First group can skip special processing
                    renderer.Requested.Material = idMaterial;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;
                    first = false;
                }
                else
                {
                    //  Following groups ignore depth from previous groups
                    //  This is done by rendering first all objects in the
                    //  group with depth range (1, 1) and then rendering
                    //  objects in the group normally with depth range (0, 1)
                    renderer.Requested.Material = resetDepth;
                    renderer.Requested.Program  = renderer.Requested.Material.Program;

                    if(
                        RenderStack.Graphics.Configuration.canUseInstancing 
                        /* && RenderStack.Graphics.Configuration.canUseBaseVertex */
                    )
                    {
                        //  Instanced code path
                        foreach(var kvp in group.AllInstances.Collection)
                        {
                            var tuple = kvp.Key;
                            var models2 = kvp.Value;
                            renderer.Requested.Mesh = tuple.Item1;
                            renderer.RenderInstancedPrepare();

                            int i = 0;
                            foreach(var model in models2)
                            {
                                renderer.Models.Floats("model_to_world_matrix").SetI(
                                    i, model.Frame.LocalToWorld.Matrix
                                );
                                i++;
                                if(i > 49)
                                {
                                    renderer.Models.Sync();
                                    renderer.RenderCurrent(i);
                                    i = 0;
                                }
                            }
                            if(i > 0)
                            {
                                renderer.Models.Sync();

                                if(i > 1)
                                {
                                    renderer.RenderCurrent(i);
                                }
                                else
                                {
                                    renderer.RenderCurrent();
                                }
                            }
                        }
                    }
                    else
                    {
                        //  Non-instanced code path
                        foreach(var model in group.Models)
                        {
                            renderer.Requested.Mesh = model.Batch.Mesh;
                            renderer.SetFrame(model.Frame);
                            renderer.RenderCurrent();
                        }
                    }
                }

                RenderGroupId(group);
            }
            if(RuntimeConfiguration.disableReadPixels == false)
            {
                ReadId();
                ReadDepth();
                FetchDepth();
            }

            if(useFramebuffer && framebuffer != null)
            {
                framebuffer.End();
            }

            //renderer.EndScissorMouse();
            //SetMultisample(true);
        }

        private void RenderIdBegin()
        {
            foreach(var group in idGroupsRendered)
            {
                group.IdList.Clear();
            }
            idGroupsRendered.Clear();
            renderer.ApplyViewport();
            uint idClear = 0xfffffeffu;
            if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
            {
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                GL.ClearBuffer(ClearBuffer.Color, 0, ref idClear);
            }
            else
            {
                Vector3 clearColor = Vector3.Vector3FromUint(idClear);
                GL.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, 0.0f);
                //GL.ClearDepth(1.0);
                GL.Clear(
                    0
                    | ClearBufferMask.ColorBufferBit
                    | ClearBufferMask.DepthBufferBit 
                    | ClearBufferMask.StencilBufferBit
                );
            }

            sceneManager.Camera.Frame.UpdateHierarchical(sceneManager.UpdateSerial);
            sceneManager.Camera.UpdateFrame();
            sceneManager.Camera.UpdateViewport(renderer.Requested.Viewport);
            renderer.Requested.Camera = sceneManager.Camera;

            if(RenderStack.Graphics.Configuration.useGl1)
            {
            }

            idOffset = 0;
        }

        public void SetMultisample(bool enabled)
        {
            //  Never mind if this fails.. \todo Check capability
            try
            {
                if(enabled)
                {
                    GL.Enable(EnableCap.Multisample);
                }
                else
                {
                    GL.Disable(EnableCap.Multisample);
                }
            }
            catch(Exception)
            {
            }
        }

        uint NextPowerOfTwo(uint x)
        {
            x--;
            x |= x >> 1;  // handle  2 bit numbers
            x |= x >> 2;  // handle  4 bit numbers
            x |= x >> 4;  // handle  8 bit numbers
            x |= x >> 8;  // handle 16 bit numbers
            x |= x >> 16; // handle 32 bit numbers
            x++;
        
            return x;
        }
        private void RenderGroupId(Group group)
        {
            renderer.Requested.Material = idMaterial;
            renderer.Requested.Program  = renderer.Requested.Material.Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.UpdateCamera();

            if(
                RenderStack.Graphics.Configuration.canUseInstancing &&
                (RenderStack.Graphics.Configuration.useGl1 == false)
                /* && RenderStack.Graphics.Configuration.canUseBaseVertex */
            )
            {
                //  Instanced code path
                foreach(var kvp in group.AllInstances.Collection)
                {
                    var tuple = kvp.Key;
                    var models2 = kvp.Value;
                    renderer.Requested.Mesh = tuple.Item1;

                    var     indexBufferRange    = renderer.Requested.Mesh.IndexBufferRange(MeshMode.PolygonFill);
                    uint    count               = indexBufferRange.Count;
                    uint    powerOfTwo          = NextPowerOfTwo(count);
                    uint    mask                = powerOfTwo - 1;

                    renderer.RenderInstancedPrepare();

                    int i = 0;
                    foreach(var model in models2)
                    {
                        renderer.Models.Floats("model_to_world_matrix").SetI(
                            i, model.Frame.LocalToWorld.Matrix
                        );
                        uint currentBits = idOffset & mask;
                        if(currentBits != 0)
                        {
                            uint add = powerOfTwo - currentBits;
                            idOffset += add;
                        }

                        Vector3 v = Vector3.Vector3FromUint(idOffset);
                        renderer.Models.Floats("id_offset_vec3").SetI(i, v.X, v.Y, v.Z);

                        i++;
                        if(i > 49)
                        {
                            renderer.Models.Sync();
                            renderer.RenderCurrent(i);
                            i = 0;
                        }

                        if(
                            (indexBufferRange != null) && 
                            (indexBufferRange.Count > 0)
                        )
                        {
                            IDListEntry entry = new IDListEntry(idOffset, model);
                            group.IdList.Add(entry);

                            idOffset += indexBufferRange.Count;
                        }
                    }
                    if(i > 0)
                    {
                        renderer.Models.Sync();

                        if(i > 1)
                        {
                            renderer.RenderCurrent(i);
                        }
                        else
                        {
                            renderer.RenderCurrent();
                        }
                    }
                }
            }
            else
            {
                //  Non-instanced code path
                foreach(var model in group.Models)
                {
                    renderer.Requested.Mesh = model.Batch.Mesh;
                    renderer.SetFrame(model.Frame);

                    var indexBufferRange = renderer.Requested.Mesh.IndexBufferRange(MeshMode.PolygonFill);
                    if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
                    {
                        renderer.Models.UInts("id_offset_uint").SetI(0, idOffset);
                    }
                    else
                    {
                        uint count          = indexBufferRange.Count;
                        uint powerOfTwo     = NextPowerOfTwo(count);
                        uint mask           = powerOfTwo - 1;
                        uint currentBits    = idOffset & mask;
                        if(currentBits != 0)
                        {
                            uint add = powerOfTwo - currentBits;
                            idOffset += add;
                        }

                        Vector3 v = Vector3.Vector3FromUint(idOffset);
                        renderer.Models.Floats("id_offset_vec3").SetI(0, v.X, v.Y, v.Z);
                    }

                    renderer.Models.Sync();
                    renderer.RenderCurrent();

                    if(
                        (indexBufferRange != null) && 
                        (indexBufferRange.Count > 0)
                    )
                    {
                        IDListEntry entry = new IDListEntry(idOffset, model);
#if DEBUG
                        if(group.AlreadyHas(entry))
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif
                        group.IdList.Add(entry);

                        idOffset += indexBufferRange.Count;
                    }
                }
            }

            {
                IDListEntry entry = new IDListEntry(idOffset, null);
#if DEBUG
                if(group.AlreadyHas(entry))
                {
                    System.Diagnostics.Debugger.Break();
                }
#endif
                group.IdList.Add(entry);
            }
            idGroupsRendered.Add(group);
        }

        private int     px;
        private int     py;
        private byte[]  pixelsb = new byte[4];
        private uint[]  pixels  = new uint[4];
        private uint[]  depthsi = new uint[4];
        private float[] depthsf = new float[4];

        public class HoverInfo
        {
            public float            Depth;
            public uint             Id;
            public Renderer.Group[] Groups;
        }

        private HoverInfo hover = new HoverInfo();
        public HoverInfo Hover { get { return hover; } }

        public void FetchDepth()
        {
            float usedDepth = depthsf[0];
            if(RenderStack.Graphics.Configuration.canUseIntegerFramebufferFormat)
            {
                float depthi = (float)(depthsi[0]) / (float)(uint.MaxValue);
                usedDepth = depthi;
            }

            if(RuntimeConfiguration.guiExtraInfo)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if(RenderStack.Graphics.Configuration.canUseIntegerFramebufferFormat)
                {
                    sb.Append("DI: " + depthsi[0].ToString());
                }
                sb.Append(" D: " + usedDepth.ToString("#.000"));
                sb.Append(" DF: " + depthsf[0].ToString("#.000"));

                sb.Append(" < " + pixels[0].ToString("X") + " > ");
                sb.Append(" px: " + px.ToString());
                sb.Append(" py: " + py.ToString());
                textRenderer.DebugLine(sb.ToString());
            }
            hover.Depth = usedDepth;
            hover.Id = pixels[0];
            hover.Groups = idGroupsRendered.ToArray();
        }

        private void ReadId()
        {
            // \todo fix connection Services.Instance.HighLevelRenderer.MouseInScreen(out px, out py);
            if(RuntimeConfiguration.disableReadPixels == true)
            {
                return;
            }

            if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
            {
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            }
            else
            {
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                GL.ReadPixels<byte>(px, py, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, pixelsb);
                pixels[0] = 
                    (uint)(pixelsb[0] << 16) |
                    (uint)(pixelsb[1] <<  8) |
                    (uint)(pixelsb[2] <<  0);
            }
            //updateMsg += " ID: " + pixels[0].ToString("X") + " ";
        }

        public void ReadDepth()
        {
            //  \todo This needs to be fixed to support both code paths?
            if(RenderStack.Graphics.Configuration.canUseIntegerFramebufferFormat)
            {
                try
                {
                    GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                    GL.ReadPixels<uint>(px, py, 1, 1, PixelFormat.DepthComponent, PixelType.UnsignedInt, depthsi);
                }
                catch(System.Exception)
                {
                    Trace.TraceError("Depth read failure (uint)");
                }
            }

            try
            {
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                GL.ReadPixels<float>(px, py, 1, 1, PixelFormat.DepthComponent, PixelType.Float, depthsf);
            }
            catch(System.Exception)
            {
                Trace.TraceError("Depth read failure (float)");
            }
        }

    }
}
