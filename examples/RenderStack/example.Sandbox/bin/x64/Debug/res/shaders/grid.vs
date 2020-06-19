//  grid.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _polygon_normal;
in vec3 _color;
in vec3 _id_vec3;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_world_matrix;
uniform mat4 _view_to_world_matrix;
uniform mat4 _model_to_shadow_matrix[3];
uniform vec3 _id_offset_vec3;

out vec2 v_texcoord;
out vec3 v_normal;
out vec3 v_view_direction;
out vec3 v_color;
out vec4 v_shadow[3];
flat out vec3 v_id_vec3;

void main()
{
    vec4 t;
    vec4 position;
    vec3 N;

    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    t           = _model_to_world_matrix * vec4(_position, 1.0);
    for(int i = 0; i < 3; ++i)
    {
        v_shadow[i] = _model_to_shadow_matrix[i] * vec4(_position, 1.0);
    }
    v_texcoord  = t.xz;
    position    = _model_to_world_matrix * vec4(_position, 1.0);
    N           = normalize(_normal);
    v_normal    = vec3(_model_to_world_matrix * vec4(N, 0.0));

    vec3 view_position_in_world = vec3(
        _view_to_world_matrix[3][0],
        _view_to_world_matrix[3][1],
        _view_to_world_matrix[3][2]
    );

    v_view_direction    = view_position_in_world.xyz - position.xyz;
    v_color             = _color;
    v_id_vec3           = _id_vec3 + _id_offset_vec3;
}
