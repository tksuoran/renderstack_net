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
    public class LWChannelKey
    {
        public float Value;
        public float Time;
        public Shape Shape;
        public float p1;       //  tension                       incoming time     param[ 0 ]
        public float p2;       //  continuity                    incoming value  param[ 1 ]
        public float p3;       //  bias                          outgoing time   param[ 2 ]
        public float p4;       //  incoming tangent  param[ 0 ]  outgoing value  param[ 3 ]
        public float p5;       //  outgoing tangent  param[ 1 ]    
        public float p6;       //  ignored 0                        

        public float Tension    { get { return p1; } }
        public float Continuity { get { return p2; } }
        public float Bias       { get { return p3; } }

        public LWChannelKey(
            float value,
            float time,
            Shape shape,
            float p1,
            float p2,
            float p3,
            float p4,
            float p5,
            float p6
        )
        {
            this.Value = value;
            this.Time = time;
            this.Shape = shape;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
            this.p5 = p5;
            this.p6 = p6;
        }
    }
}

/*

In scene files, an envelope is stored in a block named Envelope that
contains one or more nested Key blocks and one Behaviors block.

   { Envelope
     nkeys
     Key value time spantype p1 p2 p3 p4 p5 p6
     Key ...
     Behaviors pre post
   }

The nkeys value is an integer, the number of Key blocks in the envelope.
Envelopes must contain at least one Key block. The contents of a Key block
are as follows. 

value  The key value, a floating-point number. The units and limits of the
       value depend on what parameter the envelope represents. 

time   The time in seconds, a float. This can be negative, zero or positive.
       Keys are listed in the envelope in increasing time order. 

spantype  The curve type, an integer. This determines the kind of interpolation
          that will be performed on the span between this key and the previous
          key, and also indicates what interpolation parameters are stored for
          the key. 

0 - TCB (Kochanek-Bartels) 
1 - Hermite 
2 - 1D Bezier (obsolete, equivalent to Hermite) 
3 - Linear 
4 - Stepped 
5 - 2D Bezier 

p1...p6   Curve parameters. The data depends on the span type. 

TCB, Hermite, 1D Bezier

The first three parameters are tension, continuity and bias. The fourth and fifth
parameters are the incoming and outgoing tangents. The sixth parameter is ignored
and should be 0. Use the first three to evaluate TCB spans, and the other two to
evaluate Hermite spans. 

2D Bezier 

The first two parameters are the incoming time and value, and the second two are
the outgoing time and value. The Behaviors block contains two integers.

pre, post 

Pre- and post-behaviors. These determine how the envelope is extrapolated at
times before the first key and after the last one. 

0 - Reset         Sets the value to 0.0. 
1 - Constant      Sets the value to the value at the nearest key. 
2 - Repeat        Repeats the interval between the first and last keys (the primary interval). 
3 - Oscillate     Like Repeat, but alternating copies of the primary interval are time-reversed. 
4 - Offset Repeat Like Repeat, but offset by the difference between the values of the first and last keys. 
5 - Linear        Linearly extrapolates the value based on the tangent at the nearest key. */
