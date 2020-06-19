//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using example.Renderer;

namespace example.Sandbox
{
    /*  Comment: Highly experimental  */ 
    public class Sorry : System.Exception
    {
        public Sorry(string msg):base(msg)
        {
        }
    }

    /*  Comment: Highly experimental  */ 
    public partial class Application : OpenTK.GameWindow
    {
        public static void StopMessage(string message, string title)
        {
            Trace.TraceError("StopMessage");
            Trace.TraceError(title);
            Trace.TraceError(message);
            try
            {
                System.Windows.Forms.MessageBox.Show(
                    message,
                    title,
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Stop
                );
            }
            catch(System.Exception)
            {
                System.Console.WriteLine(title);
                System.Console.WriteLine(message);
            }
        }
        public static void ExclamationMessage(string message, string title)
        {
            Trace.TraceError("ExclamationMessage");
            Trace.TraceError(title);
            Trace.TraceError(message);
            try
            {
                System.Windows.Forms.MessageBox.Show(
                    message,
                    title, 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Exclamation
                );
            }
            catch(System.Exception)
            {
                System.Console.WriteLine(title);
                System.Console.WriteLine(message);
            }
        }
        public static void ParseArgs(string[] args)
        {
            IList<string> argsList = (IList<string>)(args);
            if(argsList.Contains("-gl1"))
            {
                Configuration.forceGL1 = true;
            }
            if(argsList.Contains("-nosounds"))
            {
                Configuration.sounds = false;
            }
            if(argsList.Contains("-novsync"))
            {
                Configuration.vsync = false;
            }
            /*if(argsList.Contains("-curveTool"))
            {
                Configuration.curveTool = true;
            }
            if(argsList.Contains("-curveTool"))
            {
                Configuration.curveTool = true;
                Configuration.physics = false;
            }*/
            if(argsList.Contains("-nogui"))
            {
                Configuration.graphicalUserInterface = false;
            }
            if(argsList.Contains("-physics"))
            {
                Configuration.physics = true;
            }
            if(argsList.Contains("-stereo"))
            {
                Configuration.stereo = true;
            }
            if(argsList.Contains("-nophysics"))
            {
                Configuration.physics = false;
            }
            if(argsList.Contains("-binaryShaders"))
            {
                Configuration.useBinaryShaders = true;
            }
            if(argsList.Contains("-nobinaryShaders"))
            {
                Configuration.useBinaryShaders = false;
            }
            if(argsList.Contains("-slow"))
            {
                Configuration.slow = true;
            }

            if(Configuration.useLogListeners)
            {
                //Debug.Listeners.Add();
            }
        }
        public static System.Net.IPAddress GetExternalIp()
        {
            try
            {
                string whatIsMyIp = "http://automation.whatismyip.com/n09230945.asp";
                System.Net.WebClient wc = new System.Net.WebClient();
                System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
                string requestHtml = "";
                requestHtml = utf8.GetString(wc.DownloadData(whatIsMyIp));

                System.Net.IPAddress externalIp = System.Net.IPAddress.Parse(requestHtml);
                return externalIp;
            }
            catch(Exception)
            {
                //System.Net.IPHostEntry IPHost = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName());
                System.Net.IPHostEntry IPHost = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                return IPHost.AddressList[0];
            }
        }
        public static void TraceInfo()
        {
            Trace.TraceInformation("DateTime:       " + DateTime.Now.ToString());
            Trace.TraceInformation("Build:          " + AssemblyUtils.RetrieveLinkerTimestamp());
            Trace.TraceInformation("Region:         " + System.Globalization.RegionInfo.CurrentRegion.ToString());
            // Trace.TraceInformation("ComputerName:   " + System.Windows.Forms.SystemInformation.ComputerName);
            // Trace.TraceInformation("UserName:       " + System.Windows.Forms.SystemInformation.UserName);
            // Trace.TraceInformation("DebugOS:        " + System.Windows.Forms.SystemInformation.DebugOS);
            Trace.TraceInformation("OSVersion:      " + Environment.OSVersion.ToString());
            Trace.TraceInformation("ProcessorCount: " + Environment.ProcessorCount);
            // Trace.TraceInformation("WorkingSet:     " + Environment.WorkingSet);
            if(Configuration.getIp)
            {
                Trace.TraceInformation("IP:             " + GetExternalIp().ToString());
            }
        }
        public static OpenTK.DisplayDevice ChooseDisplay(bool wantPrimary)
        {
            OpenTK.DisplayDevice chosenDisplay = null; //OpenTK.DisplayDevice.Default;
            List<OpenTK.DisplayIndex> indices = new List<OpenTK.DisplayIndex>();
            indices.Add(OpenTK.DisplayIndex.Default);
            indices.Add(OpenTK.DisplayIndex.First);
            indices.Add(OpenTK.DisplayIndex.Second);
            indices.Add(OpenTK.DisplayIndex.Third);
            indices.Add(OpenTK.DisplayIndex.Fourth);
            indices.Add(OpenTK.DisplayIndex.Fifth);
            indices.Add(OpenTK.DisplayIndex.Sixth);
            foreach(var index in indices)
            {
                var display = OpenTK.DisplayDevice.GetDisplay(index);
                if(display == null)
                {
                    continue;
                }
                if((chosenDisplay == null) && (display.IsPrimary == wantPrimary))
                {
                    chosenDisplay = display;
                }
                if(Configuration.trace)
                {
                    Trace.TraceInformation(
                        " " 
                        + index.ToString() 
                        + " " 
                        + display.Width 
                        + " x " 
                        + display.Height
                        + ((display == chosenDisplay) ? " (using this one)" : "")
                    );
                }
            }
            return chosenDisplay;
        }
    }
}
