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
        public void LWSC()
        {
            scene.LwsVersion = file.read_int();
        }

        /*  FirstFrame <frame number>

            example:  FirstFrame 1

            The FirstFrame function provides the starting frame for the rendering process. 
            This is a global function for all LightWave functions that use frame 
            information
        */
        void FirstFrame()
        {
            scene.FirstFrame = file.read_int();
        }

        /*  LastFrame <frame number>

            example:  LastFrame 30

            The LastFrame function provides the last frame to be rendered.  This also 
            supplies the defaults for the MakePreview function and the current frame 
            slider from Layout with the last frame information.  This is a global 
            function for all LightWave functions that use frame information.
        */
        void LastFrame()
        {
            scene.LastFrame = file.read_int();
        }

        /*  FrameStep <int>

            example:  FrameStep 1

            The FrameStep function provides the number of frames to increment between 
            rendered frames during the rendering process.  
        */
        void FrameStep()
        {
            scene.FrameStep = file.read_int();
        }

        /*  FramesPerSecond <float>

            example:  FramesPerSecond 30.000000

            The FramesPerSecond function provides the number of frames per second.
            This controls the duration of each frame. 
        */
        void FramesPerSecond()
        {
            scene.FramesPerSecond = file.read_int();
        }

        /*  PreviewFirstFrame nfirst
            PreviewLastFrame nlast
            PreviewFrameStep nstep 

            The frame range and step size for previewing. These may be unrelated
            to the values for rendering. They also control the visible ranges of
            certain interface elements, for example the frame slider in the main
            window. 
        */
        void PreviewFirstFrame()
        {
            scene.PreviewFirstFrame = file.read_int();
        }
        void PreviewLastFrame()
        {
            scene.PreviewLastFrame = file.read_int();
        }
        void PreviewFrameStep()
        {
            scene.PreviewFrameStep = file.read_int();
        }

        /*  CurrentFrame nframe 

            The frame displayed in the interface when the scene is loaded.
        */
        void CurrentFrame()
        {
            scene.CurrentFrame = file.read_int();
        }

        void Plugin()
        {
            Token   tmp;
            string  unknown_1 = file.read_string();
            int     unknown_2 = file.read_int();
            string  unknown_3 = file.read_string();

            do
            {
                tmp = file.read_token();
            }
            while
            (
                (tmp != Token.EndPlugin) &&
                (tmp != Token.LWS_ERROR     ) &&
                (tmp != Token.LWS_EOF       )
            );
        }

        void EndPlugin()
        {
        }
    }
}



/*
  The LightWave scene file is a standard ASCII text file that contains the 
  information necessary to reconstruct a LightWave Scene.

  In addition to the objects, lights and camera, the scene file contains 
  standard Layout settings that are set on a per scene basis.  Layout 
  information that is more "permanent" is saved in the LightWave config 
  file. (For LightWave Config File information see Chapter 10.)

  The object geometry is not included in the scene file.  For each object 
  instance the objects path and filename are listed along with other scene 
  specific attributes.  The objects are stored as seperate binary files 
  that contain the object geometry along with the objects surface attributes.

  Objects are not saved when the Save Scene function is selected from the 
  Scene Menu.  The user is required to save these by going to the Objects Menu 
  and selecting either the Save Object or Save All Objects function.  This 
  saves each object as an individual file and sets the path and filename that 
  will be saved for that object instance in the scene file.

  Values are listed to 6 decimal places.
  Position values are given in meters.
  Rotation values are given in degrees.
  Scaling values are given as a multiplier.
  Frame numbers are given as a value greater than or equal to 0.

  Color values are given as an Red, Green, and Blue triple, each with a range 
  of 0 to 255.

  TIS: Appears to be floats in range 0..1 in LWSC 3

The scene files basic structure is divided into separate logical sections.  
Each section relates to a group of LightWave's functions.  These sections 
are arranged in an order similar to the menu structure in LightWave's Layout.  

    Scene
    Objects
    Lights
    Camera
    Effects
    Render
    Layout Options

  The Scene Section consists of the LightWave scene file header and frame 
  information.  The functions in this section always produce a listing 
  and have no optional functions.

The following scene functions are listed in the order in which they appear 
in the scene file.

  Italicized entries denote function labels and are not true function names.

        LWSC
        Scene File Version

        FirstFrame
        LastFrame
        FrameStep
        FramesPerSecond

Item Numbers

When a scene file needs to refer to specific items to establish item
relationships (parenting, for example), it uses item numbers. Items
are numbered in the order in which they appear in the file, starting
with 0.

Item numbers can be written in one of two ways, depending on which
keyword they're used with. In general, if the type of the item
(object, bone, light, camera) can be determined from the keyword
alone, the item number will simply be the ordinal, written as a
decimal integer. When the keyword can be used with items of more
than one type, the item number is an unsigned integer written as
an 8-digit hexadecimal string, the format produced by the C-language
"%8X" print format specifier, and the high bits identify the item type.

The first hex digit (most significant 4 bits) of the hex item number
string identifies the item type.

1 - Object
2 - Light
3 - Camera
4 - Bone

The other digits make up the item number, except in the case of
bones. For bones, the next 3 digits (bits 16-27) are the bone
number and the last 4 digits (bits 0-15) are the object number
for the object the bone belongs to. Some examples:

10000000 - the first object
20000000 - the first light
4024000A - the 37th bone (24 hex) in the 11th object (0A hex)

Blocks

Information in a scene file is organized into blocks, the ASCII
text analog of the chunks described in the IFF specification.
Each block consists of an identifier or name followed by some
data. The format of the data is determined by the block name.
Block names resemble C-style identifiers. In particular, they
never contain spaces or other non-alphanumeric characters.

A single-line block is delimited by the newline that terminates
the line. Multiline blocks are delimited by curly braces (the { and }
characters, ASCII codes 123 and 125). The name of a multiline block
follows the opening curly brace on the same line. The curly brace
and the name are separated by a single space. The data follows
on one or more subsequent lines. Each line of data is indented
using two spaces. The closing brace is on a line by itself and
is not indented.

Individual data elements are separated from each other by a
single space. String data elements are enclosed in double
quotes and may contain spaces.

Blocks can be nested. In other words, the data of a block
can include other blocks. A block that contains nested
blocks is always a multiline block. At each nesting level,
the indention of the data is incremented by two additional
spaces. 

   SingleLineBlock data
   { MultiLineBlock
     data
     { NestedMultiLineBlock
       data
     }
   }
*/

