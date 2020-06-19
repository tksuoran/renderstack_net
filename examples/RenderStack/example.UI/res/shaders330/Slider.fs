//  Slider.fs

#version 330

in      vec2        v_texcoord;
in      vec4        v_color;

out     vec4        out_color;

uniform sampler2D   _texture;
uniform vec4        _global_add_color;
uniform float       _alpha;
uniform float       _slider_t;

void main(void)
{
    vec2 texcoord  = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample    = texture2D(_texture, texcoord);
    out_color.rgb  = sample.rgb;
    out_color.a    = sample.a;

    if(gl_FragCoord.x <= _slider_t)
    {
        out_color.rgb += sample.a * _global_add_color.rgb;
    }
}
