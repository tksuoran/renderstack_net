namespace example.Renderer
{
    /// \brief Some configuration options for Renderer
    /// \note lightCount will be gone eventually
    public class Configuration
    {
        public static bool  AMDGPUPerf          = false;
        //public static int   maxLightCount       = 3;
        public static int   maxLightCount       = 14;
        public static bool  hardwareShadowPCF   = true;
        public static int   instanceCount       = 200;
    }
}