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

//#define DEBUG_CHECK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RenderStack.Geometry
{
    /*  Comment: Mostly stable.  */
    public class TriangulateGeometryOperation : GeometryOperation
    {
        public TriangulateGeometryOperation(Geometry src)
        {
            Source = src;

            foreach(Point oldPoint in Source.Points)
            {
                MakeNewPointFromPoint(oldPoint);
            }

            foreach(Polygon oldPolygon in Source.Polygons)
            {
                MakeNewPointFromPolygonCentroid(oldPolygon);
            }

            for(uint polygonIndex = 0; polygonIndex < Source.Polygons.Count; ++polygonIndex)
            {
                Polygon oldPolygon = Source.Polygons[(int)polygonIndex];

                if(oldPolygon.Corners.Count == 3)
                {
                    Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);
                    AddPolygonCorners(newPolygon, oldPolygon);
                    continue;
                }

                for(int i = 0; i < oldPolygon.Corners.Count; ++i)
                {
                    Corner  oldCorner   = oldPolygon.Corners[i];
                    Corner  nextCorner  = oldPolygon.Corners[(i + 1) % oldPolygon.Corners.Count];
                    Polygon newPolygon  = Destination.MakePolygon();

                    MakeNewCornerFromPolygonCentroid(newPolygon, oldPolygon);
                    MakeNewCornerFromCorner         (newPolygon, oldCorner);
                    MakeNewCornerFromCorner         (newPolygon, nextCorner);
                }
            }

            BuildDestinationEdgesWithSourcing();
            InterpolateAllAttributeMaps();
        }
    }
}
