using System.Collections.Generic;
using System.IO;

using RenderStack.Math;

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
        void AddCamera()
        {
            currentItem = currentCamera = new LWCamera();
            scene.Cameras.Add(currentCamera);
        }
        void CameraName()
        {
            currentCamera.Name = file.read_string();
        }

        /*  ShowCamera <Refresh value> <Color value>

            example:     ShowCamera 0 5

            The ShowCamera function determines how the camera is going to be displayed 
            in Layout.  The above example would hide the camera until selected.  If it 
            were set to visible refresh, it would be purple when not selected.


            Refresh value

            This argument sets the camera display type in Layout.  

            <Refresh value>:
                    0 - No Refresh (Hide)
                    1 - Refresh (Show)

            User:  This value is selected in the second column of the Scene Overview from 
            the Scene Menu.      


            Color value

            This argument sets the color of the camera wireframe in Layout.  When the 
            camera is selected, the wireframe highlights to yellow. 

            <Color value>:    1 - Blue
                2 - Green
                3 - Light Blue
                4 - Red
                5 - Purple
                6 - Orange
                7 - Gray

            User: This value is selected in the first column of the Scene Overview from 
            the Scene Menu.
        */
        void ShowCamera()
        {
            currentCamera.Refresh = file.read_int();
            currentCamera.WireColor = file.read_int();
        }

        void CameraMotion()
        {
            currentMotion = new LWMotion();
            currentCamera.Motion = currentMotion;
        }

        /*  TargetObject <object instance>

            example:  TargetObject 3

            This function provides LightWave with the camera's target object in the 
            scene.  The value is equal to the target object's position in the 
            loading/creation sequence.  The example function would target the 
            camera at the third object instance in the scene file. 
        */
        void TargetObject()
        {
            currentCamera.TargetObject = file.read_int();
        }

        /*  ZoomFactor <float> ¦ (envelope)

            example:  ZoomFactor 3.200000

            The ZoomFactor function provides a floating point number that represents 
            the zoom amount of the camera's lens.  This function can be fluctuated over 
            time with an envelope.  If an envelope is chosen, the floating point value 
            is replaced with an envelope identifier.
        */
        void ZoomFactor()
        {
            currentCamera.ZoomFactor.Value = file.read_double();
            currentFloatEnvelope = currentCamera.ZoomFactor;
        }

        /*  MotionBlur <bit-field value>

            example:  MotionBlur 7

            The MotionBlur function determines the active motion blur functions for the 
            rendering process.  When particle blur or motion blur are selected, they 
            produce an additional BlurLength listing.

            The value is produced by calculating the decimal value of a 3 position 
            bit-field whose bits represent logical on/off switches that are numbered 
            left to right from 0 - 2.  The field's least-significant bit is the 
            rightmost bit.  Each motion blur option has a corresponding bit in this 
            bit-field.  When an option is selected, it's bit (or switch) is turned on.

            <bit position>:
                0 - Particle Blur (Additional Listing)
                1 - Motion Blur (Additional Listing)
                2 - Dithered Motion Blur
        */
        void MotionBlur()
        {
            currentCamera.MotionBlur = file.read_int();
        }

        /*  Additional:  BlurLength <percentage> ¦ (envelope)

            example:  BlurLength 0.500000

            The BlurLength function provides the amount of blur to be applied 
            during the rendering process.
        */
        void BlurLength()
        {
            currentCamera.BlurLength.Value = file.read_double();
            currentFloatEnvelope = currentCamera.BlurLength;
        }

        /*  DepthofField <flag>

            example:  DepthofField 1

            The DepthofField flag activates the depth of field function for the 
            rendering process.  This function, when activated, produces additional 
            FocalDistance and LensFStop listings.

            <flag>:    0 - Off (Listing)
                1 - On (Listing plus additional Listings)
        */
        void DepthofField()
        {
            currentCamera.DepthOfField = file.read_int();
        }

        /*  Additional:  FocalDistance <Distance> ¦ (envelope)

            example:  FocalDistance 25.0000

            The FocalDistance function provides the distance from the camera of 
            it's focal point.  This distance is given in meters.
        */
        void FocalDistance()
        {
            currentCamera.FocalDistance.Value = file.read_double();
            currentFloatEnvelope = currentCamera.FocalDistance;
        }

        /*  Additional:  LensFStop <float> ¦ (envelope)

            example:  LensFStop 4.000000

            The LensFStop function determines the range of in-focus items from 
            the focal point.  The larger the F-stop, the larger the focal range.
        */
        void LensFStop()
        {
            currentCamera.LensFStop.Value = file.read_double();
            currentFloatEnvelope = currentCamera.LensFStop;
        }

        void ResolutionMultiplier()
        {
            currentCamera.ResolutionMultiplier = file.read_double();
        }

        void FrameSize()
        {
            double width  = file.read_double();
            double height = file.read_double();
            currentCamera.FrameSize = new Vector2(width, height);
        }

        /*  Resolution <value>

            example:  Resolution 1

            The Resolution function determines the resolution of the rendering in 
            the current scene.

            <value>:
                -1 - Super Low Resolution
                 0 - Low Resolution
                 1 - Medium Resolution
                 2 - High Resolution
                 3 - Print Resolution
        */
        void Resolution()
        {
            currentCamera.Resolution = file.read_int();
        }

        /*  CustomSize <Horizontal resolution> <Vertical resolution>

            example:  Custom Size 1024 768

            The CustomSize function provides the horizontal and vertical pixel 
            resolutions for rendering.
        */
        void CustomSize()
        {
            double width  = file.read_double();
            double height = file.read_double();
            currentCamera.CustomSize = new Vector2(width, height);
        }

        /*  FilmSize <value>

            example:  FilmSize 1

            The FilmSize function determines what type of film LightWave is going to 
            simulate during the rendering process.  This adjusts the optical qualities 
            in the cameras adjustment of zoom factor and depth of field.

            <value>:
                 0 - Super 8 motion picture
                 1 - 16mm motion picture
                 2 - 35mm motion picture (Default)
                 3 - 65mm Super Panavision motion picture
                 4 - 65mm Imax motion picture
                 5 - Size 110 (pocket camera)
                 6 - Size 135 (35mm SLR)
                 7 - Size 120 (60 x 45 mm rollfilm camera)
                 8 - Size 120 (90 x 60 mm rollfilm camera)
                 9 - 1/3 " CCD video camera
                10 - 1/2" CCD video camera
        */
        void FilmSize()
        {
            currentCamera.FilmSize = file.read_int();
        }

        /*  NTSCWidescreen <flag>

            example:  NTSCWidescreen 1

            The NTSCWidescreen flag activates a function that will compress the 
            rendered image horizontally.  When this image is displayed in the 
            NTSC Widescreen format it will display in the proper aspect.
        */
        void NTSCWidescreen()
        {
            currentCamera.NTSCWidescreen = file.read_int();
        }

        void PixelAspect()
        {
            currentCamera.PixelAspect = file.read_double();
        }

        /*  PixelAspectRatio <value>

            example:  PixelAspectRatio 0

            The PixelAspectRatio function provides the aspect (shape) of the pixel 
            in a rendered image.

            <value>:
                    -1 - Custom (Produces Additional CustomPixelRatio Listing)
                     0 - D2 NTSC
                     1 - D1 NTSC
                     2 - Square Pixels
                     3 - D2 PAL
                     4 - D1 PAL
        */
        void PixelAspectRatio()
        {
            int pixel_aspect_ratio = file.read_int();
        }

        /*  Additional:  CustomPixelRatio <float>

            example:  CustomPixelRatio 1.000000

            The CustomPixelRatio function provides a custom pixel aspect for 
            rendering.  The floating point value is the height to width ratio 
            of the needed pixels. 
        */
        void CustomPixelRatio()
        {
            double custom_pixel_ration = file.read_double();
        }

        /*  LimitedRegion <flag>

            example:  LimitedRegion 1

            The LimitedRegion flag activates the limited region function to render a 
            portion of the full camera view.  This function, when activated, produces 
            an additional RegionLimits listing.

            <flag>:
                0 - Off (No Listing)
                1 - On (Listing plus additional RegionLimits listing)
        */
        void LimitedRegion()
        {
            int limited_region = file.read_int();
        }

        /*  Additional:  RegionLimits <Left %><Right %><Top %><Bottom %>

            example: RegionLimits 0.50000 1.000000 0.500000 1.000000

            The RegionLimits function provides the dimensions of the area to be
            rendered for the LimitedRegion function.  The values given are a 
            percentage of screen size.

            <% limits>:
                Left    -     0.000000 to 0.990000
                Right   -     0.010000 to 1.000000
                Top     -     0.000000 to 0.990000
                Bottom  -     0.010000 to 1.000000
        */
        void RegionLimits()
        {
            double left   = file.read_double();
            double right  = file.read_double();
            double top    = file.read_double();
            double bottom = file.read_double();
        }

        void MaskPosition()
        {
            int mask_x1 = file.read_int();
            int mask_y1 = file.read_int();
            int mask_x2 = file.read_int();
            int mask_y2 = file.read_int();
        }

        void ApertureHeight()
        {
            currentCamera.ApertureHeight = file.read_double();
        }

    }
}

/*

The Camera Section contains all of the information that relates to the 
camera in a LightWave Scene.

  There is only one (1) camera instance per scene file.

  The Target and Parent functions use a value that is equal to the order 
  in which the referenced object was loaded/created in the scene.  i.e. The 
  value given in the example: ParentObject 3  references the third object 
  instance in the scene file.


The following is a list of light functions that are listed in the order in 
which they appear in the scene file.

  Functions denoted with an asterisk (*) are required for all light loading 
  instances.
  Italicized entries denote function labels and not true function names.
  Indented entries denote an optional function of the preceding function.
  Optional functions will produce a listing only when activated by the user.

        ShowCamera *
        CameraMotion (identifier) *
        Number of Information Channels *
        Number of Keyframes *
        Keyframe Information *
        EndBehavior *
        LockedChannels
        ParentObject
        TargetObject
        ZoomFactor *
        RenderMode *
        RayTraceEffects *
        Resolution *
        CustomSize
        NTSCWidescreen
        PixelAspectRatio
            CustomPixelRatio
        LimitedRegion
            RegionLimits
        SegmentMemory *
        Antialiasing *
        FilterType
        AdaptiveSampling *
            AdaptiveThreshold
        FilmSize *
        FieldRendering *
            ReverseFields
        MotionBlur *
            BlurLength
        DepthofField *
            FocalDistance
            LensFStop
*/

