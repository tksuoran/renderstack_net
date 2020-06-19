//  VisualizeShadowMap.fs

#version 330

in  vec3 v_texcoord;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec4 sample     = texture(t_shadowmap_vis, v_texcoord);

    out_color.rgb   = sample.rrr;
    out_color.a     = global.alpha;
}
