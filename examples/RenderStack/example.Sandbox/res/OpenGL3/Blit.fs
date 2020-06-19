//  Blit.fs

#version 330

in vec2 v_texcoord;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    out_color.rgba = texture2D(t_left, v_texcoord);
}
