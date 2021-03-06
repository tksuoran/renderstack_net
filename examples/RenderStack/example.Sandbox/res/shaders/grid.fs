﻿//  grid.fs

#version 330

out     vec4            out_color;

uniform sampler2DArray  _shadowmap;
uniform sampler3D       _jittermap;
uniform vec4            _viewport;
in      vec4            v_shadow[3];


float sampleLightVisibility(vec4 coord, vec3 jcoord, float lightIndex)
{
    vec4    coordProj   = coord / coord.w;
    float   fragDepth   = coordProj.z;
    
    if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy, lightIndex)).x) return 1.0;
    return 0.0;
    
    float   visibility  = 0.0;

    float   scale               = 1.0 / float(textureSize(_shadowmap, 0).x);

    int     sampleCount         = 4;
    int     halfSampleCount     = sampleCount / 2;
    float   invSampleCount      = 1.0 / sampleCount;
    float   invHalfSampleCount  = 1.0 / 64.0;

    vec3 jcoord2 = jcoord;
    jcoord *= 0.0;
    vec4 offset;
    vec2 delta;
    for(int i = 0; i < sampleCount / 4; ++i)
    {
        // For GLSL 120 use texture3D
        offset = texture(_jittermap, jcoord);
        jcoord.z += invHalfSampleCount;
        
        delta = offset.xy;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;
        
        delta = offset.zw;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;

        // For GLSL 120 use texture3D
        offset = texture(_jittermap, jcoord2);
        jcoord2.z += invHalfSampleCount;
        coord.z += invHalfSampleCount;
        
        delta = offset.xy;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;
        
        delta = offset.zw;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;
    }

    return visibility;
}

uniform vec2    _grid_size;
uniform vec4    _global_add_color;
in      vec2    v_texcoord;
in      vec3    v_normal;

float gridFS(void)
{
    vec4    grid_color;
    vec2    frequency   = _grid_size;
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

uniform vec3    _ambient_light_color;
uniform vec3    _light_color[3];
uniform float   _light_radiance[3];
uniform vec3    _light_direction[3];
uniform vec3    _surface_diffuse_reflectance_color;
uniform vec3    _surface_specular_reflectance_color;
uniform float   _surface_specular_reflectance_exponent;
uniform float   _exposure;

uniform float   _alpha;

in      vec3    v_view_direction;
in      vec3    v_color;

void main(void)
{
    vec4  blinn_phong_color;
    vec3  N = normalize(v_normal);
    vec3  V = normalize(v_view_direction);

    float g = gridFS();
    float grid = 0.9 + 0.1 * g;
    vec3  surface_diffuse_reflectance  = _surface_diffuse_reflectance_color.rgb * grid;
    vec3  surface_specular_reflectance = 0.7 * (_surface_specular_reflectance_color.rgb - vec3(0.33 * g));

    vec3  surface_diffuse_reflectance_linear    = surface_diffuse_reflectance * surface_diffuse_reflectance;
    vec3  surface_specular_reflectance_linear   = surface_specular_reflectance * surface_specular_reflectance;

    vec3  exitant_radiance = vec3(0.0);

    vec3 jcoord = vec3(gl_FragCoord.xy / 32.0, 0);

    float specular_exponent = _surface_specular_reflectance_exponent;

    for(int i = 0; i < 3; ++i)
    {
        vec3    L  = _light_direction[i].xyz;
        float   ln = dot(L, N);
        if(ln > 0.0)
        {
            vec3    H                       = normalize(L + V);
            float   ln_clamped              = max(ln, 0.0);
            float   hn                      = max(dot(H, N), 0.0);
            vec3    light_color_linear      = _light_color[i].rgb * _light_color[i].rgb;
            float   light_visibility        = sampleLightVisibility(v_shadow[i], jcoord, float(i));
            vec3    light_radiance          = light_color_linear * _light_radiance[i];
            vec3    incident_radiance       = light_radiance * light_visibility;
            vec3    brdf_diffuse_component  = surface_diffuse_reflectance_linear;
            float   specular_term           = pow(hn, specular_exponent);
            vec3    brdf_specular_component = surface_specular_reflectance_linear * specular_term;
            vec3    brdf                    = brdf_diffuse_component + brdf_specular_component;

            exitant_radiance += brdf * incident_radiance * ln_clamped;
        }
    }

    vec3 ambient_light_color_linear = _ambient_light_color.rgb * _ambient_light_color.rgb;
    vec3 ambient_factor = surface_diffuse_reflectance_linear;
    float ambient_visibility = 1.0;
    exitant_radiance += ambient_factor * ambient_light_color_linear * ambient_visibility;

    blinn_phong_color.rgb = vec3(1.0) - exp(-exitant_radiance * _exposure);

    out_color.rgb = pow(blinn_phong_color.rgb, vec3(1.0 / 2.2));

    float scale = 1.0 / 4.0;
    vec3 coord =  vec3(jcoord.xy * scale, 0.0);
    // For GLSL 120 use texture3D
    vec4 offset = texture(_jittermap, coord);
    out_color.rgb  *= _alpha;
    //out_color.rgb  *= 0.01;
    //out_color.rg  += offset.xz;
    //out_color.r      += jcoord.x / _viewport.z;
    //out_color.g      += jcoord.y / _viewport.w;
    //out_color.rgb   += coord;
    //out_color.r     += offset.w;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
