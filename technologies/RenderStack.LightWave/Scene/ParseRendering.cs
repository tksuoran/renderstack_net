using System.Collections.Generic;
using System.IO;

using RenderStack.Math;

//using ID4 = System.UInt32;
using I1 = System.SByte;
using I2 = System.Int16;
using I4 = System.Int32;
using U1 = System.Byte;
using U2 = System.UInt16;
using U4 = System.UInt32;
using F4 = System.Single;
using S0 = System.String;
using VX = System.UInt32;
using COL4 = RenderStack.Math.Vector4;
using COL12 = RenderStack.Math.Vector3;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public partial class LWSceneParser
    {
        /*  RenderMode <value>

            example:  RenderMode 2

            The RenderMode function determines the type of rendering for the scene.

            <value>:
                0 - WireFrame
                1 - Quickshade
                2 - Realistic (Default)
        */
        void RenderMode()
        {
            int render_mode = file.read_int();
        }

        /*  RayTraceEffects <bit-field value>

            example:  RayTraceEffects 7

            The RayTraceEffects function determines the ray trace options for the scene.

            The value is produced by calculating the decimal value of a 3 position 
            bit-field whose bits represent logical on/off switches that are numbered 
            left to right from 0 - 2.  The field's least-significant bit is the 
            rightmost bit.  Each ray trace option has a corresponding bit in this 
            bit-field.  When an option is selected, it's bit (or switch) is turned on.

            <bit position>:
                0 - Trace Shadows
                1 - Trace Reflection
                2 - Trace Refraction
        */
        void RayTraceEffects()
        {
            int ray_trace_effects = file.read_int();
        }

        void RayTraceOptimization()
        {
            int ray_trace_optimization = file.read_int();
        }

        void RayRecursionLimit()
        {
            int ray_recursion_limit = file.read_int();
        }

        /*  DataOverlay <flag>

            example:  DataOverlay 1

            The DataOverlay flag activates the data overlay function that overlays a 
            string provide by the DataOverlayLabel function on the rendered frames.

            <flag>:
                0 - Off (No Listing)
                1 - On
        */
        void DataOverlay()
        {
            int data_overlay = file.read_int();
        }

        /*  DataOverlayLabel <string>

            example:  DataOverlayLabel Scene1_4/16/95

            The DataOverlayLabel function provides the string to be used by the 
            DataOverlay function.
        */
        void DataOverlayLabel()
        {
            file.skip();
        }

        void OutputFilenameFormat()
        {
            file.skip();
        }

        void SaveRGB()
        {
            int save_rgb = file.read_int();
        }

        void SaveAlpha()
        {
            int save_alpha = file.read_int();
        }

        /*  SaveANIMFileName <image path + filename>

            example:  SaveANIMFileName Anims\LogoAnim

            The SaveANIMFileName function provides the path and filename for the 
            animation to be saved during rendering.

            In this example, if the current content directory is <c:\NewTek>, 
            LightWave would attempt to save the anim file as <c:\NewTek\Anims\LogoAnim>.

            Some animation formats produce additional function listings.
        */
        void SaveANIMFileName()
        {
            string anim_file_name = file.read_string();
        }

        /*  Additional:  LockANIMPaletteFrame <frame number>

            example:  LockANIMPaletteFrame 12

            The LockANIMPaletteFrame function provides the frame number to be 
            rendered for the palette information.  The palette of all frames 
            rendered in the animation will then use the given frames palette.
        */
        void LockANIMPaletteFrame()
        {
            int lock_anim_palette_frame = file.read_int();
        }

        /*  Additional:  BeginANIMLoopFrame <frame number>

            example:  BeginANIMLoopFrame 15

            The BeginANIMLoopFrame function provides the frame number to loop the 
            animation from.  After the animation is completed, it would continue 
            looping from the given frame to the end of the animation.
        */
        void BeginANIMLoopFrame()
        {
            int begin_anim_loop_frame = file.read_int();
        }

        /*  SaveRGBImagesPrefix<image path + filename>

            example:  SaveRGBImagesPrefix Images\LogoFrames

            The SaveRGBImagesPrefix function provides the path and filename prefix 
            for the images to be saved during rendering.  A frame number and optional 
            file extension will be added to this filename.  This additional information 
            is provided by the Output Filename Format config file listing and the 
            RGBImageFormat function.  

            In this example, if the current content directory is <c:\NewTek>, the 
            Output Filename Format is set to Name001.xxx, and the Image Format is 
            24-bit Targa,  LightWave would attempt to save the first image file as 
            <c:\NewTek\Images\LogoFrames001.tga>.
        */
        void SaveRGBImagesPrefix()
        {
            string rgb_images_prefix = file.read_string();
        }

        /*  Additional:  RGBImageFormat <value>

            example:  RGBImageFormat 2

            The RGBImageFormat function determines which file format will be 
            used in the image saving process.

            <value>:
                0 - 24-bit IFF (.IFF)
                1 - 24-bit RAW (.RAW)
                2 - 24-bit Targa (.TGA)
        */
        void RGBImageFormat()
        {
            int rgb_image_format = file.read_int();
        }

        /*  SaveAlphaImagesPrefix<image path + filename>

            example:  SaveAlphaImagesPrefix Images\LogoAlpha

            The SaveAlphaImagesPrefix function provides the path and filename prefix 
            for the alpha images to be saved during rendering.  A frame number and 
            optional file extension will be added to this filename.  This additional 
            information is provided by the Output  Filename Format config file listing 
            and the AlphaImageFormat function.  

            In this example, if the current content directory is <c:\NewTek>, the 
            Output Filename Format is set to Name001.xxx, and the Alpha Image Format 
            is 24-bit IFF,  LightWave would attempt to save the first image file as 
            <c:\NewTek\Images\LogoAlpha001.tga>.
        */
        void SaveAlphaImagesPrefix()
        {
            string alpha_images_prefix = file.read_string();
        }

        /*  Additional:  AlphaImageFormat <value>

            example:  AlphaImageFormat 1

            The AlphaImageFormat function determines which file format will 
            be used in the image saving process.

            <value>:
                0 - 8-bit IFF (.IFF)
                1 - 24-bit IFF (.IFF)
        */
        void AlphaImageFormat()
        {
            int alpha_image_format = file.read_int();
        }

        /*  AlphaMode <value>

            example:  AlphaMode 1

            The AlphaMode functions determines which type of alpha image is 
            going to be produced during the rendering process.

            <value>:
                0 - Normal Alpha
                1 - Fader Alpha Mode
        */
        void AlphaMode()
        {
            int alpha_mode = file.read_int();
        }

        /*
            SaveFramestoresComment <image path + filename>

            example:  SaveFramestoreComment  Images\Frame

            The SaveFramestoreComment function provides the path and
            filename prefix for the framestores to be saved during
            rendering.  A frame number and optional file extension
            will be added to this filename.  

            In this example, if the current content directory i
            <c:\NewTek>, LightWave would attempt to save the first
            image file as <c:\NewTek\Images\001.FS.Frame>.
        */
        void SaveFramestoresComment()
        {
            string framestores_comment = file.read_string();
        }

        void FullSceneParamEval()
        {
            int full_scene_param_evel = file.read_int();
        }

        /*  SegmentMemory <bytes>

            example: SegmentMemory 88000000

            The SegmentMemory determines the amount of memory to be allocated for the 
            rendering process.  If the amount of memory is too low, LightWave will 
            divide the rendering process into separate segments.
        */
        void SegmentMemory()
        {
            int segment_memory = file.read_int();
        }

        /*  Antialiasing <value>

            example:  Antialiasing  2

            The Antialiasing function determines the number of antialiasing (smoothing) 
            passes that will be used for rendering.

            <value>:    0 - Off
                    1 - Low Antialiasing (5 passes)
                    2 - Medium Antialiasing (9 passes)
                    3 - High Antialiasing (17 passes)
        */
        void Antialiasing()
        {
            int antialiasing = file.read_int();
        }

        void EnhancedAA()
        {
            int enchaced_aa = file.read_int();
        }

        /*  FilterType<flag>

            example:  FilterType 1

            The FilterType flag activates the Soft Filter effect for the rendering process.

            <flag>:    0 - Off (No listing)
                1 - On (Listing)
        */
        void FilterType()
        {
            int filter_type = file.read_int();
        }

        /*  AdaptiveSampling <flag>

            example:  AdaptiveSampling 1

            The AdaptiveSampling flat activates adaptive sampling for the rendering 
            process.  This function, when activated, produces an additional 
            AdaptiveThreshold listing.

            <flag>:   
                0 - Off (Listing)
                1 - On (Listing plus additional AdaptiveThreshold listing)
        */
        void AdaptiveSampling()
        {
            int adaptive_sampling = file.read_int();
        }

        /*  Additional:  AdaptiveThreshold <int>

            TIS: appears to use doubles in LWSC 3

            example:  AdaptiveThreshold 8

            The AdaptiveThreshold function provides a value for the level of 
            adaptive sampling during the rendering process.  This value is a 
            threshold, or cutoff level, for the antialiasing process. 
        */
        void AdaptiveThreshold()
        {
            double adaptive_treshold = file.read_double();
        }

        /*  FieldRendering <flag>

            example:  FieldRendering 1

            The FieldRendering flag activates the field rendering function during the 
            rendering process.  This function, when activated, produces an additional 
            ReverseFields listing.

            <flag>:    0 - Off (Listing)
                1 - On (Listing plus additional ReverseFields listing)
        */
        void FieldRendering()
        {
            int field_rendering = file.read_int();
        }

        /*  Additional:  ReverseFields <flag>

            example:  ReverseFields 1

            The ReverseFields flag activates the reverse fields function.  
            This function shifts the order in which the fields are rendered.
        */
        void ReverseFields()
        {
            int reverse_field = file.read_int();
        }
    }
}

/*
The effects section contains the information on the saving of animations, 
RGB images and Alpha images.

Record functions produce a listing only when activated by the user.

The file saving process is not active when the scene is loaded into
LightWave.  The file name is available to the record functions.  The user 
must activate the save function manually to begin the saving process.

    SaveANIMFileName
        LockANIMPaletteFrame
        BeginANIMLoopFrame
    SaveRGBImagesPrefix
        RGBImageFormat
    SaveAlphaImagesPrefix
        AlphaImageFormat
        AlphaMode
    SaveFramestoresComment
*/

