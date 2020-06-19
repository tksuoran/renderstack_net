//  t_float.vs

#version 330

in vec3 _position;
in float _t;

out vec4 v_t;

uniform mat4 _model_to_clip_matrix;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_t         = vec4(_t, _t, _t, _t);
}
