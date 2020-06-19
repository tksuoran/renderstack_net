//  visualizeNormal.fs

#version 330

uniform vec4  _global_add_color;
uniform float _alpha;

in vec3 v_normal;

out vec4 out_color;

void main(void)
{
    vec3  N          = normalize(v_normal);
    out_color.rgb   = 0.5 + 0.5 * N;
    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
