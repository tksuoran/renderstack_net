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

// #define DEBUG_PROGRAM
// #define DEBUG_UNIFORM_BUFFER

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using Boolean = OpenTK.Graphics.OpenGL.Boolean;

namespace RenderStack.Graphics
{
    /// \brief Holds GL object name for Program. Allows GL object garbage collection thread to function without GL context.
    public class ProgramGhost : IDisposable
    {
        int programObject;
        public ProgramGhost(int programObject)
        {
            this.programObject = programObject;
        }
        public void Dispose()
        {
            if(programObject != 0)
            {
#if DEBUG_PROGRAM
                Trace.WriteLine("GL.DeleteProgram(" + programObject + ")");
#endif
                GL.DeleteProgram(programObject);
                GhostManager.Delete();
                programObject = 0;
            }
        }
    }
    public class ProgramGL3 : IProgram
#if ASSET_MONITOR
        , AssetMonitor.IMonitored
#endif
    {
        internal static string              ShaderSearchPath    { get; set; }

        private List<ShaderGL3>             shaders         = new List<ShaderGL3>();
        private List<ProgramAttribute>      attributes      = new List<ProgramAttribute>();
        private List<Uniform>               uniforms        = new List<Uniform>();
        private string[]                    uniformBlocks;
        private int                         programObject;

        internal List<ShaderGL3>            Shaders         { get { return shaders; } }
        internal List<ProgramAttribute>     Attributes      { get { return attributes; } }
        internal List<Uniform>              Uniforms        { get { return uniforms; } }
        internal int                        ProgramObject   { get { return programObject; } }
        internal string[]                   UniformBlocks   { get { return uniformBlocks; } }

        private string                      name;
        private bool                        valid;
        public bool                         Valid   { get { return valid; } }
        public string                       Name    { get { return name; } set { name = value; } }
        public int                          SortValue;

        public int[]            SamplerUniformIndices;

        private AttributeMappings   attributeMappings = AttributeMappings.Global;
        public AttributeMappings    AttributeMappings { get { return attributeMappings; } set { attributeMappings = value; } }

        private Samplers        samplers;
        public Samplers Samplers
        {
            get
            {
                return samplers;
            }
            set
            {
                if(value != samplers)
                {
                    samplers = value;
                    UpdateSamplerUniformIndices();
                }
            }
        }

        private void UpdateSamplerUniformIndices()
        {
            //  We need to Use() the program in order
            //  to set values for sampler uniforms
            Push();
            Use(0);

            //System.Diagnostics.Debug.WriteLine("Binding samplers for program " + this.Name);

            SamplerUniformIndices = (Samplers != null) ? new int[Samplers.SamplerUniforms.Count] : new int[0];

            //  Go through all uniforms specified by Samplers
            for(int i = 0; i < samplers.SamplerUniforms.Count; ++i)
            {
                SamplerUniformIndices[i] = -1;

                var samplerUniform = samplers.SamplerUniforms[i];
                if(samplerUniform == null)
                {
                    continue;
                }

                //  See if this program (template) has that sampler uniform
                foreach(var uniform in Uniforms)
                {
                    if(uniform == null)
                    {
                        continue;
                    }
                    if(samplerUniform.Name == uniform.Name)
                    {
                        //  Set value for SamplerUniformIndices, telling which uniform index to use for this sampler in this program
                        SamplerUniformIndices[i] = uniform.Index;
                        GL.Uniform1(uniform.Index, samplerUniform.TextureUnitIndex);
                        //System.Diagnostics.Debug.WriteLine(samplerUniform.Name + " bound to texture unit " + samplerUniform.TextureUnitIndex);
                        continue;
                    }
                }
            }

            //  Restore whatever program we had before (if any)
            Pop();
        }

        protected void Gen()
        {
            programObject = GL.CreateProgram();
            GhostManager.Gen();
            //GL.BindAttribLocation(this.programObject, 0, "_position");
            foreach(var mapping in AttributeMappings.Global.Mappings)
            {
                GL.BindAttribLocation(ProgramObject, mapping.Slot, mapping.Name);
            }
            if(RenderStack.Graphics.Configuration.glVersion >= 300)
            {
#if DEBUG_PROGRAM
                Trace.WriteLine("Binding Frag Data Locations " + Name + " " + programObject);
#endif
                GL.BindFragDataLocation(this.programObject, 0, "out_color");
                GL.BindFragDataLocation(this.programObject, 0, "out0");
                GL.BindFragDataLocation(this.programObject, 1, "out1");
            }
        }
        public ProgramGL3()
        {
            Gen();
        }
        public ProgramGL3(string vertexShader, string fragmentShader)
        {
            Gen();
            Load(vertexShader, fragmentShader);
        }
        bool disposed;
        ~ProgramGL3()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                foreach(var shader in shaders)
                {
                    shader.Dispose();
                }
                shaders.Clear();
                if(programObject != 0)
                {
#if DEBUG_PROGRAM
                    Trace.WriteLine("Ghosting program " + Name + " " + programObject);
#endif
                    GhostManager.Add(new ProgramGhost(programObject));
                    programObject = 0;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        #region Loading
        public static ProgramGL3 Load(string filename)
        {
#if !DEBUG
            try
#endif
            {
                var program = new ProgramGL3();
                string fullpath = Configuration.ProgramSearchPathGL + filename;
                if(System.IO.File.Exists(filename))
                {
                    fullpath = filename;
                }

//#if DEBUG_PROGRAM
                Trace.WriteLine("Loading program " + fullpath);
//#endif

                program.Name = fullpath;
                string binaryPath = fullpath.Replace("res/", "data/") + ".binary";
                bool useBinary = Configuration.useBinaryShaders && Configuration.canUseBinaryShaders && System.IO.File.Exists(binaryPath);

                if(useBinary && program.LoadFromBinary(binaryPath))
                {
                    return program;
                }

                program.LoadFromFiles(
                    Configuration.canUseGeometryShaders ? fullpath + ".gs" : null, 
                    fullpath + ".vs", 
                    fullpath + ".fs"
                );

                if(Configuration.useBinaryShaders && Configuration.canUseBinaryShaders)
                {
                    program.StoreBinaryFile(binaryPath);
                }
                return program;
            }
#if !DEBUG
            catch(System.Exception e)
            {
                Trace.TraceError("Program.Load(" + filename + ") failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
                return null;
            }
#endif
        }
        public void LoadFromFiles(string geometryShader, string vertexShader, string fragmentShader)
        {
#if DEBUG_PROGRAM
            Trace.WriteLine("LoadFromFiles " + Name + " " + programObject);
#endif
#if !DEBUG
            try
#endif
            {
                if(System.IO.File.Exists(geometryShader))
                {
                    MakeShaderFromFile(ShaderType.GeometryShader,   geometryShader);
                }
                if(System.IO.File.Exists(vertexShader))
                {
                    MakeShaderFromFile(ShaderType.VertexShader,     vertexShader);
                }
                else
                {
                    Trace.TraceError("Missing vertex shader source file:" + vertexShader);
                    throw new System.IO.FileNotFoundException("Missing vertex shader source file", vertexShader);
                }
                if(System.IO.File.Exists(fragmentShader))
                {
                    MakeShaderFromFile(ShaderType.FragmentShader,   fragmentShader);
                }
                else
                {
                    Trace.TraceError("Missing fragment shader source file:" + vertexShader);
                    throw new System.IO.FileNotFoundException("Missing fragment shader source file", vertexShader);
                }
                Link();
                valid = true;
            }
#if !DEBUG
            catch(Exception e)
            {
                Trace.TraceError(
                    "Program.LoadFromFiles("
                    + geometryShader + ", "
                    + vertexShader + ", "
                    + fragmentShader + ", "
                    + ") failed - exception " + e.ToString()
                );
                valid = false;
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
            }
#endif
        }
        public void Load(string vertexShader, string fragmentShader)
        {
#if !DEBUG
            try
#endif
            {
                MakeShader(ShaderType.VertexShader, vertexShader);
                MakeShader(ShaderType.FragmentShader, fragmentShader);
                Link();
                valid = true;
            }
#if !DEBUG
            catch(System.Exception e)
            {
                valid = false;
                Trace.TraceError("Program.Load() failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
            }
#endif
        }
        #endregion Loading
        #region Asset monitor
        public void OnFileChanged(string fullpath)
        {
            // NOP - Programs are not loaded from files, shaders are
        }
        public void OnChanged()
        {
            try
            {
                Link();
                valid = true;
            }
            catch(System.Exception e)
            {
                valid = false;
                Trace.TraceError("Program.OnChanged() failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
            }
        }
        #endregion Asset monitor
        #region Binary shaders
        private     byte[]          binary;
        private     BinaryFormat    binaryFormat;
        internal    byte[]          Binary          { get { return binary; } }
        internal    BinaryFormat    BinaryFormat    { get { return binaryFormat; } }
        public static Stopwatch BinaryTime = new Stopwatch();
        private bool LoadFromBinary(string path)
        {
//#if DEBUG_PROGRAM
            // Trace.WriteLine("Program.LoadFromBinary " + path + " " + programObject);
//#endif
#if !DEBUG
            try
#endif
            {
                BinaryTime.Start();
                int numFormats;
                GL.GetInteger(GetPName.NumProgramBinaryFormats, out numFormats);

                int[] formats = new int[numFormats];
                GL.GetInteger(GetPName.ProgramBinaryFormats, formats);

                binaryFormat = (BinaryFormat)(formats[0]);

                binary = System.IO.File.ReadAllBytes(path);
                GL.ProgramBinary(programObject, binaryFormat, binary, binary.Length);

                int linkStatus;
                GL.GetProgram(programObject, ProgramParameter.LinkStatus, out linkStatus);

                //  validation would require proper sampler states
#if false
                GL.ValidateProgram(programObject);

                int validateStatus;
                GL.GetProgram(programObject, ProgramParameter.ValidateStatus, out validateStatus);
#endif
                BinaryTime.Stop();

                valid = true;

                GetAttributes();
                GetUniforms();
                Prepare();

                return true;
            }
#if !DEBUG
            catch(Exception e)
            {
                Trace.TraceError("Program.LoadFromBinary() failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
                return false;
            }
#endif
        }
        private void StoreBinaryFile(string path)
        {
#if DEBUG_PROGRAM
            Trace.WriteLine("Program.StoreBinaryFile " + path + " " + programObject);
#endif
#if !DEBUG
            try
#endif
            {
                int numFormats;
                GL.GetInteger(GetPName.NumProgramBinaryFormats, out numFormats);

                if(numFormats < 1)
                {
                    return;
                }

                int[] formats = new int[numFormats];
                GL.GetInteger(GetPName.ProgramBinaryFormats, formats);

                int length = 0;

                int linkStatus;
                GL.GetProgram(programObject, ProgramParameter.LinkStatus, out linkStatus);

#if false
                //  I would need to make sure i have samplers properly set
                GL.ValidateProgram(programObject);

                int validateStatus;
                GL.GetProgram(programObject, ProgramParameter.ValidateStatus, out validateStatus);
#endif

                GL.GetProgram(programObject, ProgramParameter.ProgramBinaryLength, out length);

                if(length == 0)
                {
                    Trace.TraceWarning("Program.StoreBinaryFile() failed - binary length 0");
                    return;
                }

                BinaryFormat format = (BinaryFormat)(formats[0]);
                byte[] binary = new byte[length];
                int usedLength = 0;

                GL.GetProgramBinary<byte>(
                    programObject, 
                    binary.Length, 
                    out usedLength, 
                    out format, 
                    binary
                );

                string directory = System.IO.Path.GetDirectoryName(path);
                if(System.IO.Directory.Exists(directory) == false)
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                System.IO.File.WriteAllBytes(path, binary);
            }
#if !DEBUG
            catch(Exception e)
            {
                //  Storing shader binary failed, no serious harm done
                Trace.TraceWarning("Program.StoreBinaryFile() failed - exception " + e.ToString());
            }
#endif
        }
        #endregion Binary shaders

        private void MakeShader(ShaderType type, string source)
        {
            if(source == null)
            {
                return;
            }

            var shader = new ShaderGL3(type);
            shader.Load(source);
            Shaders.Add(shader);

            GL.AttachShader(ProgramObject, ((IShaderGL3)shader).ShaderObject);
        }
        private void MakeShaderFromFile(ShaderType type, string fullpath)
        {
            if(fullpath == null)
            {
                return;
            }

            var shader = new ShaderGL3(type);
            shader.LoadFromFile(fullpath);
#if ASSET_MONITOR
            shader.AddProgram(this);
#endif
            Shaders.Add(shader);

            GL.AttachShader(ProgramObject, ((IShaderGL3)shader).ShaderObject);
        }


        private static ProgramGL3          currentProgram;
        private static Stack<ProgramGL3>   programStack = new Stack<ProgramGL3>();
        public static void Push()
        {
            programStack.Push(currentProgram);
        }
        public static void Pop()
        {
            var program = programStack.Pop();
            if(program != currentProgram)
            {
                if(program != null)
                {
                    program.Use(0);
                }
            }
            currentProgram = program;
        }
        public void Use(int baseInstance)
        {
            if(Valid)
            {
                GL.UseProgram(ProgramObject);
#if DEBUG_PROGRAM
                Trace.WriteLine("Program.Use " + this.Name + " object " + ProgramObject);
#endif
            }
            else
            {
                throw new Exception();
            }
            currentProgram = this;
        }
        public static Stopwatch LinkTime = new Stopwatch();
        private void Link()
        {
            int linkStatus;

            LinkTime.Start();
            if(Configuration.useBinaryShaders && Configuration.canUseBinaryShaders)
            {
                try
                {
                    GL.ProgramParameter(programObject, AssemblyProgramParameterArb.ProgramBinaryRetrievableHint, (int)All.True);
                }
                catch(Exception)
                {
                }
            }

            GL.LinkProgram(ProgramObject);
            GL.GetProgram(ProgramObject, ProgramParameter.LinkStatus, out linkStatus);
            LinkTime.Stop();

            if(linkStatus == (int)Boolean.False)
            {
                string infoLog = GL.GetProgramInfoLog(ProgramObject);

                System.Diagnostics.Trace.TraceError("Program linking failed:");
                System.Diagnostics.Trace.TraceError(infoLog);
                foreach(var shader in Shaders)
                {
                    System.Console.WriteLine("---------------------------");
                    System.Console.WriteLine(shader.FormatSource);
                }
                System.Console.WriteLine("---------------------------");

                GL.DeleteProgram(ProgramObject);

                System.Diagnostics.Trace.TraceError("Program linking failed:");
                System.Diagnostics.Trace.TraceError(infoLog);

                valid = false;

                throw new System.InvalidOperationException();
            }
            else
            {
                valid = true;
            }

            GetAttributes();
            GetUniforms();
            Prepare();
        }

        private void Prepare()
        {
            if(Configuration.canUseUniformBufferObject)
            {
                //  Associate uniform blocks with binding points
                for(int i = 0; i < UniformBlocks.Length; ++i)
                {
                    var name = UniformBlocks[i];
                    var uniformBlock = UniformBlockGL.Instances[name];
                    GL.UniformBlockBinding(programObject, i, uniformBlock.BindingPointGL);
                    /*System.Diagnostics.Debug.WriteLine(
                        "GL.UniformBlockBinding(" +
                        programObject + ", " +
                        i + ", " +
                        uniformBlock.BindingPointGL +
                        "); program name " + Name + ", uniform block name " + uniformBlock.Name + " (" + name + ")"
                    );*/
                }
            }

            //  Set default samplers
            Samplers = Samplers.Global;
        }

        private void GetAttributes()
        {
            int attributeCount = 0;

            GL.GetProgram(
                ProgramObject, 
                ProgramParameter.ActiveAttributes, 
                out attributeCount
            );

            Attributes.Clear();
            for(int i = 0; i < attributeCount; ++i)
            {
                int                 size;
                ActiveAttribType    type;

                string name = GL.GetActiveAttrib(
                    ProgramObject,
                    i,
                    out size,
                    out type
                );

                int slot = GL.GetAttribLocation(ProgramObject, name);

                // Trace.WriteLine("\tAttribute " + name + " slot " + slot.ToString());

                var attribute = new ProgramAttribute(
                    name, 
                    slot, 
                    size, 
                    type
                );

                Attributes.Add(attribute);
            }
        }
        private void GetUniforms()
        {
            int                 size;
            ActiveUniformType   type;
            int                 uniformCount = 0;

            if(Configuration.canUseUniformBufferObject)
            {
                int uniformBlockCount;
                GL.GetProgram(
                    ProgramObject,
                    ProgramParameter.ActiveUniformBlocks,
                    out uniformBlockCount
                );

                uniformBlocks = new string[uniformBlockCount];
                for(int i = 0; i < uniformBlockCount; ++i)
                {
                    string name = GL.GetActiveUniformBlockName(ProgramObject, i);
                    GL.GetActiveUniformBlock(
                        ProgramObject, 
                        i, 
                        ActiveUniformBlockParameter.UniformBlockActiveUniforms, 
                        out uniformCount
                    );
                    uniformBlocks[i] = name;
#if true
                    int dataSize;
                    GL.GetActiveUniformBlock(
                        ProgramObject, 
                        i, 
                        ActiveUniformBlockParameter.UniformBlockDataSize,
                        out dataSize
                    );
#if DEBUG_UNIFORM_BUFFER
                    System.Diagnostics.Debug.WriteLine("UniformBlock " + i + " " + name + " with " + uniformCount + " uniforms, dataSize = " + dataSize);
#endif
                    int[] indices = new int[uniformCount];
                    int[] offsets = new int[uniformCount];
                    int[] strides = new int[uniformCount];
                    int[] matrixStrides = new int[uniformCount];
                    int[] sizes = new int[uniformCount];
                    int[] types = new int[uniformCount];
                    GL.GetActiveUniformBlock(
                        ProgramObject, 
                        i, 
                        ActiveUniformBlockParameter.UniformBlockActiveUniformIndices,
                        indices
                    );
                    int referencedByFragmentShader;
                    int referencedByVertexShader;
                    GL.GetActiveUniformBlock(
                        ProgramObject, 
                        i, 
                        ActiveUniformBlockParameter.UniformBlockReferencedByFragmentShader,
                        out referencedByFragmentShader
                    );
                    GL.GetActiveUniformBlock(
                        ProgramObject, 
                        i, 
                        ActiveUniformBlockParameter.UniformBlockReferencedByVertexShader,
                        out referencedByVertexShader
                    );
#if DEBUG_UNIFORM_BUFFER
                    //System.Diagnostics.Debug.WriteLine("referencedByVertexShader: " + referencedByVertexShader);
                    //System.Diagnostics.Debug.WriteLine("referencedByFragmentShader: " + referencedByFragmentShader);
#endif
                    GL.GetActiveUniforms(ProgramObject, uniformCount, indices, ActiveUniformParameter.UniformOffset, offsets);
                    GL.GetActiveUniforms(ProgramObject, uniformCount, indices, ActiveUniformParameter.UniformArrayStride, strides);
                    GL.GetActiveUniforms(ProgramObject, uniformCount, indices, ActiveUniformParameter.UniformMatrixStride, matrixStrides);
                    GL.GetActiveUniforms(ProgramObject, uniformCount, indices, ActiveUniformParameter.UniformSize, sizes);
                    GL.GetActiveUniforms(ProgramObject, uniformCount, indices, ActiveUniformParameter.UniformType, types);
                    for(int j = 0; j < uniformCount; ++j)
                    {
                        name = GL.GetActiveUniform(
                            ProgramObject,
                            indices[j],
                            out size,
                            out type
                        );

                        if(size != sizes[j]) System.Diagnostics.Debug.WriteLine("\tSIZE MISMATCH");
                        if(type != (ActiveUniformType)types[j]) System.Diagnostics.Debug.WriteLine("\tTYPE MISMATCH");

#if false //DEBUG_UNIFORM_BUFFER
                        System.Diagnostics.Debug.WriteLine(
                            "\tindex: " + indices[j] + ", " + type + " " + name + 
                            ", size = " + size + 
                            ", offset = " + offsets[j] + 
                            ", stride = " + strides[j]
                        );
#endif
                        /*if(indices[j] > maxIndex)
                        {
                            maxIndex = indices[j];
                        }*/
                    }
#endif
                }
            }

            GL.GetProgram(
                ProgramObject,
                ProgramParameter.ActiveUniforms,
                out uniformCount
            );

            for(int i = 0; i < uniformCount; ++i)
            {
                string name = GL.GetActiveUniform(
                    ProgramObject,
                    i,
                    out size,
                    out type
                );

                int index = GL.GetUniformLocation(ProgramObject, name);

                if(index >= 0)
                {
                    //Trace.WriteLine("\tUniform " + name + " index " + index);

                    var uniform = new Uniform(
                        name,
                        index,
                        size,
                        type
                    );

                    uniforms.Add(uniform);
                }
            }
        }

        internal bool HasAttribute(string name)
        {
            foreach(var attribute in Attributes)
            {
                if(attribute.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
        public ProgramAttribute Attribute(string name)
        {
            foreach(var a in Attributes)
            {
                if(a.Name == name)
                {
                    return a;
                }
            }
            return null;
        }

        public override string ToString()
        {
            if(valid)
            {
                return Name;
            }
            return Name + "(!)";
        }
    }
}

