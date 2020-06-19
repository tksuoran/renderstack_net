//  blend.fs

#version 330

uniform sampler2D   _left;
uniform sampler2D   _right;
uniform float       _t;
uniform float       _saturation;

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sampleL    = texture2D(_left, texcoord);
    vec4 sampleR    = texture2D(_right, texcoord);

    float ly = 0.2125 * sampleL.r + 0.7154 * sampleL.g + 0.0721 * sampleL.b;
    float ry = 0.2125 * sampleR.r + 0.7154 * sampleR.g + 0.0721 * sampleR.b;

    sampleL.rgb = mix(vec3(ly, ly, ly), sampleL.rgb, _saturation);
    sampleR.rgb = mix(vec3(ry, ry, ry), sampleR.rgb, _saturation);

    out_color = mix(sampleL, sampleR, _t);
}
