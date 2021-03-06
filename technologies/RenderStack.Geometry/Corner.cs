﻿using System;
using System.Collections.Generic;

using RenderStack.Math;

namespace RenderStack.Geometry
{
    [Serializable]
    /// \brief A connection between Point and Polygon.
    /// \note Corner stores only connectivity information.
    /// All corner attributes such as texture coordinates
    /// are stored externally in attribute maps such as
    /// Dictionary<Corner, Vector2>.
    /// \note Mostly stable.
    public class Corner : IDisposable
    {
        public Point   Point    { get; private set; }
        public Polygon Polygon  { get; private set; }

        public Corner(Point point, Polygon polygon)
        {
            Point   = point;
            Polygon = polygon;
        }

        /// \brief Computes smoothed attribute value for corner
        /// \param cornerAttribute          Attribute map where to store smoothed attribute value
        /// \param polygonAttribute         Attribute map where to retrieve polygon attribute values from
        /// \param polygonNormals           Attribute map where to retrieve polygon normals from
        /// \param cosMaxSmoothingAngle     Cosine of maximum smoothing angle. Edges sharper than this are not smoothed
        public void SmoothNormalize(
            Dictionary<Corner,  Vector3>    cornerAttribute,
            Dictionary<Polygon, Vector3>    polygonAttribute,
            Dictionary<Polygon, Vector3>    polygonNormals,
            float                           cosMaxSmoothingAngle
        )
        {
            if(polygonNormals.ContainsKey(Polygon) == false)
            {
                return;
            }
            Vector3 polygonNormal = polygonNormals[this.Polygon];
            Vector3 polygonValue = polygonAttribute[this.Polygon];
            Vector3 cornerValue = polygonValue;

            int pointCorners = 0;
            int participants = 0;
            foreach(var pointCorner in Point.Corners)
            {
                ++pointCorners;
                var neighborPolygon = pointCorner.Polygon;
                if(
                    (Polygon != neighborPolygon) &&
                    (polygonNormals.ContainsKey(neighborPolygon) == true) &&
                    (polygonAttribute.ContainsKey(neighborPolygon) == true) &&
                    (neighborPolygon.Corners.Count > 2)
                )
                {
                    Vector3 neighborNormal = polygonNormals[neighborPolygon];
                    //float pLen = polygonNormal.Length;
                    //float nLen = neighborNormal.Length;
                    float cosAngle = Vector3.Dot(
                        polygonNormal, 
                        neighborNormal
                    );
                    if(cosAngle > 1.0f)
                    {
                        cosAngle = 1.0f;
                    }
                    if(cosAngle < -1.0f)
                    {
                        cosAngle = -1.0f;
                    }
                    //  Smaller cosine means larger angle means less sharp
                    //  Higher cosine means lesser angle means more sharp
                    //  Cosine == 1 == maximum sharpness
                    //  Cosine == -1 == minimum sharpness (flat)
                    if(cosAngle <= cosMaxSmoothingAngle)
                    {
                        cornerValue += polygonAttribute[neighborPolygon];
                        ++participants;
                    }
                }
            }

            cornerValue = Vector3.Normalize(cornerValue);
            cornerAttribute[this] = cornerValue;
        }
        public void SmoothAverage(
            Dictionary<Corner,  Vector4>    newCornerAttribute,
            Dictionary<Corner,  Vector4>    oldCornerAttribute,
            Dictionary<Corner,  Vector3>    cornerNormals,
            Dictionary<Point,   Vector3>    pointNormals
        )
        {
            bool hasCornerNormal = cornerNormals.ContainsKey(this);
            if(
                (hasCornerNormal == false) &&
                (pointNormals.ContainsKey(this.Point) == false)
            )
            {
                return;
            }
            Vector3 cornerNormal = hasCornerNormal ? cornerNormals[this] : pointNormals[this.Point];
            Vector4 cornerValue = Vector4.Zero;

            int pointCorners = 0;
            int participants = 0;
            foreach(Corner pointCorner in Point.Corners)
            {
                ++pointCorners;
                hasCornerNormal = cornerNormals.ContainsKey(pointCorner);
                if(
                    (hasCornerNormal == false) ||
                    (cornerNormals[pointCorner] == cornerNormal)
                )
                {
                    if(oldCornerAttribute.ContainsKey(pointCorner))
                    {
                        cornerValue += oldCornerAttribute[pointCorner];
                        ++participants;
                    }
                }
            }

            cornerValue = cornerValue / (float)(participants);
            newCornerAttribute[this] = cornerValue;
        }

        public void Dispose()
        {
            Point = null;
            Polygon = null;
        }
    }
}
