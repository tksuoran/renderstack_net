//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Complex /*: IComparable*/
{
    public double Re;
    public double Im;

    //  Modulus() is also know as absolute value - Abs()
    public double   Modulus         { get { return Math.Sqrt(Re * Re + Im * Im); } }
    public double   Argument        { get { return Math.Atan2(Im, Re); } }
    public double   ModulusSquared  { get { return Re * Re + Im * Im; } }
    public Complex  Conjugate       { get { return new Complex(Re, -Im); } }

    public static Complex Zero = new Complex(0, 0);
    public static Complex I    = new Complex(0, 1);

    public Complex(double re, double im)
    {
        Re = re;
        Im = im;
    }

    public Complex(double re)
    {
        Re = re;
        Im = 0.0;
    }

    public static Complex CreateModulusArgument(double modulus, double argument)
    {
        Complex c;
        CreateModulusArgument(modulus, argument, out c);
        return c;
    }

    public static Complex CreateModulusArgument(double modulus, double argument, out Complex c)
    {
        c.Re = modulus * System.Math.Cos(argument);
        c.Im = modulus * System.Math.Sin(argument);
        return c;
    }

    public static Complex operator+(Complex a, Complex b)
    {
        a.Re += b.Re;
        a.Im += b.Im;
        return a;
    }

    public static Complex operator-(Complex a)
    {
        a.Re = -a.Re;
        a.Im = -a.Im;
        return a;
    }

    public static Complex operator+(Complex a, double s)
    {
        a.Re += s;
        return a;
    }

    public static Complex operator-(Complex a, double s)
    {
        a.Re -= s;
        return a;
    }

    public static Complex operator+(double s, Complex a)
    {
        a.Re += s;
        return a;
    }

    public static Complex operator-(double s, Complex a)
    {
        a.Re = s - a.Re;
        return a;
    }

    public static Complex operator-(Complex a, Complex b)
    {
        a.Re -= b.Re;
        a.Im -= b.Im;
        return a;
    }

    public static Complex operator*(Complex a, Complex b)
    {
        return new Complex(
            a.Re * b.Re - a.Im * b.Im, 
            a.Re * b.Im + a.Im * b.Re
        );
    }

    public static Complex operator*(Complex c, double s)
    {
        c.Re *= s; 
        c.Im *= s;
        return c;
    }

    public static Complex operator*(double s, Complex c)
    {
        c.Re *= s;
        c.Im *= s;
        return c;
    }

    public static Complex operator/(Complex c, double s)
    {
        c.Re /= s;
        c.Im /= s;
        return c;
    }

    public static Complex operator/(Complex a, Complex b)
    {
        double  x = a.Re;
        double  y = a.Im;
        double  u = b.Re;
        double  v = b.Im;
        double  denom = u * u + v * v;

        a.Re = (x * u + y * v) / denom;
        a.Im = (y * u - x * v) / denom;

        return a;
    }

    public static Complex Normalize(Complex c)
    {
        double modulus = c.Modulus;
        if(modulus == 0)
        {
            throw new DivideByZeroException();
        }
        return new Complex(c.Re / modulus, c.Im / modulus);
    }

    static private double HalfOfRoot2 = 0.5 * Math.Sqrt(2.0);

    static public Complex Sqrt(Complex c)
    {
        double  modulus = c.Modulus;
        int     sign    = (c.Im < 0) ? -1 : 1;

        c.Im = HalfOfRoot2 * sign * Math.Sqrt(modulus - c.Re);
        c.Re = HalfOfRoot2 * Math.Sqrt(modulus + c.Re);

        return c;
    }

    static public Complex Pow(Complex c, double exponent)
    {
        double modulus  = c.Modulus;
        double argument = c.Argument * exponent;

        c.Re = modulus * System.Math.Cos(argument);
        c.Im = modulus * System.Math.Sin(argument);

        return c;
    }

    public static bool operator==(Complex a, Complex b)
    {
        return (a.Re == b.Re) && (a.Im == b.Im);
    }

    public static bool operator!=(Complex a, Complex b)
    {
        return (a.Re != b.Re) || (a.Im != b.Im);
    }

    public override int GetHashCode()
    {
        return Re.GetHashCode() ^ Im.GetHashCode();
    }

    public override bool Equals(object o)
    {
        if(o is Complex)
        {
            Complex c = (Complex)o;
            return this == c;
        }
        return false;
    }

    public override string ToString() {
        return String.Format("({0}, {1}i)", Re, Im);
    }
}

public class BRDF
{
    public static double Beckmann(double psi, double m)
    {
        double e = Math.E;
        double c = Math.Cos(psi);
        double d = 1.0 / (Math.PI * m * m * c * c * c * c);
        double t = Math.Tan(psi) / m;
        return d * Math.Pow(e, -t * t);
    }

    public static double Gaussian(double psi, double c, double m)
    {
        return c * Math.Pow(Math.E, -psi * psi / (m * m));
    }

    public static double Square(double x)
    {
        return x * x;
    }

    public static double Fresnel(double psi, double F)
    {
        double eta = (1.0 + Math.Sqrt(F)) / (1.0 - Math.Sqrt(F));
        if(eta < 1.0)
        {
            return 1.0;
        }
        double c = Math.Cos(psi);
        double g = Math.Sqrt(eta * eta + c * c - 1.0);
        return 
            0.5 * Square(g - c) / Square(g + c) * (1.0 + Square(c * (g + c) - 1.0) / Square(c * (g - c) + 1.0));
    }

    //------------------------------------------------------
    // Fresnel factors (kr and kt) using full formula for
    // unpolarized light.
    //------------------------------------------------------
    public static double FresnelKr(double cosAlpha, Complex ior)
    {
        if(cosAlpha <= 0)
        {
            return 1.0;
        }
        double  kr  = 1.0;
        double  c2  = cosAlpha * cosAlpha;
        Complex g2  = ior * ior - 1.0 + c2;
        double  ag2 = g2.Modulus;
        double  a   = Math.Sqrt(0.5 * (ag2 + g2.Re));
        double  b   = Math.Sqrt(0.5 * (ag2 - g2.Re));
        double  b2  = b * b;
        double  s   = (1.0 - c2) / cosAlpha;
        double  amc = a - cosAlpha;
        double  apc = a + cosAlpha;
        double  ams = a - s;
        double  aps = a + s;

        kr = 0.5 * ((amc * amc + b2) / (apc * apc + b2)) * 
            (1.0 + ((ams * ams + b2) / (aps * aps + b2)));
        return kr;
    }


    public static double FresnelKt(double cosAlpha, Complex ior)
    {
        return Math.Max(
            0.0, 
            1.0 - FresnelKr(cosAlpha, ior)
        );
    }


    //------------------------------------------------------
    // Fresnel factors using Schlick's approximation
    //------------------------------------------------------
    public static double SchlickKr(double cosAlpha, double ior)
    {
        double kr = (ior - 1.0) / (ior + 1.0); kr *= kr;
        return kr + (1.0 - kr) * Math.Pow(1.0 - cosAlpha, 5);
    }

    public static double SchlickKt(double cosAlpha, double ior)
    {
        return Math.Max(
            0.0, 
            1.0 - SchlickKr(cosAlpha, ior)
        );
    }


}