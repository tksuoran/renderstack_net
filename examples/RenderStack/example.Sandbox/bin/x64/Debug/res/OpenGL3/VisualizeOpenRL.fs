//  VisualizeOpenRL.fs

#version 330

in  vec2 v_texcoord;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec4 sample     = texture(t_left, v_texcoord);

    out_color.rgb   = sample.rgb;
    out_color.a     = global.alpha;
}
