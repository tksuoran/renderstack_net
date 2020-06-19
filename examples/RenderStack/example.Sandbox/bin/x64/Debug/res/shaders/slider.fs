//  slider.fs

#version 330

uniform sampler2D   _texture;
uniform vec4        _global_add_color;
uniform float       _alpha;
uniform float       _cutoff;

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample     = texture2D(_texture, texcoord);
    out_color.rgb   = sample.rgb;// * v_color.rgb;
    out_color.a     = max(sample.r, sample.a);

    if(gl_FragCoord.x <= _cutoff)
    {
        out_color.rgb  += sample.a * _global_add_color.rgb;
    }
}
