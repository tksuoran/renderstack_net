//  visualizeTangent.fs

#version 330

uniform vec4  _global_add_color;
uniform float _alpha;

in vec3 v_tangent;

out vec4 out_color;

void main(void)
{
    vec3  T         = normalize(v_tangent);
    out_color.rgb   = 0.5 + 0.5 * T;
    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
