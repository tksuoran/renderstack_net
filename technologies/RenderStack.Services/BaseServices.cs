//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Diagnostics;
using System.Collections.Generic;


namespace RenderStack.Services
{
    public abstract class BaseServices
    {
        public static   BaseServices        BaseInstance { get; private set; }

        private         HashSet<IService>   servicesSet = new HashSet<IService>();
        protected       HashSet<IService>   ServicesSet { get { return servicesSet; } }
        private         Dictionary<System.Type, object> servicesDictionary = new Dictionary<Type,object>();

        public static T Get<T>(){ return (T)BaseInstance.Get2(typeof(T)); }

        public object Get2(System.Type type)
        {
            if(servicesDictionary.ContainsKey(type))
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
            servicesSet = null;
        }
        public void Add(IService service)
        {
            servicesSet.Add(service);
            servicesDictionary[service.ServiceType] = service;
        }
        //abstract void Cleanup()
        protected abstract void InitializeService(object obj);
    }

}
