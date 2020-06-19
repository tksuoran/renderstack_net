//  Anisotropic.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _tangent;
in vec3 _color;

out vec3 v_normal;
out vec3 v_tangent;
out vec3 v_view_direction;
out vec4 v_shadow[MAX_LIGHT_COUNT];
out vec3 v_color;

UNIFORMS;

void main()
{
    INSTANCING;
    
    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    v_normal    = vec3(model_to_world_matrix * vec4(_normal, 0.0));
    v_tangent   = vec3(model_to_world_matrix * vec4(_tangent, 0.0));

    for(int i = 0; i < lights.count; ++i)
    {
        v_shadow[i] = lights.world_to_shadow_matrix[i] * model_to_world_matrix * vec4(_position, 1.0);
    }

    vec4 position = model_to_world_matrix * vec4(_position, 1.0);

    v_view_direction = view_position_in_world.xyz - position.xyz;
    v_color = _color;
}
