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
        /*  AmbientColor <Red value> <Green value> <Blue value>

            example:  AmbientColor 200 200 200

            The AmbientColor function provides the RGB color values for the ambient 
            lighting in the scene.

            <value range>:                  ||| LWSC 3
                    Red value   - 0 - 255   ||| 0..1
                    Green value - 0 - 255   ||| 0..1
                    Blue value  - 0 - 255   ||| 0..1

        */
        void AmbientColor()
        {
            double red    = file.read_double();
            double green  = file.read_double();
            double blue   = file.read_double();

            scene.AmbientColor = new Vector3(red, green, blue);
        }

        /*  AmbientIntensity <percentage> ¦ (envelope)

            example:  AmbIntensity 0.250000

            The AmbientIntensity function provides the intensity of the ambient light 
            in the scene.  The functions percentage value can be changed over time 
            with an envelope.  If an envelope is chosen, the functions value is 
            replaced with an envelope identifier.
        */
        void AmbientIntensity()
        {
            scene.AmbientIntensity.Value = file.read_double();
            currentFloatEnvelope = scene.AmbientIntensity;
        }

        void GlobalFlareIntensity()
        {
            scene.GlobalFlareIntensity.Value = file.read_double();
            currentFloatEnvelope = scene.GlobalFlareIntensity;
        }

        /*  EnableLensFlares <flag>

            example:  EnableLensFlares 0

            This function sets a global flag that enables/disables all lens flares 
            in the scene.  This function produces a listing only when disabled (0).   

            <flag>:        0 - Off
                    1 - On (Default : No function listing)
        */
        void EnableLensFlares()
        {
            int e = file.read_int();
            scene.EnableLensFlare = (e == 1) ? true : false;
        }

        /*  EnableShadowMaps <flag>

            example:  EnableShadowMaps 0

            This function sets a global flag the enables/disables the calculation of 
            shadow maps for all lights.  This function produces a listing only when 
            disabled (0).

            <flag>:
                0 - Off
                1 - On (Default: No function listing)
        */
        void EnableShadowMaps()
        {
            int e = file.read_int();
            scene.EnableShadowMaps = (e == 1) ? true : false;
        }

        /*  AddLight

            example:  AddLight

            The AddLight function  is the first function in a light segment.    
            It is called for each instance in a light in the scene file.
        */
        void AddLight()
        {
            currentItem = currentLight = new LWLight();
            scene.Lights.Add(currentLight);
        }

        /*  ShowLight <Refresh value> <Color value>

            example:     ShowLight 0 5

            The ShowLight function determines how the light is going to be displayed 
            in Layout.  The above example would hide this bone until selected.  If it 
            were set to visible refresh, it would be purple.

                 Refresh value

            This argument sets the lights display type in Layout.  

            <Refresh value>:
                    0 - No Refresh (Hide)
                    1 - Refresh (Show)

            User:  This value is selected in the second column of the Scene Overview 
            from the Scene Menu.      

                 Color value

            This argument sets the color of the light in Layout

            <Color value>:
                    1 - Blue
                    2 - Green
                    3 - Light Blue
                    4 - Red
                    5 - Purple
                    6 - Orange
                    7 - Gray

            User: This value is selected in the first column of the Scene Overview 
            from the Scene Menu.

        */
        void ShowLight()
        {
            currentLight.Refresh = file.read_int();
            currentLight.WireColor = file.read_int();
        }

        /*  LightName Light ¦ <string>

            example:  LightName HeadLight

            This function provides a name for the light created with the AddLight 
            function.  If the user renames the light using the Rename Light function 
            from the Light menu, the string is listed following the function name.  
            If the user does not rename the light, it is given the default name of 
            Light.  If multiple light instances have the same name, duplicate light 
            names are given a numbered suffix to the name during the loading/creation 
            process.  This number is enclosed in parenthesis and follows the light name.  

            An example:  HeadLight (2) is the second instance of a HeadLight.  The 
            number is not saved in the scene file, and is used only as a user reference.
        */
        void LightName()
        {
            currentLight.Name = file.read_string();
        }

        /*  LightMotion (unnamed)

            example:     LightMotion (unnamed)

            This is a light motion label for the keyframe information of the current 
            light segment.  It does not require any arguments to be passed to it.

            The LightMotion identifier is listed with all light instances.
        */
        void LightMotion()
        {
            currentMotion = new LWMotion();
            currentLight.Motion = currentMotion;
        }

        /*  LightColor <Red value> <Green value> <Blue value>

            TIS: Appears to use doubles in LWSC 3

            example:  LightColor 200 200 150

            The LightColor function provides the RGB color values for the light that 
            is cast from the current light source.

            <value range>:
                    Red value -     0 - 255
                    Green value -     0 - 255
                    Blue value -     0 - 255
        */
        void LightColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
            currentLight.Color = new Vector3(red, green, blue);
        }

        /*  LightType <value>

            example:  LightType 0

            The LightType function provides type of light source for the current 
            light instance.  

            <value>:
                    0 - Distant
                    1 - Point
                    2 - Spot
                ?   3 - Linear
                ?   4 - Area

            User:  This function's value is chosen from the Light Type selections 
            on the Lights Panel.
        */
        void LightType()
        {
            currentLight.Type = file.read_int();
        }

        void LightFalloffType()
        {
            currentLight.FalloffType = file.read_int();
        }

        void LightRange()
        {
            currentLight.Range.Value = file.read_double();
            currentFloatEnvelope = currentLight.Range;
        }

        /*  LightConeAngle <angle> ¦ (envelope)

            example:  LightConeAngle 30.000000

            The LightConeAngle function provides the degree angle of the light cone 
            projected from the current light source.  This function only applies 
            to Spot Lights (LightType 2).  This function's angle can be fluctuated 
            over time with an envelope.  If an envelope is chosen, the functions 
            value is replaced with an envelope identifier.

            User:  This function is set from the SpotLight Cone Angle field on the 
            Lights Panel.
        */
        void LightConeAngle()
        {
            currentLight.ConeAngle.Value = file.read_double();
            currentFloatEnvelope = currentLight.ConeAngle;
        }

        /*  LightEdgeAngle <angle> ¦ (envelope)

            example:  LightEdgeAngle 5.000000

            The LightEdgeAngle function provides the degree angle for the soft edge of 
            the light cone projected from the current light source.  This function 
            only applies to SpotLights (LightType 2).  This function's angle can be 
            fluctuated over time with an envelope.  If an envelope is chosen, the 
            function's value is replaced with an envelope identifier.

            User:  This functions value is set from the Spot Soft Edge Angle field 
            on the Lights Panel.
        */
        void LightEdgeAngle()
        {
            currentLight.EdgeAngle.Value = file.read_double();
            currentFloatEnvelope = currentLight.EdgeAngle;
        }

        /*  LightIntensity <percentage> ¦ (envelope)

            example:  LgtIntensity 0.750000

            This function provides the intensity of the current light source as a 
            percentage value.  This is equivalent to the "brightness of the light 
            source".  The intensity value can go above 100%.  The function's value 
            can be changed over time with an envelope.  If an envelope is chosen, 
            the functions value is replaced with an envelope identifier.  

            User:  This function is set from the Light Intensity field on the Lights 
            Panel.
        */
        void LightIntensity()
        {
            currentLight.Intensity.Value = file.read_double();
            currentFloatEnvelope = currentLight.Intensity;
        }

        /*  Falloff <percentage> ¦ (envelope)

            example:  Falloff 0.350000

            The Falloff function provides the percentage of light intensity falloff 
            per meter.  This function is only used by Point (LightType 1) and 
            Spot (LightType 2) lights.  This function's percentage can be fluctuated 
            over time with an envelope.  If an envelope is chosen, the functions value 
            is replaced with an envelope identifier.

            User:  This function is set from the Intensity Falloff field on the 
            Lights Panel.
        */
        void Falloff()
        {
            currentLight.Falloff.Value = file.read_double();
            currentFloatEnvelope = currentLight.Falloff;
        }

        void AffectCaustics()
        {
            currentLight.AffectCaustics = file.read_int();
        }

        void AffectDiffuse()
        {
            currentLight.AffectDiffuse = file.read_int();
        }

        void AffectSpecular()
        {
            currentLight.AffectSpecular = file.read_int();
        }

        void AffectOpenGL()
        {
            currentLight.AffectOpenGL = file.read_int();
        }

        /*  UseConeAngle <flag>

            example:  UseConeAngle 1

            This flag determines whether the shadow map will use the light's cone 
            angle or a separate angle for the shadowmap calculations.  If the flag 
            is turned off, an additional function listing for ShadowMapAngle is produced.

            <flag>
                0 - Use angle from ShadowMapAngle
                1 - Use Cone Angle
        */
        void UseConeAngle()
        {
            currentLight.UseConeAngle = file.read_int();
        }

        /*  LensFlare <flag>

            example:  LensFlare 1

            This flag turns the LensFlare function on.  This function produces a 
            LensFlare listing with a value of 1 and additional function listings 
            when turned on.  When turned off, this function does not produce a 
            listing in the scene file.

            User:  This function is activated from the Lens Flare check box on the 
            Lights Panel.
        */
        void LensFlare()
        {
            currentLight.LensFlare = file.read_int();
        }

        /*  Additional:  FlareIntensity <percentage> ¦ (envelope)

            example:  FlareIntensity 0.750000

            This function provides the intensity/size of the lens flare.  
            The intensity can be fluctuated with an envelope.

            User:  This function's value is set in the Flare Intensity field 
            from the Lens Flare Options Panel.
        */
        void FlareIntensity()
        {
            currentLight.FlareIntensity.Value = file.read_double();
            currentFloatEnvelope = currentLight.FlareIntensity;
        }

        /*  Additional:  FlareDissolve <percentage> ¦ (envelope)

            example:  FlareDissolve 0.350000

            This function provides the percentage of dissolve of the current 
            light's lens flare.  The dissolve can be fluctuated with an envelope.

            User:  This function's value is set on the Flare Dissolve field 
            from the Lens Flare Options Panel.
        */
        void FlareDissolve()
        {
            currentLight.FlareDissolve.Value = file.read_double();
            currentFloatEnvelope = currentLight.FlareDissolve;
        }

        /*  Additional:  LensFlareFade <bit-field value>

            example:  LensFlareFade 4

            This function determines the fade options for the current light's 
            lens flare.

            The value is produced by calculating the decimal value of a 5 position 
            bit-field whose bits represent logical on/off switches that are 
            numbered left to right from 0 - 4.  The field's least-significant bit 
            is the rightmost bit.  Each lens flare fade option has a corresponding 
            bit in this bit-field.  When an option is selected, it's bit (or 
            switch) is turned on.

            <bit position>:    
                0 - RESERVED
                1 - Fade With Distance
                2 - Fade Off Screen
                3 - Fade Behind Objects
                4 - Fade In Fog
        */
        void LensFlareFade()
        {
            currentLight.LensFlareFade = file.read_int();
        }

        /*  Additional:  LensFlareOptions <bit-field value>

            example:  LensFlareOptions 85

            This function provides the flare settings for the current light's 
            lens flare.

            The value is produced by calculating the decimal value of a 10 position 
            bit-field whose bits represent logical on/off switches that are 
            numbered left to right from 0 - 9.  The field's least-significant 
            bit is the rightmost bit.  Each lens flare option has a corresponding 
            bit in this bit-field.  When an option is selected, it's bit (or 
            switch) is turned on. 

            <bit positions>:    
                0 - Central Glow + Red Outer Glow
                1 - Central Ring
                2 - Star Filter
                3 - Random Streaks
                4 - Lens Reflections
                5 - Suppress Red Outer Glow
                6 - Anamorphic Distort
                7 - Anamorphic Streaks
                8 - Off Screen Streaks
                9 - Glow Behind Objs
        */
        void LensFlareOptions()
        {
            currentLight.LensFlareOptions = file.read_int();
        }

        /*  Additional:  FlareRandStreakInt <percentage>

            example:  FlareRandStreakInt 0.030000

            The FlareRandStreakInt function provides the percentage of intensity 
            for the current light's random streaks.
        */
        void FlareRandStreakInt()
        {
            currentLight.FlareRandStreakInt = file.read_double();
        }

        /*  Additional:  FlareRandStreakDens <float>

            example:  FlareRandStreakDens 0.500000

            The FlareRandStreakDens function provides a floating point value for 
            the density of the current light's lens flare random streaks.

          */
        void FlareRandStreakDens()
        {
            currentLight.FlareRandStreakDens = file.read_double();
        }

        /*  Additional:  FlareRandStreakSharp <float>

            example:  FlareRandStreakSharp 0.060000

            The FlareRandStreakSharp function provides a floating point value 
            for the density of the current light's lens flare random streaks.
        */
        void FlareRandStreakSharp()
        {
            currentLight.FlareRandStreakSharp = file.read_double();
        }

        void ShadowType()
        {
            currentLight.ShadowType = file.read_int();
        }

        /*  ShadowCasting <value>

            example:     ShadowCasting 7

            The ShadowCasting function determines what type of shadows the current 
            light is going to produce during the rendering process.  This function 
            chooses the type of shadow rendering, but it does not initiate the 
            process.  Additional functions must be activated to turn the shadowing 
            process on.  For Raytrace Shadows, the Camera function, RayTraceEffects 
            must include the TraceShadows option.  For ShadowMap Shadows the Light 
            function: EnableShadowMaps must be set to 1.

            <value>:
                0 - No Shadows
                1 - Raytrace Shadows
                2 - ShadowMap Shadows
        */
        void ShadowCasting()
        {
            currentLight.ShadowCasting = file.read_int();
        }

        /*  ShadowMapSize <int>

            example:  ShadowMapSize 512

            The ShadowMapSize function sets the amount of memory, in bytes, that the 
            current light is going to allocate for it's shadow map calculations.  
            A higher memory size will result in a smoother shadow map.
        */
        void ShadowMapSize()
        {
            currentLight.ShadowMapSize = file.read_int();
        }

        /*  additional:  ShadowMapAngle <angle>

            example:  ShadowMapAngle 0.450000

            This function provides the angle for the shadow map calculations 
            if the UseConeAngle function is turned off.
        */
        void ShadowMapAngle()
        {
            currentLight.ShadowMapAngle = file.read_double();
        }

        /*  ShadowFuzziness <float>

            example:  ShadowFuzziness 1.000000

            The ShadowFuzziness function provides a floating point value for the edge 
            softness of the current light's shadow map calculations.
        */
        void ShadowFuzziness()
        {
            currentLight.ShadowFuzziness = file.read_double();
        }

        void ShadowColor()
        {
            double red   = file.read_double();
            double green = file.read_double();
            double blue  = file.read_double();
        }
    }
}

/*
Chapter 4:            LIGHT SECTION


4.1  LIGHT SECTION INFORMATION


4.1.1 Light Section Description


  The Light Section contains all of the information that pertains to the 
  lights in a LightWave scene.

  LightWave loads and lists all lights in the order in which they appear 
  in the scene file.

  Duplicate light names are given a numbered suffix during the loading 
  process.  This number is enclosed in parenthesis and follows the light 
  name.  An example:  HeadLight (3) is the third instance of the light     
  name HeadLight.  The number is not saved in the scene file, and is 
  used only as a user reference.

  The Target and Parent functions use a value that is equal to the order 
  in which the referenced object was loaded in the scene.  i.e. The value 
  in the function ParentObject 3 means that the current light is parented 
  to the third object instance in the scene file.



4.1.2 Individual Light Segment


    Preceding Scene and Object sections......

AmbientColor 255 255 255
AmbIntensity 0.250000
AddLight
LightName Light
ShowLight 0 7
LightMotion (unnamed)
 9
 1
 0 0 0 60 30 0 1 1 1 
 0 0 0 0 0 
EndBehavior 1
LightColor 255 255 255
LgtIntensity 1.000000
LightType 2
FallOff 0.000000
ConeAngle 30.000000
EdgeAngle 5.000000
ShadowCasting 1

    Additional Scene Listings.......



4.2  LIGHT FUNCTIONS


4.2.1 Function Order

The following is a list of light functions that are listed in the order 
in which they appear in the scene file.


  Functions denoted with an asterisk (*) are required for all light 
  loading instances.

  Italicized entries denote function labels and not true function names.

  Indented entries denote an optional function of the preceding function.

  Optional functions will produce a listing only when activated by the 
  user.

        AmbientColor  *
        AmbIntensity  *
        GlobalFlareIntensity (envelope)
        EnableLensFlare
        EnableShadowMaps
        AddLight  *
        LightName *
        ShowLight *
        LightMotion (identifier) *
        Number of Information Channels *
        Number of Keyframes *
        Keyframe Information *
        EndBehavior *
        LockedChannels
        ParentObject
        TargetObject
        LightColor *
        LgtIntensity *
        LightType *
        Falloff  (Point & Spot only)
        ConeAngle  (Spot only)
        EdgeAngle  (Spot only)
        LensFlare
            FlareIntensity
            FlareDissolve
            LensFlareFade
            LensFlareOptions
            FlareRandStreakInt
            FlareRandStreakDens
            FlareRandStreakSharp
        ShadowCasting *
        ShadowMapSize  (Spot only w/ Shadow Map)
        UseConeAngle  (Spot only w/ Shadow Map)
            ShadowMapAngle 
        ShadowFuzzines  (Spot only w/ Shadow Map)



4.2.2 Function Descriptions

The following functions are listed in the order in which they appear in 
the scene file.

AmbientColor and AmbientIntensity are functions that are called once to 
set up the ambient lighting in a scene.  GlobalFlareIntensity, 
EnableLensFlare, and EnableShadowMaps are all global functions that 
effect all lights in a LightWave scene.  They are also called just once.

Beginning with AddLight, all functions following can be called for each 
light instance in a scene file. 
*/


/*

GlobalFlareIntensity (envelope)

The GlobalFlareIntensity function provides an envelope to fluctuate the 
intensity of all lens flares in the scene at the same time.  This function 
does not have the option for a single percentage, it is adjusted only 
through an envelope.
*/


/*
LightMotion (unnamed)

example:     LightMotion (unnamed)

This is a light motion label for the keyframe information of the current 
light segment.  It does not require any arguments to be passed to it.

The LightMotion identifier is listed with all light instances.


Number of Information Channels:  <9>

This is a numeric value with no header that follows the LightMotion 
identifier.  The value for the number of information channels is equal 
to the number of variables to be provided per keyframe.  For LightWave 
keyframes, the variables are listed as follows:  X position, Y position, 
Z position, Heading, Pitch, Bank, X Scale, Y Scale, and Z Scale.  For 
light motions, the number of information channels value is automatically 
set to 9 by LightWave.  The user has no access to this value.  

The number of information channels is listed with all light instances.


Number of Keyframes:  <int>

This is a numeric value with no header that follows the Number of 
Information Channels.  This value provides the number of keyframes for 
the current light.  It is immediately followed by the keyframe information.  
Every light instance  will have at least one keyframe at frame 0.  


The Number of Keyframes is listed with all light instances.


Keyframe Information:

-1.321534   2.235439  2.164330  -60.000000  360.000000  180.000000   
1.0   1.0   1.0      15  0  1.0   0.0   0.0 

The values are listed as follows:

1st Line:

XPosition, YPosition, ZPosition, Heading, Pitch, Bank

2nd Line:

XScale, YScale, ZScale, Frame Number, Linear Value, Tension, Continuity, Bias

At least one keyframe (frame 0) is listed for each light.

(See Section 1.3 Keyframes)
*/

/*

EndBehavior <value>

example:     EndBehavior 2

The EndBehavior function determines how the light will react when the 
last keyframe has been reached. The available choices are:  reset, stop 
and repeat.  The default setting is stop.  

<value>:    0 - Reset
        1 - Stop (Default)
        2 - Repeat

User: This value is set from the light's motion graph panel.

The EndBehavior option is listed with all lights.

*/

/*
LockedChannels <bit-field value>

example:     LockedChannels 4093

This function determines the extent of the mouse control from LightWave's 
Layout.  Separate independent channels of motion, rotation, etc. can be 
locked off to restrict the mouse's  control on each channel.  The mouse 
functions that it can effect are:  Move (X,Y,Z), Rotate(H,P,B), 
Scale/Stretch(X,Y,Z), and RestLength(X,Y,Z).

The value is produced by calculating the decimal value of a 6 position 
bit-field whose bits represent logical on/off switches that are numbered 
left to right from 0 to 5.  The least significant bit for this field is 
the rightmost bit.  Each channel has a corresponding bit in the bit-field.  
When a channel is locked, it 's  bit (or switch) is turned on.

<bit positions>:    0 - Move X
            1 - Move Y
            2 - Move Z
            3 - Rotate Heading
            4 - Rotate Pitch
            5 - Rotate Bank

User:  This function is set from the Layout mouse control area.
*/

/*

ParentObject <object instance>

example:     ParentObject 4

This function provides LightWave with the current lights parent object 
in the hierarchical chain.  The value is equal to the parent objects 
position in the loading/creation sequence.  The example function would 
parent the current light to the fourth object instance in the scene file.  
When the ParentObject function is active, all keyframe information for 
the light becomes an offset from the parents information. 

*/

/*
TargetObject <object instance>

example:  TargetObject 3

This function provides LightWave with the current lights target object 
in the scene.  The value is equal to the target object's position in the 
loading/creation sequence.  The example function would target the current 
light to the fourth object instance in the scene file. 

*/




