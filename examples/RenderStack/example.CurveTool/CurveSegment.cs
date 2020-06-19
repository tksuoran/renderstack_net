using System;

using RenderStack.Graphics;
using RenderStack.Math;

using example.Renderer;


namespace example.CurveTool
{
    public class CurveSegment
    {
        private CurveTool    tool;
        private float        scale;
        private bool         dirty = true;

        public CurveHandle[] Handles   { get; private set; }
        public ICurve        Curve     { get; }
        public CurveTube     Tube      { get; }
        public Model         TubeModel { get; private set; }

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

            this.Curve = curve;
            Tube = new GenericCurveTube(curve);
            Tube.TubeRadius = scale;

            AddHandles(offset);
            AddTube();
            AddLines();
        }

        public CurveHandle Handle(Model hoverModel)
        {
            foreach(var handle in Handles)
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
            TubeModel = new Model("Tube", Tube.TubeMesh, tool.MaterialManager["Tube"]); // tool.MaterialManager["Schlick"]);
            tool.TubeGroup.Add(TubeModel);
        }

        private void AddHandles(Vector3 offset)
        {
            Handles = new CurveHandle[Curve.Count];
            //double r = 4.0;
            for(int i = 0; i < Curve.Count; ++i)
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
                Handles[i].Position = Curve[i].Position;
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
            tool.LineGroup.Add(new Model("Tube lines", Tube.LineMesh,  tool.MaterialManager["EdgeLines"]));
            tool.LineGroup.Add(new Model("Tube Lines 2", Tube.Line2Mesh, tool.MaterialManager["EdgeLines"]));

            tool.LineGroup.Visible = Configuration.curveToolLines;
        }

        public bool CurvePointsToControlHandles()
        {
            bool change = false;

            for(int i = 0; i < Curve.Count; ++i)
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
            for(int i = 0; i < Curve.Count; ++i)
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
                Tube.UpdateTubeMesh();      //  updates adaptive points
                Tube.UpdatePointMesh();     //  needs adaptive points
                Tube.UpdateLine2Mesh();     //  needs adaptive points
                Tube.UpdateLineMesh();
                dirty = false;
            }
        }

    }

}