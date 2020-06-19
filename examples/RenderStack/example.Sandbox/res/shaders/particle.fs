//  particle.fs

#version 330

uniform sampler2D _texture;

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

void main(void)
{
    vec2 texcoord = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample = texture2D(_texture, texcoord);
    out_color.rgb = sample.rgb * v_color.rgb * v_color.aaa;
    out_color.a = v_color.a;
//    out_color.rgba = v_color.rgba;
}
