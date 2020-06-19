
using System.Collections.Generic;
using System.Diagnostics;

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
using COL12 = RenderStack.Math.Vector4;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public class LWClip
    {
        //  These are public for easy access from LWSurfaceBlok
        public FNAM0            StillImage;                     //  STIL
        public U1               ImageSequenceNumDigits;         //  ISEQ
        public U1               ImageSequenceFlags;             //  ISEQ
        public I2               ImageSequenceOffset;            //  ISEQ
        public I2               ImageSequenceLoopLength;        //  ISEQ
        public FNAM0            ImageSequencePrefix;            //  ISEQ
        public S0               ImageSequenceSuffix;            //  ISEQ
        public FNAM0            PluginAnimationFilename;        //  ANIM
        public S0               PluginAnimationServerName;      //  ANIM
        public U2               PluginAnimationFlags;           //  ANIM
        public I2               ColorCyclingStillImageLo;       //  STCC
        public I2               ColorCyclingStillImageHi;       //  STCC
        public FNAM0            ColorCyclingStillImageName;     //  STCC
        public FP4Envelope      ContrastModifier;               //  CONT
        public FP4Envelope      BrightnessModifier;             //  BRIT
        public FP4Envelope      SaturationModifier;             //  SATR
        public FP4Envelope      HueModifier;                    //  HUE
        public FloatEnvelope    GammaModifier;                  //  GAMM
        public U2               NegativeModifier;               //  NEGA
        public S0               PluginImageFilterServerName;    //  IFLT
        public U2               PluginImageFilterFlags;         //  IFLT
        public S0               PluginPixelFilterServerName;    //  PFLT
        public U2               PluginPixelFilterFlags;         //  PFLT
        public FP4              StartTime;
        public FP4              Duration;
        public FP4              FrameRate;
        public U2               ColorSpaceRGBFlags;
        public U2               ColorSpaceRGB;
        public FNAM0            ColorSpaceRGBName;
        public U2               ColorSpaceAFlags;
        public U2               ColorSpaceA;
        public FNAM0            ColorSpaceAName;
        public U2               ImageFiltering;
        public U2               ImageDithering;
    }

    public partial class LWModelParser
    {
        /*  LWO2 Image or Sequence Definition 

            CLIP { index[U4], attributes[SUB-CHUNK] * } 

            Each CLIP chunk defines a image which can be used for applying as a texture
            map in surfaces. The term 'clip' is used to describe these since they may be
            time-varying sequences or animations rather than just stills. The index
            identifies this clip uniquely and may be any non-zero value less than 0x1000000.
            The attributes which define the source imagery and modifiers follow as a variable
            list of sub-chunks
        */
        void clip_U4_sc()
        {
            U4 index = f.ReadU4();

            currentClip = model.MakeClip(index);

            while(f.Left() > 0)
            {
                CLIPchunk();
            }
        }

        void CLIPchunk()
        {
            var type   = f.ReadID4();
            var length = f.ReadU2();

            f.Push(length);

            switch(type.value)
            {
                case ID.STIL: readStillImage_FNAM0                  (); break;
                case ID.ISEQ: readImageSequence_U1_U1_I2_I2_FNAM0_S0(); break;
                case ID.ANIM: readAnimation_FNAM0_S0_U2_d           (); break;
                case ID.STCC: readStillColorCycle_I2_I2_FNAM0       (); break;
                case ID.TIME: readTime_FP4_FP4_FP4                  (); break;
                case ID.CLRS: readColorSpaceRGB_U2_U2_FNAM0         (); break;
                case ID.CLRA: readColorSpaceA_U2_U2_FNAM0           (); break;
                case ID.FILT: readImageFiltering_U2                 (); break;
                case ID.DITH: readImageDithering_U2                 (); break;
                case ID.CONT: readContrast_FP4_VX                   (); break;
                case ID.BRIT: readBrightness_FP4_VX                 (); break;
                case ID.SATR: readSaturation_FP4_VX                 (); break;
                case ID.HUE:  readHue_FP4_VX                        (); break;
                case ID.GAMM: readGamma_F4_VX                       (); break;
                case ID.NEGA: readNegative_U2                       (); break;
                case ID.IFLT: readImageFilter_S0_U2_d               (); break;
                case ID.PFLT: readPixelFilter_S0_U2_d               (); break;
            }
            f.Pop(true);
        }

        /*  Still Image 

            STIL { name[FNAM0] } 

            This source chunk describes a single still image. The image is
            referenced by a filename in neutral path format. 
        */
        void readStillImage_FNAM0()
        {
            currentClip.StillImage = f.ReadFNAM0();
        }

        /*  LWO2::CLIP  Image Sequence 

            ISEQ { num-digits[U1],  flags[U1],     offset[I2],
                   loop-length[I2], prefix[FNAM0], suffix[S0] } 

            This source chuck describes an image sequence, which is basically a filename
            with a number in the middle. The number of digits is the number of zeros used
            to encode the sequence number in the filename. The flags has bits for looping
            and interlace. Offset and loop-length define the frames in the sequence. The
            prefix and suffix are stuck before and after the frame number to make the
            filename for each frame, which is in neutral path format. 
        */
        void readImageSequence_U1_U1_I2_I2_FNAM0_S0()
        {
            currentClip.ImageSequenceNumDigits  = f.ReadU1();
            currentClip.ImageSequenceFlags      = f.ReadU1();
            currentClip.ImageSequenceOffset     = f.ReadI2();
            currentClip.ImageSequenceLoopLength = f.ReadI2();
            currentClip.ImageSequencePrefix     = f.ReadFNAM0();
            currentClip.ImageSequenceSuffix     = f.ReadS0();
        }

        /*  LWO2::CLIP  Plug-in Animation 

            ANIM { filename[FNAM0], server-name[S0], flags[U2], data[...] } 

            This chunk indicates that the source imagery comes from a plug-in animation
            loader. The loader is defined by the server name and its data which just
            follows as binary bytes.
        */
        void readAnimation_FNAM0_S0_U2_d()
        {
            currentClip.PluginAnimationFilename     = f.ReadFNAM0();
            currentClip.PluginAnimationServerName   = f.ReadS0();
            currentClip.PluginAnimationFlags        = f.ReadU2();

            while(f.Left() > 0)
            {
                U1 data = f.ReadU1();
                //  \todo
            }
        }

        /*  LWO2::CLIP    Color-cycling Still 

            STCC { lo[I2], hi[I2], name[FNAM0] } 

            A still image with color-cycling is a source defined by a neutral-format
            name and cycling parameters. If lo is less than hi, the colors cycle
            forward, and if hi is less than lo, they go backwards. 
        */
        void readStillColorCycle_I2_I2_FNAM0()
        {
            currentClip.ColorCyclingStillImageLo   = f.ReadI2();
            currentClip.ColorCyclingStillImageHi   = f.ReadI2();
            currentClip.ColorCyclingStillImageName = f.ReadFNAM0();
        }

        /*  Time

            TIME { start-time[FP4], duration[FP4], frame-rate[FP4] }

            Defines source times for an animated clip.
        */
        void readTime_FP4_FP4_FP4()
        {
            currentClip.StartTime = f.ReadFP4();
            currentClip.Duration = f.ReadFP4();
            currentClip.FrameRate = f.ReadFP4();
        }

        /*  Color Space RGB

            CLRS { flags[U2], colorspace[U2], filename[FNAM0] }

            Contains the color space of the texture. If the flag is 0, 
            then the color space is contained in the following 2 bytes. 
            That color space is defined by the LWCOLORSPACE enum. If the
            flag is set to 1, then the file name of the color space is
            save as a local string.
        */
        void readColorSpaceRGB_U2_U2_FNAM0()
        {
            currentClip.ColorSpaceRGBFlags = f.ReadU2();
            currentClip.ColorSpaceRGB = f.ReadU2();
            currentClip.ColorSpaceRGBName = f.ReadFNAM0();
        }

        /*
            Color Space Alpha

            CLRA { flags[U2], colorspace[U2], filename[FNAM0] }

            Contains the color space of the texture alpha. If the flag is 0, 
            then the color space is contained in the following 2 bytes.
            That color space is defined by the LWCOLORSPACE enum. If the flag
            is set to 1, then the file name of the color space is save as a
            local string.
        */
        void readColorSpaceA_U2_U2_FNAM0()
        {
            currentClip.ColorSpaceAFlags = f.ReadU2();
            currentClip.ColorSpaceA = f.ReadU2();
            currentClip.ColorSpaceAName = f.ReadFNAM0();
        }

        /*  Image Filtering

            FILT { flags[U2] }

            Contains the index to the current image filtering.
        */
        void readImageFiltering_U2()
        {
            currentClip.ImageFiltering = f.ReadU2();
        }

        /*  Image Dithering

            DITH { flags[U2] }

            Contains the index to the current image dithering.
        */
        void readImageDithering_U2()
        {
            currentClip.ImageDithering = f.ReadU2();
        }

        /*  LWO2::CLIP Contrast 

            CONT { contrast-delta[FP4], envelope[VX] } 

            RGB levels are altered in proportion to their distance
            from 0.5. Positive deltas move the levels toward one of
            the extremes (0.0 or 1.0), while negative deltas move
            them toward 0.5. The default is 0.
        */
        void readContrast_FP4_VX()
        {
            currentClip.ContrastModifier.Value = f.ReadFP4();
            currentClip.ContrastModifier.Envelope = f.ReadVX();
        }

        /*  LWO2::CLIP Brightness 

            BRIT { brightness-delta[FP4], envelope[VX] } 

            The delta is added to the RGB levels. The default is 0.
        */
        void readBrightness_FP4_VX()
        {
            currentClip.BrightnessModifier.Value = f.ReadFP4();
            currentClip.BrightnessModifier.Envelope = f.ReadVX();
        }

        /*  LWO2::CLIP Saturation 

            SATR { saturation-delta[FP4], envelope[VX] } 

            The saturation of an RGB color is defined as (max - min)/max, 
            where max and min are the maximum and minimum of the three RGB
            levels. This is a measure of the intensity or purity of a color.
            Positive deltas turn up the saturation by increasing the max
            component and decreasing the min one, and negative deltas have
            the opposite effect. The default is 0.
        */
        void readSaturation_FP4_VX()
        {
            currentClip.SaturationModifier.Value = f.ReadFP4();
            currentClip.SaturationModifier.Envelope = f.ReadVX();
        }

        /*  LWO2::CLIP Hue 

            HUE { hue-rotation[FP4], envelope[VX] } 

            The hue of an RGB color is an angle defined as

            r is max: 1/3 (g - b)/(r - min)
            g is max: 1/3 (b - r)/(g - min) + 1/3
            b is max: 1/3 (r - g)/(b - min) + 2/3

            with values shifted into the [0, 1] interval when necessary.
            The levels between 0 and 1 correspond to angles between 0 and
            360 degrees. The hue delta rotates the hue. The default is 0.
        */
        void readHue_FP4_VX()
        {
            currentClip.HueModifier.Value = f.ReadFP4();
            currentClip.HueModifier.Envelope = f.ReadVX();
        }

        /*  LWO2::CLIP Gamma Correction 

            GAMM { gamma[F4], envelope[VX] } 

            Gamma correction alters the distribution of light and dark
            in an image by raising the RGB levels to a small power.
            By convention, the gamma is stored as the inverse of this
            power. A gamma of 0.0 forces all RGB levels to 0.0.
            The default is 1.0.
        */
        void readGamma_F4_VX()
        {
            currentClip.GammaModifier.Value = f.ReadF4();
            currentClip.GammaModifier.Envelope = f.ReadVX();
        }

        /*  LWO2::CLIP    Negative 

            NEGA { enable[U2] } 

            If non-zero, the RGB values are inverted, (1.0 - r, 1.0 - g, 1.0 - b),
            to form a negative of the image.
        */
        void readNegative_U2()
        {
            currentClip.NegativeModifier = f.ReadU2();
        }

        /*  LWO2::CLIP Plug-in Image Filters 

            IFLT { server-name[S0], flags[U2], data[...] } 

            Plug-in image filters can be used to pre-filter an image
            before rendering. The filter has to be able to exist outside
            of the special environment of rendering in order to work here.
            Filters are given by a server name, an enable flag, and plug-in
            server data as raw bytes. 
        */
        void readImageFilter_S0_U2_d()
        {
            currentClip.PluginImageFilterServerName = f.ReadS0();
            currentClip.PluginImageFilterFlags = f.ReadU2();

            while(f.Left() > 0)
            {
                U1 data = f.ReadU1();
                //  \todo 
            }
        }

        /*  LWO2::CLIP Plug-in Pixel Filters 

            PFLT { server-name[S0], flags[U2], data[...] } 

            Pixel filters may also be used as clip modifiers, and they are
            stored and used in a way that is exactly like image filters above.
        */
        void readPixelFilter_S0_U2_d()
        {
            currentClip.PluginPixelFilterServerName = f.ReadS0();
            currentClip.PluginImageFilterFlags = f.ReadU2();

            while(f.Left() > 0)
            {
                U1 data = f.ReadU1();
                //  \todo
            }
        }
    }
}
