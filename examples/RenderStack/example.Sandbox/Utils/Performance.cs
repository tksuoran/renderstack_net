using System.Diagnostics;

namespace example.Sandbox
{
    public class Performance
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        public float Cpu { get { return cpuCounter.NextValue(); } }
        public float Ram { get { return ramCounter.NextValue(); } }

        public Performance()
        {
            Process p = System.Diagnostics.Process.GetCurrentProcess();
            ramCounter = new PerformanceCounter("Process", "Working Set", p.ProcessName);
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName);

            //cpuCounter.CategoryName = "Processor";
            //cpuCounter.CounterName = "% Processor Time";
            //cpuCounter.InstanceName = "_Total";

            //ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
    }

    public partial class Application 
    {
        Performance performance;

        public float CPUUsage
        {
            get
            {
                if(performance != null)
                {
                    return performance.Cpu;
                }
                else
                {
                    return 0f;
                }
            }
        }

        public float MemoryUsage
        {
            get
            {
                if(performance != null)
                {
                    return performance.Ram;
                }
                else
                {
                    return 0f;
                }
            }
        }
    }
}
