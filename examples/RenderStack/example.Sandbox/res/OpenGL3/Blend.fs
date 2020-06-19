//  blend.fs

#version 330

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sampleL    = texture2D(t_left, texcoord);
    vec4 sampleR    = texture2D(t_right, texcoord);

    float ly = 0.2125 * sampleL.r + 0.7154 * sampleL.g + 0.0721 * sampleL.b;
    float ry = 0.2125 * sampleR.r + 0.7154 * sampleR.g + 0.0721 * sampleR.b;

    sampleL.rgb = mix(vec3(ly, ly, ly), sampleL.rgb, material.saturation);
    sampleR.rgb = mix(vec3(ry, ry, ry), sampleR.rgb, material.saturation);

    out_color = mix(sampleL, sampleR, material.t);
}
