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

// #define DEBUG_UNIFORM_BUFFER

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using RenderStack.Graphics;

namespace RenderStack.Graphics
{
    public class UniformBufferRL : IUniformBuffer
    {
        private UniformBufferData   data;
        private IBufferRange        bufferRange;
        private Callback            syncDelegate;

        public Callback SyncDelegate { get { return syncDelegate; } set { syncDelegate = value; } }
        public bool     Contains(string key) { return data.Parameters.ContainsKey(key); }
        public Floats   Floats  (string key) { return data.Floats(key);   }
        public Ints     Ints    (string key) { return data.Ints(key);     }
        public UInts    UInts   (string key) { return data.UInts(key);    }

        public UniformBufferRL(IUniformBlock uniformBlock)
        {
            data = new UniformBufferData(uniformBlock);

            IBuffer buffer = BufferPool.Instance.GetUniformBufferRL(OpenTK.Graphics.OpenGL.BufferUsageHint.DynamicDraw);
            bufferRange = buffer.CreateUniformBufferRange(uniformBlock);
            bufferRange.Name = uniformBlock.Name;

            Sync();
        }

        public void Use()
        {
            throw new System.InvalidOperationException();
        }
        public void UseRL(ProgramRL program)
        {
            int blockIndex = program.UniformBlocks[data.UniformBlock.Name];
            RL.UniformBlockBuffer(blockIndex, bufferRange.BufferRL.BufferObjectRL);
        /* \todo
            RL.UniformBlockBuffer(
                0, 
                bufferRange.BufferRL.BufferObjectRL
            );
         * */
        }

        public void Sync()
        {
            data.Sync(bufferRange);
        }
    }
}
