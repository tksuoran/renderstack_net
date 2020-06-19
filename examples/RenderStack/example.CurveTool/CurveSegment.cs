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

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;
using Sphere = RenderStack.Geometry.Shapes.Sphere;

namespace example.CurveTool
{
    public class CurveSegment
    {
        private CurveTool       tool;
        private float           scale;
        private ICurve          curve;
        private CurveTube       tube;
        private Model           tubeModel;
        private bool            dirty = true;
        private CurveHandle[]   handles;

        public CurveHandle[]    Handles     { get { return handles; } }
        public ICurve           Curve       { get { return curve; } }
        public CurveTube        Tube        { get { return tube; } }
        public Model            TubeModel   { get { return tubeModel; } }

        public CurveSegment(
            CurveTool   tool, 
            float       scale,
            Vector3     offset
        )
        {
            this.tool = tool;
            this.scale = scale;

            GenericCurve    curve = new GenericCurve();
            int             count = 16;
            double          rotations = 2.5;
            double          radius = 4.0;
            for(int i = 0; i < count; ++i)
            {
                double rel = (double)(i) / (double)(count);
                double theta = rotations * rel * 2.0 * Math.PI;
                double r = (1.0 - rel) * radius * 0.8;
                curve.Add(
                    new ControlPoint(
                        new Vector3(
                            r * Math.Cos(theta + Math.PI),
                            scale * (1.0 + theta * 0.33333), 
                            r * Math.Sin(theta + Math.PI)
                        ),
                        new Floats(0.0f, 0.0f, 0.0f)
                    )
                );
            }

            this.curve = curve;
            tube            = new GenericCurveTube(curve);
            tube.TubeRadius = scale;

            AddHandles(offset);
            AddTube();
            AddLines();
        }

        public CurveHandle Handle(Model hoverModel)
        {
            foreach(var handle in handles)
            {
                if(handle.model == hoverModel)
                {
                    return handle;
                }
            }
            return null;
        }

        private void AddTube()
        {
            Tube.UpdateFrozenMesh();
            dirty = true;
            Update();
            //tubeModel = new Model("Tube", tube.TubeMesh, tool.MaterialManager["Tube"]);
            tubeModel = new Model("Tube", tube.TubeMesh, tool.MaterialManager["Tube"]); // tool.MaterialManager["Schlick"]);
            tool.TubeGroup.Add(tubeModel);
        }

        private void AddHandles(Vector3 offset)
        {
            handles = new CurveHandle[curve.Count];
            //double r = 4.0;
            for(int i = 0; i < curve.Count; ++i)
            {
                //double rel = (double)(i) / (double)(curve.Count - 1);
                //double theta = rel * 2.0 * Math.PI;
                Handles[i] = new CurveHandle();
                Handles[i].model = new Model(
                    "controlHandle" + i.ToString(), 
                    tool.HandleMesh, 
                    tool.HandleMaterial
                );
                tool.HandleGroup.Add(Handles[i].model);
                handles[i].Position = curve[i].Position;
#if false
                handles[i].Position = new Vector3(
                    r * Math.Cos(theta),
                    rel, 
                    r * Math.Sin(theta)
                ) + offset;
#endif
            }
        }

        private void AddLines()
        {
            tool.LineGroup.Add(new Model("Tube lines",    tube.LineMesh,  tool.MaterialManager["EdgeLines"]));
            tool.LineGroup.Add(new Model("Tube Lines 2",  tube.Line2Mesh, tool.MaterialManager["EdgeLines"]));

            tool.LineGroup.Visible = Configuration.curveToolLines;
        }

        public bool CurvePointsToControlHandles()
        {
            bool change = false;

            for(int i = 0; i < curve.Count; ++i)
            {
                if(
                    (Curve[i].Position.X != Handles[i].Position.X) ||
                    (Curve[i].Position.Y != Handles[i].Position.Y) ||
                    (Curve[i].Position.Z != Handles[i].Position.Z) ||
                    //(Curve[i].Parameters[0] != Handles[i].Parameters[0]) ||
                    (Curve[i].Parameters[1] != Handles[i].Parameters[1]) ||
                    (Curve[i].Parameters[2] != Handles[i].Parameters[2])
                )
                {
                    Curve[i].Position = Handles[i].Position;
                    //Curve[i].Parameters[0] = Handles[i].Parameters[0];
                    Curve[i].Parameters[1] = Handles[i].Parameters[1];
                    Curve[i].Parameters[2] = Handles[i].Parameters[2];
                    dirty = true;
                    Curve.Dirty = true;
                    change = true;
                }
            }
            return change;
        }
        public void ControlHandlesToCurve()
        {
            for(int i = 0; i < curve.Count; ++i)
            {
                Handles[i].Position = Curve[i].Position;
            }
        }
        public void UpdateControlPoints()
        {
            Vector3 editHandleModelPosition = new Vector3(
                tool.EditHandle.Frame.LocalToWorld.Matrix._03,
                tool.EditHandle.Frame.LocalToWorld.Matrix._13,
                tool.EditHandle.Frame.LocalToWorld.Matrix._23
            );
            CurvePointsToControlHandles();
#if false
            if(tool.EditHandle.Selected || tool.LockEditHandle)
            {
                tube.curve.AdjustControlPointsToMakeCurveGoThrough(
                    tool.EditT, 
                    editHandleModelPosition
                );
                dirty = true;
            }
#endif
            ControlHandlesToCurve();
        }
        public void Update()
        {
            if(dirty)
            {
                tube.UpdateTubeMesh();      //  updates adaptive points
                tube.UpdatePointMesh();     //  needs adaptive points
                tube.UpdateLine2Mesh();     //  needs adaptive points
                tube.UpdateLineMesh();
                dirty = false;
            }
        }

    }

}