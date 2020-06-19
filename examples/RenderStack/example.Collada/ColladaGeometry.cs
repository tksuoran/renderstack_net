using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using RenderStack.Geometry;
using RenderStack.Math;

using Collada;

namespace examples
{
    public class ColladaGeometry : Geometry
    {
        public ColladaGeometry(string file)
        {
            var collada = Grendgine_Collada.Grendgine_Load_File(file);

            if(collada == null)
            {
                return;
            }

            foreach(var geometry in collada.Library_Geometries.Geometry)
            {
                var mesh = geometry.Mesh;

                Dictionary<string, Collada_Source> sources = new Dictionary<string,Collada_Source>();
                foreach(var source in mesh.Source)
                {
                    sources[source.ID] = source;
                }

                foreach(var polylist in mesh.Polylist)
                {
                    // input
                    int[]   vcount  = polylist.VCount.Value();
                    int[]   p       = polylist.P.Value();
                    StringBuilder vSb = new StringBuilder();
                    StringBuilder pSb = new StringBuilder();

                    foreach(int i in vcount)
                    {
                        vSb.Append(i.ToString());
                        vSb.Append(" ");
                    }
                    foreach(int pi in p)
                    {
                        pSb.Append(pi.ToString());
                        pSb.Append(" ");
                    }
                    System.Diagnostics.Trace.TraceInformation(
                        "Polylist.VCount : " + vSb.ToString()
                    );
                    System.Diagnostics.Trace.TraceInformation(
                        "Polylist.P : " + pSb.ToString()
                    );
                    foreach(var input in polylist.Input)
                    {
                        System.Diagnostics.Trace.TraceInformation(
                            "input.Semantic : " + input.Semantic.ToString() + "\n" +
                            "input.Source   : " + input.source + "\n" +
                            "input.Offset   : " + input.Offset + "\n" +
                            "input.Set      : " + input.Set + "\n"
                        );
                        Collada_Source source = sources[input.source];
                        if(source.Technique_Common != null)
                        {
                        }

                    }
                    System.Diagnostics.Debugger.Break();
                }

                /*var sources = mesh.Source;
                var vertices = mesh.Vertices;
                foreach(var input in vertices.Input)
                {
                }*/
                
                foreach(var polygon in geometry.Mesh.Polygons)
                {
                    // input points to sources either directly 
                    // or through sources defined in vertices
                    // (vertex semantics)
                    foreach(var input in polygon.Input)
                    {
                    }
                    foreach(int index in polygon.P.Value())
                    {
                        
                    }
                }
            }

        }
    }
}
