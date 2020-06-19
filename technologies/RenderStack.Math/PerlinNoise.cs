using System;

//  Trivially C# port of the improved perlin noise reference implementation
//  http://mrl.nyu.edu/~perlin/noise/
namespace RenderStack.Math
{
    public class Perlin
    {
        static readonly int[] p = {
            151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
            140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
            247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
             57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
             74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
             60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
             65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
            200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
             52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
            207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
            119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
            129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
            218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
            81,  51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
            184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
            222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

            151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
            140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
            247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
             57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
             74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
             60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
             65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
            200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
             52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
            207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
            119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
            129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
            218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
            81,  51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
            184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
            222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
        };

        public static double Noise(Vector3 v)
        {
            return Noise(v.X, v.Y, v.Z);
        }

#if false
        public static double Noise(double x, double y)
        {
            //  Find out unit cube that contains point
            int X = (int)System.Math.Floor(x) & 255;
            int Y = (int)System.Math.Floor(y) & 255;

            //  Find relative X, Y, Z of point in cube
            x -= System.Math.Floor(x);
            y -= System.Math.Floor(y);

            //  Compute fade curves for each of X, Y, Z
            double u = Fade(x);
            double v = Fade(y);

            //  Hash coordinates of the 4 corners
            //  Hash coordinates of the 8 cube corners
            int A  = p[X    ] + Y;
            int AA = p[A    ];
            int AB = p[A + 1];
            int B  = p[X + 1] + Y;
            int BA = p[B    ];
            int BB = p[B + 1];


            // And add blended results from 8 corners of cube
            return Lerp(v, Lerp(u, Grad(p[AA    ], x    , y    ),  
                                   Grad(p[BA    ], x - 1, y    )), 
                           Lerp(u, Grad(p[AB    ], x    , y - 1),  
                                   Grad(p[BB    ], x - 1, y - 1)));
        }
#endif

        public static double Noise(double x, double y, double z)
        {
            //  Find out unit cube that contains point
            int X = (int)System.Math.Floor(x) & 255;
            int Y = (int)System.Math.Floor(y) & 255;
            int Z = (int)System.Math.Floor(z) & 255;

            //  Find relative X, Y, Z of point in cube
            x -= System.Math.Floor(x);
            y -= System.Math.Floor(y);
            z -= System.Math.Floor(z);

            //  Compute fade curves for each of X, Y, Z
            double u = Fade(x);
            double v = Fade(y);
            double w = Fade(z);

            //  Hash coordinates of the 8 cube corners
            int A  = p[X    ] + Y;
            int AA = p[A    ] + Z;
            int AB = p[A + 1] + Z;
            int B  = p[X + 1] + Y;
            int BA = p[B    ] + Z;
            int BB = p[B + 1] + Z;

            // And add blended results from 8 corners of cube
            return Lerp(w, Lerp(v, Lerp(u, Grad(p[AA    ], x    , y    , z     ),  
                                           Grad(p[BA    ], x - 1, y    , z     )), 
                                   Lerp(u, Grad(p[AB    ], x    , y - 1, z     ),  
                                           Grad(p[BB    ], x - 1, y - 1, z     ))),
                           Lerp(v, Lerp(u, Grad(p[AA + 1], x    , y    , z - 1 ),  
                                           Grad(p[BA + 1], x - 1, y    , z - 1 )), 
                                   Lerp(u, Grad(p[AB + 1], x    , y - 1, z - 1 ),
                                           Grad(p[BB + 1], x - 1, y - 1, z - 1 ))));
        }

        //  This is an addition to the reference implementation. I have not yet
        //  tested this much.
        //  Reference:  http://www.iquilezles.org/www/articles/morenoise/morenoise.htm
        static double NoiseWithDerivates(
            double x,
            double y,
            double z,
            out double du, 
            out double dv,
            out double dw
        )
        {
            //  Find out unit cube that contains point
            int X = (int)System.Math.Floor(x) & 255;
            int Y = (int)System.Math.Floor(y) & 255;
            int Z = (int)System.Math.Floor(z) & 255;

            //  Find relative X, Y, Z of point in cube
            x -= System.Math.Floor(x);
            y -= System.Math.Floor(y);
            z -= System.Math.Floor(z);

            double dx = 30.0 * x * x * (x * (x - 2.0f) + 1.0f);
            double dy = 30.0 * y * y * (y * (y - 2.0f) + 1.0f);
            double dz = 30.0 * z * z * (z * (z - 2.0f) + 1.0f);

            //  Compute fade curves for each of X, Y, Z
            double u = Fade(x);
            double v = Fade(y);
            double w = Fade(z);

            //  Hash coordinates of the 8 cube corners
            int A  = p[X    ] + Y;
            int AA = p[A    ] + Z;
            int AB = p[A + 1] + Z;
            int B  = p[X + 1] + Y;
            int BA = p[B    ] + Z;
            int BB = p[B + 1] + Z;

            double a = Grad(p[AA    ], x    , y    , z     );
            double b = Grad(p[BA    ], x - 1, y    , z     );
            double c = Grad(p[AB    ], x    , y - 1, z     );
            double d = Grad(p[BB    ], x - 1, y - 1, z     );
            double e = Grad(p[AA + 1], x    , y    , z - 1 );
            double f = Grad(p[BA + 1], x - 1, y    , z - 1 );
            double g = Grad(p[AB + 1], x    , y - 1, z - 1 );
            double h = Grad(p[BB + 1], x - 1, y - 1, z - 1 );

            double k0 =   a;
            double k1 =   b - a;
            double k2 =   c - a;
            double k3 =   e - a;
            double k4 =   a - b - c + d;
            double k5 =   a - c - e + g;
            double k6 =   a - b - e + f;
            double k7 = - a + b + c - d + e - f - g + h;

            du = dx * (k1 + k4*v + k6*w + k7*v*w);
            dv = dy * (k2 + k5*w + k4*u + k7*w*u);
            dw = dz * (k3 + k6*u + k5*v + k7*u*v);
            return k0 + k1*u + k2*v + k3*w + k4*u*v + k5*v*w + k6*w*u + k7*u*v*w;

#if false
            int   i, j, k;
            float u, v, w;

            iGetIntegerAndFractional( x, &X, &u );
            iGetIntegerAndFractional( y, &Y, &v );
            iGetIntegerAndFractional( z, &Z, &w );

            const float du = 30.0f*u*u*(u*(u-2.0f)+1.0f);
            const float dv = 30.0f*v*v*(v*(v-2.0f)+1.0f);
            const float dw = 30.0f*w*w*(w*(w-2.0f)+1.0f);

            u = u*u*u*(u*(u*6.0f-15.0f)+10.0f);
            v = v*v*v*(v*(v*6.0f-15.0f)+10.0f);
            w = w*w*w*(w*(w*6.0f-15.0f)+10.0f);

            const float a = myRandomMagic( i+0, j+0, k+0 );
            const float b = myRandomMagic( i+1, j+0, k+0 );
            const float c = myRandomMagic( i+0, j+1, k+0 );
            const float d = myRandomMagic( i+1, j+1, k+0 );
            const float e = myRandomMagic( i+0, j+0, k+1 );
            const float f = myRandomMagic( i+1, j+0, k+1 );
            const float g = myRandomMagic( i+0, j+1, k+1 );
            const float h = myRandomMagic( i+1, j+1, k+1 );

            const float k0 =   a;
            const float k1 =   b - a;
            const float k2 =   c - a;
            const float k3 =   e - a;
            const float k4 =   a - b - c + d;
            const float k5 =   a - c - e + g;
            const float k6 =   a - b - e + f;
            const float k7 = - a + b + c - d + e - f - g + h;

            vout[0] = k0 + k1*u + k2*v + k3*w + k4*u*v + k5*v*w + k6*w*u + k7*u*v*w;
            vout[1] = du * (k1 + k4*v + k6*w + k7*v*w);
            vout[2] = dv * (k2 + k5*w + k4*u + k7*w*u);
            vout[3] = dw * (k3 + k6*u + k5*v + k7*u*v);
#endif
        }

        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10); 
        }
        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }
        private static double Grad(int hash, double x, double y, double z)
        {
            //  Convert lower 4 bits of hash code into twelve gradient directions.
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = (h < 4) ? y : (h == 12) || (h == 14) ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
        private static double Grad(int hash, float x, float y)
        {
            int h = hash & 3;
            return (((h & 2) == 0) ? x : -x) + (((h & 1) == 0) ? y : -y);
        }
    }
}

