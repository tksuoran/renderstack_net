using System.Collections.Generic;
using System.Diagnostics;

using RenderStack.Math;
using RenderStack.Geometry;

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
    public enum LWChannel
    {
        X  =  0,  //   0 - Move X                                          
        Y  =  1,  //   1 - Move Y                                          
        Z  =  2,  //   2 - Move Z                                          
        H  =  3,  //   3 - Rotate Heading                                  
        P  =  4,  //   4 - Rotate Pitch                                    
        B  =  5,  //   5 - Rotate Bank                                     
        SX =  6,  //   6 - Scale X / Size X (channels are connected)      
        SY =  7,  //   7 - Scale Y / Size Y (channels are connected)       
        SZ =  8,  //   8 - Scale Z / Size Z (channels are connected)       
        RX =  9,  //   9 - RestLength X                                    
        RY = 10,  //  10 - RestLength Y                                    
        RZ = 11,  //  11 - RestLength Z                                    
        PX =  9,  //   9 - Move PivotPoint X 
        PY = 10,  //  10 - Move PivotPoint Y
        PZ = 11,  //  11 - Move PivotPoint Z
    }
    public class LWMotion
    {
        private Dictionary<LWChannel, LWSEnvelope> envelopes = new Dictionary<LWChannel,LWSEnvelope>();

        public void insert(LWChannel channel, LWSEnvelope envelope)
        {
            envelopes[channel] = envelope;
        }

        public LWSEnvelope Channel(LWChannel channel_id)
        {
            if(envelopes.ContainsKey(channel_id))
            {
                return envelopes[channel_id];
            }
            return null;
        }
    }

}