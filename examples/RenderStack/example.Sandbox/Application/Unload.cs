using System;
using RenderStack.Graphics;
using example.Renderer;

namespace example.Sandbox
{
    public partial class Application
    {
        private void Application_Unload(object sender, EventArgs e)
        {
            render = false;
            if(Configuration.threadedRendering && renderThread != null)
            {
                renderThread.Join();
                renderThread = null;
            }
            Context.MakeCurrent(WindowInfo);
            IRenderer renderer = RenderStack.Services.BaseServices.Get<IRenderer>();
            if(renderer != null)
            {
                renderer.Unload();
            }
            GC.Collect();
            GhostManager.Process();
        }
    }
}

            /*
                Triangulate(
                Truncate(
                Flat(
                Flat(
                Truncate(
                Triangulate(
                Flat(
                Triangulate(
                Flat(
                Truncate(
                    Cube
                ))))))))))
*/


#if false
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Debug.WriteLine("Assembly: " + assembly.FullName);
                foreach(var attribute_ in assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false))
                {
                    System.Reflection.AssemblyProductAttribute attribute = (System.Reflection.AssemblyProductAttribute)attribute_;
                        Debug.WriteLine("Assembly.Product: " + attribute.Product);
                    /*if(attribute.Product == "RenderStack")
                    {
                        Debug.WriteLine("Assembly.Product: " + attribute.Product);
                    }*/
                }
            }
            foreach(var assembly in assemblies)
            {
                Debug.WriteLine("Assembly: " + assembly.FullName);
            }
            //var types = assemblies.SelectMany(a => a.GetTypes()).Where(t => System.Attribute.IsDefined(t, typeof(SerializableAttribute)));
            foreach(var assembly in assemblies)
            {
                Debug.WriteLine("Type: " + assembly.FullName);
            }
#if false
        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if(
                args.LoadedAssembly.GetCustomAttributes(
                    typeof(System.Reflection.AssemblyProductAttribute), 
                    false
                ).Cast<System.Reflection.AssemblyProductAttribute>().Any(b => b.Product == "RenderStack")
            )
            {
                var types = args.LoadedAssembly.GetTypes().Where(t => t.IsDefined(typeof(SerializableAttribute), true));
                foreach(var type in types)
                {
                    Debug.WriteLine("Preparing Serialization Type: " + type.FullName);
                }
                NetSerializer.Serializer.Initialize(types.ToArray());
            }
        }
#endif

        protected void InitializeSerializer()
        {
#if false
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(
                a => a.GetCustomAttributes(
                    typeof(System.Reflection.AssemblyProductAttribute), 
                    false
                ).Cast<System.Reflection.AssemblyProductAttribute>().Any(b => b.Product == "RenderStack"))
            ;
            var types = assemblies.SelectMany(a => a.GetTypes()).Where(
                t => t.IsDefined(typeof(SerializableAttribute), true)
            );
#endif
            Type[] types = {
                typeof(RenderStack.Mesh.GeometryMesh),
                typeof(RenderStack.Geometry.Geometry),
                typeof(RenderStack.Graphics.Buffer),
            };
            foreach(var type in types)
            {
                Debug.WriteLine("Preparing Serialization Type: " + type.FullName);
            }
            NetSerializer.Serializer.Initialize(types.ToArray());
            //AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
        }
#endif
