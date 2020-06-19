// visualizeCubemap.fs

#version 330

in vec3 v_texcoord;

out vec4 out_color;

void main(void)
{
    out_color.rgb   = texture(_cube_texture, v_texcoord).rgb;
    out_color.a     = 1.0;
}
