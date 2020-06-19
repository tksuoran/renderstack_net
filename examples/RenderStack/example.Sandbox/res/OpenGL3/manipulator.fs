// manipulator.fs

#version 330

in vec3 v_normal;
in vec3 v_view_direction;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec3  N          = normalize(v_normal);
    vec3  V          = normalize(v_view_direction);
    float vn         = dot(V, N);
    float vn_clamped = max(vn, 0.0);

    out_color.rgb   = material.surface_diffuse_reflectance_color.rgb * (0.5 * vn_clamped + vec3(0.5));
    out_color.rgb = pow(out_color.rgb, vec3(1.0 / 2.2));
    out_color.rgb  *= global.alpha;
    out_color.a     = global.alpha;
    out_color.rgba += global.add_color;
}
