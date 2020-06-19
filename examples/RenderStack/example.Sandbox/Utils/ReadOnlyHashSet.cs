//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;

namespace example.Sandbox
{
    //[Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class ReadOnlyHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private HashSet<T> collection;

        public ReadOnlyHashSet(HashSet<T> collection)
        {
            this.collection = collection;
        }
        public IEqualityComparer<T> Comparer { get { return collection.Comparer; } }
        public int Count { get { return collection.Count; } }

        public void Add(T item) { throw new NotSupportedException(); }
        public void Clear() { throw new NotSupportedException(); }
        //public void Remove(T item) { throw new NotSupportedException(); }
        public bool Remove(T item) { throw new NotSupportedException(); }
        public bool IsReadOnly { get {return true; } }

        public bool Contains(T item){ return collection.Contains(item); }
        public void CopyTo(T[] array){ collection.CopyTo(array); }
        public void CopyTo(T[] array, int arrayIndex){ collection.CopyTo(array, arrayIndex); }
        public void CopyTo(T[] array, int arrayIndex, int count){ collection.CopyTo(array, arrayIndex, count); }
        public static IEqualityComparer<HashSet<T>> CreateSetComparer(){ return HashSet<T>.CreateSetComparer(); }
        IEnumerator<T> IEnumerable<T>.GetEnumerator(){ return collection.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator(){ return collection.GetEnumerator(); }
        [SecurityCritical]
        public bool IsProperSubsetOf(IEnumerable<T> other){ return collection.IsProperSubsetOf(other); }
        [SecurityCritical]
        public bool IsProperSupersetOf(IEnumerable<T> other){ return collection.IsProperSupersetOf(other); }
        [SecurityCritical]
        public bool IsSubsetOf(IEnumerable<T> other){ return collection.IsSubsetOf(other); }
        public bool IsSupersetOf(IEnumerable<T> other){ return collection.IsSupersetOf(other); }
        public bool Overlaps(IEnumerable<T> other){ return collection.Overlaps(other); }
        [SecurityCritical]
        public bool SetEquals(IEnumerable<T> other){ return collection.SetEquals(other); }
    }
}
