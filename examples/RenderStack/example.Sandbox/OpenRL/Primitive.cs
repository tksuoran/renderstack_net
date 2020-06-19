using RenderStack.Math;

using Caustic.OpenRL;
using RLboolean     = System.Int32;
using RLbuffer      = System.IntPtr;
using RLtexture     = System.IntPtr;
using RLframebuffer = System.IntPtr;
using RLshader      = System.IntPtr;
using RLprogram     = System.IntPtr;
using RLprimitive   = System.Int32;

using RenderStack.Graphics;
using RenderStack.Mesh;

using example.Renderer;

namespace example.Sandbox
{
    //  See 6.3.4. rlBindPrimitive
    public class Primitive : System.IDisposable
    {
        public void Dispose()
        {
            if(PrimitiveHandle != 0)
            {
                RL.BindPrimitive(PrimitiveTarget.Primitive, 0);
                RL.DeletePrimitives(ref PrimitiveHandle);
                PrimitiveHandle = 0;
            }
        }
        private RLprimitive         PrimitiveHandle;
        private IVertexStream       VertexStream;       //  rlVertexAttribBuffer
        private FrontFaceDirection  FrontFace = FrontFaceDirection.Ccw;

        //  Hints
        private bool                IsVisible;
        private bool                IsOccluder;
        private bool                IsStatic;
        private bool                DynamicVertices;
        private bool                DynamicTransform;

        private IProgram            Program;
        private Mesh                Mesh;
        private IBufferRange        vertexBufferRange;
        private IBufferRange        indexBufferRange;

        private Model               Model;

        public void Draw()
        {
            RL.BindPrimitive(PrimitiveTarget.Primitive, PrimitiveHandle);
            Program.Use(0);
            vertexBufferRange.BufferRL.UseRL();
            if(vertexBufferRange.NeedsUploadRL)
            {
                vertexBufferRange.UpdateRL();
            }
            indexBufferRange.BufferRL.UseRL();
            if(indexBufferRange.NeedsUploadRL)
            {
                indexBufferRange.UpdateRL();
            }
            VertexStream.SetupAttributePointers();
            RL.DrawElements(
                (Caustic.OpenRL.BeginMode)indexBufferRange.BeginMode,
                (int)indexBufferRange.Count,
                (Caustic.OpenRL.DrawElementsType)indexBufferRange.DrawElementsTypeGL,
                (int)indexBufferRange.OffsetBytes
            );
        }

        public void Update()
        {
            RL.BindPrimitive(PrimitiveTarget.Primitive, PrimitiveHandle);
            float[] matrix = Model.Frame.LocalToWorld.Matrix.RLMatrix();
            RL.PrimitiveParameterMatrix(
                PrimitiveTarget.Primitive, 
                PrimitiveParameter_m.PrimitiveTransformMatrix, 
                matrix
            );
        }

        public Primitive(Model model, IProgram program)
        {
            Model = model;
            Program = program;

            RL.GenPrimitives(1, out PrimitiveHandle);
            RL.BindPrimitive(PrimitiveTarget.Primitive, PrimitiveHandle);

            RL.PrimitiveParameterString(PrimitiveTarget.Primitive, PrimitiveStringParameter.PrimitiveName, model.Name);

            model.Frame.UpdateHierarchicalNoCache();
            float[] matrix = model.Frame.LocalToWorld.Matrix.RLMatrix();
            RL.PrimitiveParameterMatrix(
                PrimitiveTarget.Primitive, 
                PrimitiveParameter_m.PrimitiveTransformMatrix, 
                matrix
            );

            RL.FrontFace(FrontFace);
            Program.Use(0);

            //RL.GetPrimitiveParameteri(PrimitiveTarget.Primitive, PrimitiveParameter_i.PrimitiveAnimationHint, 
            IsVisible = true;
            IsOccluder = false;
            IsStatic = true;
            DynamicVertices = false;
            DynamicTransform = false;

            Mesh = model.Batch.Mesh;
            this.vertexBufferRange = Mesh.VertexBufferRange;
            VertexStream = vertexBufferRange.VertexStreamRL(Program);
            this.indexBufferRange = Mesh.IndexBufferRange(MeshMode.PolygonFill);
            indexBufferRange.Name = Model.Name + " OpenRL IBO";
            vertexBufferRange.Name = Model.Name + " OpenRL VBO";
            /*System.Console.WriteLine(
                "-----------------------------------------------------------------------------\n" +
                "Model " + Model.Name + " primitive: " + PrimitiveHandle
            );*/
            vertexBufferRange.BufferRL.UseRL();
            if(vertexBufferRange.NeedsUploadRL)
            {
                vertexBufferRange.UpdateRL();
            }
            indexBufferRange.BufferRL.UseRL();
            if(indexBufferRange.NeedsUploadRL)
            {
                indexBufferRange.UpdateRL();
            }
            VertexStream.SetupAttributePointers();
            /*System.Diagnostics.Debug.WriteLine(
                "OpenRL " + Model.Name + " primitive: " + PrimitiveHandle + 
                " VBO: " + Mesh.VertexBufferRange.BufferRL.BufferObject.ToString("X") +
                " IBO: " + indexBufferRange.BufferRL.BufferObject.ToString("X") + 
                " offset: " + indexBufferRange.OffsetBytes + 
                " count: " + indexBufferRange.Count
            );*/
            RL.DrawElements(
                (Caustic.OpenRL.BeginMode)indexBufferRange.BeginMode,
                (int)indexBufferRange.Count,
                (Caustic.OpenRL.DrawElementsType)indexBufferRange.DrawElementsTypeGL,
                (int)indexBufferRange.OffsetBytes
            );
        }
    }
}