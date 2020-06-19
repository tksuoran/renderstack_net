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
    public enum TextureAxis
    {
        X,
        Y,
        Z
    }
    public class LWTextureStack
    {
        private Dictionary<string, LWTexture> layers = new Dictionary<string,LWTexture>();
        public Dictionary<string, LWTexture> Layers { get { return layers; } }
    }
    public class LWGradientKey
    {
        public FP4      Key;
        public Vector4  Value;

        public LWGradientKey(FP4 key, Vector4 value)
        {
            Key = key;
            Value = value;
        }
    }
    public class LWTexture
    {
        //  Header values
        public ID           BlockType;                  //  IMAP / PROC / GRAD / SHDR
        public S0           Ordinal;

        public ID           TextureChannel;             //  CHAN
        public U2           Enable;                     //  ENAB
        public U2           OpacityType;                //  OPAC
        public FP4Envelope  Opacity = new FP4Envelope();//  OPAC
        public U2           DisplacementAxis;           //  AXIS

        //  Texture maps
        public VecEnvelope  TextureCenter = new VecEnvelope();              //  CNTR | TCTR
        public VecEnvelope  TextureSize = new VecEnvelope();                //  SIZE | TSIZ
        public VecEnvelope  TextureRotation = new VecEnvelope();            //  ROTA
        public S0           TextureReferenceObject;     //  OREF
        public U2           TextureFalloffType;         //  FALL
        public VecEnvelope  TextureFalloff = new VecEnvelope();             //  FALL
        public U2           TextureCoordinateSystem;    //  CSYS

        //  Image maps
        public U2           ProjectionMode;             //  PROJ | CTEX
        public U2           MajorAxis;                  //  AXIS
        public VX           ImageMap;                   //  IMAG
        public U2           WidthWrap;                  //  WRAP
        public U2           HeightWrap;                 //  WRAP
        public FP4Envelope  WrapWidthCycles = new FP4Envelope();            //  WRPW
        public FP4Envelope  WrapHeightCycles = new FP4Envelope();           //  WRPH
        public S0           UVVertexMap;                //  VMAP
        public U2           AntialiasingType;           //  AAST
        public FP4          AntialiasingStrength;       //  AAST
        public U2           PixelBlending;              //  PIXB
        public VX           Stack;                      //  STCK
        public FP4Envelope  Amplitude = new FP4Envelope();                  //  TAMP
        public U2           Negative;                   //  NEGA

        //  Procedurals and Shaders --> m_function
        // S0       procedural_algorithm;
        // S0       m_shader_algorithm;
        public S0           ProceduralFunction;         //  PROC
        public byte[]       ProceduralData;
        public FP4[]        ProceduralBasicValue = new FP4[4];

        public S0           ShaderFunction;             //  SHDR
        public byte[]       ShaderData;

        //  UV animation handler
        public U1           UVAnimUnknown1;              // AUVN
        public U2           UVAnimUnknown2;              // AUVN
        public U1           UVAnimUnknown3;              // AUVU
        public S0           UVAnimPluginName;            // AUVN
        public S0           UVAnimPluginUser;            // AUVU
        public int          UVAnimPluginDataLength;      // AUVO
        public U1[]         UVAnimPluginData;            // AUVO  in big endian format!

        //  Gradients
        public S0           GradientParameter;           //  PNAM
        public S0           GradientItem;                //  INAM
        public FP4          GradientRangeStart;          //  GTST
        public FP4          GradientRangeEnd;            //  GREN
        public U2           GradientRepeat;              //  GRPT
        private List<LWGradientKey> gradientKeys = new List<LWGradientKey>();
        public List<LWGradientKey>  GradientKeys { get { return gradientKeys; } }
        public U2           GradientInterpolation;       //  IKEY

        public LWTexture()
        {
            TextureSize.Value = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }

    public partial class LWModelParser
    {
        // texture_pixel_blending != 0 --> linear mag filter, otherwise nearest
        // texture_antialiasing_type != 0 --> linear min filter, otherwise linear mipmap linear

        /*
        void LWTexture::processTexture(){
            ID4 chunk_type   = f.read_ID4();
            U2  chunk_length = f.read_U2();

            f.pushDomain( chunk_length );

            switch( chunk_type ){
                case ID.TFLG: readTextureFlags_U2          (); break;
                case ID.TSIZ: readTextureSize_VEC12        (); break;
                case ID.TCTR: readTextureCenter_VEC12      (); break;
                case ID.TFAL: readTextureFallOff_VEC12     (); break;
                case ID.TVEL: readTextureVelocity_VEC12    (); break;
                case ID.TREF: readTextureReferenceObject_S0(); break;
                case ID.TCLR: readTextureColor_COL4        (); break;
                case ID.TVAL: readTextureValue_IP2         (); break;
                case ID.TAMP: readBumpTextureAmplitude_FP4 (); break;
                case ID.TFP : readTextureAlgorithm_F4      (); break;  // ns not yet handled
                case ID.TIP : readTextureAlgorithm_I2      (); break;
                case ID.TSP : readTextureAlgorithm_F4      (); break;  // obsolete
                case ID.TFRQ: readTextureAlgorithm_I2      (); break;  // obsolete
                case ID.TIMG: readImageMap_FNAM0           (); break;
                case ID.TALP: readImageAlpha_FNAM0         (); break;
                case ID.TWRP: readImageWarpOptions_U2_U2   (); break;
                case ID.TAAS: readAntialiasingStrength_FP4 (); break;
                case ID.TOPC: readTextureOpacity_FP4       (); break;
                default: break;
            }

            f.popDomain( true );

        } */

        public void readTextureFlags_U2()
        {
#if false
            U2  flags = f.ReadU2();

            if((flags & LW_TF_AXIS_X) == LW_TF_AXIS_X)
            {
                texture_major_axis = TextureAxis.X;
            }

            if((flags & LW_TF_AXIS_Y) == LW_TF_AXIS_Y)
            {
                texture_major_axis = TextureAxis.Y;
            }
            if((flags & LW_TF_AXIS_Z) == LW_TF_AXIS_Z)
            {
                texture_major_axis = TextureAxis.Z;
            }

            //if( flags & LW_TF_WORLD_COORDINATES ){}
            //if( flags & LW_TF_NEGATIVE_IMAGE ){}
            if((flags & LW_TF_PIXEL_BLENDING) == LW_TF_PIXEL_BLENDING)
            {
                texture_antialiasing_type = 1;
            }
            if((flags & LW_TF_ANTIALISING) == LW_TF_ANTIALISING)
            {
                texture_pixel_blending = 1;
            }
#endif
        }

        public void readTextureSize_VEC12()
        {
            //texture_size = f.ReadVEC12();
        }

        public void readTextureCenter_VEC12()
        {
            //texture_center = f.ReadVEC12();
        }

        public void readTextureFallOff_VEC12()
        {
        }

        public void readTextureVelocity_VEC12()
        {
        }

        public void readTextureReferenceObject_S0()
        {
        }

        public void readTextureColor_COL4()
        {
        }

        public void readTextureValue_IP2()
        {
        }

        public void readBumpTextureAmplitude_FP4()
        {
        }

        public void readTextureAlgorithm_F4()
        {
        }

        public void readTextureAlgorithm_I2()
        {
        }

        public void readImageMap_FNAM0()
        {
            //texture_image_map = f.ReadFNAM0();
        }

        public void readImageAlpha_FNAM0()
        {
        }

        public void readImageWarpOptions_U2_U2()
        {
        }

        public void readAntialiasingStrength_FP4()
        {
        }

        public void readTextureOpacity_FP4()
        {
        }

    }
}
