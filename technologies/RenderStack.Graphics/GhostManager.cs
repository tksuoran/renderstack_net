using System;
using System.Collections.Generic;

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
