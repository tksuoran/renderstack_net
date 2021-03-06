﻿//#define LOG_BINDINGS

using System;
using System.Collections.Generic;
using GL = OpenTK.Graphics.OpenGL.GL;
using OpenGL = OpenTK.Graphics.OpenGL;
using Int32 = System.Int32;
using VertexAttribPointerType   = OpenTK.Graphics.OpenGL.VertexAttribPointerType;
using VertexAttribIPointerType  = OpenTK.Graphics.OpenGL.VertexAttribIPointerType;

namespace RenderStack.Graphics
{
    //  \brief Collection of uniform bindings
    //  \todo Ghost
    public class VertexStreamGL : IVertexStream, IDisposable
    {
        // GL3
        private Int32                   vertexArrayObject   = Int32.MaxValue;
        private List<AttributeBinding>  bindings            = new List<AttributeBinding>();
        private bool                    dirty               = true;

        public Int32                    VertexArrayObject   { get { return vertexArrayObject; } }
        public List<AttributeBinding>   Bindings            { get { return Bindings; } }
        public bool                     Dirty               { get { return dirty; } set { dirty = value; } }

        public void Dispose()
        {
            if(
                (vertexArrayObject != Int32.MaxValue) &&
                (
                    Configuration.mustUseVertexArrayObject || 
                    (
                        Configuration.canUseVertexArrayObject && 
                        Configuration.useVertexArrayObject
                    )
                )
            )
            {
                GL.DeleteVertexArrays(1, ref vertexArrayObject);
                vertexArrayObject = Int32.MaxValue;
            }
        }

        public VertexStreamGL()
        {
            if(
                Configuration.mustUseVertexArrayObject || 
                (
                    Configuration.canUseVertexArrayObject && 
                    Configuration.useVertexArrayObject
                )
            )
            {
                GL.GenVertexArrays(1, out vertexArrayObject);
            }
        }

        public void Use()
        {
            GL.BindVertexArray(vertexArrayObject);
        }
        public void SetupAttributePointers()
        {
            if(Configuration.useGl1)
            {
                SetupAttributePointersOld();
            }
            else
            {
                SetupAttributePointersNew();
            }
        }
        public void SetupAttributePointersOld()
        {
            foreach(var binding in bindings)
            {
                var attribute = binding.Attribute;
                switch(binding.AttributeMapping.DstUsage)
                {
                    case VertexUsage.Position:
                    {
                        GL.EnableClientState(OpenGL.ArrayCap.VertexArray);
                        GL.VertexPointer(
                            attribute.Dimension, 
                            (OpenGL.VertexPointerType)attribute.Type, 
                            binding.Stride, 
                            attribute.Offset
                        );
                        break;
                    }
                    case VertexUsage.Normal:
                    {
                        GL.EnableClientState(OpenGL.ArrayCap.NormalArray);
                        GL.NormalPointer(
                            (OpenGL.NormalPointerType)attribute.Type,
                            binding.Stride,
                            attribute.Offset
                        );
                        break;
                    }
                    case VertexUsage.Color:
                    {
                        if((attribute.Dimension != 3) && (attribute.Dimension != 4))
                        {
                            //  \todo
                            continue;
                        }
                        GL.EnableClientState(OpenGL.ArrayCap.ColorArray);
                        GL.ColorPointer(
                            attribute.Dimension, 
                            (OpenGL.ColorPointerType)attribute.Type, 
                            binding.Stride, 
                            attribute.Offset
                        );
                        break;
                    }
                    case VertexUsage.TexCoord:
                    {
                        GL.ClientActiveTexture(OpenGL.TextureUnit.Texture0 + binding.AttributeMapping.DstIndex);
                        GL.EnableClientState(OpenGL.ArrayCap.TextureCoordArray);
                        GL.TexCoordPointer(
                            attribute.Dimension, 
                            (OpenGL.TexCoordPointerType)attribute.Type, 
                            binding.Stride, 
                            attribute.Offset
                        );
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }
        public void SetupAttributePointersNew()
        {
            foreach(AttributeBinding binding in bindings)
            {
                GL.EnableVertexAttribArray(binding.AttributeMapping.Slot);
                if(
                    (binding.Type == VertexAttribPointerType.Double) ||
                    (binding.Type == VertexAttribPointerType.Float) ||
                    (binding.Type == VertexAttribPointerType.HalfFloat)
                )
                {
                    GL.VertexAttribPointer(
                        binding.AttributeMapping.Slot,
                        binding.Size,
                        binding.Type,
                        binding.Normalized,
                        binding.Stride,
                        (IntPtr)((long)(binding.Offset)/* + (vertexBufferRange.OffsetBytes)*/)
                    );
                }
                else
                {
                    GL.VertexAttribIPointer(
                        binding.AttributeMapping.Slot,
                        binding.Size,
                        (VertexAttribIPointerType)binding.Type,
                        binding.Stride,
                        (IntPtr)((long)(binding.Offset)/* + (vertexBufferRange.OffsetBytes)*/)
                    );
                }
            }
        }
        public void DisableAttributes()
        {
            if(Configuration.useGl1)
            {
                DisableAttributesOld();
            }
            else
            {
                DisableAttributesNew();
            }
        }
        public void DisableAttributesOld()
        {
            foreach(var binding in bindings)
            {
                var attribute = binding.Attribute;
                switch(binding.AttributeMapping.DstUsage)
                {
                    case VertexUsage.Position:
                    {
                        GL.DisableClientState(OpenGL.ArrayCap.VertexArray);
                        break;
                    }
                    case VertexUsage.Normal:
                    {
                        GL.DisableClientState(OpenGL.ArrayCap.NormalArray);
                        break;
                    }
                    case VertexUsage.Color:
                    {
                        GL.DisableClientState(OpenGL.ArrayCap.ColorArray);
                        break;
                    }
                    case VertexUsage.TexCoord:
                    {
                        GL.ClientActiveTexture(OpenGL.TextureUnit.Texture0 + binding.AttributeMapping.DstIndex);
                        GL.DisableClientState(OpenGL.ArrayCap.TextureCoordArray);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }
        public void DisableAttributesNew()
        {
            foreach(AttributeBinding binding in bindings)
            {
                GL.DisableVertexAttribArray(binding.AttributeMapping.Slot);
            }
        }
        public AttributeBinding Add(AttributeMapping mapping, Attribute attribute, int stride)
        {
            var binding = new AttributeBinding(mapping, attribute, stride);
            bindings.Add(binding);
            return binding;
        }
        public void Clear()
        {
            bindings.Clear();
        }
    }

}
