//  Musgrave fractal noise functions from "Texturing and Modeling: Procedural Approach".
//  
//  Sample code from the book is available from:                                  
//  http://cobweb.ecn.purdue.edu/~ebertd/texture/1stEdition/musgrave/musgrave.c   
//  http://cobweb.ecn.purdue.edu/~ebertd/texture/musgrave/Multicolor/noise.c      

namespace RenderStack.Math
{
    //  This class is used to cache power function values
    //  as done book reference code. 
    public class Musgrave
    {
        protected double    H;              //  Determines the highest fractal dimension
        protected double    lacunarity;     //  Gap between successive frequencies
        protected double    octaves;        //  The number of frequencies in the fBm
        protected double[]  exponent_array; //  Cached power function values

        public Musgrave(double H, double lacunarity, double octaves)
        {
            this.H = H;
            this.lacunarity = lacunarity;
            this.octaves = octaves;
            exponent_array = new double[(int)(octaves + 1.0)];
            double frequency = 1.0;
            for(int i = 0; i <= (int)octaves; i++)
            {
                exponent_array[i] = System.Math.Pow(frequency, -H);
                frequency *= lacunarity;
            }
        }

        //  Ken Perlin's original version of vector-valued fBm
        public static Vector3 Wrinkled(Vector3 point, double lacunarity, double H, double octaves)
        {
            Vector3 result = Vector3.Zero;
            float f = 1.0f;
            float s = 1.0f;
            for(int i = 0; i < octaves; i++)
            {
                Vector3 sPoint = f * point;
                Vector3 temp = Musgrave.VecNoise(sPoint);
                result += temp * (float)s;
                s *= (float)H;
                f *= (float)lacunarity;
            }
            return(result);
        }

        //  Simple Vector3 variation of noise
        public static Vector3 VecNoise(Vector3 point)
        {
            Vector3 result;
            result.X = (float)Perlin.Noise(point);
            point.X += 10.0f;
            result.Y = (float)Perlin.Noise(point);
            point.Y += 10.0f;
            result.Z = (float)Perlin.Noise(point);
            return result;
        }

        //  Variable Lacunarity Noise - a distorted variety of Perlin noise.
        public static double VLNoise(Vector3 point, double distortion)
        {
            //  Misregister domain
            Vector3 offset = point + new Vector3(0.5f, 0.5f, 0.5f);

            //  Get a random vector
            offset = VecNoise(offset);

            //  Scale the randomization
            offset *= (float)distortion;

            //  "point" is the domain; distort domain by adding "offset"
            point = point + offset;

            //  Distorted-domain noise
            return Perlin.Noise(point);
        }

        //  A simplified multifractal function
        public static double SimplifiedMultifractal(Vector3 pos, float H, float lacunarity, float octaves, float zero_offset)
        {
            float y = 1.0f;
            float f = 1.0f;

            for(int i = 0; i < octaves; i++)
            {
                y *= zero_offset + f * (float)(Perlin.Noise(pos));
                f *= H;
                pos *= (float)lacunarity;
            }

            return y;
        }

        //  Procedural fBm evaluated at "point"; returns value stored in "value".
        public double FBm(Vector3 point)
        {
            //  Initialize vars to proper values
            double value = 0.0;

            //  Inner loop of spectral construction
            int i;
            for(i = 0; i < (int)octaves; i++)
            {
                value += Perlin.Noise(point) * exponent_array[i];
                point *= (float)lacunarity;
            }

            double remainder = octaves - (int)octaves;
            if(remainder != 0 )
            {
                //  Add in "octaves" remainder
                //  "i" and spatial freq. are preset in loop above
                value += remainder * Perlin.Noise(point) * exponent_array[i];
            }

            return value;
        }

        public double Multifractal(Vector3 point, double offset)
        {
            double value = 1.0;
            double frequency = 1.0;

            //  Inner loop of multifractal construction
            int i;
            for(i = 0; i < octaves; i++)
            {
                value *= offset * frequency * Perlin.Noise(point);
                point *= (float)lacunarity;
            }

            double remainder = octaves - (int)octaves;
            if(remainder != 0.0)
            {
                //  Add in "octaves" remainder
                //  "i"  and spatial freq. are preset in loop above
                value += remainder * Perlin.Noise(point) * exponent_array[i];
            }

            return value;
        }

        //  Heterogeneous procedural terrain function: stats by altitude method. 
        //  Evaluated at "point"; returns value stored in "value".               
        //                                                                       
        //  offset  raises the terrain from `sea level'                          
        public double HeteroTerrain(Vector3 point, double offset)
        {
            //  First unscaled octave of function; later octaves are scaled
            double value = offset + Perlin.Noise(point);
            point.X *= (float)lacunarity;

            //  Spectral construction inner loop, where the fractal is built
            int i;
            for(i = 1; i < octaves; i++)
            {
                //  Obtain displaced noise value
                double increment = Perlin.Noise(point) + offset;

                //  Scale amplitude appropriately for this frequency
                increment *= exponent_array[i];

                //  Scale increment by current `altitude' of function
                increment *= value;

                //  Add increment to "value"
                value += increment;

                //  Raise spatial frequency
                point *= (float)lacunarity;
            }

            //  Take care of remainder in "octaves"
            double remainder = octaves - (int)octaves;
            if(remainder != 0.0)
            {
                //  "i"  and spatial freq. are preset in loop above
                //  Note that the main loop code is made shorter here
                //  You may want to that loop more like this.
                double increment = (Perlin.Noise(point) + offset) * exponent_array[i];
                value += remainder * increment * value;
            }

            return value;
        }

        //  Hybrid additive/multiplicative multifractal terrain model. 
        //  Some good parameter values to start with:                  
        //                                                             
        //  H = 0.25, offset = 0.7                                     
        public double HybridMultifractal(Vector3 point, double offset)
        {
            //  Get first octave of function
            double result = (Perlin.Noise(point) + offset) * exponent_array[0];
            double weight = result;

            //  Increase frequency
            point *= (float)lacunarity;

            //  Spectral construction inner loop, where the fractal is built
            int i;
            for(i = 1; i < octaves; i++)
            {
                //  prevent divergence
                if(weight > 1.0) weight = 1.0;

                //  Get next higher frequency
                double signal = (Perlin.Noise(point) + offset) * exponent_array[i];

                //  Add it in, weighted by previous freq's local value
                result += weight * signal;

                //  Update the (monotonically decreasing) weighting value
                //  (this is why H must specify a high fractal dimension)
                weight *= signal;

                //  Increase frequency
                point *= (float)lacunarity;
            }

            //  Take care of remainder in "octaves"
            double remainder = octaves - (int)octaves;
            if(remainder != 0.0)
            {
                //  "i" and spatial freq. are preset in loop above
                result += remainder * Perlin.Noise(point) * exponent_array[i];
            }

            return result;
        }

        //  Ridged multifractal terrain model.         
        //  Some good parameter values to start with:  
        //  H = 1.0, offset = 1.0, gain = 2.0          
        public double RidgedMultifractal(Vector3 point, double offset, double gain)
        {
            //  Get first octave
            double signal = Perlin.Noise(point);

            //  Get absolute value of signal (this creates the ridges)
            if(signal < 0.0)
            {
                signal = -signal;
            }

            //  Invert and translate (note that "offset" should be ~= 1.0)
            signal = offset - signal;

            //  Square the signal, to increase "sharpness" of ridges
            signal *= signal;

            //  Assign initial values
            double result = signal;
            double weight = 1.0;

            int i;
            for(i = 1; i < octaves; i++)
            {
                //  Increase the frequency
                point *= (float)lacunarity;

                //  Weight successive contributions by previous signal
                weight = signal * gain;
                if(weight > 1.0) weight = 1.0;
                if(weight < 0.0) weight = 0.0;
                signal = Perlin.Noise(point);
                if(signal < 0.0) signal = -signal;
                signal = offset - signal;
                signal *= signal;

                //  Weight the contribution
                signal *= weight;
                result += signal * exponent_array[i];
            }

            return result;
        }
    }

    //  Simple interface for classes below
    public interface INoiseGenerator
    {
        float Frequency
        {
            get;
            set;
        }

        float Amplitude
        {
            get;
            set;
        }

        float Lacunarity
        {
            get;
            set;
        }

        float Persistence
        {
            get;
            set;
        }

        int Octaves
        {
            get;
            set;
        }

        float Generate(Vector2 position);

        float Generate(Vector3 position);
    }

    public abstract class BaseNoiseGenerator
    {
        public float    Frequency   = 1.0f;
        public float    Amplitude   = 1.0f;
        public float    Lacunarity  = 2.0f;
        public float    Persistence = 0.5f;
        public int      Octaves     = 8;
        //public abstract float Generate(Vector2 position);
        public abstract float Generate(Vector3 position);
    }

    public class NoiseGenerator : BaseNoiseGenerator
    {
        public NoiseGenerator(float frequency, float amplitude, float lacunarity, float persistence, int octaves)
        {
            Frequency = frequency;
            Amplitude = amplitude;
            Lacunarity = lacunarity;
            Persistence = persistence;
            Octaves = octaves;
        }
        public override float Generate(Vector3 position)
        {
            float result = 0.0f;
            float amplitude = Amplitude;
            float frequency = Frequency;
            for(int i = 0; i < Octaves; ++i)
            {
                result    += (float)Perlin.Noise(position * frequency) * amplitude ;
                //result    += (float)WebGLNoise.Noise(position * frequency) * amplitude ;
                amplitude *= Persistence;
                frequency *= Lacunarity;
            }

            return result;
        }
    }

#if false
    public class FractalBrownianMotion : BaseNoiseGenerator
    {
        private Musgrave musgrave;

        public FractalBrownianMotion(double H, double lacunarity, double octaves)
        {
            musgrave = new Musgrave(H, lacunarity, octaves);
        }
        public override float Generate(Vector3 point)
        {
            return (float)musgrave.FBm(point);
        }
    }
    public class HybridMultifractal
    {
        private Musgrave    musgrave;
        private double      offset;

        public HybridMultifractal(double H, double lacunarity, double octaves, double offset)
        {
            musgrave = new Musgrave(H, lacunarity, octaves);
            this.offset = offset;
        }
        public double Generate(Vector3 point)
        {
            return musgrave.HybridMultifractal(point, offset);
        }
    }
    public class HeteroTerrain
    {
        private Musgrave    musgrave;
        private double      offset;

        public HeteroTerrain(double H, double lacunarity, double octaves, double offset)
        {
            musgrave = new Musgrave(H, lacunarity, octaves);
            this.offset = offset;
        }
        public double Evaluate(Vector3 point)
        {
            return musgrave.HeteroTerrain(point, offset);
        }
    }
    public class RidgedMultiFractal
    {
        private Musgrave    musgrave;
        private double      offset;
        private double      gain;

        public RidgedMultiFractal(double H, double lacunarity, double octaves, double offset, double gain)
        {
            musgrave = new Musgrave(H, lacunarity, octaves);
            this.offset = offset;
            this.gain = gain;
        }
        public double Evaluate(Vector3 point)
        {
            return musgrave.RidgedMultifractal(point, offset, gain);
        }
    }
#endif
}


