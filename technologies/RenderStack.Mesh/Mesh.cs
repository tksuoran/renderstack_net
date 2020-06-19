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
using System.Runtime.Serialization;
using System.Collections.Generic;
using RenderStack.Graphics;

using BeginMode         = OpenTK.Graphics.OpenGL.BeginMode;
using BufferTarget      = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint   = OpenTK.Graphics.OpenGL.BufferUsageHint;
using DrawElementsType  = OpenTK.Graphics.OpenGL.DrawElementsType;
using GL                = OpenTK.Graphics.OpenGL.GL;

namespace RenderStack.Mesh
{
    [Serializable]
    /// \brief Basic renderable shape, vertex buffer and index buffer. Can also have multiple representations, see MeshMode.
    /// \note Mostly stable, somewhat experimental.
    public class Mesh : IMeshSource
    {
        Mesh    IMeshSource.GetMesh { get { return this; } }

        private System.Diagnostics.StackTrace constructorStackTrace;

        public Mesh()
        {
            constructorStackTrace = new System.Diagnostics.StackTrace(true);
        }

        private string          name;
        private IBufferRange[]  indexBufferRanges = new IBufferRange[(int)(MeshMode.Count)];

        public  string          Name                { get { return name; } set { name = value; } }
        public  IBufferRange    VertexBufferRange   { get; set; } // \todo use constructor to set VertexBuffer?

        public IBufferRange IndexBufferRange(MeshMode meshMode)
        {
            return indexBufferRanges[(int)meshMode];
        }
        public bool HasIndexBufferRange(MeshMode meshMode)
        {
            if(meshMode == MeshMode.NotSet) return false;
            return indexBufferRanges[(int)meshMode] != null;
        }
        public IBufferRange FindOrCreateIndexBufferRange(
            MeshMode    meshMode, 
            IBuffer     buffer, 
            BeginMode   beginMode
        )
        {
            if(HasIndexBufferRange(meshMode) == false)
            {
                var indexBufferRange = buffer.CreateIndexBufferRange(beginMode);
                indexBufferRanges[(int)meshMode] = indexBufferRange;

                return indexBufferRange;
            }
            else
            {
                var indexBufferRange = indexBufferRanges[(int)meshMode];
                return indexBufferRanges[(int)meshMode];
            }
        }

    }
}
