//  anachrome.fs

#version 330

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec2  texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4  sampleL    = texture2D(t_left, texcoord);
    vec4  sampleR    = texture2D(t_right, texcoord);

    float ly = 0.2125 * sampleL.r + 0.7154 * sampleL.g + 0.0721 * sampleL.b;
    float ry = 0.2125 * sampleR.r + 0.7154 * sampleR.g + 0.0721 * sampleR.b;

    sampleL.rgb = mix(vec3(ly, ly, ly), sampleL.rgb, material.saturation);
    sampleR.rgb = mix(vec3(ry, ry, ry), sampleR.rgb, material.saturation);

    vec4  accum;
    vec4  image = vec4(1.0, 1.0, 1.0, 1.0);
    float contrast = (material.contrast * 0.5) + 0.5;
    float l1 = contrast * 0.45;
    float l2 = (1.0 - l1) * 0.5;
    float r1 = contrast;
    float r2 = 1.0 - r1;
    float deghost = material.deghost * 0.1;

    accum = sampleL * vec4(l1, l2, l2, 1.0);
    image.r = pow(accum.r + accum.g + accum.b, 1.00);
    image.a = accum.a;

    accum = sampleR * vec4(r2, r1, 0.0, 1.0);
    image.g = pow(accum.r + accum.g + accum.b, 1.15);
    image.a = image.a + accum.a;

    accum = sampleR * vec4(r2, 0.0, r1, 1.0);
    image.b = pow(accum.r + accum.g + accum.b, 1.15);
    image.a = (image.a + accum.a) / 3.0;

    accum = image;
    image.r = (accum.r + (accum.r * (deghost        )) + (accum.g * (deghost * -0.50)) + (accum.b * (deghost * -0.50)));
    image.g = (accum.g + (accum.r * (deghost * -0.25)) + (accum.g * (deghost *  0.50)) + (accum.b * (deghost * -0.25)));
    image.b = (accum.b + (accum.r * (deghost * -0.25)) + (accum.g * (deghost * -0.25)) + (accum.b * (deghost *  0.50)));

    out_color = image;
}
