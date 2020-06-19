using System.Diagnostics;
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
        /*  AddBone

            example:  AddBone

            The AddBone function is the first function called in
            an object skeleton. It is called for each instance of
            a bone loading. This function will add a bone to the
            current object and produce a series of function listings
            for this bone.
        */
        void AddBone()
        {
            ulong boneIndex = (ulong)currentObject.Bones.Count; // add to the previous object
            ulong objectIndex = (ulong)scene.Objects.Count;

            currentItem = currentBone = new LWBone();
            currentItem.ItemID = 0x40000000;
            currentItem.ItemID |= (ulong)boneIndex << 16;
            currentItem.ItemID |= objectIndex;
            currentObject.Bones.Add(currentBone);
        }

        /*  BoneName  Bone ¦ <string>

            example:  BoneName FootBone

            The BoneName function provides a name for the bone
            created with the AddBone function. If the user renames
            the bone using the Rename Bone function from the Object
            Skeleton control panel the string is listed following
            the function name. If the user does not rename the bone,
            it is given the default name of Bone. If multiple bones
            instances have the same name, duplicate bones are given
            a numbered suffix to the name during the loading/creation
            process. This number is enclosed in parenthesis and
            follows the bone name.  

            An example:  FootBone (2) is the second instance of a
            bone with the name FootBone. The suffix is not saved in
            the scene file, and is used only as a user reference.
        */
        void BoneName()
        {
            currentBone.Name = file.read_string();
        }

        /*  ShowBone  <refresh value> <color value> 

            example:     ShowBone 1 2

            The ShowBone function determines how the bone is
            going to be displayed in Layout. The above example
            would display this (as opposed to hiding it) as
            a green bone.

             Refresh value

            This argument sets the bones display type in Layout.

            <Refresh value>:
                0 - No Refresh (Hide)
                1 - Refresh (Show)

            User Interface:  The refresh value is selected in the
            second column of the Scene Overview from the Scene Menu.      

             Color value

            This argument sets the color of the bone when not
            selected in Layout. When selected, all items highlight
            to yellow.

            <Color value>:
                1 - Blue
                2 - Green
                3 - Light Blue
                4 - Red
                5 - Purple
                6 - Orange
                7 - Gray

            User Interface:
            
            The color value is selected in the first column of the 
            Scene Overview from the Scene Menu.
        */
        void ShowBone()
        {
            currentBone.Refresh = file.read_int();
            currentBone.WireColor = file.read_int();
        }

        /*  BoneActive <flag>

            example:  BoneActive 1

            The BoneActive flag activates the bone in layout and
            will allow it to begin deforming the object's geometry.
            This function produces a BoneActive listing with a value
            of 1 when turned on. When turned off, this function 
            does not produce a listing in the scene file.

            <flag>:
                0 - Off (No function listing)
                1 - On

            User Interface:

            This function is set from the Object Skeleton control 
            panel or by the keyboard shortcut of <r> for rest.

            The BoneActive function is listed for all bones.
        */
        void BoneActive()
        {
            currentBone.Active = file.read_int();
        }

        /*  BoneRestPosition <x position> <y position> <z position>

            example:  BoneRestPosition 0.500000 0.200000 1.35000

            The BoneRestPosition function provides the initial
            rest x, y, z position of the bone. In this position,
            the bone does not influence (distort) the object geometry.  

            User Interface:

            The BoneRestPosition function is set from the 
            Object Skeleton control panel or by the keyboard 
            shortcut of <r> for rest.

            The BoneRestPosition function is listed for all bones.
        */
        void BoneRestPosition()
        {
            double x = file.read_double();
            double y = file.read_double();
            double z = file.read_double();
            currentBone.RestPosition = new Vector3(x, y, z);
        }

        /*  BoneRestDirection <Heading angle> <Pitch angle> <Bank angle>

            example:  BoneRestDirection 39.000000 7.900000 0.000000

            The BoneRestDirection function provides the initial
            rest H, P, B rotations of the bone. In this position,
            the bone does not influence (distort) the object geometry.

            User Interface:
            
            The BoneRestDirection function is set from the Object 
            Skeleton control panel or by the keyboard shortcut of <r> 
            for rest.

            The BoneRestDirection function is listed for all bones.
        */
        void BoneRestDirection()
        {
            double h = file.read_double();
            double p = file.read_double();
            double b = file.read_double();
            currentBone.RestPosition = new Vector3(h, p, b);
        }

        /*  BoneRestLength <float>

            example:  BoneRestLength 1.078000

            The BoneRestLength function provides the initial rest
            length of the bone. This is the "size" of the bone in Layout.

            User Interface:
            
            The BoneRestLength function is set from the Rest Length 
            field on the Object Skeleton control panel or by the 
            Rest Length mouse control in Layout.

            The BoneRestLength function is listed for all bones.
        */
        void BoneRestLength()
        {
            currentBone.RestLength = file.read_double();
        }

        /*  BoneStrength <float>

            example:  BoneStrength 2.500000

            The BoneStrength function provides the strength of a bone
            that is separate from it's rest length. This functions
            value is used when the ScaleBoneStrength flag is turned
            off (0). When the ScaleBoneStrength function is turned on, 
            the bone strength is equal to the BoneRestLength.

            User Interface:
            
            The BoneStrength functions value is set from the Strength 
            field on the Object Skeleton control panel.

            The BoneStrength function is listed for all bones.
        */
        void BoneStrength()
        {
            currentBone.Strength = file.read_double();
        }

        /*  ScaleBoneStrength <flag>

            example:  ScaleBoneStrength 1

            The ScaleBoneStrength flag  turns the ScaleBoneStrength
            function on. The listing is produced by the Scale Strength
            by Rest Length check box on the Object Skeleton control
            panel. This function allows the user to either lock the
            bone strength to the rest length of the bone, or to adjust
            them separately. This function produces a ScaleBoneStrength
            listing  with a value of 1 when turned on.  

            <flag>:
                0 - Off (Default)
                1 - On (Scale Strength by Rest Length)

            User Interface:
            
            The ScaleBoneStrength function is set from the Scale 
            Strength by Rest Length check box on the Object Skeleton 
            control panel.

            The ScaleBoneStrength flag is listed for all bones.
        */
        void ScaleBoneStrength()
        {
            currentBone.ScaleBoneStrength = file.read_int();
        }

        void BoneFalloffType()
        {
            currentBone.FalloffType = file.read_int();
        }

        void BoneLimitedRange()
        {
            // \todo check
            Trace.TraceWarning("Do not know how to handle BoneLimitedRange");
            currentBone.LimitedRange = file.read_int();
        }
        void BoneMinRange()
        {
            currentBone.Range.Min = file.read_double();
        }
        void BoneMaxRange()
        {
            currentBone.Range.Max = file.read_double();
        }

        /*  IKAnchor <flag>

            example:  IKAnchor 1

            The IKAnchor function sets the current bone as an anchor bone in an inverse 
            kinematics chain.  The predecessors of this bone would not be affected by 
            the goal of its children.

            <flag>:
                0 - Off (No function listing)
                1 - On  (Function listing)

            User Interface:  The IKAnchor function is set from the IK options from Layout.
        */
        void IKAnchor()
        {
            currentBone.IKAnchor = file.read_int();
        }

        /*  GoalObject <object instance>

            example:  GoalObject 5

            The GoalObject function provides LightWave with the current
            bone's goal object for an inverse kinematics chain.  The
            value is equal to the goal object position in the loading
            sequence. The example function would goal the current bone
            to the fifth object instance in the scene file.

            User Interface: 
            
            The GoalObject function is set from the IK options from
            Layout.
        */
        void GoalObject()
        {
            currentBone.GoalObject = file.read_int();
        }

        void HController()
        {
            currentBone.HController = file.read_int();
        }
        void PController()
        {
            currentBone.PController = file.read_int();
        }
        void BController()
        {
            currentBone.BController = file.read_int();
        }

        /*
            HLimits <min. angle> <max. angle>

            example:    HLimits -37.5 180

            The HLimits function provides the minimum and maximum angles of heading 
            rotation for the Inverse Kinematics function.  

            <min. angle>,<max. angle>:  Range = -360 to 360

            User Interface:  
                The HLimits angles are set from the IK Options controls 
                from Layout.
        */
        void HLimits()
        {
            currentBone.HLimits.Min = file.read_double();
            currentBone.HLimits.Max = file.read_double();
        }

        /*  PLimits <min. angle> <max. angle>

            example:    PLimits -37.5 180

            The PLimits function provides the minimum and maximum angles of the pitch 
            rotation for the Inverse Kinematics function.  

            <min. angle>,<max. angle>:  Range = -360 to 360

            User Interface:  The PLimits angles are set from the IK Options controls 
                     from Layout.
        */
        void PLimits()
        {
            currentBone.PLimits.Min = file.read_double();
            currentBone.PLimits.Max = file.read_double();
        }

        /*  BLimits <min. angle> <max. angle>

            example:    BLimits -37.5 180

            The BLimits function provides the minimum and maximum angles of the banking 
            rotation for the Inverse Kinematics function.  

            <min. angle>,<max. angle>:  Range = -360 to 360

            User Interface:  
                The BLimits angles are set from the IK Options controls 
                from Layout.
        */
        void BLimits()
        {
            currentBone.BLimits.Min = file.read_double();
            currentBone.BLimits.Max = file.read_double();
        }

        /*
            BoneMotion (unnamed) 

            example:     BoneMotion (unnamed)

            The BoneMotion identifier denotes the beginning of the keyframe information 
            for the current bone segment. It does not require any arguments to be passed 
            to it.

            The BoneMotion identifier is listed with all bones.
        */
        void BoneMotion()
        {
            currentMotion = new LWMotion();
            currentBone.Motion = currentMotion;
        }

        void BoneNormalization()
        {
            currentBone.Normalization = file.read_int();
        }

        void BoneWeightMapName()
        {
            currentBone.WeightMapName = file.read_string();
        }

        void BoneWeightMapOnly()
        {
            currentBone.WeightMapOnly = file.read_int();
        }

    }
}


/*
  The Object Skeleton Section is a series of one or more bone descriptions 
  that are listed within an object segment.

  An object skeleton does not appear in all object segments.  It is listed 
  when at least one bone has been added to an object.

  Multiple bones in an object segment are listed sequentially in the 
  order in which they were created.

        AddBone *
        BoneName *
        ShowBone *
        BoneActive *
        BoneRestPosition *
        BoneRestDirection *
        BoneRestLength *
        ScaleBoneStrength *
        BoneStrength *
        BoneLimitedRange
            BoneMinRange
            BoneMaxRange
        BoneMotion (identifier) *
        Number of Information Channels *
        Number of Keyframes *
        Keyframe Information *
        EndBehavior *
        LockedChannels
        HLimits
        PLimits
        BLimits
        ParentObject
        GoalObject
        IKAnchor
*/

