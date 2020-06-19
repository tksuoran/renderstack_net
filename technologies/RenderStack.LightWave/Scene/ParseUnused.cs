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
        /*  DisplacementMap <mapping type> ¦ Clip Map <mapping type> 

            DisplacementMap <projection type> ¦ <procedural texture>

            The DisplacementMap function is the first line of a
            Displacement Map segment. It provides the mapping type
            and selection.

            - Image Mapping

            example:  DisplacementMap cylindrical image map

            If image mapping is used, the argument of the projection
            type is provided. This mapping type produces additional
            listings for the TextureImage and the TextureAxis functions.

            <projection type>:
                Planar Image Map
                Cylindrical Image Map
                Spherical Image Map

            - Procedural Texture Mapping

            example:  DisplacementMap Ripples

            If procedural texture mapping is used, the argument of
            a texture name is provided. This mapping type produces
            the additional listings given with each procedural below:

            <procedural type>:    

                    Ripples:
                        TextureInt0   <int>   - Wave Sources
                        TextureFloat0 <float> - Wavelength
                        TextureFloat1 <float> - Wave Speed

                    Fractal Bumps: 
                        TextureInt0 <int> - Frequencies

            - ClipMap <projection type> ¦ <procedural texture>

            The ClipMap function is the first line of a Clip
            Map segment. It provides the mapping type and selection.

            - Image Mapping

            If image mapping is used, the argument of the projection
            type is provided. This mapping type produces an additional
            listing for the TextureImage and TextureAxis functions.

            example:  ClipMap Planar Image Map

            <projection type>:
                Planar Image Map
                Cylindrical Image Map
                Spherical Image Map
                Cubic Image Map
                Front Projection Map


            - Procedural Texture Mapping

            example:  ClipMap Underwater

            If procedural texture mapping is used, the argument of a
            texture name is provided. This mapping type produces the
            additional listings given with each procedural below:            

            <procedural texture>:    
                Checkerboard: no additional listings.
                Grid:         TextureFloat0 <float> - Line Thickness
                Dots:         TextureFloat0 <float> - Dot Diameter
                              TextureFloat1 <float> - Fuzzy Edge Width
                Marble:       TextureInt0   <int>   - Frequencies
                              TextureFloat0 <float> - Turbulence
                              TextureFloat1 <float> - Vein Spacing
                              TextureFloat2 <float> - Vein Sharpness
                Wood:         TextureInt0   <int>   - Frequencies
                              TextureFloat0 <float> - Turbulence
                              TextureFloat1 <float> - Ring Spacing
                              TextureFloat2 <float> - Ring Sharpness
                Underwater:   TextureInt0   <int>   - Wave Sources
                              TextureFloat0 <float> - Wavelength
                              TextureFloat1 <float> - Wave Speed
                              TextureFloat2 <float> - Band Sharpness
                Fractal Noise:TextureInt0   <int>   - Frequencies
                              TextureFloat0 <float> - Contrast
                Bump Array:   TextureFloat0 <float> - Radius
                              TextureFloat1 <float> - Spacing
                              TextureFloat2 <float> - Bump Strength
                Crust:        TextureFloat0 <float> - Coverage
                              TextureFloat1 <float> - Ledge Level
                              TextureFloat2 <float> - Ledge Width
                              TextureFloat3 <float> - Bump Strength
                Veins:        TextureFloat0 <float> - Coverage
                              TextureFloat1 <float> - Ledge Level
                              TextureFloat2 <float> - Ledge Width
                              TextureFloat3 <float> - Bump Strength
        */

        /*  DisplacementMap:  (See Displacement Map Section 3.4)

            The DisplacementMap function deforms the geometry of an object
            using either image maps or procedural textures.

            This function is described in detail in the DisplacementMap/ClipMap 
            Section 3.4.

            User Interface:  The DisplacementMap controls are located on the Objects Panel.
        */
        void DisplacementMap()
        {
        }

        /*  ClipMap:  (See Clip Map Section 3.4)

            The ClipMap function "clips" or removes parts of an object's
            geometry using either image maps or procedural textures.

            This function is described in detail in the ClipMap Section 3.4.

            User Interface:  The ClipMap controls are located on the Objects Panel.
        */
        void ClipMap()
        {
        }

        /*  TextureImage <path + filename> [ (sequence) ]

            example:  TextureImage Images\LWLogo.tga

            The TextureImage function provides the path and
            filename for the image to be loaded. The path is
            generated by checking the current content directory 
            for the listed filename.  

            In this example if the current content directory
            is <c:\NewTek>, LightWave would attempt to load
            the file <c:\NewTek\Images\LWLogo.tga>.

            It is possible to use image sequences as texture
            images. If an image sequence is chosen, a sequence
            identifier is appended to the image path and filename.
            An image sequence also produces an additional
            ImageSequenceInfo listing. (See Image Sequences Section 1.5)
        */
        void TextureImage()
        {
        }

        /*  TextureFlags <bit-field value>

            example:  TextureFlags 12

            The TextureFlags function provides additional texture settings.  

            The bit-field value is produced by calculating the
            decimal value of a 4 position bit-field whose bits
            represent logical on/off switches that are number
            left to right from 0 - 3. The least-significant bit
            for this field is the rightmost bit.  Each channel
            has a corresponding bit in the bit-field.  When a
            texture setting is chosen, its bit (or switch) 
            is turned on.

            <bit positions>:
                0 - World Coordinates
                1 - Negative Image
                2 - Pixel Blending
                3 - Antialiasing
        */
        void TextureFlags()
        {
            int texture_flags = file.read_int();
        }

        void TextureAxis()
        {
            int texture_axis = file.read_int();
        }

        /*  TextureSize <X size> <Y size> <Z size>

            example:  TextureSize 1.5 2 5.25

            The TextureSize function provides the x, y and z
            dimensions of the image map or the procedural texture.

            The arguments are given in meters.
        */
        void TextureSize()
        {
            double texture_x_size = file.read_double();
            double texture_y_size = file.read_double();
            double texture_z_size = file.read_double();
        }

        /*  TextureCenter <X position> <Y position> <Z position>

            example:  TextureCenter 5 5 10

            The TextureCenter function provides the X, Y, and Z
            positions of the center of the image map or the procedural
            texture.

            The argument are given in meters from the origin (0,0,0)
        */
        void TextureCenter()
        {
            double texture_x_center = file.read_double();
            double texture_y_center = file.read_double();
            double texture_z_center = file.read_double();
        }

        /*  TextureFalloff <X percentage> <Y percentage> <Z percentage>

            example:  TextureFalloff 25 10.5 25

            This function provides the percentage of falloff per
            meter in the X, Y and Z directions.

            The arguments are given in percentage per unit measure
            (meter).
        */
        void TextureFalloff()
        {
            double texture_x_falloff = file.read_double();
            double texture_y_falloff = file.read_double();
            double texture_z_falloff = file.read_double();
        }

        /*  TextureVelocity <X  velocity> <Y velocity> <Z velocity>

            example:  TextureVelocity 2.5 0 0

            The TextureVelocity function provides the velocity
            of image map or procedural texture. The value given
            is the value in units per frame. The example would 
            move the center of the texture 2.5 meters per frame
            in the positive x direction.
        */
        void TextureVelocity()
        {
            double texture_x_velocity = file.read_double();
            double texture_y_velocity = file.read_double();
            double texture_z_velocity = file.read_double();
        }

        /*  Texture Amplitude <float>  / Texture Value <percentage>

            -  TextureAmplitude (Displacement map only)

            example:  TextureAmplitude .25

            The TextureAmplitude function provides the amplitude
            (offset amount) for the displacement map.
        */
        void TextureAmplitude()
        {
            double texture_amplitude = file.read_double();
        }

        /*  -  TextureValue (Clip map only)

            example:  TextureValue 0.500000

            The TextureValue function provides a percentage value
            for the texture.
        */
        void TextureValue()
        {
            double texture_value = file.read_double();
        }

        /*  TextureInt(index) <int>   (Multiple Instances Possible)

            example:  TextureInt0 3

            The TextureInt function provides an integer value for
            a parameter of the texture selected in the DisplacementMap
            or ClipMap functions. This function may be called multiple
            times, each time the index is incremented and added to the
            function name. For instance, the third parameter that
            required an integer value would be TextureInt2.
        */
        void TextureInt()
        {
            file.skip();
            //int texture_int = file.read_int();
        }

        /*  TextureFloat(index) <float>  (Multiple Instances Possible)

            example:  TextureFloat2 0.250000

            The TextureFloat function provides a floating point
            value for a parameter of a texture selected in the
            DisplacementMap or ClipMap functions. This function
            may be called multiple times, each time the index is
            incremented and added to the function name. For
            instance, the third parameter that required a floating
            point value would be TextureFloat2.
        */
        void TextureFloat()
        {
            file.skip();
            //double texture_float 
        }
    }
}

/*
  The DisplacementMap function deforms the geometry of an
  object using either image maps or procedural textures.

  The ClipMap function "clips" or removes parts of an
  object's geometry using either image maps or procedural
  textures.

  Two types of mapping available to these functions. An image
  map or a built-in procedural texture can be used with these
  functions.


        DisplacementMap ¦ ClipMap *
        TextureImage *  (Image Mapping only)
        TextureFlags *
        TextureAxis *  (Image Mapping only)
        TextureSize *
        TextureCenter
        TextureFalloff
        TextureVelocity
        TextureAmplitude ¦  TextureValue
        TextureInt(index)   (Multiple instances possible) 
        TextureFloat(index)   (Multiple instances possible)
*/

