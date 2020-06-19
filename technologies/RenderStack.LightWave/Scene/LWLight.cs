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
    public class LWLight : LWItem
    {
        public Vector3          Color               ;
        public int              Type                ;
        public SFloatEnvelope   ConeAngle = new SFloatEnvelope();
        public SFloatEnvelope   EdgeAngle = new SFloatEnvelope();
        public int              FalloffType         ;
        public SFloatEnvelope   Range = new SFloatEnvelope();
        public SFloatEnvelope   Intensity = new SFloatEnvelope();
        public SFloatEnvelope   Falloff = new SFloatEnvelope();
        public int              AffectCaustics      ;
        public int              AffectDiffuse       ;
        public int              AffectSpecular      ;
        public int              AffectOpenGL        ;
        public int              UseConeAngle        ;
        public int              LensFlare           ;
        public SFloatEnvelope   FlareIntensity = new SFloatEnvelope();
        public SFloatEnvelope   FlareDissolve = new SFloatEnvelope();
        public int              LensFlareFade       ;
        public int              LensFlareOptions    ;
        public double           FlareRandStreakInt  ;
        public double           FlareRandStreakDens ;
        public double           FlareRandStreakSharp;
        public int              ShadowType          ;
        public int              ShadowCasting       ;
        public int              ShadowMapSize       ;
        public double           ShadowMapAngle      ;
        public double           ShadowFuzziness     ;
    }
}
