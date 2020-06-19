using System.Collections.Generic;
using System.IO;

using RenderStack.Math;

//using ID4 = System.UInt32;
using I1 = System.SByte;
using I2 = System.Int16;
using I4 = System.Int32;
using U1 = System.Byte;
using U2 = System.UInt16;
using U4 = System.UInt32;
using F4 = System.Single;
using S0 = System.String;
using VX = System.UInt32;
using COL4 = RenderStack.Math.Vector4;
using COL12 = RenderStack.Math.Vector3;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public partial class LWScene
    {
        //  Parsing
        public int             LwsVersion;

        //  Scenes
        public int             FirstFrame;
        public int             LastFrame;
        public int             FrameStep;
        public int             FramesPerSecond;
        public int             PreviewFirstFrame;
        public int             PreviewLastFrame;
        public int             PreviewFrameStep;
        public int             CurrentFrame;

        //  Objects

        //  Lights - global
        public Vector3          AmbientColor;
        public SFloatEnvelope   AmbientIntensity = new SFloatEnvelope();
        public bool             EnableLensFlare = true;
        public bool             EnableShadowMaps = true;
        public SFloatEnvelope   GlobalFlareIntensity = new SFloatEnvelope();

        //  Options (Layout)
        public int              GridNumber;
        public double           GridSize;
        public Vector3          ViewAimpoint;
        public Quaternion       ViewRotation;
        public double           ViewZoomFactor;
        public float            CameraZoomFactor;

        //  Effects
        public Vector3          ZenithColor;
        public Vector3          SkyColor;
        public Vector3          GroundColor;
        public Vector3          NadirColor;
        public int              FogType;
        public double           FogMinDist;
        public double           FogMaxDist;
        public double           FogMaxAmount;
        public Vector3          FogColor;
        public Vector3          BackgroundColor;

        private List<LWItem>    items   = new List<LWItem>();
        private List<LWSObject> objects = new List<LWSObject>();
        private List<LWLight>   lights  = new List<LWLight>();
        private List<LWCamera>  cameras = new List<LWCamera>();

        public List<LWItem>     Items   { get { return Items; } }
        public List<LWSObject>  Objects { get { return objects; } }
        public List<LWLight>    Lights  { get { return lights; } }
        public List<LWCamera>   Cameras { get { return cameras; } }

        public LWScene()
        {
            LwsVersion          = 0;
            GridNumber          = 16;
            GridSize            = 1.0f;

            FogType             = 0; // off
            FogMinDist          = 0;
            FogMaxDist          = 1;
            FogMaxAmount        = 1.0;

            CameraZoomFactor    = 1.0f;
            BackgroundColor     = new Vector3(0.5f, 0.5f, 0.5f);
            ZenithColor         = new Vector3(0.0f, 0.0f, 0.5f);
            SkyColor            = new Vector3(0.5f, 0.5f, 1.0f);
            GroundColor         = new Vector3(0.0f, 0.5f, 0.0f);
            NadirColor          = new Vector3(0.0f, 0.5f, 0.0f);
        }
    }
}