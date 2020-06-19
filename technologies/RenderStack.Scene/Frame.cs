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

using RenderStack.Math;

namespace RenderStack.Scene
{
    [Serializable]
    /// A node in scene transformation hierarchy.
    /// 
    /// Models, cameras and lights in 3D scene use frames to specify
    /// transformations.
    /// 
    /// \note Mostly stable.
    public class Frame
    {
        private string  name;
        private ulong   lastUpdateSerial;
        public  bool    Updated = false;

        public string Name { get { return name; } set { name = value; } }

        private Frame       parent;
        private Transform   localToParent   = new Transform(Matrix4.Identity, Matrix4.Identity);
        private Transform   localToWorld    = new Transform(Matrix4.Identity, Matrix4.Identity);

        public Frame        Parent          { get { return parent; } set { parent = value; } }
        public Transform    LocalToParent   { get { return localToParent; } }
        public Transform    LocalToWorld    { get { return localToWorld; } }

        public void Debug(int nest)
        {
            Vector3 positionInParent = LocalToParent.Matrix.TransformPoint(Vector3.Zero);
            Vector3 positionInWorld = LocalToWorld.Matrix.TransformPoint(Vector3.Zero);
            System.Diagnostics.Trace.TraceInformation("Frame: " + Name, nest);
            if(Parent != null)
            {
                System.Diagnostics.Trace.TraceInformation("Position in Parent: " + positionInParent.ToString(), nest);
            }
            System.Diagnostics.Trace.TraceInformation("Position in World:  " + positionInWorld.ToString(), nest);
            if (Parent != null)
            {
                Parent.Debug(nest + 1);
            }
        }

        public void UpdateHierarchicalNoCache()
        {
            if(Parent == this)
            {
                System.Diagnostics.Trace.TraceError("Frame parent is self");
            }
            if(Parent != null && Parent != this)
            {
                Parent.UpdateHierarchicalNoCache();
                localToWorld.Set(
                    Parent.LocalToWorld.Matrix * localToParent.Matrix,
                    localToParent.InverseMatrix * Parent.LocalToWorld.InverseMatrix
                );
            }
            else
            {
                localToWorld.Set(
                    localToParent.Matrix,
                    localToParent.InverseMatrix
                );
            }
        }

        public void UpdateHierarchical(ulong updateSerial)
        {
            if(Updated == true)
            {
                return;
            }
            if(this.lastUpdateSerial == updateSerial)
            {
                return;
            }
            this.lastUpdateSerial = updateSerial;
            if(Parent == this)
            {
                System.Diagnostics.Trace.TraceError("Frame parent is self");
            }
            if(Parent != null && Parent != this)
            {
                Parent.UpdateHierarchical(updateSerial);
                localToWorld.Set(
                    Parent.LocalToWorld.Matrix * localToParent.Matrix,
                    localToParent.InverseMatrix * Parent.LocalToWorld.InverseMatrix
                );
            }
            else
            {
                localToWorld.Set(
                    localToParent.Matrix,
                    localToParent.InverseMatrix
                );
            }
        }
    }
}
