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
using System.Threading;
using System.Collections.Generic;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

#if ASSET_MONITOR
using RenderStack.Graphics.AssetMonitor;
#endif
using RenderStack.Graphics;
using RenderStack.Services;

namespace example.Renderer
{
    // \brief Maintains a centralized collection of Programs to avoid duplicates
    public class Programs : IDisposable
    {
        private Dictionary<string, IProgram> programs = new Dictionary<string,IProgram>();

        public IProgram this[string name]
        {
            get
            {
                if(name == null)
                {
                    return null;
                }
                if(programs.ContainsKey(name))
                {
                    return programs[name];
                }
                {
                    return Load(name);
                }
            }
        }

        private IProgram Load(string name)
        {
            IProgram program = ProgramFactory.Load(name);
            programs[name] = program;
            return program;
        }

#if ASSET_MONITOR
        public Programs()
        {
            var monitor = BaseServices.Get<AssetMonitor>();
            if(monitor == null)
            {
                return;
            }
            try
            {
                string dstPath;
                string srcPath;
                dstPath = System.IO.Path.GetFullPath("res");
                    srcPath = System.IO.File.ReadAllText(
                        System.IO.Path.Combine(dstPath, "source.txt")
                    ).Trim();

                string shaders;
                if(RenderStack.Graphics.Configuration.glslVersion < 330)
                {
                    shaders = "OpenGL1";
                }
                else
                {
                    shaders = "OpenGL3";
                }

                dstPath = System.IO.Path.Combine(dstPath, shaders);
                srcPath = System.IO.Path.Combine(srcPath, shaders);
                dstPath = System.IO.Path.GetFullPath("res");
                srcPath = System.IO.File.ReadAllText(
                    System.IO.Path.Combine(dstPath, "source.txt")
                ).Trim();

                dstPath = System.IO.Path.Combine(dstPath, shaders);
                srcPath = System.IO.Path.Combine(srcPath, shaders);

                monitor.SrcPath = srcPath;
                monitor.DstPath = dstPath;
                monitor.Start();
            }
            catch(System.Exception)
            {
                // \todo What should we do?
            }
       }
        ~Programs()
        {
            Dispose();
        }
#endif

        public void Dispose()
        {
            foreach(var kvp in programs)
            {
                kvp.Value.Dispose();
            }
            programs.Clear();
        }

    }
}
