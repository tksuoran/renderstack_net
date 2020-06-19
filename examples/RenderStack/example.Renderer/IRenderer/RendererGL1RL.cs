using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Caustic.OpenRL;
using OpenRLContext = System.IntPtr;
using OpenRLContextAttribute = System.IntPtr;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

namespace example.Renderer
{
    public partial class RendererGL1 : RenderStack.Services.Service, IRenderer
    {
#if false
        private OpenRLContext context;

        public void Notify(ErrorCode code, string message)
        {
            System.Console.WriteLine(code.ToString() + " : " + message);
        }

        private void InitializeOpenRL()
        {
            context = OpenRL.CreateContext(null, Notify, IntPtr.Zero);
            OpenRL.SetCurrentContext(context);

            string vendor     = RL.GetString(StringParameter.Vendor);
            string renderer   = RL.GetString(StringParameter.Renderer);
            string version    = RL.GetString(StringParameter.Version);
            string extensions = RL.GetString(StringParameter.Extensions);

            System.Console.WriteLine(vendor);
            System.Console.WriteLine(renderer);
            System.Console.WriteLine(version);
            System.Console.WriteLine(extensions);

#if false
            int     MaxVertexUniformVectors;
            int     MaxUniformBlocks;
            int     MaxVertexAttribs;
            int     MaxVaryingVectors;
            int     MaxColorAttachments;
            int     MaxTextureSize;
            int[]   MaxViewportDims = new int[2];
            int[]   Viewport = new int[4];
            int     MaxOutputRayCount;
            int     MaxRayDepthLimit;
            int     MaxRayClasses;
            int     PrimitiveCount;

            RL.GetInteger(IntegerParameter.MaxVertexUniformVectors, out MaxVertexUniformVectors);
            RL.GetInteger(IntegerParameter.MaxUniformBlocks, out MaxUniformBlocks);
            RL.GetInteger(IntegerParameter.MaxVertexAttribs, out MaxVertexAttribs);
            RL.GetInteger(IntegerParameter.MaxVaryingVectors, out MaxVaryingVectors);
            RL.GetInteger(IntegerParameter.MaxColorAttachments, out MaxColorAttachments);
            RL.GetInteger(IntegerParameter.MaxTextureSize, out MaxTextureSize);
            RL.GetInteger(IntegerParameter.MaxViewportDims, MaxViewportDims);
            RL.GetInteger(IntegerParameter.Viewport, Viewport);
            RL.GetInteger(IntegerParameter.MaxOutputRayCount, out MaxOutputRayCount);
            RL.GetInteger(IntegerParameter.MaxRayDepthLimit, out MaxRayDepthLimit);
            RL.GetInteger(IntegerParameter.MaxRayClasses, out MaxRayClasses);
            RL.GetInteger(IntegerParameter.PrimitiveCount, out PrimitiveCount);
            System.Console.WriteLine("MaxVertexUniformVectors  " + MaxVertexUniformVectors );
            System.Console.WriteLine("MaxUniformBlocks         " + MaxUniformBlocks        );
            System.Console.WriteLine("MaxVertexAttribs         " + MaxVertexAttribs        );
            System.Console.WriteLine("MaxVaryingVectors        " + MaxVaryingVectors       );
            System.Console.WriteLine("MaxColorAttachments      " + MaxColorAttachments     );
            System.Console.WriteLine("MaxTextureSize           " + MaxTextureSize          );
            System.Console.WriteLine("MaxViewportDims          " + MaxViewportDims[0] + " x " + MaxViewportDims[1]);
            System.Console.WriteLine("Viewport                 " + Viewport[0] + ", " + Viewport[1] + ", " + Viewport[2] + ", " + Viewport[3]);
            System.Console.WriteLine("MaxOutputRayCount        " + MaxOutputRayCount       );
            System.Console.WriteLine("MaxRayDepthLimit         " + MaxRayDepthLimit        );
            System.Console.WriteLine("MaxRayClasses            " + MaxRayClasses           );
            System.Console.WriteLine("PrimitiveCount           " + PrimitiveCount          );

            int PrimitiveMaxElements;
            RL.GetSize(SizeParameter.PrimitiveMaxElements, out PrimitiveMaxElements);
            System.Console.WriteLine("PrimitiveMaxElements     " + PrimitiveMaxElements);

            int IsHardwareAccelerated;
            RL.GetBoolean(BooleanParameter.IsHardwareAccelerated, out IsHardwareAccelerated);
            System.Console.WriteLine("IsHardwareAccelerated    " + IsHardwareAccelerated);
#endif
        }
#endif
    }
}
