using System.Collections.Generic;
using System.IO;

using RenderStack.Math;

using ID4 = System.UInt32;
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
    public class LWItem
    {
        public enum ItemVisibility
        {
            Hidden = 0,
            BoundingBox = 1,
            VerticesOnly = 2,
            Wireframe = 3,
            FrontFaceWireframe = 4,
            ShadedSolid = 5,
            TexturedShadedSolid = 6
        }

        public  int         Active = 1;
        public  ulong       ItemID;
        public  int         Refresh;
        public  int         WireColor;
        public  Vector2     SchematicPosition;
        public  Vector3     PivotPosition;
        public  string      Name;
        public  ulong       ParentObjectId = 0xffffffff;
        public  LWItem      ParentObject;
        public  LWMotion    Motion;

        public LWItem()
        {
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

#if false
        float Time
        {
            set
            {
                //Vector3 position = evalPosition(value);
                //model.setPosition( position );
                //model.getAttitude() = evalRotation( time );
            }
        }
#endif

#if false
        public Vector3 evalPosition(float time)
        {
            //  Evaluate
            LWChannelEnvelope x_env = Motion.Channel(LWChannel.X);
            LWChannelEnvelope y_env = Motion.Channel(LWChannel.Y);
            LWChannelEnvelope z_env = Motion.Channel(LWChannel.Z);

            double x = (x_env != null) ? x = x_env.eval(time) : 0.0;
            double y = (y_env != null) ? y = y_env.eval(time) : 0.0;
            double z = (z_env != null) ? z = z_env.eval(time) : 0.0;

            Vector3 position = new Vector3(x, y, z);

            if(ParentObject != null)
            {
                position  = ParentObject.evalRotation(time) * position;
                position += ParentObject.evalPosition(time);
            }
            position += evalPivot(time);

            return position;
        }
        Vector3 evalPivot(float time)
        {
            Vector3 pivot = evalRotation(time) * -PivotPosition;
            return pivot;
        }
        Matrix4 evalRotation(float time)
        {
            //  Evaluate
            LWChannelEnvelope h_env = Motion.Channel(LWChannel.H);
            LWChannelEnvelope p_env = Motion.Channel(LWChannel.P);
            LWChannelEnvelope b_env = Motion.Channel(LWChannel.B);

            float h = (h_env != null) ? h = h_env.eval(time) : 0.0f;
            float p = (p_env != null) ? p = p_env.eval(time) : 0.0f;
            float b = (b_env != null) ? b = b_env.eval(time) : 0.0f;

            Matrix4 rotation = Matrix4.Identity;
#if true
            Quaternion 
            q  = new Quaternion(Vector3.UnitZ,  -b);
            q *= new Quaternion(Vector3.UnitX,  -p);
            q *= new Quaternion(Vector3.UnitY,  -h);
            Matrix4.CreateFromQuaternion(ref q, out rotation);
            //last_rotation = q;
#else
            last_rotation.rotateYMatrix(  h );
            last_rotation.rotateX      (  p );
            last_rotation.rotateZ      (  b );
#endif

            if(parentObject != null)
            {
                rotation = parentObject.evalRotation(time) * rotation;
            }

            return rotation;
        }
#endif
    }
}