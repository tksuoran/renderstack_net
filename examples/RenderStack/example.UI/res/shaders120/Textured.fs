//  Textured.fs

#version 120

varying vec2 v_texcoord;

uniform sampler2D   _texture;
uniform vec4        _global_add_color;
uniform float       _alpha;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample     = texture2D(_texture, texcoord);

    gl_FragData[0].rgb   = sample.rgb;
    gl_FragData[0].a     = sample.a;
    gl_FragData[0].rgb  += sample.a * _global_add_color.rgb;
}

