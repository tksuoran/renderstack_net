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
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using Boolean = OpenTK.Graphics.OpenGL.Boolean;

#if ASSET_MONITOR
using RenderStack.Services;
#endif

namespace RenderStack.Graphics
{
    /// \brief Holds GL object name for Shader. Allows GL object garbage collection thread to function without GL context.
    /// \note Mostly stable.
    public class ShaderGL3Ghost : IDisposable
    {
        int shaderObject;
        public ShaderGL3Ghost(int shaderObject)
        {
            this.shaderObject = shaderObject;
        }
        public void Dispose()
        {
            if(shaderObject != 0)
            {
                GL.DeleteShader(shaderObject);
                GhostManager.Delete();
                shaderObject = 0;
            }
        }
    }
    public interface IShaderGL3
    {
        int ShaderObject { get; }
    }
    /// \brief Abstraction for OpenGL shader object
    /// \note Mostly stable.
    public class ShaderGL3 : IShaderGL3, IDisposable
#if ASSET_MONITOR
        , AssetMonitor.IMonitored
#endif
    {
        private int                             shaderObject;
        private ShaderType                      type;
        private List<AssetMonitor.IMonitored>   programs = new List<AssetMonitor.IMonitored>();

        private static List<KeyValuePair<string,string>> replacements = new List<KeyValuePair<string,string>>();

        public static void Replace(string old, string @new)
        {
            replacements.Add(new KeyValuePair<string,string>(old, @new));
        }

        public string Source       { get; private set; }
        int    IShaderGL3.ShaderObject { get { return shaderObject; } }

        bool disposed;
        ~ShaderGL3()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(shaderObject != 0)
                {
                    GhostManager.Add(new ShaderGL3Ghost(shaderObject));
                    shaderObject = 0;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        public ShaderGL3(ShaderType type)
        {
            shaderObject = GL.CreateShader(type);
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

#if SHADER_COMPATIBILITY_HACK
            if(source.IndexOf("textureCube") >= 0)
            {
                source = source.Replace("textureCube(", "texture(");
                source = source.Replace("texture3D(", "texture(");
            }
            if(RenderStack.Graphics.Configuration.GL3)
            {
                source = source.Replace("textureCube(", "texture(");
                source = source.Replace("texture3D(", "texture(");
            }
            else
            {
                source = source.Replace("#version 330", "#version 120");
                source = source.Replace("uint ", "vec4 ");
                source = source.Replace("uint\t", "vec4\t");

                switch(type)
                {
                    case ShaderType.VertexShader:
                    {
                        source = source.Replace("in ", "attribute ");
                        source = source.Replace("in\t", "attribute\t");
                        source = source.Replace("out ", "varying ");
                        source = source.Replace("out\t", "varying\t");
                        break;
                    }
                    case ShaderType.GeometryShader:
                    {
                        break;
                    }
                    case ShaderType.FragmentShader:
                    {
                        source = source.Replace("in ", "varying ");
                        source = source.Replace("in\t", "varying\t");
                        source = source.Replace("out_color ", "gl_FragData[0] ");
                        source = source.Replace("out_color\t", "gl_FragData[0]\t");
                        source = source.Replace("out_color.", "gl_FragData[0].");
                        source = source.Replace("out ", "// ");
                        source = source.Replace("out\t", "//\t");
                        break;
                    }
                }
            }
#endif

            compileTime.Start();
            GL.ShaderSource(shaderObject, source);
            GL.CompileShader(shaderObject);
            GL.GetShader(shaderObject, ShaderParameter.CompileStatus, out compileStatus);
            compileTime.Stop();

            if(compileStatus == (int)Boolean.False)
            {
                string infoLog = GL.GetShaderInfoLog(shaderObject);
                //GL.DeleteShader(ShaderObject);
                Trace.TraceError("Shader compilation failed: ");
                Trace.TraceError(infoLog + "\n");
                Trace.TraceError(Format(source));

                if(Source != null)
                {
                    GL.ShaderSource(shaderObject, Source);
                    GL.CompileShader(shaderObject);
                    GL.GetShader(shaderObject, ShaderParameter.CompileStatus, out compileStatus);
                }
                else
                {
                    string typeStr = "? ";
                    switch(type)
                    {
                        case ShaderType.VertexShader:
                        {
                            typeStr = "Vertex ";
                            break;
                        }
                        case ShaderType.GeometryShader:
                        {
                            typeStr = "Geometry ";
                            break;
                        }
                        case ShaderType.FragmentShader:
                        {
                            typeStr = "Fragment ";
                            break;
                        }
                    }

                        throw new System.Exception(
                        typeStr + " shader compilation failed:\n" + infoLog + "\n"
                    );
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
