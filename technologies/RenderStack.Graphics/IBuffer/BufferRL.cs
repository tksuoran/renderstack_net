#if true
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

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    /// \brief Holds GL buffer object name for Buffer. Allows GL object garbage collection thread to function without GL context.
    /// \note Mostly stable.
    public class BufferRLGhost : IDisposable
    {
        RLbuffer bufferObject;
        public BufferRLGhost(RLbuffer bufferObject)
        {
            this.bufferObject = bufferObject;
        }
        public void Dispose()
        {
            if(bufferObject != IntPtr.Zero)
            {
                RL.DeleteBuffers(ref bufferObject);
                GhostManager.Delete();
                bufferObject = IntPtr.Zero;
            }
        }
    }

    /// \brief Abstraction for OpenGL buffer objects. Currently supports vertex and index buffers.
    /// 
    /// \note Current functionality is somewhat stable. New functionality may need introduce some changes.
    public class BufferRL : IBuffer, IDisposable
    {
        private List<BufferRangeRL>     bufferRanges = new List<BufferRangeRL>();
        private int                     size;
        private BufferTarget            bufferTarget;
        private BufferUsageHint         bufferUsageHint;
        private DrawElementsType        drawElementsType;   /*  index type  */ 
        private VertexFormat            vertexFormat;
        private RLbuffer                bufferObject        = IntPtr.Zero;

        public  UInt32                  BufferObjectGL      { get { throw new InvalidOperationException(); } }
        public  IntPtr                  BufferObjectRL      { get { return bufferObject; } }

        public  OpenTK.Graphics.OpenGL.BufferTarget
                                        BufferTargetGL      { get { return (OpenTK.Graphics.OpenGL.BufferTarget)bufferTarget; } }
        public  OpenTK.Graphics.OpenGL.DrawElementsType
                                        DrawElementsTypeGL  { get { return (OpenTK.Graphics.OpenGL.DrawElementsType)drawElementsType; } }
        public  VertexFormat            VertexFormat        { get { return vertexFormat; } }
        public  RLbuffer                BufferObject        { get { return bufferObject; } }

        [NonSerialized]
        private VertexStreamGL[] vertexStreamsGL = new VertexStreamGL[Configuration.maxAttributeMappings];
        [NonSerialized]
        private Dictionary<IProgram, VertexStreamRL> vertexStreamsRL = new Dictionary<IProgram,VertexStreamRL>();

        public void BindRange(int bindingPoint, IntPtr offsetBytes, IntPtr size)
        {
            throw new InvalidOperationException();
        }

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
        public VertexStreamGL VertexStreamGL(AttributeMappings mappings)
        {
            int index = mappings.InstanceIndex;
            var bindings0 = vertexStreamsGL[index];
            if(bindings0 == null)
            {
                var bindings = vertexStreamsGL[index] = new VertexStreamGL();
                mappings.BindAttributes(
                    bindings, 
                    vertexFormat
                );
                return bindings;
            }
            else
            {
                return bindings0;
            }
        }

        #region Dispose
        bool disposed;
        ~BufferRL()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(bufferObject != IntPtr.Zero)
                {
                    GhostManager.Add(new BufferRLGhost(bufferObject));
                    bufferObject = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }
        #endregion Dispose

        public void RLGen()
        {
            RL.GenBuffers(1, out bufferObject);
            if(bufferObject == IntPtr.Zero)
            {
                throw new System.InvalidOperationException();
            }
#if DEBUG_BUFFER_OBJECTS
            System.Diagnostics.Debug.WriteLine("RL.GenBuffers(1) = " + bufferObject.ToString("X"));
#endif
            GhostManager.Gen();
        }

        public void Add(BufferRangeRL bufferRange)
        {
            lock(this)
            {
                if(bufferRange is BufferRangeRL)
                {
                    bufferRanges.Add((BufferRangeRL)bufferRange);
                }
                else
                {
                    throw new ArgumentException("bufferRange does not match buffer - expecting OpenRL buffer range");
                }
            }
        }

        public IBufferRange CreateIndexBufferRange(OpenTK.Graphics.OpenGL.BeginMode beginMode)
        {
            return new BufferRangeRL(this, (BeginMode)beginMode);
        }
        public IBufferRange CreateVertexBufferRange()
        {
            return new BufferRangeRL(this);
        }
        public IBufferRange CreateUniformBufferRange(IUniformBlock uniformBlock)
        {
            return new BufferRangeRL(this, uniformBlock);
        }

        internal BufferRL(BufferTarget target, BufferUsageHint usageHint)
        {
            bufferTarget = target;
            bufferUsageHint = usageHint;
            this.vertexFormat = null;
        }
        internal BufferRL(VertexFormat vertexFormat, BufferUsageHint usageHint)
        {
            bufferTarget = Caustic.OpenRL.BufferTarget.ArrayBuffer;
            bufferUsageHint = usageHint;
            this.vertexFormat = vertexFormat;
        }
        internal BufferRL(DrawElementsType indexType, BufferUsageHint usageHint)
        {
            bufferTarget = Caustic.OpenRL.BufferTarget.ElementArrayBuffer;
            bufferUsageHint = usageHint;
            drawElementsType = indexType;
        }

        public void UpdateGL()
        {
            throw new InvalidOperationException();
        }
        public void UpdateRL()
        {
            UpdateAll();
        }

        //  \brief Copies all buffer ranges to buffer
        public void UpdateAll()
        {
            lock(this)
            {
                int newSize = 0;
                bool sizeChanged = false;
                foreach(var bufferRange in bufferRanges)
                {
                    newSize += (int)bufferRange.Size;
                }
                Use();
#if DEBUG_BUFFER_OBJECTS
                System.Diagnostics.Debug.WriteLine("Buffer.UpdateAll() " + bufferObject + " with " + bufferRanges.Count.ToString() + " BufferRanges, BufferObject = " + BufferObject + ", new size = " + newSize +  ", old size = " + size);
#endif
                if(newSize != size)
                {
#if DEBUG_BUFFER_OBJECTS
                    //Debug.WriteLine("Buffer.UpdateAll() " + bufferObject + " with " + bufferRanges.Count.ToString() + " BufferRanges, BufferObject = " + BufferObject +  " new size = " + newSize +  " old size + " + size);
                    System.Console.WriteLine("BufferRL.UpdateAll() " + bufferObject.ToString("X") + " with " + bufferRanges.Count.ToString() + " BufferRanges, new size = " + newSize + " old size " + size);
#endif
                    size = newSize;
                    sizeChanged = true;
                    RL.BufferData(
                        bufferTarget,
                        size,
                        IntPtr.Zero,
                        bufferUsageHint
                    );
                }

                //long align = 0xf;
                int offset = 0;

#if DEBUG_BUFFER_OBJECTS
                Debug.WriteLine("Buffer.UpdateAll() with " + bufferRanges.Count.ToString() + " BufferRanges, BufferObject = " + BufferObject + " " + bufferTarget.ToString());
#endif

                //  Collect all buffer range data to a single MemoryStream
                foreach(var bufferRange in bufferRanges)
                {
                    //  Write buffer range to memory stream
                    bufferRange.WriteTo(offset, sizeChanged);

                    //  EXtra alignment just in case
                    offset += (int)bufferRange.Size;
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
            throw new InvalidOperationException();
        }
        public void UseRL()
        {
            Use();
        }

        public void Use()
        {
            if(bufferObject == IntPtr.Zero)
            {
                RLGen();
            }
#if DEBUG_BUFFER_OBJECTS
            System.Diagnostics.Trace.WriteLine("Buffer.Use() " + bufferObject + " " + bufferTarget.ToString());
#endif
            if(bufferObject == IntPtr.Zero)
            {
                throw new System.InvalidOperationException();
            }
            RL.BindBuffer(bufferTarget, bufferObject);
#if DEBUG_BUFFER_OBJECTS
            //System.Diagnostics.Trace.WriteLine("GL.BindBuffer(" + BufferTarget.ToString() + ", " + bufferObject + ");");
#endif
        }

    }
}
#endif