//  VisualizeStencil.fs

#version 330

in  vec2 v_texcoord;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    float s = texture(t_left, v_texcoord).x;

    out_color.rgb = vec3(s * 255.5 / 3.0);
    out_color.a   = global.alpha;
}
