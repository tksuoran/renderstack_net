//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;

namespace RenderStack.Math
{
    /// \note Somewhat experimental.
    public class Conversions
    {
        public static float DegreesToRadians(float degrees)
        {
            const float degToRad = (float)System.Math.PI / 180.0f;
            return degrees * degToRad;
        }

        public static void HSVtoRGB(float h, float s, float v, out float r, out float g, out float b)
        {
            r = 0.0f;
            g = 0.0f;
            b = 0.0f;
            float f;
            float p;
            float q;
            float t;
            int   i;

            if(s == 0.0f)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                if(h == 360.0f)
                {
                    h = 0.0f;
                }
                h = h / 60.0f;
                i = (int)(h);
                f = h - i;
                p = v * (1.0f -  s             );
                q = v * (1.0f - (s *         f ));
                t = v * (1.0f - (s * (1.0f - f)));
                switch(i)
                {
                    case 0:
                    {
                        r = v;
                        g = t;
                        b = p;
                        break;
                    }
                    case 1:
                    {
                        r = q;
                        g = v;
                        b = p;
                        break;
                    }
                    case 2:
                    {
                        r = p;
                        g = v;
                        b = t;
                        break;
                    }
                    case 3:
                    {
                        r = p;
                        g = q;
                        b = v;
                        break;
                    }
                    case 4:
                    {
                        r = t;
                        g = p;
                        b = v;
                        break;
                    }
                    case 5:
                    {
                        r = v;
                        g = p;
                        b = q;
                        break;
                    }
                    default:
                    {
                        r = 1.0f;
                        b = 1.0f;
                        b = 1.0f;
                        break;
                    }
                }
            }
        }
        public static void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
        {
            h = 0;
            s = 1.0f;
            v = 1.0f;
            float diff;
            float rDist;
            float gDist;
            float bDist;
            float undefined = 0;

            float max;
            float min;

            if(r > g)
            {
                max = r;
            }
            else
            {
                max = g;
            }
            if(b > max)
            {
                max = b;
            }
            if(r < g)
            {
                min = r;
            }
            else
            {
                min = g;
            }
            if(b < min)
            {
                min = b;
            }

            diff = max - min;
            v    = max;

            if(max < 0.0001f)
            {
                s = 0;
            }
            else
            {
                s = diff / max;
            }
            if(s == 0)
            {
                h = undefined;
            }
            else
            {
                rDist = (max - r) / diff;
                gDist = (max - g) / diff;
                bDist = (max - b) / diff;
                if(r == max)
                {
                    h = bDist - gDist;
                }
                else
                {
                    if(g == max)
                    {
                        h = 2.0f + rDist - bDist;
                    }
                    else
                    {
                        if(b == max)
                        {
                            h = 4.0f + gDist - rDist;
                        }
                    }
                }
                h = h * 60.0f;
                if(h < 0)
                {
                    h += 360.0f;
                }
            }
        }
        public static float sRGBtoLinear(float cs)
        {
            float cl = 0.0f;

            if(cs < 0.0f)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            if(cs > 1.0f)
            {
                throw new System.ArgumentOutOfRangeException();
            }

            if(cs <= 0.04045f)
            {
                cl = cs / 12.92f;
            }
            else
            {
                cl = (float)(System.Math.Pow(
                    (cs + 0.055f) / 1.055f, 
                    2.4f
                ));
            }

            return cl;
        }
        public static float LinearRGBtosRGB(float cl)
        {
            if(cl > 1.0f)
            {
                return 1.0f;
            }
            else if(cl < 0.0)
            {
                return 0.0f;
            }
            else if(cl < 0.0031308f)
            {
                return 12.92f * cl;
            }
            else
            {
                return 1.055f * (float)(System.Math.Pow(cl, 0.41666f)) - 0.055f;
            }
        }
        public static Vector3 sRGBtoLinearRGB(Vector3 sRGB)
        {
            return new Vector3(
                sRGBtoLinear(sRGB.X),
                sRGBtoLinear(sRGB.Y),
                sRGBtoLinear(sRGB.Z)
            );
        }
        public static Vector3 LinearRGBtosRGB(Vector3 linearRGB)
        {
            return new Vector3(
                LinearRGBtosRGB(linearRGB.X),
                LinearRGBtosRGB(linearRGB.Y),
                LinearRGBtosRGB(linearRGB.Z)
            );
        }

    }
}
