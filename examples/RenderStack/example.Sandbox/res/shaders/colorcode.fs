//  colorcode.fs

#version 330

uniform sampler2D   _left;
uniform sampler2D   _right;
uniform float       _contrast;
uniform float       _deghost;
uniform float       _saturation;

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

float contrast_f(float img, float cst)
{
    float img1 = pow(img, cst);
    float img2 = 1.0 - pow(1.0 - img, cst);
    return mix(img1, img2, img);
}
void main(void)
{
    vec2  texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4  sampleL    = texture2D(_left, texcoord);
    vec4  sampleR    = texture2D(_right, texcoord);

    float ly = 0.2125 * sampleL.r + 0.7154 * sampleL.g + 0.0721 * sampleL.b;
    float ry = 0.2125 * sampleR.r + 0.7154 * sampleR.g + 0.0721 * sampleR.b;

    sampleL.rgb = mix(vec3(ly, ly, ly), sampleL.rgb, _saturation);
    sampleR.rgb = mix(vec3(ry, ry, ry), sampleR.rgb, _saturation);

    vec4  accum;
    vec4  image = vec4(1.0, 1.0, 1.0, 1.0);
    float contrast = (_contrast * 0.5) + 0.5;
    float l1 = contrast * 0.45;
    float l2 = (1.0 - l1) * 0.5;
    float r1 = contrast;
    float r2 = 1.0 - r1;
    float deghost = _deghost * 0.275;

    accum = sampleL * vec4(r1, 0.0, r2, 1.0);
    image.r = pow(accum.r + accum.g + accum.b, 1.05);
    image.a = accum.a;

    accum = sampleL * vec4(0.0, r1, r2, 1.0);
    image.g = pow(accum.r + accum.g + accum.b, 1.10);
    image.a = image.a + accum.a;

    accum = sampleR * vec4(l2, l2, l1, 1.0);
    image.b = pow(accum.r + accum.g + accum.b, 1.0);
    image.b = contrast_f(image.b, (deghost * 0.15) + 1.0);
    image.a = (image.a + accum.a) / 3.0;

    accum = image;
    image.r = (accum.r + (accum.r * (deghost *  1.50)) + (accum.g * (deghost * -0.75)) + (accum.b * (deghost * -0.75)));
    image.g = (accum.g + (accum.r * (deghost * -0.75)) + (accum.g * (deghost *  1.50)) + (accum.b * (deghost * -0.75)));
    image.b = (accum.b + (accum.r * (deghost * -1.50)) + (accum.g * (deghost * -1.50)) + (accum.b * (deghost *  3.00)));

    out_color = image;
}
