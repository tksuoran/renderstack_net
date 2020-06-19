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


namespace example.Loading
{
    public interface ILoadingWindow : IDisposable
    {
        object SyncFormVisible { get; }

        void Span(float expectedTime, float start, float end);
        void Run();
        void Close();
        void Message(string message);

    }
}
