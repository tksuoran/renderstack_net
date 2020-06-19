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

using System.Collections.Generic;
using RenderStack.Math;

namespace RenderStack.Geometry.Shapes
{
    /*  Comment: Mostly stable.  */
    [System.Serializable]
    public class HeightField : Geometry
    {
        public HeightField(float[,] heights, float scaleX, float scaleZ)
        {
            var pointLocations = PointAttributes.FindOrCreate<Vector3>("point_locations");
            var pointTexcoords = PointAttributes.FindOrCreate<Vector2>("point_texcoords");

            //  Generate vertices
            int xCount = heights.GetLength(0);
            int zCount = heights.GetLength(1);
            for(int x = 0; x < xCount; x++)
            {
                float s = (float)x / (float)xCount;
                float xP = (float)x * scaleX;
                for(int z = 0; z < zCount; z++ )
                {
                    float t = (float)z / (float)zCount;
                    float zP = (float)z * scaleZ;
                    float yP = heights[x, z];

                    var point = MakePoint();

                    pointLocations[point] = new Vector3(xP, yP, zP);
                    pointTexcoords[point] = new Vector2(s, t);

                    Points[x * zCount + z] = point;
                }
            }

            //  Generate quads
            for(int x = 0; x < xCount - 1; x++)
            {
                for(int z = 0; z < zCount - 1; z++)
                {
                    var pol = MakePolygon();
                    pol.MakeCorner(Points[(x + 1) * zCount + z    ]);
                    pol.MakeCorner(Points[(x + 1) * zCount + z + 1]);
                    pol.MakeCorner(Points[(x    ) * zCount + z + 1]);
                    pol.MakeCorner(Points[(x    ) * zCount + z    ]);
                }
            }

            ComputePolygonCentroids();
            ComputePolygonNormals();
            SmoothNormalize("corner_normals", "polygon_normals", (2.0f * (float)System.Math.PI));
        }
    }
}
