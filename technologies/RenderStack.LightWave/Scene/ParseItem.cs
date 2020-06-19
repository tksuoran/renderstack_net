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
        void ItemActive()
        {
            currentItem.Active = file.read_int();
        }

        /*  PivotPoint <x position> <y position> <z position>

            example:  PivotPoint 0 16.9 -11.6

            The PivotPoint function provides the x, y, and z
            positions for the pivot point of the object. This
            determines the center of rotation for the current
            object. The position values are given as a distance
            (meters) of offset from the original object center.

            User Interface:
            
            The PivotPoint values are set from the Move Pivot Point 
            mouse function in Layout.
        */
        void PivotPosition()
        {
            double x = file.read_double();
            double y = file.read_double();
            double z = file.read_double();

            //z = -z;  \todo?

            currentObject.PivotPosition = new Vector3(x, y, z);
        }

        /*  When a scene file needs to refer to specific items to establish item
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
        */
        void ParentItem()
        {
            ulong all_bits = file.read_hex_int();

            currentItem.ParentObjectId = all_bits;
        }

        /*  ParentObject <object instance>

            example:     ParentObject 4

            The ParentObject function provides LightWave with the
            current object's parent object in the hierarchical chain.
            The value is equal to the parent objects position in the
            loading sequence. The example function would parent the
            current object to the fourth object instance in the scen
            file. When the ParentObject function is active, all keyframe 
            information for the object becomes an offset from the
            parents information.

            User Interface:

            Objects hierarchies are created by selecting the parent 
            function from layout. The parent object is then selected 
            from the pop-up listing provided.
        */
        void ParentObject()
        {
            currentObject.ParentObjectId = (ulong)file.read_int();
        }

        void SchematicPosition()
        {
            double x = file.read_double();
            double y = file.read_double();
            currentItem.SchematicPosition = new Vector2(x, y);
        }

        void KeyableChannels()
        {
            string str_channels = file.read_string();
        }
    }
}

