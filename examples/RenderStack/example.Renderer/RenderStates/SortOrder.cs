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
