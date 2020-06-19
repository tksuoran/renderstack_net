namespace RenderStack.Math
{
    public class MathHelper
    {
        //  Relative machine precision.
        private static double Epsilon = 2.22e-16;
        //  The smallest positive floating-point number such that 1/xminin is machine representable.
        private static double XMinInv = 2.23e-308;
        private static double Sqrt2Pi = System.Math.Sqrt(2.0 * System.Math.PI);
        private static double LogSqrt2Pi = System.Math.Log(Sqrt2Pi);
        /**
        * Gamma function.
        * Based on public domain NETLIB (Fortran) code by W. J. Cody and L. Stoltz<BR>
        * Applied Mathematics Division<BR>
        * Argonne National Laboratory<BR>
        * Argonne, IL 60439<BR>
        * <P>
        * References:
        * <OL>
        * <LI>"An Overview of Software Development for Special Functions", W. J. Cody, Lecture Notes in Mathematics, 506, Numerical Analysis Dundee, 1975, G. A. Watson (ed.), Springer Verlag, Berlin, 1976.
        * <LI>Computer Approximations, Hart, Et. Al., Wiley and sons, New York, 1968.
        * </OL></P><P>
        * From the original documentation:
        * </P><P>
        * This routine calculates the GAMMA function for a real argument X. 
        * Computation is based on an algorithm outlined in reference 1. 
        * The program uses rational functions that approximate the GAMMA 
        * function to at least 20 significant decimal digits.  Coefficients 
        * for the approximation over the interval (1,2) are unpublished. 
        * Those for the approximation for X .GE. 12 are from reference 2. 
        * The accuracy achieved depends on the arithmetic system, the 
        * compiler, the intrinsic functions, and proper selection of the 
        * machine-dependent constants. 
        * </P><P>
        * Error returns:<BR>
        * The program returns the value XINF for singularities or when overflow would occur.
        * The computation is believed to be free of underflow and overflow.
        * </P>
        * @return double.MaxValue if overflow would occur, i.e. if abs(x) > 171.624
        * @planetmath GammaFunction
        * @author Jaco van Kooten
        */
        public static double Gamma(double x)
        {
            // Gamma function related constants
            double[] g_p = {
                -1.71618513886549492533811, 24.7656508055759199108314, -379.804256470945635097577,
                629.331155312818442661052, 866.966202790413211295064, -31451.2729688483675254357,
                -36144.4134186911729807069, 66456.1438202405440627855
            };
            double[] g_q = {
                -30.8402300119738975254353, 315.350626979604161529144, -1015.15636749021914166146,
                -3107.77167157231109440444, 22538.1184209801510330112, 4755.84627752788110767815,
                -134659.959864969306392456, -115132.259675553483497211
            };
            double[] g_c = {
                -.001910444077728,8.4171387781295e-4, -5.952379913043012e-4, 7.93650793500350248e-4,
                -.002777777777777681622553, .08333333333333333331554247, .0057083835261
            };

            double  g_xbig = 171.624;
            double  fact = 1.0, xden, xnum;
            int     i, n = 0;
            double  y = x, z, y1;
            bool    parity = false;
            double  res, sum, ysq;

            if(y <= 0.0)
            {
                //  Argument is negative
                y = -(x);
                y1 = (int)y;
                res = y - y1;
                if(res != 0.0)
                {
                    if(y1 != (((int)(y1 * 0.5)) * 2.0))
                    {
                        parity = true;
                    }
                    fact = -System.Math.PI / System.Math.Sin(System.Math.PI * res);
                    y++;
                }
                else
                {
                    return double.MaxValue;
                }
            }

            //  Argument is positive
            if(y < Epsilon)
            {
                //  Argument .LT. EPS
                if(y >= XMinInv)
                {
                    res = 1.0 / y;
                }
                else
                {
                    return double.MaxValue;
                }
            }
            else if (y < 12.0)
            {
                y1 = y;
                if(y < 1.0)
                {
                    //  0.0 .LT. argument .LT. 1.0
                    z = y;
                    y++;
                }
                else
                {
                    //  1.0 .LT. argument .LT. 12.0, reduce argument if necessary
                    n = (int)y - 1;
                    y -= (double) n;
                    z = y - 1.0;
                }
                //  Evaluate approximation for 1.0 .LT. argument .LT. 2.0
                xnum = 0.0;
                xden = 1.0;
                for(i = 0; i < 8; ++i)
                {
                    xnum = (xnum + g_p[i]) * z;
                    xden = xden * z + g_q[i];
                }
                res = xnum / xden + 1.0;
                if(y1 < y)
                {
                    //  Adjust result for case  0.0 .LT. argument .LT. 1.0
                    res /= y1;
                }
                else if (y1 > y)
                {
                    //  Adjust result for case  2.0 .LT. argument .LT. 12.0
                    for(i = 0; i < n; ++i)
                    {
                        res *= y;
                        y++;
                    }
                }
            }
            else
            {
                //  Evaluate for argument .GE. 12.0
                if(y <= g_xbig)
                {
                    ysq = y * y;
                    sum = g_c[6];
                    for (i = 0; i < 6; ++i)
                    {
                        sum = sum / ysq + g_c[i];
                    }
                    sum = sum / y - y + LogSqrt2Pi;
                    sum += (y - 0.5) * System.Math.Log(y);
                    res = System.Math.Exp(sum);
                }
                else
                {
                    return double.MaxValue;
                }
            }
            //  Final adjustments and return
            if(parity)
            {
                res = -res;
            }
            if(fact != 1.0)
            {
                res = fact / res;
            }
            return res;
        } 

        // The largest argument for which logGamma(x) is representable in the machine.
        private static double logGamma_xBig = 2.55e305;

        // Function cache for logGamma
        private static double logGammaCache_res = 0.0;
        private static double logGammaCache_x = 0.0;

        /**
        * The natural logarithm of the gamma function.
        * Based on public domain NETLIB (Fortran) code by W. J. Cody and L. Stoltz<BR>
        * Applied Mathematics Division<BR>
        * Argonne National Laboratory<BR>
        * Argonne, IL 60439<BR>
        * <P>
        * References:
        * <OL>
        * <LI>W. J. Cody and K. E. Hillstrom, 'Chebyshev Approximations for the Natural Logarithm of the Gamma Function,' Math. Comp. 21, 1967, pp. 198-203.
        * <LI>K. E. Hillstrom, ANL/AMD Program ANLC366S, DGAMMA/DLGAMA, May, 1969.
        * <LI>Hart, Et. Al., Computer Approximations, Wiley and sons, New York, 1968.
        * </OL></P><P>
        * From the original documentation:
        * </P><P>
        * This routine calculates the LOG(GAMMA) function for a positive real argument X.
        * Computation is based on an algorithm outlined in references 1 and 2.
        * The program uses rational functions that theoretically approximate LOG(GAMMA)
        * to at least 18 significant decimal digits.  The approximation for X > 12 is from reference 3,
        * while approximations for X < 12.0 are similar to those in reference 1, but are unpublished.
        * The accuracy achieved depends on the arithmetic system, the compiler, the intrinsic functions,
        * and proper selection of the machine-dependent constants.
        * </P><P>
        * Error returns:<BR>
        * The program returns the value XINF for X .LE. 0.0 or when overflow would occur.
        * The computation is believed to be free of underflow and overflow.
        * </P>
        * @return double.MAX_VALUE for x < 0.0 or when overflow would occur, i.e. x > 2.55E305
        * @author Jaco van Kooten
	*/
        public static double LogGamma(double x) {
            // Log Gamma related constants
            double lg_d1 = -.5772156649015328605195174;
            double lg_d2 = .4227843350984671393993777;
            double lg_d4 = 1.791759469228055000094023;
            double[] lg_p1 = {
                4.945235359296727046734888, 201.8112620856775083915565, 2290.838373831346393026739,
                11319.67205903380828685045, 28557.24635671635335736389, 38484.96228443793359990269,
                26377.48787624195437963534, 7225.813979700288197698961
            };
            double[] lg_q1 = {
                67.48212550303777196073036, 1113.332393857199323513008, 7738.757056935398733233834,
                27639.87074403340708898585, 54993.10206226157329794414, 61611.22180066002127833352,
                36351.27591501940507276287, 8785.536302431013170870835
            };
            double[] lg_p2 = {
                4.974607845568932035012064, 542.4138599891070494101986, 15506.93864978364947665077,
                184793.2904445632425417223, 1088204.76946882876749847, 3338152.967987029735917223,
                5106661.678927352456275255, 3074109.054850539556250927
            };
            double[] lg_q2 = {
                183.0328399370592604055942, 7765.049321445005871323047, 133190.3827966074194402448,
                1136705.821321969608938755, 5267964.117437946917577538, 13467014.54311101692290052,
                17827365.30353274213975932, 9533095.591844353613395747
            };
            double[] lg_p4 = {
                14745.02166059939948905062, 2426813.369486704502836312, 121475557.4045093227939592,
                2663432449.630976949898078, 29403789566.34553899906876, 170266573776.5398868392998,
                492612579337.743088758812, 560625185622.3951465078242
            };
            double[] lg_q4 = {
                2690.530175870899333379843, 639388.5654300092398984238, 41355999.30241388052042842,
                1120872109.61614794137657, 14886137286.78813811542398, 101680358627.2438228077304,
                341747634550.7377132798597, 446315818741.9713286462081
            };
            double[] lg_c = {
                -0.001910444077728,8.4171387781295e-4, -5.952379913043012e-4, 7.93650793500350248e-4,
                -0.002777777777777681622553, 0.08333333333333333331554247, 0.0057083835261
            };
            //  Rough estimate of the fourth root of logGamma_xBig
            double  lg_frtbig = 2.25e76;
            double  pnt68 = 0.6796875;
            double  xden, corr, xnum;
            int     i;
            double  y, xm1, xm2, xm4, res, ysq;

            if(x == logGammaCache_x)
            {
                return logGammaCache_res;
            }
            y = x;
            if(y > 0.0 && y <= logGamma_xBig)
            {
                if(y <= Epsilon)
                {
                    res = -System.Math.Log(y);
                }
                else if(y <= 1.5)
                {
                    //  EPS .LT. X .LE. 1.5
                    if(y < pnt68)
                    {
                        corr = -System.Math.Log(y);
                        xm1 = y;
                    }
                    else
                    {
                        corr = 0.0;
                        xm1 = y - 1.0;
                    }
                    if(y <= 0.5 || y >= pnt68)
                    {
                        xden = 1.0;
                        xnum = 0.0;
                        for(i = 0; i < 8; i++)
                        {
                            xnum = xnum * xm1 + lg_p1[i];
                            xden = xden * xm1 + lg_q1[i];
                        }
                        res = corr + xm1 * (lg_d1 + xm1 * (xnum / xden));
                    }
                    else
                    {
                        xm2 = y - 1.0;
                        xden = 1.0;
                        xnum = 0.0;
                        for(i = 0; i < 8; i++)
                        {
                            xnum = xnum * xm2 + lg_p2[i];
                            xden = xden * xm2 + lg_q2[i];
                        }
                        res = corr + xm2 * (lg_d2 + xm2 * (xnum / xden));
                    }
                }
                else if (y <= 4.0)
                {
                    //  1.5 .LT. X .LE. 4.0
                    xm2 = y - 2.0;
                    xden = 1.0;
                    xnum = 0.0;
                    for(i = 0; i < 8; i++)
                    {
                        xnum = xnum * xm2 + lg_p2[i];
                        xden = xden * xm2 + lg_q2[i];
                    }
                    res = xm2 * (lg_d2 + xm2 * (xnum / xden));
                }
                else if(y <= 12.0)
                {
                    //  4.0 .LT. X .LE. 12.0
                    xm4 = y - 4.0;
                    xden = -1.0;
                    xnum = 0.0;
                    for(i = 0; i < 8; i++)
                    {
                        xnum = xnum * xm4 + lg_p4[i];
                        xden = xden * xm4 + lg_q4[i];
                    }
                    res = lg_d4 + xm4 * (xnum / xden);
                }
                else
                {
                    //  Evaluate for argument .GE. 12.0
                    res = 0.0;
                    if (y <= lg_frtbig)
                    {
                        res = lg_c[6];
                        ysq = y * y;
                        for (i = 0; i < 6; i++)
                            res = res / ysq + lg_c[i];
                    }
                    res /= y;
                    corr = System.Math.Log(y);
                    res = res + LogSqrt2Pi - 0.5 * corr;
                    res += y * (corr - 1.0);
                }
            }
            else
            {
                //  Return for bad arguments
                res = double.MaxValue;
            }
            //  Final adjustments and return
            logGammaCache_x = x;
            logGammaCache_res = res;
            return res;
        }

        /**
        * Beta function.
        * @param p require p>0
        * @param q require q>0
        * @return 0 if p<=0, q<=0 or p+q>2.55E305 to avoid errors and over/underflow
        * @author Jaco van Kooten
        */
        public static double Beta(double p, double q)
        {
            if(p <= 0.0 || q <= 0.0 || (p + q) > logGamma_xBig)
            {
                return 0.0;
            }
            else
            {
                return System.Math.Exp(
                    LogBeta(p, q)
                );
            }
        }

        // Function cache for logBeta
        private static double logBetaCache_res = 0.0;
        private static double logBetaCache_p = 0.0;
        private static double logBetaCache_q = 0.0;

        /**
        * The natural logarithm of the beta function.
        * @param p require p>0
        * @param q require q>0
        * @return 0 if p <= 0, q <= 0 or p + q > 2.55E305 to avoid errors and over/underflow
        * @author Jaco van Kooten
        */
        public static double LogBeta(double p, double q)
        {
            if (p != logBetaCache_p || q != logBetaCache_q)
            {
                logBetaCache_p = p;
                logBetaCache_q = q;
                if (p <= 0.0 || q <= 0.0 || (p + q) > logGamma_xBig)
                {
                    logBetaCache_res = 0.0;
                }
                else
                {
                    logBetaCache_res = LogGamma(p) + LogGamma(q) - LogGamma(p + q);
                }
            }
            return logBetaCache_res;
        }
    }
}