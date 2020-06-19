using System;
using System.Collections.Generic;
using System.IO;

namespace example.Brushes
{

    [Serializable]
    public class InertiaCache
    {
        private Dictionary<string, InertiaData> cache = new Dictionary<string,InertiaData>();

        public InertiaData this[string name]
        {
            get
            {
                if(string.IsNullOrEmpty(name))
                {
                    return null;
                }
                lock(this)
                {
                    if(cache.ContainsKey(name))
                    {
                        return cache[name];
                    }
                    return null;
                }
            }
            set
            {
                if(string.IsNullOrEmpty(name))
                {
                    return;
                }
                lock(this)
                {
                    cache[name] = value;
                }
            }
        }
    }
}