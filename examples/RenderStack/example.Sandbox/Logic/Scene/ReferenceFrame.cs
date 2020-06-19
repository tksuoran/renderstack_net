//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using RenderStack.Math;

namespace example.Sandbox
{
    /*  Comment: Experimental  */ 
    public class ReferenceFrame
    {
        public Vector3 Center;          //  Polygon center
        public Vector3 Normal;          //  Polygon normal
        public Vector3 LastPosition;    //  Position of last corner of polygon
        public Vector3 FirstPosition;   //  Position of first corner of polygon

        public void Transform(Matrix4 m)
        {
            Center          = m.TransformPoint(Center);
            Normal          = Vector3.Normalize(m.TransformDirection(Normal));
            LastPosition    = m.TransformPoint(LastPosition);
            FirstPosition   = m.TransformPoint(FirstPosition);
        }

        public float Scale
        {
            get
            {
                return FirstPosition.Distance(Center);
            }
        }

        public Matrix4 GetTransform()
        {
            Matrix4 transform = Matrix4.Identity;
            Vector3 view = Normal;
            Vector3 edge = Vector3.Normalize(FirstPosition - Center);
            Vector3 side = Vector3.Normalize(Vector3.Cross(view, edge));

            /*  Right axis is column 0  */ 
            transform._00 = edge.X;
            transform._10 = edge.Y;
            transform._20 = edge.Z;

            /*  Up    axis is column 1  */ 
            transform._01 = side.X;
            transform._11 = side.Y;
            transform._21 = side.Z;

            /*  View  axis is column 2  */ 
            transform._02 = view.X;
            transform._12 = view.Y;
            transform._22 = view.Z;

            /*  Translation is column 3  */ 
            transform._03 = Center.X;
            transform._13 = Center.Y;
            transform._23 = Center.Z;
            return transform;
        }
    }
}
