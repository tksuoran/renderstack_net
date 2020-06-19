//  visualizeTangent.fs

#version 330

in vec3 v_tangent;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec3  T         = normalize(v_tangent);
    out_color.rgb   = 0.5 + 0.5 * T;
    out_color.rgb  *= global.alpha;
    out_color.a     = global.alpha;
    out_color.rgba += global.add_color;
}
