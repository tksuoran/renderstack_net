//  Grid.vs

#version 120

attribute vec3 _position;
attribute vec3 _normal;
attribute vec3 _polygon_normal;
attribute vec3 _color;

varying vec2 v_texcoord;
varying vec3 v_normal;
varying vec3 v_view_direction;
varying vec3 v_color;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_world_matrix;
uniform mat4 _view_to_world_matrix;
uniform mat4 _model_to_shadow_matrix[3];
uniform vec3 _id_offset_vec3;

void main()
{
    vec4 t;
    vec4 position;
    vec3 N;

    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    t           = _model_to_world_matrix * vec4(_position, 1.0);
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
}
