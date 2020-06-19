using System.Collections.Generic;
using System.Diagnostics;

using RenderStack.Math;
using RenderStack.Geometry;

using I1 = System.SByte;
using I2 = System.Int16;
using I4 = System.Int32;
using U1 = System.Byte;
using U2 = System.UInt16;
using U4 = System.UInt32;
using F4 = System.Single;
using S0 = System.String;
using VX = System.UInt32;
using COL4 = RenderStack.Math.Vector4;
using COL12 = RenderStack.Math.Vector4;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

//  NOTE: Vertex maps (UMAP, UMAD) are Layer spesific
//  Surface references to the maps with names.
//  Many Layers can have the same vertex map name
//  where they have different coordinates.

namespace RenderStack.LightWave
{
    public class LWLayer
    {
        private RenderStack.Geometry.Geometry   geometry    = new RenderStack.Geometry.Geometry();
        private Dictionary<int, LWEnvelope>     envelopes   = new Dictionary<int,LWEnvelope>();

        public RenderStack.Geometry.Geometry    Geometry    { get { return geometry; } }
        public Dictionary<int, LWEnvelope>      Envelopes   { get { return envelopes; } }
        public string                           Name;
        public int                              ParentLayer;
        public Vector3                          Pivot;
        public U2                               Flags;
        public string                           DescriptionLine;
        public string                           CommentaryText;
        public Vector3                          BBoxMin;
        public Vector3                          BBoxMax;

        public LWLayer(
            string  name,
            U2      flags,
            Vector3 pivot,
            int     parent
        )
        {
            this.Flags          = flags;
            this.ParentLayer   = parent;
            Name                = name;
            Pivot               = pivot;
            BBoxMin             = Vector3.Zero;
            BBoxMax             = Vector3.Zero;
        }
    }

    public partial class LWModelParser
    {
        /*  LWLO LAYR Chunk

            An LAYR chunk must precede each set of PNTS, POLS and CRVS data
            chunks and indicates to which layer those data belong. An LAYR
            chunk consists of two unsigned short values and a string. The first
            is the layer number which should be from 1 to 10 to operate
            correctly in Modeler. This restriction may be lifted in future
            versions of the format. The second value is a bitfield where only
            the lowest order bit is defined and all others should be zero. This
            bit is one if this is an active layer and zero if it is a background
            layer. The string which follows is the name of the layer and should
            be null-terminated and padded to even length.
        */
        void layer_U2_U2_S0()
        {
            var number = f.ReadU2();
            var flags  = f.ReadU2();
            var name   = f.ReadS0();

            Debug.WriteLine("Layer " + number + " name " + name);

            currentLayer = model.MakeLayer(number, name, flags, Vector3.Zero, -1);
        }

        /*  LWO2 Start Layer 

            LAYR { number[U2], flags[U2], pivot[VEC12], name[S0], parent[U2] } 

            Signals the start of a new layer. All the data chunks
            which follow will be included in this layer until
            another layer chunk is encountered. If data is encountered
            before a layer chunk, it goes into an arbitrary layer.
            If the least significant bit of flags is set, the layer
            is hidden. The parent index indicates the default parent
            for this layer and can be -1 or missing to indicate no
            parent. 
        */
        void layer_U2_U2_VEC12_S0_U2()
        {
            var number = f.ReadU2();
            var flags  = f.ReadU2();
            var pivot  = f.ReadVEC12();
            var name   = f.ReadS0();
            I2  parent = 1;

            Debug.WriteLine("Layer " + number + " name " + name);

            if(f.Left() >= 2)
            {
                I2 lp = f.ReadI2();
                if(lp > 1)
                {
                    parent = lp;
                }
            }

            currentLayer = model.MakeLayer(number, name, flags, Vector3.Zero, parent);
        }

        public void LAYRchunk()
        {
            ID chunkId     = f.ReadID4();
            U4 chunkLength = f.ReadU4();

            Debug.WriteLine(f.Type.ToString() + "::" + chunkId.ToString() + " " + chunkLength + " bytes");

            f.Push(chunkLength);

            switch(chunkId.value)
            {
                //  LWOB, LWLO, LWO2
                case ID.PNTS: pointList       (); break;

                //  LWOB, LWLO
                case ID.CRVS: oldCurveList    (); break;
                case ID.PCHS: oldPatchList    (); break;
                case ID.SRFS: readSurfaceList (); break;

                //  LWO2
                case ID.VMAP: vertexMapping_ID4_U2_S0_d (); break;
                case ID.VMAD: vertexMappingD_ID4_U2_S0_d(); break;
                case ID.VMPA: vertexMapParameter_I4_I4  (); break;
                case ID.TAGS: tags_d                    (); break;
                case ID.CLIP: clip_U4_sc                (); break;
                case ID.PTAG: polygonTags_ID4_d         (); break;
                case ID.ENVL: envelope_U4_sc            (); break;
                case ID.BBOX: boundingBox_VEC12_VEC12   (); break;
                case ID.DESC: descriptionLine_S0        (); break;
                case ID.TEXT: comments_S0               (); break;
                case ID.ICON: thumbnail_U2_U2_d         (); break;

                case ID.POLS: if(f.Type.value == ID.LWO2) polygonList();             else oldPolygonList(); break;
                case ID.SURF: if(f.Type.value == ID.LWO2) surf_S0_S0_sc();           else readSurface_sc(); break;
                case ID.LAYR: if(f.Type.value == ID.LWO2) layer_U2_U2_VEC12_S0_U2(); else layer_U2_U2_S0(); break;

                default:        Debug.WriteLine("Unknown chunk " + chunkId); break;
            }

            f.Pop(true);
        }

        /*  LWOB, LWLO, LWO2  PNTS { point-location[VEC12] * } 
            
            This chunk contains a list of the X, Y, and Z coordinates of all the points 
            in an object. Since each coordinate has three components, and each component 
            is stored as a four byte floating point number, the number of points in an 
            object can be determined by dividing the size in bytes of the PNTS chunk by 
            12. 
            
            By convention, the +X direction is to the right or east, the +Y direction is 
            upward, and the +Z direction is forward or north. For models of real-world 
            objects, the unit size is usually considered to be one meter. The 
            coordinates are specified relative to an object's pivot point. See the 
            LightWave Modeler manual for more information about LightWave 3D's geometric 
            conventions. 
            
            Points in the PNTS chunk are numbered in the order they occur, starting with 
            zero. This index is then used by polygons to define their vertices. The PNTS 
            chunk must be before the POLS, CRVS, and PCHS chunks in the file. 
        */
        void pointList()
        {
            int pointCount = (int)(f.Left() / 12);
            var pointLocations = currentLayer.Geometry.PointAttributes.FindOrCreate<Vector3>("point_locations");
            for(int i = 0; i < pointCount; i++)
            {
                if(f.Left() < 3 * 4)
                {
                    throw new System.InvalidOperationException("Not enough floats found in chunk for point");
                }

                Point   p = currentLayer.Geometry.MakePoint();
                Vector3 vec = f.ReadVEC12();
                vec += currentLayer.Pivot;       //  FIX
                vec.Z = -vec.Z;     //  FIX check this and matrix mirroring

                pointLocations[p] = vec;
            }
        }

        /*  LWOB POLS { ( numvert[U2], vert[U2] # numvert, surf[I2] )* } 
            
            This chunk contains a list of all the polygons in an object. 
            Each entry consists of a short integer specifying the number
            of vertices in the polygon followed by that many short integers 
            specifying the vertices themselves (as indices into the points 
            list) followed by a short integer specifying which surface is 
            used by the polygon (as an index into the surfaces list). The 
            number of vertices in a polygon currently may vary from one to 
            200. The vertex list for each polygon should begin at a convex 
            vertex and proceed clockwise as seen from the visible side of 
            the polygon (LightWave 3D polygons are single-sided, except 
            for those whose surfaces have the double-sided flag set). 
            Polygons should be read from the file until as many 
            bytes as the chunk size specifies have been read. 
            
            Since the points in the PNTS chunk are referenced using 
            two-byte integers, the effective maximum number of points 
            in a LightWave object file is 65,536. This is an inherient 
            limitation of this current format. 
            
            A negative surface number for a polygon indicates that the 
            polygon has detail polygons (which are drawn on top of the 
            main polygon and may be  coplanar with it). In this case, 
            the next number in the file is a short integer specifying 
            how many detail polygons belong to the current polygon. 
            This is followed by a list of those detail polygons, where 
            each entry is of the same format as described above for 
            regular polygons (except that the detail polygons cannot 
            have details of their own). The list of regular polygons 
            then resumes. To determine which surface is used by a polygon 
            with a negative surface number, the absolute value of that 
            number should be used. Note, however, that detail polygons 
            are mostly obsolete, so even though they may be recognized 
            by LightWave and old files contain them, they should be 
            ignored.
        */
        void oldPolygonList()
        {
            var polygonSurfaces = currentLayer.Geometry.PolygonAttributes.FindOrCreate<int>("polygon_surfaces");
            while(f.Left() > 0)
            {
                U2 numVertices = f.ReadU2();

                var face = currentLayer.Geometry.MakePolygon();
                for(int i = 0; i < numVertices; ++i)
                {
                    if(f.Left() < 2)
                    {
                        throw new System.Exception("Not enough shorts found in chunk");
                    }
                    U2 vertexIndex = f.ReadU2();

                    Point pnt = currentLayer.Geometry.Points[vertexIndex];
                    if(pnt == null)
                    {
                        throw new System.Exception("Point " + vertexIndex + " not found");
                    }
                    Corner cor = face.MakeCorner(pnt);
                }
                face.Reverse();

                int surfaceIndex = f.ReadU2();
                polygonSurfaces[face] = surfaceIndex;
            }
        }

        void oldCurveList()
        {
            // \todo
            Trace.TraceWarning("LWOB curves are not implemented");
        }
        void oldPatchList()
        {
            // \todo
            Trace.TraceWarning("LWOB patches are not implemented");
        }

        /*  LWO2 Polygon List

            POLS { type[ID4], ( numvert+flags[U2], vert[VX] # numvert )* } 

            A list of polygons for the current layer. Possible polygon types include:

            FACE
                "Regular" polygons, the most common.

            CURV
                Catmull-Rom splines. These are used during modeling and are
                currently ignored by the renderer.

            PTCH
                Subdivision patches. The POLS chunk contains the definition
                of the control cage polygons, and the patch is created by
                subdividing these polygons. The renderable geometry that
                results from subdivision is determined interactively by the
                user through settings within LightWave®. The subdivision
                method is undocumented.

            MBAL
                Metaballs. These are single-point polygons. The points are
                associated with a VMAP of type MBAL that contains the radius
                of influence of each metaball. The renderable polygonal
                surface constructed from a set of metaballs is inferred as an
                isosurface on a scalar field derived from the sum of the
                influences of all of the metaball points.

            BONE
                Line segments representing the object's skeleton. These are
                converted to bones for deformation during rendering.

            Each polygon is defined by a vertex count followed by a list of
            indexes into the most recent PNTS chunk. The maximum number of
            vertices is 1023. The 6 high-order bits of the vertex count are
            flag bits with different meanings for each polygon type. When
            reading POLS, remember to mask out the flags to obtain numverts.
            (For CURV polygon: The two low order flags are for continuity
            control point toggles. The four remaining high order flag bits
            are additional vertex count bits; this brings the maximum number
            of vertices for CURV polygons to 2^14 = 16383.)

            When writing POLS, the vertex list for each polygon should begin
            at a convex vertex and proceed clockwise as seen from the visible
            side of the polygon. LightWave® polygons are single-sided (although
            double-sidedness is a possible surface property), and the normal
            is defined as the cross product of the first and last edges.
        */
        void polygonList()
        {
            // \todo type = ID.FACE / ID.CURV / ID.PACH - have separate geometry for each?
            var type = f.ReadID4();

            while(f.Left() > 0)
            {
                U2  data    = f.ReadU2();
                U2  numvert = (U2)(data & 0x03ff);
                U2  flags   = (U2)((data & ~numvert) >> 10);
                var face    = currentLayer.Geometry.MakePolygon();

                for(int i = 0; i < numvert; ++i)
                {
                    if(f.Left() < 2)
                    {
                        throw new System.Exception("Not enough bytes left in chunk");
                    }

                    U4 vertex_index = f.ReadVX();
                    face.MakeCorner(
                        currentLayer.Geometry.Points[(int)vertex_index]
                    );
                }

                face.Reverse();
            }
        }

        /*  LWO2 Discontinuous Vertex Mapping 

            VMAD { 
                type      [ID4], 
                dimension [U2], 
                name      [S0],
                ( 
                    vert  [VX], 
                    poly  [VX], 
                    value [F4] # dimension 
                )* 
            } 

            (Introduced with LightWave 6.5.) 
            
            Associates a set of floating-point vectors with 
            the vertices of specific polygons. VMADs are similar 
            to VMAPs, but they assign vectors to polygon vertices 
            rather than points. For a given mapping, a VMAP always 
            assigns only one vector to a point, while a VMAD can 
            assign as many vectors to a point as there are polygons 
            sharing the point.

            The motivation for VMADs is the problem of seams in
            UV texture mapping. If a UV map is topologically 
            equivalent to a cylinder or a sphere, a seam is formed 
            where the opposite edges of the map meet. Interpolation 
            of UV coordinates across this discontinuity is 
            aesthetically and mathematically incorrect. The VMAD 
            substitutes an equivalent mapping that interpolates 
            correctly. It only needs to do this for polygons in 
            which the seam lies.

            VMAD chunks are paired with VMAPs of the same name, 
            if they exist. The vector values in the VMAD will 
            then replace those in the corresponding VMAP, but 
            only for calculations involving the specified polygons. 
            When the same points are used for calculations on 
            polygons not specified in the VMAD, the VMAP values 
            are used.

            VMADs need not be associated with a VMAP. They can 
            also be used simply to define a (discontinuous) 
            per-polygon mapping. But not all mapping types are 
            valid for VMADs, since for some types it makes no sense 
            for points to have more than one map value. TXUV, RGB, 
            RGBA and WGHT types are supported for VMADs, for example, 
            while MORF and SPOT are not. VMADs of unsupported types 
            are preserved but never evaluated.
        */
        void vertexMappingD_ID4_U2_S0_d()
        {
            var type        = f.ReadID4();
            var dimension   = f.ReadU2();
            var name        = f.ReadS0();

            Debug.WriteLine("vertexMapping " + name);

            switch(dimension)
            {
                case 0: break;
                case 1:
                {
                    var map = currentLayer.Geometry.CornerAttributes.FindOrCreate<float>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var j = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        var c = currentLayer.Geometry.Polygons[(int)j].Corner(p);
                        F4  x = f.ReadF4();
                        map[c] = x;
                    }
                    break;
                }
                case 2:
                {
                    var map = currentLayer.Geometry.CornerAttributes.FindOrCreate<Vector2>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var j = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        var c = currentLayer.Geometry.Polygons[(int)j].Corner(p);
                        F4  x = f.ReadF4();
                        F4  y = f.ReadF4();
                        map[c] = new Vector2(x, y);
                    }
                    break;
                }
                case 3:
                {
                    var map = currentLayer.Geometry.CornerAttributes.FindOrCreate<Vector3>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var j = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        var c = currentLayer.Geometry.Polygons[(int)j].Corner(p);
                        F4  x = f.ReadF4();
                        F4  y = f.ReadF4();
                        F4  z = f.ReadF4();
                        map[c] = new Vector3(x, y, z);
                    }
                    break;
                }
                case 4:
                {
                    var map = currentLayer.Geometry.CornerAttributes.FindOrCreate<Vector4>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var j = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        var c = currentLayer.Geometry.Polygons[(int)j].Corner(p);
                        F4  x = f.ReadF4();
                        F4  y = f.ReadF4();
                        F4  z = f.ReadF4();
                        F4  w = f.ReadF4();
                        map[c] = new Vector4(x, y, z, w);
                    }
                    break;
                }
                default:
                {
                    Debug.WriteLine("Unsupported dimension for VMAP");
                    break;
                }
            }
        }

        /*  LWO2 Vertex Map Parameter

            VMPA { UV subdivision type[I4], sketch color[I4] }

            Describes special properties of VMAPs. 
            The UV subdivision type ids are: 

            0 - linear
            1 - subpatch
            2 - subpatch linear corners
            3 - subpatch linear edges
            4 - subpatch disco edges
        */
        void vertexMapParameter_I4_I4()
        {
            //  \todo is this for each vertex map or global?
            I4 subdivisionType = f.ReadI4();
            I4 sketchColor = f.ReadI4();
        }

        /*  LWO2 Vertex Mapping 

            VMAP { 
                type      [ID4], 
                dimension [U2], 
                name      [S0], 
                ( 
                    vert  [VX], 
                    value [F4] # dimension 
                )* 
            } 

            Vertex Mapping

            Associates a set of floating-point vectors with a set of points.
            VMAPs begin with a type, a dimension (vector length) and a name.
            These are followed by a list of vertex/vector pairs. The vertex
            is given as an index into the most recent PNTS chunk, in VX
            format. The vector contains dimension floating-point values.
            There can be any number of these chunks, but they should all
            have different types or names.

            Some common type codes are

                PICK
                    Selection set. This is a VMAP of dimension 0 that
                    marks points for quick selection by name during
                    modeling. It has no effect on the geometry of
                    the object.

                WGHT
                    Weight maps have a dimension of 1 and are generally
                    used to alter the influence of deformers such as
                    bones. Weights can be positive or negative, and the
                    default weight for unmapped vertices is 0.0.

                MNVW
                    Subpatch weight maps affect the shape of geometry
                    created by subdivision patching.
                TXUV
                    UV texture maps have a dimension of 2.
                RGB, RGBA
                    Color maps, with a dimension of 3 or 4.
                MORF
                    These contain vertex displacement deltas.
                SPOT
                    These contain absolute vertex displacements
                    (alternative vertex positions).

            Other widely used map types will almost certainly appear in the future.
        */
        void vertexMapping_ID4_U2_S0_d()
        {
            var type        = f.ReadID4();
            var dimension   = f.ReadU2();
            var name        = f.ReadS0();

            Debug.WriteLine("vertexMapping " + name);

            switch(dimension)
            {
                case 0: break;
                case 1:
                {
                    var map = currentLayer.Geometry.PointAttributes.FindOrCreate<float>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        F4  x = f.ReadF4();
                        map[p] = x;
                    }
                    break;
                }
                case 2:
                {
                    var map = currentLayer.Geometry.PointAttributes.FindOrCreate<Vector2>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        F4  x = f.ReadF4();
                        F4  y = f.ReadF4();
                        map[p] = new Vector2(x, y);
                    }
                    break;
                }
                case 3:
                {
                    var map = currentLayer.Geometry.PointAttributes.FindOrCreate<Vector3>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        F4  x = f.ReadF4();
                        F4  y = f.ReadF4();
                        F4  z = f.ReadF4();
                        map[p] = new Vector3(x, y, z);
                    }
                    break;
                }
                case 4:
                {
                    var map = currentLayer.Geometry.PointAttributes.FindOrCreate<Vector4>(name);
                    while(f.Left() > 0)
                    {
                        var i = f.ReadVX();
                        var p = currentLayer.Geometry.Points[(int)i];
                        F4  x = f.ReadF4();
                        F4  y = f.ReadF4();
                        F4  z = f.ReadF4();
                        F4  w = f.ReadF4();
                        map[p] = new Vector4(x, y, z, w);
                    }
                    break;
                }
                default:
                {
                    Debug.WriteLine("Unsupported dimension for VMAP");
                    break;
                }
            }
        }

        /*  LWO2 Polygon Tag Mapping 

            PTAG { type[ID4], ( poly[VX], tag[U2] )* } 

            This chunk contains the all the tags of a given
            type for some subset of the polygons defined in
            the preceding POLS chunk. The type of the tag
            association is given by the first element of the
            chunk and is an normal 4-character ID code. The
            rest of the chunk is a list of polygon/tag
            associations. The polygon is identified by its
            index into the previous POLS chunk, and the tag
            is given by its index into the previous TAGS chunk.
            Any number of polygons may be mapped with this
            type of tag, and mappings should be read from the
            file until as many bytes as the chunk size
            specifies have been read. 

            Polygon tags types and their values are extensible,
            but there are some pre-defined types. 
            
            The SURF type tags each polygon with the name of its 
            surface. In LightWave 3D terminology, a "surface" is 
            defined as a named set of shading attributes and may 
            be described in the object file in SURF chunks.
            
            Another pre-defined type is PART which describes what
            aspect of the model each polygon belongs to, 
            
            and SMGP which names the smoothing group for each polygon. 
            Not all polygons have a value for every tag type, and the 
            behavior for polygon which lack a certain tag depends on 
            the type.

            BNWT - skelegon weight map name
            BONE - skelegon name
        */
        void polygonTags_ID4_d()
        {
            var type = f.ReadID4();
            var map = currentLayer.Geometry.PolygonAttributes.FindOrCreate<int>("TAG<" + type + ">");

            while(f.Left() > 0)
            {
                var polygonIndex    = f.ReadVX();
                var tagIndex        = f.ReadU2();
                var face            = currentLayer.Geometry.Polygons[(int)polygonIndex];
                map[face] = (int)tagIndex;
            }
        }

        /*  LWO2 Bounding Box 

            BBOX { min[VEC12], max[VEC12] } 

            This is an optional chunk which can be included to store the bounding box
            for the vertex data in a layer. The min and max vectors are the lower and
            upper corners of the bounding box. 
        */
        void boundingBox_VEC12_VEC12()
        {
            currentLayer.BBoxMin = f.ReadVEC12();
            currentLayer.BBoxMax = f.ReadVEC12();
        }

        /*  LWO2 Description Line 

            DESC { description-line[S0] } 

            This is an optional chunk which can be used to hold an object description.
            This should be a simple line of upper and lowercase characters, punctuation
            and spaces which describes the contents of the object file. There should be
            no control characters in this text string and it should generally be kept
            short. 
        */
        void descriptionLine_S0()
        {
            currentLayer.DescriptionLine = f.ReadS0();
        }

        /*  LWO2 Commentary Text 

            TEXT { comment[S0] } 

            This is another optional chunk which can be used to hold comments about the
            object. The text is just like the DESC chunk, but it can be about any subject,
            it may contain newline characters and it does not need to be particularly short. 
        */
        void comments_S0()
        {
            currentLayer.CommentaryText = f.ReadS0();
        }

        /*  LWO2 Thumbnail Icon Image 

            ICON { encoding[U2], width[U2], data[U1] * } 

            This optional chunk contains an iconic or thumbnail image for the object
            which can be used when viewing the file in a browser. The encoding is a
            code for the data format which can only be zero for now meaning uncompressed,
            unsigned bytes as RGB triples. The width specifies the number of pixels
            contained in each row of the image. The data consists of rows of pixels
            (RGB triples for now), and the height of the image is determined by the
            length of the data.
        */
        void thumbnail_U2_U2_d()
        {
            U2 encoding = f.ReadU2();
            U2 width    = f.ReadU2();

            if(encoding != 0)
            {
                Trace.TraceWarning("Unknown thumbnail encoding");
                return;
            }

            while(f.Left() > 0)
            {
                for(int x = 0; x < width; ++x)
                {
                    U1 red      = f.ReadU1();
                    U1 green    = f.ReadU1();
                    U1 blue     = f.ReadU1();
                    // \todo store data 
                }
            }
        }
    }
}
