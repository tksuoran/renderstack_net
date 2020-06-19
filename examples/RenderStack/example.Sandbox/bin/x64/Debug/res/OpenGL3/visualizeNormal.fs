//  visualizeNormal.fs

#version 330

in vec3 v_normal;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec3  N          = normalize(v_normal);
    out_color.rgb   = 0.5 + 0.5 * N;
    out_color.rgb  *= global.alpha;
    out_color.a     = global.alpha;
    out_color.rgba += global.add_color;
}
