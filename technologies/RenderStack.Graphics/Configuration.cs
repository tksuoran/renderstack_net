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
using System.Linq;
using System.Diagnostics;
using System.Text;

using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Configuration options for RenderStack.Graphics.
    /// 
    /// \note Experimental.
    public class Configuration
    {
        public static int   glVersion;
        public static int   glslVersion;

        public static int   maxAttributeMappings            = 32;
        public static bool  useGl1                          = false;

        //public static bool  useOpenRL                       = true;
        public static bool  useOpenRL                       = false;

        public static bool  coreProfile                     = false;
        public static bool  compatibilityProfile            = false;
        public static bool  forwardCompatible               = false;
        public static bool  useBinaryShaders                = false;
        public static bool  useIntegerPolygonIDs            = false;
        public static bool  useVertexArrayObject            = true;
        public static bool  mustUseVertexArrayObject        = false;

        public static bool  canUseBaseVertex                = false;
        public static bool  canUseBinaryShaders             = false;
        public static bool  canUseFloatTextures             = false;
        public static bool  canUseFramebufferObject         = false;
        public static bool  canUseFrameTerminator           = false;
        public static bool  canUseGeometryShaders           = false;
        public static bool  canUseInstancing                = false;
        public static bool  canUseIntegerFramebufferFormat  = false;    //  \todo this needs to be clarified
        public static bool  canUsePixelBufferObject         = false;    //  \todo
        public static bool  canUseSeamlessCubeMap           = false;
        public static bool  canUseStringMarker              = false;
        public static bool  canUseTesselationShaders        = false;
        public static bool  canUseTextureArrays             = false;    //  \todo also support extension
        public static bool  canUseTextureBufferObject       = false;
        public static bool  canUseTimerQuery                = false;    //  \todo check which GL has this in core
        public static bool  canUseUniformBufferObject       = false;
        public static bool  canUseVertexArrayObject         = false;
        public static bool  throwProgramExceptions          = true;
        private static int  defaultVAO                      = int.MaxValue;
        public static int MaxTextureSize                    = 64;
        public static int Max3DTextureSize                  = 0;
        public static int MaxCubeMapTextureSize             = 0;
        public static int MaxTextureBufferSize              = 0;
        public static int MaxTextureUnits                   = 0; /* fixed function */
        public static int MaxTextureImageUnits              = 0; /* shaders */
        public static int MaxCombinedTextureImageUnits      = 0;
        public static int MaxUniformBlockSize               = 0;
        public static int MaxUniformBufferBindings          = 8;
        public static int MaxVertexUniformBlocks            = 0;
        public static int MaxFragmentUniformBlocks          = 0;
        public static int MaxGeometryUniformBlocks          = 0;
        public static int MaxTessControlUniformBlocks       = 0;
        public static int MaxTessEvaluationUniformBlocks    = 0;
        public static int UniformBufferOffsetAlignment      = 1;
        public static int DefaultVAO { get { return defaultVAO; } }

        public static string    ProgramSearchPathGL;
        public static string    ProgramSearchPathRL;


        private static string DigitsOnly(string s)
        {
            for(int i = 0; i < s.Length; ++i)
            {
                if(char.IsDigit(s[i]) == false)
                {
                    if(i == 0)
                    {
                        return "";
                    }
                    else
                    {
                        return s.Substring(0, i);
                    }
                }
            }
            return s;
        }

        public static void Initialize()
        {
            Trace.WriteLine("---------------------------------------");

            string vendor   = GL.GetString(StringName.Vendor);
            string renderer = GL.GetString(StringName.Renderer);
            string version  = GL.GetString(StringName.Version);

            Trace.WriteLine("Vendor:   " + vendor);
            Trace.WriteLine("Renderer: " + renderer);
            Trace.WriteLine("Version:  " + version);

            string[] versions = version.Split('.');

            int major = int.Parse(DigitsOnly(versions[0]));
            int minor = int.Parse(DigitsOnly(versions[1]));

            glVersion = (major * 100) + (minor * 10);

            GL.GetInteger(GetPName.MaxTextureSize, out MaxTextureSize);

            string[] extensions;
            if(glVersion >= 300)
            {
                //  GL 3 introduced a new way to access extension strings
                int numExtensions;

                GL.GetInteger(GetPName.NumExtensions, out numExtensions);
                extensions = new string[numExtensions];
                for(int i = 0; i < numExtensions; ++i)
                {
                    extensions[i] = GL.GetString(StringName.Extensions, i);
                }
            }
            else
            {
                string e = GL.GetString(StringName.Extensions);
                extensions = e.Split(' ');
            }

            if(glVersion >= 200 || extensions.Contains("GL_ARB_shading_language_100"))
            {
                try
                {
                    string shadingLanguageVersion = GL.GetString(StringName.ShadingLanguageVersion);
                    versions = shadingLanguageVersion.Split('.');

                    major = int.Parse(DigitsOnly(versions[0]));
                    minor = int.Parse(DigitsOnly(versions[1]));
                    glslVersion = (major * 100) + (minor);
                }
                catch(System.Exception)
                {
                    //  This should not happen; glslVersion is left to 0.
                    //  Note: implicit value for glsl shader source version is 110 if not explicitly set
                }
            }

            Trace.WriteLine("glVersion:   " + glVersion);
            Trace.WriteLine("glslVersion: " + glslVersion);

            if(glVersion < 300)
            {
                MaxTextureUnits = 1;
                if(glVersion >= 121 || extensions.Contains("GL_ARB_multitexture"))
                {
                    GL.GetInteger(GetPName.MaxTextureUnits, out MaxTextureUnits);
                }
            }

            if(glVersion >= 120 || extensions.Contains("GL_EXT_texture3D"))
            {
                GL.GetInteger((GetPName)All.Max3DTextureSize, out Max3DTextureSize);
            }

            if(glVersion >= 130 || extensions.Contains("GL_ARB_texture_cube_map"))
            {
                GL.GetInteger((GetPName)All.MaxCubeMapTextureSize, out MaxCubeMapTextureSize);
            }

            if(glVersion >= 200)
            {
                GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureImageUnits);
                GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxCombinedTextureImageUnits);
            }

            //  GL 3.0 introduced context flags
            if(glVersion >= 300)
            {
                int contextFlags;
                GL.GetInteger(GetPName.ContextFlags, out contextFlags);
                if((contextFlags & (int)(All.ContextFlagForwardCompatibleBit)) != 0)
                {
                    forwardCompatible = true;
                    Trace.WriteLine("forwardCompatible");
                }
            }

            //  GL 3.3 introduced context profile mask
            if(glVersion >= 330)
            {
                int contextProfileMask;
                GL.GetInteger((GetPName)(All.ContextProfileMask), out contextProfileMask);
                if((contextProfileMask & (int)(All.ContextCoreProfileBit)) != 0)
                {
                    coreProfile = true;
                    Trace.WriteLine("coreProfile");
                }
                if((contextProfileMask & (int)(All.ContextCompatibilityProfileBit)) != 0)
                {
                    compatibilityProfile = true;
                    Trace.WriteLine("compatibilityProfile");
                    //  This is for testing
                    //  glslVersion = 120;
                }
            }

            if(glslVersion >= 300 || extensions.Contains("GL_EXT_texture_array"))
            {
                canUseTextureArrays = true;
                Trace.WriteLine("canUseTextureArrays");
            }

            if(glVersion >= 300 || extensions.Contains("GL_ARB_texture_buffer_object"))
            {
                // \todo check gl core version requirement
                // \todo enable support for EXT version (different entry points)
                canUseTextureBufferObject = true;
                GL.GetInteger((GetPName)All.MaxTextureBufferSize, out MaxTextureBufferSize);
                Trace.WriteLine("canUseTextureBufferObject (" + MaxTextureBufferSize + ")");
                //GL.GetInteger(GetPName.MaxTextureBufferSize, out MaxTextureBufferSize);
            }

            if(glslVersion >= 320 || extensions.Contains("GL_ARB_seamless_cube_map"))
            {
                canUseSeamlessCubeMap = true;
                Trace.WriteLine("canUseSeamlessCubeMap");
            }
            if(glVersion >= 320 || extensions.Contains("GL_ARB_geometry_shader4"))
            {
                canUseGeometryShaders = true;
                Trace.WriteLine("canUseGeometryShaders");
            }

            if(glVersion >= 400 || extensions.Contains("GL_ARB_tesselation_shader"))
            {
                canUseTesselationShaders = true;
                Trace.WriteLine("canUseTesselationShaders");
            }

            if(extensions.Contains("GL_ARB_timer_query"))
            {
                canUseTimerQuery = true;
                Trace.WriteLine("canUseTimerQuery");
            }

            if(glVersion >= 300 || extensions.Contains("GL_ARB_uniform_buffer_object"))
            {
                canUseUniformBufferObject = true;
                GL.GetInteger(GetPName.MaxUniformBlockSize, out MaxUniformBlockSize);
                GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out UniformBufferOffsetAlignment);
                GL.GetInteger(GetPName.MaxUniformBufferBindings, out MaxUniformBufferBindings);
                GL.GetInteger(GetPName.MaxVertexUniformBlocks, out MaxVertexUniformBlocks);
                GL.GetInteger(GetPName.MaxFragmentUniformBlocks, out MaxFragmentUniformBlocks);
                if(canUseGeometryShaders)
                {
                    GL.GetInteger(GetPName.MaxGeometryUniformBlocks, out MaxGeometryUniformBlocks);
                }
                if(canUseTesselationShaders)
                {
                    GL.GetInteger(GetPName.MaxTessControlUniformBlocks, out MaxTessControlUniformBlocks);
                    GL.GetInteger(GetPName.MaxTessEvaluationUniformBlocks, out MaxTessEvaluationUniformBlocks);
                }
                Trace.WriteLine(
                    "canUseUniformBufferObject " + 
                    "(max block size = " + MaxUniformBlockSize + 
                    ", offset alignment = " + UniformBufferOffsetAlignment + 
                    ", max bindings = " + MaxUniformBufferBindings +
                    ", max vertex blocks = " + MaxVertexUniformBlocks +
                    ", max fragment blocks = " + MaxFragmentUniformBlocks +
                    ")"
                );
            }

            if(
                (glVersion >= 300) || 
                (
                    extensions.Contains("GL_ARB_draw_instanced")        //  DrawArraysInstanced, DrawElementsInstanced
#if false
                    && extensions.Contains("GL_ARB_instanced_arrays")   //  VertexAttribDivisor
#endif
                )
            )
            {
                // GL_ARB_draw_elements_base_vertex
                // GL_ARB_draw_indirect
                // GL_ARB_draw_instanced
                // \todo check gl core version requirement
                canUseInstancing = true;
                Trace.WriteLine("canUseInstancing");
            }

            if(glVersion >= 300 || extensions.Contains("GL_ARB_framebuffer_object"))
            {
                canUseFramebufferObject = true;
                Trace.WriteLine("canUseFramebufferObject");
            }

            if(glVersion >= 300 || extensions.Contains("GL_ARB_vertex_array_object"))
            {
                canUseVertexArrayObject = true;
                Trace.WriteLine("canUseVertexArrayObject");
                GL.GenVertexArrays(1, out defaultVAO);
                GL.BindVertexArray(defaultVAO);
            }

            if(glVersion >= 300 || extensions.Contains("GL_ARB_texture_float"))
            {
                canUseFloatTextures = true;
                Trace.WriteLine("canUseFloatTextures");
            }

            //  GL versions 3.0, 3.1 and 3.2 use forwardCompatible
            //  GL versions 3.3 and later have coreProfile
            //  Default VAO no longer exists in GL version 3.1 forward compatible contexts
            if(glVersion >= 310 && (coreProfile || forwardCompatible))
            {
                mustUseVertexArrayObject = true;
                Trace.WriteLine("mustUseVertexArrayObject");
            }

            if(glVersion >= 320 || extensions.Contains("GL_ARB_draw_elements_base_vertex"))
            {
                canUseBaseVertex = true;
                Trace.WriteLine("canUseBaseVertex");
            }

            if(glVersion >= 300) //  || GL_EXT_texture_integer / GL_EXT_gpu_shader4 - \bug clarify
            {
                canUseIntegerFramebufferFormat = true;
                Trace.WriteLine("canUseIntegerFramebufferFormat");
            }

            if(glVersion >= 410 || extensions.Contains("GL_ARB_get_program_binary"))
            {
                canUseBinaryShaders = true;
                Trace.WriteLine("canUseBinaryShaders");
            }

            if(extensions.Contains("GL_GREMEDY_frame_terminator"))
            {
                canUseFrameTerminator = true;
                Trace.WriteLine("canUseFrameTerminator");
            }
            if(extensions.Contains("GL_GREMEDY_string_marker"))
            {
                canUseStringMarker = true;
                Trace.WriteLine("canUseStringMarker");
            }

            /*
            foreach(var e in extensions)
            {
                Trace.WriteLine(e);
            }*/
            Trace.WriteLine("---------------------------------------");
        }
    }
}