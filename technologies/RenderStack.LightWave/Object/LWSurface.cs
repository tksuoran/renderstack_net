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
using COL12 = RenderStack.Math.Vector3;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public class LWSurface
    {
        public string           Name;
        public string           Comment;
        public ColorEnvelope    BaseColor = new ColorEnvelope();
        public FP4Envelope      Luminosity = new FP4Envelope();
        public FP4Envelope      Diffuse = new FP4Envelope();
        public FP4Envelope      Specular = new FP4Envelope();
        public FloatEnvelope    Shininess = new FloatEnvelope();
        public FP4Envelope      Reflection = new FP4Envelope();
        public FP4Envelope      Transparency = new FP4Envelope();
        public FP4Envelope      Translucency = new FP4Envelope();           //  LWO2
        public FP4Envelope      SpecularGlossiness = new FP4Envelope();
        public FP4Envelope      DiffuseSharpness = new FP4Envelope();
        public FP4Envelope      BumpIntensity = new FP4Envelope();
        public long             ReflectionMode;
        public string           OldReflectionMapImage;
        public FloatEnvelope    RefractiveIndex = new FloatEnvelope();
        public float            EdgeTransparencyTreshold;
        public float            maxSmoothingAngle;
        public float            MaxSmoothingAngle;
        public Vector3          Border;
        public U2               PolygonSidedness;
        public U2               ReflectionOptions;
        public VX               ReflectionMapImage;
        public ANG4Envelope     ReflectionMapImageSeamAngle = new ANG4Envelope();
        public FP4Envelope      ColorHighlights = new FP4Envelope();
        public U2               TransparencyOptions;
        public VX               RefractionMapImage;
        public FP4Envelope      ColorFilter = new FP4Envelope();
        public FP4Envelope      AdditiveTransparency = new FP4Envelope();
        public U2               GlowType;
        public FloatEnvelope    GlowIntensity = new FloatEnvelope();
        public FloatEnvelope    GlowSize = new FloatEnvelope();
        public U2               RenderOutlineFlags;
        public FloatEnvelope    RenderOutlineSize = new FloatEnvelope();
        public ColorEnvelope    RenderOutlineColor = new ColorEnvelope();
        public U2               AlphaMode;
        public FP4              AlphaValue;
        public FP4Envelope      VertexColorMapIntensity = new FP4Envelope();
        public ID               VertexColorMapType;
        public S0               VertexColorMapName;

        public LWSurface Source
        {
            set
            {
                //  \todo Copy everything from source
            }
        }

        public static Vector3 lwdef = new Vector3(200.0f / 255.0f, 200.0f / 255.0f, 200.0f / 255.0f);
        public static Vector3 white = new Vector3(1.0f, 1.0f, 1.0f);

        private Dictionary<ID, LWTextureStack> textureStacks = new Dictionary<ID,LWTextureStack>();

        public Dictionary<ID, LWTextureStack> TextureStacks { get { return textureStacks; } }

        public LWSurface(string name)
        {
            Name = name;
            BaseColor.Value = white;
            Diffuse.Value = 1.0f;
            Shininess.Value = System.Math.Pow(2.0, (10.0 * 0.40) + 2.0);
            Border = white;
        }

        public LWTextureStack FindOrCreateTextureStack(ID id)
        {
            if(textureStacks.ContainsKey(id))
            {
                return textureStacks[id];
            }
            LWTextureStack stack = new LWTextureStack();
            textureStacks[id] = stack;
            return stack;
        }

        public void UpdateColors()
        {
            Vector3 emission    = Vector3.Zero;
            Vector3 ambient     = lwdef;
            Vector3 diffuse     = lwdef;
            Vector3 specular    = new Vector3(0.25f, 0.25f, 0.25f);

            emission  = BaseColor.Value * (float)Luminosity.Value;
            ambient   = BaseColor.Value * (float)Diffuse.Value;
            diffuse   = BaseColor.Value * (float)Diffuse.Value;
            specular  = white * (float)Specular.Value;
        }
    }

    public partial class LWModelParser
    {
        private static string[] LW_ImageMapStrings = {
            "Planar Image Map",
            "Cylindrical Image Map",
            "Spherical Image Map",
            "Cubic Image Map",
        };

        /*  #define LW_PLANAR_IMAGE_MAP         1
            #define LW_CYLINDRICAL_IMAGE_MAP    2
            #define LW_SPHERICAL_IMAGE_MAP      3
            #define LW_CUBIC_IMAGE_MAP          4   */

        static int MapType(string s)
        {
            int type = 0;

            if(s == "Planar Image Map"      ) return Constants.LW_PLANAR_IMAGE_MAP;
            if(s == "Cylindrical Image Map" ) return Constants.LW_CYLINDRICAL_IMAGE_MAP;
            if(s == "Spherical Image Map"   ) return Constants.LW_SPHERICAL_IMAGE_MAP;
            if(s == "Cubic Image Map"       ) return Constants.LW_CUBIC_IMAGE_MAP;

            return type;
        }

        /*  LWOB SRFS { surf-name[S0] * } 
            
            This chunk contains a list of the names of all the surfaces
            in an object. Each surface name appears as a null-terminated 
            character string. If the length of the string (including the 
            null) is odd, an extra null byte is added. Surface names
            should be read from the file until as many bytes as the 
            chunk size specifies have been read. 
            
            In LightWave 3D terminology, a "surface" is defined as a
            named set of shading attributes. Each polygon contains a 
            reference to the surface used to color the polygon. The 
            names as listed in the SRFS chunk are numbered in the 
            order they occur, starting from 1, and this index is used 
            by polygons to define their surface. The SRFS chunk must 
            be before the POLS, CRVS, and PCHS chunks in the file.
        */
        void readSurfaceList()
        {
            while(f.Left() > 0)
            {
                string name = f.ReadS0();
                Debug.WriteLine("Surface " + name);

                currentSurface = model.MakeSurface(name);
            }
        }

        /*  LWOB SURF { name[S0], attributes[SUB-CHUNK] * } 
            
            Each SURF chunk describes the surface attributes of a particular surface. 
            These chunks begin with the name of the surface being described. Following 
            the name is a series of sub-chunks, which are like normal IFF chunks except 
            that their sizes are specified by short integers instead of longs. It is 
            likely that the variety of sub-chunks will grow as new surface attributes 
            are added to the program, but any unknown sub-chunks may be skipped over by 
            using the size. Sub-chunks should be read from the file until as many bytes 
            as the chunk size specifies have been read. 
        */
        void readSurface_sc()
        {
            var name = f.ReadS0();

            currentSurface = model.FindOrCreateSurface(name);

            while(f.Left() > 0)
            {
                SURFchunk();
            }
            endSurface();
        }

        /*  LWO2 Surface Definition 

            SURF { name[S0], source[S0], attributes[SUB-CHUNK] * } 

            Describes the shading attributes of a surface. The name uniquely
            identifies the surface. This is the string that's stored in TAGS
            and referenced by tag index in PTAG. If the source name is non-null,
            then this surface is derived from, or composed with, the source surface.
            The base attributes of the source surface can be overridden by this
            surface, and texture blocks can be added to the source surface. The
            material attributes follow as a variable list of subchunks documented
            below in the Surface Subchunks section.        
        */
        void surf_S0_S0_sc()
        {
            var name        = f.ReadS0();
            var source_name = f.ReadS0();

            currentSurface = model.FindOrCreateSurface(name);

            if(
                (source_name != null) && 
                (source_name.Length > 0)
            )
            {
                currentSurface.Source = model.Surfaces[source_name];
            }

            while(f.Left() > 0)
            {
                SURFchunk();
            }
            endSurface();
        }

        public void SURFchunk()
        {
            var subchunkId     = f.ReadID4();
            var subchunkLength = f.ReadU2 ();

            Debug.WriteLine(f.Type.ToString() + "::" + subchunkId.ToString() + " " + subchunkLength + " bytes");

            f.Push(subchunkLength);
            switch(f.Type.value)
            {
                case ID.LWOB:   handleLWOB(subchunkId); break;
                case ID.LWO2:   handleLWO2(subchunkId); break;
                default:        throw new System.Exception("Unrecogniced LWO file version " + f.Type.ToString());
            }
            f.Pop(true);
        }

        //  Some chunks have different handlers for LWOB / LWLO / LWO2 
        void handleLWOB(ID subchunkId)
        {
            switch(subchunkId.value)
            {
                case ID.FLAG: readStateFlags_U2                   (); break;
                case ID.COLR: readBaseColor_COL4                  (); break;
                case ID.LUMI: readLuminosity_IP2                  (); break;
                case ID.DIFF: readDiffuse_IP2                     (); break;
                case ID.SPEC: readSpecular_IP2_4                  (); break;
                case ID.REFL: readReflection_IP2_4                (); break;
                case ID.TRAN: readTransparency_IP2                (); break;
                case ID.TRNL: readTranslucency_FP4_VX             (); break;
                case ID.VLUM: readLuminosity_FP4                  (); break;
                case ID.VDIF: readDiffuse_FP4                     (); break;
                case ID.VSPC: readSpecular_FP4                    (); break;
                case ID.VRFL: readReflection_FP4                  (); break;
                case ID.VTRN: readTransparency_FP4                (); break;
                case ID.GLOS: readSpecularGlossiness_I2_4         (); break;
                case ID.RFLT: readReflectionMode_U2               (); break;
                case ID.RIMG: readReflectionMapImage_FNAM0        (); break;
                case ID.RSAN: readReflectionMapImageSeamAngle_DEG4(); break;
                case ID.RIND: readRefractiveIndex_F4              (); break;
                case ID.EDGE: readEdgeTransparencyTreshold_F4     (); break;
                case ID.SMAN: readMaxSmoothingAngle_DEG4          (); break;
                case ID.ALPH: readAlphaMode_U2_U2                 (); break;
                case ID.SHDR: readShaderPlugin_S0                 (); break;
                case ID.SDAT: readShaderData_f                    (); break;
                case ID.IMSQ: readSequenceOptions_U2_U2_U2        (); break;
                case ID.FLYR: readFlyerClipOptions_U4_U4          (); break;
                case ID.IMCC: readColorCycleOptions_U2_U2_U2      (); break;
                case ID.CTEX: readColorTexture_S0                 (); break;
                case ID.DTEX: readDiffuseTexture_S0               (); break;
                case ID.STEX: readSpecularTexture_S0              (); break;
                case ID.RTEX: readReflectionTexture_S0            (); break;
                case ID.TTEX: readTransparencyTexture_S0          (); break;
                case ID.LTEX: readLuminosityTexture_S0            (); break;
                case ID.BTEX: readBumpTexture_S0                  (); break;
                //  Texture parameters are handled by current tesxture

                default:
                {
                    if(currentTexture != null)
                    {
                        handleTextureChunks(subchunkId);
                    }
                    else
                    {
                        Trace.TraceWarning("There is no texture where to bind found texture subchunk");
                    }
                    break;
                }
            }
        }

        void handleLWO2(ID subchunkId)
        {
            switch(subchunkId.value)
            {
                case ID.COLR: readBaseColor_COL12_VX                 (); break;
                case ID.LUMI: readLuminosity_FP4_VX                  (); break;
                case ID.DIFF: readDiffuse_FP4_VX                     (); break;
                case ID.SPEC: readSpecular_FP4_VX                    (); break;
                case ID.REFL: readReflection_FP4_VX                  (); break;
                case ID.TRAN: readTransparency_FP4_VX                (); break;
                case ID.TRNL: readTranslucency_FP4_VX                (); break;
                case ID.GLOS: readSpecularGlossiness_FP4_VX          (); break;
                case ID.SHRP: readDiffuseSharpness_FP4_VX            (); break;
                case ID.BUMP: readBumpIntensity_FP4_VX               (); break;
                case ID.SIDE: readPolygonSidedness_U2                (); break;
                case ID.SMAN: readMaxSmoothingAngle_ANG4             (); break;
                case ID.RFOP: readReflectionOptions_U2               (); break;
                case ID.RIMG: readReflectionMapImage_VX              (); break;
                case ID.RSAN: readReflectionMapImageSeamAngle_ANG4_VX(); break;
                case ID.RIND: readRefractiveIndex_F4_VX              (); break;
                case ID.CLRH: readColorHighlights_FP4_VX             (); break;
                case ID.TROP: readTransparencyOptions_U2             (); break;
                case ID.TIMG: readRefractionMapImage_VX              (); break;
                case ID.CLRF: readColorFilter_FP4_VX                 (); break;
                case ID.ADTR: readAdditiveTransparency_FP4_VX        (); break;
                case ID.GLOW: readGlowEffect_U2_F4_VX_F4_VX          (); break;
                case ID.LINE: readRenderOutlines_U2_F4_VX_COL12_VX   (); break;
                case ID.ALPH: readAlphaMode_U2_FP4                   (); break;
                case ID.VCOL: readVertexColorMap_FP4_VX_ID4          (); break;
                case ID.BLOK: BLOKchunks                             (); break;
                case ID.CMNT: readComment_S0                         (); break;
                //    Texture parameters are handled by current texture

                default:
                {
                    if(currentTexture != null)
                    {
                        handleTextureChunks(subchunkId);
                    }
                    else
                    {
                        Trace.TraceWarning("There is no texture where to bind found texture subchunk");
                    }
                    break;
                }
            }
        }

        void handleTextureChunks(ID subchunkId)
        {
            switch(subchunkId.value)
            {
                case ID.TFLG: readTextureFlags_U2          (); break;
                case ID.TSIZ: readTextureSize_VEC12        (); break;
                case ID.TCTR: readTextureCenter_VEC12      (); break;
                case ID.TFAL: readTextureFallOff_VEC12     (); break;
                case ID.TVEL: readTextureVelocity_VEC12    (); break;
                case ID.TREF: readTextureReferenceObject_S0(); break;
                case ID.TCLR: readTextureColor_COL4        (); break;
                case ID.TVAL: readTextureValue_IP2         (); break;
                case ID.TAMP: readBumpTextureAmplitude_FP4 (); break;
                case ID.TFP : readTextureAlgorithm_F4      (); break;
                case ID.TIP : readTextureAlgorithm_I2      (); break;
                case ID.TSP : readTextureAlgorithm_F4      (); break;  // obsolete
                case ID.TFRQ: readTextureAlgorithm_I2      (); break;  // obsolete
                case ID.TIMG: readImageMap_FNAM0           (); break;
                case ID.TALP: readImageAlpha_FNAM0         (); break;
                case ID.TWRP: readImageWarpOptions_U2_U2   (); break;
                case ID.TAAS: readAntialiasingStrength_FP4 (); break;
                case ID.TOPC: readTextureOpacity_FP4       (); break;
                default:
                {
                    Trace.TraceWarning("Handler not implemented");
                    break;
                }
            }
        }

        void endSurface()
        {
            endTexture();
        }

        void endTexture()
        {
            currentTexture = null;
        }

        /*  LWOB

            COLR { base-color[COL4] } 

            This defines the base color of the surface, which is the color that lies 
            under all the other texturing attributes.
        */
        void readBaseColor_COL4()
        {
            U1 red   = f.ReadU1();
            U1 green = f.ReadU1();
            U1 blue  = f.ReadU1();
            f.ReadU1();

            currentSurface.BaseColor.Value = new Vector3(red / 255.0f, green / 255.0f, blue / 255.0f);
            currentSurface.UpdateColors();
        }

        void readStateFlags_U2()
        {
            U2  flags = f.ReadU2();
        }

        /*  LWOB, LWLO, LWO2

            LUMI, DIFF, SPEC, REFL, TRAN { percentage[IP2] }
            VLUM, VDIF, VSPC, VRFL, VTRN { percentage[FP4] } 

            These sub-chunks specify the base level of the surface's luminosity, 
            diffuse, specular, reflection, or transparency settings. Each setting has a 
            fixed-point and a floating-point form, but if both are present the 
            floating-point form should take precedence. The fixed-point value should be 
            rounded to the nearest half percent. Even though the floating-point form is 
            prefered, the convention is to write both sub-chunks to a surface 
            description to support older parsers. If any of these sub-chunks are absent 
            for a surface, a value of zero is assumed. The LUMI or VLUM sub-chunk 
            overrides the Luminous bit of the FLAG sub-chunk. 
        */

        //  LWOB Luminosity
        void readLuminosity_IP2()
        {
            currentSurface.Luminosity.Value = f.ReadU2() / 255.0f;
            currentSurface.UpdateColors();
        }

        //  LWOB Luminosity
        void readLuminosity_FP4()
        {
            currentSurface.Luminosity.Value = f.ReadFP4();
            currentSurface.UpdateColors();
        }

        //  LWOB Diffuse
        void readDiffuse_IP2()
        {
            currentSurface.Diffuse.Value = (float)( f.ReadU2() / 255.0f );
            currentSurface.UpdateColors();
        }
 
        //  LWOB Diffuse
        void readDiffuse_FP4()
        {
            currentSurface.Diffuse.Value = f.ReadFP4();
            currentSurface.UpdateColors();
        }

        //  LWOB Specularity
        void readSpecular_IP2_4()
        {
            currentSurface.Specular.Value = (float)( f.ReadU2() / 255.0f );
            currentSurface.UpdateColors();
        }

        //  LWOB Specularity
        void readSpecular_FP4()
        {
            currentSurface.Specular.Value = f.ReadFP4();
            currentSurface.UpdateColors();
        }

        //  LWOB Specular glossiness
        void readSpecularGlossiness_I2_4()
        {
            ulong dl = f.Left();

            if(dl != 2)
            {
                //emsg( M_LWO, "Domain left %ld bytes, expecting 2", dl );
            }

            I2 int_shi = f.ReadI2();
            currentSurface.Shininess.Value = System.Math.Pow(2.0, (10.0 * int_shi) + 2.0);
        }

        void readReflection_IP2_4()
        {
            Trace.TraceWarning("readReflection_IP2_4() not yet implemented");
        }
        void readTransparency_IP2()
        {
            Trace.TraceWarning("readTransparency_IP2() not yet implemented");
        }
        void readReflection_FP4()
        {
            currentSurface.Reflection.Value = f.ReadFP4();
        }
        void readTransparency_FP4()
        {
            currentSurface.Transparency.Value = f.ReadFP4();
        }
        void readReflectionMode_U2()
        {
            Trace.TraceWarning("readReflectionMode_U2() not yet implemented");
        }
        void readReflectionMapImage_FNAM0()
        {
            Trace.TraceWarning("readReflectionMapImage_FNAM0() not yet implemented");
        }
        void readReflectionMapImageSeamAngle_DEG4()
        {
            Trace.TraceWarning("readReflectionMapImageSeamAngle_DEG4() not yet implemented");
        }
        void readRefractiveIndex_F4()
        {
            currentSurface.RefractiveIndex.Value = f.ReadF4();
        }
        void readEdgeTransparencyTreshold_F4()
        {
            Trace.TraceWarning("readEdgeTransparencyTreshold_F4() not yet implemented");
        }

        void readMaxSmoothingAngle_DEG4()
        {
            currentSurface.MaxSmoothingAngle = f.ReadANG4();
        }

        void readAlphaMode_U2_U2()
        {
            Trace.TraceWarning("readAlphaMode_U2_U2() not yet implemented");
        }
        void readShaderPlugin_S0()
        {
            Trace.TraceWarning("readShaderPlugin_S0() not yet implemented");
        }
        void readShaderData_f()
        {
            Trace.TraceWarning("readShaderData_f() not yet implemented");
        }
        void readSequenceOptions_U2_U2_U2()
        {
            Trace.TraceWarning("readSequenceOptions_U2_U2_U2() not yet implemented");
        }
        void readFlyerClipOptions_U4_U4()
        {
            Trace.TraceWarning("readFlyerClipOptions_U4_U4() not yet implemented");
        }
        void readColorCycleOptions_U2_U2_U2()
        {
            Trace.TraceWarning("readColorCycleOptions_U2_U2_U2() not yet implemented");
        }

        /*  LWOB Start Texture Definition

            CTEX, DTEX, STEX, RTEX, TTEX, LTEX, BTEX { texture-type[S0] } 

            The presence of one of these sub-chunks indicates that the current surface 
            has a color, diffuse, specular, reflection, transparency, luminosity, or 
            bump texture. The contents of the sub-chunk is a character string specifying 
            the texture type as shown on the control panel. Once one of these sub-chunks 
            is encountered within a SURF chunk, all subsequent texture-related 
            sub-chunks are considered to pertain to the current texture, until another 
            one of these texture starting sub-chunks is read. There may be any number of 
            textures for each parameter, and the textures are layered in the order they 
            are read. 

        */
        void readColorTexture_S0()
        {
            Trace.TraceWarning("readColorTexture_S0() not yet implemented");
            S0 color_texture = f.ReadS0();
            endTexture();
        }

        void readDiffuseTexture_S0()
        {
            Trace.TraceWarning("readDiffuseTexture_S0() not yet implemented");
            S0 diffuse_texture = f.ReadS0();
        }

        void readSpecularTexture_S0()
        {
            Trace.TraceWarning("readSpecularTexture_S0() not yet implemented");
            S0 specular_texture = f.ReadS0();
        }

        void readReflectionTexture_S0()
        {
            Trace.TraceWarning("readReflectionTexture_S0() not yet implemented");
            S0 reflection_texture = f.ReadS0();
        }

        void readTransparencyTexture_S0()
        {
            Trace.TraceWarning("readTransparencyTexture_S0() not yet implemented");
            S0 transparency_texture = f.ReadS0();
        }

        void readLuminosityTexture_S0()
        {
            Trace.TraceWarning("readLuminosityTexture_S0() not yet implemented");
            S0 luminosity_texture = f.ReadS0();
        }

        void readBumpTexture_S0()
        {
            Trace.TraceWarning("readBumpTexture_S0() not yet implemented");
            S0 bump_texture = f.ReadS0();
        }

        //  LWO2

        void readComment_S0()
        {
            currentSurface.Comment = f.ReadS0();
        }

        /*  LWO2 Base Color

            COLR { base-color[COL12], envelope[VX] } 

            This defines the base color of the surface, which is the color that
            lies under all the other texturing attributes. 
        */
        void readBaseColor_COL12_VX()
        {
            currentSurface.BaseColor.Value = f.ReadCOL12();
            currentSurface.BaseColor.Envelope = f.ReadVX();
            currentSurface.UpdateColors();
        }

        /*  LWO2 Base Shading Values

            DIFF, LUMI, SPEC, REFL, TRAN, TRNL { intensity[FP4], envelope[VX] } 

            These sub-chunks specify the base level of the surface's
            diffuse, luminosity, specular, reflection, transparency, or
            tranlucency settings. If any of these sub-chunks are absent
            for a surface, a value of zero is assumed. 
        */
        void readLuminosity_FP4_VX()
        {
            currentSurface.Luminosity.Value = f.ReadFP4();
            currentSurface.Luminosity.Envelope = f.ReadVX();
        }

        void readDiffuse_FP4_VX()
        {
            currentSurface.Diffuse.Value = f.ReadFP4();
            currentSurface.Diffuse.Envelope = f.ReadVX();
        }

        /*  LWO2 Specular Glossiness

            GLOS { glossiness[FP4], envelope[VX] }

            Glossiness controls the falloff of specular highlights.
            The intensity of a specular highlight is calculated as
            cos^n a, where a is the angle between the reflection and
            view vectors. The power n is the specular exponent. The
            GLOS chunk stores a glossiness g as a floating point
            fraction related to n by: n = 2(10g + 2). A glossiness
            of 20% (0.2) gives a specular exponent of 2^4, or 16,
            equivalent to the "Low" glossiness preset in versions of
            LightWave prior to 6.0. Likewise 40% is 64 or "Medium,"
            60% is 256 or "High," and 80% is 1024 or "Maximum." The
            GLOS subchunk is only meaningful when the specularity in
            SPEC is non-zero. If GLOS is missing, a value of 40% is
            assumed.
        */
        void readSpecular_FP4_VX()
        {
            currentSurface.Specular.Value = f.ReadFP4();
            currentSurface.Specular.Envelope = f.ReadVX();
        }

        void readReflection_FP4_VX()
        {
            currentSurface.Reflection.Value = f.ReadFP4();
            currentSurface.Reflection.Envelope  = f.ReadVX();
        }

        void readTransparency_FP4_VX()
        {
            currentSurface.Transparency.Value = f.ReadFP4();
            currentSurface.Transparency.Envelope = f.ReadVX();
        }

        void readTranslucency_FP4_VX()
        {
            currentSurface.Translucency.Value = f.ReadFP4();
            currentSurface.Translucency.Envelope = f.ReadVX();
        }

        /*  LWO2 Specular Glossiness 

            GLOS { glossiness[FP4], envelope[VX] } 

            Glossiness controls the falloff of specular highlights.
            The intensity of a specular highlight is calculated as
            cos^n a, where a is the angle between the reflection and
            view vectors. The power n is the specular exponent. The
            GLOS chunk stores a glossiness g as a floating point
            fraction related to n by: n = 2(10g + 2). A glossiness
            of 20% (0.2) gives a specular exponent of 2^4, or 16,
            equivalent to the "Low" glossiness preset in versions of
            LightWave prior to 6.0. Likewise 40% is 64 or "Medium,"
            60% is 256 or "High," and 80% is 1024 or "Maximum." The
            GLOS subchunk is only meaningful when the specularity in
            SPEC is non-zero. If GLOS is missing, a value of 40% is
            assumed. 
        */
        void readSpecularGlossiness_FP4_VX()
        {
            float shi = f.ReadFP4();
            currentSurface.Shininess.Value = System.Math.Pow(2.0, (10.0 * shi) + 2.0);
            currentSurface.Shininess.Envelope = f.ReadVX();
        }

        /*  LWO2 Diffuse Sharpness 

            SHRP { sharpness[FP4], envelope[VX] } 

            Diffuse sharpness models non-Lambertian surfaces. The
            sharpness refers to the transition from lit to unlit
            portions of the surface, where the difference in diffuse
            shading is most obvious. For a sharpness of 0.0, diffuse
            shading of a sphere produces a linear gradient. A sharpness
            of 50% (0.5) corresponds to the fixed "Sharp Terminator"
            switch in versions of LightWave prior to 6.0. It produces
            planet-like shading on a sphere, with a brightly lit day
            side and a rapid falloff near the day/night line (the
            terminator). 100% sharpness is more like the Moon, with no
            falloff until just before the terminator. 
        */
        void readDiffuseSharpness_FP4_VX()
        {
            currentSurface.DiffuseSharpness.Value = f.ReadFP4();
            currentSurface.DiffuseSharpness.Envelope = f.ReadVX();
        }

        /*  LWO2 Bump Intensity 

            BUMP { strength[FP4], envelope[VX] } 

            Bump strength scales the height of the bumps
            in the gradient calculation. Higher values have
            the effect of increasing the contrast of the
            bump shading. The default value is 1.0. 
        */
        void readBumpIntensity_FP4_VX()
        {
            currentSurface.BumpIntensity.Value = f.ReadFP4();
            currentSurface.BumpIntensity.Envelope = f.ReadVX();
        }

        /*  LWO2 Polygon Sidedness 

            SIDE { sidedness[U2] } 

            The sidedness of a polygon can be 1 for front-only, or 3 for
            front and back. This replaces the old "Double Sided" flag bit.
            If missing, single-sided polygons are assumed. 
        */
        void readPolygonSidedness_U2()
        {
            currentSurface.PolygonSidedness = f.ReadU2();
        }

        /*  LWO2 Max Smoothing Angle 

            SMAN { max-smoothing-angle[ANG4] } 

            The maximum angle between adjacent polygons that will
            be smooth shaded. Shading across edges at higher angles
            won't be interpolated (the polygons will appear to meet
            at a sharp seam). If this chunk is missing, or if the
            value is <= 0, then the polygons are not smoothed.
        */
        void readMaxSmoothingAngle_ANG4()
        {
            currentSurface.MaxSmoothingAngle = f.ReadANG4();
        }

        /*  LWO2 Reflection Options 

            RFOP { reflection-options[U2] } 

            Reflection options is a numeric code that describes
            how reflections are handled for this surface and is
            only meaningful if the reflectivity in REFL is non-zero.

            0 - Backdrop Only 

            Only the backdrop is reflected. 

            1 - Raytracing + Backdrop 

            Objects in the scene are reflected when raytracing
            is enabled. Rays that don't intercept an object are
            assigned the backdrop color. 

            2 - Spherical Map 

            If an image is provided in an RIMG subchunk, the image
            is reflected as if it were spherically wrapped around
            the scene. 

            3 - Raytracing + Spherical Map 

            Objects in the scene are reflected when raytracing is
            enabled. Rays that don't intercept an object are assigned
            a color from the image map. 

            If there is no RFOP subchunk, a value of 0 is assumed. 
        */
        void readReflectionOptions_U2()
        {
            currentSurface.ReflectionOptions = f.ReadU2();
        }

        /*  LWO2 Reflection Map Image 

            RIMG { image[VX] } 

            A surface reflects this image as if it were
            spherically wrapped around the scene. The
            RIMG is only used if the reflection options
            in RFOP are set to use an image and the
            reflectivity of the surface in REFL is non-zero.
            The image is the index of a CLIP chunk, or
            zero to indicate no image
        */
        void readReflectionMapImage_VX()
        {
            currentSurface.ReflectionMapImage = f.ReadVX();
        }

        /*  LWO2 Reflection Map Image Seam Angle 

            RSAN { seam-angle[ANG4], envelope[VX] } 

            This angle is the heading angle of the reflection map seam.
            If missing, a value of zero is assumed. 
        */
        void readReflectionMapImageSeamAngle_ANG4_VX()
        {
            currentSurface.ReflectionMapImageSeamAngle.Value = f.ReadANG4();
            currentSurface.ReflectionMapImageSeamAngle.Envelope = f.ReadVX();
        }

        /*  LWO2 Refractive Index 

            RIND { refractive-index[F4], envelope[VX] } 

            The surface's refractive index determines how much light rays bend
            when passing into or out of this material, and is defined as the
            ratio of the speed of light in a vacuum to the speed of light in
            the material. Since light is fastest in a vacuum, this value should
            therefore be greater than or equal to 1.0. 
        */
        void readRefractiveIndex_F4_VX()
        {
            currentSurface.RefractiveIndex.Value = f.ReadF4();
            currentSurface.RefractiveIndex.Envelope = f.ReadVX();
        }

        /*  LWO2 Color Highlights 

            CLRH { color-highlights[FP4], envelope[VX] } 

            The color highlight percentage determines how
            much the reflections on a surface, including
            specular highlights, are tinted by the color
            of the surface. This replaces the old discrete
            "Color Highlights" flag. 

            Specular highlights are ordinarily the color of
            the incident light. Color highlights models the
            behavior of dialectric and conducting materials,
            in which the color of the specular highlight tends
            to be closer to the color of the material. A higher
            color highlight value blends more of the surface
            color and less of the incident light color. 
        */
        void readColorHighlights_FP4_VX()
        {
            currentSurface.ColorHighlights.Value = f.ReadFP4();
            currentSurface.ColorHighlights.Envelope = f.ReadVX();
        }

        /*  LWO2 Transparency Options 

            TROP { transparency-options[U2] } 

            Transparency options are just the same as reflection options
            except that they describe how refraction is handled. Refraction
            can be any combination of background color, image or raytracing. 
        */
        void readTransparencyOptions_U2()
        {
            currentSurface.TransparencyOptions = f.ReadU2();
        }

        /*  LWO2 Refraction Map Image 

            TIMG { image[VX] } 

            The refraction image is wrapped around the scene and is used for
            refraction mapping if the TROP mode is set to use an image and
            the transparency of the surface is non-zero. This setting is the
            refractive analogue of RIMG. 
        */
        void readRefractionMapImage_VX()
        {
            currentSurface.RefractionMapImage = f.ReadVX();
        }

        /*  LWO2 Color Filter 

            CLRF { color-filter[FP4], envelope[VX] } 

            The color filter percentage determines how much rays passing through
            a transparent surface are tinted by the color of the surface. This
            replaces the old discrete "Color Filter" flag. 
        */
        void readColorFilter_FP4_VX()
        {
            currentSurface.ColorHighlights.Value = f.ReadFP4();
            currentSurface.ColorHighlights.Envelope = f.ReadVX();
        }

        /*  LWO2 Additive Transparency

            ADTR { additive[FP4], envelope[VX] }

            This percentage selects how "additive" transparency effect are.
            The default value of zero indicates that transparent surfaces
            fully attenuate the background color while a value of 100%
            indicates that that the background color is full strength under
            the transparent surface.
        */
        void readAdditiveTransparency_FP4_VX()
        {
            currentSurface.AdditiveTransparency.Value = f.ReadFP4();
            currentSurface.AdditiveTransparency.Envelope = f.ReadVX();
        }

        /*  LWO2 Glow Effect

            GLOW { type[U2], intensity[F4], intensity-envelope[VX], size[F4], size-envelope[VX] } 

            The glow effect causes a surface to spread and effect neighboring
            areas of the image.The type can be 0 for Hastings glow, and 1 for
            image convolution. The size and intensity modulate how large and
            how strong the effect is, but the exact units are unclear. 
        */
        void readGlowEffect_U2_F4_VX_F4_VX()
        {
            currentSurface.GlowType                 = f.ReadU2();
            currentSurface.GlowIntensity.Value      = f.ReadF4();
            currentSurface.GlowIntensity.Envelope   = f.ReadVX();
            currentSurface.GlowSize.Value           = f.ReadF4();
            currentSurface.GlowSize.Envelope        = f.ReadVX();
        }

        /*  LWO2 Render Outlines

            LINE { flags[U2], ( size[F4], size-envelope[VX], ( color[COL12], color-envelope[VX] )? )? }

            The line effect draws the surface as a wireframe
            of the polygon edges. Currently the only flag
            defined is an enable switch in the low bit. The
            size is the thickness of the lines in pixels,
            and the color, if not given, is the base color
            of the surface. Note that you may encounter LINE
            subchunks with no color information (these will
            have a subchunk length of 8 bytes) and possibly
            without size information (subchunk length 2)

        */
        void readRenderOutlines_U2_F4_VX_COL12_VX()
        {
            currentSurface.RenderOutlineFlags           = f.ReadU2();
            currentSurface.RenderOutlineSize.Value      = f.ReadF4();
            currentSurface.RenderOutlineSize.Envelope   = f.ReadVX();
            currentSurface.RenderOutlineColor.Value     = f.ReadCOL12();
            currentSurface.RenderOutlineColor.Envelope  = f.ReadVX();
        }

        /*  LWO2 Alpha Mode

            ALPH { mode[U2], value[FP4] } 

            This chunk defines the alpha channel output options for the surface.

            If mode is 0, this surface does not affect the Alpha channel at all
            when rendered.
            
            If mode is 1, the alpha channel will have a fixed value which is the second
            parameter in the chunk and should normally have a value between 0.0 and 1.0.
            
            If mode is 2, the alpha value is derived from surface opacity, which is the
            default if the ALPH chunk is missing.

            If mode is 3, the alpha value comes from shadow density.
        */
        void readAlphaMode_U2_FP4()
        {
            currentSurface.AlphaMode = f.ReadU2();
            currentSurface.AlphaValue = f.ReadFP4();
        }

        /*  VCOL Vertex Color Map

            VCOL { intensity[FP4], envelope[VX], vmap-type[ID4], name[S0] }

            The vertex color map subchunk identifies an RGB or RGBA
            VMAP that will be used to color the surface.
        */
        void readVertexColorMap_FP4_VX_ID4()
        {
            currentSurface.VertexColorMapIntensity.Value    = f.ReadFP4();
            currentSurface.VertexColorMapIntensity.Envelope = f.ReadVX();
            currentSurface.VertexColorMapType               = f.ReadID4(); // ID_RGB, ID_RGBA
            currentSurface.VertexColorMapName               = f.ReadS0();
        }
    }
}
