using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public class ProgramGL1 : IProgram
    {
        private string name;
        public string Name { get { return name; } set { name = value; } }

        public bool Valid { get { return true; } }

        private AttributeMappings       attributeMappings = AttributeMappings.Global;
        public AttributeMappings        AttributeMappings { get { return attributeMappings; } set { attributeMappings = value; } }

        private FixedFunctionProgram    fixedFunctionProgram = new FixedFunctionProgram();
        public FixedFunctionProgram     FixedFunctionProgram { get { return fixedFunctionProgram; } }

        public void Use(int baseInstance)
        {
            fixedFunctionProgram.Apply(baseInstance);
        }

        public static ProgramGL1 Load(string filename)
        {
#if !DEBUG
            try
#endif
            {
                var program = new ProgramGL1(filename);
                program.Name = filename;

#if true

                var dualTexture = AttributeMappings.Pool["dualtexture"];
                dualTexture.Add( 0, "_position",  VertexUsage.Position, 0,    3);
                dualTexture.Add( 1, "_texcoord",  VertexUsage.TexCoord, 0, VertexUsage.TexCoord, 0, 2);
                dualTexture.Add( 2, "_texcoord",  VertexUsage.TexCoord, 0, VertexUsage.TexCoord, 1, 2);
                dualTexture.Add( 3, "_color",     VertexUsage.Color,    0,    4);

                var idToColor = AttributeMappings.Pool["idToColor"];
                idToColor.Add(0, "_position",  VertexUsage.Position, 0, 3);
                idToColor.Add(1, "_id_vec3",   VertexUsage.Id,       0, VertexUsage.Color, 0, 3);

                if(filename == "Anachrome")
                {
                    //  \todo this requires GL_ATI_texture_env_combine3
                    //  Not available on i915
                    program.AttributeMappings = dualTexture;

                    //  Unit 0: left times red
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Modulate;
                    texture0.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.Texture;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src1RGB          = TextureEnvModeSource.Constant;
                    texture0.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Constant;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture0.TextureEnvColor.SetConstant(1.0f, 0.0f, 0.0f, 1.0f);

                    //  Unit 1: right times cyan plus previous
                    var texture1 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[1];
                    texture1.TextureTarget    = TextureTargetRS.Texture2D;
                    texture1.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture1.CombineRGB       = TextureEnvModeCombineRS.ModulateAddAti;
                    texture1.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture1.Src0RGB          = TextureEnvModeSource.Texture;
                    texture1.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src1RGB          = TextureEnvModeSource.Previous;
                    texture1.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src2RGB          = TextureEnvModeSource.Constant;
                    texture1.Operand2RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src0Alpha        = TextureEnvModeSource.Constant;
                    texture1.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture1.TextureEnvColor.SetConstant(0.0f, 1.0f, 1.0f, 1.0f);
                }
                if(filename == "Blend")
                {
                    program.AttributeMappings = dualTexture;

                    //  Unit 0: set to texture 0 color
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Replace;
                    texture0.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.Texture;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Texture;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;

                    //  Unit 1: interpolate previous and this texture
                    var texture1 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[1];
                    texture1.TextureTarget    = TextureTargetRS.Texture2D;
                    texture1.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture1.CombineRGB       = TextureEnvModeCombineRS.Interpolate;
                    texture1.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture1.Src0RGB          = TextureEnvModeSource.Previous;
                    texture1.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src1RGB          = TextureEnvModeSource.Texture;
                    texture1.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src2RGB          = TextureEnvModeSource.Constant;
                    texture1.Operand2RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src0Alpha        = TextureEnvModeSource.Constant;
                    texture1.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture1.TextureEnvColor.SetConstant(0.5f, 0.5f, 0.5f, 1.0f);
                }
                if(filename == "Font")
                {
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Modulate;
                    texture0.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.Texture;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src1RGB          = TextureEnvModeSource.PrimaryColor;
                    texture0.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Texture;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                }
                if(filename == "Schlick")
                {
                    program.fixedFunctionProgram.Lighting = true;
                }
                if(filename == "Manipulator")
                {
                    program.fixedFunctionProgram.Lighting = true;
                }
                if(filename == "Grid")
                {
                    program.fixedFunctionProgram.Lighting = true;
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Modulate;
                    texture0.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.PrimaryColor;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src1RGB          = TextureEnvModeSource.Constant;
                    texture0.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Texture;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture0.TextureEnvColor.SetConstant(0.5f, 0.5f, 0.5f, 1.0f);
                }
                if((filename == "WideLineUniformColor") || (filename == "FatTriangle"))
                {
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.Constant;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Constant;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture0.TextureEnvColor.Bind("material", "line_color");
                }
                if((filename == "Ninepatch") || (filename == "Slider"))
                {
                    program.AttributeMappings = dualTexture;

                    //  Use unit 0 to set global add color
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Replace;
                    texture0.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.Constant;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Constant;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture0.TextureEnvColor.Bind("global", "add_color");

                    //  Use unit 1 to add texture (ninepatch background)
                    var texture1 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[1];
                    texture1.TextureTarget    = TextureTargetRS.Texture2D;
                    texture1.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture1.CombineRGB       = TextureEnvModeCombineRS.Add;
                    texture1.CombineAlpha     = TextureEnvModeCombineRS.Add;
                    texture1.Src0RGB          = TextureEnvModeSource.Texture;
                    texture1.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src0Alpha        = TextureEnvModeSource.Texture;
                    texture1.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture1.Src1RGB          = TextureEnvModeSource.Previous;
                    texture1.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture1.Src1Alpha        = TextureEnvModeSource.Previous;
                    texture1.Operand1Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;

                    //  Use unit 2 to apply alpha premultiplication
                    var texture2 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[2];
                    texture2.TextureTarget    = TextureTargetRS.Texture2D;
                    texture2.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture2.CombineRGB       = TextureEnvModeCombineRS.Modulate;
                    texture2.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture2.Src0RGB          = TextureEnvModeSource.Previous;
                    texture2.Operand0RGB      = TextureEnvModeOperandRgb.SrcAlpha;
                    texture2.Src0Alpha        = TextureEnvModeSource.Previous;
                    texture2.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture2.Src1RGB          = TextureEnvModeSource.Previous;
                    texture2.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture2.Src1Alpha        = TextureEnvModeSource.Constant;
                    texture2.Operand1Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                }

                if(filename == "IdVec3")
                {
                    program.AttributeMappings = idToColor;

                    program.FixedFunctionProgram.UsePerInstance = true;

                    //  Use unit 0 to set add color
                    var texture0 = program.fixedFunctionProgram.TextureEnvironment.TextureUnits[0];
                    texture0.TextureTarget    = TextureTargetRS.Texture2D;
                    texture0.TextureEnvMode   = TextureEnvModeRS.Combine;
                    texture0.CombineRGB       = TextureEnvModeCombineRS.Add;
                    texture0.CombineAlpha     = TextureEnvModeCombineRS.Replace;
                    texture0.Src0RGB          = TextureEnvModeSource.Constant;
                    texture0.Operand0RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src1RGB          = TextureEnvModeSource.PrimaryColor;
                    texture0.Operand1RGB      = TextureEnvModeOperandRgb.SrcColor;
                    texture0.Src0Alpha        = TextureEnvModeSource.Constant;
                    texture0.Operand0Alpha    = TextureEnvModeOperandAlpha.SrcAlpha;
                    texture0.TextureEnvColor.Bind("models", "id_offset_vec3", 4);
                }
    
#if false
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(program.fixedFunctionProgram, Newtonsoft.Json.Formatting.Indented);
                    string path = Configuration.ProgramSearchPath + filename + ".json";
                    System.IO.File.WriteAllText(path, json);

                    path = Configuration.ProgramSearchPath + filename + ".xml";
                    XmlSerializer x = new XmlSerializer(program.fixedFunctionProgram.GetType());
                    StreamWriter writer = new StreamWriter(path);
                    x.Serialize(writer, program.fixedFunctionProgram);
                } 
#endif
#endif

#if false
                string fullpath = Configuration.ProgramSearchPath + filename + ".json";
                if(System.IO.File.Exists(filename))
                {
                    fullpath = filename;
                }

                System.Console.WriteLine("Loading program " + fullpath);

                program.LoadFromFile(fullpath);
#endif

                return program;
            }
#if !DEBUG
            catch(System.Exception e)
            {
                System.Console.WriteLine("Program.Load(" + filename + ") failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
                return null;
            }
#endif
        }

        public void LoadFromFile(string fullpath)
        {
            string json = System.IO.File.ReadAllText(fullpath);
            LoadFromSource(json);
        }
        public void LoadFromSource(string json)
        {
            fixedFunctionProgram = Newtonsoft.Json.JsonConvert.DeserializeObject<FixedFunctionProgram>(json);
        }

        public ProgramGL1(string name)
        {
            Name = name;
        }

        public ProgramAttribute Attribute(string name)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }
}
