using System.Collections.Generic;
using System.IO;

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
using COL12 = RenderStack.Math.Vector3;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public class LWSObject : LWItem
    {
        private List<LWBone>        bones = new List<LWBone>();
        private List<int>           excludeLights = new List<int>();

        public  List<LWBone>        Bones           { get { return bones; } }
        public  List<int>           Excludelights   { get { return excludeLights; } }
        public  int                 UseBonesFrom;
        public  int                 BoneFalloffType;
        public  LWModel             Model;
        public  int                 Layer;
        public  SFloatEnvelope       Dissolve;
        public  int                 ShadowOptions;
        public  int                 UnseenByRays;
        public  int                 UnseenByCamera;
        public  int                 UnaffectedByFog;
        public  int                 AffectedByFog;
        public  double              ParticleSize;
        public  double              LineSize;
        public  SFloatEnvelope       PolygonSize;
        public  int                 PolygonEdges;
        public  int                 PolygonEdgeFlags;
        public  double              PolygonEdgeThickness;
        public  double              PolygonEdgesZScale;
        public  double              EdgeNominalDistance;
        public  Vector3             ObjEdgeColor;
        public  int                 SubdivisionOrder;
        public  int[]               SubPatchLevel = new int[2];
        public  int                 DistanceDissolve;
        public  double              MaxDissolveDistance;

        public  int                 MorphTarget;
        public  SFloatEnvelope       Metamorph;
        public  LWEnvelope          MorphEnvelope;
        public  int                 MorphSurfaces;
        public  ItemVisibility      Visibility;  // old refresh value

    }
}