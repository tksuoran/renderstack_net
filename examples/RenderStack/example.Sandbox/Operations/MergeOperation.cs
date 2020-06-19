//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Physics;

using example.Renderer;

namespace example.Sandbox
{
    public partial class Operations
    {
        public void Merge()
        {
            // \todo 
#if true
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count <= 1)
            {
                return;
            }

            //  First pass: Find which model to use as new reference
            //  There should be one model which parent is not any
            //  of the models in selection.
            Model referenceModel = null;
            foreach(Model referenceCandidate in selectionManager.Models)
            {
                if(referenceCandidate == null)
                {
                    continue;
                }

                bool candidateHasParentInSelection = false;
                foreach(Model otherModel in selectionManager.Models)
                {
                    if(otherModel == null)
                    {
                        continue;
                    }
                    if(referenceCandidate.Frame.Parent == otherModel.Frame)
                    {
                        candidateHasParentInSelection = true;
                        break;
                    }
                }

                if(candidateHasParentInSelection == false)
                {
                    referenceModel = referenceCandidate;
                    break;
                }
            }
            if(referenceModel == null)
            {
                throw new Exception("Unable to pick selection root model for merge operation");
            }

            //  This is where compound shapes are collected. 
            //  All shapes are added in the coordinate system of the reference model.
            //  Models that have no rigid bodies are skipped
            List<CompoundShape.TransformedShape> shapes = new List<CompoundShape.TransformedShape>();

            //  Add reference model (if it has rigidbody)
            if(referenceModel.RigidBody != null)
            {
                //var shape = new CompoundShape.TransformedShape(
                //    referenceModel.RigidBody.Shape, 
                //    JMatrix.Identity, 
                //    JVector.Zero
                //);
                //shapes.Add(shape);
                Shape oldShape = referenceModel.RigidBody.Shape;
                CompoundShape compound = oldShape as CompoundShape;
                if(compound != null)
                {
                    foreach(var part in compound.Shapes)
                    {
                        var shape = new CompoundShape.TransformedShape(
                            part.Shape, 
                            part.Transform
                            //part.Orientation,
                            //part.Position
                        );
                        shapes.Add(shape);
                    }
                }
                else
                {
                    var shape = new CompoundShape.TransformedShape(
                        referenceModel.RigidBody.Shape, 
                        Matrix4.Identity
                        //JMatrix.Identity, 
                        //JVector.Zero
                    );
                    shapes.Add(shape);
                }

            }

            GeometryMesh referencePolymesh  = referenceModel.Batch.MeshSource as GeometryMesh;
            Geometry     mergedGeometry     = new CloneGeometryOperation(referencePolymesh.Geometry, null).Destination;

            //  Remove original reference model from scene graph
            sceneManager.RemoveModel(referenceModel);
            foreach(Model mergeModel in selectionManager.Models)
            {
                if(mergeModel == null || mergeModel == referenceModel)
                {
                    continue;
                }
                GeometryMesh addPolymesh = mergeModel.Batch.MeshSource as GeometryMesh;
                Geometry     addGeometry = new CloneGeometryOperation(addPolymesh.Geometry, null).Destination;
                Matrix4      toSameSpace = referenceModel.Frame.LocalToWorld.InverseMatrix * mergeModel.Frame.LocalToWorld.Matrix;
                addGeometry.Transform(toSameSpace);

                if(mergeModel.RigidBody != null)
                {
                    //  TODO: Decompose - for now we only have rotations and translations so this should work
                    /*JMatrix orientation;
                    orientation.M11 = toSameSpace._00;
                    orientation.M21 = toSameSpace._01;
                    orientation.M31 = toSameSpace._02;
                    orientation.M12 = toSameSpace._10;
                    orientation.M22 = toSameSpace._11;
                    orientation.M32 = toSameSpace._12;
                    orientation.M13 = toSameSpace._20;
                    orientation.M23 = toSameSpace._21;
                    orientation.M33 = toSameSpace._22;
                    orientation.M13 = toSameSpace._20;
                    orientation.M23 = toSameSpace._21;
                    orientation.M33 = toSameSpace._22;
                    Vector3 p = toSameSpace.GetColumn3(3);
                    JVector position = new JVector(p.X, p.Y, p.Z);*/

                    Shape oldShape = mergeModel.RigidBody.Shape;
                    CompoundShape compound = oldShape as CompoundShape;
                    if(compound != null)
                    {
                        foreach(var part in compound.Shapes)
                        {
                            var shape = new CompoundShape.TransformedShape(
                                part.Shape, 
                                toSameSpace * part.Transform
                                //JMatrix.Multiply(orientation, part.Orientation),
                                //position + JVector.Transform(part.Position, orientation)
                            );
                            shapes.Add(shape);
                        }
                    }
                    else
                    {
                        var shape = new CompoundShape.TransformedShape(
                            oldShape, 
                            toSameSpace
                            //orientation, 
                            //position 
                        );
                        shapes.Add(shape);
                    }
                }

                mergedGeometry.Merge(addGeometry);
                sceneManager.RemoveModel(mergeModel);
            }

            Matrix4 modelOffset = Matrix4.Identity;
            CompoundShape compoundShape = null;
            if(shapes.Count > 0)
            {
                compoundShape = new CompoundShape(shapes);

                Matrix4 vertexOffset = Matrix4.CreateTranslation(compoundShape.Shift.X, compoundShape.Shift.Y, compoundShape.Shift.Z);
                modelOffset = Matrix4.CreateTranslation(-compoundShape.Shift.X, -compoundShape.Shift.Y, -compoundShape.Shift.Z);

                mergedGeometry.Transform(vertexOffset);
            }
            mergedGeometry.ComputePolygonCentroids();
            mergedGeometry.ComputePolygonNormals();
            //mergedGeometry.ComputeCornerNormals(0.0f);
            mergedGeometry.SmoothNormalize("corner_normals", "polygon_normals", (0.0f * (float)Math.PI));
            mergedGeometry.BuildEdges();

            GeometryMesh mergedPolyMesh = new GeometryMesh(mergedGeometry, NormalStyle.PolygonNormals);

            //  Compute bounding box - debug
            {
                var bbox = new BoundingBox();
                var pointLocations = mergedGeometry.PointAttributes.Find<Vector3>("point_locations");
                bbox.Clear();
                foreach(Point point in mergedGeometry.Points)
                {
                    bbox.ExtendBy(pointLocations[point]);
                }
            }

            Model newModel = new Model(
                "Merge(" + referenceModel.Name + ", " + selectionManager.Models.Count + ")",
                mergedPolyMesh,
                referenceModel.Batch.Material,
                referenceModel.Frame.LocalToParent.Matrix * modelOffset
            );
            if(compoundShape != null)
            {
                newModel.PhysicsShape = compoundShape;
            }
            //sceneManager.MakeModelPhysicsConvexHull(newModel);  This won't work as Shift is not properly taken into account
            newModel.Frame.Parent = referenceModel.Frame.Parent;
            newModel.Static = referenceModel.Static;

            selectionManager.ClearSelection();

            //  This will create rigidbody using .PhysicsShape if set, otherwise it will call
            //  MakeModelPhysicsConvexHull().
            sceneManager.AddModel(newModel);

            selectionManager.HoverModel = null;
            selectionManager.HoverPolygon = null;
#endif
        }
    }
}
