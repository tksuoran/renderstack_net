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
    /*  Parameter holds a value or a list of values of the
     *  type specified by the parameter definition.
     *  Parameters are named.
     */
    [Serializable]
    /*  Comment: Experimental  */ 
    public class Parameter
    {
        private string name;
        public string Name { get { return name; } }

        private List<IParameterValue>   values     = new List<IParameterValue>();
        private ParameterDefinition     definition = null;

        public IParameterValue          Value { get { return values[0]; } } 
        public List<IParameterValue>    Values { get { return values; } }
        public ParameterDefinition Definition
        {
            get
            {
                return definition;
            }
            set
            {
                if(definition != value)
                {
                    definition = value;
                }
            }
        }

        public IParameterValue this[int index]
        {
            get
            {
                return values[index];
            }
        }

        /*public override void Serialize(ISerializer s)
        {
            base.Serialize(s);
            s.SerializeAsset(ref definition);
            s.Serialize(ref values, definition);
        }*/

        public Parameter(ParameterDefinition definition)
        {
            this.name = definition.Name;

            /*  Creates instance of ParameterValue<definition.Type>  */ 
            object ob = System.Activator.CreateInstance(
                typeof(ParameterValue<>).MakeGenericType(definition.Type),
                definition.Default
            );
            IParameterValue i = ob as IParameterValue;
            values.Add(i);
        }

    }
}

