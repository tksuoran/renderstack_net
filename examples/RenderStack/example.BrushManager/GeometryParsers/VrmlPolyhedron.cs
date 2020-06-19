//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

using RenderStack.Geometry;
using RenderStack.Math;

namespace example.Brushes
{
    /*  Comment: Highly experimental  */ 
    public class VrmlPolyhedron
    {
        private List<Vector3>   locations = new List<Vector3>();
        private List<List<int>> polygons = new List<List<int>>();

        public List<Vector3>    Locations   { get { return locations; } }
        public List<List<int>>  Polygons    { get { return polygons; } }

        private string  text;
        private int     pos = 0;

        private bool Seek(string label)
        {
            int store = pos;
            pos = text.IndexOf(label);
            if(pos == -1)
            {
                pos = store;
                return false;
            }
            pos += label.Length + 1;
            return true;
        }
        private bool SeekNext(string label)
        {
            int store = pos;
            if(pos == -1)
            {
                pos = store;
                return false;
            }
            pos = text.IndexOf(label, pos);
            if(pos == -1)
            {
                pos = store;
                return false;
            }
            pos += label.Length + 1;
            string debug = text.Substring(pos - 1);
            return true;
        }
        private void Eol()
        {
            int store = pos;
            pos = text.IndexOf("\n", pos);
            if(pos == -1)
            {
                pos = store;
            }
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
            string debug = text.Substring(startPos, 20);
            SeekWhiteSpace();
            float value = float.Parse(valueString);
            return value;
        }

        public VrmlPolyhedron(string file)
        {
            text = System.IO.File.ReadAllText(file);
            text = text.Replace(',', ' ');

            if(Seek("point [") == false)
            {
                if(Seek("point[") == false)
                {
                    return;
                }
            }

            float scale = 1.0f;

            Vector3 p = new Vector3();
            Vector3 sum = new Vector3();

            try
            {
                while(true)
                {
                    p.X = ParseFloat() * scale;
                    p.Y = ParseFloat() * scale;
                    p.Z = ParseFloat() * scale;
                    locations.Add(p);
                    sum.X += p.X;
                    sum.Y += p.Y;
                    sum.Z += p.Z;
                    Eol();
                }
            }
            catch(System.Exception)
            {
            }

            Vector3 center = sum / locations.Count;

            while(true)
            {
                int store0 = pos;
                if(SeekNext("coordIndex [") == false)
                {
                    int store1 = pos;
                    if(SeekNext("coordIndex[") == false)
                    {
                        return;
                    }
                }
                try
                {
                    int index;
                    var polygon = new List<int>();
                    while(true)
                    {
                        index = ParseInt();
                        if(index == -1)
                        {
                            if(polygon.Count > 2)
                            {
                                polygons.Add(polygon);
                            }
                            polygon = new List<int>();
                            continue;
                        }
                        if(polygon.Contains(index) == false)
                        {
                            /*  filter out duplicates  */
                            // throw new System.Exception("duplicate polygon vertex index");
                            polygon.Add(index);
                        }
                    }
                }
                catch(System.Exception)
                {
                }
            }
        }
    }
}
