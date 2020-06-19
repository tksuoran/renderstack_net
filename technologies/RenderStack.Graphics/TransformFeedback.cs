#if false  // implementation deferred
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

using OpenTK.Graphics.OpenGL;

using RenderStack.Math;

namespace RenderStack.Graphics
{
    public class TransformFeedback
    {
        private UInt32      transformFeedbackObject = UInt32.MaxValue;
        private Program     program;
        private BufferRange bufferRange;
        
        public TransformFeedback(Program program, BufferRange bufferRange)
        {
            this.program = program;
            this.bufferRange = bufferRange;
        }

        public void GLGen()
        {
            if(OpenTK.Graphics.GraphicsContext.CurrentContext != null)
            {
                GL.GenTransformFeedback(1, out transformFeedbackObject);
                if(transformFeedbackObject == 0)
                {
                    throw new System.InvalidOperationException();
                }
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("GL.GenTransformFeedback(1) skipped - OpenTK.Graphics.GraphicsContext.CurrentContext == null");
            }
        }

        public void Use()
        {
            GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, transformFeedbackObject);

            foreach(var attribute in program.Attributes)
            {
                attribute.
            }
            foreach(var mapping in Program.AttributeMappings.Mappings)
            {
                GL.BindAttribLocation(ProgramObject, mapping.Slot, mapping.Name);
            }
        }

    }

}

#endif