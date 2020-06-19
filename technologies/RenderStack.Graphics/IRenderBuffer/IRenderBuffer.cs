using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    public interface IRenderBuffer : IDisposable
    {
        RenderStack.Math.Viewport   Viewport        { get; }
        RenderbufferStorage         InternalFormat  { get; }
        int                         SampleCount     { get; }

        void Resize(int width, int height);
    }
    public class RenderBufferFactory
    {
        public static IRenderBuffer Create(int width, int height, RenderbufferStorage internalFormat, int sampleCount)
        {
            if(Configuration.useGl1)
            {
                return (IRenderBuffer)new RenderBufferGL1(width, height, internalFormat, sampleCount);
            }
            return (IRenderBuffer)new RenderBufferGL3(width, height, internalFormat, sampleCount);
        }
    }
}
