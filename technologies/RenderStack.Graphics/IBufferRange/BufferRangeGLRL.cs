﻿// #define DEBUG_BUFFER_OBJECTS
// #define DEBUG_UNIFORM_BUFFER

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
    [Serializable]
    public class BufferRangeGLRL : IBufferRange
    {
        private BufferRangeGL   bufferRangeGL;
        private BufferRangeRL   bufferRangeRL;

        public  string              Name                { get { return bufferRangeGL.Name; } set { bufferRangeGL.Name = value; } }
        public  DrawElementsType    DrawElementsTypeGL  { get { return bufferRangeGL.BufferGL.DrawElementsTypeGL; } }
        public  BufferTarget        BufferTargetGL      { get { return bufferRangeGL.BufferGL.BufferTargetGL; } }
        public  VertexFormat        VertexFormat        { get { return bufferRangeGL.BufferGL.VertexFormat; } }
        public  BufferGL            BufferGL            { get { return bufferRangeGL.BufferGL; } }
        public  UInt32              Count               { get { return bufferRangeGL.Count; } }
        public  BeginMode           BeginMode           { get { return bufferRangeGL.BeginMode; } }
        public  long                Size                { get { return bufferRangeGL.Size; } }
        public  IUniformBlock       UniformBlock        { get { return bufferRangeGL.UniformBlock; } }
        public  long                OffsetBytes         { get { return bufferRangeGL.OffsetBytes; } }
        public  int                 BaseVertex          { get { return bufferRangeGL.BaseVertex; } }
        public  bool                NeedsUploadGL       { get { return bufferRangeGL.NeedsUploadGL; } }
        public  bool                NeedsUploadRL       { get { return bufferRangeRL.NeedsUploadRL; } }

        public VertexStreamRL VertexStreamRL(IProgram program)
        {
            return bufferRangeRL.VertexStreamRL(program);
        }
        public VertexStreamGL VertexStreamGL(AttributeMappings mappings)
        {
            return bufferRangeGL.VertexStreamGL(mappings);
        }

        public void UpdateAll()
        {
            bufferRangeGL.UpdateAll();
            bufferRangeRL.UpdateAll();
        }
        public void UpdateGL()
        {
            bufferRangeGL.UpdateGL();
        }
        public void UpdateRL()
        {
            bufferRangeRL.UpdateRL();
        }
        public bool Match(IBufferRange other)
        {
            return bufferRangeGL.Match(other);
        }

        public void UseGL()
        {
            bufferRangeGL.UseGL();
        }
        public void UseRL()
        {
            bufferRangeRL.UseRL();
        }
        public BufferRL BufferRL { get { return bufferRangeRL.BufferRL; } }

        internal BufferRangeGLRL(BufferRangeGL gl, BufferRangeRL rl)
        {
            bufferRangeGL = gl;
            bufferRangeRL = rl;
        }

        public void Allocate(int size)
        {
            bufferRangeGL.Allocate(size);
            bufferRangeRL.Allocate(size);
        }

        public void UseUniformBufferGL()
        {
            bufferRangeGL.UseUniformBufferGL();
        }
        public void UseUniformBufferRL()
        {
            bufferRangeRL.UseUniformBufferRL();
        }

        public void WriteTo(long offset, bool force)
        {
            bufferRangeGL.WriteTo(offset, force);
            bufferRangeRL.WriteTo((int)offset, force);
        }
        public void Touch(uint count, byte[] data)
        {
            bufferRangeGL.Touch(count, data);
            bufferRangeRL.Touch(count, data);
        }

        public void Floats(int offset, Floats param)
        {
            bufferRangeGL.Floats(offset, param);
            bufferRangeRL.Floats(offset, param);
        }
        public void Ints(int offset, Ints param)
        {
            bufferRangeGL.Ints(offset, param);
            bufferRangeRL.Ints(offset, param);
        }
        public void UInts(int offset, UInts param)
        {
            bufferRangeGL.UInts(offset, param);
            bufferRangeRL.UInts(offset, param);
        }
    }
}
