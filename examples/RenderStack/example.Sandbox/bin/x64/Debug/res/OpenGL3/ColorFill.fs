//  ColorFill.fs

#version 330

in vec4 v_color;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    out_color.rgb = v_color.rgb * global.alpha;
    out_color.a = global.alpha;
}
