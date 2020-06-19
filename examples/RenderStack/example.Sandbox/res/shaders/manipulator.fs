﻿// manipulator.fs

#version 330

//uniform vec3  _surface_rim_color;
uniform vec3  _surface_color;
uniform vec4  _global_add_color;
uniform float _alpha;
uniform mat4 _view_to_world_matrix;

in vec3 v_normal;
in vec3 v_view_direction;

out vec4 out_color;

void main(void)
{
    vec3  N          = normalize(v_normal);
    vec3  V          = normalize(v_view_direction);
    float vn         = dot(V, N);
    float vn_clamped = max(vn, 0.0);
    //float rim        = 1.0 - vn_clamped;
    
    out_color.rgb   = _surface_color * (0.5 * vn_clamped + vec3(0.4));
    out_color.rgb = pow(out_color.rgb, vec3(1.0 / 2.2));
    //out_color.rgb  += rim * _surface_rim_color;
    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
