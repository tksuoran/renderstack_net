//  Schlick.fs

#version 120

uniform vec3    _ambient_light_color;
uniform vec3    _light_color[3];
uniform float   _light_radiance[3];
uniform vec3    _light_direction[3];
uniform vec3    _surface_diffuse_reflectance_color;
uniform vec3    _surface_specular_reflectance_color;
uniform float   _surface_specular_reflectance_exponent;
uniform float   _exposure;
uniform float   _surface_roughness;  /*  0.02  */
uniform float   _t;

uniform vec4    _global_add_color;
uniform float   _alpha;

varying vec3    v_view_direction;
varying vec3    v_color;
varying vec3    v_normal;

void main(void)
{
    float r             = _surface_roughness;
    float one_minus_r   = 1.0 - r;

    vec3  N     = normalize(v_normal);
    vec3  V     = normalize(v_view_direction);

    float vn            = dot(V, N);
    float vn_clamped    = max(vn, 0.0);
    float GvnDenom      = r + one_minus_r * vn;     //  <=>  r + vn - (r * vn)  <=>  r - (r * vn) + vn

    vec3  surface_diffuse_reflectance  = v_color * _surface_diffuse_reflectance_color.rgb;
    vec3  surface_specular_reflectance = v_color * _surface_specular_reflectance_color.rgb;

    vec3  surface_diffuse_reflectance_linear    = surface_diffuse_reflectance * surface_diffuse_reflectance;
    vec3  surface_specular_reflectance_linear   = surface_specular_reflectance * surface_specular_reflectance;

    // S(HV) = C + (1 - C)(1 - VN)^5
    vec3 C = surface_diffuse_reflectance.rgb;
    vec3 white_minus_C = vec3(1.0, 1.0, 1.0) - C;
    vec3 S = C + white_minus_C * pow(1.0 - vn_clamped, 5.0);

    vec3 exitant_radiance = vec3(0.0);

    for(int i = 0; i < 3; ++i)
    {
        vec3    L   = _light_direction[i].xyz;
        float   ln  = dot(L, N);

        if(ln > 0.0)
        {
            float ln_clamped    = max(ln, 0.0);
            vec3  H             = normalize(L + V);
            float hn            = dot(H, N);
            float hv            = dot(H, V);
            float hn_clamped    = max(hn, 0.0);
            float hv_clamped    = max(hv, 0.0);
            float hn2           = hn * hn;

            float GlnDenom      = (r   + one_minus_r * ln);
            float Zhn0          = (1.0 - one_minus_r * hn * hn);
            float D             = (ln * r) / (GvnDenom * GlnDenom * Zhn0 * Zhn0);

            float light_visibility      = 1.0;
            vec3  light_color_linear    = _light_color[i].rgb * _light_color[i].rgb;
            vec3  light_radiance        = light_color_linear * _light_radiance[i];
            vec3  incident_radiance     = light_radiance * light_visibility;

            exitant_radiance += incident_radiance * surface_diffuse_reflectance_linear * ln_clamped;   //  diffuse
            exitant_radiance += incident_radiance * S * D;                                          //  specular
        }
    }

    vec3    ambient_light_color_linear = _ambient_light_color.rgb * _ambient_light_color.rgb;
    vec3    ambient_factor             = surface_diffuse_reflectance;
    float   ambient_visibility         = 1.0;
    exitant_radiance                  += ambient_factor * ambient_light_color_linear * ambient_visibility;

    gl_FragData[0].rgb = vec3(1.0) - exp(-exitant_radiance * _exposure);

    gl_FragData[0].rgb = sqrt(gl_FragData[0].rgb);

    gl_FragData[0].rgb  *= _alpha;
    gl_FragData[0].a     = _alpha;
    gl_FragData[0].rgba += _global_add_color;
}
