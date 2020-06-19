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
using System.Collections.Generic;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// GhostManager supports C# garbage collection, which likely runs in a thread without a GL
    /// context. Gargabe collecting RenderStack objects that hold OpenGL object names will push
    /// a ghost to GhostManager. GhostManager can be called from a thread with GL context to
    /// delete the GL objects, for example once per frame.
    /// 
    /// \note Mostly stable.
    public class GhostManager
    {
        private static List<IDisposable> ghosts = new List<IDisposable>();
        private static int genCounter = 0;
        private static int deleteCounter = 0;
        public static int GenCount { get { return genCounter; } }
        public static int DeleteCount { get { return deleteCounter; } }
        public static void Gen()
        {
            lock(ghosts)
            {
                genCounter++;
            }
        }
        public static void Delete()
        {
            lock(ghosts)
            {
                deleteCounter++;
            }
        }
        public static void Add(IDisposable ghost)
        {
            lock(ghosts)
            {
                ghosts.Add(ghost);
            }
        }

        public static void Process()
        {
            lock(ghosts)
            {
                if(ghosts.Count == 0)
                {
                    Debug.WriteLine("----- No ghosts to delete -----");
                    return;
                }
                Debug.WriteLine("----- Ghosts to delete: " + ghosts.Count + " -----");
                foreach(var ghost in ghosts)
                {
                    ghost.Dispose();
                }
                ghosts.Clear();
                Debug.WriteLine("----- Ghosts deleted -----");
            }
        }
        public static void CheckAllDeleted()
        {
            if(genCounter != deleteCounter)
            {
                //Debugger.Break(); This does not work right yet with Monodevelop
                throw new OpenTK.GraphicsException("Ghost resource synchronization fail");
            }
        }
    }
}
