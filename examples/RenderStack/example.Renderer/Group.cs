//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Cadenza;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Mesh;
using RenderStack.Scene;

namespace example.Renderer
{
    // \brief Helper class to support ID rendering
    public struct IDListEntry
        :   IComparable<IDListEntry>
        ,   IEquatable<IDListEntry>
    {
        public uint     Key;
        public Model    Model;

        public IDListEntry(uint key)
        {
            Key = key;
            Model = null;
        }

        public IDListEntry(uint key, Model model)
        {
            Key = key;
            Model = model;
        }

        public int CompareTo(IDListEntry other)
        {
            return Key.CompareTo(other.Key);
        }

        public bool Equals(IDListEntry other)
        {
            return Key.Equals(other.Key);
        }
    }

    // \brief Helper class to support instanced rendering
    public class Instances
    {
        private Dictionary<Tuple<Mesh, Material>, List<Model>>  collection = new Dictionary<Tuple<Mesh,Material>,List<Model>>();
        public Dictionary<Tuple<Mesh, Material>, List<Model>>   Collection { get { return collection; } }

        public void Clear()
        {
            collection.Clear();
        }
        public bool ContainsKey(Tuple<Mesh,Material> tuple)
        {
            return collection.ContainsKey(tuple);
        }
        public List<Model> this[Tuple<Mesh,Material> key]
        {
            get { return collection[key]; }
            set { collection[key] = value; }
        }
    }

    // \brief Collection of Models and Lights. Supports instanced and ID rendering.
    public class Group
    {
        [NonSerialized] private List<IDListEntry> idList = new List<IDListEntry>();

        private List<Model>                 models                  = new List<Model>();
        private List<Light>                 lights                  = new List<Light>();
        private Instances                   opaqueInstances         = new Instances();
        private Instances                   transparentInstances    = new Instances();
        private Instances                   allInstances            = new Instances();

        public ReadOnlyCollection<Model>    Models                  { get { return models.AsReadOnly(); } }
        public List<Light>                  Lights                  { get { return lights; } }
        public List<IDListEntry>            IdList                  { get { return idList; } }
        public Instances                    OpaqueInstances         { get { return opaqueInstances; } }
        public Instances                    TransparentInstances    { get { return transparentInstances; } }
        public Instances                    AllInstances            { get { return allInstances; } }

        public string                       Name;
        public bool                         Visible = true;

        public Group(string name)
        {
            Name = name;
        }

        public void Clear()
        {
            models.Clear();
            opaqueInstances.Clear();
            transparentInstances.Clear();
            allInstances.Clear();
            lights.Clear();
        }
        public void Add(Model model)
        {
            models.Add(model);
            var key = new Tuple<Mesh,Material>(model.Batch.Mesh, model.Batch.Material);
            var instances = model.Batch.Material.BlendState.Enabled
                ? transparentInstances
                : opaqueInstances;
            if(instances.ContainsKey(key) == false)
            {
                instances[key] = new List<Model>();
            }
            instances[key].Add(model);
            if(allInstances.ContainsKey(key) == false)
            {
                allInstances[key] = new List<Model>();
            }
            allInstances[key].Add(model);
        }
        public void Remove(Model model)
        {
            models.Remove(model);
            var key = new Tuple<Mesh,Material>(model.Batch.Mesh, model.Batch.Material);
            var instances = model.Batch.Material.BlendState.Enabled
                ? transparentInstances
                : opaqueInstances;
            if(instances.ContainsKey(key))
            {
                instances[key].Remove(model);
            }
            if(allInstances.ContainsKey(key))
            {
                allInstances[key].Remove(model);
            }
        }
        public bool AlreadyHas(IDListEntry compareKey)
        {
            if(IdList.Count == 0 || compareKey.Key >= IdList.Last().Key)
            {
                return false;
            }
            int index = IdList.BinarySearch(compareKey);
            if(index == ~IdList.Count)
            {
                return false;
            }
            else if(index < 0)
            {
                int larger = ~index;
                if(larger == 0)
                {
                    /*  Index was before this IdList  */ 
                    return false;
                }
            }
            return true;
        }
        public Model IdTest(IDListEntry compareKey, out uint polygonId)
        {
            polygonId = 0;
            if(IdList.Count == 0 || compareKey.Key >= IdList.Last().Key)
            {
                return null;
            }
            int index = IdList.BinarySearch(compareKey);
            if(index == ~IdList.Count)
            {
                /*  Index was after this IdList  */ 
                return null;
            }
            else if(index < 0)
            {
                int larger = ~index;
                if(larger == 0)
                {
                    /*  Index was before this IdList  */ 
                    return null;
                }
                Model model = IdList[larger - 1].Model;
                if(model != null)
                {
                    uint offset = IdList[larger - 1].Key;
                    polygonId = compareKey.Key - offset;
                    return model;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Model model = IdList[index].Model;
                if(model != null)
                {
                    uint offset = IdList[index].Key;
                    polygonId = compareKey.Key - offset;
                    return model;
                }
                else
                {
                    return null;
                }
            }
        }

        public override string ToString()
        {
            return Name + " " + models.Count;
        }
    }
}
