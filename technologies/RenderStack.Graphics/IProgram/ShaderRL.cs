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

#if ASSET_MONITOR
using RenderStack.Services;
#endif

namespace RenderStack.Graphics
{
    /// \brief Holds GL object name for Shader. Allows GL object garbage collection thread to function without GL context.
    /// \note Mostly stable.
    public class ShaderRLGhost : IDisposable
    {
        RLshader shaderObject;
        public ShaderRLGhost(RLshader shaderObject)
        {
            this.shaderObject = shaderObject;
        }
        public void Dispose()
        {
            if(shaderObject != System.IntPtr.Zero)
            {
                RL.DeleteShader(shaderObject);
                GhostManager.Delete();
                shaderObject = System.IntPtr.Zero;
            }
        }
    }
    /// \brief Abstraction for OpenGL shader object
    /// \note Mostly stable.
    public class ShaderRL : IDisposable
#if ASSET_MONITOR
        , AssetMonitor.IMonitored
#endif
    {
        private RLshader                        shaderObject;
        private ShaderType                      type;
        private List<AssetMonitor.IMonitored>   programs = new List<AssetMonitor.IMonitored>();

        private static List<KeyValuePair<string,string>> replacements = new List<KeyValuePair<string,string>>();

        public static void Replace(string old, string @new)
        {
            replacements.Add(new KeyValuePair<string,string>(old, @new));
        }

        public string   Source       { get; private set; }
        public RLshader ShaderObject { get { return shaderObject; } }

        bool disposed;
        ~ShaderRL()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(shaderObject != IntPtr.Zero)
                {
                    GhostManager.Add(new ShaderRLGhost(shaderObject));
                    shaderObject = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        public ShaderRL(ShaderType type)
        {
            shaderObject = RL.CreateShader(type);
            GhostManager.Gen();
            this.type = type;
        }

        public void LoadFromFile(string fullpath)
        {
            string source = System.IO.File.ReadAllText(fullpath);
#if ASSET_MONITOR
            var monitor = BaseServices.Get<AssetMonitor.AssetMonitor>();
            if(monitor != null)
            {
                monitor.AssetLoaded(this, fullpath);
            }
#endif
            Load(source);
        }

        public static Stopwatch compileTime = new Stopwatch();
        public void Load(string source)
        {
            int compileStatus = int.MaxValue;

            foreach(var replacement in replacements)
            {
                source = source.Replace(replacement.Key, replacement.Value);
            }

            compileTime.Start();
            RL.ShaderSource(ShaderObject, ref source);
            RL.CompileShader(ShaderObject);
            RL.GetShader(ShaderObject, ShaderParameter.CompileStatus, out compileStatus);
            compileTime.Stop();

            if(compileStatus == 0)
            {
                string infoLog = RL.GetShaderString(ShaderObject, ShaderStringParameter.CompileLog);
                //GL.DeleteShader(ShaderObject);
                Trace.TraceError("Shader compilation failed: ");
                Trace.TraceError(infoLog + "\n");
                Trace.TraceError(Format(source));

                if(Source != null)
                {
                    source = Source;
                    RL.ShaderSource(ShaderObject, ref source);
                    RL.CompileShader(ShaderObject);
                    RL.GetShader(ShaderObject, ShaderParameter.CompileStatus, out compileStatus);
                }
                else
                {
                    string typeStr = type.ToString();

                    throw new System.Exception(typeStr + " shader compilation failed:\n" + infoLog + "\n");
                }
            }
            else
            {
                Source = source;
                //System.Diagnostics.Debug.WriteLine(Format(source));
            }
        }

        public void AddProgram(AssetMonitor.IMonitored program)
        {
            programs.Add(program);
        }

        public void OnChanged()
        {
            foreach(var program in programs)
            {
                program.OnChanged();
            }
        }

        public void OnFileChanged(string fullpath)
        {
            string source = null;

            for(int i = 0; i < 3; ++i)
            {
                try
                {
                    source = System.IO.File.ReadAllText(fullpath);
                    break;
                }
                catch(System.Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    System.Threading.Thread.Sleep(i);
                }
            }
            if(source != null)
            {
                Load(source);
                OnChanged();
            }
            else
            {
                Trace.TraceWarning("Could not reload " + fullpath);
            }
        }

        public string FormatSource { get { return Format(Source); } }

        public static string Format(string source)
        {
            source = source.Replace("\r\n", "\n");
            source = source.Replace("\r", "\n");
            if(source != null)
            {
                var sb = new System.Text.StringBuilder();

                int lineNumber = 1;

                string[] lines = source.Split('\n');
                foreach(string line in lines)
                {
                    sb.Append(lineNumber.ToString());
                    sb.Append(": ");
                    sb.Append(line);
                    sb.Append('\n');
                    ++lineNumber;
                }
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
