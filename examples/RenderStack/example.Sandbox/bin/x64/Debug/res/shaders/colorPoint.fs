//  colorPoint.fs

#version 330

uniform vec4  _point_color;

in vec4 v_color;

out vec4 out_color;

void main(void)
{
    out_color.rgb = v_color.rrr;
    out_color.a   = 1.0f;
}
