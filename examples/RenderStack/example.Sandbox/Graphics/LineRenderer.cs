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

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;

namespace example.Sandbox
{
    public class LineRenderer : Service
    {
        public override string Name
        {
            get { return "LineRenderer"; }
        }

        IRenderer                   renderer;

        private Mesh                mesh;
        private IBuffer             vertexBuffer;
        private IBuffer             indexBuffer;
        private IBufferRange        vertexBufferRange;
        private IBufferRange        indexBufferRange;
        private VertexBufferWriter  vertexWriter;
        private IndexBufferWriter   indexWriter;
        private Attribute           position;
        private Attribute           edgeColor;
        private Material            material;

        public Mesh                 Mesh        { get { return mesh; } }
        public bool                 NotEmpty    { get { return indexBufferRange.Count > 0; } }

        public void Connect(IRenderer renderer)
        {
            this.renderer = renderer;
            InitializationDependsOn(renderer);
        }

        protected override void InitializeService()
        {
            if(
                (RenderStack.Graphics.Configuration.canUseGeometryShaders) &&
                (RenderStack.Graphics.Configuration.glslVersion >= 330)
            )
            {
                material = new Material("", renderer.Programs["WideLine"], renderer.MaterialUB);
            }
            else
            {
                material = new Material("", renderer.Programs["ColorFill"], renderer.MaterialUB);
            }

            LineWidth = 1.0f;

            mesh = new Mesh();

            var vertexFormat = new VertexFormat();
            position = vertexFormat.Add(new Attribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3));
            edgeColor = vertexFormat.Add(new Attribute(VertexUsage.Color, VertexAttribPointerType.Float, 1, 4));

            vertexBuffer = BufferFactory.Create(vertexFormat, BufferUsageHint.DynamicDraw);
            indexBuffer = BufferFactory.Create(DrawElementsType.UnsignedInt, BufferUsageHint.DynamicDraw);

            mesh.VertexBufferRange = vertexBufferRange = vertexBuffer.CreateVertexBufferRange();
            indexBufferRange = mesh.FindOrCreateIndexBufferRange(
                MeshMode.EdgeLines,
                indexBuffer,
                BeginMode.Lines
            );

            vertexWriter = new VertexBufferWriter(mesh.VertexBufferRange);
            indexWriter = new IndexBufferWriter(indexBufferRange);
        }

        private float lineWidth = 1.0f;
        public float LineWidth
        {
            set
            {
                if(
                    (RenderStack.Graphics.Configuration.canUseGeometryShaders) &&
                    (RenderStack.Graphics.Configuration.glslVersion >= 330)
                )
                {
                    material.Floats("line_width").Set(value, value * value * 0.25f);
                    material.Floats("line_color").Set(1.0f, 1.0f, 1.0f, 1.0f);
                    material.Sync();
                }
                lineWidth = value;
            }
            get
            {
                return lineWidth;
            }
        }

        /*public void Initialize()
        {
        }*/

        public void Render(RenderStack.Scene.Frame frame)
        {
            if(indexBufferRange.Count == 0)
            {
                return;
            }
#if false
            IMeshSource meshSource = HoverModel.Batch.MeshSource;
            if(meshSource is GeometryMesh && selectionMesh != null)
            {
                renderer.CurrentModel       = null;
                renderer.CurrentMaterial    = null;
                renderer.CurrentProgram     = renderer.Programs["WideLine"];
                renderer.CurrentFrame       = HoverModel.Frame;
                renderer.CurrentMesh        = selectionMesh.GetMesh;
                if(!RenderStack.Graphics.Configuration.GL3)
                {
                    GL.LineWidth(lineWidth);
                }
            }
#endif
            //  \todo Use renderstate
            GL.PolygonOffset(-1.0f, 1.0f);
            GL.Enable(EnableCap.PolygonOffsetFill);

            renderer.Requested.Material = material;
            renderer.Requested.Program = material.Program;
            if(
                (RenderStack.Graphics.Configuration.canUseGeometryShaders) ||
                (RenderStack.Graphics.Configuration.glslVersion < 330)
            )
            {
                GL.LineWidth(LineWidth);
            }
            renderer.SetFrame(frame);
            renderer.Requested.Mesh      = Mesh;
            renderer.Requested.MeshMode  = MeshMode.EdgeLines;

            //  \todo Use renderstate
            GL.PolygonOffset(-1.0f, 1.0f);
            GL.Disable(EnableCap.PolygonOffsetFill);

            renderer.RenderCurrent();
        }

        public void Begin()
        {
            vertexWriter.BeginEdit();
            indexWriter.BeginEdit();
        }
        public void Line(Vector3 start, Vector3 end, Vector4 rgba)
        {
            indexWriter.Line(vertexWriter.CurrentIndex, vertexWriter.CurrentIndex + 1);
            indexWriter.CurrentIndex += 2;
            vertexWriter.Set(edgeColor, rgba.X,     rgba.Y,     rgba.Z,     rgba.W);
            vertexWriter.Set(position,  start.X,    start.Y,    start.Z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(edgeColor, rgba.X,     rgba.Y,     rgba.Z,     rgba.W);
            vertexWriter.Set(position,  end.X,      end.Y,      end.Z);     ++vertexWriter.CurrentIndex;
        }
        public void Line(Vector3 start, Vector4 c0, Vector3 end, Vector4 c1)
        {
            indexWriter.Line(vertexWriter.CurrentIndex, vertexWriter.CurrentIndex + 1);
            indexWriter.CurrentIndex += 2;
            vertexWriter.Set(edgeColor, c0.X,       c0.Y,       c0.Z,       c0.W);
            vertexWriter.Set(position,  start.X,    start.Y,    start.Z);   ++vertexWriter.CurrentIndex;
            vertexWriter.Set(edgeColor, c1.X,       c1.Y,       c1.Z,       c1.W);
            vertexWriter.Set(position,  end.X,      end.Y,      end.Z);     ++vertexWriter.CurrentIndex;
        }
        public void Bone(
            Matrix4 boneMatrix,
            float   length,
            Vector4 color
        )
        {
            float   scale1      = -0.05f;
            float   scale2      = -0.10f;
            Vector3 localRoot   =  Vector3.Zero;
            Vector3 localTip    = -Vector3.UnitZ * length;
            Vector3 localMid0   = new Vector3(-scale1,  scale1, scale2) * length;
            Vector3 localMid1   = new Vector3( scale1,  scale1, scale2) * length;
            Vector3 localMid2   = new Vector3( scale1, -scale1, scale2) * length;
            Vector3 localMid3   = new Vector3(-scale1, -scale1, scale2) * length;
            Vector3 worldRoot   = boneMatrix.TransformPoint(localRoot);
            Vector3 worldTip    = boneMatrix.TransformPoint(localTip);
            Vector3 worldMid0   = boneMatrix.TransformPoint(localMid0);
            Vector3 worldMid1   = boneMatrix.TransformPoint(localMid1);
            Vector3 worldMid2   = boneMatrix.TransformPoint(localMid2);
            Vector3 worldMid3   = boneMatrix.TransformPoint(localMid3);

            Line(worldRoot, worldMid0, color);
            Line(worldRoot, worldMid1, color);
            Line(worldRoot, worldMid2, color);
            Line(worldRoot, worldMid3, color);

            Line(worldMid0, worldMid1, color);
            Line(worldMid1, worldMid2, color);
            Line(worldMid2, worldMid3, color);
            Line(worldMid3, worldMid0, color);

            Line(worldMid0, worldTip,  color);
            Line(worldMid1, worldTip,  color);
            Line(worldMid2, worldTip,  color);
            Line(worldMid3, worldTip,  color);
        }
        public void End()
        {
            vertexWriter.EndEdit();
            indexWriter.EndEdit();
        }
    }
}
