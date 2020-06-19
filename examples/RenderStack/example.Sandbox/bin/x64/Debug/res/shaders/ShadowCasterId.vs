// ShadowCasterId.vs

#version 330 

in vec3 _position;
in vec3 _polygon_normal;
in vec3 _normal_smooth;
in vec3 _id_vec3;

uniform mat4    _model_to_clip_matrix;
uniform mat4    _model_to_world_matrix;
uniform float   _bias_units;
uniform float   _bias_factor;
uniform vec3    _id_offset_vec3;

flat out vec3 v_id_vec3;

void main()
{
    gl_Position     = _model_to_clip_matrix * vec4(_position, 1.0);
    vec4 N          = _model_to_clip_matrix * vec4(_normal_smooth, 0.0);
    gl_Position    += N * 0.1 * _bias_units;
    v_id_vec3   = _id_vec3 + _id_offset_vec3;
}
