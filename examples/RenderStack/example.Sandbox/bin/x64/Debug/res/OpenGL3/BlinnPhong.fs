//  BlinnPhong.fs

#version 330

out     vec4            out_color;

uniform sampler2DArray  _shadowmap;
uniform sampler3D       _jittermap;

in      vec4            v_shadow[3];

// Derivatives of light-space depth with respect to texture coordinates
vec2 depthGradient(vec2 uv, float z)
{
    vec2 dz_duv = vec2(0);

    vec3 duvdist_dx = dFdx(vec3(uv, z));
    vec3 duvdist_dy = dFdy(vec3(uv, z));

    dz_duv.x  = duvdist_dy.y * duvdist_dx.z;
    dz_duv.x -= duvdist_dx.y * duvdist_dy.z;
    
    dz_duv.y  = duvdist_dx.x * duvdist_dy.z;
    dz_duv.y -= duvdist_dy.x * duvdist_dx.z;

    float det = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
    dz_duv /= det;

    return dz_duv;
}

float sampleLightVisibility(vec4 coord, vec3 jcoord, float lightIndex, float ln)
{
    vec4    coordProj   = coord / coord.w;
    float   fragDepth   = coordProj.z;
    
    if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy, lightIndex)).x) return 1.0;
    return 0.0;
    
    float   visibility  = 0.0;

    float   scale               = 0.5 / float(textureSize(_shadowmap, 0).x);

    int     sampleCount         = 4;
    int     halfSampleCount     = sampleCount / 2;
    float   invSampleCount      = 1.0 / sampleCount;
    float   invHalfSampleCount  = 1.0 / 64.0;

    vec3 jcoord2 = jcoord;
    jcoord *= 0.0;
    vec4 offset;
    vec2 delta;
    
    vec2 g = depthGradient(coordProj.xy, coordProj.z);
    for(int i = 0; i < sampleCount / 4; ++i)
    {
        // For GLSL 120 use texture3D
        offset = texture(_jittermap, jcoord);
        jcoord.z += invHalfSampleCount;
        
        delta = g * offset.xy;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;

        delta = g * offset.zw;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;

        // For GLSL 120 use texture3D
        offset = texture(_jittermap, jcoord2);
        jcoord2.z += invHalfSampleCount;
        coord.z += invHalfSampleCount;
        
        delta = g * offset.xy;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;
        
        delta = g * offset.zw;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += invSampleCount;
    }

    return visibility;
}

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

in      vec3    v_view_direction;
in      vec3    v_color;
in      vec3    v_normal;
in      vec3    v_normal_smooth;

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

    vec3  jcoord = vec3(gl_FragCoord.xy / 32.0, 0);

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
            float   light_visibility        = sampleLightVisibility(v_shadow[i], jcoord, float(i), dot(L, Ns));
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

    out_color.rgb = vec3(1.0) - exp(-exitant_radiance * _exposure);

    out_color.rgb = sqrt(out_color.rgb);

    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
    
    //out_color.rgb  *= 0.001;
    //vec4    coord = v_shadow[0] / v_shadow[0].w;
    //out_color.rg += depthGradient(coord.xy, coord.z);
}
