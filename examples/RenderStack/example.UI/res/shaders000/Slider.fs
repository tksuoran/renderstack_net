//  Slider.fs

#version 120

varying vec2 v_texcoord;
varying vec4 v_color;

uniform sampler2D   _texture;
uniform vec4        _global_add_color;
uniform float       _alpha;
uniform float       _slider_t;

void main(void)
{
    vec2 texcoord       = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample         = texture2D(_texture, texcoord);
    gl_FragData[0].rgb  = sample.rgb;
    gl_FragData[0].a    = sample.a;

    if(gl_FragCoord.x <= _slider_t)
    {
        gl_FragData[0].rgb += sample.a * _global_add_color.rgb;
    }
}
