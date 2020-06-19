//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;

namespace example.Sandbox
{
    /*  Comment: Highly experimental  */ 
    public class RuntimeConfiguration
    {
        public static bool gameTest                     = false;

        /*  Curve tool  */ 
        public static bool curveTool                    = false;
        public static bool curveToolLines               = false;

        /*  GUI  */ 
        public static bool debugInfo                    = true;
        public static bool manipulator                  = true;
        public static bool guiExtraInfo                 = false;
        public static bool selectionSilhouette          = true;
        public static bool selectionWireframeHidden     = false;
        public static bool selectionWireframePoints     = true;
        public static bool selectionWireframeEdges      = true;
        public static bool selectionWireframeCentroids  = true;
        public static bool selectionBoundingVolume      = true;
        public static bool doubleSided                  = false;
        public static bool allWireframe                 = false;
        public static bool hoverModelName               = true;
        public static bool disableReadPixels            = false;
    }

    [Serializable]
    public class Configuration
    {
        //public static bool vsync                        = true;
        public static bool vsync                        = false;
        public static bool trace                        = true;
        public static bool getIp                        = false;

        public static bool forceGL1                     = false;
        //public static bool forceGL1                     = true;
        public static bool lockUpVector                 = true;
        public static bool loadingWindow                = true;

        public static bool brushes                      = true;
        public static bool hoverDebug                   = false;
        public static bool selection                    = true;
        public static bool keyUserInterface             = true;
        //public static bool graphicalUserInterface       = false;
        public static bool graphicalUserInterface       = true;
        public static bool physics                      = true;
        //public static bool idBuffer                     = false;
        public static bool idBuffer                     = true;
        public static bool stereo                       = true;
        public static bool physicsCamera                = false;

        /*  Experimental  */

        public static bool voxelTest                    = false;

        public static bool threadedRendering            = false;
        public static bool sounds                       = false;
        public static bool gammaCorrect                 = false;    // not yet implemented
        public static bool performanceMonitoring        = false;
        public static bool useLogListeners              = false;

        public static bool minimal                      = false;

        public static bool useBinaryShaders             = false;    // \note if enabled asset monitoring does not work
        public static bool slow                         = false;    // assume slower computer
        public static int  shadowResolution             = 2048;     // slow ? 1024 : 2048;
    }
}