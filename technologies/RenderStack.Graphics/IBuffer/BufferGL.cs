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

// #define DEBUG_BUFFER_OBJECTS

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UInt16  = System.UInt16;
using UInt32  = System.UInt32;

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    /// \brief Holds GL buffer object name for Buffer. Allows GL object garbage collection thread to function without GL context.
    /// \note Mostly stable.
    public class BufferGhost : IDisposable
    {
        UInt32 bufferObject;
        public BufferGhost(UInt32 bufferObject)
        {
            this.bufferObject = bufferObject;
        }
        public void Dispose()
        {
            if(bufferObject != UInt32.MaxValue)
            {
                GL.DeleteBuffers(1, ref bufferObject);
                GhostManager.Delete();
                bufferObject = UInt32.MaxValue;
            }
        }
    }

    [Serializable]
    /// \brief Abstraction for OpenGL buffer objects. Currently supports vertex and index buffers.
    /// 
    /// \note Current functionality is somewhat stable. New functionality may need introduce some changes.
    public class BufferGL : IBuffer, IDisposable
    {
        private List<BufferRangeGL>     bufferRanges = new List<BufferRangeGL>();
        private long                    size;
        private BufferTarget            bufferTarget;
        private BufferUsageHint         bufferUsageHint;
        private DrawElementsType        drawElementsType;   /*  index type  */ 
        private VertexFormat            vertexFormat;

        [NonSerialized]
        private UInt32                  bufferObject        = UInt32.MaxValue;
        [NonSerialized]
        private static List<BufferGL>   deserializations    = new List<BufferGL>();
        [NonSerialized]
        private static List<BufferGL>   delayedUpdates      = new List<BufferGL>();

        public  BufferTarget            BufferTargetGL      { get { return bufferTarget; } }
        public  DrawElementsType        DrawElementsTypeGL  { get { return drawElementsType; } }
        public  VertexFormat            VertexFormat        { get { return vertexFormat; } }

        // \todo figure out max size
        [NonSerialized]
        private VertexStreamGL[] vertexStreamsGL = new VertexStreamGL[Configuration.maxAttributeMappings];
#if false
        [NonSerialized]
        private Dictionary<IProgram, VertexStreamRL> vertexStreamsRL = new Dictionary<IProgram,VertexStreamRL>();
#endif

        public void BindRange(int bindingPoint, IntPtr offsetBytes, IntPtr size)
        {
            GL.BindBufferRange(
                bufferTarget,
                bindingPoint,
                (int)bufferObject, 
                offsetBytes, 
                size
            );
        }

#if false
        public VertexStreamRL VertexStreamRL(IProgram program)
        {
            if(vertexStreamsRL.ContainsKey(program))
            {
                return vertexStreamsRL[program];
            }
            var bindings = vertexStreamsRL[program] = new VertexStreamRL();
            var mappings = program.AttributeMappings;
            mappings.BindAttributes(bindings, program, vertexFormat);
            return bindings;
        }
#endif
        public VertexStreamGL VertexStreamGL(AttributeMappings mappings)
        {
            int index = mappings.InstanceIndex;
            var existingStream = vertexStreamsGL[index];
            if(existingStream == null)
            {
                var stream = vertexStreamsGL[index] = new VertexStreamGL();
                mappings.BindAttributes(
                    stream, 
                    vertexFormat
                );
                return stream;
            }
            else
            {
                return existingStream;
            }
        }

        #region Serialization
        [OnDeserialized]
        //  After we have deserialized the buffer, queue buffer for GL upload
        //  This can happen in any thread
        internal void OnDeserialized(StreamingContext context)
        {
            lock(deserializations)
            {
                bufferObject = UInt32.MaxValue;
                deserializations.Add(this);
            }
        }

        //  Call this after deserializations
        public static void FinalizeDeserializations()
        {
            lock(deserializations)
            {
                foreach(var buffer in deserializations)
                {
                    buffer.OnDeserializedGraphicsContexThread();
                }
                deserializations.Clear();
            }
        }

        public void OnDeserializedGraphicsContexThread()
        {
            GLGen();
            UpdateAll();
        }
        #endregion Serialization
        #region Dispose
        bool disposed;
        ~BufferGL()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(bufferObject != UInt32.MaxValue)
                {
                    GhostManager.Add(new BufferGhost(bufferObject));
                    bufferObject = UInt32.MaxValue;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }
        #endregion Dispose

        public void GLGen()
        {
            if(OpenTK.Graphics.GraphicsContext.CurrentContext != null)
            {
                GL.GenBuffers(1, out bufferObject);
                if(bufferObject == 0)
                {
                    throw new System.InvalidOperationException();
                }
#if DEBUG_BUFFER_OBJECTS
                System.Diagnostics.Debug.WriteLine("GL.GenBuffers(1) = " + bufferObject);
#endif
                GhostManager.Gen();
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("GL.GenBuffers(1) skipped - OpenTK.Graphics.GraphicsContext.CurrentContext == null");
            }
        }

        public bool Match(IBuffer other)
        {
            if(BufferTargetGL != other.BufferTargetGL)
            {
                return false;
            }
            switch(BufferTargetGL)
            {
                //  Vertex Buffer
                case BufferTarget.ArrayBuffer:
                {
                    if(VertexFormat != other.VertexFormat)
                    {
                        return false;
                    }
                    break;
                }
                //  Index Buffer
                case BufferTarget.ElementArrayBuffer:
                {
                    if(DrawElementsTypeGL != other.DrawElementsTypeGL)
                    {
                        return false;
                    }
                    break;
                }
                //  UniformBuffer
                case BufferTarget.UniformBuffer:
                {
                    break;
                }
            }
            return true;
        }
        private void Add(BufferRangeGL bufferRange)
        {
            lock(this)
            {
                if(bufferRange is BufferRangeGL)
                {
                    bufferRanges.Add((BufferRangeGL)bufferRange);
                }
                else
                {
                    throw new ArgumentException("bufferRange does not match buffer - expecting GL3 buffer range");
                }
            }
        }

        internal BufferGL(BufferTarget target, BufferUsageHint usageHint)
        {
            bufferTarget = target;
            bufferUsageHint = usageHint;
            this.vertexFormat = null;
        }
        internal BufferGL(VertexFormat vertexFormat, BufferUsageHint usageHint)
        {
            bufferTarget = BufferTarget.ArrayBuffer;
            bufferUsageHint = usageHint;
            this.vertexFormat = vertexFormat;
        }
        internal BufferGL(DrawElementsType indexType, BufferUsageHint usageHint)
        {
            bufferTarget = BufferTarget.ElementArrayBuffer;
            bufferUsageHint = usageHint;
            drawElementsType = indexType;
        }

        public IBufferRange CreateIndexBufferRange(BeginMode beginMode)
        {
            var r = new BufferRangeGL(this, beginMode);
            Add(r);
            return r;
        }
        public IBufferRange CreateVertexBufferRange()
        {
            var r = new BufferRangeGL(this);
            Add(r);
            return r;
        }
        public IBufferRange CreateUniformBufferRange(IUniformBlock uniformBlock)
        {
            var r = new BufferRangeGL(this, uniformBlock);
            Add(r);
            return r;
        }

        public static void PerformDelayedUpdates()
        {
            lock(delayedUpdates)
            {
                GL.BindVertexArray(Configuration.DefaultVAO);
                foreach(var buffer in delayedUpdates)
                {
                    buffer.GLGen();
                    buffer.UpdateAll();
                }
                delayedUpdates.Clear();
            }
        }

        public void UpdateGL()
        {
            UpdateAll();
        }
        public void UpdateRL()
        {
            throw new InvalidOperationException();
        }
        public void UpdateAll()
        {
            lock(this)
            {
                if(OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                {
#if DEBUG_BUFFER_OBJECTS
                    System.Diagnostics.Debug.WriteLine("Buffer.UpdateAll() - delayed");
#endif
                    lock(delayedUpdates)
                    {
                        delayedUpdates.Add(this);
                        return;
                    }
                }

                long newSize = 0;
                bool sizeChanged = false;
                foreach(var bufferRange in bufferRanges)
                {
                    newSize += bufferRange.Size;
                }
                UseGL();
#if DEBUG_BUFFER_OBJECTS
                System.Diagnostics.Debug.WriteLine("Buffer.UpdateAll() " + bufferObject + " with " + bufferRanges.Count.ToString() + " BufferRanges, BufferObject = " + BufferObject + ", new size = " + newSize +  ", old size = " + size);
#endif
                if(newSize != size)
                {
#if DEBUG_BUFFER_OBJECTS
                    //Debug.WriteLine("Buffer.UpdateAll() " + bufferObject + " with " + bufferRanges.Count.ToString() + " BufferRanges, BufferObject = " + BufferObject +  " new size = " + newSize +  " old size + " + size);
#endif
                    size = newSize;
                    sizeChanged = true;
                    GL.BufferData(
                        BufferTargetGL,
                        (System.IntPtr)(size),
                        System.IntPtr.Zero,
                        bufferUsageHint
                    );
                }

                //long align = 0xf;
                long offset = 0;

#if DEBUG_BUFFER_OBJECTS
                Debug.WriteLine("Buffer.UpdateAll() with " + bufferRanges.Count.ToString() + " BufferRanges, BufferObject = " + BufferObject + " " + bufferTarget.ToString());
#endif

                //  Collect all buffer range data to a single MemoryStream
                foreach(var bufferRange in bufferRanges)
                {
                    //  Write buffer range to memory stream
                    bufferRange.WriteTo(offset, sizeChanged);

                    //  EXtra alignment just in case
                    offset += bufferRange.Size;
                    //long padding = (-size) & (align - 1);  //  = align - (size & (align - 1))
                    //size = (size + align - 1) & ~(align - 1);
                    //memoryStream.Seek(size, SeekOrigin.Begin);
                    //Debug.WriteLine("Padding " + padding.ToString());
                }
            }

#if DEBUG_BUFFER_OBJECTS
            System.Diagnostics.Debug.WriteLine("Buffer.UpdateAll() " + bufferObject + " total size " + size);
#endif
        }

        public void UseGL()
        {
            if(bufferObject == UInt32.MaxValue)
            {
                GLGen();
            }
#if DEBUG_BUFFER_OBJECTS
            System.Diagnostics.Trace.WriteLine("Buffer.Use() " + bufferObject + " " + bufferTarget.ToString());
#endif
            if(bufferObject == 0)
            {
                throw new System.InvalidOperationException();
            }
            GL.BindBuffer(BufferTargetGL, bufferObject);
#if DEBUG_BUFFER_OBJECTS
            //System.Diagnostics.Trace.WriteLine("GL.BindBuffer(" + BufferTarget.ToString() + ", " + bufferObject + ");");
#endif
        }
        public void UseRL()
        {
            throw new InvalidOperationException();
        }
    }
}
