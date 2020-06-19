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
        void ViewConfiguration()
        {
            int view_configuration = file.read_int();
        }

        void DefineView()
        {
            int define_view = file.read_int();
        }

        /*  ViewMode <value>

            example:  ViewMode 5

            The ViewMode function determines the default viewing mode from Layout when 
            the scene file is loaded.

            <value>:
                0 - Front
                1 - Top
                2 - Side
                3 - Perspective
                4 - Light
                5 - Camera
        */
        void ViewMode()
        {
            int view_mode = file.read_int();
        }

        /*  ViewAimpoint <x position> <y position> <z position>

            example:  ViewAimpoint 0.000000 0.000000 0.000000

            The ViewAimpoint function provides the position information for the 
            default viewing mode from Layout when the scene file is loaded.
        */
        void ViewAimpoint()
        {
            double x = file.read_double();
            double y = file.read_double();
            double z = file.read_double();
            scene.ViewAimpoint = new Vector3(x, y, z);
        }

        /*  ViewDirection <heading angle> <pitch angle> <bank angle>

            example:  ViewDirection 0.000000 -0.175433 0.000000

            The ViewDirection provides the rotation information for the default 
            viewing mode from Layout when the scene file is loaded.
        */
        double rads(double degrees)
        {
            return System.Math.PI * degrees / 180.0;
        }
        void ViewRotation()
        {
            double h = file.read_double();
            double p = file.read_double();
            double b = file.read_double();
            //    view_rotation = Quaternion( rads(h), rads(p), rads(b) );
            Quaternion q;
            q  = new Quaternion( Vector3.UnitY, (float)(System.Math.PI + rads(h)) );
            q *= new Quaternion( Vector3.UnitX, (float)(- rads(p)) );
            q *= new Quaternion( Vector3.UnitZ, (float)(- rads(b)) );
            scene.ViewRotation = q;
        }

        /*
            ViewZoomFactor <float>

            example:  ViewZoomFactor 3.200000

            The ViewZoomFactor function provides the zoom factor for the default 
            viewing mode from Layout when the scene file is loaded.
        */
        void ViewZoomFactor()
        {
            scene.ViewZoomFactor = file.read_double();
        }

        void ViewType()
        {
            int type = file.read_int();
        }

        /*  LockedChannels <bit-field value>

            example:     LockedChannels 4093

            The LockedChannels function determines the extent of the mouse control 
            from LightWave's Layout.  Separate independent channels of motion, 
            rotation, etc. can be locked off to restrict the mouse's  control on 
            the current object.  The mouse functions that it can effect are:  
            Move (X,Y,Z), Rotate(H,P,B), Scale/Stretch(X,Y,Z), and MovePivotPoint(X,Y,Z).

            The bit-field value is produced by calculating the decimal value of a 
            12 position bit-field whose bits represent logical on/off switches that 
            are number left to right from 0 - 11.  The least-significant bit for 
            this field is the rightmost bit.  Each channel has a corresponding bit 
            in the bit-field.  When a channel is locked, its bit (or switch) is 
            turned on.

            <bit positions>:
                 0 - Move X
                 1 - Move Y
                 2 - Move Z
                 3 - Rotate Heading
                 4 - Rotate Pitch
                 5 - Rotate Bank
                 6 - Scale X / Size X  (channels are connected)
                 7 - Scale Y / Size Y (channels are connected)
                 8 - Scale Z / Size Z (channels are connected)
                 9 - MovePivotPoint X
                10 - Move Pivot Point Y
                11 - Move Pivot Point Z

            User Interface:

            The LockedChannels function is set from the Layout mouse 
            control area.
        */
        void LockedChannels()
        {
            int locked_channels = file.read_int();
        }

        /*  LayoutGrid <value>

            example:  LayoutGrid 8

            The LayoutGrid function determines the number of grid squares in Layout.

            <value>:
                0 - Off
                1 - 2 x 2
                2 - 4 x 4
                3 - 6 x 6
                4 - 8 x 8
                5 - 10 x 10
                6 - 12 x 12
                7 - 14 x 14
                8 - 16 x 16
        */
        void LayoutGrid()
        {
            int layout_grid = file.read_int();
            scene.GridNumber = layout_grid * 2;
        }

        void GridNumber()
        {
            scene.GridNumber = file.read_int();
        }

        /*  GridSize <float>

            example:  GridSize 1.000000

            The GridSize function  provides the value, in meters, for the grid square 
            size in Layout.
        */
        void GridSize()
        {
            scene.GridSize = file.read_double();
        }

        void CameraViewBG()
        {
            int camera_view_bg = file.read_int();
        }

        /*  ShowMotionPath <flag>

            example:  ShowMotionPath 1

            The ShowMotionPath flag controls the display of the motion paths in Layout.

            <flag>:
                0 - Off
                1 - On
        */
        void ShowMotionPath()
        {
            int show_motion_path = file.read_int();
        }

        /*  ShowSafeAreas <flag>

            example:  ShowSafeAreas 1

            The ShowSafeAreas flag controls the display of the safe areas overlay in Layout.

            <flag>:
                0 - Off
                1 - On (Display Safe Area Chart)
        */
        void ShowSafeAreas()
        {
            int show_safe_areas = file.read_int();
        }

        /*  ShowBGImage <value>

            example:  ShowBGImage 2

            The ShowBGImage function activates the display of a background image or 
            preview anim.

            <value>:
                0 - Blank
                1 - BG Image
                2 - Preview
        */
        void ShowBGImage()
        {
            int show_bg_image = file.read_int();
        }

        /*  ShowFogRadius <flag>

            example:  ShowFogRadius 1

            The ShowFogRadius flag activates the display of the fog radius in Layout.

            <flag>:
                0 - Off
                1 - On (Display Fog Radius)
        */
        void ShowFogRadius()
        {
            int show_fog_radius = file.read_int();
        }

        void ShowFogEffect()
        {
            int show_fog_effect = file.read_int();
        }

        /*  ShowRedraw <flag>

            example:  ShowRedraw 0

            The ShowRedraw flag activates the display of the object polygon redraw 
            in Layout.

            <flag>:
                0 - Off
                1 - On (Display Object Polygon Redraw)
        */
        void ShowRedraw()
        {
            int show_redraw = file.read_int();
        }

        /*  ShowFieldChart <flag>

            example:  ShowFieldChart 1

            The ShowFieldChart flag activates the display of the Camera Field Chart 
            in Layout.

            <flag>:
                0 - Off
                1 - On (Display Field Chart)
        */
        void ShowFieldChart()
        {
            int show_field_chart = file.read_int();
        }

        void OverlayColor()
        {
            int color = file.read_int();
        }

        void CurrentObject()
        {
            int current_object = file.read_int();
        }

        void CurrentLight()
        {
            int current_light = file.read_int();
        }

        void CurrentCamera()
        {
            int current_camera = file.read_int();
        }

        void GraphEditorData()
        {
        }

        void GraphEditorFavorites()
        {
            file.read_begin_scope();
            file.read_end_scope();
        }
    }
}

/*

  The Options Section contains the information that relates to environment 
  settings for LightWave's Layout.


        ViewMode
        ViewAimpoint
        ViewDirection
        ViewZoomFactor
        LayoutGrid
        GridSize
        ShowMotionPath
        ShowSafeAreas
        ShowBGImage
        ShowFogRadius
        ShowRedraw
        ShowFieldChart
*/

