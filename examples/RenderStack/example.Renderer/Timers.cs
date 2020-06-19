//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System.Diagnostics;

using RenderStack.Services;

using example.Renderer;

namespace example.Renderer
{
    /// \brief A few low level rendering related CPU timers.
    /// \note Can not do GPU timers as ARB timer queries can not nest and I want to allow higher level GPU timers.
    public class Timers
    {
        public readonly AMDHardwareMonitor  AMDHardwareMonitor  = new AMDHardwareMonitor();
        public readonly Timer               AttributeSetup      = new Timer("AttributeSetup",   0.5, 0.5, 1.0, false);
        public readonly Timer               UniformsSetup       = new Timer("UniformSetup",     0.5, 1.0, 1.0, false);
        public readonly Timer               MaterialSwitch      = new Timer("MaterialSwitch",   0.0, 0.0, 1.0, false);
        public readonly Timer               ProgramSwitch       = new Timer("ProgramSwitch",    0.5, 1.0, 0.5, false);
        public readonly Timer               DrawCalls           = new Timer("DrawCalls",        0.0, 0.5, 0.0, false);
    }
}
