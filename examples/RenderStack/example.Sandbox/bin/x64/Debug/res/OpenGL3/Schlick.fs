//  Schlick.fs

#version 330

in  vec3    v_view_direction;
in  vec3    v_color;
in  vec3    v_normal;
in  vec4    v_shadow[MAX_LIGHT_COUNT];

out vec4    out_color;

// foo

UNIFORMS;

FS_SHADOWS;

void main(void)
{
    float r             = material.surface_roughness;
    float one_minus_r   = 1.0 - r;

    vec3  N             = normalize(v_normal);
    vec3  V             = normalize(v_view_direction);

    float vn            = dot(V, N);
    float vn_clamped    = max(vn, 0.0);
    float GvnDenom      = r + one_minus_r * vn_clamped;     //  <=>  r + vn - (r * vn)  <=>  r - (r * vn) + vn

    vec3  surface_diffuse_reflectance  = material.surface_diffuse_reflectance_color.rgb;
    vec3  surface_specular_reflectance = material.surface_specular_reflectance_color.rgb;

    vec3  surface_diffuse_reflectance_linear    = surface_diffuse_reflectance * surface_diffuse_reflectance;
    vec3  surface_specular_reflectance_linear   = surface_specular_reflectance * surface_specular_reflectance;

    // S(HV) = C + (1 - C)(1 - VN)^5
    vec3 C = surface_diffuse_reflectance.rgb;
    vec3 white_minus_C = vec3(1.0, 1.0, 1.0) - C;
    vec3 S = C + white_minus_C * pow(1.0 - vn_clamped, 5.0);

    vec3 exitant_radiance = vec3(0.0);

#if 0
	// Cheap blinn-phong path
	//float specular_exponent = material.surface_specular_reflectance_exponent;
	float specular_exponent = one_minus_r * 300.0;
    for(int i = 0; i < lights.count; ++i)
    {
        vec3    L  = lights.direction[i].xyz;
        float   ln = dot(L, N);
        if(ln > 0.0)
        {
            vec3    H                       = normalize(L + V);
            float   ln_clamped              = max(ln, 0.0);
            float   hn                      = max(dot(H, N), 0.0);

            float   light_visibility        = sampleLightVisibility(v_shadow[i], float(i));
            vec3    light_color_linear      = lights.color[i].rgb * lights.color[i].rgb;
            vec3    light_radiance          = light_color_linear * lights.color[i].a;
            vec3    incident_radiance       = light_radiance * light_visibility;

            vec3    brdf_diffuse_component  = surface_diffuse_reflectance_linear;
            float   specular_term           = pow(hn, specular_exponent);
            vec3    brdf_specular_component = surface_specular_reflectance_linear * specular_term;
            vec3    brdf                    = brdf_diffuse_component + brdf_specular_component;

            exitant_radiance += brdf * incident_radiance * ln_clamped;
        }
    }
#else
	// proper schlick path
    for(int i = 0; i < lights.count; ++i)
    {
        vec3    L   = lights.direction[i].xyz;
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
            float D             = max((ln * r) / (GvnDenom * GlnDenom * Zhn0 * Zhn0), 0.0);

            float light_visibility      = sampleLightVisibility(v_shadow[i], float(i));
            vec3  light_color_linear    = lights.color[i].rgb * lights.color[i].rgb;
            vec3  light_radiance        = light_color_linear * lights.color[i].a;
            vec3  incident_radiance     = light_radiance * light_visibility;

            exitant_radiance += incident_radiance * surface_diffuse_reflectance_linear * ln_clamped;  //  diffuse
            exitant_radiance += incident_radiance * S * D;                                            //  specular
        }
    }
#endif

    vec3    ambient_light_color_linear = lights.ambient_light_color.rgb * lights.ambient_light_color.rgb;
    vec3    ambient_factor             = surface_diffuse_reflectance;
    //float   ambient_visibility         = min(v_color.r, 1.0 + min(N.y, 0.0));
    float   ambient_visibility         = 1.0 + min(N.y, 0.0);
    
    exitant_radiance                  += ambient_factor * ambient_light_color_linear * ambient_visibility;

    out_color.rgb = vec3(1.0) - exp(-exitant_radiance * lights.exposure.x);

    //out_color.rgb = sqrt(out_color.rgb);
    out_color.rgb = pow(out_color.rgb, vec3(1.0 / 2.2));

    out_color.a     = global.alpha;
    out_color.rgba += global.add_color;
    
    //out_color.rgb *= 0.01;
    //out_color.rgb += vec3(0.5) + 0.5 * N.xyz;
    //out_color.rgb *= 0.25;
    //out_color.rgb += vec3(0.5) + 0.5 * normalize(camera.view_position_in_world.xyz);
}
