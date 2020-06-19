using System.Collections.Generic;
using Jitter.LinearMath;
using RenderStack.Math;

namespace RenderStack.Physics
{
    public class CompoundShape : Shape
    {
        public class TransformedShape
        {
            public Matrix4  Transform;
            public Shape    Shape;
            public TransformedShape(Shape shape, Matrix4 transform)
            {
                Shape = shape;
                Transform = transform;
            }
        }

        internal Jitter.Collision.Shapes.CompoundShape compoundShape;

        internal override Jitter.Collision.Shapes.Shape shape { get { return compoundShape; } }

        public Vector3  Shift { get { JVector p = compoundShape.Shift; return new Vector3(p.X, p.Y, p.Z); } }

        private TransformedShape[] shapes;
        public TransformedShape[] Shapes { get { return shapes; } }

        public CompoundShape(List<TransformedShape> shapes)
        {
            this.shapes = shapes.ToArray();
            var jshapes = new List<Jitter.Collision.Shapes.CompoundShape.TransformedShape>();

            //  Remove original reference model from scene graph
            foreach(var shape in shapes)
            {
                //  TODO: Decompose - for now we only have rotations and translations so this should work
                JMatrix orientation = Util.ToJitter(shape.Transform);
                Vector3 p = shape.Transform.GetColumn3(3);
                JVector position = Util.ToJitter(p);

                Jitter.Collision.Shapes.Shape oldShape = shape.Shape.shape;
                Jitter.Collision.Shapes.CompoundShape compound = oldShape as Jitter.Collision.Shapes.CompoundShape;
                if(compound != null)
                {
                    foreach(var part in compound.Shapes)
                    {
                        var newShape = new Jitter.Collision.Shapes.CompoundShape.TransformedShape(
                            part.Shape, 
                            JMatrix.Multiply(orientation, part.Orientation),
                            position + JVector.Transform(part.Position, orientation)
                        );
                        jshapes.Add(newShape);
                    }
                }
                else
                {
                    var newShape = new Jitter.Collision.Shapes.CompoundShape.TransformedShape(
                        oldShape, 
                        orientation, 
                        position 
                    );
                    jshapes.Add(newShape);
                }

            }
            compoundShape = new Jitter.Collision.Shapes.CompoundShape(jshapes);

            Matrix4 vertexOffset = Matrix4.CreateTranslation(compoundShape.Shift.X, compoundShape.Shift.Y, compoundShape.Shift.Z);
            Matrix4 modelOffset = Matrix4.CreateTranslation(-compoundShape.Shift.X, -compoundShape.Shift.Y, -compoundShape.Shift.Z);
        }
    }
}
