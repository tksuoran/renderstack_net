//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;

using example.Loading;
using example.Renderer;

using Attribute = RenderStack.Graphics.Attribute;

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
            var renderer = Services.Get<IRenderer>();
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
