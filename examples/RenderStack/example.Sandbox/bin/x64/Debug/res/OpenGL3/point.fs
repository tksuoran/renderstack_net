// point.fs

#version 330

out vec4 out_color;

UNIFORMS;

void main(void)
{
    out_color.rgba = material.point_color;
}
