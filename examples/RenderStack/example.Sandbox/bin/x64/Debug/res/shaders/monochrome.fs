//  monochrome.fs

#version 330

uniform sampler2D   _texture;
uniform vec4        _global_add_color;
uniform float       _alpha;
uniform float       _saturation;

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample     = texture2D(_texture, texcoord);
    
    float y = 0.2125 * sample.r + 0.7154 * sample.g + 0.0721 * sample.b;
    //out_color.rgb   = sample.rgb * v_color.rgb;
    out_color.rgb   = 1.0 * y * v_color.rgb;
    //out_color.rgb   += 0.4 * sample.rgb * v_color.rgb;
    out_color.a     = max(sample.r, sample.a); 
    out_color.rgb  += sample.a * _global_add_color.rgb;
}
