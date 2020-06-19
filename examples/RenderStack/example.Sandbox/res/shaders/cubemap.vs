// cubemap.vs

#version 330

in vec3 _position;
in vec3 _normal;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_world_matrix;
uniform mat4 _view_to_world_matrix;

out vec3 v_normal;
out vec3 v_view_direction;

void main()
{
    gl_Position   = _model_to_clip_matrix * vec4(_position, 1.0);
    v_normal      = vec3(_model_to_world_matrix * vec4(_normal, 0.0));
    vec4 position = _model_to_world_matrix * vec4(_position, 1.0);
    vec3 view_position_in_world = vec3(
        _view_to_world_matrix[3][0],
        _view_to_world_matrix[3][1],
        _view_to_world_matrix[3][2]
    );

    v_view_direction = view_position_in_world.xyz - position.xyz;
}
