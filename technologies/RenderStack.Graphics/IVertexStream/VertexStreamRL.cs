//#define LOG_BINDINGS

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using VertexAttribPointerType   = OpenTK.Graphics.OpenGL.VertexAttribPointerType;
using VertexAttribIPointerType  = OpenTK.Graphics.OpenGL.VertexAttribIPointerType;

namespace RenderStack.Graphics
{
    public class VertexStreamRL : IVertexStream, IDisposable
    {
        private List<AttributeBinding>  bindings = new List<AttributeBinding>();
        private bool                    dirty    = true;

        public List<AttributeBinding>   Bindings { get { return Bindings; } }
        public bool                     Dirty    { get { return dirty; } set { dirty = value; } }

        public void Dispose()
        {
        }

        public void Use()
        {
            throw new NotImplementedException();
        }
        public void SetupAttributePointers()
        {
            foreach(AttributeBinding binding in bindings)
            {
                RL.VertexAttribBuffer(
                    binding.Slot,
                    binding.Size,
                    (Caustic.OpenRL.VertexAttribType)binding.Type,
                    binding.Normalized,
                    binding.Stride,
                    binding.Offset
                );
            }
        }
        public void DisableAttributes()
        {
        }
        public AttributeBinding Add(AttributeMapping mapping, Attribute attribute, int stride)
        {
            var binding = new AttributeBinding(mapping, attribute, stride);
            bindings.Add(binding);
            return binding;
        }
        public AttributeBinding Add(AttributeMapping mapping, Attribute attribute, int stride, int slot)
        {
            var binding = new AttributeBinding(mapping, attribute, stride, slot);
            bindings.Add(binding);
            return binding;
        }
        public void Clear()
        {
            bindings.Clear();
        }
    }

}
