using System;

using RenderStack.Math;

namespace WildMagic
{
    public class Integrate
    {
        public delegate float Function(float t, object userData);

        public static float RombergIntegral(int order, float a, float b, Function function, object userData)
        {
            //assertion(order > 0, "Integration order must be positive\n");
            float[,] rom = new float[order, 2];

            float h = b - a;

            rom[0, 0] = (0.5f) * h * (function(a, userData) + function(b, userData));
            for(int i0 = 2, p0 = 1; i0 <= order; ++i0, p0 *= 2, h *= 0.5f)
            {
                // Approximations via the trapezoid rule.
                float sum = 0f;
                int i1;
                for(i1 = 1; i1 <= p0; ++i1)
                {
                    sum += function(a + h * (i1 - (0.5f)), userData);
                }

                // Richardson extrapolation.
                rom[1, 0] = (0.5f) * (rom[0, 0] + h * sum);
                for(int i2 = 1, p2 = 4; i2 < i0; ++i2, p2 *= 4)
                {
                    rom[1, i2] = (p2 * rom[1, i2 - 1] - rom[0, i2 - 1]) / (p2 - 1);
                }

                for (i1 = 0; i1 < i0; ++i1)
                {
                    rom[0, i1] = rom[1, i1];
                }
            }

            float result = rom[0, order - 1];
            
            return result;
        }
        public static float GaussianQuadrature(float a, float b, Function function, object userData)
        {
            // Legendre polynomials:
            // P_0(x) = 1
            // P_1(x) = x
            // P_2(x) = (3x^2-1)/2
            // P_3(x) = x(5x^2-3)/2
            // P_4(x) = (35x^4-30x^2+3)/8
            // P_5(x) = x(63x^4-70x^2+15)/8

            // Generation of polynomials:
            //   d/dx[ (1-x^2) dP_n(x)/dx ] + n(n+1) P_n(x) = 0
            //   P_n(x) = sum_{k=0}^{floor(n/2)} c_k x^{n-2k}
            //     c_k = (-1)^k (2n-2k)! / [ 2^n k! (n-k)! (n-2k)! ]
            //   P_n(x) = ((-1)^n/(2^n n!)) d^n/dx^n[ (1-x^2)^n ]
            //   (n+1)P_{n+1}(x) = (2n+1) x P_n(x) - n P_{n-1}(x)
            //   (1-x^2) dP_n(x)/dx = -n x P_n(x) + n P_{n-1}(x)

            // Roots of the Legendre polynomial of specified degree.
            int degree = 5;
            float[] root =
            {
                -0.9061798459f,
                -0.5384693101f,
                 0.0f,
                +0.5384693101f,
                +0.9061798459f
            };
            float[] coeff =
            {
                0.2369268850f,
                0.4786286705f,
                0.5688888889f,
                0.4786286705f,
                0.2369268850f
            };

            // Need to transform domain [a,b] to [-1,1].  If a <= x <= b and
            // -1 <= t <= 1, then x = ((b-a)*t+(b+a))/2.
            float radius = (0.5f) * (b - a);
            float center = (0.5f) * (b + a);

            float result = 0f;
            for(int i = 0; i < degree; ++i)
            {
                result += coeff[i] * function(radius * root[i] + center, userData);
            }
            result *= radius;

            return result;
        }
        public static float TrapezoidRule(int numSamples, float a, float b, Function function, object userData)
        {
            //assertion(numSamples >= 2, "Must have more than two samples\n");

            float h = (b - a) / (float)(numSamples - 1);
            float result = 0.5f * (function(a, userData) + function(b, userData));

            for (int i = 1; i <= numSamples - 2; ++i)
            {
                result += function(a+i*h, userData);
            }
            result *= h;
            return result;
        }
    };
}
