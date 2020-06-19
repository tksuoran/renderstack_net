//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Linq;

using RenderStack.Geometry;
using RenderStack.Mesh;
using RenderStack.Services;

using example.Renderer;

namespace example.Brushes
{

    /*  Comment: Highly experimental  */ 
    public partial class BrushManager : Service
    {
        public override string Name
        {
            get { return "BrushManager"; }
        }

        MaterialManager materialManager;
        TextRenderer    textRenderer;

        #region Data members
        private InertiaCache                                inertiaCache;
        public  InertiaCache                                InertiaCache { get { return inertiaCache; } }

        private List<Brush>                                 brushes = new List<Brush>();
        private Dictionary<string, Brush>                   namedBrushes = new Dictionary<string,Brush>();

        [NonSerialized]
        private ReadOnlyCollection<Brush>                   readOnlyBrushes;
        private Dictionary<string, Dictionary<int, Brush>>  dictionaries = new Dictionary<string,Dictionary<int,Brush>>();
        private Dictionary<string, List<Brush>>             lists = new Dictionary<string,List<Brush>>();

        [NonSerialized]
        public Brush                                        CurrentBrush;
        public ReadOnlyCollection<Brush>                    Brushes         { get { return readOnlyBrushes; } }
        public Dictionary<string, Dictionary<int, Brush>>   Dictionaries    { get { return dictionaries; } }
        public Dictionary<string, List<Brush>>              Lists           { get { return lists; } }
        public Dictionary<int, Brush>  Dictionary(string category)
        {
            if(category == null)
            {
                return null;
            }
            if(dictionaries.ContainsKey(category) == false)
            {
                return null;
            }
            return dictionaries[category];
        }

        Dictionary<int, Brush> all = new Dictionary<int,Brush>();
        #endregion

        public MaterialManager MaterialManager { get { return materialManager; } }

        public void Connect(
            MaterialManager materialManager,
            TextRenderer    textRenderer
        )
        {
            this.materialManager = materialManager;
            this.textRenderer = textRenderer;
            InitializationDependsOn(textRenderer, materialManager);
            initializeInMainThread = true;
        }

        private void Reset()
        {
            {
                CurrentBrush = null;
                brushes.Clear();
                namedBrushes.Clear();
                dictionaries.Clear();
                lists.Clear();
                if(all != null)
                {
                    all.Clear();
                }
            }
            {
                Dictionary<int, Brush> pyramids     = new Dictionary<int,Brush>();
                Dictionary<int, Brush> prisms       = new Dictionary<int,Brush>();
                Dictionary<int, Brush> antiprisms   = new Dictionary<int,Brush>();
                Dictionary<int, Brush> cupolas      = new Dictionary<int,Brush>();
                Dictionary<int, Brush> rotund       = new Dictionary<int,Brush>();
                Dictionary<int, Brush> diminished   = new Dictionary<int,Brush>();
                Dictionary<int, Brush> stella       = new Dictionary<int,Brush>();
                //Dictionary<int, Brush> all          = new Dictionary<int,Brush>();

                dictionaries["pyramid"]     = pyramids;
                dictionaries["prism"]       = prisms;
                dictionaries["antiprism"]   = antiprisms;
                dictionaries["cupola"]      = cupolas;
                dictionaries["rotund"]      = rotund;
                dictionaries["diminished"]  = diminished;
                dictionaries["stella"]      = stella;
                dictionaries["all"]         = all;
            }
        }

        protected override void InitializeService()
        {
            materialManager.MakeMaterial("BrushDefault", "ColorFill").MeshMode = MeshMode.EdgeLines;

            Message("BrushManager: DeserializeInertiaCache...");
            DeserializeInertiaCache();

            Message("BrushManager: Collections...");
            readOnlyBrushes = new ReadOnlyCollection<Brush>(brushes);

            Reset();

            try
            {
                //Debug.WriteLine("Try " + i);
                //Reset();
                //TryMakeBrushesFromGeometryCache();
                MakeBrushesFromSources();
                Debug.WriteLine("brushes.Count()      = " + brushes.Count());
                Debug.WriteLine("namedBrushes.Count() = " + namedBrushes.Count());
                if(namedBrushes.ContainsKey("tetrahedron") == false) Debug.WriteLine("tetrahedron missing!");
                if(namedBrushes.ContainsKey("cube") == false) Debug.WriteLine("cube missing!");
                if(namedBrushes.ContainsKey("octahedron") == false) Debug.WriteLine("octahedron missing!");
                if(namedBrushes.ContainsKey("dodecahedron") == false) Debug.WriteLine("dodecahedron missing!");
                if(namedBrushes.ContainsKey("icosahedron") == false) Debug.WriteLine("icosahedron missing!");
            }
            catch(Exception e)
            {
                //Trace.TraceError("exception in try " + i + ":  " + e.ToString());
                Trace.TraceError(e.ToString());
            }

#if false
            try
            {
                Message("BrushManager: TryMakeBrushesFromGeometryCache...");
                bool done = TryMakeBrushesFromGeometryCache();
                if(!done)
                {
                    Message("BrushManager: MakeBrushesFromSources...");
                    MakeBrushesFromSources();

#if false
                    //  TODO this won't work as long as Buffer needs GL context to read back buffer contents
                    //  Fire and forget
                    System.Threading.Thread thread = new System.Threading.Thread(UpdateGeometryCache);
                    thread.Start();
#else
                    Message("BrushManager: UpdateGeometryCache...");
                    UpdateGeometryCache();
#endif
                }
                if(namedBrushes.ContainsKey("tetrahedron") == false) Debug.WriteLine("tetrahedron missing!");
                if(namedBrushes.ContainsKey("cube") == false) Debug.WriteLine("cube missing!");
                if(namedBrushes.ContainsKey("octahedron") == false) Debug.WriteLine("octahedron missing!");
                if(namedBrushes.ContainsKey("dodecahedron") == false) Debug.WriteLine("dodecahedron missing!");
                if(namedBrushes.ContainsKey("icosahedron") == false) Debug.WriteLine("icosahedron missing!");
            }
            catch(Exception)
            {
                Trace.TraceError("issue making brushes");
            }
#endif

            Message("BrushManager: SerializeInertiaCache...");
            SerializeInertiaCache();

            Message("BrushManager: Preset collections...");
            try
            {
                List<Brush> list = new List<Brush>();
                lists["platonic"] = list;
                list.Add(namedBrushes["tetrahedron"]);
                list.Add(namedBrushes["cube"]);
                list.Add(namedBrushes["octahedron"]);
                list.Add(namedBrushes["dodecahedron"]);
                list.Add(namedBrushes["icosahedron"]);
                CurrentBrush = namedBrushes["cube"];
            }
            catch(Exception)
            {
                Trace.TraceError("exception in platonics");
            }

#if false
            {
                List<Brush> list = new List<Brush>();
                lists["prism"] = list;
                list.Add(namedBrushes["triangular prism"]);
                list.Add(namedBrushes["cube"]);
                list.Add(namedBrushes["pentagonal prism"]);
                list.Add(namedBrushes["hexagonal prism"]);
                list.Add(namedBrushes["octagonal prism"]);
                list.Add(namedBrushes["decagonal prism"]);
            }

            {
                List<Brush> list = new List<Brush>();
                lists["antiprism"] = list;
                list.Add(namedBrushes["square antiprism"]);
                list.Add(namedBrushes["pentagonal antiprism"]);
                list.Add(namedBrushes["hexagonal antiprism"]);
                list.Add(namedBrushes["octagonal antiprism"]);
                list.Add(namedBrushes["decagonal antiprism"]);
            }

            {
                List<Brush> list = new List<Brush>();
                lists["pyramid"] = list;
                list.Add(namedBrushes["tetrahedron"]);
                list.Add(namedBrushes["square pyramid (J1)"]);
                list.Add(namedBrushes["pentagonal pyramid (J2)"]);
            }

            {
                List<Brush> list = new List<Brush>();
                lists["cupola"] = list;
                list.Add(namedBrushes["triangular cupola (J3)"]);
                list.Add(namedBrushes["square cupola (J4)"]);
                list.Add(namedBrushes["pentagonal cupola (J5)"]);
            }

            {
                List<Brush> list = new List<Brush>();
                lists["rotunda"] = list;
                list.Add(namedBrushes["pentagonal rotunda (J6)"]);
                list.Add(namedBrushes["square cupola (J4)"]);
                list.Add(namedBrushes["pentagonal cupola (J5)"]);
            }
#endif

#if false  //  These models are missing
            List<Brush> archimedean = new List<Brush>();
            lists["archimedean"] = archimedean;

            archimedean.Add(namedBrushes["truncated tetrahedron"]);
            archimedean.Add(namedBrushes["cuboctahedron"]);
            archimedean.Add(namedBrushes["truncated cube"]);
            archimedean.Add(namedBrushes["truncated octahedron"]);
            archimedean.Add(namedBrushes["rhombicuboctahedron"]);
            archimedean.Add(namedBrushes["truncated cuboctahedron"]);
            archimedean.Add(namedBrushes["snub cube"]);
            archimedean.Add(namedBrushes["icosidodecahedron"]);
            archimedean.Add(namedBrushes["truncated dodecahedron"]);
            archimedean.Add(namedBrushes["rhombicosidodecahedron"]);
            archimedean.Add(namedBrushes["truncated icosidodecahedron"]);
            archimedean.Add(namedBrushes["snub dodecahedron"]);
#endif
        }

        private void Message(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        private void MakeBrush(string name, Geometry geometry, bool hasGLContext)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentException();
            }
            if(geometry == null)
            {
                throw new ArgumentNullException();
            }
            GeometryMesh    mesh = new GeometryMesh(geometry, NormalStyle.PolygonNormals);
            Brush           brush = new Brush(this, name, mesh);
            lock(this)
            {
                Log("Adding " + name);
                brushes.Add(brush);
                namedBrushes[name] = brush;
                foreach(var dict in dictionaries)
                {
                    if(
                        (dict.Value == all) || 
                        (
                            (dict.Value != all) &&
                            name.Contains(dict.Key)
                        )
                    )
                    {
                        foreach(var kvp in brush.PolygonDictionary)
                        {
                            if(dict.Value.ContainsKey(kvp.Key) == false)
                            {
                                 dict.Value[kvp.Key] = brush;
                            }
                        }
                    }
                }
            }
        }
        private void MakeBrush(string name, GeometryMesh mesh)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentException();
            }
            if(mesh == null)
            {
                throw new ArgumentNullException();
            }

            Brush brush = new Brush(this, name, mesh);
            lock(this)
            {
                Log("Adding " + name);
                brushes.Add(brush);
                namedBrushes[name] = brush;
                foreach(var dict in dictionaries)
                {
                    if(
                        (dict.Value == all) || 
                        (
                            (dict.Value != all) &&
                            name.Contains(dict.Key)
                        )
                    )
                    {
                        foreach(var kvp in brush.PolygonDictionary)
                        {
                            if(dict.Value.ContainsKey(kvp.Key) == false)
                            {
                                 dict.Value[kvp.Key] = brush;
                            }
                        }
                    }
                }
            }
        }

        const string geometryCachePath = "data/brushGeometries.z";
        const string geometryCacheIndexPath = "data/brushGeometries.index.z";
        private long[] index = null;

        public class BrushInfo
        {
            public int index;
            public BrushInfo(int index){this.index = index; }
        }

        private void LoadBrush(object obj)
        {
            var brushManager = BaseServices.Get<BrushManager>();
            var tls =  brushManager.ThreadInfo;
            BrushInfo bInfo = (BrushInfo)(obj);
            tls.stream.Seek(index[bInfo.index], SeekOrigin.Begin);
            string name = (string)tls.formatter.Deserialize(tls.stream);
            GeometryMesh mesh = (GeometryMesh)tls.formatter.Deserialize(tls.stream);
            MakeBrush(name, mesh);
        }

        public class BrushLoadThreadInfo
        {
            public System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter;
            public MemoryStream stream;

            public BrushLoadThreadInfo(byte[] cacheData)
            {
                formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                stream = new MemoryStream(cacheData);
                //stream.Write(cacheData, 0, cacheData.Length);
                //stream.Seek(0, SeekOrigin.Begin);
            }
        }

        //  \note Be careful to clear these when done loading
        private byte[] cacheData;
        private Dictionary<System.Threading.Thread, BrushLoadThreadInfo> threadInfo;
        public BrushLoadThreadInfo ThreadInfo
        {
            get
            {
                BrushLoadThreadInfo info;
                lock(threadInfo)
                {
                    if(threadInfo.ContainsKey(System.Threading.Thread.CurrentThread))
                    {
                        return threadInfo[System.Threading.Thread.CurrentThread];
                    }
                    info = new BrushLoadThreadInfo(cacheData);
                    threadInfo[System.Threading.Thread.CurrentThread] = info;
                }
                return info;
            }
        }

        private bool TryMakeBrushesFromGeometryCache()
        {
            long begin = System.Environment.TickCount;
            try
            {
                Action<object> loadBrush = new Action<object>(LoadBrush);

                bool useIndex = false;
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                int count = -1;

                try
                {
                    if(System.IO.File.Exists(geometryCacheIndexPath))
                    {
                        FileStream indexFile = new FileStream(geometryCacheIndexPath, FileMode.Open);
                        System.IO.Compression.GZipStream indexZip = new System.IO.Compression.GZipStream(indexFile, System.IO.Compression.CompressionMode.Decompress);
                        BufferedStream indexStream = new BufferedStream(indexZip);

                        index = (long[])f.Deserialize(indexStream);
                        indexStream.Close();
                        useIndex = true;
                    }
                }
                catch(Exception)
                {
                    Debug.WriteLine("index deserialization failed");
                }

                bool geometryCacheExists = File.Exists(geometryCachePath);
                if(geometryCacheExists == false)
                {
                    System.Diagnostics.Trace.TraceWarning("No geometry cache found");
                    return false;
                }

                //  Load file to memory
                FileStream file = new FileStream(geometryCachePath, FileMode.Open);
                System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
                BufferedStream bufferedStream = new BufferedStream(zip);
                cacheData = ReadFully(bufferedStream, 0);
                bufferedStream.Close();
                MemoryStream stream = new MemoryStream(cacheData);
                //stream.Write(cacheData, 0, cacheData.Length);
                //stream.Seek(0, SeekOrigin.Begin);

                count = (int)f.Deserialize(stream);
                if((index != null) && (index.Length == count))
                {
                    stream.Close();
                    Debug.WriteLine("Valid index found, use multi threaded path for geometry cache loading");
                    threadInfo = new Dictionary<System.Threading.Thread, BrushLoadThreadInfo>();
                    for(int i = 0; i < count; ++i)
                    {
                        ThreadManager.Instance.AddTask(loadBrush, new BrushInfo(i));
                    }
                    ThreadManager.Instance.Execute();
                    cacheData = null;
                    threadInfo = null;
                    // \todo add interface
                    RenderStack.Graphics.BufferGL.FinalizeDeserializations();
                }
                else
                {
                    //  No valid index found, single threaded path
                    Debug.WriteLine("No valid index found, single threaded path for geometry cache loading");
                    index = new long[count];
                    useIndex = false;
                    //  Create MemoryStream so we can access Position
                    for(int i = 0; i < count; ++i)
                    {
                        if(useIndex)
                        {
                            stream.Seek(index[i], SeekOrigin.Begin);
                        }
                        else
                        {
                            index[i] = stream.Position;
                        }
                        //Message("BrushManager: " + i.ToString() + "/" + count.ToString() + " GeometryMesh Deserialize");
                        string name = (string)f.Deserialize(stream);
                        GeometryMesh mesh = (GeometryMesh)f.Deserialize(stream);
                        Message("BrushManager: " + i.ToString() + "/" + count.ToString() + " MakeBrush " + name);
                        MakeBrush(name, mesh);
                    }
                    stream.Close();
                }

                UpdateGeometryCacheIndex();
                long end = System.Environment.TickCount;
                long duration = end - begin;
                System.Diagnostics.Trace.WriteLine("Geometry cache load took " + duration.ToString() + " ticks");
                return true;
            }
            catch(Exception)
            {
                System.Diagnostics.Trace.TraceWarning("There was a problem with geometrycache");
                return false;
            }
        }

        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            //  If we've been passed an unhelpful initial length, just use 32K.
            if(initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while((chunk = stream.Read(buffer, read, buffer.Length-read)) > 0)
            {
                read += chunk;

                //  If we've reached the end of our buffer, check to see if there's
                //  any more information
                if(read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    //  End of stream? If so, we're done
                    if(nextByte == -1)
                    {
                        return buffer;
                    }

                    //  Nope. Resize the buffer, put in the byte we've just
                    //  read, and continue
                    byte[] newBuffer = new byte[buffer.Length*2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read]=(byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            //  Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
        private void UpdateGeometryCacheIndex()
        {
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                FileStream indexFile = new FileStream(geometryCacheIndexPath, FileMode.Create);
                System.IO.Compression.GZipStream indexZip = new System.IO.Compression.GZipStream(indexFile, System.IO.Compression.CompressionMode.Compress);
                BufferedStream indexStream = new BufferedStream(indexZip);

                f.Serialize(indexStream, index);
                indexStream.Close();
                Debug.WriteLine("Updating geometrycache index ok");
            }
            catch(Exception)
            {
                System.Diagnostics.Trace.TraceWarning("There was a problem updating geometrycache index");
            }
        }
        private void UpdateGeometryCache()
        {
            try
            {
                FileStream file = new FileStream(geometryCachePath, FileMode.Create);
                System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Compress);
                BufferedStream stream = new BufferedStream(zip);

                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                int count = namedBrushes.Count;
                f.Serialize(stream, count);
                int serializedCount = 0;
                int i = 0;
                foreach(var kvp in namedBrushes)
                {
                    Message("BrushManager: " + i.ToString() + "/" + count.ToString() + " serialize");
                    i++;
                    GeometryMesh mesh = (GeometryMesh)kvp.Value.Model.Batch.MeshSource;
                    if(mesh == null)
                    {
                        throw new InvalidOperationException();
                    }
                    f.Serialize(stream, kvp.Key);
                    f.Serialize(stream, mesh);
                    serializedCount++;
                }
                stream.Close();
            }
            catch(Exception e)
            {
                Trace.TraceWarning("Updating geometry cache failed:\n" + e.ToString());
            }
        }
        public static void RemoveUselessGeometries()
        {
            for(int i = 0; i < 142; ++i)
            {
                string name = null;
                try
                {
                    var netlibPolyhedron = new NetlibPolyhedron("res/polyhedra/" + i.ToString());
                    name = netlibPolyhedron.Name;

                    if(
                        (name.Contains("elongated") == false) &&
                        (name.Contains("augmented") == false)
                    )
                    {
                        continue;
                    }

                    string[] names = { 
                        "res/polyhedra/" + name + ".wrl",
                        "res/polyhedra/" + i.ToString() + ".xml",
                        "res/polyhedra/" + i.ToString()
                    };

                    foreach(var path in names)
                    {
                        try
                        {
                            if(File.Exists(path))
                            {
                                File.Delete(path);
                            }
                        }catch(Exception){}
                    }
                }catch(Exception){}
            }
        }

        System.Text.StringBuilder log = new System.Text.StringBuilder();
        private void Log(string message)
        {
            //Debug.WriteLine(message);
            /*lock(log)
            {
                log.Append(message);
                log.Append('\n');
            }*/
        }
        private void CreateBrush(object obj)
        {
            BrushInfo bInfo = (BrushInfo)(obj);
            int i = bInfo.index;
            Log("Processing brush " + i.ToString());
            try
            {
                Geometry    geometry;
                string      path = "res/polyhedra/" + i.ToString();
                string      name = null;

                if(File.Exists(path) == false)
                {
                    Log("skipping " + path);
                    return;
                }

                try
                {
                    var netlibPolyhedron = new NetlibPolyhedron(path);
                    name = netlibPolyhedron.Name;
                    /*if(name == null || name.Length < 1)
                    {
                        netlibPolyhedron = new NetlibPolyhedron("res/polyhedra/" + i.ToString());
                        name = netlibPolyhedron.Name;
                    }*/
                    /*if(
                        (name != null) && 
                        (name.Length >= 1)
                    )
                    {
                        Trace.WriteLine(name);
                    }*/
                    geometry = new XmlPolyhedron("res/polyhedra/" + i.ToString() + ".xml");
                    Log("loaded " + name + " with XmlPolyhedron");
                }
                catch(System.Exception)
                {
                    try
                    {
                        if(string.IsNullOrEmpty(name) == false)
                        {
                            name = name.Replace(' ', '_');
                            geometry = new VrmlPolyhedronGeometry("res/polyhedra/" + name + ".wrl");
                            Log("loaded " + name + " with VrmlPolyhedronGeometry");
                        }
                        else
                        {
                            Log("skipped - no name");
                        }
                    }
                    catch(System.Exception)
                    {
                        return;
                    }
                    return;
                }

                if(string.IsNullOrEmpty(name))
                {
                    name = i.ToString();
                }
                if(geometry == null)
                {
                    Log("WARNING: Bad shape for file " + i.ToString() + " (" + name + ")");
                    return;
                }

                if(
                    (geometry.Points.Count > 0) && 
                    (geometry.Polygons.Count > 0)
                )
                {
                    //serializationDictionary[i] = geometry;
                    MakeBrush(name, geometry, false);
                }
                else
                {
                    Log("WARNING: Bad shape for file " + i.ToString() + " (" + name + ")");
                }
            }
            catch(System.Exception)
            {
                Log("WARNING: Bad shape for file " + i.ToString());
            }
        }
        private void MakeBrushesFromSources()
        {
            long begin = System.Environment.TickCount;
            int step = 1;

            Action<object> createBrush = new Action<object>(CreateBrush);

            Log("Making brushes from original sources");
            for(int i = 0; i < 142; i += step)
            {
                ThreadManager.Instance.AddTask(createBrush, new BrushInfo(i));
            }
            ThreadManager.Instance.Execute();
            long end = System.Environment.TickCount;
            long duration = end - begin;
            //Log("MakeBrushesFromSources took " + duration.ToString() + " ticks");
            Debug.WriteLine("MakeBrushesFromSources took " + duration.ToString() + " ticks");
            //Trace.Write(log.ToString());
        }

        const string inertiaCachePath = "data/inertiacache.z";
        public void SerializeInertiaCache()
        {
            try
            {
                FileStream file = File.Open(inertiaCachePath, FileMode.Create);
                System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Compress);
                BufferedStream stream = new BufferedStream(zip);

                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                            formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, inertiaCache);
                stream.Close();
            }
            catch(Exception e)
            {
                Trace.TraceError("SerializeInertiaCache() failed: " + e.ToString());
            }
        }

        public void DeserializeInertiaCache()
        {
            try
            {
                if(File.Exists(inertiaCachePath))
                {
                    FileStream file = File.Open(inertiaCachePath, FileMode.Open);
                    System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
                    BufferedStream stream = new BufferedStream(zip);

                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                                formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    inertiaCache = (InertiaCache)formatter.Deserialize(stream);
                    stream.Close();
                }
            }
            catch(Exception e)
            {
                Trace.TraceError("DeserializeInertiaCache() failed: " + e.ToString());
            }
            if(inertiaCache == null)
            {
                inertiaCache = new InertiaCache();
            }
        }

        public void RegisterAssemblyBrushes(System.Reflection.Assembly assembly)
        {
            Message("BrushManager: RegisterAssemblyBrushes...");
            foreach(System.Type type in assembly.GetExportedTypes())
            {
                bool hasAny = type.GetCustomAttributes(
                    typeof(BrushAttribute), false
                ).Any();
                if(hasAny)
                {
                    try
                    {
                        object o = System.Activator.CreateInstance(type);
                        Brush brush = (Brush)(o);
                        brushes.Add(brush);
                    }
                    catch(System.Reflection.TargetInvocationException e)
                    {
                        System.Diagnostics.Trace.TraceError(e.ToString());
                        if(e.InnerException != null)
                        {
                            System.Diagnostics.Trace.TraceError(e.InnerException.ToString());
                        }
                    }
                }
            }
        }

    }
}