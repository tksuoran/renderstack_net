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
using COL12 = RenderStack.Math.Vector4;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public class LWEnvelopeKey
    {
        public enum Interpolation
        {
            TCB = 0,
            Hermite = 1,
            Bezier1D = 2,
            Linear = 3,
            Stepped = 4,
            Bezier2D = 5
        }

        public F4               Time;
        public F4               Value;
        public Interpolation    InterpolationType;
        public F4[]             InterpolationParameters = new F4[8];

        public LWEnvelopeKey(F4 time, F4 value)
        {
            Time  = time;
            Value = value;
        }
    }
}


