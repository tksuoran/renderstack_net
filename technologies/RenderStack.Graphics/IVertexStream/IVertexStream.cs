//#define LOG_BINDINGS

using System;
using System.Collections.Generic;

namespace RenderStack.Graphics
{

    public interface IVertexStream : IDisposable
    {
        //Int32                    VertexArrayObject  { get; }
        List<AttributeBinding>   Bindings           { get; }
        bool                     Dirty              { get; set; }

        void                Use();
        void                SetupAttributePointers();
        void                DisableAttributes();
        AttributeBinding    Add(AttributeMapping mapping, Attribute attribute, int stride);
        void                Clear();
    }

}
