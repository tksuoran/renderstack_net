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
    public partial class LWSceneParser
    {
        void NumChannels()
        {
            /*num_channels = */file.read_int();
        }
        void Channel()
        {
            int channel_id = file.read_int();
            currentEnvelope = new LWSEnvelope();
            currentMotion.insert((LWChannel)channel_id, currentEnvelope);
        }
        void Envelope()
        {
            /*num_channel_keys = */file.read_int();
        }
        void Key()
        {
            float value    = (float)( file.read_double() );
            float time     = (float)( file.read_double() );
            Shape spantype = (Shape)file.read_int();
            float p1       = (float)( file.read_double() );
            float p2       = (float)( file.read_double() );
            float p3       = (float)( file.read_double() );
            float p4       = (float)( file.read_double() );
            float p5       = (float)( file.read_double() );
            float p6       = (float)( file.read_double() );

            var channel_key = new LWChannelKey(
                value,
                time,
                spantype,
                p1,
                p2,
                p3,
                p4,
                p5,
                p6
            );
            currentEnvelope.insert(channel_key);
        }

        void Behaviors()
        {
            Behavior pre  = (Behavior)file.read_int();
            Behavior post = (Behavior)file.read_int();
            currentEnvelope.setBehaviors(pre, post);
            file.read_end_scope();
        }
    }
}
