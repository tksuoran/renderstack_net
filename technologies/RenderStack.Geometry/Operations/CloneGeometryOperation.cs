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
    /// \brief Creates a clone of Geometry. Copies all Polygons, Points, Corners, and copies their (and edges) attributes.
    /// \note Mostly stable
    public class CloneGeometryOperation : GeometryOperation
    {
        public CloneGeometryOperation(Geometry src, HashSet<uint> selectedPolygonIndices)
        {
            Source = src;

            foreach(Point oldPoint in src.Points)
            {
                MakeNewPointFromPoint(oldPoint);
            }

            //  Clone polygons (and corners);
            for(uint polygonIndex = 0; polygonIndex < src.Polygons.Count; ++polygonIndex)
            {
                if(
                    (selectedPolygonIndices == null) || 
                    selectedPolygonIndices.Contains(polygonIndex)
                )
                {
                    Polygon oldPolygon = Source.Polygons[(int)polygonIndex];
                    Polygon newPolygon = MakeNewPolygonFromPolygon(oldPolygon);
                    AddPolygonCorners(newPolygon, oldPolygon);
                }
            }

            BuildDestinationEdgesWithSourcing();
            InterpolateAllAttributeMaps();
        }
    }
}