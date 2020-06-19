using System;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace RenderStack.Graphics
{
    public class Debug
    {
        public static void FrameTerminator()
        {
            if(Configuration.canUseFrameTerminator)
            {
                GL.Gremedy.FrameTerminator();
            }
        }
        public static void WriteLine(string text)
        {
            //System.Diagnostics.Debug.WriteLine(text);
            if(Configuration.canUseStringMarker)
            {
                byte[] strbuf = Encoding.ASCII.GetBytes(text);
                IntPtr buffer = Marshal.AllocHGlobal(strbuf.Length + 1); 
                Marshal.Copy(strbuf, 0, buffer, strbuf.Length);
                Marshal.WriteByte(buffer, strbuf.Length, 0);
                GL.Gremedy.StringMarker(0, buffer);
                Marshal.FreeHGlobal(buffer);
            }
            //System.Diagnostics.Trace.WriteLine(text);
        }
    }
}
