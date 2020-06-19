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
