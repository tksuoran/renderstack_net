
// #define DEBUG_LOADING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace example.Loading
{
    public class LoadingScreenManager
    {
        private float                   startTime;
        private float                   lastCompletedSpan;
        private readonly Queue<float>   spansReference = new Queue<float>();
        private readonly Queue<float>   spansCompleted = new Queue<float>();
        private float                   totalReference;
        private float                   soFarReference;
        private float                   soFarCompleted;
        private float                   expectedSpanTime;
        private float                   currentSpanStartRelative;
        private float                   currentSpanEndRelative;

        private ILoadingWindow          window;
        const string                    path = "data/loadInfo.z";
        private void UsePresetSpans()
        {
            spansReference.Clear();
            spansReference.Enqueue(0.06199999f);
            spansReference.Enqueue(0.016f);
            spansReference.Enqueue(0);
            spansReference.Enqueue(0);
            spansReference.Enqueue(0.296f);
            spansReference.Enqueue(0.14f);
            spansReference.Enqueue(0.141f);
            spansReference.Enqueue(0.14f);
            spansReference.Enqueue(0.234f);
            spansReference.Enqueue(0.1410001f);
            spansReference.Enqueue(0.124f);
            spansReference.Enqueue(52.9f);
            spansReference.Enqueue(0.1250009f);
            spansReference.Enqueue(1.622998f);
            spansReference.Enqueue(0.2020044f);
            spansReference.Enqueue(1.684998f);
            spansReference.Enqueue(0.1250009f);
            spansReference.Enqueue(0.07799999f);
            foreach(float span in spansReference)
            {
                totalReference += span;
            }
        }
        private void Deserialize()
        {
            bool loadInfoExists = File.Exists(path);
            if(loadInfoExists == false)
            {
                Trace.TraceWarning("no load info found");
                UsePresetSpans();
                return;
            }
            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(
                    file, 
                    System.IO.Compression.CompressionMode.Decompress
                );
                BufferedStream stream = new BufferedStream(zip);
                BinaryReader reader = new BinaryReader(stream);

                int count = reader.ReadInt32();
#if DEBUG_LOADING
                Debug.WriteLine("read span count = " + count);
#endif
                spansReference.Clear();
                for(int i = 0; i < count; ++i)
                {
                    float duration = reader.ReadSingle();
                    spansReference.Enqueue(duration);
#if DEBUG_LOADING
                    Debug.WriteLine("read span duration = " + duration);
#endif
                    totalReference += duration;
                }
#if DEBUG_LOADING
                Debug.WriteLine("totalReference = " + totalReference);
#endif
                stream.Close();
            }
            catch(Exception)
            {
                UsePresetSpans();
            }
        }
        public void Serialize()
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Create);
                System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(
                    file, 
                    System.IO.Compression.CompressionMode.Compress
                );
                BufferedStream stream = new BufferedStream(zip);
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(spansCompleted.Count);
#if DEBUG_LOADING
                Debug.WriteLine("wrote span count = " + spansCompleted.Count);
#endif
                foreach(float duration in spansCompleted)
                {
                    writer.Write(duration);
#if DEBUG_LOADING
                    Debug.WriteLine("wrote span = " + duration);
#endif
                }
                stream.Close();
            }
            catch(Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        public void Step()
        {
            /*if(Configuration.loadingWindow == false)
            {
                return;
            }*/
            if(totalReference == 0.0f)
            {
                return;
            }
            lock(this)
            {
#if DEBUG_LOADING
                Debug.WriteLine("--------------------------------------------------------------------");
#endif
                float now = Time.Now;

                //  Duration of completed span
                lastCompletedSpan = now - soFarCompleted - startTime;
#if DEBUG_LOADING
                if(lastCompletedSpan < 0.0f)
                {
                    Debug.WriteLine("lastCompletedSpan = " + lastCompletedSpan.ToString());
                }
                Debug.WriteLine("lastCompletedSpan = " + lastCompletedSpan.ToString());
#endif

                //  Store completed span
                spansCompleted.Enqueue(lastCompletedSpan);
                soFarCompleted += lastCompletedSpan;
#if DEBUG_LOADING
                Debug.WriteLine("soFarCompleted = " + soFarCompleted.ToString());
#endif

                //  End of completed span is now start of new span
                currentSpanStartRelative = currentSpanEndRelative;

                if(spansReference.Count == 0)
                {
#if DEBUG_LOADING
                    Debug.WriteLine("No reference");
#endif
                    return;
                }

                //  Next span reference and relative
                float nextSpanReference = spansReference.Dequeue();
                currentSpanEndRelative = (soFarReference + nextSpanReference) / totalReference;

#if DEBUG_LOADING
                Debug.WriteLine("nextSpanReference = " + nextSpanReference);
                Debug.WriteLine("currentSpanStartRelative = " + currentSpanStartRelative);
                Debug.WriteLine("currentSpanEndRelative = " + currentSpanEndRelative);
#endif

                if(soFarReference > 0.0f)
                {
                    //  Estimate velocity
                    float velocity = soFarCompleted / soFarReference;
#if DEBUG_LOADING
                    Debug.WriteLine("velocity = " + velocity);
#endif

                    //  How long this span would take with estimated velocity
                    float expectedSpanTime = velocity * nextSpanReference;
#if DEBUG_LOADING
                    Debug.WriteLine("expectedSpanTime = " + expectedSpanTime);
#endif

                    if(window != null)
                    {
                        window.Span(expectedSpanTime, currentSpanStartRelative, currentSpanEndRelative);
                    }
                }

                //  This will be valid for next Step() execution
                soFarReference += nextSpanReference;
            }
        }
        private object syncFormCreated = new object();
        private System.Threading.Thread loadingThread;
        private void LoadingScreen()
        {
            //ILoadingWindow window =  new LoadingForm();
            using(ILoadingWindow window =  new LoadingWindow())
            {
                this.window = window;
                lock(syncFormCreated)
                {
                    System.Threading.Monitor.Pulse(syncFormCreated);
                }
                window.Run();
            }
        }
        public void Message(string message)
        {
#if false
            if(window != null)
            {
                window.Message(message);
            }
#endif
        }
        protected void SpawnLoadingScreen()
        {
            loadingThread = new System.Threading.Thread(LoadingScreen);
            //loadingThread.ApartmentState = System.Threading.ApartmentState.MTA;
            loadingThread.Start();
            lock(syncFormCreated)
            {
                System.Threading.Monitor.Wait(syncFormCreated);
            }
            lock(window.SyncFormVisible)
            {
                System.Threading.Monitor.Wait(window.SyncFormVisible);
            }
#if DEBUG_LOADING
            Debug.WriteLine("Done");
#endif
        }
        public void Prepare()
        {
            /*if(Configuration.loadingWindow == false)
            {
                return;
            }*/
            SpawnLoadingScreen();
            Deserialize();
            startTime = Time.Now;
            lastCompletedSpan = 0.0f;
            soFarCompleted = 0.0f;
            soFarReference = (spansReference.Count > 0) ? spansReference.Dequeue() : totalReference;
            currentSpanStartRelative = 0.0f;
            currentSpanEndRelative = (totalReference > 0.0f) ? (soFarReference / totalReference) : 1.0f;

            expectedSpanTime = soFarReference;

#if DEBUG_LOADING
            Debug.WriteLine("nextSpanReference = " + soFarReference);
            Debug.WriteLine("currentSpanStartRelative = " + currentSpanStartRelative);
            Debug.WriteLine("currentSpanEndRelative = " + currentSpanEndRelative);
            Debug.WriteLine("expectedSpanTime = " + expectedSpanTime);
            Debug.WriteLine("--------------------------------------------------------------------");
#endif

            //  TODO wait for Form OnShown?
            if(window != null)
            {
                window.Span(expectedSpanTime, currentSpanStartRelative, currentSpanEndRelative);
            }
        }
        public void Finish(bool exceptions)
        {
            /*if(Configuration.loadingWindow == false)
            {
                return;
            }*/
            if(exceptions == false)
            {
                Step();
                Serialize();
            }
            try
            {
                window.Close();
            }
            catch(System.Exception)
            {
            }
            if(exceptions == false)
            {
                loadingThread.Join();
            }
        }
   }
}