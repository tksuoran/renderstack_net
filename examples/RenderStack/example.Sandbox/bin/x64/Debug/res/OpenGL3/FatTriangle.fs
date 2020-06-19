//  fat_triangle.fs

//  Fragment shader for rendering wide lines
//  with GL 3.3+ with forward compatible context.
//
//  To be used with fat_triangle.gs (same as line.gs) geometry shader.
//
//  Not optimized.
//
//  Timo Suoranta - tksuoran@gmail.com

#version 330

in  vec2    v_start;
in  vec2    v_line;
in  float   v_l2;

out vec4    out_color;

UNIFORMS;

void main(void)
{
    float   t           = dot(gl_FragCoord.xy - v_start, v_line) / v_l2;
    vec2    projection  = v_start + clamp(t, 0.0, 1.0) * v_line;
    vec2    delta       = gl_FragCoord.xy - projection;
    float   d2          = dot(delta, delta);

    float   k           = clamp(material.line_width.y - d2, 0.0, 1.0);      //k = pow(k, 0.416666666);
    float   endWeight   = step(abs(t * 2.0 - 1.0), 1);
    float   alpha       = mix(k, 1.0, endWeight);
    out_color = vec4(material.line_color.rgb, alpha);

#if 0
    float d = length(delta);
    float k = (d / (0.5 * material.line_width.x));

    out_color = vec4(k, k, k, 1.0);
#endif
}
