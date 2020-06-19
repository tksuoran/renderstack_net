using System;
using System.Collections.Generic;

namespace RenderStack.Services
{
    public abstract class BaseServices
    {
        public static   BaseServices        BaseInstance { get; private set; }
        protected HashSet<IService> ServicesSet { get; private set; } = new HashSet<IService>();
        private Dictionary<Type, object> servicesDictionary = new Dictionary<Type, object>();

        public static T Get<T>() { return (T)BaseInstance.Get2(typeof(T)); }

        public object Get2(Type type)
        {
            if (servicesDictionary.ContainsKey(type))
            {
                return servicesDictionary[type];
            }
            return null;
        }

        protected BaseServices()
        {
            BaseInstance = this;
        }

        protected void ClearServices()
        {
            ServicesSet = null;
        }
        public void Add(IService service)
        {
            ServicesSet.Add(service);
            servicesDictionary[service.ServiceType] = service;
        }
        //abstract void Cleanup()
        protected abstract void InitializeService(object obj);
    }

}
