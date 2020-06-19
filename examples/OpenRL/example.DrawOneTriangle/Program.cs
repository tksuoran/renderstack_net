/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
* Copyright(c) 2006-2012 Caustic Graphics, Inc.    All rights reserved.        *
*                                                                              *
* The following program code (the "Caustic Sample Code") is provided only      *
* under license (the "Caustic Graphics - Development License Agreement")       *
* from Caustic Graphics, Inc. It may be modified and/or redistributed only     *
* under the terms of that license. Disclosure of the Software to third         *
* parties, in any form, in whole or in part, is expressly prohibited           *
* except as authorized by that license.                                        *
*                                                                              *
* THE MATERIAL EMBODIED IN THIS SOFTWARE IS PROVIDED TO YOU "AS-IS" AND        *
* WITHOUT ANY REPRESENTATIONS OR WARRANTIES OF ANY KIND, EXPRESS, IMPLIED      *
* BY STATUTE OR OTHERWISE, INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF       *
* MERCHANTABILITY, NONINFRINGEMENT OR FITNESS FOR A PARTICULAR PURPOSE.        *
*                                                                              *
* IN NO EVENT SHALL CAUSTIC GRAPHICS, INC. BE LIABLE TO YOU OR ANYONE ELSE     *
* FOR ANY DAMAGES WHATSOEVER, INCLUDING DIRECT, SPECIAL, INCIDENTAL, INDIRECT  *
* OR CONSEQUENTIAL DAMAGES OF ANY KIND, INCLUDING WITHOUT LIMITATION, LOSS OF  *
* PROFIT, LOSS OF USE, SAVINGS OR REVENUE, OR THE CLAIMS OF THIRD PARTIES,     *
* WHETHER OR NOT CAUSTIC GRAPHICS, INC. HAS BEEN ADVISED OF THE POSSIBILITY OF *
* SUCH LOSS, HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, ARISING OUT OF OR  *
* IN CONNECTION WITH THE POSSESSION, USE OR PERFORMANCE OF THIS SOFTWARE.      *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using Caustic.Glut;
using Caustic.OpenRL;

using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

namespace example.DrawOneTriangle
{
    static class Program
    {
        [System.STAThread]
        static void Main(string[] args)
        {
            Glut.Init(args);
            Glut.InitWindowPosition(100, 100);
            Glut.InitWindowSize(512, 512);
            Glut.InitDisplayMode(WindowFlags.Rgba | WindowFlags.Double);
            Glut.CreateWindow("Draw One Triangle");
            DrawOneTriangleInit();
            Glut.DisplayFunc(DrawOneTriangleFrame);
            Glut.ReshapeFunc(Reshape);
            Glut.KeyboardFunc(Keyboard);
    
            Glut.MainLoop();
        }

        static uint[] indices = { 0, 1, 2 };

        static float[] positions = { 0.0f , 0.0f,
                                             0.9f , 0.0f,
                                             0.0f , 0.9f };

        static RLprimitive trianglePrimitive;

        static RLbuffer positionBuffer;
        static RLbuffer indexBuffer;

        static RLshader vertexShader;
        static RLshader rayShader;
        static RLprogram triangleProgram;

        static RLshader frameShader;
        static RLprogram frameProgram;

        static RLshader compileShader(string source, ShaderType shaderType, string shaderDescription)
        {
            int status;
            RLshader shader;

            //source = source.Replace("\r\n","\n");
            string[] aStrings = { source };
            shader = RL.CreateShader(shaderType); 
            RL.ShaderString(shader, ShaderStringParameter.ShaderName, shaderDescription);
            RL.ShaderSource (shader, aStrings);
            RL.CompileShader(shader);
            RL.GetShader    (shader, ShaderParameter.CompileStatus, out status);
            if(status == 0)
            {
                string log = RL.GetShaderString(shader, ShaderStringParameter.CompileLog);
                string msg = shaderDescription + " compilation failed:\n" + log;
                System.Console.WriteLine(msg);
                throw new System.Exception(shaderDescription + " compilation failed:\n" + log);
            }
            else
            {
                string log = RL.GetShaderString(shader, ShaderStringParameter.CompileLog);
                string msg = shaderDescription + " compilation ok:\n" + log;
                System.Console.WriteLine(msg);
            }
            return shader;
        }

        static RLprogram linkProgram(RLshader shader0, RLshader shader1, string programDescription)
        {
            int status;
            RLprogram programName;

            programName = RL.CreateProgram();
            RL.ProgramString(programName, ProgramStringParameter.ProgramName, programDescription);
            if(shader0 != System.IntPtr.Zero) RL.AttachShader(programName, shader0);
            if(shader1 != System.IntPtr.Zero) RL.AttachShader(programName, shader1);
            RL.LinkProgram (programName);
            RL.GetProgram(programName, ProgramParameter.LinkStatus, out status);
            if(status == 0)
            {
                string log;
                RL.GetProgramString(programName, ProgramStringParameter.LinkLog, out log);
                throw new System.Exception(programDescription + " linking failed:\n" + log);
            }
            return programName;
        }

        static int getAttribLocation(RLprogram programName, string attribName)
        {
            int loc = RL.GetAttribLocation(programName, attribName);

            if (loc == -1)
            {
                throw new System.Exception("Attribute " + attribName + " not found.");
            }

            return loc;
        }

        static void DrawOneTriangleInit()
        {
            string vertexShaderSource =
                /* This is pretty much the simplest useful vertex shader you can have.
                 * It just passes the incoming position attribute on to rl_Position.
                 */
@"attribute vec4 positionAttribute;
void main()
{
    rl_Position = positionAttribute;
}
";
            string rayShaderSource =
                /* This is pretty much the simplest useful ray shader you can have.
                 * It just accumulates a constant color into the framebuffer.
                 */
@"void main()
{
    accumulate(vec3(0.0, 0.5, 1.0));
}
";
            string frameShaderSource =
                /* This is a simple orthographic camera. It shoots parallel rays in the
                 * +z direction and has the frame cover x and y from -1.0 to 1.0.
                 */
@"void setup() { rl_OutputRayCount = 1; }
void main()
{
    createRay();
    rl_OutRay.origin    = vec3((rl_FrameCoord / rl_FrameSize * 2.0 - 1.0).xy, -1.0);
    rl_OutRay.direction = vec3(0.0, 0.0, 1.0);
    emitRayWithoutDifferentials();
}
";
            int positionAttributeLoc;

            vertexShader = compileShader(vertexShaderSource, ShaderType.VertexShader,   "Vertex shader");
            rayShader    = compileShader(rayShaderSource,    ShaderType.RayShader,      "Ray shader");
            frameShader  = compileShader(frameShaderSource,  ShaderType.FrameShader,    "Frame shader");

            triangleProgram = linkProgram(vertexShader, rayShader,          "Program");
            frameProgram    = linkProgram(frameShader,  System.IntPtr.Zero, "Frame program");

#if true
            positionAttributeLoc = getAttribLocation(triangleProgram, "positionAttribute");

            // ELEMENT_ARRAY_BUFFERs are created, bound and populated using OpenRL
            RL.GenBuffers(1, out indexBuffer);
            RL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            RL.BufferData(BufferTarget.ElementArrayBuffer, 3 * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // ARRAY_BUFFERs are created, bound and populated using OpenRL
            RL.GenBuffers(1, out positionBuffer);
            RL.BindBuffer(BufferTarget.ArrayBuffer, positionBuffer);
            RL.BufferData(BufferTarget.ArrayBuffer, 3 * 2 * sizeof(float), positions, BufferUsageHint.StaticDraw);

            // An object in the raytraced scene is represented by a Primitive.
            // Primitives are generated and bound just like GL textures.
            // When a primitive is bound, certain RL calls change the primitive's state.
            RL.GenPrimitives(1, out trianglePrimitive);
            RL.BindPrimitive(PrimitiveTarget.Primitive, trianglePrimitive);
            RL.PrimitiveParameterString(PrimitiveTarget.Primitive, PrimitiveStringParameter.PrimitiveName, "triangle");
    
            // Bind the triangle program to the triangle primitive. 
            RL.UseProgram(triangleProgram);

            // Bind the position VBO and call VertexAttribBuffer to associate the VBO with the vertex shader's positionAttribute.
            // OpenRL only supports VBO storage for vertex data.
            RL.BindBuffer(BufferTarget.ArrayBuffer, positionBuffer);
            RL.VertexAttribBuffer(positionAttributeLoc, 2, VertexAttribType.Float, false, sizeof(float)*2, 0);

            // DrawElements and friends set the primitive type, indexing mode, and number of elements for a primitive. 
            // In this case, we'll use an index VBO
            RL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            RL.DrawElements(BeginMode.Triangles, 3, DrawElementsType.UnsignedInt, 0);

            Glut.DebugProgram();

            PrimitiveStatus ok1 = RL.CheckPrimitiveStatus(PrimitiveTarget.Primitive);
#endif

            // Unbind the currently bound primitive because the active frame shader is set in global RL state. 
            RL.BindPrimitive(PrimitiveTarget.Primitive, 0);
            RLframebuffer fbo;
            RL.GetFramebuffer(FramebufferBinding.FramebufferBinding, out fbo);

            RLprimitive prim;
            RL.GetPrimitive(PrimitiveBinding.PrimitiveBinding, out prim);
            RL.UseProgram(frameProgram);

            Glut.DebugProgram();

            RL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        static void Reshape(int w, int h)
        {
            RL.Viewport(0, 0, w, h);
        }

        static void DrawOneTriangleFrame()
        {
            RL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            RL.Clear(ClearBufferMask.ColorBufferBit);
            //RL.RenderFrame();
    
            Glut.SwapBuffers();
        }

        static void DrawOneTriangleShutdown()
        {
            RL.DeleteProgram(triangleProgram);
            RL.DeleteShader(vertexShader);
            RL.DeleteShader(rayShader);
    
            RL.DeleteProgram(frameProgram);
            RL.DeleteShader(frameShader);
    
            RL.DeleteBuffers(ref indexBuffer);
            RL.DeleteBuffers(ref positionBuffer);
    
            RL.DeletePrimitives(ref trianglePrimitive);
        }

        static void Keyboard(System.Windows.Forms.Keys c, int x, int y)
        {
            if (c == System.Windows.Forms.Keys.Escape)
            {
                DrawOneTriangleShutdown();
            }
        }
    }
}
