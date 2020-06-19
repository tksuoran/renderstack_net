using System;
using System.Collections.Generic;

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
