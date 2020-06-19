
#version 330

in vec3 v_normal;
in vec3 v_view_direction;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec3  N             = normalize(v_normal);
    vec3  V             = normalize(v_view_direction);
    vec3  R             = reflect(-V, N);
    float vn            = dot(V, N);
    float vn_clamped    = max(vn, 0.0);

    out_color.rgb   = texture(t_cube, R).rgb;
    out_color.rgb  *= global.alpha;
    out_color.a     = global.alpha;
    out_color.rgba += global.global_add_color;
}
