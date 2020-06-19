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
