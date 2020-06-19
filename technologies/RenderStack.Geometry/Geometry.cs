//#define DEBUG_CHECK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RenderStack.Math;

namespace RenderStack.Geometry
{
    [Serializable]
    /// \brief Maintains connectivity and attribute information for polygon based meshes.
    /// 
    /// Connectivity is stored between Points, Corners and Polygons.
    /// Optionally Edge connectivity information can be constructed when needed.
    /// It is also possibly to construct Vertex and Index buffers suitable for
    /// use with Graphics APIs such as OpenGL.
    /// 
    /// Points, Corners, Polygons and Edges only store connectivity information.
    /// Point locations, corner normals, point and corner texture coordinates and so
    /// on are stored in attribute maps such as Dictionary<Corner, Vector2> for corner
    /// texture coordinates. These attribute maps are named, and they are hosted in
    /// Geometry.
    /// 
    /// Following standard attribute map names are used:
    /// "point_locations",
    /// "corner_normals",
    /// "polygon_normals",
    /// "polygon_centroids",
    /// ...
    /// 
    /// \noet Mostly stable.
    public class Geometry
    {
        private AttributeMapCollection<Point>       pointAttributes   = new AttributeMapCollection<Point>();
        private AttributeMapCollection<Corner>      cornerAttributes  = new AttributeMapCollection<Corner>();
        private AttributeMapCollection<Polygon>     polygonAttributes = new AttributeMapCollection<Polygon>();
        private AttributeMapCollection<Edge>        edgeAttributes    = new AttributeMapCollection<Edge>();

        public AttributeMapCollection<Point>        PointAttributes   { get { return pointAttributes; } }
        public AttributeMapCollection<Corner>       CornerAttributes  { get { return cornerAttributes; } }
        public AttributeMapCollection<Polygon>      PolygonAttributes { get { return polygonAttributes; } }
        public AttributeMapCollection<Edge>         EdgeAttributes    { get { return edgeAttributes; } }

        private List<Point>                         points      = new List<Point>();
        private List<Polygon>                       polygons    = new List<Polygon>();

        private Dictionary<Edge, HashSet<Polygon>>  edges       = new Dictionary<Edge, HashSet<Polygon>>();

        public List<Point>                          Points      { get { return points; } }
        public List<Polygon>                        Polygons    { get { return polygons; } }

        /// \brief Collection of Edges and the Polygons that share each edge.
        public Dictionary<Edge, HashSet<Polygon>> Edges { get { return edges; } }

        public Geometry()
        {
        }

        public static Geometry Sqrt3(Geometry src)
        {
            CloneGeometryOperation clone = new CloneGeometryOperation(src, null);

            return clone.Destination;
        }


        public static CloneGeometryOperation Clone(Geometry src, HashSet<uint> selectedPolygonIndices)
        {
            return new CloneGeometryOperation(src, selectedPolygonIndices);
        }

        /// \warning This is inefficient! Try not to use this
        public void MergePoints()
        {
            Dictionary<Point,Point> removedPointToRemainingPoint = new Dictionary<Point,Point>();
            var pointLocations = PointAttributes.FindOrNull<Vector3>("point_locations");
            float pointEpsilon = 0.0000001f;

            //  Merge points 
            foreach(Point p1 in Points)
            {
                Vector3 pos1 = pointLocations[p1];

                bool alreadyUsed = false;
                foreach(Point p2 in Points)
                {
                    if(p1 == p2)
                    {
                        continue;
                    }
                    if(removedPointToRemainingPoint.ContainsKey(p1))
                    {
                        continue;
                    }
                    Vector3 pos2 = pointLocations[p2];

                    float distanceSquared = pos1.DistanceSquared(pos2);

                    if(distanceSquared < pointEpsilon)
                    {
                        if(alreadyUsed)
                        {
                            System.Diagnostics.Debug.WriteLine("point already used");
                        }
                        removedPointToRemainingPoint[p2] = p1;
                        alreadyUsed = true;
                    }
                }
            }

            //  Merge polygons
            foreach(var polygon in Polygons)
            {
                polygon.ReplacePoints(removedPointToRemainingPoint);
            }

            foreach(var point in removedPointToRemainingPoint.Keys)
            {
                Points.Remove(point);
            }
        }

        /// \todo Accelerate by sorting along major axis
        public void Merge(Geometry other)
        {
            Dictionary<Point,Point> otherPointsToNewPoints = new Dictionary<Point,Point>();

            var pointLocationsSelf  = PointAttributes.FindOrNull<Vector3>("point_locations");
            var pointLocationsOther = other.PointAttributes.FindOrNull<Vector3>("point_locations");
            float pointEpsilon = 0.0001f;

            //  Merge points 
            foreach(Point otherPoint in other.Points)
            {
                Vector3 otherPosition = pointLocationsOther[otherPoint];

                bool existingPointCloseEnough = false;
                foreach(Point existingPoint in Points)
                {
                    Vector3 existingPosition = pointLocationsSelf[existingPoint];

                    float distanceSquared = existingPosition.DistanceSquared(otherPosition);

                    if(distanceSquared < pointEpsilon)
                    {
                        existingPointCloseEnough = true;
                        otherPointsToNewPoints[otherPoint] = existingPoint;
                        break;
                    }
                }
                if(existingPointCloseEnough == false)
                {
                    otherPointsToNewPoints[otherPoint] = MakePoint(otherPosition);
                }
            }

            //  Merge polygons
            foreach(Polygon otherPolygon in other.Polygons)
            {
                //  Test if we can find at least single matching polygon
                Polygon matchingPolygon = null;
                foreach(Polygon existingPolygon in Polygons)
                {
                    //  Test that all other corners can be found
                    bool missingCorners = false;
                    foreach(Corner otherCorner in otherPolygon.Corners)
                    {
                        Point newPoint = otherPointsToNewPoints[otherCorner.Point];
                        bool cornerFound = false;
                        //  Find matching corner
                        foreach(Corner existingCorner in existingPolygon.Corners)
                        {
                            if(newPoint == existingCorner.Point)
                            {
                                cornerFound = true;
                                break;
                            }
                        }
                        //  No matching corner found, continue
                        if(cornerFound == false)
                        {
                            missingCorners = true;
                            break;
                        }
                    }
                    if(missingCorners == true)
                    {
                        continue;
                    }
                    //  No missing corners - polygons match
                    matchingPolygon = existingPolygon;
                    break;
                }

                if(matchingPolygon != null)
                {
                    //  There was a matching polygon, delete it and do not create new from other
                    RemovePolygon(matchingPolygon);
                    //Polygons.Remove(matchingPolygon);
                }
                else
                {
                    //  There was no match, add new from other
                    Polygon newPolygon = MakePolygon();
                    foreach(Corner otherCorner in otherPolygon.Corners)
                    {
                        Point otherPoint = otherCorner.Point;
                        Point newPoint   = otherPointsToNewPoints[otherPoint];
                        newPolygon.MakeCorner(newPoint);
                    }
                }
            }

            //  Finally, remove unused points
            HashSet<Point> usedPoints = new HashSet<Point>();
            foreach(Polygon polygon in Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    usedPoints.Add(corner.Point);
                }
            }
            List<Point> pointsToRemove = new List<Point>();
            foreach(Point point in Points)
            {
                if(usedPoints.Contains(point) == false)
                {
                    pointsToRemove.Add(point);
                }
            }
            foreach(Point pointToRemove in pointsToRemove)
            {
                Points.Remove(pointToRemove);
            }
        }
        public void MergeFast(Geometry other, Polygon thisPolygon, Polygon otherPolygon)
        {
            Dictionary<Point,Point> otherPointsToNewPoints = new Dictionary<Point,Point>();

            var pointLocationsSelf  = PointAttributes.FindOrNull<Vector3>("point_locations");
            var pointLocationsOther = other.PointAttributes.FindOrNull<Vector3>("point_locations");
            float pointEpsilon = 0.0001f;

            //  Merge points 
            foreach(Point otherPoint in other.Points)
            {
                Vector3 otherPosition = pointLocationsOther[otherPoint];

                bool existingPointCloseEnough = false;
                foreach(Corner existingCorner in thisPolygon.Corners)
                {
                    Point existingPoint = existingCorner.Point;
                    Vector3 existingPosition = pointLocationsSelf[existingPoint];

                    float distanceSquared = existingPosition.DistanceSquared(otherPosition);

                    if(distanceSquared < pointEpsilon)
                    {
                        existingPointCloseEnough = true;
                        otherPointsToNewPoints[otherPoint] = existingPoint;
                        break;
                    }
                }
                if(existingPointCloseEnough == false)
                {
                    otherPointsToNewPoints[otherPoint] = MakePoint(otherPosition);
                }
            }

            RemovePolygon(thisPolygon);

            //  Merge polygons
            foreach(Polygon otherPolygon2 in other.Polygons)
            {
                if(otherPolygon != otherPolygon2)
                {
                    Polygon newPolygon = MakePolygon();
                    foreach(Corner otherCorner in otherPolygon.Corners)
                    {
                        Point otherPoint = otherCorner.Point;
                        Point newPoint   = otherPointsToNewPoints[otherPoint];
                        newPolygon.MakeCorner(newPoint);
                    }
                }
            }

            //  Finally, remove unused points
            HashSet<Point> usedPoints = new HashSet<Point>();
            foreach(Polygon polygon in Polygons)
            {
                foreach(Corner corner in polygon.Corners)
                {
                    usedPoints.Add(corner.Point);
                }
            }
            List<Point> pointsToRemove = new List<Point>();
            foreach(Point point in Points)
            {
                if(usedPoints.Contains(point) == false)
                {
                    pointsToRemove.Add(point);
                }
            }
            foreach(Point pointToRemove in pointsToRemove)
            {
                Points.Remove(pointToRemove);
            }
        }

        void RemoveCorner(Corner corner)
        {
            corner.Point.Corners.Remove(corner);
            if(corner.Point.Corners.Count == 0)
            {
                Points.Remove(corner.Point);
                //  Point has nothing else, no need to Dispose()
            }
            corner.Polygon.RemoveCorner(corner);
            if(corner.Polygon.Corners.Count == 0)
            {
                Polygons.Remove(corner.Polygon);
                corner.Polygon.Dispose();
            }
            corner.Dispose();
        }
        public void RemovePolygon(Polygon polygon)
        {
            foreach(Corner corner in new List<Corner>(polygon.Corners))
            {
                RemoveCorner(corner);
            }
            if(Polygons.Contains(polygon))
            {
                //  This should not happen, removing last corner should remove polygon
                System.Diagnostics.Trace.TraceError("polygon should have been removed");
            }
        }

        public void Transform(Matrix4 transform)
        {
            Matrix4 inverseTransposeTransform = Matrix4.Transpose(Matrix4.Invert(transform));

            //  Check.. Did I forget something?
            //  \todo Mark each attributemap how they should be transformed
            var polygonCentroids    = PolygonAttributes.FindOrNull<Vector3>("polygon_centroids");
            var polygonNormals      = PolygonAttributes.FindOrNull<Vector3>("polygon_normals");
            var pointLocations      = PointAttributes.FindOrNull<Vector3>("point_locations");
            var pointNormals        = PointAttributes.FindOrNull<Vector3>("point_normals");
            var cornerNormals       = CornerAttributes.FindOrNull<Vector3>("corner_normals");

            //  Make copies of old points
            foreach(Point point in Points)
            {
                if(pointLocations != null && pointLocations.ContainsKey(point))  pointLocations[point]   = transform.TransformPoint(pointLocations[point]);
                if(pointNormals != null   && pointNormals.ContainsKey(point))    pointNormals[point]     = inverseTransposeTransform.TransformDirection(pointNormals[point]);
            }
            foreach(Polygon polygon in Polygons)
            {
                if(polygonCentroids != null && polygonCentroids.ContainsKey(polygon))    polygonCentroids[polygon] = transform.TransformPoint(polygonCentroids[polygon]);
                if(polygonNormals != null   && polygonNormals.ContainsKey(polygon))      polygonNormals[polygon]   = inverseTransposeTransform.TransformDirection(polygonNormals[polygon]);
                if(cornerNormals != null)
                {
                    foreach(Corner corner in polygon.Corners)
                    {
                        if(cornerNormals.ContainsKey(corner)) cornerNormals[corner] = inverseTransposeTransform.TransformDirection(cornerNormals[corner]);
                    }
                }
            }
        }

        public void Noise(NoiseGenerator noise)
        {
            ComputePolygonNormals();
            ComputePointNormals("point_normals_smooth");

            var pointLocations  = PointAttributes.FindOrNull<Vector3>("point_locations");
            var pointNormals    = PointAttributes.FindOrNull<Vector3>("point_normals_smooth");

            foreach(Point point in Points)
            {
                if(
                    (pointLocations != null) && 
                    pointLocations.ContainsKey(point)
                )
                {
                    var pos = pointLocations[point];
                    var normal = Vector3.Normalize(pointLocations[point]);
                    pointLocations[point] = pos + noise.Generate(pos) * normal;
                }
            }

            ComputePolygonCentroids();
            ComputePolygonNormals();
            SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)System.Math.PI));
            BuildEdges();

        }
        public void Geodesate(float radius)
        {
            float rInv = 1.0f / radius;
            var polygonCentroids    = PolygonAttributes.FindOrNull<Vector3>("polygon_centroids");
            var polygonNormals      = PolygonAttributes.FindOrNull<Vector3>("polygon_normals");
            var pointLocations      = PointAttributes.FindOrNull<Vector3>("point_locations");
            var pointNormals        = PointAttributes.FindOrNull<Vector3>("point_normals");
            var cornerNormals       = CornerAttributes.FindOrNull<Vector3>("corner_normals");

            //  Make copies of old points
            foreach(Point point in Points)
            {
                if(
                    (pointLocations != null) && 
                    pointLocations.ContainsKey(point)
                )
                {
                    Vector3 newNormal = Vector3.Normalize(pointLocations[point]);
                    pointLocations[point] = radius * newNormal;
                    if(
                        (pointNormals != null) && 
                        pointNormals.ContainsKey(point)
                    )
                    {
                        pointNormals[point] = newNormal;
                    }
                }
            }
            foreach(Polygon polygon in Polygons)
            {
                if(
                    (polygonCentroids != null) && 
                    polygonCentroids.ContainsKey(polygon)
                )
                {
                    Vector3 newNormalizedCentroid = Vector3.Normalize(polygonCentroids[polygon]);
                    polygonCentroids[polygon] = radius * newNormalizedCentroid;
                    if(
                        (polygonNormals != null) &&
                        polygonNormals.ContainsKey(polygon)
                    )
                    {
                        polygonNormals[polygon]  = newNormalizedCentroid;
                    }
                }
                if(cornerNormals != null)
                {
                    foreach(Corner corner in polygon.Corners)
                    {
                        if(
                            cornerNormals.ContainsKey(corner)
                        )
                        {
                            if(pointNormals.ContainsKey(corner.Point))
                            {
                                cornerNormals[corner] = pointNormals[corner.Point];
                            }
                            else if(pointLocations.ContainsKey(corner.Point))
                            {
                                cornerNormals[corner] = pointLocations[corner.Point] * rInv;
                            }
                        }
                    }
                }
            }
        }

        public Geometry Clone()
        {
            return Clone(this, null).Destination;
        }

        public Corner ClosestPolygonCorner(Polygon polygon, Vector3 position)
        {
            Corner  closestCorner = null;
            float   closestDistanceSquared = float.MaxValue;
            var     pointLocations = PointAttributes.Find<Vector3>("point_locations");

            foreach(Corner corner in polygon.Corners)
            {
                Vector3 cornerLocation = pointLocations[corner.Point];
                float distanceSquared = cornerLocation.DistanceSquared(position);
                if(distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestCorner = corner;
                }
            }

            return closestCorner;
        }
        public void PolygonCornerEdges(Polygon polygon, Corner pivot, out Edge edgeOne, out Edge edgeTwo)
        {
            edgeOne = new Edge(polygon.Corners.Last().Point, polygon.Corners.First().Point);
            edgeTwo = new Edge(polygon.Corners.First().Point, polygon.Corners.Last().Point);
            Corner prevCorner = polygon.Corners.Last();
            foreach(Corner corner in polygon.Corners)
            {
                if(corner == pivot)
                {
                    edgeOne = new Edge(prevCorner.Point, corner.Point);
                }
                if(prevCorner == pivot)
                {
                    edgeTwo = new Edge(prevCorner.Point, corner.Point);
                }
                prevCorner = corner;
            }
            Corner firstCorner = polygon.Corners.First();
            if(firstCorner == pivot)
            {
                edgeOne = new Edge(prevCorner.Point, firstCorner.Point);
            }
            if(prevCorner == pivot)
            {
                edgeTwo = new Edge(prevCorner.Point, firstCorner.Point);
            }
        }

        public Geometry CloneSelectedPolygons(HashSet<uint> selectedPolygonIndices)
        {
            return Clone(this, selectedPolygonIndices).Destination;
        }

        /// \brief Construct a new Point to Geometry
        /// \return Newly created Point without any connections
        public Point MakePoint()
        {
            Point point = new Point();
            Points.Add(point);
            return point;
        }
        /// \brief Constructs a new Polygon.
        /// \return Newly created Polygon without any connections.
        public Polygon MakePolygon()
        {
            Polygon polygon = new Polygon();
            Polygons.Add(polygon);
            return polygon;
        }

        /// Compute polygon normals based on point position attributes.
        /// Results are stored in internally stored attribute map named "polygon_normals".
        public void ComputePolygonNormals()
        {
            var polygonNormals = PolygonAttributes.FindOrCreate<Vector3>("polygon_normals");
            var pointLocations = PointAttributes.Find<Vector3>("point_locations");

            polygonNormals.Clear();
            foreach(Polygon polygon in Polygons)
            {
                polygon.ComputeNormal(polygonNormals, pointLocations);
            }
        }
        public Sphere ComputeBoundingSphere()
        {
#if false
            return ComputeBoundingSphereNaive();
#else
            var s1 = ComputeBoundingSphereNaive();
            var s2 = ComputeBoundingSphereRitter();
            return (s1.Radius < s2.Radius) ? s1 : s2;
#endif
        }
        public Sphere ComputeBoundingSphereNaive()
        {
            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");

            //  FIRST PASS: Find 6 minima/maxima points
            BoundingBox box = new BoundingBox();
            box.Clear();
            foreach(Point point in Points)
            {
                box.ExtendBy(pointPositions[point]);
            }

            float maxRadiusSquare = 0.0f;
            foreach(Point point in Points)
            {
                Vector3 p = pointPositions[point];
                float radiusSquare = box.Center.DistanceSquared(pointPositions[point]);
                if(radiusSquare > maxRadiusSquare)
                {
                    maxRadiusSquare = radiusSquare;
                }
            }
            return new Sphere(box.Center, (float)System.Math.Sqrt(maxRadiusSquare));
        }
        public Sphere ComputeBoundingSphereRitter()
        {
            //  An Efficient Bounding Sphere                
            //  by Jack Ritter                              
            //  from "Graphics Gems", Academic Press, 1990  

            //  Routine to calculate near-optimal bounding
            //  sphere over a set of points in 3D
            //  Code written by Jack Ritter and Lyle Rains.

            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");

            //  FIRST PASS: Find 6 minima/maxima points
            Vector3 xmin, xmax, ymin, ymax, zmin, zmax;
            xmin = ymin = zmin = Vector3.MaxValue;
            xmax = ymax = zmax = Vector3.MinValue;
            foreach(Point point in Points)
            {
                Vector3 p = pointPositions[point];
                if(p.X < xmin.X) xmin = p; 
                if(p.X > xmax.X) xmax = p;
                if(p.Y < ymin.Y) ymin = p;
                if(p.Y > ymax.Y) ymax = p;
                if(p.Z < zmin.Z) zmin = p;
                if(p.Z > zmax.Z) zmax = p;
            }

            //  Set span.X = distance between the 2 points xmin & xmax (squared)
            Vector3 span;
            Vector3 d;
            d.X = xmax.X - xmin.X;
            d.Y = xmax.Y - xmin.Y;
            d.Z = xmax.Z - xmin.Z;
            span.X = d.LengthSquared;

            //  Same for Y & Z spans
            d.X = ymax.X - ymin.X;
            d.Y = ymax.Y - ymin.Y;
            d.Z = ymax.Z - ymin.Z;
            span.Y = d.LengthSquared;

            d.X = zmax.X - zmin.X;
            d.Y = zmax.Y - zmin.Y;
            d.Z = zmax.Z - zmin.Z;
            span.Z = d.LengthSquared;

            //  Set points dia1 & dia2 to the maximally separated pair
            Vector3 dia1 = xmin;
            Vector3 dia2 = xmax;  //  Assume xspan biggest
            float   maxspan = span.X;
            if(span.Y > maxspan)
            {
                maxspan = span.Y;
                dia1 = ymin; 
                dia2 = ymax;
            }
            if(span.Z > maxspan)
            {
                dia1 = zmin; 
                dia2 = zmax;
            }

            //  dia1, dia2 is a diameter of initial sphere
            //  Calculate initial center
            Vector3 cen = (dia1 + dia2) / 2.0f;

            //  Calculate initial radius^2 and radius
            d = dia2 - cen;  //  Radius vector
            float rad_sq = d.LengthSquared;;
            float rad = d.LengthSquared;

            //  SECOND PASS: Increment current sphere
            foreach(Point point in Points)
            {
                Vector3 p = pointPositions[point];
                d = p - cen;
                float old_to_p_sq = d.LengthSquared;

                //  Do r^2 test first
                if(old_to_p_sq > rad_sq)
                {
                    //  This point is outside of current sphere
                    float old_to_p = (float)System.Math.Sqrt(old_to_p_sq);

                    //  Calc radius of new sphere
                    rad = (rad + old_to_p) / 2.0f;

                    //  For next r^2 compare
                    rad_sq = rad * rad;
                    float old_to_new = old_to_p - rad;

                    //  Calculate center of new sphere
                    cen = (rad * cen + old_to_new * p) / old_to_p;

                    //  Suppress if desired
#if false
                    System.Diagnostics.Debug.WriteLine(
                        " New sphere: cen,rad = "
                        + cen.ToString() + " " + rad
                    );
#endif
                }
            }

            //  THIRD PASS: Adjust the sphere along major axis for potential improvements
            Sphere s = new Sphere(cen, rad);
            AdjustBoundingSphere(ref s, -Vector3.UnitX);
            AdjustBoundingSphere(ref s,  Vector3.UnitX);
            AdjustBoundingSphere(ref s, -Vector3.UnitY);
            AdjustBoundingSphere(ref s,  Vector3.UnitY);
            AdjustBoundingSphere(ref s, -Vector3.UnitZ);
            AdjustBoundingSphere(ref s,  Vector3.UnitZ);

            return s;
        }

        private void AdjustBoundingSphere(ref Sphere boundingSphere, Vector3 direction)
        {
            float delta = 0.1f;
            Vector3 lastCenter = boundingSphere.Center;
            Vector3 tryCenter;
            float radius = boundingSphere.Radius;
            while(delta > 0.0001f)
            {
                //  Try to add delta * direction
                tryCenter = lastCenter + delta * direction;
                if(AllInSphere(tryCenter, ref radius))
                {
                     lastCenter = tryCenter;
                }
                else
                {
                    delta *= 0.5f;
                }
            }
            boundingSphere.Center = lastCenter;
            boundingSphere.Radius = radius;
        }

        private bool AllInSphere(Vector3 center, ref float radius)
        {
            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");
            float r2 = radius * radius;
            float maxDistanceSquared = 0.0f;
            foreach(Point point in Points)
            {
                float distanceSquared = center.DistanceSquared(pointPositions[point]);
                if(distanceSquared > r2)
                {
                    return false;
                }
                if(distanceSquared > maxDistanceSquared)
                {
                    maxDistanceSquared = distanceSquared;
                }
            }
            radius = (float)System.Math.Sqrt(maxDistanceSquared);
            return true;
        }

        /// Compute polygon centroids based on point position attributes
        /// Results are stored in internally stored attribute map named "polygon_centroids".
        public void ComputePolygonCentroids()
        {
            var polygonCentroids = PolygonAttributes.FindOrCreate<Vector3>("polygon_centroids");
            var pointLocations   = PointAttributes.Find<Vector3>("point_locations");

            polygonCentroids.Clear();
            foreach(Polygon polygon in Polygons)
            {
                polygon.ComputeCentroid(polygonCentroids, pointLocations);
            }
        }
        public Vector3 ComputePointNormal(Point point)
        {
            var polygonNormals = PolygonAttributes.Find<Vector3>("polygon_normals");

            Vector3 normalSum = new Vector3(0.0f, 0.0f, 0.0f);
            foreach(Corner corner in point.Corners)
            {
                normalSum += polygonNormals[corner.Polygon];
            }
            Vector3 averageNormal = normalSum / (float)point.Corners.Count;
            return averageNormal;
        }
        public void ComputePointNormals(string mapName)
        {
            var pointNormals    = PointAttributes.FindOrCreate<Vector3>(mapName);
            var polygonNormals  = PolygonAttributes.Find<Vector3>("polygon_normals");

            pointNormals.Clear();
            foreach(Point point in Points)
            {
                Vector3 normalSum = new Vector3(0.0f, 0.0f, 0.0f);
                foreach(Corner corner in point.Corners)
                {
                    normalSum += polygonNormals[corner.Polygon];
                }
                Vector3 averageNormal = normalSum / (float)point.Corners.Count;
                pointNormals[point] = averageNormal;
            }
        }
        /// Compute corner normals based on polygon normal and point location attributes
        /// Results are stored in internally stored attribute map named "corner_normals".
        public void SmoothNormalize(
            string cornerAttribute,
            string polygonAttribute,
            float maxSmoothingAngleRadians
        )
        {
            var cornerAttributes  = CornerAttributes.FindOrCreate<Vector3>(cornerAttribute/*"corner_normals"*/);
            var polygonAttributes = PolygonAttributes.Find<Vector3>(polygonAttribute/*"polygon_normals"*/);
            var polygonNormals = PolygonAttributes.Find<Vector3>("polygon_normals");

            float cosMaxSmoothingAngle = (float)System.Math.Cos((double)maxSmoothingAngleRadians);

            cornerAttributes.Clear();
            foreach(Polygon polygon in Polygons)
            {
                if(maxSmoothingAngleRadians == 0)
                {
                    polygon.CopyToCorners(
                        cornerAttributes, 
                        polygonAttributes
                    );
                }
                else
                {
                    polygon.SmoothNormalize(
                        cornerAttributes,
                        polygonAttributes,
                        polygonNormals,
                        cosMaxSmoothingAngle
                    );
                }
            }
        }
        public void SmoothAverage(
            string cornerAttribute,
            string pointNormalName
        )
        {
            var cornerAttributes = CornerAttributes.FindOrCreate<Vector4>(cornerAttribute);
            var cornerNormals = CornerAttributes.FindOrCreate<Vector3>("corner_normals");
            var pointNormals = PointAttributes.Find<Vector3>(pointNormalName);

            var newCornerAttributes = CornerAttributes.FindOrCreate<Vector4>("temp");
            foreach(Polygon polygon in Polygons)
            {
                polygon.SmoothAverage(
                    newCornerAttributes,
                    cornerAttributes,
                    cornerNormals,
                    pointNormals
                );
            }
            CornerAttributes.Replace(cornerAttribute, "temp");
        }

        public Matrix4 MakePointTransform = Matrix4.Identity;

        public Point MakePoint(Vector3 p)
        {
            Point point = MakePoint();
            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");
#if ENABLE_CREATE_TRANSFORM
            pointPositions[point] = MakePointTransform * new Vector3(p.x, p.y, p.z);
#else
            pointPositions[point] = new Vector3(p.X, p.Y, p.Z);
#endif
            return point;
        }
        /// \brief Constructs a new Point and assosiates a position attribute for it.
        public Point MakePoint(float x, float y, float z)
        {
            Point point = MakePoint();
            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");
#if ENABLE_CREATE_TRANSFORM
            pointPositions[point] = MakePointTransform * new Vector3(x, y, z);
#else
            pointPositions[point] = new Vector3(x, y, z);
#endif
            return point;
        }
        /// \brief Constructs a new Point and assosiates a position attribute for it.
        public Point MakePoint(double x, double y, double z)
        {
            Point point = MakePoint();
            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");
#if ENABLE_CREATE_TRANSFORM
            pointPositions[point] = MakePointTransform * Vector3((float)x, (float)y, (float)z);
#else
            pointPositions[point] = new Vector3((float)x, (float)y, (float)z);
#endif
            return point;
        }

        public Point MakePoint(double x, double y, double z, double s, double t)
        {
            Point point = MakePoint();
            var pointPositions = PointAttributes.FindOrCreate<Vector3>("point_locations");
            var pointTexCoords = PointAttributes.FindOrCreate<Vector2>("point_texcoords");
#if ENABLE_CREATE_TRANSFORM
            pointPositions[point] = MakePointTransform * Vector3((float)x, (float)y, (float)z);
#else
            pointPositions[point] = new Vector3((float)x, (float)y, (float)z);
#endif
            pointTexCoords[point] = new Vector2((float)s, (float)t);
            return point;
        }

        /// \brief Construct a new Polygon from a given set of Points
        /// \param points oints that are connected to the new Polygon</param>
        /// \return Newly created Polygon connected to given Points
        public Polygon MakePolygon(params Point[] points)
        {
            Polygon polygon = MakePolygon();
            foreach(Point point in points)
            {
                polygon.MakeCorner(point);
            }
            return polygon;
        }
        /// \brief Construct a new Polygon from a given set of Point indices
        /// \param pointIndices Indices to Points that are connected to the new Polygon</param>
        /// \return Newly created Polygon
        public Polygon MakePolygon(params int[] pointIndices)
        {
            Polygon polygon = MakePolygon();
            foreach(int pointIndex in pointIndices)
            {
                Point point = Points[pointIndex];
                polygon.MakeCorner(point);
            }
#if DEBUG_CHECK
            var pointLocations = PointAttributes.Find<Vector3>("point_locations");
            if(polygon.DebugCheck(pointLocations) == false)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for(int i = pointIndices.Length - 1; i >= 0; --i)
                {
                    sb.Append(pointIndices[i].ToString());
                    if(i > 0)
                    {
                        sb.Append(", ");
                    }
                }
                Logger.Log(sb.ToString() + ");");
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for(int i = 0; i < pointIndices.Length; i++)
                {
                    sb.Append(pointIndices[i].ToString());
                    if(i < pointIndices.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                Logger.Log(sb.ToString() + ");");
            }
#endif
            return polygon;
        }
        public Polygon MakePolygon(List<int> pointIndices)
        {
            Polygon polygon = MakePolygon();

            /*  Sanity check  */
            for(int i = 0; i < pointIndices.Count; ++i)
            {
                for(int j = 0; j < pointIndices.Count; ++j)
                {
                    if(i == j)
                    {
                        continue;
                    }
                    if(pointIndices[i] == pointIndices[j])
                    {
                        throw new System.Exception("duplicate polygon corner point indices");
                    }
                }
            }

            foreach(int pointIndex in pointIndices)
            {
                Point point = Points[pointIndex];
                polygon.MakeCorner(point);
            }
#if DEBUG_CHECK
            var pointLocations = PointAttributes.Find<Vector3>("point_locations");
            if(polygon.DebugCheck(pointLocations) == false)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for(int i = pointIndices.Length - 1; i >= 0; --i)
                {
                    sb.Append(pointIndices[i].ToString());
                    if(i > 0)
                    {
                        sb.Append(", ");
                    }
                }
                Logger.Log(sb.ToString() + ");");
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for(int i = 0; i < pointIndices.Length; i++)
                {
                    sb.Append(pointIndices[i].ToString());
                    if(i < pointIndices.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                Logger.Log(sb.ToString() + ");");
            }
#endif
            return polygon;
        }
        public void CheckEdge(Edge edge)
        {
            var cornerIndices = CornerAttributes.FindOrCreate<uint>("corner_indices");
            /*  Required: A != B  */ 
            if(edge.A == edge.B)
            {
                throw new InvalidOperationException();
            }
            /*  Required: A and B share no corners  */ 
            foreach(var cornerA in edge.A.Corners)
            {
                foreach(var cornerB in edge.B.Corners)
                {
                    if(cornerA == cornerB)
                    {
                        throw new InvalidOperationException();
                    }
                    uint i0 = cornerIndices[cornerA];
                    uint i1 = cornerIndices[cornerB];
                    if(i0 == i1)
                    {
                        throw new InvalidCastException();
                    }
                }
            }
        }
        /// \brief Builds Edge connectivity information
        public void BuildEdges()
        {
            Edges.Clear();

            int polygonIndex = 0;
            foreach(Polygon polygon in polygons)
            {
                Point firstPoint    = polygon.Corners[0].Point;
                Point previousPoint = null;

                int cornerIndex = 0;
                foreach(Corner corner in polygon.Corners)
                {
                    if(previousPoint != null)
                    {
                        Point a = previousPoint;
                        Point b = corner.Point;
                        if(a == b)
                        {
                            throw new InvalidOperationException("Degenerate edge");
                        }
                        Edge edge = new Edge(a, b);
                        //CheckEdge(edge);
                        if(Edges.ContainsKey(edge) == false)
                        {
                            Edges[edge] = new HashSet<Polygon>();
                        }
                        /*else
                        {
                            Logger.Log("old edge");
                        }*/
                        Edges[edge].Add(polygon);
                    }

                    previousPoint = corner.Point;
                    ++cornerIndex;
                }

                Edge lastEdge = new Edge(previousPoint, firstPoint);
                //CheckEdge(lastEdge);
                if(edges.ContainsKey(lastEdge) == false)
                {
                    Edges[lastEdge] = new HashSet<Polygon>();
                }
                Edges[lastEdge].Add(polygon);
                ++polygonIndex;
            }

            /*foreach(HashSet<Polygon> polys in Edges.Values)
            {
                int count = polys.Count;
                if(count != 2)
                {
                    Logger.Log("Warning: edge with != 2 polygons, this can cause issues");
                }
            }*/
        }
    }
}
