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

using System.Diagnostics;
using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace example.UI
{
    public partial class Application
    {
        private static Application  services;
        public static Application   Services { get { return services; } }
        private HashSet<Service>    servicesSet = new HashSet<Service>();

        Renderer                    renderer;
        MaterialManager             materialManager;
        SceneManager                sceneManager;
        TextRenderer                textRenderer;
        UserInterfaceManager        userInterfaceManager;

        public Renderer             Renderer                { get { return renderer; } }
        public MaterialManager      MaterialManager         { get { return materialManager; } }
        public SceneManager         SceneManager            { get { return sceneManager; } }
        public TextRenderer         TextRenderer            { get { return textRenderer; } }
        public UserInterfaceManager UserInterfaceManager    { get { return userInterfaceManager; } }

        internal void Add(Service service)
        {
            servicesSet.Add(service);
        }
        public void CleanupServices()
        {
            services = null;
            servicesSet = null;
            renderer = null;
            materialManager = null;
            sceneManager = null;
            textRenderer = null;
            userInterfaceManager = null;
        }
        public void InitializeServices()
        {
            services                = this;
            materialManager         = new MaterialManager();
            renderer                = new Renderer();
            sceneManager            = new SceneManager();
            textRenderer            = new TextRenderer();
            userInterfaceManager    = new UserInterfaceManager();

            if(materialManager != null) materialManager.Connect(renderer, textRenderer);
            if(sceneManager != null)    sceneManager.Connect(materialManager);
            if(textRenderer != null)    textRenderer.Connect(this, renderer);
            if(userInterfaceManager != null) userInterfaceManager.Connect(materialManager, renderer, sceneManager, textRenderer, this);

            HashSet<Service> uninitialized = new HashSet<Service>(servicesSet);
            HashSet<Service> removeSet = new HashSet<Service>();
            int count = uninitialized.Count;
            while(uninitialized.Count > 0)
            {
                removeSet.Clear();
                foreach(var service in uninitialized)
                {
                    if(service.Dependencies.Count == 0)
                    {
                        string message =
                            "Initializing " + service.Name 
                            + " (" + count + " left)";
                        Trace.TraceInformation(message);
                        if(textRenderer != null && textRenderer.IsInitialized)
                        {
                            textRenderer.Message(message);
                        }
                        service.Initialize();
                        removeSet.Add(service);
                        --count;
                    }
                }
                if(removeSet.Count == 0)
                {
                    Trace.TraceError("Circular Service Dependencies Detected");
                    foreach(var service in uninitialized)
                    {
                        Trace.TraceInformation(service.ToString());
                    }
                    //Debugger.Break(); This does not work right yet with Monodevelop
                    throw new System.InvalidOperationException();
                }
                uninitialized.ExceptWith(removeSet);
                foreach(var other in uninitialized)
                {
                    other.Dependencies.ExceptWith(removeSet);
                }
            }
        }
    }

}
