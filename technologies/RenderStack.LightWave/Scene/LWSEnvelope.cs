using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    public enum Shape
    {
        TCB      = 0,
        Hermite  = 1,
        Bezier1D = 2,
        Linear   = 3,
        Stepped  = 4,
        Bezier2D = 5
    }

    public enum Behavior
    {
        Reset           = 0,
        Constant        = 1,
        Repeat          = 2,
        Oscillate       = 3,
        OffsetRepeat    = 4,
        Linear          = 5
    }
    public class LWSEnvelope
    {
        private List<LWChannelKey>  keys = new List<LWChannelKey>();
        private int                 steps;
        private Behavior            pre_behavior;
        private Behavior            post_behavior;
        private float               last_time;
        private float               last_value;

        public LWSEnvelope()
        {
            steps           = 0;
            pre_behavior    = 0;
            post_behavior   = 0;
            last_time       = float.MaxValue;
            last_value      = 0;
        }

        public List<LWChannelKey>   Keys    { get { return keys; } }
        public Behavior             Post    { get { return post_behavior; } }
        public Behavior             Pre     { get { return pre_behavior; } }
        public int                  Steps   { get { return steps; } }

        public void insert(LWChannelKey channel_key)
        {
            keys.Add(channel_key);
            steps++;
        }
        public void setBehaviors(Behavior pre, Behavior post)
        {
            this.pre_behavior  = pre;
            this.post_behavior = post;
        }

        /*  Given the value v of a periodic function, returns the equivalent value
            v2 in the principal interval [lo, hi].  If i isn't NULL, it receives
            the number of wavelengths between v and v2.

            v2 = v - i * (hi - lo)

            For example, range( 3 pi, 0, 2 pi, i ) returns pi, with i = 1.
        */
        public static float range(float v, float lo, float hi, out int i)
        {
            float v2;
            float r = hi - lo;

            if(r == 0.0f)
            {
                i = 0;
                return lo;
            }
            
            v2 = v - r * (float)System.Math.Floor((v - lo) / r);

            i = -(int)((v2 - v) / r + (v2 > v ? 0.5 : -0.5));

            return v2;
        }

        //  Calculate the Hermite coefficients.
        public static void hermite(float t, out float h1, out float h2, out float h3, out float h4)
        {
            float t2;
            float t3;
            
            t2 = t * t;
            t3 = t * t2;
            
            h2 = 3.0f * t2 - t3 - t3;
            h1 = 1.0f - h2;
            h4 =  t3 - t2;
            h3 = h4 - t2 + t;
        }

        //  Interpolate the value of a 1D Bezier curve.
        public static float bezier(float x0, float x1, float x2, float x3, float t)
        {
            float a;
            float b;
            float c;
            float t2;
            float t3;

            t2 = t  * t;
            t3 = t2 * t;

            c  = 3.0f * ( x1 - x0 );
            b  = 3.0f * ( x2 - x1 ) - c;
            a  = x3 - x0 - c - b;

            return a * t3 + b * t2 + c * t + x0;
        }

        /*  Find the t for which bezier() returns the input time.  The handle
            endpoints of a BEZ2 curve represent the control points, and these have
            (time, value) coordinates, so time is used as both a coordinate and a
            parameter for this curve type.
        */
        public static float bez2_time(float x0, float x1, float x2, float x3, float time, ref float t0, ref float t1)
        {
            float v;
            float t;
            
            t = t0 + (t1 - t0) * 0.5f;
            v = bezier(x0, x1, x2, x3, t);
            if(System.Math.Abs(time - v) > .0001f)
            {
                if(v > time)
                {
                   t1 = t;
                }
                else
                {
                   t0 = t;
                }
                return bez2_time(x0, x1, x2, x3, time, ref t0, ref t1);
            }
            else
            {
                return t;
            }
        }

        //  Interpolate the value of a BEZ2 curve.
        public static float bez2(LWChannelKey key0, LWChannelKey key1, float time)
        {
            float t0 = 0.0f;
            float t1 = 1.0f;

            float x = key0.Time  + (( key0.Shape == Shape.Bezier2D ) ? key0.p3 : ( key1.Time - key0.Time ) / 3.0f);
            float t = bez2_time     ( key0.Time, x, key1.Time + key1.p1, key1.Time, key0.Time, ref t0, ref t1);
            float y = key0.Value + (( key0.Shape == Shape.Bezier2D ) ? key0.p4 : key0.p2 / 3.0f);

            return bezier(key0.Value, y, key1.p2 + key1.Value, key1.Value, t);
        }

        /*  Return the outgoing tangent to the curve at key0.  The value returned
            for the BEZ2 case is used when extrapolating a linear pre behavior and
            when interpolating a non-BEZ2 span.
        */
        public float outgoing(LWChannelKey prev, LWChannelKey key0, LWChannelKey key1)
        {
            float a;
            float b;
            float d;
            float t;
            float @out;

            switch(key0.Shape)
            {
                case Shape.TCB:
                {
                    a = (1.0f - key0.Tension) * (1.0f + key0.Continuity) * (1.0f + key0.Bias);
                    b = (1.0f - key0.Tension) * (1.0f - key0.Continuity) * (1.0f - key0.Bias);
                    d = key1.Value - key0.Value;

                    if(prev != null)
                    {
                        t   = (key1.Time - key0.Time) / (key1.Time - prev.Time);
                        @out = t * (a * (key0.Value - prev.Value) + b * d);
                    }
                    else
                    {
                        @out = b * d;
                    }
                    break;
                }

                case Shape.Linear:
                {
                    d = key1.Value - key0.Value;
                    if(prev != null)
                    {
                        t   = (key1.Time - key0.Time) / (key1.Time - prev.Time);
                        @out = t * (key0.Value - prev.Value + d);
                    }else{
                        @out = d;
                    }
                    break;
                }

                case Shape.Bezier1D: goto case Shape.Hermite;
                case Shape.Hermite:
                {
                    @out = key0.p5;
                    if(prev != null)
                    {
                        @out *= (key1.Time - key0.Time) / (key1.Time - prev.Time);
                    }
                    break;
                }

                case Shape.Bezier2D:
                {
                    @out = key0.p4 * (key1.Time - key0.Time);
                    if(System.Math.Abs(key0.p3) > 1e-5f)
                    {
                        @out /= key0.p3;
                    }else{
                        @out *= 1e5f;
                    }
                    break;
                }

                case Shape.Stepped: goto default;
                default:
                {
                    @out = 0.0f;
                    break;
                }
            }
            
            return @out;
        }

        /*  Return the incoming tangent to the curve at key1.  The value returned
            for the BEZ2 case is used when extrapolating a linear post behavior.
        */
        public static float incoming(LWChannelKey key0, LWChannelKey key1, LWChannelKey next)
        {
            float a;
            float b;
            float d;
            float t;
            float @in;

            switch(key1.Shape)
            {
                case Shape.Linear:
                {
                    d = key1.Value - key0.Value;
                    if(next != null)
                    {
                        t = (key1.Time - key0.Time) / (next.Time - key0.Time);
                        @in = t * (next.Value - key1.Value + d);
                    }else{
                        @in = d;
                    }
                    break;
                }
                case Shape.TCB:
                {
                    a = (1.0f - key1.Tension) * (1.0f - key1.Continuity) * (1.0f + key1.Bias);
                    b = (1.0f - key1.Tension) * (1.0f + key1.Continuity) * (1.0f - key1.Bias);
                    d = key1.Value - key0.Value;
                    
                    if(next != null)
                    {
                        t  = ( key1.Time - key0.Time ) / ( next.Time - key0.Time );
                        @in = t * ( b * ( next.Value - key1.Value ) + a * d );
                    }else{
                        @in = a * d;
                    }
                    break;
                }
                case Shape.Bezier1D: goto case Shape.Hermite;
                case Shape.Hermite:
                {
                    @in = key1.p4;
                    if(next != null)
                    {
                        @in *= (key1.Time - key0.Time) / (next.Time - key0.Time);
                    }
                    break;
                }
                case Shape.Bezier2D:
                {
                    @in = key1.p2 * (key1.Time - key0.Time);
                    if(System.Math.Abs(key1.p1) > 1e-5f)
                    {
                        @in /= key1.p1;
                    }
                    else
                    {
                        @in *= 1e5f;
                    }
                    break;
                }
                case Shape.Stepped: goto default;
                default:
                {
                    @in = 0.0f;
                    break;
                }
            }

            return @in;
        }

        /*  Given a list of keys and a time, returns the interpolated value of the
            envelope at that time.
        */
        public float eval(float time)
        {
            if(time == last_time)
            {
                return last_value;
            }
            else
            {
                last_time = time;
            }

            float  t;
            float  @in;
            float  @out;
            float  offset = 0.0f;
            int    noff;

            if(keys.Count == 0)
            {
                last_value = 0;
                return last_value;
            }

            //  If there's only one key, the value is constant
            if(keys.Count == 1)
            {
                last_value = keys.First().Value;
                return last_value;
            }

            //  Get the first key
            LWChannelKey skey = keys.First();
            LWChannelKey ekey = keys.Last();
            LWChannelKey next = (keys.Count > 1) ? keys[1] : null;
            LWChannelKey prev = (keys.Count > 1) ? keys[keys.Count -2] : null;

            //  Use pre-behavior if time is before first key time
            if(time < skey.Time)
            {
                switch(pre_behavior)
                {
                    case Behavior.Reset:
                    {
                        last_value = 0;
                        return last_value;
                    }
                    case Behavior.Constant:
                    {
                        last_value = skey.Value;
                        return last_value;
                    }
                    case Behavior.Repeat:
                    {
                        int dummy;
                        last_time = time = range(time, skey.Time, ekey.Time, out dummy);
                        break;
                    }
                    case Behavior.Oscillate:
                    {
                        last_time = time = range(time, skey.Time, ekey.Time, out noff);
                        if((noff % 2) != 0)
                        {
                            last_time = time = ekey.Time - skey.Time - time;
                        }
                        break;
                    }
                    case Behavior.OffsetRepeat:
                    {
                        last_time = time = range(time, skey.Time, ekey.Time, out noff);
                        offset    = noff * (ekey.Value - skey.Value);
                        break;
                    }
                    case Behavior.Linear:
                    {
                        @out       = outgoing(null, skey, next) / ( next.Time - skey.Time);
                        last_value = @out * (time - skey.Time) + skey.Value;;
                        return last_value;
                    }
                    default:
                    {
                        last_value = 0;
                        return last_value;
                    }
                }
            }

            //  Use post-behavior if time is after last key time
            else if(time > ekey.Time)
            {
                switch(post_behavior)
                {
                    case Behavior.Reset:
                    {
                        last_value = 0;
                        return last_value;
                    }
                    case Behavior.Constant:
                    {
                        last_value = ekey.Value;
                        return last_value;
                    }
                    case Behavior.Repeat:
                    {
                        int dummy;
                        time = range(time, skey.Time, ekey.Time, out dummy);
                        break;
                    }
                    case Behavior.Oscillate:
                    {
                        last_time = time = range(time, skey.Time, ekey.Time, out noff);
                        if((noff % 2) != 0)
                        {
                           last_time = time = ekey.Time - skey.Time - time;
                        }
                        break;
                    }
                    case Behavior.OffsetRepeat:
                    {
                        last_time = time = range(time, skey.Time, ekey.Time, out noff);
                        offset    = noff * ( ekey.Value - skey.Value );
                        break;
                    }
                    case Behavior.Linear:
                    {
                        @in = incoming(prev, ekey, null) / (ekey.Time - prev.Time);
                        last_value = @in * ( time - ekey.Time ) + ekey.Value;
                        return last_value;
                    }
                    default:
                    {
                        last_value = 0;
                        return last_value;
                    }
                }
            }

            //  Get the endpoints of the interval being evaluated
            //k_it = keys.begin();
            prev = null;
            LWChannelKey key0 = null;
            LWChannelKey key1 = null;
            next = null;
            int i;
            for(i = 0; i < keys.Count; ++i)
            {
                prev = key0;
                key0 = key1;
                key1 = keys[i];
                if(time <= key1.Time)
                {
                    if(key0 == null)
                    {
                        key0 = key1;
                        if(i + 1 < keys.Count)
                        {
                            key1 = keys[i + 1];
                        }
                    }
                    break;
                }
            }
            if(i + 1 < keys.Count)
            {
                next = keys[i + 1];
            }

        /*    debug_msg(
                "%9.4f <= %9.4f <= %9.4f",
                key0 != NULL ? key0.time : 0,
                time,
                key1 != NULL ? key1.time : 0
            );*/

            //  Check for singularities first
            if(time == key0.Time)
            {
                last_value = key0.Value + offset;
                return last_value;
            }
            else if(time == key1.Time)
            {
                last_value = key1.Value + offset;
                return last_value;
            }

            if((key0 == null) || (key1 == null) )
            {
                throw new System.Exception("Channel Interpolation error");
                //return 0;
            }

            //  Get interval length, time in [0, 1]
            t = (time - key0.Time) / (key1.Time - key0.Time);

            //  Interpolate
            switch(key1.Shape)
            {
                case Shape.TCB: goto case Shape.Hermite;
                case Shape.Bezier1D: goto case Shape.Hermite;
                case Shape.Hermite:
                {
                    @out = outgoing(prev, key0, key1);
                    @in  = incoming(key0, key1, next);
                    float h1;
                    float h2;
                    float h3;
                    float h4;
                    hermite(t, out h1, out h2, out h3, out h4);
                    last_value = h1 * key0.Value + h2 * key1.Value + h3 * @out + h4 * @in + offset;
                    return last_value;
                }
                case Shape.Bezier2D:
                {
                    last_value = bez2(key0, key1, time) + offset;
                    return last_value;
                }
                case Shape.Linear:
                {
                    last_value = key0.Value + t * (key1.Value - key0.Value) + offset;
                    return last_value;
                }
                case Shape.Stepped:
                {
                    last_value = key0.Value + offset;
                    return last_value;
                }
                default:
                {
                    last_value = offset;
                    return last_value;
                }
            }
        }
    }
}



/*
Envelopes

An envelope defines a function of time. For any animation time,
an envelope's parameters can be combined to generate a value at
that time. Envelopes are used to store position coordinates,
rotation angles, scale factors, camera zoom, light intensity,
texture parameters, and anything else that can vary over time.

The envelope function is a piecewise polynomial curve. The
function is tabulated at specific points, called keys. The
curve segment between two adjacent keys is called a span, and
values on the span are calculated by interpolating between the
keys. The interpolation can be linear, cubic, or stepped, and
it can be different for each span. The value of the function
before the first key and after the last key is calculated by
extrapolation.

In scene files, an envelope is stored in a block named Envelope
that contains one or more nested Key blocks and one Behaviors block.

   { Envelope
     nkeys
     Key value time spantype p1 p2 p3 p4 p5 p6
     Key ...
     Behaviors pre post
   }

The nkeys value is an integer, the number of Key blocks in
the envelope. Envelopes must contain at least one Key block.
The contents of a Key block are as follows. 

value 

The key value, a floating-point number. The units and limits
of the value depend on what parameter the envelope represents. 

time 

The time in seconds, a float. This can be negative, zero or
positive. Keys are listed in the envelope in increasing time
order. 

spantype 

The curve type, an integer. This determines the kind of
interpolation that will be performed on the span between
this key and the previous key, and also indicates what
interpolation parameters are stored for the key. 

0 - TCB (Kochanek-Bartels) 
1 - Hermite 
2 - 1D Bezier (obsolete, equivalent to Hermite) 
3 - Linear 
4 - Stepped 
5 - 2D Bezier 

p1...p6 Curve parameters. The data depends on the span type. 

TCB, Hermite, 1D Bezier 

The first three parameters are tension(), continuity() and bias().
The fourth and fifth parameters are the incoming and outgoing
tangents. The sixth parameter is ignored and should be 0. Use
the first three to evaluate TCB spans, and the other two to
evaluate Hermite spans. 

2D Bezier 

The first two parameters are the incoming time and value,
and the second two are the outgoing time and value. 

The Behaviors block contains two integers.

pre, post 

Pre- and post-behaviors. These determine how the envelope
is extrapolated at times before the first key and after
the last one. 

0 - Reset          Sets the value to 0.0. 
1 - Constant       Sets the value to the value at the nearest key. 
2 - Repeat         Repeats the interval between the first and last keys (the primary interval). 
3 - Oscillate      Like Repeat, but alternating copies of the primary interval are time-reversed. 
4 - Offset Repeat  Like Repeat, but offset by the difference between the values of the first and last keys. 
5 - Linear         Linearly extrapolates the value based on the tangent at the nearest key. 

The source code in the sample/envelope directory of the LightWave plug-in SDK demonstrates
how envelopes are evaluated.
*/

