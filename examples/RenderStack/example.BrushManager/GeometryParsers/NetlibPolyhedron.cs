using System.Collections.Generic;

using RenderStack.Geometry;
using RenderStack.Math;

/*  http://www.georgehart.com/virtual-polyhedra/netlib-info.html  */

namespace example.Brushes
{
    /*  Comment: Highly experimental  */ 
    public class NetlibPolyhedron
    {
        private string          name;
        private int             number;
        private string          symbol;
        private string          dual;
        private Vector3[]       locations;
        private List<List<int>> polygons = new List<List<int>>();

        public string           Name        { get { return name; } }
        public int              Number      { get { return number; } }
        public string           Symbol      { get { return symbol; } }
        public string           Dual        { get { return dual; } }
        public Vector3[]        Locations   { get { return locations; } }
        public List<List<int>>  Polygons    { get { return polygons; } }

        private string  text;
        private int     pos = 0;

        private HashSet<int> usedVertices = new HashSet<int>();

        private bool Seek(string label)
        {
            pos = text.IndexOf(label);
            if(pos == -1)
            {
                return false;
            }
            pos += label.Length + 1;
            return true;
        }
        private void Eol()
        {
            pos = text.IndexOf("\n", pos);
        }
        private string ParseLine()
        {
            int startPos = pos;
            Eol();
            return text.Substring(startPos, pos - startPos);
        }
        private void SkipWhiteSpaces()
        {
            while(char.IsWhiteSpace(text[pos]))
            {
                ++pos;
            }
        }
        private void SeekWhiteSpace()
        {
            while(
                (pos < text.Length) &&
                (char.IsWhiteSpace(text[pos]) == false)
            )
            {
                ++pos;
            }
        }
        private void SeekNonNumber()
        {
            while(
                (pos < text.Length) &&
                (char.IsWhiteSpace(text[pos]) == false) &&
                (text[pos] != '[')
            )
            {
                ++pos;
            }
        }
        private int ParseInt()
        {
            SkipWhiteSpaces();
            int startPos = pos;
            SeekNonNumber();
            string valueString = text.Substring(startPos, pos - startPos);
            int value = int.Parse(valueString);
            return value;
        }
        private float ParseFloat()
        {
            //  A value consists of a floating point number optionally
            //  followed by a expression enclosed by `[]'. The expression
            //  is the exact value represented in bc(1) code with the
            //  following function meanings: a(x) = tan-1(x), b(x) = (x)1/3,
            //  c(x) = cos(x), d(x) = tan(x), p = J, q(x) = x2,
            //  r(x) = cos-1(x), s(x) = sin(x), t = U. The code may include
            //  assignments but does not include white space.

            SkipWhiteSpaces();
            int startPos = pos;
            SeekNonNumber();
            string valueString = text.Substring(startPos, pos - startPos);
            SeekWhiteSpace();
            float value = float.Parse(valueString);
            return value;
        }

        private void ParseName()
        {
            //  The polyhedron's name is less than 128 characters
            //  long and is not capitalized.
            if(Seek(":name") == false)
            {
                return;
            }
            name = ParseLine();
        }
        private void ParseNumber()
        {
            //  The polyhedron's number (written and read with the
            //  %d printf/scanf format).
            Seek(":number");
            number = ParseInt();
        }
        private void ParseSymbol()
        {
            //  The eqn(1) input for two symbols separated by a
            //  tab; the Johnson symbol, and the Schlafli symbol.
            Seek(":symbol");
            symbol = ParseLine();
        }
        private void ParseDual()
        {
            //  The name of the dual polyhedron optionally followed
            //  by a horizontal tab and the number of the dual.
            if(Seek(":dual") == false)
            {
                return;
            }
            dual = ParseLine();
        }
        private void ParseSFaces()
        {
            Seek(":sfaces");
        }
        private void ParseSVertices()
        {
            Seek(":svertices");
        }
        private void ParseNet()
        {
            //  The first line contains the number of faces and
            //  the maximum number of vertices in a face. The
            //  remaining lines are the faces in the planar net.
            //  Each face has a vertex count followed by the
            //  vertex numbers. The vertices are listed in
            //  counter-clockwise order as viewed from outside
            //  the polyhedron.
            Seek(":net");
            int faceCount = ParseInt();
            int maxVertices = ParseInt();

            for(int i = 0; i < faceCount; ++i)
            {
                int vertexCount = ParseInt();
                for(int j =-0; j < vertexCount; ++j)
                {
                    int vertex = ParseInt();
                }
            }
        }
        private void ParseSolidUsedVertices()
        {
            if(Seek(":solid") == false)
            {
                return;
            }
            int faceCount = ParseInt();
            int maxVertices = ParseInt();

            usedVertices.Clear();
            for(int i = 0; i < faceCount; ++i)
            {
                int indexCount = ParseInt();
                for(int j = 0; j < indexCount; ++j)
                {
                    int index = ParseInt();
                    usedVertices.Add(index);
                }
            }
        }
        private void ParseSolid()
        {
            polygons.Clear();
            //  The first line contains the number of faces and
            //  the maximum number of vertices in a face. The
            //  remaining lines are the faces in the 3D
            //  polyhedron. Each face has a vertex count
            //  followed by the vertex numbers. The vertices
            //  are listed in counter-clockwise order as viewed
            //  from outside the polyhedron.
            if(Seek(":solid") == false)
            {
                return;
            }
            int faceCount = ParseInt();
            int maxVertices = ParseInt();

            for(int i = 0; i < faceCount; ++i)
            {
                List<int> polygon = new List<int>();
                polygons.Add(polygon);
                int indexCount = ParseInt();
                for(int j = 0; j < indexCount; ++j)
                {
                    int index = ParseInt();
                    polygon.Add(index);
                }
                polygon.Reverse();
            }
        }
        private void ParseHinges()
        {
            // the first line contains the number of hinges in
            // the planar net. The remaining lines are hinge
            // connections. The format is face1 side1 face2
            // side2 value. Sides are numbered from zero.  If
            // the dihedral value is greater than J, it is a
            // reflex or re-entrant hinge.
            if(Seek(":hinges") == false)
            {
                return;
            }
            int hingeCount = ParseInt();

            for(int i = 0; i < hingeCount; ++i)
            {
                int face1 = ParseInt();
                int side1 = ParseInt();
                int face2 = ParseInt();
                int side2 = ParseInt();
                int value = ParseInt();
            }
        }
        private void ParseDihedrals()
        {
            //  The first line contains the number of distinct
            //  dihedrals. Each dihedral starts on a new line
            //  and has a count and a value. If the count is
            //  non-zero, then that many face edge pairs (one
            //  per line) follow the dihedral value.
            Seek(":dih");
        }
        private void ParseVertices()
        {
            //  The first line contains the number of vertices.
            //  The vertices are arranged one per line as an
            //  (x, y, z) coordinate of white-space separated
            //  values (described below). The vertices are
            //  implicitly numbered starting at zero.

            if(Seek(":vertices") == false)
            {
                return;
            }

            int vertexCount = ParseInt();
            Eol(); // sometimes there is extra int

            Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);
            locations = new Vector3[vertexCount];
            for(int i = 0; i < vertexCount; ++i)
            {
                float scale = 1.0f / 2.0f;
                locations[i].X = ParseFloat() * scale;
                locations[i].Y = ParseFloat() * scale;
                locations[i].Z = ParseFloat() * scale;

                if(usedVertices.Contains(i))
                {
                    sum.X += locations[i].X;
                    sum.Y += locations[i].Y;
                    sum.Z += locations[i].Z;
                }
            }
            Vector3 center = sum / (float)(usedVertices.Count);

            for(int i = 0; i < vertexCount; ++i)
            {
                locations[i] -= center;
            }
        }

        public NetlibPolyhedron(string file)
        {
            text = System.IO.File.ReadAllText(file);
            text = text.Replace("\r\n", "\n");

            ParseName();
#if false
            ParseNumber();
            //ParseSymbol();
            ParseDual();
            //ParseSFaces();
            //ParseSVertices();
            //ParseNet();
            ParseSolidUsedVertices();
            ParseVertices();
            ParseSolid();
            //ParseHinges();
            //ParseDihedrals();
            //":EOF"
#endif
        }
    }
}
