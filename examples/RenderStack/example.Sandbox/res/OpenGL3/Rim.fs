//  Rim.fs

#version 330

in vec3 v_normal;
in vec3 v_view_direction;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec3    N           = normalize(v_normal);
    vec3    V           = normalize(v_view_direction);
    float   vn          = dot(V, N);
    float   vn_clamped  = max(vn, 0.0);
    float   rim         = 1.0 - vn_clamped;
    float   rim5        = pow(rim, 0.5);
    float   rim8        = pow(rim, 0.8);
    float   rim58       = rim5 - rim8;
    vec3    rim_color   = rim * material.fill_color.rgb;

    //out_color.rgb   = rim5 * rim_color;
    //out_color.a     = rim5 - 0.2;

    vec3 inv_rim_color = vec3(1.0) - rim_color;

#if 0
    out_color.rgb   = 8.0 * vec3(rim58) * rim_color;
    out_color.a     = 0.3 + 0.3 * rim58; //13.5 * rim5 - 0.3;
#endif

#if 0
    out_color.rgb   = mix(vec3(rim5), rim_color, 1.0 - rim5);
    out_color.a     = rim5 - 0.2;
#endif
    float a = 5.5 * (pow(rim, 0.8) - pow(rim, 1.8));
    out_color.rgb   = vec3(0.2, 0.3, 0.5) + max(N.y, 0.0) * vec3(0.5, 0.5, 0.5);
    //out_color.a     = 1.0 - pow(vn_clamped, 0.4);
    out_color.a     = 0.8 * pow(vn_clamped, 5);
}
