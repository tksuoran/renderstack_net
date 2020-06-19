//  Grid.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _polygon_normal;
in vec3 _color;

out vec2 v_texcoord;
out vec3 v_normal;
out vec3 v_view_direction;
out vec3 v_color;
out vec4 v_shadow[MAX_LIGHT_COUNT];

UNIFORMS;

void main()
{
    vec4 t;
    vec4 position;
    vec3 N;

    INSTANCING;

    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    t           = model_to_world_matrix * vec4(_position, 1.0);

    for(int i = 0; i < lights.count; ++i)
    {
        v_shadow[i] = lights.world_to_shadow_matrix[i] * model_to_world_matrix * vec4(_position, 1.0);
    }

    v_texcoord  = t.xz;
    position    = model_to_world_matrix * vec4(_position, 1.0);
    N           = normalize(_normal);
    v_normal    = vec3(model_to_world_matrix * vec4(N, 0.0));

    v_view_direction    = view_position_in_world.xyz - position.xyz;
    v_color             = _color;
}
