using System.Collections.Generic;
using System.Diagnostics;

namespace RenderStack.Services
{
    public interface IService
    {
        HashSet<IService>   Dependencies            { get; }
        bool                InitializeInMainThread  { get; }
        bool                IsInitialized           { get; }

        void InitializationDependsOn(params IService[] services);
        void Initialize();

        string      Name        { get; }
        System.Type ServiceType { get; }
    }
    public abstract class Service : IService
    {
        protected   bool                initializeInMainThread = true;
        private     bool                initialized = false;
        private     HashSet<IService>   dependencies = new HashSet<IService>();
        private     Stopwatch           stopwatch = new Stopwatch();

        public      HashSet<IService>   Dependencies            { get { return dependencies; } }
        public      bool                InitializeInMainThread  { get { return initializeInMainThread; } }
        public      bool                IsInitialized           { get { return initialized; } }

        public static T Get<T>(){ return (T)BaseServices.BaseInstance.Get2(typeof(T)); }

        public Service()
        {
            BaseServices.BaseInstance.Add(this);
        }

        public void InitializationDependsOn(params IService[] services)
        {
            foreach(var service in services)
            {
                if(service != null)
                {
                    dependencies.Add(service);
                }
            }
        }

        public void Initialize()
        {
            stopwatch.Start();
            InitializeService();
            initialized = true;
            stopwatch.Stop();
            System.Console.WriteLine("Initialized " + Name + " in " + stopwatch.ElapsedMilliseconds + " ms");
        }
        protected abstract void InitializeService();

        public abstract string      Name        { get; }

        public virtual System.Type  ServiceType
        {
            get
            {
                return GetType(); 
            }
        }
    }
}
