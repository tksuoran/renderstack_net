using System;
using System.Collections.Generic;

using OpenTK.Graphics;

using OpenHardwareMonitor.Hardware.ATI;

namespace example.Renderer
{
    public class AMDGPUCounter
    {
        public uint                         Index;
        public GPUPerfAPI.GPA_Type          Type;
        public GPUPerfAPI.GPA_Usage_Type    Usage;
        public string                       Name;
        public string                       Description;
        public double                       DoubleValue;

        public AMDGPUCounter(
            uint                        index, 
            GPUPerfAPI.GPA_Type         type,
            GPUPerfAPI.GPA_Usage_Type   usage,
            string                      name,
            string                      description
        )
        {
            Index = index;
            Type = type;
            Usage = usage;
            Name = name;
            Description = description;
        }
        public override string ToString()
        {
            return Index.ToString() + ": " + Type + " " + Name + " - " + Usage + " : " + Description;
        }
    }

    public class AMDHardwareMonitor : IDisposable
    {
        public string   AdapterName { get; private set; }
        public string   UDID        { get; private set; }
        public int      Present     { get; private set; }
        public int      VendorID    { get; private set; }

        public float[]  Temperature { get; private set; }

        public AMDHardwareMonitor()
        {
            try
            {
                int status = OpenHardwareMonitor.Hardware.ATI.ADL.ADL_Main_Control_Create(1);
                if(status == ADL.ADL_OK)
                {
                    int numberOfAdapters = 0;
                    ADL.ADL_Adapter_NumberOfAdapters_Get(ref numberOfAdapters);

                    if(numberOfAdapters > 0)
                    {
                        Temperature = new float[numberOfAdapters];
                        ADLAdapterInfo[] adapterInfo = new ADLAdapterInfo[numberOfAdapters];
                        if(ADL.ADL_Adapter_AdapterInfo_Get(adapterInfo) == ADL.ADL_OK)
                        {
                            for(int i = 0; i < numberOfAdapters; i++)
                            {
                                int isActive;
                                int adapterID;

                                ADL.ADL_Adapter_Active_Get(adapterInfo[i].AdapterIndex, out isActive);
                                ADL.ADL_Adapter_ID_Get(adapterInfo[i].AdapterIndex, out adapterID);

                                AdapterName = adapterInfo[i].AdapterName;
                                UDID = adapterInfo[i].UDID;
                                Present = adapterInfo[i].Present;
                                VendorID = adapterInfo[i].VendorID;
                                var busNumber = adapterInfo[i].BusNumber;
                                var deviceNumber = adapterInfo[i].DeviceNumber;
                                var functionNumber = adapterInfo[i].FunctionNumber;
                                var adapterId = adapterID.ToString("X");
                            }
                        }
                    }
                }
            }
            catch(System.Exception)
            {
                Temperature = null;
            }
        }
        ~AMDHardwareMonitor()
        {
            Dispose();
        }
        private bool disposed;
        public void Dispose()
        {
            if(!disposed)
            {
                try
                {
                    ADL.ADL_Main_Control_Destroy();
                }
                catch(Exception)
                {
                }
                disposed = true;
            }
        }
        public void Update()
        {
            if(Temperature == null)
            {
                return;
            }
            ADLTemperature adlt = new ADLTemperature();
            for(int i = 0; i < Temperature.Length; ++i)
            {
                var res = ADL.ADL_Overdrive5_Temperature_Get(i, 0, ref adlt);
                if(res == ADL.ADL_OK) 
                {
                    Temperature[i] = 0.001f * adlt.Temperature;
                }
            }
        }
    }

    public class AMDGPUPerf : IDisposable
    {
        private static AMDGPUPerf instance;
        public static AMDGPUPerf Instance { get { return instance; } }
        private GPUPerfAPI.GPA_LoggingCallbackDelegate log;

        private AMDGPUCounter[]                    counters;
        private Dictionary<string,AMDGPUCounter>   dictionary = new Dictionary<string,AMDGPUCounter>();
        private uint sessionId;

        public float this[string name]
        {
            get
            {
                if(dictionary.ContainsKey(name) == false)
                {
                    return 0.0f;
                }
                return (float)(dictionary[name].DoubleValue);
            }
        }

        public static void Log(GPUPerfAPI.GPA_Logging_Type messageType, string message)
        {
            Console.WriteLine(messageType.ToString() + " : " + message);
        }

        public AMDGPUPerf()
        {
            instance = this;
            if(Configuration.AMDGPUPerf)
            {
                try
                {
                    var res = GPUPerfAPI.GPA.GPA_Initialize();
                    switch(res)
                    {
                        case GPUPerfAPI.GPA_Status.GPA_STATUS_OK:
                        {
                            Console.WriteLine("GPA_Initialize() returned OK");
                            break;
                        }
                        default:
                        {
                            Console.WriteLine("GPA_Initialize() returned " +  res.ToString());
                            break;
                        }
                    }
                    log = Log;
                    res = GPUPerfAPI.GPA.GPA_RegisterLoggingCallback(GPUPerfAPI.GPA_Logging_Type.GPA_LOGGING_ERROR_AND_MESSAGE, log);
                    switch(res)
                    {
                        case GPUPerfAPI.GPA_Status.GPA_STATUS_OK:
                        {
                            Console.WriteLine("GPA_RegisterLoggingCallback() returned OK");
                            break;
                        }
                        default:
                        {
                            Console.WriteLine("GPA_RegisterLoggingCallback() returned " +  res.ToString());
                            break;
                        }
                    }
                }
                catch(Exception)
                {
                    //  Could not initialize AMDGPUPerf - disable
                    Configuration.AMDGPUPerf = false;
                }
            }
        }
        private bool disposed = false;
        ~AMDGPUPerf()
        {
            Dispose();
        }
        public void Dispose()
        {
            if(!disposed)
            {
                if(Configuration.AMDGPUPerf)
                {
                    GPUPerfAPI.GPA.GPA_Destroy();
                }
                disposed = true;
            }
        }

        public void OpenContext()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            IntPtr context_handle = (GraphicsContext.CurrentContext as IGraphicsContextInternal).Context.Handle;
            var status = GPUPerfAPI.GPA.GPA_OpenContext(context_handle);
            if(status != GPUPerfAPI.GPA_Status.GPA_STATUS_OK)
            {
                Console.WriteLine("GPA_OpenContext() returned " + status.ToString());
            }
            SetupCounters();
        }
        uint currentPass = uint.MaxValue;
        public void BeginFrame()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            if(currentPass >= requiredPassCount)
            {
                currentPass = 0;
                BeginSession();
            }
            BeginPass();
            BeginSample();
        }
        public void EndFrame()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            EndSample();
            EndPass();
            ++currentPass;
            if(currentPass >= requiredPassCount)
            {
                EndSession();
                GetResults();
            }
        }
        public uint BeginSession()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return 0;
            }

            GPUPerfAPI.GPA.GPA_BeginSession(out sessionId);
            return sessionId;
        }
        public void BeginPass()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            GPUPerfAPI.GPA.GPA_BeginPass();
            currentSample = 0;
        }
        uint currentSample;
        public void BeginSample()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }
            GPUPerfAPI.GPA.GPA_BeginSample(currentSample);
        }
        public void EndSample()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            GPUPerfAPI.GPA.GPA_EndSample();
            ++currentSample;
        }
        public void EndPass()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            GPUPerfAPI.GPA.GPA_EndPass();
        }
        public void EndSession()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            GPUPerfAPI.GPA.GPA_EndSession();
        }
        uint currentWaitSession = 1;
        public void GetResults()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            bool readyResult = false;
            if(sessionId != currentWaitSession)
            {
                var sessionStatus = GPUPerfAPI.GPA.GPA_IsSessionReady(out readyResult, currentWaitSession);
                while(sessionStatus == GPUPerfAPI.GPA_Status.GPA_STATUS_ERROR_SESSION_NOT_FOUND)
                {
                    // skipping a session which got overwritten
                    ++currentWaitSession;
                    sessionStatus = GPUPerfAPI.GPA.GPA_IsSessionReady(out readyResult, currentWaitSession);
                }
            }
            if(readyResult)
            {
                GetSessionResults(currentWaitSession);
                ++currentWaitSession;
            }
        }
        public void GetSessionResults(uint sessionId)
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            uint sampleCount = 0;
            GPUPerfAPI.GPA.GPA_GetSampleCount(sessionId, out sampleCount);
            for(uint sample = 0; sample < sampleCount; ++sample)
            {
                uint counterCount;
                GPUPerfAPI.GPA.GPA_GetEnabledCount(out counterCount);
                for(uint j = 0; j < counterCount; ++j)
                {
                    uint index;
                    GPUPerfAPI.GPA.GPA_GetEnabledIndex(j, out index);
                    AMDGPUCounter counter = counters[index];
                    GPUPerfAPI.GPA.GPA_GetSampleFloat64(sessionId, sample, index, out counter.DoubleValue);
                }
            }
        }
        uint requiredPassCount;
        public void SetupCounters()
        {
            if(Configuration.AMDGPUPerf == false)
            {
                return;
            }

            uint count;
            var status = GPUPerfAPI.GPA.GPA_GetNumCounters(out count);
            Console.WriteLine("Got " + count + " performance counters");

            counters = new AMDGPUCounter[count];
            for(uint i = 0; i < count; ++i)
            {
                GPUPerfAPI.GPA_Type type;
                GPUPerfAPI.GPA_Usage_Type usage;
                string description;
                string name;
                GPUPerfAPI.GPA.GPA_GetCounterDataType(i, out type);
                GPUPerfAPI.GPA.GPA_GetCounterDescription(i, out description);
                GPUPerfAPI.GPA.GPA_GetCounterName(i, out name);
                GPUPerfAPI.GPA.GPA_GetCounterUsageType(i, out usage);
                counters[i] = new AMDGPUCounter(i, type, usage, name, description);
                dictionary[name] = counters[i];
                Console.WriteLine(counters[i].ToString());
            }
            Console.WriteLine("----");

            //  All are GPA_TYPE_FLOAT64

            //  GPA_USAGE_TYPE_MILLISECONDS
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["GPUTime"].Index);
            //  GPA_USAGE_TYPE_PERCENTAGE
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["GPUBusy"].Index);
            //GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["TessellatorBusy"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ShaderBusy"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ShaderBusyVS"].Index);
            //GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ShaderBusyHS"].Index);
            //GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ShaderBusyDS"].Index);
            //GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ShaderBusyGS"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ShaderBusyPS"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["PSTexBusy"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["PSExportStalls"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["PSTexInstCount"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["PSPixelsIn"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["PSPixelsOut"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["HiZQuadsCulled"].Index);
            GPUPerfAPI.GPA.GPA_EnableCounter(dictionary["ZUnitStalled"].Index);
            

            GPUPerfAPI.GPA.GPA_GetPassCount(out requiredPassCount);
            Console.WriteLine("NumPasses = " + requiredPassCount);
        }
    }
}

