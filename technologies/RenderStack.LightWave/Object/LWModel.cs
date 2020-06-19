using System.Collections.Generic;
using System.Diagnostics;

using RenderStack.Math;

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
using COL12 = RenderStack.Math.Vector4;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public class LWModel
    {
        private Dictionary<U4, LWLayer>         layers      = new Dictionary<uint,LWLayer>();
        private List<LWSurface>                 surfaceList = new List<LWSurface>();
        private Dictionary<string, LWSurface>   surfaces    = new Dictionary<string,LWSurface>();
        private Dictionary<U4, LWClip>          clips       = new Dictionary<uint,LWClip>();
        private List<string>                    tags        = new List<string>();

        public Dictionary<U4, LWLayer>          Layers      { get { return layers; } }
        public List<LWSurface>                  SurfaceList { get { return surfaceList; } }
        public Dictionary<string, LWSurface>    Surfaces    { get { return surfaces; } }
        public Dictionary<U4, LWClip>           Clips       { get { return clips; } }
        public List<string>                     Tags        { get { return tags; } }

        public LWLayer MakeLayer(U4 number, string name, ushort flags, Vector3 pivot, int parent)
        {
            var layer = new LWLayer(name, flags, Vector3.Zero, parent);
            layers[number] = layer;
            return layer;
        }

        public LWLayer Layer(U4 layer_number)
        {
            return layers[layer_number];
        }

        public LWSurface MakeSurface(string name)
        {
            LWSurface surface = new LWSurface(name);
            surfaces[name] = surface;
            surfaceList.Add(surface);
            return surface;
        }

        public LWSurface FindOrCreateSurface(string name)
        {
            if(surfaces.ContainsKey(name))
            {
                return surfaces[name];
            }
            LWSurface surface = new LWSurface(name);
            surfaces[name] = surface;
            surfaceList.Add(surface);
            return surface;
        }

        public string Tag(VX tag_index)
        {
            return tags[(int)tag_index];
        }

        public LWClip MakeClip(U4 index)
        {
            if(index >= 0x1000000)
            {
                throw new System.Exception("Clip index not below 0x1000000");
            }

            var clip = new LWClip();
            clips[index] = clip;
            return clip;
        }
    }
}
