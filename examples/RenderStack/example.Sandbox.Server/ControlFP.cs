using System;
using RenderStack.Math;

public class Utils
{
    //  http://stereopsis.com/FPU.html
    //  The .NET framework expects the floating point control word to mask all exceptions internally.
    [System.Runtime.InteropServices.DllImport("msvcrt.dll")]
    public static extern int _controlfp(int IN_New, int IN_Mask);

    private static int DefaultCW;

    public static void ControlFP()
    {
        DefaultCW = _controlfp(0, 0);
        _controlfp(DefaultCW, 0xfffff);
    }

}
