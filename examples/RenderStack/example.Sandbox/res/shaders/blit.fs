//  blit.fs

#version 330

uniform sampler2D _texture;

in vec2 v_texcoord;

out vec4 out_color;

void main(void)
{
    out_color.rgba = texture2D(_texture, v_texcoord);
}
