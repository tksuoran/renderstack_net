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
        /*  BGImage < image path + filename> [(sequence)]
            FGImage <image path + filename> [(sequence)]
            FGAlphaImage < image path + filename> [(sequence)]

            example:  BGImage Images/Sky.tga
            example:  BGImage Images/BldColor.tga

            example:  FGAlphaImage Images/BldAlpha.tga

            The BGImage, FGImage and FGAlphaImage functions provides
            the path and filename for the background, foreground and
            foreground alpha channel images respectively. The paths
            are generated by checking the current content directory
            for the listed filenames.  

            In these examples if the current content directory is
            <c:\NewTek>, LightWave would attempt to load the files
            <c:\NewTek\Images\Sky.tga>, <c:\NewTek\Images\BldColor.tga>
            and <c:\NewTek\Images\BldAlpha.tga>.

            It is possible to use image sequences for these functions.
            If an image sequence is chosen, a sequence identifier is
            appended to the image path and filename. An additional
            ImageSequenceInfo listing is also produced. (See Image
            Sequences Section 1.5)
        */
        void BGImage()
        {
            string bg_image = file.read_string();
        }

        void FGImage()
        {
            string fg_image = file.read_string();
        }

        void FGAlphaImage()
        {
            string fg_alpha_image = file.read_string();
        }

        /*  FGDissolve <percentage> � (envelope)

            example:  FGDissolve 0.750000

            The FGDissolve function provides the dissolve
            percentage of the foreground image. This function
            is most widely used with an envelope to produce a
            smooth fade from a foreground image to the current
            scene.
        */
        void FGDissolve()
        {
            double fg_dissolve = file.read_double();
        }

        /*  FGFaderAlphaMode <flag>

            example:  FGFaderAlphaMode 1

            The FGFaderAlphaMode flag activates the foreground
            fader alpha mode. This modes provides an additional
            type of Alpha channel image compositing.

            <flag>:
                0 - Off (No Listing)
                1 - On (Listing)
        */
        void FGFaderAlphaMode()
        {
            int fg_fader_alpha_mode = file.read_int();
        }

        /*  Additional: ImageSequenceInfo <frame offset> <loop flag> <loop length>

            The ImageSequenceInfo listing is produced only when
            an image sequence is chosen. It provides optional
            information for the image sequence. (See Image
            Sequences Section 1.5)
        */
        void ImageSequenceInfo()
        {
            int frame_offset = file.read_int();
            int loop_flag    = file.read_int();
            int loop_length  = file.read_int();
        }

        /*  ForegroundKey <flag>

            example:  ForegroundKey 1

            The ForegroundKey flag activates the foreground keying
            function in the image composting process of LightWave.
            This function allows the user to set a HighClipColor
            and a LowClipColor for the clipping of the foreground 
            image.

            <flag>:
                0 - Off (No Listing)
                1 - On (Listing plus additional listings)
        */
        void ForegroundKey()
        {
            int foreground_key = file.read_int();
        }

        /*  Additional:  LowClipColor <Red value> <Green value> <Blue value>

            Additional:  HighClipColor <Red value> <Green value> <Blue value>

            example:  LowClipColor 0 0 0

            example:  HighClipColor 125 125 125

            The LowClipColor and HighClipColor functions provides
            the "darkest" and the "brightest" RGB color respectively
            for the ForegroundKey function.
        */
        void LowClipColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
        }
        void HighClipColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
        }

        /*  BackdropColor <Red value> <Green value> <Blue value>

            example:  BackdropColor 125 125 125

            The BackdropColor function provides the RGB values
            for the backdrop color. These values are used only
            when the SolidBackdrop flag is turned on.
        */
        void BackdropColor()
        {
            double red       = file.read_double();
            double green     = file.read_double();
            double blue      = file.read_double();
            scene.BackgroundColor = new Vector3(red, green, blue);
        }

        /*  SolidBackdrop <flag>

            example:  SolidBackdrop 1

            The SolidBackdrop flag activates a single color
            backdrop. If the flag is turned off, a gradient
            backdrop is produced using the RGB color values 
            provided in the Backdrop Color, Zenith Color,
            Sky Color, and Nadir Color listings.

            <flag>:
                0 - Off (Gradient Backdrop)
                1 - On (SolidBackdrop)
        */
        void SolidBackdrop()
        {
            int solid_backdrop = file.read_int();
        }

        /*  The ZenithColor, SkyColor, GroundColor, and Nadir
            Color functions provide the RBG values for the
            gradient backdrop. These values are used only when
            the SolidBackdrop flag is turned off.
        */
        void ZenithColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
            scene.ZenithColor = new Vector3(red, green, blue);
        }
        void SkyColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
            scene.SkyColor = new Vector3(red, green, blue);
        }
        void GroundColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
            scene.GroundColor = new Vector3(red, green, blue);
        }
        void NadirColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
            scene.NadirColor  = new Vector3(red, green, blue);
        }
        void SkySqueezeAmount()
        {
        }
        void GroundSqueezeAmount()
        {
        }

        /*  FogType <value>

            example:  FogType 1

            The FogType function determines which type of fog
            will be generated during the rendering process. If
            the Fog effect is turned on in LightWave,
            additional listings are produced.

            <value>:
                0 - Off
                1 - Linear
                2 - NonLinear 1
                3 - NonLinear 2
        */
        void FogType()
        {
            scene.FogType = file.read_int();
        }

        /*  Additional:  FogMinDist <Distance> � (envelope)

            example:  FogMinDist 25.000000

            The FogMinDist function provides the distance from
            the camera that the fog will begin. This functions
            value can be fluctuated over time with an envelope.
            If an envelope is chosen, the distance value is
            replace with an envelope identifier.
        */
        void FogMinDist()
        {
            scene.FogMinDist = file.read_double();
        }

        /*  Additional:  FogMaxDist <Distance> � (envelope)

            example:  FogMaxDist 350.00000

            The Fog MaxDist function provides the distance from
            camera that objects in the fog will remain visible.
            This functions value can be fluctuated over time with
            an envelope. If an envelope is chosen, the distance
            value is replace with an envelope identifier.
        */
        void FogMaxDist()
        {
            scene.FogMaxDist = file.read_double();
        }

        /*  Additional:  FogMinAmount <percentage> � (envelope)

            example:  FogMinAmount 0.250000

            The FogMinAmount function provides the lower bounding
            value for the density (amount) of fog in the scene.
        */
        void FogMinAmount()
        {
            double fog_min_amount = file.read_double();
        }

        /*  Additional:  FogMaxAmount <percentage> � (envelope)

            example:  FogMaxAmount 0.750000

            The FogMaxAmount function provides the upper bounding
            value for the density (amount) of fog in the scene.
        */
        void FogMaxAmount()
        {
            scene.FogMaxAmount = file.read_double();
        }

        /*  Additional:  FogColor <Red value> <Green value> <Blue value>

            example:  FogColor 200 200 215

            The FogColor function provides the RGB values for the
            color of the fog. These values are used only when the
            BackdropFog flag is turned off.
        */
        void FogColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
            scene.FogColor = new Vector3(red, green, blue);
        }

        /*  Additional:  BackdropFog <flag>

            example:  BackdropFog 1

            The BackdropFog flag determines how the fog will
            be colored. If the flag is on, the fog will use
            the backdrop color. If the flag is off, it will
            use the values provided in the     FogColor function.

            <flag>:
                0 - Use FogColor
                1 - Use Backdrop Colors
        */
        void BackdropFog()
        {
            int backdrop_fog = file.read_int();
        }

        /*  DitherIntensity <value>

            example:  DitherIntensity 2

            The DitherIntensity function provides the type of
            dithering to be used during the rendering process.

            <value>:
                0 - Off
                1 - Normal
                2 - 2 x Normal
                3 - 4 x Normal
        */
        void DitherIntensity()
        {
            int dither_intensity = file.read_int();
        }

        /*  AnimatedDither <flag>

            example:  AnimatedDither 1

            The AnimatedDither flag activates the animated dither function.
            This will use an alternate dithering pattern frame by frame.

            <flag>:
                0 - Off
                1 - On 
        */
        void AnimatedDither()
        {
            int animated_dither = file.read_int();
        }

        /*  Saturation <percentage> � (envelope)

            example:  Saturation 0.350000

            The Saturation function provides the percentage of
            color saturation to be used during the rendering
            process. The function produces a listing only 
            when set below 100%.
        */
        void Saturation()
        {
            double saturation = file.read_double();
        }

        /*  GlowEffect <flag>

            example:  GlowEffect 1

            The GlowEffect flag activates the glow effect for
            the rendering process. When this function is turned
            on, it allows any surface that has it's glow effect
            flag turned on to be affected by the glow post-process.
            This function, when turned on, produces a listing plus
            additional option listings.

            <flag>:
                0 - Off (No Listing)
                1 - On (Listing plus additional listings)
        */
        void GlowEffect()
        {
            int glow_effect = file.read_int();
        }

        /*  Additional:  GlowIntensity <percentage> � (envelope)

            example:  GlowIntensity 1.000000

            The GlowIntensity provides the percentage of glow intensity 
            (brightness) that the glow post-process will produce.
        */
        void GlowIntensity()
        {
            double glow_intensity = file.read_double();
        }

        /*  Additional:  GlowRadius <pixels> � (envelope)

            example:  GlowRadius 8.000000

            The GlowRadius provides the glow distance, in pixels,
            that the glow post-process will produce.
        */
        void GlowRadius()
        {
            double glow_radius = file.read_double();
        }
    }
}
/*

The Effects section contains the information that relates
to the different effects that are available in a LightWave
scene.

The following is a list of Effects functions that are
listed in the order in which they appear in the scene file.

        BGImage
        FGImage
        FGAlphaImage
        FGDissolve
        FGFaderAlphaMode
        ForegroundKey
            LowClipColor
            HighClipColor
        SolidBackdrop *
        BackdropColor *
        ZenithColor *
        SkyColor *
        GroundColor *
        NadirColor *
        FogType *
            FogMinDist
            FogMaxDist
            FogMinAmount
            FogMaxAmount
            FogColor
            BackdropFog
        DitherIntensity *
        AnimatedDither *
        Saturation
        GlowEffect
            GlowIntensity
            GlowRadius
        DataOverlay
        DataOverlayLabel *
*/
