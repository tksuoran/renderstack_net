//  colorFill.fs

#version 330

in vec4 v_color;

uniform vec4  _global_add_color;
uniform float _alpha;

out vec4 out_color;

void main(void)
{
    out_color       = v_color;
    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
