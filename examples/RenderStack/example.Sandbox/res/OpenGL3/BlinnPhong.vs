//  lightReflectionBlinnPhong.vs

#version 330

in      vec3    _position;
in      vec3    _normal;
in      vec3    _normal_smooth;
in      vec3    _color;

out     vec3    v_normal;
out     vec3    v_normal_smooth;
out     vec3    v_view_direction;
out     vec3    v_color;
out     vec4    v_shadow[3];

UNIFORMS;

void main()
{
    gl_Position     = model_to_clip_matrix * vec4(_position, 1.0);
    vec4 position   = model_to_world_matrix * vec4(_position, 1.0);
    
    for(int i = 0; i < MAX_LIGHT_COUNT; ++i)
    {
        v_shadow[i] = world_to_shadow_matrix[i] * model_to_world_matrix * vec4(_position, 1.0);
    }

    v_normal        = vec3(model_to_world_matrix * vec4(_normal, 0.0));
    v_normal_smooth = vec3(model_to_world_matrix * vec4(_normal_smooth, 0.0));

    vec3 view_position_in_world = vec3(
        view_to_world_matrix[3][0],
        view_to_world_matrix[3][1],
        view_to_world_matrix[3][2]
    );

    v_view_direction    = view_position_in_world.xyz - position.xyz;
    v_color             = _color;
}
