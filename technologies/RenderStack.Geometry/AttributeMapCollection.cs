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
using System.Linq;

namespace RenderStack.Geometry
{
    [Serializable]
    /// \brief Collection of attributes, to be used in Geometry for example.
    /// \note Mostly stable.
    public class AttributeMapCollection<KeyType>
    {
        public Dictionary<string, object> AttributeMaps = new Dictionary<string, object>();

        public int Count { get { return AttributeMaps.Count; } }

        public bool Contains<ValueType>(string name)
        {
            if(AttributeMaps.ContainsKey(name) == true)
            {
                if(AttributeMaps[name] is Dictionary<KeyType, ValueType>)
                {
                    return true;
                }
                else
                {
                    throw new System.Exception("Wrong type in attribute map collection");
                }
            }
            return false;
        }

        /// \brief Retrieve existing named collection of attributes.
        /// \tparam KeyType     Attribute key type
        /// \tparam ValueType   Attribute value type
        /// \param name         Name of attribute collection
        /// \return Collection of attributes with specified key and value types and name
        public Dictionary<KeyType, ValueType> Find<ValueType>(string name)
        {
            return (Dictionary<KeyType, ValueType>) AttributeMaps[name];
        }
        public Dictionary<KeyType, ValueType> FindOrNull<ValueType>(string name)
        {
            if(AttributeMaps.ContainsKey(name))
            {
                return (Dictionary<KeyType, ValueType>) AttributeMaps[name];
            }
            return null;
        }
        /// \brief Retrieve existing named collection of attributes, or create a new one.
        /// \tparam KeyType     Attribute key type
        /// \tparam ValueType   Attribute value type
        /// \param  name        Name of attribute collection
        /// \return Collection of attributes with specified key and value types and name.
        /// This can be a new, empty collection in case existing collection
        /// was not found.
        public Dictionary<KeyType, ValueType> FindOrCreate<ValueType>(string name)
        {
            if(Contains<ValueType>(name) == false)
            {
                Dictionary<KeyType, ValueType> dictionary = new Dictionary<KeyType, ValueType>();
                AttributeMaps[name] = dictionary;
                return dictionary;
            }
            else
            {
                return (Dictionary<KeyType, ValueType>) AttributeMaps[name];
            }
        }
        public void Replace(string name, string temp)
        {
            AttributeMaps[name] = AttributeMaps[temp];
            AttributeMaps.Remove(temp);
        }
    }
}
