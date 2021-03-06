﻿//  Slider.fs

#version 330

in  vec2 v_texcoord;
in  vec4 v_color;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec2 texcoord  = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample    = texture2D(t_ninepatch, texcoord);
    out_color.rgb  = sample.rgb;
    out_color.a    = sample.a;

    if(gl_FragCoord.x <= material.t)
    {
        out_color.rgb += sample.a * global.add_color.rgb;
    }
}
