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

using OpenGL = OpenTK.Graphics.OpenGL;

using VertexAttribPointerType   = OpenTK.Graphics.OpenGL.VertexAttribPointerType;
using VertexAttribIPointerType  = OpenTK.Graphics.OpenGL.VertexAttribIPointerType;

namespace RenderStack.Graphics
{
    /// \brief Contains information necessary to feed uniform data to GL.
    /// 
    /// \note Mostly stable.
    public class AttributeBinding
    {
        public AttributeMapping         AttributeMapping    { get; private set; }
        public Attribute                Attribute           { get; private set; }
        public int                      Stride              { get; private set; }
        public int                      Slot                { get; private set; }

        public int                      Size                { get { return Attribute.Dimension; } }
        public VertexAttribPointerType  Type                { get { return Attribute.Type; } }
        public bool                     Normalized          { get { return Attribute.Normalized; } }
        public int                      Offset              { get { return Attribute.Offset; } }

        public AttributeBinding(
            AttributeMapping    mapping,
            Attribute           attribute,
            int                 stride
        )
        {
            AttributeMapping = mapping;
            Attribute = attribute;
            Stride = stride;
        }
        public AttributeBinding(
            AttributeMapping    mapping,
            Attribute           attribute,
            int                 stride,
            int                 slot
        )
        {
            AttributeMapping = mapping;
            Attribute = attribute;
            Stride = stride;
            Slot = slot;
        }
    }
}