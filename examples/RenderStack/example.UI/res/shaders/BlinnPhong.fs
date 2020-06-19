//  BlinnPhong.fs

#version 120

uniform vec3    _ambient_light_color;
uniform vec3    _light_color[3];
uniform float   _light_radiance[3];
uniform vec3    _light_direction[3];
uniform vec3    _surface_diffuse_reflectance_color;
uniform vec3    _surface_specular_reflectance_color;
uniform float   _surface_specular_reflectance_exponent;
uniform float   _exposure;

uniform vec4    _global_add_color;
uniform float   _alpha;

varying vec3    v_view_direction;
varying vec3    v_color;
varying vec3    v_normal;
varying vec3    v_normal_smooth;

void main(void)
{
    vec3  N     = normalize(v_normal);
    vec3  Ns    = normalize(v_normal_smooth);
    vec3  V     = normalize(v_view_direction);

    vec3  surface_diffuse_reflectance  = v_color * _surface_diffuse_reflectance_color.rgb;
    vec3  surface_specular_reflectance = _surface_specular_reflectance_color.rgb;

    vec3  surface_diffuse_reflectance_linear    = surface_diffuse_reflectance * surface_diffuse_reflectance;
    vec3  surface_specular_reflectance_linear   = surface_specular_reflectance * surface_specular_reflectance;

    vec3  exitant_radiance = vec3(0.0);

    for(int i = 0; i < 3; ++i)
    {
        vec3  L  = _light_direction[i].xyz;
        float ln = dot(L, N);
        if(ln > 0.0)
        {
            vec3    H                       = normalize(L + V);
            float   ln_clamped              = max(ln, 0.0);
            float   hn                      = max(dot(H, N), 0.0);
            vec3    light_color_linear      = _light_color[i].rgb * _light_color[i].rgb;
            float   light_visibility        = 1.0;
            vec3    light_radiance          = light_color_linear * _light_radiance[i];
            vec3    incident_radiance       = light_radiance * light_visibility;
            vec3    brdf_diffuse_component  = surface_diffuse_reflectance_linear;
            float   specular_term           = pow(hn, _surface_specular_reflectance_exponent);
            vec3    brdf_specular_component = surface_specular_reflectance_linear * specular_term;
            vec3    brdf = brdf_diffuse_component + brdf_specular_component;

            exitant_radiance += brdf * incident_radiance * ln_clamped;
        }
    }
    

    vec3    ambient_factor      = surface_diffuse_reflectance;
    float   ambient_visibility  = 0.5;
    exitant_radiance           += ambient_factor * _ambient_light_color.rgb * ambient_visibility;

    gl_FragData[0].rgb = vec3(1.0) - exp(-exitant_radiance * _exposure);

    gl_FragData[0].rgb = sqrt(gl_FragData[0].rgb);

    gl_FragData[0].rgb  *= _alpha;
    gl_FragData[0].a     = _alpha;
    gl_FragData[0].rgba += _global_add_color;
    
    //out_color.rgb  *= 0.001;
    //vec4    coord = v_shadow[0] / v_shadow[0].w;
    //out_color.rg += depthGradient(coord.xy, coord.z);
}
