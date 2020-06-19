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
using System.Collections.Generic;
using UInt32 = System.UInt32;
using OpenTK.Graphics.OpenGL;

using RenderStack.Geometry;
using RenderStack.Graphics;
using RenderStack.Math;

using Attribute = RenderStack.Graphics.Attribute;
using Buffer    = RenderStack.Graphics.BufferGL;

namespace RenderStack.Mesh
{
    public enum NormalStyle
    {
        CornerNormals,
        PolygonNormals,
        PointNormals
    }
    [Serializable]
    /// \brief Converts Geometry to Mesh.
    /// \warning Does not currently optimize.
    /// \todo Add vertex and index buffer optimizations.
    /// \note Mostly stable. somewhat experimental.
    public partial class GeometryMesh : IMeshSource
    {
        private Geometry.Geometry   geometry;
        private Mesh                mesh;

        public Geometry.Geometry    Geometry    { get { return geometry; } set { geometry = value; } }
        public Mesh                 GetMesh     { get { return mesh; } set { mesh = value; } }
        public string               Name        { get { return GetMesh.Name; } set { GetMesh.Name = value; } }

        public GeometryMesh(
            Geometry.Geometry   geometry,
            NormalStyle         normalStyle,
            VertexFormat        vertexFormat
        )
        {
            Geometry = geometry;
            BuildMeshFromGeometry(BufferUsageHint.StaticDraw, normalStyle, vertexFormat);
        }

        public GeometryMesh(Geometry.Geometry geometry, NormalStyle normalStyle)
        {
            Geometry = geometry;
            BuildMeshFromGeometry(BufferUsageHint.StaticDraw, normalStyle);
        }
#if false
        public GeometryMesh()
        {
            Geometry = new Geometry.Geometry();
            GetMesh = new Mesh(BufferUsageHint.StaticDraw);
        }
        public GeometryMesh(BufferUsageHint bufferUsageHint) 
        {
            Geometry = new Geometry.Geometry();
            GetMesh = new Mesh(bufferUsageHint);
        }
        public void Reset()
        {
            Geometry = new Geometry.Geometry();
            GetMesh = new Mesh(BufferUsageHint.StaticDraw);
        }
#endif
        public void BuildMeshFromGeometry(
            BufferUsageHint bufferUsageHint, 
            NormalStyle     normalStyle
        )
        {
            var attributePosition       = new Attribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3);
            var attributeNormal         = new Attribute(VertexUsage.Normal,   VertexAttribPointerType.Float, 0, 3);   /*  content normals     */
            var attributeNormalFlat     = new Attribute(VertexUsage.Normal,   VertexAttribPointerType.Float, 1, 3);   /*  flat normals        */
            var attributeNormalSmooth   = new Attribute(VertexUsage.Normal,   VertexAttribPointerType.Float, 2, 3);   /*  smooth normals      */
            var attributeColor          = new Attribute(VertexUsage.Color,    VertexAttribPointerType.Float, 0, 4);
            var attributeIdVec3         = new Attribute(VertexUsage.Id,       VertexAttribPointerType.Float, 0, 3);
            var attributeIdUInt         = (RenderStack.Graphics.Configuration.useIntegerPolygonIDs) 
                                        ? new Attribute(VertexUsage.Id,       VertexAttribPointerType.UnsignedInt, 0, 1)
                                        : null;

            VertexFormat vertexFormat = new VertexFormat();
            vertexFormat.Add(attributePosition);
            vertexFormat.Add(attributeNormal);
            vertexFormat.Add(attributeNormalFlat);
            vertexFormat.Add(attributeNormalSmooth);

            Dictionary<Corner, Vector2> cornerTexcoords = null;
            Dictionary<Point, Vector2>  pointTexcoords  = null;

            if(Geometry.CornerAttributes.Contains<Vector2>("corner_texcoords"))
            {
                cornerTexcoords = Geometry.CornerAttributes.Find<Vector2>("corner_texcoords");
            }
            if(Geometry.PointAttributes.Contains<Vector2>("point_texcoords"))
            {
                pointTexcoords = Geometry.PointAttributes.Find<Vector2>("point_texcoords");
            }

            if(cornerTexcoords != null || pointTexcoords != null)
            {
                var attributeTexcoord = new Attribute(VertexUsage.TexCoord, VertexAttribPointerType.Float, 0, 2);
                vertexFormat.Add(attributeTexcoord);
            }
            // \todo When do we want color attribute and when we don't?
            //if(cornerColors != null || pointColors != null)
            //{
                vertexFormat.Add(attributeColor);
            //}
            vertexFormat.Add(attributeIdVec3);
            if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
            {
                vertexFormat.Add(attributeIdUInt);
            }

            BuildMeshFromGeometry(bufferUsageHint, normalStyle, vertexFormat);
        }
        public void BuildMeshFromGeometry(
            BufferUsageHint bufferUsageHint, 
            NormalStyle     normalStyle,
            VertexFormat    vertexFormat
        )
        {
            if(Geometry.PolygonAttributes.Contains<Vector3>("polygon_normals") == false)
            {
                Geometry.ComputePolygonNormals();
            }
            Geometry.ComputePointNormals("point_normals_smooth");

            if(Geometry.PointAttributes.Contains<Vector3>("polygon_centroids") == false)
            {
                Geometry.ComputePolygonCentroids();
            }

            var polygonIdsVector3   = Geometry.PolygonAttributes.FindOrCreate<Vector3>("polygon_ids_vec3");
            var polygonIdsUInt32    = (RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
                                    ? Geometry.PolygonAttributes.FindOrCreate<UInt32>("polygon_ids_uint")
                                    : null;

            Dictionary<Corner, Vector3> cornerNormals = null;
            Dictionary<Point, Vector3>  pointNormals  = null;
            Dictionary<Point, Vector3>  pointNormalsSmooth = Geometry.PointAttributes.Find<Vector3>("point_normals_smooth");

            bool normalsFound = false;
            if(Geometry.CornerAttributes.Contains<Vector3>("corner_normals"))
            {
                cornerNormals = Geometry.CornerAttributes.Find<Vector3>("corner_normals");
                normalsFound = true;
            }
            if(Geometry.PointAttributes.Contains<Vector3>("point_normals"))
            {
                pointNormals = Geometry.PointAttributes.Find<Vector3>("point_normals");
                normalsFound = true;
            }
            if(normalsFound == false)
            {
                //Geometry.ComputeCornerNormals(0.0f * (float)System.Math.PI);
                Geometry.SmoothNormalize("corner_normals", "polygon_normals", (0.0f * (float)System.Math.PI));
                cornerNormals = Geometry.CornerAttributes.Find<Vector3>("corner_normals");
            }

            Dictionary<Corner, Vector2> cornerTexcoords = null;
            Dictionary<Point, Vector2>  pointTexcoords  = null;

            if(Geometry.CornerAttributes.Contains<Vector2>("corner_texcoords"))
            {
                cornerTexcoords = Geometry.CornerAttributes.Find<Vector2>("corner_texcoords");
            }
            if(Geometry.PointAttributes.Contains<Vector2>("point_texcoords"))
            {
                pointTexcoords = Geometry.PointAttributes.Find<Vector2>("point_texcoords");
            }

            //Dictionary<Corner, Vector4> cornerColors = null;
            var cornerColors = default(Dictionary<Corner, Vector4>);
            Dictionary<Point, Vector4>  pointColors = null;
            if(Geometry.CornerAttributes.Contains<Vector4>("corner_colors"))
            {
                cornerColors = Geometry.CornerAttributes.Find<Vector4>("corner_colors");
            }
            if(Geometry.PointAttributes.Contains<Vector4>("point_colors"))
            {
                pointColors = Geometry.PointAttributes.Find<Vector4>("point_colors");
            }

            var polygonNormals   = Geometry.PolygonAttributes.Find<Vector3>("polygon_normals");
            var polygonCentroids = Geometry.PolygonAttributes.Find<Vector3>("polygon_centroids");
            var pointLocations   = Geometry.PointAttributes.Find<Vector3>("point_locations");
            var cornerIndices    = Geometry.CornerAttributes.FindOrCreate<uint>("corner_indices");

            var attributePosition       = vertexFormat.FindAttribute(VertexUsage.Position, 0);
            var attributeNormal         = vertexFormat.FindAttribute(VertexUsage.Normal,   0);   /*  content normals     */
            var attributeNormalFlat     = vertexFormat.FindAttribute(VertexUsage.Normal,   1);   /*  flat normals        */
            var attributeNormalSmooth   = vertexFormat.FindAttribute(VertexUsage.Normal,   2);   /*  smooth normals      */
            var attributeColor          = vertexFormat.FindAttribute(VertexUsage.Color,    0);
            var attributeTexcoord       = vertexFormat.FindAttribute(VertexUsage.TexCoord, 0);
            var attributeIdVec3         = vertexFormat.FindAttribute(VertexUsage.Id,       0);
            var attributeIdUInt         = vertexFormat.FindAttribute(VertexUsage.Id,       0);

            // \note work in progress
            GetMesh = new Mesh();
            IBuffer vertexBuffer = BufferPool.Instance.GetVertexBuffer(vertexFormat, bufferUsageHint);
            GetMesh.VertexBufferRange = vertexBuffer.CreateVertexBufferRange();
            IBuffer indexBuffer = BufferPool.Instance.GetIndexBuffer(DrawElementsType.UnsignedInt, bufferUsageHint);

            #region prepare index buffers
            var polygonFillIndices = GetMesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonFill, 
                indexBuffer,
                BeginMode.Triangles
            );
            var edgeLineIndices = GetMesh.FindOrCreateIndexBufferRange(
                MeshMode.EdgeLines,
                indexBuffer,
                BeginMode.Lines
            );
            /*BufferRange silhouetteLineIndices = GetMesh.FindOrCreateIndexBuffer(
                MeshMode.EdgeLines,
                indexBuffer,
                BeginMode.Lines
            );*/
            var cornerPointIndices = GetMesh.FindOrCreateIndexBufferRange(
                MeshMode.CornerPoints, 
                indexBuffer, 
                BeginMode.Points
            );
            var polygonCentroidIndices = GetMesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonCentroids,
                indexBuffer,
                BeginMode.Points
            );
            #endregion prepare index buffers

            var vertexWriter                = new VertexBufferWriter(GetMesh.VertexBufferRange);
            var polygonFillIndexWriter      = new IndexBufferWriter(polygonFillIndices);
            var edgeLineIndexWriter         = new IndexBufferWriter(edgeLineIndices);
            var cornerPointIndexWriter      = new IndexBufferWriter(cornerPointIndices);
            var polygonCentroidIndexWriter  = new IndexBufferWriter(polygonCentroidIndices);

            vertexWriter.BeginEdit();
            polygonFillIndexWriter.BeginEdit();
            edgeLineIndexWriter.BeginEdit();
            cornerPointIndexWriter.BeginEdit();
            polygonCentroidIndexWriter.BeginEdit();

            UInt32 polygonIndex = 0;

            #region polygons
            cornerIndices.Clear();
            foreach(Polygon polygon in Geometry.Polygons)
            {
                if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs)
                {
                    polygonIdsUInt32[polygon] = polygonIndex;
                }
                polygonIdsVector3[polygon] = Vector3.Vector3FromUint(polygonIndex);

                Vector3 polygonNormal = Vector3.UnitY;

                if(polygon.Corners.Count > 2 && polygonNormals != null && polygonNormals.ContainsKey(polygon))
                {
                    polygonNormal = polygonNormals[polygon];
                }

                uint firstIndex    = vertexWriter.CurrentIndex;
                uint previousIndex = firstIndex;

                #region corners
                foreach(Corner corner in polygon.Corners)
                {
                    //  Position
                    vertexWriter.Position(pointLocations[corner.Point]);

                    //  Normal
                    Vector3 normal = Vector3.UnitY;
                    if(
                        (cornerNormals != null) &&
                        (polygon.Corners.Count > 2) &&
                        (cornerNormals.ContainsKey(corner) == true)
                    )
                    {
                        normal = cornerNormals[corner];
                    }
                    else if(pointNormals != null && pointNormals.ContainsKey(corner.Point))
                    {
                        normal = pointNormals[corner.Point];
                    }
                    else if(pointNormalsSmooth != null && pointNormalsSmooth.ContainsKey(corner.Point))
                    {
                        normal = pointNormalsSmooth[corner.Point];
                    }
                    Vector3 pointNormal = Vector3.UnitY;
                    if(pointNormals != null && pointNormals.ContainsKey(corner.Point))
                    {
                        pointNormal = pointNormals[corner.Point];
                    }
                    else if(pointNormalsSmooth != null && pointNormalsSmooth.ContainsKey(corner.Point))
                    {
                        pointNormal = pointNormalsSmooth[corner.Point];
                    }

                    switch(normalStyle)
                    {
                        case NormalStyle.CornerNormals:     vertexWriter.Normal(normal); break;
                        case NormalStyle.PointNormals:      vertexWriter.Normal(pointNormal); break;
                        case NormalStyle.PolygonNormals:    vertexWriter.Normal(polygonNormal); break;
                    }

                    if(attributeNormalFlat != null)     vertexWriter.Set(attributeNormalFlat, polygonNormal);
                    if(attributeNormalSmooth != null)   vertexWriter.Set(attributeNormalSmooth, pointNormalsSmooth[corner.Point]);

                    //  Texcoord
                    if(attributeTexcoord != null)
                    {
                        if(
                            (cornerTexcoords != null) &&
                            (cornerTexcoords.ContainsKey(corner) == true)
                        )
                        {
                            vertexWriter.Set(attributeTexcoord, cornerTexcoords[corner]);
                        }
                        else if(
                            (pointTexcoords != null) &&
                            (pointTexcoords.ContainsKey(corner.Point) == true)
                        )
                        {
                            vertexWriter.Set(attributeTexcoord, pointTexcoords[corner.Point]);
                        }
                    }

                    //  Vertex Color
                    if(attributeColor != null)
                    {
                        if(
                            (cornerColors != null) &&
                            (cornerColors.ContainsKey(corner) == true)
                        )
                        {
                            vertexWriter.Set(attributeColor, cornerColors[corner]);
                        }
                        else if(
                            (pointColors != null) &&
                            (pointColors.ContainsKey(corner.Point) == true)
                        )
                        {
                            vertexWriter.Set(attributeColor, pointColors[corner.Point]);
                        }
                        else
                        {
                            vertexWriter.Set(attributeColor, Vector4.One);
                        }
                    }

                    //  PolygonId
                    if(RenderStack.Graphics.Configuration.useIntegerPolygonIDs && (attributeIdUInt != null))
                    {
                        vertexWriter.Set(attributeIdUInt, polygonIndex);
                    }
                    if(attributeIdVec3 != null)
                    {
                        Vector3 v = Vector3.Vector3FromUint(polygonIndex);
                        vertexWriter.Set(attributeIdVec3, v);
                    }

                    cornerPointIndexWriter.Point(vertexWriter.CurrentIndex);
                    cornerPointIndexWriter.CurrentIndex++;

                    cornerIndices[corner] = vertexWriter.CurrentIndex;

                    if(previousIndex != firstIndex)
                    {
                        polygonFillIndexWriter.Triangle(firstIndex, vertexWriter.CurrentIndex, previousIndex);
                        polygonFillIndexWriter.CurrentIndex += 3;
                    }

                    previousIndex = vertexWriter.CurrentIndex;

                    ++vertexWriter.CurrentIndex;
                }
                #endregion corners

                ++polygonIndex;
            }
            #endregion polygons

            #region edges
            Geometry.BuildEdges();

            foreach(Edge edge in Geometry.Edges.Keys)
            {
                if(
                    cornerIndices.ContainsKey(edge.A.Corners[0]) && 
                    cornerIndices.ContainsKey(edge.B.Corners[0])
                )
                {
                    uint i0 = cornerIndices[edge.A.Corners[0]];
                    uint i1 = cornerIndices[edge.B.Corners[0]];
                    edgeLineIndexWriter.Line(i0, i1);
                    edgeLineIndexWriter.CurrentIndex += 2;
                }
            }
            #endregion edges

            #region polygon centroids
            foreach(Polygon polygon in Geometry.Polygons)
            {
                Vector3 normal;

                if(polygon.Corners.Count > 2)
                {
                    normal = polygonNormals[polygon];
                }
                else
                {
                    normal = new Vector3(0.0f, 1.0f, 0.0f);
                }

                vertexWriter.Position(polygonCentroids[polygon]);
                if(attributeNormal != null)     vertexWriter.Set(attributeNormal, normal);
                if(attributeNormalFlat != null) vertexWriter.Set(attributeNormalFlat, normal);
                polygonCentroidIndexWriter.Point(vertexWriter.CurrentIndex);
                ++vertexWriter.CurrentIndex;
                ++polygonCentroidIndexWriter.CurrentIndex;
            }
            #endregion polygon centroids

            vertexWriter.EndEdit();
            polygonFillIndexWriter.EndEdit();
            edgeLineIndexWriter.EndEdit();
            cornerPointIndexWriter.EndEdit();
            polygonCentroidIndexWriter.EndEdit();
        }
    }
}
