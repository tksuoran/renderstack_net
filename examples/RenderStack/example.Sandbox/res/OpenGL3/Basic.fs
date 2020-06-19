//  Basic.fs

#version 330

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec4 sample     = texture2D(t_surface_color, v_texcoord);

    out_color.rgb   = sample.rgb * v_color.rgb;
    //out_color.rgb   = v_color.rgb;
    //out_color.rgb   = sample.rgb;
    out_color.a     = 1.0;//sample.a;
}
