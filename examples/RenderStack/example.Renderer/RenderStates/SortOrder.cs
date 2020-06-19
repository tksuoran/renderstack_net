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
using RenderStack.Graphics;
using RenderStack.Scene;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace example.Renderer
{
    public enum SortOrder
    {
        NotSet,                 //  Use parent
        NoCare,                 //  Does not care
        DepthSortNearToFar,
        DepthSortFarToNear,
        ListOrder,
        ReverseListOrder
    }

    //  Update  - update roots
    //  Cull    - cull roots
    //  Sort    - sort roots
    //  Render 

    // [RenderPass][program][depth]
    // [framebuffer][zbufferbits][renderstates][low frequency shader params][textures][high frequency shader params / material][mesh]
    // |rt:2|viewport:3|layer:3|translucency:2|depth:24|material_id,pass:30|
    //  Sort:
    //    - render target, render target sequence number (render pass)
    //          shadow-fbo
    //          gbuffer
    //          linear
    //          filter1
    //          filter2
    //          filter1
    //          filter2
    //          screen
    //    - KeyValuePair<viewport, camera>
    //          viewport A, camera B
    //          viewport C, camera D
    //    - blend mode:
    //          opaque
    //          blend
    //    - program (near to far)   (from material)
    //          shadowCaster
    //          gbuffer
    //          blinnPhong
    //          anisotropic
    //          floor
    //          skybox
    //    - texture
    //    - sequence
}
