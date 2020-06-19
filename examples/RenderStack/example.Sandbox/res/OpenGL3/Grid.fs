//  Grid.fs

#version 330

in      vec4    v_shadow[MAX_LIGHT_COUNT];
in      vec3    v_view_direction;
in      vec3    v_color;
in      vec2    v_texcoord;
in      vec3    v_normal;

out     vec4    out_color;

UNIFORMS;
FS_SHADOWS;

#if 1

float gridFS(void)
{
    vec2 frequency   = material.grid_size.xy;
    vec2 texcoord    = vec2(v_texcoord.x, v_texcoord.y);
    vec2 checkPos    = fract(texcoord * frequency);
    vec2 p = step(0.5, checkPos);

    return p.x * p.y + (1.0 - p.x) * (1.0 - p.y);
}

#else

float gridFS(void)
{
    vec4    grid_color;
    vec2    frequency   = material.grid_size.xy;
    vec2    epsilon2    = vec2(0.00004);
    float   color1      = 0.0;
    float   color2      = 1.0;
    float   avgColor    = 0.5;

    vec2    texcoord    = vec2(v_texcoord.x, v_texcoord.y);
    vec2    fw          = sqrt(dFdx(texcoord) * dFdx(texcoord) + dFdy(texcoord) * dFdy(texcoord));
    vec2    fuzz        = fw * frequency * 1.0;
    float   fuzzMax     = max(fuzz.s, fuzz.t);

    vec2    checkPos    = fract(texcoord * frequency - epsilon2);
    float   color;

    if(fuzzMax < 0.5)
    {
        vec2 a = smoothstep(
            vec2(0.5), 
            fuzz + vec2(0.5), 
            checkPos
        );
        vec2 b = smoothstep(vec2(0.0), fuzz, checkPos);
        vec2 p = a + (1.0 - b);

        color = mix(color1, color2, p.x * p.y + (1.0 - p.x) * (1.0 - p.y));
        color = mix(color, avgColor, smoothstep(0.125, 0.5, fuzzMax));
    }
    else
    {
        color = avgColor;
    }

    return color;
}

#endif

void main(void)
{
    vec4  blinn_phong_color;
    vec3  N = normalize(v_normal);
    vec3  V = normalize(v_view_direction);

    float g = gridFS();
    float grid = 0.9 + 0.1 * g;
    vec3  surface_diffuse_reflectance  = material.surface_diffuse_reflectance_color.rgb * grid;
    vec3  surface_specular_reflectance = 0.7 * (material.surface_specular_reflectance_color.rgb - vec3(0.33 * g));

    vec3  surface_diffuse_reflectance_linear    = surface_diffuse_reflectance * surface_diffuse_reflectance;
    vec3  surface_specular_reflectance_linear   = surface_specular_reflectance * surface_specular_reflectance;

    vec3  exitant_radiance = vec3(0.0);

    float specular_exponent = material.surface_specular_reflectance_exponent;

#if 1
	// blinn phong shading
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
	// only one shadow with full diffuse color/no shading
	exitant_radiance = sampleLightVisibility(v_shadow[0], float(0)) * surface_diffuse_reflectance_linear;
#endif

    vec3 ambient_light_color_linear = lights.ambient_light_color.rgb * lights.ambient_light_color.rgb;
    vec3 ambient_factor = surface_diffuse_reflectance_linear;
    float ambient_visibility = 1.0;
    exitant_radiance += ambient_factor * ambient_light_color_linear * ambient_visibility;

    blinn_phong_color.rgb = vec3(1.0) - exp(-exitant_radiance * lights.exposure.x);

    out_color.rgb = pow(blinn_phong_color.rgb, vec3(1.0 / 2.2));
    //out_color.r = 1.0;

    //out_color.rgb  += material.surface_diffuse_reflectance_color.rgb;
    //out_color.rgb  += lights.color[0].rgb;
    //out_color.rgb  *= 0.01;
    //out_color.rg   = surface_diffuse_reflectance.rg;
    //out_color.rgb  += lights.exposure.xxx;
    //out_color.rgb  *= global.alpha;
    //out_color.a     = global.alpha;
    //out_color.rgba += global.add_color;
}
