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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using RenderStack.Graphics;

using BeginMode         = OpenTK.Graphics.OpenGL.BeginMode;
using BufferTarget      = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint   = OpenTK.Graphics.OpenGL.BufferUsageHint;
using DrawElementsType  = OpenTK.Graphics.OpenGL.DrawElementsType;
using GL                = OpenTK.Graphics.OpenGL.GL;
using Buffer            = RenderStack.Graphics.BufferGL;
using Debug             = System.Diagnostics.Debug;

namespace RenderStack.Graphics
{
    public class BufferPool : IDisposable 
    {
        public static BufferPool Instance = new BufferPool();

        private Dictionary<KeyValuePair<VertexFormat, BufferUsageHint>, IBuffer>     vertexBuffers    = new Dictionary<KeyValuePair<VertexFormat,BufferUsageHint>, IBuffer>();
        private Dictionary<KeyValuePair<DrawElementsType, BufferUsageHint>, IBuffer> indexBuffers     = new Dictionary<KeyValuePair<DrawElementsType,BufferUsageHint>, IBuffer>();
        private Dictionary<BufferUsageHint, BufferGL>                                uniformBuffersGL = new Dictionary<BufferUsageHint, BufferGL>();

        public IBuffer GetVertexBuffer(VertexFormat vertexFormat, BufferUsageHint bufferUsageHint)
        {
            if(Configuration.useGl1 || Configuration.useOpenRL)
            {
                //  \todo Do index remapping due to lack on DrawElementsBaseVertex()
                return BufferFactory.Create(vertexFormat, bufferUsageHint);
            }
            //return BufferFactory.Create(vertexFormat, bufferUsageHint);
#if true
            lock(this)
            {
                var kvp = new KeyValuePair<VertexFormat, BufferUsageHint>(vertexFormat, bufferUsageHint);
                if(vertexBuffers.ContainsKey(kvp))
                {
                    //Console.WriteLine("Returning old vertex buffer, vertex format = " + vertexFormat.ToString());
                    if(vertexFormat.Stride != vertexBuffers[kvp].VertexFormat.Stride)
                    {
                        throw new Exception("");
                    }
                    return vertexBuffers[kvp];
                }
                //Console.WriteLine("Creating new vertex buffer, vertex format = " + vertexFormat.ToString());
                var buffer = BufferFactory.Create(vertexFormat, bufferUsageHint);
                vertexBuffers[kvp] = buffer;
                return buffer;
            }
#endif
        }
        public IBuffer GetIndexBuffer(DrawElementsType indexType, BufferUsageHint bufferUsageHint)
        {
            if(RenderStack.Graphics.Configuration.useGl1 || Configuration.useOpenRL)
            {
                //  \todo Do index remapping due to lack on DrawElementsBaseVertex()
                return BufferFactory.Create(indexType, bufferUsageHint);
            }
//            return new Buffer(indexType, bufferUsageHint);
#if true
            lock(this)
            {
                var kvp = new KeyValuePair<DrawElementsType, BufferUsageHint>(indexType, bufferUsageHint);
                if(indexBuffers.ContainsKey(kvp))
                {
                    //Debug.WriteLine("Returning old index buffer, index type = " + indexType.ToString());
                    return indexBuffers[kvp];
                }
                //Debug.WriteLine("Creating new index buffer, index type = " + indexType.ToString());
                var buffer = BufferFactory.Create(indexType, bufferUsageHint);
                indexBuffers[kvp] = buffer;
                return buffer;
            }
#endif
        }
        public BufferGL GetUniformBufferGL(BufferUsageHint usage)
        {
            if(Configuration.useGl1)
            {
                throw new InvalidOperationException("GL1 does not support uniform buffers");
            }
            //return new Buffer(BufferTarget.UniformBuffer, usage);
#if true
            lock(this)
            {
                if(uniformBuffersGL.ContainsKey(usage))
                {
                    //Debug.WriteLine("Returning old uniform buffer, usage = " + usage.ToString());
                    return uniformBuffersGL[usage];
                }
                //Debug.WriteLine("Creating new uniform buffer, usage = " + usage.ToString());
                var buffer = new BufferGL(BufferTarget.UniformBuffer, usage);
                uniformBuffersGL[usage] = buffer;
                return buffer;
            }
#endif
        }
#if false
        public BufferRL GetUniformBufferRL(BufferUsageHint usage)
        {
            //  Current OpenRL API rlUniformBlockBuffer() needs
            //  exactly one buffer object per uniform block
            //  so we can not share the same buffer to multiple
            //  uniform blocks.
            var buffer = new BufferRL(Caustic.OpenRL.BufferTarget.UniformBlockBuffer, (Caustic.OpenRL.BufferUsageHint)usage);
            return buffer;
        }
#endif

        #region IDisposable Members

        public void Dispose()
        {
            foreach(var buffer in vertexBuffers)
            {
                buffer.Value.Dispose();
            }
            vertexBuffers.Clear();
            foreach(var buffer in indexBuffers)
            {
                buffer.Value.Dispose();
            }
            indexBuffers.Clear();
        }

        #endregion
    }
}
