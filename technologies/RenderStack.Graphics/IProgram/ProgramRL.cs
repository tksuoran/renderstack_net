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
using Caustic.OpenRL;

using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

namespace RenderStack.Graphics
{
    /// \brief Holds RL handle for Program. Allows RL object garbage collection thread to function without RL context.
    public class ProgramRLGhost : IDisposable
    {
        RLprogram programObject;
        public ProgramRLGhost(RLprogram programObject)
        {
            this.programObject = programObject;
        }
        public void Dispose()
        {
            if(programObject != System.IntPtr.Zero)
            {
#if DEBUG_PROGRAM
                Trace.WriteLine("RL.DeleteProgram(" + programObject + ")");
#endif
                RL.DeleteProgram(programObject);
                GhostManager.Delete();
                programObject = System.IntPtr.Zero;
            }
        }
    }
    public class ProgramRL : IProgram
#if ASSET_MONITOR
        , AssetMonitor.IMonitored
#endif
    {
        internal static string              ShaderSearchPath    { get; set; }

        private List<ShaderRL>              shaders         = new List<ShaderRL>();
        private List<ProgramAttribute>      attributes      = new List<ProgramAttribute>();
        private List<Uniform>               uniforms        = new List<Uniform>();
        private Dictionary<string,int>      uniformBlocks   = new Dictionary<string, int>();
        private RLprogram                   programObject;

        internal List<ShaderRL>             Shaders         { get { return shaders; } }
        internal List<ProgramAttribute>     Attributes      { get { return attributes; } }
        internal List<Uniform>              Uniforms        { get { return uniforms; } }
        internal RLprogram                  ProgramObject   { get { return programObject; } }
        internal Dictionary<string,int>     UniformBlocks   { get { return uniformBlocks; } }

        private string                      name;
        private bool                        valid;
        public bool                         Valid   { get { return valid; } }
        public string                       Name    { get { return name; } set { name = value; } }
        public int                          SortValue;

        public int[]                        SamplerUniformIndices;

        private AttributeMappings   attributeMappings = AttributeMappings.Global;
        public AttributeMappings    AttributeMappings { get { return attributeMappings; } set { attributeMappings = value; } }

        public Samplers Samplers
        {
            get
            {
                return null;
            }
            set
            {
                // \todo
            }
        }

        protected void Gen()
        {
            programObject = RL.CreateProgram();
            GhostManager.Gen();
        }
        public ProgramRL()
        {
            Gen();
        }
        public ProgramRL(string rayShader, string vertexShader, string frameShader)
        {
            Gen();
            Load(rayShader, vertexShader, frameShader);
        }
        bool disposed;
        ~ProgramRL()
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
                if(programObject != System.IntPtr.Zero)
                {
#if DEBUG_PROGRAM
                    Trace.WriteLine("Ghosting program " + Name + " " + programObject);
#endif
                    GhostManager.Add(new ProgramRLGhost(programObject));
                    programObject = System.IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        #region Loading
        public static ProgramRL Load(string filename)
        {
#if !DEBUG
            try
#endif
            {
                var program = new ProgramRL();
                string fullpath = Configuration.ProgramSearchPathRL + filename;
                if(System.IO.File.Exists(filename))
                {
                    fullpath = filename;
                }

//#if DEBUG_PROGRAM
                Trace.WriteLine("Loading program " + fullpath);
//#endif

                program.Name = fullpath;

                program.LoadFromFiles(
                    fullpath + ".rs", 
                    fullpath + ".vs",
                    fullpath + ".fs" 
                );

                return program;
            }
#if !DEBUG
            catch(System.Exception e)
            {
                Trace.TraceError("ProgramRL.Load(" + filename + ") failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
                return null;
            }
#endif
        }
        public void LoadFromFiles(string rayShader, string vertexShader, string frameShader)
        {
#if DEBUG_PROGRAM
            Trace.WriteLine("LoadFromFiles " + Name + " " + programObject);
#endif
#if !DEBUG
            try
#endif
            {
                bool loaded = false;
                if(System.IO.File.Exists(rayShader))
                {
                    MakeShaderFromFile(ShaderType.RayShader,    rayShader);
                    loaded = true;
                }
                if(System.IO.File.Exists(vertexShader))
                {
                    MakeShaderFromFile(ShaderType.VertexShader, vertexShader);
                    loaded = true;
                }
                if(System.IO.File.Exists(frameShader))
                {
                    MakeShaderFromFile(ShaderType.FrameShader,  frameShader);
                    loaded = true;
                }
                if(loaded == false)
                {
                    throw new System.IO.FileNotFoundException("Missing shader source files " + rayShader + " / " + vertexShader + " / " + frameShader);
                }
                Link();
                valid = true;
            }
#if !DEBUG
            catch(Exception e)
            {
                Trace.TraceError(
                    "ProgramRL.LoadFromFiles("
                    + rayShader + ", "
                    + vertexShader + ", "
                    + frameShader + ", "
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
        public void Load(string rayShader, string vertexShader, string frameShader)
        {
#if !DEBUG
            try
#endif
            {
                MakeShader(ShaderType.RayShader,    rayShader);
                MakeShader(ShaderType.VertexShader, vertexShader);
                MakeShader(ShaderType.FrameShader,  frameShader);
                Link();
                valid = true;
            }
#if !DEBUG
            catch(System.Exception e)
            {
                valid = false;
                Trace.TraceError("ProgramRL.Load() failed - exception " + e.ToString());
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
                Trace.TraceError("ProgramRL.OnChanged() failed - exception " + e.ToString());
                if(Configuration.throwProgramExceptions)
                {
                    throw;
                }
            }
        }
        #endregion Asset monitor

        private void MakeShader(ShaderType type, string source)
        {
            if(source == null)
            {
                return;
            }

            var shader = new ShaderRL(type);
            shader.Load(source);
            Shaders.Add(shader);

            RL.AttachShader(ProgramObject, shader.ShaderObject);
        }
        private void MakeShaderFromFile(ShaderType type, string fullpath)
        {
            if(fullpath == null)
            {
                return;
            }

            var shader = new ShaderRL(type);
            shader.LoadFromFile(fullpath);
#if ASSET_MONITOR
            shader.AddProgram(this);
#endif
            Shaders.Add(shader);

            RL.AttachShader(ProgramObject, shader.ShaderObject);
        }


        private static ProgramRL        currentProgram;
        private static Stack<ProgramRL> programStack = new Stack<ProgramRL>();
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
                RL.UseProgram(ProgramObject);
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
            RL.LinkProgram(ProgramObject);
            RL.GetProgram(ProgramObject, ProgramParameter.LinkStatus, out linkStatus);
            LinkTime.Stop();

            if(linkStatus == 0)
            {
                string infoLog;
                RL.GetProgramString(ProgramObject, ProgramStringParameter.LinkLog, out infoLog);
                RL.DeleteProgram(ProgramObject);

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
            //  Associate uniform blocks with binding points
            //  \todo

            //  Set default samplers
            Samplers = Samplers.Global;
        }

        private void GetAttributes()
        {
            int attributeCount = 0;

            RL.GetProgram(ProgramObject, ProgramParameter.ActiveAttributes, out attributeCount);

            if(Name.EndsWith("simpleFrame"))
            {
                //  No attributes in the frame shader
            }
            else if(Name.EndsWith("simplePrimitive"))
            {
                int pos = RL.GetAttribLocation(ProgramObject, "_position");
                int nor = RL.GetAttribLocation(ProgramObject, "_normal");
                Attributes.Add(new ProgramAttribute("_position", pos, 1, OpenTK.Graphics.OpenGL.ActiveAttribType.FloatVec3));
                Attributes.Add(new ProgramAttribute("_normal",   nor, 1, OpenTK.Graphics.OpenGL.ActiveAttribType.FloatVec3));
            }
            else
            {
                Attributes.Clear();
                for(int i = 0; i < attributeCount; ++i)
                {
                    int                 size;
                    ActiveAttribType    type;
                    string              name;

                    RL.GetActiveAttrib(ProgramObject, i, out size, out type, out name);

                    int slot = RL.GetAttribLocation(ProgramObject, name);

                    Trace.WriteLine("\tAttribute " + name + " slot " + slot.ToString());

                    var attribute = new ProgramAttribute(
                        name, 
                        slot, 
                        size, 
                        (OpenTK.Graphics.OpenGL.ActiveAttribType)type
                    );

                    Attributes.Add(attribute);
                }
            }
        }
        private void GetUniforms()
        {
            int                 size;
            ActiveUniformType   type;
            string              name;

            if(Configuration.canUseUniformBufferObject)
            {
                int uniformBlockCount;
                RL.GetProgram(
                    ProgramObject,
                    ProgramParameter.ActiveUniformBlocks,
                    out uniformBlockCount
                );

                //uniformBlocks = new string[uniformBlockCount];
                for(int i = 0; i < uniformBlockCount; ++i)
                {
                    int fieldCount;
                    int blockSize;

                    RL.GetActiveUniformBlock(ProgramObject, i, out name, out fieldCount, out blockSize);

                    //uniformBlocks[i] = name;
                    int index = RL.GetUniformBlockIndex(ProgramObject, name);
                    uniformBlocks[name] = index;
#if true
#if DEBUG_UNIFORM_BUFFER
                    System.Diagnostics.Debug.WriteLine("UniformBlock " + i + " " + name + " with " + fieldCount + " uniforms, blockSize = " + blockSize);
#endif
                    for(int j = 0; j < fieldCount; ++j)
                    {
                        int                 offset;
                        RL.GetActiveUniformBlockField(ProgramObject, i, j, out size, out type, out name);
                        offset = RL.GetUniformBlockFieldOffset(ProgramObject, i, name);
#if DEBUG_UNIFORM_BUFFER
                        System.Diagnostics.Debug.WriteLine(
                            "\tindex: " + j + ", " + type + " " + name + 
                            ", size = " + size + 
                            ", offset = " + offset
                        );
#endif
                    }
#endif
                }
            }

            int uniformCount = 0;
            RL.GetProgram(ProgramObject, ProgramParameter.ActiveUniforms, out uniformCount);

            for(int i = 0; i < uniformCount; ++i)
            {
                RL.GetActiveUniform(ProgramObject, i, out size, out type, out name);
                int index = RL.GetUniformLocation(ProgramObject, name);

                if(index >= 0)
                {
                    //Trace.WriteLine("\tUniform " + name + " index " + index);

                    var uniform = new Uniform(
                        name,
                        index,
                        size,
                        (OpenTK.Graphics.OpenGL.ActiveUniformType)type
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
