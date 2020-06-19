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
using System.Collections.Generic;
using Single = System.Single;
using Double = System.Double;
using Int16  = System.Int16;
using Int32  = System.Int32;
using UInt16 = System.UInt16;
using UInt32 = System.UInt32;

namespace RenderStack.Parameters
{
    /*  Parameter definition specifies type and name of
     *  parameter, recommended UI control type if any, 
     *  default value, optional minimum and maximum values 
     *  etc. */ 
    [Serializable]
    /*  Comment: Experimental  */ 
    public class ParameterDefinition
    {
        private System.Type type;
        private string      name;
        public System.Type Type { get { return type; } }
        public string Name { get { return name; } }

        #region Combo and Fields
        private Dictionary<int, string> keyToName   = new Dictionary<int, string>();
        private Dictionary<int, string> indexToName = new Dictionary<int, string>();
        private Dictionary<string, int> nameToIndex = new Dictionary<string, int>();
        private Dictionary<string, int> nameToKey   = new Dictionary<string, int>();

        public Dictionary<int, string> KeyToName   { get { return keyToName;   } }
        public Dictionary<int, string> IndexToName { get { return indexToName; } }
        public Dictionary<string, int> NameToIndex { get { return nameToIndex; } }
        public Dictionary<string, int> NameToKey   { get { return nameToKey;   } }

        public string this[int key]
        {
            get
            {
                return this.keyToName[key];
            }
            set
            {
                int number = this.keyToName.Count;
                this.keyToName  [key]    = value;
                this.indexToName[number] = value;
                this.nameToIndex[value]  = number;
                this.nameToKey  [value]  = key;
            }
        }
        #endregion 
        #region File
        private string path;
        private string filter;

        public string Path   { get { return path;   } set { path   = value; } }
        public string Filter { get { return filter; } set { filter = value; } }
        #endregion File

        public ParameterDefinition()
        {
            name = null;

            type        = null;
            description = null;
            control     = ParameterControl.None;
            index       = 0;
        }

        public ParameterDefinition(
            System.Type      type,
            string           name,
            string           description,
            ParameterControl control,
            int              index
        )
        {
            this.name = name;

            this.type        = type;
            this.description = description;
            this.control     = control;
            this.index       = index;
        }

        private int index;
        public  int Index { get { return index; } }

        public IParameterValue Default;
        public IParameterValue Min;
        public IParameterValue Max;
        public IParameterValue SuggestedMin;
        public IParameterValue SuggestedMax;

        private string description;
        public string  Description
        {
            get { return description; }
            set { description = value; }
        }

        private ParameterControl control = ParameterControl.LinearSlider;
        public  ParameterControl Control { get { return control; } }

#if false
        public void SerializeParameterValue(ISerializer s, ref IParameterValue value)
        {
            var genericType = value.GetType().GetGenericArguments()[0];
            if(value is IParameterValue<bool>)
            {
                var typedValueParameter = value as IParameterValue<bool>;
                typedValueParameter.Value.Serialize(s);
            } 
            else if(value is IParameterValue<bool[]>)
            {
                var typedValueParameter = value as IParameterValue<bool[]>;
                typedValueParameter.Value.Serialize(s);
            } 
            if(value is IParameterValue<byte>)
            {
                var typedValueParameter = value as IParameterValue<byte>;
                typedValueParameter.Value.Serialize(s);
            } 
            else if(value is IParameterValue<byte[]>)
            {
                var typedValueParameter = value as IParameterValue<byte[]>;
                typedValueParameter.Value.Serialize(s);
            } 
            if(value is IParameterValue<Single>)
            {
                var typedValueParameter = value as IParameterValue<Single>;
                typedValueParameter.Value.Serialize(s);
            } 
            else if(value is IParameterValue<Single[]>)
            {
                var typedValueParameter = value as IParameterValue<Single[]>;
                typedValueParameter.Value.Serialize(s);
            } 
            else if(value is IParameterValue<Double>)
            {
                var typedValueParameter = value as IParameterValue<Double>;
                typedValueParameter.Value.Serialize(s);
            } 
            else if(value is IParameterValue<Double[]>)
            {
                var typedValueParameter = value as IParameterValue<Double[]>;
                typedValueParameter.Value.Serialize(s);
            } 
            else if(value is IParameterValue<Int16>)
            {
                var typedValueParameter = value as IParameterValue<Int16>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Int16[]>)
            {
                var typedValueParameter = value as IParameterValue<Int16>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Int32>)
            {
                var typedValueParameter = value as IParameterValue<Int32>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Int32[]>)
            {
                var typedValueParameter = value as IParameterValue<Int32>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<UInt16>)
            {
                var typedValueParameter = value as IParameterValue<UInt16>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<UInt16[]>)
            {
                var typedValueParameter = value as IParameterValue<UInt16>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<UInt32>)
            {
                var typedValueParameter = value as IParameterValue<UInt32>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<UInt32[]>)
            {
                var typedValueParameter = value as IParameterValue<UInt32>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Float2>)
            {
                var typedValueParameter = value as IParameterValue<Float2>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Float2[]>)
            {
                var typedValueParameter = value as IParameterValue<Float2[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Float3>)
            {
                var typedValueParameter = value as IParameterValue<Float3>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Float3[]>)
            {
                var typedValueParameter = value as IParameterValue<Float3[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Float4>)
            {
                var typedValueParameter = value as IParameterValue<Float4>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Float4[]>)
            {
                var typedValueParameter = value as IParameterValue<Float4[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Vector2>)
            {
                var typedValueParameter = value as IParameterValue<Vector2>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Vector2[]>)
            {
                var typedValueParameter = value as IParameterValue<Vector2[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Vector3>)
            {
                var typedValueParameter = value as IParameterValue<Vector3>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Vector3[]>)
            {
                var typedValueParameter = value as IParameterValue<Vector3[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Vector4>)
            {
                var typedValueParameter = value as IParameterValue<Vector4>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Vector4[]>)
            {
                var typedValueParameter = value as IParameterValue<Vector4[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Matrix4>)
            {
                var typedValueParameter = value as IParameterValue<Matrix4>;
                typedValueParameter.Value.Serialize(s);
            }
            else if(value is IParameterValue<Matrix4[]>)
            {
                var typedValueParameter = value as IParameterValue<Matrix4[]>;
                typedValueParameter.Value.Serialize(s);
            }
            else
            {
                throw new System.Exception("Unhandled IParameterType<ValueType> value type");
            }
        }

        //  ISerializable
        public override void Serialize(ISerializer s)
        {
            base.Serialize(s);

            s.Serialize(ref type);
            s.Serialize(ref keyToName);
            s.Serialize(ref indexToName);
            s.Serialize(ref nameToIndex);
            s.Serialize(ref nameToKey);
            s.Serialize(ref keyToName);
            s.Serialize(ref indexToName);
            s.Serialize(ref nameToIndex);
            s.Serialize(ref nameToKey);
            s.Serialize(ref path);
            s.Serialize(ref filter);
            s.Serialize(ref index);
            SerializeParameterValue(s, ref Default);
            SerializeParameterValue(s, ref Min);
            SerializeParameterValue(s, ref Max);
            SerializeParameterValue(s, ref SuggestedMin);
            SerializeParameterValue(s, ref SuggestedMax);
            s.Serialize(ref description);
            s.Serialize(ref control);
        }
#endif
    }

}

