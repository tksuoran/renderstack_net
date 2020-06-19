// point.fs

#version 330

uniform vec4  _point_color;

out vec4 out_color;

void main(void)
{
    out_color.rgba = _point_color;
}
