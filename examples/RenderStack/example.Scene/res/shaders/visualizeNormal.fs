//  visualizeNormal.fs

#version 120

uniform vec4  _global_add_color;
uniform float _alpha;

varying vec3 v_normal;

void main(void)
{
    vec3 N = normalize(v_normal);
    gl_FragData[0].rgb   = 0.5 + 0.5 * N;
    gl_FragData[0].rgb  *= _alpha;
    gl_FragData[0].a     = _alpha;
    gl_FragData[0].rgba += _global_add_color;
}
