using System.Collections.Generic;
using System.Diagnostics;

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
    public partial class LWModelParser
    {
        /*  LWO2 BLOK

            A surface may contain any number of 'blocks' which hold texture layers or
            shaders. The blocks are defined by a sub-chunk with the following formacurrentTexture. 

            BLOK { 
                    type { ordinal[ID4], header[SUB-CHUNK] * },
                            attributes[SUB-CHUNK] * 
            } 

            Immediately inside the block sub-chunk is another sub-chunk which is the
            header for the block. The type code for this header block defines the type
            of the block. This header block contains an ordinal string followed by any
            number and type of header sub-chunks. Following the required header sub-chunk,
            sub-chunks which contain the attributes of this block are found in any order.
            The chunks here will depend on the overall type of the block. 

            Ordinary string compare functions are used to sort the ordinal strings. These
            are never read by the user, so they can contain any characters from 1 to 255
            and tend to be very shorcurrentTexture. The only rule is that an ordinal string must not
            end with a 1 byte, since that prevents arbitrary insertion. 

            The header for any block can contain the following sub-chunks, although some
            are ignored for different block types. 
        */
        public void BLOKchunks()
        {
            currentTexture = null;

            while(f.Left() > 0)
            {
                ID subchunk = f.ReadID4();
                U2 length   = f.ReadU2();

                // LW8 objects with AUVN subchunks for example can
                // can subchunk length odd, even though in file
                // all chunk lengths are even
                while(length % 2 != 0)
                { 
                    ++length;
                }

                Debug.WriteLine("\t\t" + subchunk + " " + length + " bytes");

                f.Push(length);

                switch(subchunk.value)
                {
                    //  headers
                    case ID.IMAP:
                    {
                        //  texture_layer which we parse in lw_surface_blok
                        currentTexture = new LWTexture();
                        currentTexture.BlockType = subchunk;
                        currentTexture.Ordinal = f.ReadS0();

                        //  \todo add to currentTextureStack TODO MUSTFIX

                        ReadBlockHeader();
                        break;
                    }
                    case ID.PROC: goto case ID.IMAP;
                    case ID.GRAD: goto case ID.IMAP;
                    case ID.SHDR: goto case ID.IMAP;

                    case ID.TMAP: read_texture_map                 (); break;

                    // IMAP
                    case ID.PROJ: read_projection_mode_U2          (); break;
                    case ID.AXIS: read_major_axis_U2               (); break;
                    case ID.IMAG: read_image_map_VX                (); break;
                    case ID.WRAP: read_wrap_options_U2_U2          (); break;
                    case ID.WRPW: read_wrap_width_amount_FP4_VX    (); break;
                    case ID.WRPH: read_wrap_height_amount_FP4_VX   (); break;
                    case ID.VMAP: read_uv_vertex_map_S0            (); break;
                    case ID.AAST: read_antialiasing_strength_U2_FP4(); break;
                    case ID.PIXB: read_pixel_blending_U2           (); break;
                    case ID.STCK: read_stack_VX                    (); break;
                    case ID.TAMP: read_amplitude_FP4_VX            (); break;
                    case ID.NEGA: read_negative_U2                 (); break;

                    case ID.AUVN: read_anim_uv_plugin_name         (); break;
                    case ID.AUVU: read_anim_uv_plugin_user         (); break;
                    case ID.AUVO: read_anim_uv_plugin_data         (); break;

                    // PROC
                    case ID.VALU: read_procedural_basic_value_FP4_1_or_3(); break;

                    // PROC and SHDR
                    case ID.FUNC: read_function_S0_data             (); break;

                    // GRAD
                    case ID.PNAM: read_gradient_parameter_S0          (); break;
                    case ID.INAM: read_gradient_item_S0               (); break;
                    case ID.GRST: read_gradient_range_start_FP4       (); break;
                    case ID.GREN: read_gradient_range_end_FP4         (); break;
                    case ID.GRPT: read_gradient_repeat_U2             (); break;
                    case ID.FKEY: read_gradient_keys_FP4_data_FP4     (); break;
                    case ID.IKEY: read_gradient_key_parameters_data_U2(); break;

                    default:
                    {
                        Trace.TraceError("SURF::BLOK parser unknown chunk " + subchunk);
                        break;
                    }
                }

                f.Pop(true);
            }
        }

        void ReadBlockHeader()
        {
            while(f.Left() > 0)
            {
                ID subchunk = f.ReadID4();
                U2 length   = f.ReadU2 ();

                Debug.WriteLine("\t\t\t" + subchunk + " " + length + " bytes");

                f.Push(length);

                switch(subchunk.value)
                {
                    case ID.CHAN: read_channel_ID4         (); break;
                    case ID.ENAB: read_enable_U2           (); break;
                    case ID.OPAC: read_opacity_U2_FP4_VX   (); break;
                    case ID.AXIS: read_displacement_axis_U2(); break;
                    case ID.NEGA: read_nega_U2             (); break;
                }

                f.Pop(true);
            }
        }

        void read_anim_uv_plugin_name()
        {
            //  \todo read using length?
            currentTexture.UVAnimUnknown1    = f.ReadU1(); // length of m_uv_anim_plugin_name
            currentTexture.UVAnimPluginName  = f.ReadS0();
            currentTexture.UVAnimUnknown2    = f.ReadU2(); // ??
        }

        void read_anim_uv_plugin_user()
        {
            currentTexture.UVAnimUnknown3 = f.ReadU1(); // length of m_uv_anim_plugin_user
            currentTexture.UVAnimPluginUser = f.ReadS0();
        }

        void read_anim_uv_plugin_data()
        {
            currentTexture.UVAnimPluginDataLength = (int)f.Left();
            currentTexture.UVAnimPluginData = new U1[currentTexture.UVAnimPluginDataLength];
            for(int i = 0; i < currentTexture.UVAnimPluginDataLength; ++i)
            {
                currentTexture.UVAnimPluginData[i] = f.ReadU1(); // in big endian format!
            }
        }

        /*  BLOK Header Channel 

            CHAN { texture-channel[ID4] } 

            This is required in all texture layer blocks and can have a
            value of COLR, DIFF, LUMI, SPEC, GLOS, REFL, TRAN, RIND, TRNL,
            or BUMP, The texture layer is applied to the corresponding
            surface attribute. If present in a shader block, this value
            is ignored. 
        */
        void read_channel_ID4()
        {
            currentTexture.TextureChannel = f.ReadID4();
            currentTextureStack = currentSurface.FindOrCreateTextureStack(currentTexture.TextureChannel);
            currentTextureStack.Layers[currentTexture.Ordinal] = currentTexture;
        }

        /*  BLOK Header Enable State 

            ENAB { enable[U2] } 

            True if the texture layer or shader should be evaluated during
            rendering. If ENAB is missing, the block is assumed to be enabled. 
        */
        void read_enable_U2()
        {
            currentTexture.Enable = f.ReadU2();
        }

        /*  BLOK Header Opacity 

            OPAC { type[U2], opacity[FP4], envelope[VX] } 

            Opacity is valid only for texture layers. It specifies how opaque
            the layer is with respect to the layers before it (beneath it) on
            the same channel, or how the layer is combined with the previous
            layers. The types can be 

            0 - Additive
            1 - Subtractive
            2 - Difference
            3 - Multiply
            4 - Divide
            5 - Alpha
            6 - Texture Displacement

            Alpha opacity uses the current layer as an alpha channel. The
            previous layers are visible where the current layer is white
            and transparent where the current layer is black. Texture
            Displacement distorts the underlying layers. If OPAC is
            missing, 100% Additive opacity is assumed. 
        */
        void read_opacity_U2_FP4_VX()
        {
            currentTexture.OpacityType       = f.ReadU2 ();
            currentTexture.Opacity.Value     = f.ReadFP4();
            currentTexture.Opacity.Envelope  = f.ReadVX ();
        }

        /*  BLOK Header NEGA

            NEGA { unknown[U2] } 
        */
        void read_nega_U2()
        {
            currentTexture.Negative = f.ReadU2();
        }

        /*  BLOK Header Displacement Axis

            AXIS { displacement-axis[U2] } 

            For displacement mapping, defines the plane from which
            displacements will occur. The value is 0, 1 or 2 for
            the X, Y or Z axis. 
        */
        void read_displacement_axis_U2()
        {
            currentTexture.DisplacementAxis = f.ReadU2();
        }

        /*  BLOK Texture Mapping

            Image map and procedural textures employ the TMAP subchunk
            to define the mapping they use to get from object or world
            coordinate space to texture space. 

            TMAP { attributes[SUB-CHUNK] * } 

            The TMAP subchunk contains a set of attribute chunks which
            describe the different aspects of this mapping. 
        */
        void read_texture_map()
        {
            while(f.Left() > 0)
            {
                ID subchunk = f.ReadID4();
                U2 length   = f.ReadU2 ();

                Debug.WriteLine("\t\t\t" + subchunk + " " + length + " bytes");

                f.Push(length);
                switch(subchunk.value)
                {
                    case ID.CNTR: read_texture_center_VEC12_VX     (); break;
                    case ID.SIZE: read_texture_size_VEC12_VX       (); break;
                    case ID.ROTA: read_texture_rotation_VEC12_VX   (); break;
                    case ID.OREF: read_texture_reference_object_S0 (); break;
                    case ID.FALL: read_texture_falloff_U2_VEC12_VX (); break;
                    case ID.CSYS: read_texture_coordinate_system_U2(); break;

                    //  New blok state?
                    default:
                    {
                        Trace.TraceError("Unknown chunk " + subchunk);
                        break;
                    }
                }

                f.Pop(true);
            }
        }

        /*  BLOK::TMAP Position, Orientation and Size 

            CNTR, SIZE, ROTA { vector[VEC12], envelope[VX] } 

            These subchunks each consist of a vector for the texture's
            size, center and rotation. The size and center are normal
            positional vectors in meters, and the rotation is a vector
            of heading, pitch and bank in radians. If missing, the
            center and rotation are assumed to be zero. The size should
            always be specified if it si to be used for any given mapping. 
        */
        void read_texture_center_VEC12_VX()
        {
            currentTexture.TextureCenter.Value    = f.ReadVEC12();
            currentTexture.TextureCenter.Envelope = f.ReadVX   ();
        }

        void read_texture_size_VEC12_VX()
        {
            currentTexture.TextureSize.Value    = f.ReadVEC12();
            currentTexture.TextureSize.Envelope = f.ReadVX   ();
        }

        void read_texture_rotation_VEC12_VX()
        {
            currentTexture.TextureRotation.Value    = f.ReadVEC12();
            currentTexture.TextureRotation.Envelope = f.ReadVX   ();
        }

        /*  BLOK::TMAP Reference Object 

            OREF { object-name[S0] } 

            Specifies a reference object for the texture. The reference
            object is given by name, and the scene position, rotation
            and scale of the object are combined with the previous
            chunks to compute the texture mapping. If the object name
            is "(none)" or OREF is missing, no reference object is used.
        */
        void read_texture_reference_object_S0()
        {
            currentTexture.TextureReferenceObject = f.ReadS0();
        }

        /*  BLOK::TMAP Falloff 

            FALL { type[U2], vector[VEC12], envelope[VX] } 

            Texture effects may fall off with distance from the texture
            center if this subchunk is presencurrentTexture. The vector represents a
            rate per unit distance along each axis. The type can be

            0 - Cubic;     Falloff is linear along all three axes independently. 
            1 - Spherical; Falloff is proportional to the Euclidean distance from the center. 
            2 - Linear X
            3 - Linear Y
            4 - Linear Z 

            Falloff is linear only along the specified axis. The other
            two vector components are ignored. 
        */
        void read_texture_falloff_U2_VEC12_VX()
        {
            currentTexture.TextureFalloffType      = f.ReadU2   ();
            currentTexture.TextureFalloff.Value    = f.ReadVEC12();
            currentTexture.TextureFalloff.Envelope = f.ReadVX   ();
        }

        /*  BLOK::TMAP Coordinate System 

            CSYS { type[U2] } 

            The coordinate system can be 0 for object coordinates
            (the default if the chunk is missing) or 1 for world
            coordinates. 
        */
        void read_texture_coordinate_system_U2()
        {
            currentTexture.TextureCoordinateSystem = f.ReadU2();
        }

        /*  BLOK::IMAP Projection Mode 

            PROJ { projection-mode[U2] } 

            The projection defines how 2D coordinates in the image are
            transformed into 3D coordinates in the scene. In the following
            list of projections, image coordinates are called r (horizontal)
            and s (vertical).

            0 - Planar 

            The image is projected on a plane along the major axis
            (specified in the AXIS subchunk). r and s map to the
            other two axes. 

            1 - Cylindrical 

            The image is wrapped cylindrically around the major axis.
            r maps to longitude (angle around the major axis). 

            2 - Spherical 

            The image is wrapped spherically around the major axis.
            r and s map to longitude and latitude. 

            3 - Cubic 

            Like Planar, but projected along all three axes. The dominant
            axis of the geometric normal selects the projection axis for
            a given surface spocurrentTexture. 

            4 - Front Projection 

            The image is projected on the current camera's viewplane.
            r and s map to points on the viewplane. 

            5 - UV 

            r and s map to points (u, v) defined for the geometry using a
            vertex map (identified in the BLOK's VMAP subchunk). 
        */
        void read_projection_mode_U2()
        {
            currentTexture.ProjectionMode = f.ReadU2();
        }

        /*  BLOK::IMAP Major Axis 

            AXIS { texture-axis[U2] } 

            The major axis used for planar, cylindrical and spherical
            projections. The value is 0, 1 or 2 for the X, Y or Z axis. 
        */
        void read_major_axis_U2()
        {
            currentTexture.MajorAxis = f.ReadU2();
        }

        /*  BLOK::IMAP Image Map 

            IMAG { texture-image[VX] } 

            The CLIP index of the mapped image. 
        */
        void read_image_map_VX()
        {
            currentTexture.ImageMap = f.ReadVX();
        }

        /*  BLOK::IMAP Image Wrap options 

            WRAP { width-wrap[U2], height-wrap[U2] } 

            Specifies how the color of the texture is derived for
            areas outside the image.

            0 - Reset 

            Areas outside the image are assumed to be black. The ultimate
            effect of this depends on the opacity settings. For an additive
            texture layer on the color channel, the final color will come
            from the preceding layers or from the base color of the surface. 

            1 - Repeat (default)

            The image is repeated or tiled. 

            2 - Mirror 

            Like repeat, but alternate tiles are mirror-reversed. 

            3 - Edge 

            The color is taken from the image's nearest edge pixel. 
        */
        void read_wrap_options_U2_U2()
        {
            currentTexture.WidthWrap  = f.ReadU2();
            currentTexture.HeightWrap = f.ReadU2();
        }

        /*  BLOK::IMAP Image Wrap Amount 

            WRPW, WRPH { cycles[FP4], envelope[VX] } 

            For cylindrical and spherical projections, these parameters
            control how many times the image repeats over each full interval. 
        */
        void read_wrap_width_amount_FP4_VX()
        {
            currentTexture.WrapWidthCycles.Value    = f.ReadFP4();
            currentTexture.WrapWidthCycles.Envelope = f.ReadVX ();
        }
        void read_wrap_height_amount_FP4_VX()
        {
            currentTexture.WrapHeightCycles.Value    = f.ReadFP4();
            currentTexture.WrapHeightCycles.Envelope = f.ReadVX ();
        }

        /*  BLOK::IMAP UV Vertex Map 

            VMAP { txuv-map-name[S0] } 

            For UV projection, which depends on texture coordinates at
            each vertex, this selects the name of the TXUV vertex map
            that contains those coordinates. 
        */
        void read_uv_vertex_map_S0()
        {
            currentTexture.UVVertexMap = f.ReadS0();
        }

        /*  BLOK::IMAP Antialiasing Strength 

            AAST { flags[U2], antialising-strength[FP4] } 

            The low bit of the flags word is an enable flag for texture
            antialiasing. The antialiasing strength is proportional to
            the width of the sample filter, so larger values sample a
            larger area of the image.
        */
        void read_antialiasing_strength_U2_FP4()
        {
            currentTexture.AntialiasingType     = f.ReadU2 ();
            currentTexture.AntialiasingStrength = f.ReadFP4();
        }


        /*  BLOK::IMAP Pixel Blending 

            PIXB { flags[U2] } 

            Pixel blending enlarges the sample filter when it would
            otherwise be smaller than a single image map pixel. If
            the low-order flag bit is set, then pixel blending is
            enabled.
        */
        void read_pixel_blending_U2()
        {
            currentTexture.PixelBlending = f.ReadU2();
        }

        /*  BLOK::IMAP Stack

            STCK { value[FP4], envelope[VX] } 

            Reserved for future use. The default value is 0.0.
        */
        void read_stack_VX()
        {
            currentTexture.Stack = f.ReadVX();
        }

        /*  BLOK::IMAP Texture Amplitude 

            TAMP { amplitude[FP4], envelope[VX] } 

            Appears in image texture layers applied to the bump channel.
            Texture amplitude scales the bump height derived from the
            pixel values. The default is 1.0.
        */
        void read_amplitude_FP4_VX()
        {
            currentTexture.Amplitude.Value    = f.ReadFP4();
            currentTexture.Amplitude.Envelope = f.ReadVX ();
        }

        void read_negative_U2()
        {
            currentTexture.Negative = f.ReadU2();
        }

        /*  BLOK::PROC Basic Value 

            VALU { value[FP4] # (1, 3) } 

            Procedurals are often modulations between the current
            channel value and another value, given here. This may
            be a scalar or a vector. 
        */
        void read_procedural_basic_value_FP4_1_or_3()
        {
            int i = 0;
            while(f.Left() > 0)
            {
                currentTexture.ProceduralBasicValue[i] = f.ReadFP4();
                ++i;
            }
        }


        /*  BLOK::PROC Algorithm and Parameters 

            FUNC { algorithm-name[S0], data[...] } 

            The FUNC subchunk names the procedural and stores its
            parameters. The name will often map to a plug-in name.
            The variable-length data following the name belongs to
            the procedural.
        */
        void read_procedural_function_S0_data()
        {
            currentTexture.ProceduralFunction = f.ReadS0();
            int left = (int)f.Left();

            Debug.WriteLine("Procedural function " + currentTexture.ShaderFunction + " " + left + " bytes of data");

            if(left > 0)
            {
                currentTexture.ProceduralData = new byte[left];
                for(int i = 0; i < left; ++i)
                {
                    currentTexture.ProceduralData[i] = f.ReadU1();
                }
            }
        }
        /*  BLOK::SHDR Shader Algorithm 

            FUNC { algorithm-name[S0], data[...] } 

            Just like a procedural texture layer, a shader
            is defined by an algorithm name (often a plug-in),
            followed by data owned by the shader.
        */
        void read_function_S0_data()
        {
            currentTexture.ShaderFunction = f.ReadS0();
            int left = (int)f.Left();

            Debug.WriteLine("Shader function " + currentTexture.ShaderFunction + " " + left + " bytes of data");

            if(left > 0)
            {
                currentTexture.ShaderData = new byte[left];
                for(int i = 0; i < left; ++i)
                {
                    currentTexture.ShaderData[i] = f.ReadU1();
                }
            }
        }

        /*  BLOK::GRAD Parameter Name 

            PNAM { parameter[S0] } 

            The input parameter. Possible values include

            "Previous Layer"
            "Bump"
            "Slope"
            "Incidence Angle"
            "Light Incidence"
            "Distance to Camera"
            "Distance to Object"
            "X Distance to Object"
            "Y Distance to Object"
            "Z Distance to Object"
            "Weight Map"
        */
        void read_gradient_parameter_S0()
        {
            currentTexture.GradientParameter = f.ReadS0();
        }

        /*  BLOK::GRAD Item Name 

            INAM { item-name[S0] } 

            The name of a scene item. This is used when the input
            parameter is derived from a property of an item in the
            scene.
        */
        void read_gradient_item_S0()
        {
            currentTexture.GradientItem = f.ReadS0();
        }

        /*  BLOK::GRAD Gradient Range

            GRST, GREN { input-range[FP4] }

            The start and end of the input range. These values only
            affect the display of the gradient in the user interface.
            They don't affect rendering.
        */
        void read_gradient_range_start_FP4()
        {
            currentTexture.GradientRangeStart = f.ReadFP4();
        }

        void read_gradient_range_end_FP4()
        {
            currentTexture.GradientRangeEnd = f.ReadFP4();
        }

        /*  BLOK::GRAD Repeat Mode 

            GRPT { repeat-mode[U2] } 

            The repeat mode. This is currently undefined.
        */
        void read_gradient_repeat_U2()
        {
            currentTexture.GradientRepeat = f.ReadU2();
        }

        /*  BLOK::GRAD Key Values 

            FKEY { ( input[FP4], output[FP4] # 4 )* } 

            The transfer function is defined by an array of keys,
            each with an input value and an RGBA output vector.
            Given an input value, the gradient can be evaluated
            by selecting the keys whose positions bracket the
            value and interpolating between their outputs. If the
            input value is lower than the first key or higher than
            the last key, the gradient value is the value of the
            closest key.
        */
        void read_gradient_keys_FP4_data_FP4()
        {
            while(f.Left() > 0)
            {
                FP4 key = f.ReadFP4();
                FP4 r   = f.ReadFP4();
                FP4 g   = f.ReadFP4();
                FP4 b   = f.ReadFP4();
                FP4 a   = f.ReadFP4();
                currentTexture.GradientKeys.Add(
                    new LWGradientKey(key, new Vector4(r, g, b, a))
                );
            }
        }


        /*  BLOK::GRAD Key Parameters 

            IKEY { interpolation[U2] * } 

            An array of integers defining the interpolation for the
            span preceding each key. Possible values include

            0 - Linear
            1 - Spline
            2 - Step
        */
        void read_gradient_key_parameters_data_U2()
        {
            currentTexture.GradientInterpolation = f.ReadU2();
        }

    }
}


/*  BLOK  Surface Blocks

    A surface may contain any number of blocks which hold
    texture layers or shaders. Each block is defined by a
    subchunk with the following formacurrentTexture.

    BLOK { header[SUB-CHUNK], attributes[SUB-CHUNK] * } 

    Since this regular expression hides much of the structure
    of a block, it may be helpful to visualize a typical texture
    block in outline form. 

    -block
        -header
            -ordinal string 
            -channel 
            -enable flag 
            -opacity... 
        -texture mapping
            -center 
            -size...    
        -other attributes... 

    The first subchunk is the header. The subchunk ID specifies
    the block type, and the subchunks within the header subchunk
    define properties that are common to all block types. The
    ordinal string defines the sorting order of the block relative
    to other blocks. The header is followed by other subchunks
    specific to each type. For some texture layers, one of these
    will be a texture mapping subchunk that defines the mapping
    from object to texture space. All of these components are
    explained in the following sections.

    Ordinal Strings

    Each BLOK represents a texture layer applied to one of the
    surface channels, or a shader plug-in applied to the surface.
    If more than one layer is applied to a channel, or more than
    one shader is applied to the surface, we need to know the
    evaluation order of the layers or shaders, or in what order
    they are "stacked." The ordinal string defines this order.

    Readers can simply compare ordinal strings using the C strcmp
    function to sort the BLOKs into the correct order. Writers of
    LWO2 files need to generate valid ordinal strings that put the
    texture layers and shaders in the right order. See the Object
    Examples supplement for an example function that generates
    ordinal strings.

    To understand how LightWave uses these, imagine that instead
    of strings, it used floating-point fractions as the ordinals.
    Whenever LightWave needed to insert a new block between two
    existing blocks, it would find the new ordinal for the inserted
    block as the average of the other two, so that a block inserted
    between ordinals 0.5 and 0.6 would have an ordinal of 0.55.

    But floating-point ordinals would limit the number of insertions
    to the (fixed) number of bits used to represent the mantissa.
    Ordinal strings are infinite-precision fractions written in
    base 255, using the ASCII values 1 to 255 as the digits (0 isn't
    used, since it's the special character that marks the end of the
    string).

    Ordinals can't end on a 1, since that would prevent arbitrary
    insertion of other blocks. A trailing 1 in this system is like
    a trailing 0 in decimal, which can lead to situations like this, 

       0.5    "\x80"
       0.50   "\x80\x01"

    where there's no daylight between the two ordinals for inserting
    another block.

    Image Maps

    Texture blocks with a header type of IMAP are image maps.
    These use an image to modulate one of the surface channels.
    In addition to the basic parameters listed below, the block
    may also contain a TMAP chunk. 

    Procedural Textures

    Texture blocks of type PROC are procedural textures
    that modulate the value of a surface channel
    algorithmically. 

    Gradient Textures

    Texture blocks of type GRAD are gradient textures that
    modify a surface channel by mapping an input parameter
    through an arbitrary transfer function. Gradients are
    represented to the user as a line containing keys. Each
    key is a color, and the gradient function is an interpolation
    of the keys in RGB space. The input parameter selects a
    point on the line, and the output of the texture is the
    value of the gradient at that poincurrentTexture. 

    Shaders

    Shaders are BLOK subchunks with a header type
    of SHDR. They are applied to a surface after
    all basic channels and texture layers are
    evaluated, and in the order specified by the
    ordinal sequence. The only header chunk they
    support is ENAB and they need only one data
    chunk to describe them. 
*/
