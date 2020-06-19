// shadowCaster.vs

#version 330 

in vec3 _position;
in vec3 _polygon_normal;
in vec3 _normal_smooth;

uniform mat4    _model_to_clip_matrix;
uniform mat4    _model_to_world_matrix;
uniform float   _bias_units;
uniform float   _bias_factor;

out         vec4    v_position;
out         vec3    v_normal;

void main()
{
    gl_Position     = _model_to_clip_matrix * vec4(_position, 1.0);
    vec4 N          = _model_to_clip_matrix * vec4(_normal_smooth, 0.0);
    gl_Position    += N * 0.1 * _bias_units;
    v_position      = gl_Position;
    v_position.z   -= 0.1 * _bias_factor;
    v_normal        = (_model_to_world_matrix * vec4(_polygon_normal, 0.0)).xyz;
}
