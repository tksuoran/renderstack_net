using System.Collections.Generic;
using System.Diagnostics;

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
    /*  For objects  */
    public class LWEnvelope 
    {
        public string               Name;
        public U1                   UserFormat;
        public U1                   Type;
        public U2                   PreBehaviour;
        public U2                   PostBehaviour;
        public List<LWEnvelopeKey>  Keys = new List<LWEnvelopeKey>();
    }

    public partial class LWModelParser
    {
        /*  LWO2 Envelope Definition 

            ENVL { index[U4], attributes[SUB-CHUNK] * } 

            An array of keys. Each ENVL chunk defines the value of a
            single parameter channel as a function of time. The index
            is used to identify this envelope uniquely and can have any
            non-zero value less than 0x1000000. Following the index is
            a collection of subchunks that describe the envelope. These
            are documented below, in the Envelope Subchunks section.
        */
        void envelope_U4_sc()
        {
            var envelopeIndex = f.ReadU4();
            currentEnvelope = new LWEnvelope();

            currentLayer.Envelopes[(int)envelopeIndex] = currentEnvelope;
            while(f.Left() > 0)
            {
                ENVLchunks();
            }
        }

        public void ENVLchunks()
        {
            var chunk = f.ReadID4();
            var length = f.ReadU2();
            f.Push(length);
            switch(chunk.value)
            {
                case ID.TYPE: readType_U1_U1(); break;
                case ID.PRE:  readPreBehaviour_U2(); break;
                case ID.POST: readPostBehaviour_U2(); break;
                case ID.KEY:  readKeyframe_F4_F4(); break;
                case ID.SPAN: readInterpolation_ID4_d(); break;
                case ID.CHAN: readChannel_S0_U2_d(); break;
                case ID.NAME: readName_S0(); break;
                default:
                {
                    Debug.WriteLine("Unknown chunk " + chunk);
                    break;
                }
            }
            f.Pop(true);
        }

        /*  Envelope Type

            TYPE { user-format[U1], type[U1] }

            The type subchunk records the format in which the 
            envelope is displayed to the user and a type code that 
            identifies the components of certain predefined envelope 
            triples. The user format has no effect on the actual 
            values, only the way they're presented in LightWave®'s 
            interface.

            02 - Float
            03 - Distance
            04 - Percent
            05 - Angle

            The predefined envelope types include the following.

            01, 02, 03 - Position: X, Y, Z
            04, 05, 06 - Rotation: Heading, Pitch, Bank
            07, 08, 09 - Scale: X, Y, Z
            0A, 0B, 0C - Color: R, G, B
            0D, 0E, 0F - Falloff: X, Y, Z

        */
        void readType_U1_U1()
        {
            currentEnvelope.UserFormat = f.ReadU1();
            currentEnvelope.Type       = f.ReadU1();
        }

        /*  Pre-Behavior 

            PRE { type[U2] } 

            The pre-behavior for an envelope defines the signal value for times before
            the first key. The integer code selects one of several predefined behaviors,
            starting from zero: Reset, Constant, Repeat, Ocsillate, Offset Repeat, Linear. 
        */
        void readPreBehaviour_U2()
        {
            currentEnvelope.PreBehaviour = f.ReadU2();
        }

        /*  Post-Behavior 

            POST { type[U2] } 

            The post-behavior determines the signal value for times after the last key.
            The type codes are the same as for pre-behaviors. 
        */
        void readPostBehaviour_U2()
        {
            currentEnvelope.PostBehaviour = f.ReadU2();
        }

        /*  Keyframe Time and Value

            KEY { time[F4], value[F4] }

            The value of the envelope at the specified time in seconds. 
            The signal value between keyframes is interpolated. The time 
            of a keyframe isn't restricted to integer frames.
        */
        void readKeyframe_F4_F4()
        {
            F4 keyframe_time  = f.ReadF4();
            F4 keyframe_value = f.ReadF4();
            currentkey = new LWEnvelopeKey(keyframe_time, keyframe_value);
            currentEnvelope.Keys.Add(currentkey);
        }

        /*  Interval Interpolation 

            SPAN { type[ID4], value[F4] * } 

            This sub-chunk defines the interpolation between the most recently
            specified KEY chunk and the keyframe immediately before it in time.
            The type ID code defines the interpolation algorithm and can be
            STEP, LINE, TCB, HERM or BEZI. The variable number of parameters
                values that follow define the particulars of the interpolation. 
        */
        void readInterpolation_ID4_d()
        {
            var type = f.ReadID4();

            switch(type.value)
            {
                case ID.TCB:  currentkey.InterpolationType = LWEnvelopeKey.Interpolation.TCB;       break;
                case ID.HERM: currentkey.InterpolationType = LWEnvelopeKey.Interpolation.Hermite;   break;
                case ID.BEZI: currentkey.InterpolationType = LWEnvelopeKey.Interpolation.Bezier1D;  break;
                case ID.LINE: currentkey.InterpolationType = LWEnvelopeKey.Interpolation.Linear;    break;
                case ID.STEP: currentkey.InterpolationType = LWEnvelopeKey.Interpolation.Stepped;   break;
                case ID.BEZ2: currentkey.InterpolationType = LWEnvelopeKey.Interpolation.Bezier2D;  break;
                default:
                {
                    Trace.TraceWarning("Unknown interoplation type " + type);
                    break;
                }
            }

            for(int i = 0; f.Left() >= 4; ++i)
            {
                F4 x = f.ReadF4();
                currentkey.InterpolationParameters[i] = x;
            }
        }

        /*  Plug-in Channel Operators 

            CHAN { server-name[S0], flags[U2], data[...] } 

            Channel filters can be layered on top of a basic keyframed envelope to provide
            some more elaborate effects. Each channel chunk contains the name of the plug-in
            server and some flags bits. Only the first flag bit is defined, which if set
            indicates that the filter is disabled. The plug-in data follows as raw bytes. 
        */
        public void readChannel_S0_U2_d()
        {
            //  \todo plugins ignored
            string  server_name = f.ReadS0();
            U2      flags       = f.ReadU2();

            while(f.Left() > 0)
            {
                U1 data = f.ReadU1();
            }
        }

        /*  Channel Name 

            NAME { channel-name[S0] } 

            This is an optional envelope sub-chunk which is not used by LightWave
            in any way. It is only provided to allow external programs to browse
            through the envelopes available in an object fille.
        */
        public void readName_S0()
        {
            currentEnvelope.Name = f.ReadS0();
        }

    }
}
