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
    [Serializable]
    /*  Comment: Experimental  */ 
    public class ParameterValues 
    {
        private string                              name;
        private Dictionary<string, IParameterValue> values = new Dictionary<string, IParameterValue>();

        public string                               Name    { get { return name; } }
        public Dictionary<string, IParameterValue>  Values  { get { return values; } }

        public static ParameterValues Create(string name)
        {
            return new ParameterValues(name);
        }
        public ParameterValues(string name)
        :   base()
        {
            this.name = name;
        }

        public IParameterValue<T> Add<T>(string name)
        {
            /*  Creates instance of ParameterValue<type>  */ 
            object ob = System.Activator.CreateInstance(
                typeof(ParameterValue<>).MakeGenericType(typeof(T))
            );
            values.Add(name, ob as IParameterValue);
            return ob as IParameterValue<T>;
        }
        public IParameterValue Add(System.Type type, string name)
        {
            /*  Creates instance of ParameterValue<type>  */ 
            object ob = System.Activator.CreateInstance(
                typeof(ParameterValue<>).MakeGenericType(type)
            );
            values.Add(name, ob as IParameterValue);
            return ob as IParameterValue;
        }
    }
}

