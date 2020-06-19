//  shadowMappedSpotBlinnPhong.fs

#version 330

out     vec4        out_color;

uniform sampler2D   _shadowmap;
in      vec4        v_shadow;

float lightVisibility(void)
{
    vec4    shadow      = v_shadow / v_shadow.w;
    float   distance    = shadow.z;
    vec2    moments     = texture2D(_shadowmap, shadow.xy).rg;

    //  Surface is fully lit. as the current fragment is before the light occluder
    if(distance <= moments.x)
    {
        return 1.0;
    }

    // The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
    // How likely this pixel is to be lit (p_max)
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, 0.00002);

    float d     = distance - moments.x;
    float p_max = variance / (variance + d * d);

    return p_max;
}

uniform vec3        _ambient_light_color;
uniform vec3        _light_color[3];
uniform float       _light_radiance[3];
uniform vec3        _light_direction[3];
uniform vec3        _light_position[3];
uniform vec3        _surface_diffuse_reflectance_color;
uniform vec3        _surface_specular_reflectance_color;
uniform float       _surface_specular_reflectance_exponent;
uniform sampler2D   _shadow_texture;
uniform float       _exposure;

uniform vec3        _surface_rim_color;
uniform vec4        _global_add_color;
uniform float       _alpha;

in      vec3        v_normal;
in      vec3        v_view_direction;
in      vec3        v_color;

void main(void)
{
    vec3  N = normalize(v_normal);
    vec3  V = normalize(v_view_direction);

    vec3  surface_diffuse_reflectance  = _surface_diffuse_reflectance_color.rgb;
    vec3  surface_specular_reflectance = _surface_specular_reflectance_color.rgb;

    vec3  exitant_radiance = vec3(0.0);

    for(int i = 0; i < 3; ++i)
    {
        vec3  L          = _light_direction[i].xyz;
        vec3  H          = normalize(L + V);
        float ln         = dot(L, N);
        float ln_clamped = max(ln, 0.0);
        float hn         = max(dot(H, N), 0.0);

        vec3  light_radiance       = _light_color[i].rgb * _light_radiance[i];
        vec3  light_visibility     = vec3(1.0, 1.0, 1.0);
        vec3  incident_radiance    = light_radiance * lightVisibility;

        vec3  brdf_diffuse_component  = surface_diffuse_reflectance;
        vec3  brdf_specular_component = vec3(0.0);
        if(ln > 0.0)
        {
            float specular_term = pow(hn, _surface_specular_reflectance_exponent);
            brdf_specular_component = surface_specular_reflectance * specular_term;
        }

        vec3 brdf  = brdf_diffuse_component + brdf_specular_component;

        exitant_radiance += brdf * incident_radiance * ln_clamped;
    }

    vec3 ambient_factor = surface_diffuse_reflectance;
    float ambient_visibility   = 1.0;  /*v_color.r; */
    exitant_radiance   += ambient_factor * _ambient_light_color.rgb * ambient_visibility;

    float vn         = dot(V, N);
    float vn_clamped = max(vn, 0.0);
    float rim        = 1.0 - vn_clamped;

    exitant_radiance += pow(rim, 4.0) * _surface_rim_color;

    out_color.rgb = vec3(1.0) - exp(-exitant_radiance * _exposure);

    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
