using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using OpenTK.Graphics.OpenGL;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public enum TextureEnvModeRS : int
    {
        Replace = ((int)0x1E01),
        Modulate = ((int)0x2100),
        Decal = ((int)0x2101),
        Blend = ((int)0x0BE2),
        Add = ((int)0x0104),
        Combine = ((int)0x8570),
        Combine4Nv = ((int)0x8503),
    }

    //  MODULATE_ADD_ATI        Arg0 * Arg2 + Arg1
    //  MODULATE_SIGNED_ADD_ATI Arg0 * Arg2 + Arg1 - 0.5
    //  MODULATE_SUBTRACT_ATI   Arg0 * Arg2 - Arg1
    public enum TextureEnvModeCombineRS : int
    {
        Add = ((int)0x0104),
        Replace = ((int)0x1E01),
        Modulate = ((int)0x2100),
        Subtract = ((int)0x84E7),
        AddSigned = ((int)0x8574),
        Interpolate = ((int)0x8575),
        Dot3Rgb = ((int)0x86AE),
        Dot3Rgba = ((int)0x86AF),
        ModulateAddAti = ((int)0x8744),
        ModulateSignedAddAti = ((int)0x8745),
        ModulateSubtractAti = ((int)0x8746),
    }

    public enum TextureTargetRS : int
    {
        Disabled = ((int)0x0000),
        Texture1D = ((int)0x0DE0),
        Texture2D = ((int)0x0DE1),
        Texture3D = ((int)0x806F),
        TextureCubeMap = ((int)0x8513)
    }

    public enum FogCoordSrc : int
    {
        FogCoord = ((int)0x8451),
        FragmentDepth = ((int)0x8452),
    }

    /*public class Vector4
    {
        float X;
        float Y;
        float Z;
        float W;
    }*/

    public class FixedUniform
    {
        public string   UniformBlockName;
        public string   UniformName;
        public int      Stride;
        [JsonIgnore]
        public Floats   Floats;
        [JsonIgnore]
        public float[]  Value = new float[4];

        public FixedUniform(float x)
        {
            Value[0] = x;
        }
        public FixedUniform(float x, float y, float z, float w)
        {
            Value[0] = x;
            Value[1] = y;
            Value[2] = z;
            Value[3] = w;
        }
        public void Update(int instance)
        {
            if(
                (Floats == null) &&
                (UniformBlockName != null) &&
                (UniformName != null) &&
                (UniformBlockGL.Instances.ContainsKey(UniformBlockName))
            )
            {
                var unifornBlock    = UniformBlockGL.Instances[UniformBlockName];
                var uniformBuffer   = unifornBlock.UniformBuffer;
                if(uniformBuffer != null)
                {
                    var floats = uniformBuffer.Floats(UniformName);
                    for(int i = 0; i < floats.Elements; ++i)
                    {
                        Value[i] = floats.Value[i + instance * Stride];
                    }
                }
            }
            else
            {
                if(Floats != null)
                {
                    for(int i = 0; i < Floats.Elements; ++i)
                    {
                        Value[i] = Floats.Value[i + instance * Stride];
                    }
                }
            }
        }

        public void Bind(string block, string uniform)
        {
            UniformBlockName = block + "_block";
            UniformName = uniform;
            Stride = 0;
        }
        public void Bind(string block, string uniform, int stride)
        {
            UniformBlockName = block + "_block";
            UniformName = uniform;
            Stride = stride;
        }
        public void SetConstant(float x)
        {
            Floats = new Floats(x);
        }
        public void SetConstant(float x, float y, float z, float w)
        {
            Floats = new Floats(x, y, z, w);
        }
    }

    [Serializable]
    public class TexGen
    {
        public bool             TextureGen      = false;
        public FixedUniform     EyePlane        = new FixedUniform(1.0f, 0.0f, 0.0f, 0.0f);
        public FixedUniform     ObjectPlane     = new FixedUniform(1.0f, 0.0f, 0.0f, 0.0f);
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureGenMode   TextureGenMode  = TextureGenMode.EyeLinear;

        public unsafe void Apply(int texcoordIndex, int instance)
        {
            if(TextureGen)
            {
                GL.Enable(EnableCap.TextureGenS + texcoordIndex);
                EyePlane.Update(instance);
                GL.TexGen(TextureCoordName.S + texcoordIndex, TextureGenParameter.EyePlane, EyePlane.Value);
                ObjectPlane.Update(instance);
                GL.TexGen(TextureCoordName.S + texcoordIndex, TextureGenParameter.ObjectPlane, ObjectPlane.Value);
                GL.TexGen(TextureCoordName.S + texcoordIndex, TextureGenParameter.TextureGenMode, (int)TextureGenMode);
            }
            else
            {
                GL.Disable(EnableCap.TextureGenS + texcoordIndex);
            }
        }
    }

    [Serializable]
    public class TextureEnvironmentUnit
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureTargetRS              TextureTarget   = TextureTargetRS.Disabled;

        //  Table 6.27
        // ACTIVE TEXTURE
        // COORD_REPLACE   (this would be for point sprite)

        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeRS             TextureEnvMode  = TextureEnvModeRS.Modulate;
        public FixedUniform                 TextureEnvColor = new FixedUniform(0.0f, 0.0f, 0.0f, 0.0f);
        public FixedUniform                 TextureLodBias  = new FixedUniform(0.0f);
        public TexGen                       TextureGenS = new TexGen();
        public TexGen                       TextureGenT = new TexGen();
        public TexGen                       TextureGenR = new TexGen();
        public TexGen                       TextureGenQ = new TexGen();
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeCombineRS      CombineRGB      = TextureEnvModeCombineRS.Modulate;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeCombineRS      CombineAlpha    = TextureEnvModeCombineRS.Modulate;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeSource         Src0RGB         = TextureEnvModeSource.Texture;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeSource         Src1RGB         = TextureEnvModeSource.Previous;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeSource         Src2RGB         = TextureEnvModeSource.Constant;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeSource         Src0Alpha       = TextureEnvModeSource.Texture;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeSource         Src1Alpha       = TextureEnvModeSource.Previous;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeSource         Src2Alpha       = TextureEnvModeSource.Constant;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeOperandRgb     Operand0RGB     = TextureEnvModeOperandRgb.SrcColor;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeOperandRgb     Operand1RGB     = TextureEnvModeOperandRgb.SrcColor;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeOperandRgb     Operand2RGB     = TextureEnvModeOperandRgb.SrcColor;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeOperandAlpha   Operand0Alpha   = TextureEnvModeOperandAlpha.SrcAlpha;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeOperandAlpha   Operand1Alpha   = TextureEnvModeOperandAlpha.SrcAlpha;
        [JsonConverter(typeof(StringEnumConverter))]
        public TextureEnvModeOperandAlpha   Operand2Alpha   = TextureEnvModeOperandAlpha.SrcAlpha;
        public FixedUniform                 RGBScale        = new FixedUniform(1.0f);   // post combiner scaling
        public FixedUniform                 AlphaScale      = new FixedUniform(1.0f);   // post combiner scaling

        public unsafe void Apply(int instance)
        {
            bool enable1D = (TextureTarget == TextureTargetRS.Texture1D);
            bool enable2D = (TextureTarget == TextureTargetRS.Texture2D);
            bool enable3D = (TextureTarget == TextureTargetRS.Texture3D);
            bool enableCubeMap = (TextureTarget == TextureTargetRS.TextureCubeMap);

            if(enable1D)
            {
                GL.Enable(EnableCap.Texture1D);
            }
            else
            {
                GL.Disable(EnableCap.Texture1D);
            }

            if(enable2D)
            {
                GL.Enable(EnableCap.Texture2D);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
            }

            if(enable3D)
            {
                GL.Enable(EnableCap.Texture3DExt);
            }
            else
            {
                GL.Disable(EnableCap.Texture3DExt);
            }

            if(enableCubeMap)
            {
                GL.Enable(EnableCap.TextureCubeMap);
            }
            else
            {
                GL.Disable(EnableCap.TextureCubeMap);
            }

            
            if(
                (enable1D == false) && 
                (enable2D == false) && 
                (enable3D == false) && 
                (enableCubeMap == false))

            {
                /*
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,  (int)TextureEnvModeRS.Replace);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb,      (int)TextureEnvModeSource.Previous);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb,     (int)TextureEnvModeOperandRgb.SrcColor);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha,       (int)TextureEnvModeSource.Previous);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Alpha,   (int)TextureEnvModeOperandAlpha.SrcAlpha);
                */
                return;
            }

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode);
            TextureEnvColor.Update(instance);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, TextureEnvColor.Value);
            TextureLodBias.Update(instance);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureLodBias, TextureLodBias.Value);

            TextureGenS.Apply(0, instance);
            TextureGenT.Apply(1, instance);
            TextureGenR.Apply(2, instance);
            TextureGenQ.Apply(3, instance);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb,      (int)CombineRGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha,    (int)CombineAlpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb,      (int)Src0RGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb,         (int)Src1RGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src2Rgb,         (int)Src2RGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha,       (int)Src0Alpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Alpha,       (int)Src1Alpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src2Alpha,       (int)Src2Alpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb,     (int)Operand0RGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Rgb,     (int)Operand1RGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand2Rgb,     (int)Operand2RGB);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Alpha,   (int)Operand0Alpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Alpha,   (int)Operand1Alpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand2Alpha,   (int)Operand2Alpha);
            RGBScale.Update(instance);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.RgbScale, RGBScale.Value);
            AlphaScale.Update(instance);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.AlphaScale, AlphaScale.Value);
        }
    }

    [Serializable]
    public class TextureEnvironment
    {
        public TextureEnvironmentUnit[] TextureUnits;

        public TextureEnvironment()
        {
            TextureUnits = new TextureEnvironmentUnit[4];
            for(int i = 0; i < TextureUnits.Length; ++i)
            {
                TextureUnits[i] = new TextureEnvironmentUnit();
            }
        }

        public void Apply(int instance)
        {
            for(int i = 0; i < TextureUnits.Length; ++i)
            {
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + i);
                TextureUnits[i].Apply(instance);
            }
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
        }
    }

    [Serializable]
    public class Fog
    {
        public FixedUniform     FogColor;
        public FixedUniform     FogDensity;     /*  1.0 exponential fog density  */
        public FixedUniform     FogStart;       /*  0.0 linear fog start         */
        public FixedUniform     FogEnd;         /*  1.0 linear fog end           */
        [JsonConverter(typeof(StringEnumConverter))]
        public FogMode          FogMode     = FogMode.Exp;
        public bool             Enabled     = false; 
        [JsonConverter(typeof(StringEnumConverter))]
        public FogCoordSrc      FogCoordSrc = FogCoordSrc.FragmentDepth;

        public unsafe void Apply(int instance)
        {
            if(Enabled)
            {
                GL.Enable(EnableCap.Fog);
                FogColor.Update(instance);
                GL.Fog(FogParameter.FogColor, FogColor.Value);
                FogEnd.Update(instance);
                GL.Fog(FogParameter.FogEnd, FogEnd.Value);
                FogStart.Update(instance);
                GL.Fog(FogParameter.FogStart, FogStart.Value);
                GL.Fog(FogParameter.FogMode, (int)FogMode);
            }
            else
            {
                GL.Disable(EnableCap.Fog);
            }
        }
    }

    [Serializable]
    public class Lighting
    {
        public bool         Enable;
        [JsonConverter(typeof(StringEnumConverter))]
        public ShadingModel ShadeModel = ShadingModel.Flat;
    }

    [Serializable]
    public class FixedFunctionProgram
    {
        public bool                     UsePerInstance = false;
        public bool                     Lighting = false;
        public Fog                      Fog = new Fog();
        public bool                     ColorMaterial = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public ColorMaterialParameter   ColorMaterialParameter = ColorMaterialParameter.AmbientAndDiffuse;
        public TextureEnvironment       TextureEnvironment = new TextureEnvironment();

        public void Apply(int instance)
        {
            // \todo mostly not implemented
            if(Lighting)
            {
                GL.Enable(EnableCap.Lighting);
            }
            else
            {
                GL.Disable(EnableCap.Lighting);
            }
            Fog.Apply(instance);
            TextureEnvironment.Apply(instance);
        }
    }
}

/*
{
    "TextureUnits":
    [
        {
            "TextureTarget":"Texture2D",
            "TextureEnvMode":"Combine",
            "TextureEnvColor":[1.0,1.0,1.0,1.0],
            "TextureLodBias":0.0,
            "TextureGens":
            [
                {
                    "TextureGen":false,
                    "EyePlane":[1.0,0.0,0.0,0.0],
                    "ObjectPlane":[1.0,0.0,0.0,0.0],
                    "TextureGenMode":"EyeLinear"
                },
                {
                    "TextureGen":false,
                    "EyePlane":[1.0,0.0,0.0,0.0],
                    "ObjectPlane":[1.0,0.0,0.0,0.0],
                    "TextureGenMode":"EyeLinear"
                },
                {
                    "TextureGen":false,
                    "EyePlane":[1.0,0.0,0.0,0.0],
                    "ObjectPlane":[1.0,0.0,0.0,0.0],
                    "TextureGenMode":"EyeLinear"
                },
                {
                    "TextureGen":false,
                    "EyePlane":[1.0,0.0,0.0,0.0],
                    "ObjectPlane":[1.0,0.0,0.0,0.0],
                    "TextureGenMode":"EyeLinear"
                }
            ],
            "CombineRGB":"Replace",
            "CombineAlpha":"Replace",
            "Src0RGB":"Texture",
            "Src1RGB":"Previous",
            "Src2RGB":"Constant",
            "Src0Alpha":"Texture",
            "Src1Alpha":"Previous",
            "Src2Alpha":"Constant",
            "Operand0RGB":"SrcColor",
            "Operand1RGB":"SrcColor",
            "Operand2RGB":"SrcAlpha",
            "Operand0Alpha":"SrcAlpha",
            "Operand1Alpha":"SrcAlpha",
            "Operand2Alpha":"SrcAlpha",
            "RGBScale":1.0,
            "AlphaScale":1.0
        },
        {"TextureTarget":0,"TextureEnvMode":"Modulate","TextureEnvColor":[0.0,0.0,0.0,0.0],"TextureLodBias":0.0,"TextureGens":[{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"}],"CombineRGB":"Modulate","CombineAlpha":"Modulate","Src0RGB":"Texture","Src1RGB":"Previous","Src2RGB":"Constant","Src0Alpha":"Texture","Src1Alpha":"Previous","Src2Alpha":"Constant","Operand0RGB":"SrcColor","Operand1RGB":"SrcColor","Operand2RGB":"SrcAlpha","Operand0Alpha":"SrcAlpha","Operand1Alpha":"SrcAlpha","Operand2Alpha":"SrcAlpha","RGBScale":1.0,"AlphaScale":1.0},{"TextureTarget":0,"TextureEnvMode":"Modulate","TextureEnvColor":[0.0,0.0,0.0,0.0],"TextureLodBias":0.0,"TextureGens":[{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"}],"CombineRGB":"Modulate","CombineAlpha":"Modulate","Src0RGB":"Texture","Src1RGB":"Previous","Src2RGB":"Constant","Src0Alpha":"Texture","Src1Alpha":"Previous","Src2Alpha":"Constant","Operand0RGB":"SrcColor","Operand1RGB":"SrcColor","Operand2RGB":"SrcAlpha","Operand0Alpha":"SrcAlpha","Operand1Alpha":"SrcAlpha","Operand2Alpha":"SrcAlpha","RGBScale":1.0,"AlphaScale":1.0},{"TextureTarget":0,"TextureEnvMode":"Modulate","TextureEnvColor":[0.0,0.0,0.0,0.0],"TextureLodBias":0.0,"TextureGens":[{"Texture'Gen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"},{"TextureGen":false,"EyePlane":[1.0,0.0,0.0,0.0],"ObjectPlane":[1.0,0.0,0.0,0.0],"TextureGenMode":"EyeLinear"}],"CombineRGB":"Modulate","CombineAlpha":"Modulate","Src0RGB":"Texture","Src1RGB":"Previous","Src2RGB":"Constant","Src0Alpha":"Texture","Src1Alpha":"Previous","Src2Alpha":"Constant","Operand0RGB":"SrcColor","Operand1RGB":"SrcColor","Operand2RGB":"SrcAlpha","Operand0Alpha":"SrcAlpha","Operand1Alpha":"SrcAlpha","Operand2Alpha":"SrcAlpha","RGBScale":1.0,"AlphaScale":1.0}]}

*/