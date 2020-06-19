#if false  // implementation deferred

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