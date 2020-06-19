//  VisualizeShadowMap.fs

#version 330

out     vec4            out_color;

uniform sampler2DArray  _shadowmap;
uniform vec4            _global_add_color;
uniform float           _alpha;
uniform float           _t;

in      vec2            v_texcoord;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample     = texture(_shadowmap, vec3(texcoord, _t));

    out_color.rgb   = sample.rrr;
    out_color.a     = max(sample.r, sample.a); 
    out_color.rgb  += sample.a * _global_add_color.rgb;
}
