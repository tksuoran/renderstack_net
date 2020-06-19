//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;
using Conversions = RenderStack.Math.Conversions;

namespace example.CurveTool
{
    /*  Comment: Experimental  */ 
    public abstract class CurveTube
    {
        #region members
        public      ICurve              curve;

        private     Mesh                pointMesh;
        private     Mesh                lineMesh;
        private     Mesh                line2Mesh;
        private     Mesh                tubeMesh;
        private     GeometryMesh        frozenMesh;

        public      Mesh                PointMesh       { get { return pointMesh; } }
        public      Mesh                LineMesh        { get { return lineMesh; } }
        public      Mesh                Line2Mesh       { get { return line2Mesh; } }
        public      Mesh                TubeMesh        { get { return tubeMesh; } }
        public      GeometryMesh        FrozenMesh      { get { return frozenMesh; } }
        //private     BufferRangeGL       tubeVertexBufferRange;
        //private     BufferRangeGL       tubeIndexBufferRange;
        private     VertexBufferWriter  tubeVertexWriter;
        private     IndexBufferWriter   tubeIndexWriter;
        private     VertexBufferWriter  pointVertexWriter;
        private     IndexBufferWriter   pointIndexWriter;
        private     VertexBufferWriter  lineVertexWriter;
        private     IndexBufferWriter   lineIndexWriter;
        private     VertexBufferWriter  line2VertexWriter;
        private     IndexBufferWriter   line2IndexWriter;
        private     Attribute           tubePosition;
        private     Attribute           tubeNormal;
        private     Attribute           tubeTangent;
        private     Attribute           tubeColor;
        private     Attribute           tubeT;
        private     Attribute           tubeId;
        private     Vector3             LastN                   = Vector3.UnitX;
        public      List<float>         adaptivePoints          = new List<float>();
        public      float               AdaptiveEpsilon         = 0.9995f;
        public      float               AdaptiveNastyEpsilon    = 0.9999f;
        public      float               TubeRadius              = 1.0f;
        private     int                 tubeStackCount          =  0;
        private     int                 tubeSliceCount          = 28;
        public      int                 TubeStackCount { get { return tubeStackCount; } set { tubeStackCount = value; } }

        #endregion members

        public CurveTube(ICurve curve)
        {
            this.curve = curve;

            pointMesh   = new Mesh();
            lineMesh    = new Mesh();
            line2Mesh   = new Mesh();
            tubeMesh    = new Mesh();

            var positionColor = new VertexFormat(); // pointMesh, lineMesh, line2Mesh
            var tubeVertexFormat = new VertexFormat();

            positionColor.   Add(new Attribute(VertexUsage.Position,    VertexAttribPointerType.Float, 0, 3));
            positionColor.   Add(new Attribute(VertexUsage.Color,       VertexAttribPointerType.Float, 0, 4));
            tubeVertexFormat.Add(new Attribute(VertexUsage.Position,    VertexAttribPointerType.Float, 0, 3));
            tubeVertexFormat.Add(new Attribute(VertexUsage.Tangent,     VertexAttribPointerType.Float, 0, 3));
            tubeVertexFormat.Add(new Attribute(VertexUsage.Normal,      VertexAttribPointerType.Float, 0, 3));
            tubeVertexFormat.Add(new Attribute(VertexUsage.Color,       VertexAttribPointerType.Float, 0, 4));
            tubeVertexFormat.Add(new Attribute(VertexUsage.Color,       VertexAttribPointerType.Float, 1, 1));
            tubeVertexFormat.Add(new Attribute(VertexUsage.Id,          VertexAttribPointerType.UnsignedInt, 0, 1));

            //  Could perhaps use BufferPool
            var positionColorVertexBuffer  = BufferFactory.Create(positionColor, BufferUsageHint.DynamicDraw);
            var tubeVertexBuffer           = BufferFactory.Create(tubeVertexFormat, BufferUsageHint.DynamicDraw);
            var indexBuffer                = BufferFactory.Create(DrawElementsType.UnsignedInt, BufferUsageHint.DynamicDraw);

            pointMesh.VertexBufferRange = positionColorVertexBuffer.CreateVertexBufferRange();
            lineMesh.VertexBufferRange  = positionColorVertexBuffer.CreateVertexBufferRange();
            line2Mesh.VertexBufferRange = positionColorVertexBuffer.CreateVertexBufferRange();
            tubeMesh.VertexBufferRange  = tubeVertexBuffer.CreateVertexBufferRange();

            var pointIndexBuffer = pointMesh.FindOrCreateIndexBufferRange(
                MeshMode.CornerPoints, 
                indexBuffer,
                BeginMode.Points
            );
            var lineIndexBuffer = lineMesh.FindOrCreateIndexBufferRange(
                MeshMode.EdgeLines, 
                indexBuffer,
                BeginMode.Lines
            );
            var line2IndexBuffer =line2Mesh.FindOrCreateIndexBufferRange(
                MeshMode.EdgeLines, 
                indexBuffer,
                BeginMode.Lines
            );
            var tubeIndexBuffer = tubeMesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonFill,
                indexBuffer,
                BeginMode.Triangles
            );

            tubeVertexWriter    = new VertexBufferWriter(tubeMesh.VertexBufferRange);
            tubeIndexWriter     = new IndexBufferWriter(tubeIndexBuffer);
            pointVertexWriter   = new VertexBufferWriter(pointMesh.VertexBufferRange);
            pointIndexWriter    = new IndexBufferWriter(pointIndexBuffer);
            lineVertexWriter    = new VertexBufferWriter(lineMesh.VertexBufferRange);
            lineIndexWriter     = new IndexBufferWriter(lineIndexBuffer);
            line2VertexWriter   = new VertexBufferWriter(line2Mesh.VertexBufferRange);
            line2IndexWriter    = new IndexBufferWriter(line2IndexBuffer);

            UpdateIndexBuffers();
        }

        public void UpdateIndexBuffers()
        {
            lineIndexWriter.BeginEdit();

            UInt32 index = 0;
            for(int i = 0; i < curve.Count - 1; ++i)
            {
                lineIndexWriter.Line(index, index + 1); lineIndexWriter.CurrentIndex += 2;
                index += 1;
            }
            for(int i = 0; i < curve.Count; ++i)
            {
                lineIndexWriter.Line(index + 1, index + 2); lineIndexWriter.CurrentIndex += 2;
                index += 2;
            }

            lineIndexWriter.EndEdit();
        }

        public void UpdateFrozenMesh()
        {
            if(frozenMesh == null)
            {
                frozenMesh = new GeometryMesh(new Tube(curve, TubeRadius, 12, 60), NormalStyle.CornerNormals);
            }
            else
            {
                //frozenMesh.Reset();
                frozenMesh.Geometry = new Tube(curve, TubeRadius, 12, 60);
                frozenMesh.BuildMeshFromGeometry(BufferUsageHint.StaticDraw, NormalStyle.CornerNormals);
            }
        }

        public void UpdatePointMesh()
        {
            if(adaptivePoints.Count == 0)
            {
                UpdateTubeMesh();
            }

            var vertexFormat = pointMesh.VertexBufferRange.VertexFormat;
            var position    = vertexFormat.FindAttribute(VertexUsage.Position, 0);
            var color       = vertexFormat.FindAttribute(VertexUsage.Color, 0);

            pointVertexWriter.BeginEdit();
            pointIndexWriter.BeginEdit();

            foreach(float t in adaptivePoints)
            {
                Vector3 pos = curve.PositionAt(t);

                pointVertexWriter.Set(position, pos.X, pos.Y, pos.Z);
                pointVertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
                pointIndexWriter.Point(pointVertexWriter.CurrentIndex);
                ++pointVertexWriter.CurrentIndex; 
                ++pointIndexWriter.CurrentIndex;
            }
            pointVertexWriter.EndEdit();
            pointIndexWriter.EndEdit();
        }

        public virtual void UpdateTubeMesh()
        {
            var vertexFormat = tubeMesh.VertexBufferRange.VertexFormat;
            tubePosition    = vertexFormat.FindAttribute(VertexUsage.Position, 0);
            tubeNormal      = vertexFormat.FindAttribute(VertexUsage.Normal, 0);
            tubeTangent     = vertexFormat.FindAttribute(VertexUsage.Tangent, 0);
            tubeColor       = vertexFormat.FindAttribute(VertexUsage.Color, 0);
            tubeT           = vertexFormat.FindAttribute(VertexUsage.Color, 1);
            tubeId          = vertexFormat.FindAttribute(VertexUsage.Id, 0);

            tubeVertexWriter.BeginEdit();
            tubeIndexWriter.BeginEdit();

            //  \todo hack fixme
            ((GenericCurve)(curve)).UpdateNURBS();

            //  Compute initial N
            Vector3 pos     = curve.PositionAt(0.0f);
            Vector3 posNext = curve.PositionAt(1.0f / 512.0f);
            Vector3 d1      = posNext - pos;
            Vector3 T       = Vector3.Normalize(d1);
            Vector3 N       = d1.MinAxis;
            Vector3 B       = Vector3.Normalize(Vector3.Cross(T, N));
            LastN           = Vector3.Normalize(Vector3.Cross(B, T));

            UpdateTubeMeshWithAdaptiveSubdivision();

            for(int stack = 1; stack < tubeStackCount - 2; ++stack)
            {
                int nextStack = stack + 1;
                for(int slice = 0; slice < tubeSliceCount; ++slice)
                {
                    int nextSlice = (slice + 1) % tubeSliceCount;
                    tubeIndexWriter.Quad(
                        (uint)(stack     * tubeSliceCount + nextSlice),
                        (uint)(stack     * tubeSliceCount + slice),
                        (uint)(nextStack * tubeSliceCount + slice),
                        (uint)(nextStack * tubeSliceCount + nextSlice)
                    );
                    tubeIndexWriter.CurrentIndex += 6;
                }
            }
            for(int slice = 0; slice < tubeSliceCount; ++slice)
            {
                int nextSlice1 = (slice + 1) % tubeSliceCount;
                tubeIndexWriter.Triangle(
                    (uint)(0 * tubeSliceCount),
                    (uint)(0 * tubeSliceCount + slice),
                    (uint)(0 * tubeSliceCount + nextSlice1)
                );
                tubeIndexWriter.CurrentIndex += 3;
            }
            for(int slice = 0; slice < tubeSliceCount; ++slice)
            {
                int nextSlice1 = (slice + 1) % tubeSliceCount;
                tubeIndexWriter.Triangle(
                    (uint)((tubeStackCount - 1) * tubeSliceCount + nextSlice1),
                    (uint)((tubeStackCount - 1) * tubeSliceCount + slice),
                    (uint)((tubeStackCount - 1) * tubeSliceCount)
                );
                tubeIndexWriter.CurrentIndex += 3;
            }
            tubeVertexWriter.EndEdit();
            tubeIndexWriter.EndEdit();
        }

        public abstract void UpdateTubeMeshWithAdaptiveSubdivision();

        public void UpdateLineMesh()
        {
            var vertexFormat = lineMesh.VertexBufferRange.VertexFormat;
            var position = vertexFormat.FindAttribute(VertexUsage.Position, 0);
            var color    = vertexFormat.FindAttribute(VertexUsage.Color, 0);

            lineVertexWriter.BeginEdit();
            for(int i = 0; i < curve.Count; ++i)
            {
                lineVertexWriter.Set(color, 1.0f, 1.0f, 1.0f, 1.0f);
                lineVertexWriter.Set(position, curve[i].Position); ++lineVertexWriter.CurrentIndex;
            }

            for(int i = 0; i < curve.Count; ++i)
            {
                lineVertexWriter.Set(color, 1.0f, 0.0f, 0.0f, 1.0f);
                lineVertexWriter.Set(position, curve[i].Position); ++lineVertexWriter.CurrentIndex;
                lineVertexWriter.Set(color, 0.0f, 0.0f, 1.0f, 1.0f);
                lineVertexWriter.Set(position, curve[i].Position.X, 0.0f, curve[i].Position.Z); ++lineVertexWriter.CurrentIndex;
            }
            lineVertexWriter.EndEdit();
        }
        public void UpdateLine2Mesh()
        {
            if(adaptivePoints.Count == 0)
            {
                UpdateTubeMesh();
            }

            var vertexFormat = Line2Mesh.VertexBufferRange.VertexFormat;
            var position = vertexFormat.FindAttribute(VertexUsage.Position, 0);
            var color    = vertexFormat.FindAttribute(VertexUsage.Color, 0);

            line2VertexWriter.BeginEdit();
            line2IndexWriter.BeginEdit();

            foreach(float t in adaptivePoints)
            {
                Vector3 pos = curve.PositionAt(t);

                float r, g, b;
                RenderStack.Math.Conversions.HSVtoRGB(360.0f * t, 1.0f, 1.0f, out r, out g, out b);

                line2IndexWriter.Line(line2VertexWriter.CurrentIndex, line2VertexWriter.CurrentIndex + 1);
                line2IndexWriter.CurrentIndex += 2;
                line2VertexWriter.Set(color, r, g, b, 1.0f);
                line2VertexWriter.Set(position, pos.X, pos.Y, pos.Z); ++line2VertexWriter.CurrentIndex;
                line2VertexWriter.Set(color, r, g, b, 1.0f);
                line2VertexWriter.Set(position, pos.X, 0.0f,  pos.Z); ++line2VertexWriter.CurrentIndex;
            }
            line2VertexWriter.EndEdit();
            line2IndexWriter.EndEdit();
        }

        public void GenerateTubeVerticesForCusp(Vector3 T1, Vector3 T2, float t0, float t1)
        {
            float tm = t0 + (t1 - t0) * 0.5f;

            Vector3 cross = Vector3.Cross(T1, T2);

            //  Special case: cross product is 0, there is no unique axis
            //  for rotation. Pick 'any' perpendicular vector.
            if(cross.LengthSquared < 0.001f)
            {
                cross = Vector3.Cross(T1, T1.MinAxis);
            }
            Vector3 axis    = Vector3.Normalize(cross);
            float   dot     = Vector3.Dot(T1, T2);
            float   theta   = (float)Math.Acos(dot);
            Vector3 pos     = curve.PositionAt(tm);

            float qtStep = 1.0f / (float)8.0;
            for(float qt = 0.0f; qt <= 1.0f; qt += qtStep)
            {
                Matrix4 R = Matrix4.CreateRotation(qt * theta, axis);
                Vector3 T = R * T1;
                Vector3 B = Vector3.Normalize(Vector3.Cross(T, LastN));
                LastN   = Vector3.Normalize(Vector3.Cross(B, T));

                ++tubeStackCount;
                for(int slice = 0; slice < tubeSliceCount; ++slice)
                {
                    float relPhi    = (float)slice / (float)tubeSliceCount;
                    float phi       = (float)Math.PI * 2.0f * relPhi;
                    float sinPhi    = (float)Math.Sin(phi);
                    float cosPhi    = (float)Math.Cos(phi);

                    Vector3 v = pos;
                    Vector3 n = LastN * sinPhi + B * cosPhi;
                    v += n * TubeRadius;

                    float r, g, b;
                    RenderStack.Math.Conversions.HSVtoRGB(360.0f * tm, 1.0f, 1.0f, out r, out g, out b);

                    tubeVertexWriter.Set(tubePosition,  v.X, v.Y, v.Z); 
                    tubeVertexWriter.Set(tubeNormal,    n.X, n.Y, n.Z); 
                    tubeVertexWriter.Set(tubeTangent,   T.X, T.Y, T.Z); 
                    //tubeVertexWriter.Set(tubeTangent, B.X, B.Y, B.Z); 
                    tubeVertexWriter.Set(tubeColor,     r,   g,   b, 1.0f); 
                    tubeVertexWriter.Set(tubeT,         tm); 
                    tubeVertexWriter.Set(tubeId, 0);
                    ++tubeVertexWriter.CurrentIndex;
                }
            }
        }
        public void GenerateTubeVertexRing(float t, bool cap)
        {
            //float tStep = 1.0f / 512.0f;

            Vector3 position    = curve.PositionAt(t);
            Vector3 tangent     = curve.TangentAt(t);
            if(cap)
            {
                TubeRingCap(t, position, tangent);
            }
            else
            {
                TubeRing(t, position, tangent);
            }
        }
        public void TubeRing(float t, Vector3 pos, Vector3 tangent)
        {
            ++tubeStackCount;

            Vector3 T   = tangent;
            Vector3 B   = Vector3.Normalize(Vector3.Cross(T, LastN));
            LastN       = Vector3.Normalize(Vector3.Cross(B, T));

            for(int slice = 0; slice < tubeSliceCount; ++slice)
            {
                float relPhi    = (float)slice / (float)tubeSliceCount;
                float phi       = (float)Math.PI * 2.0f * relPhi;
                float sinPhi    = (float)Math.Sin(phi);
                float cosPhi    = (float)Math.Cos(phi);

                Vector3 v = pos;
                Vector3 n = LastN * sinPhi + B * cosPhi;
                v += n * TubeRadius;

                float r, g, b;
                RenderStack.Math.Conversions.HSVtoRGB(360.0f * t, 1.0f, 1.0f, out r, out g, out b);

                tubeVertexWriter.Set(tubePosition,  v.X, v.Y, v.Z); 
                tubeVertexWriter.Set(tubeNormal,    n.X, n.Y, n.Z); 
                tubeVertexWriter.Set(tubeTangent,   T.X, T.Y, T.Z); 
                //tubeVertexWriter.Set(tubeTangent, B.X, B.Y, B.Z); 
                tubeVertexWriter.Set(tubeColor,     r,   g,   b, 1.0f); 
                tubeVertexWriter.Set(tubeT,         t); 
                tubeVertexWriter.Set(tubeId, 0);
                ++tubeVertexWriter.CurrentIndex;
            }
        }
        public void TubeRingCap(float t, Vector3 pos, Vector3 tangent)
        {
            ++tubeStackCount;

            Vector3 T   = tangent;
            Vector3 B   = Vector3.Normalize(Vector3.Cross(T, LastN));
            LastN       = Vector3.Normalize(Vector3.Cross(B, T));

            for(int slice = 0; slice < tubeSliceCount; ++slice)
            {
                float relPhi    = (float)slice / (float)tubeSliceCount;
                float phi       = (float)Math.PI * 2.0f * relPhi;
                float sinPhi    = (float)Math.Sin(phi);
                float cosPhi    = (float)Math.Cos(phi);

                Vector3 v = pos;
                Vector3 n = LastN * sinPhi + B * cosPhi;
                v += n * TubeRadius;

                float r, g, b;
                RenderStack.Math.Conversions.HSVtoRGB(360.0f * t, 1.0f, 1.0f, out r, out g, out b);

                if(t == 0.0f)
                {
                    n = -T;
                }
                if(t == 1.0f)
                {
                    n = T;
                }

                tubeVertexWriter.Set(tubePosition,  v.X, v.Y, v.Z); 
                tubeVertexWriter.Set(tubeNormal,    n.X, n.Y, n.Z); 
                tubeVertexWriter.Set(tubeTangent,   T.X, T.Y, T.Z); 
                //tubeVertexWriter.Set(tubeTangent, B.X, B.Y, B.Z); 
                tubeVertexWriter.Set(tubeColor,     r,   g,   b, 1.0f); 
                tubeVertexWriter.Set(tubeT,         t); 
                tubeVertexWriter.Set(tubeId, 0);
                ++tubeVertexWriter.CurrentIndex;
            }
        }

    }
}
