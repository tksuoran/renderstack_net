//  Anisotropic.fs

#version 330

out     vec4            out_color;

uniform sampler2DArray  _shadowmap;
uniform sampler3D       _jittermap;

in      vec4            v_shadow[3];

float sampleLightVisibility(vec4 coord, vec3 jcoord, float lightIndex)
{
    vec4    coordProj   = coord / coord.w;
    float   fragDepth   = coordProj.z;
    
    if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy, lightIndex)).x) return 1.0;
    return 0.0;
    
    float   visibility  = 0.0;

    float   scale               = 1.0 / float(textureSize(_shadowmap, 0).x);

    int     jitterSampleCount   = 8;
    int     sampleCount         = 8;
    int     halfSampleCount     = 4;
    float   invSampleCount      = 1.0 / 8.0;
    float   invHalfSampleCount  = 1.0 / 64.0;

    for(int i = 0; i < 4; ++i)
    {
        // For GLSL 120 use texture3D
        vec4 offset = texture(_jittermap, jcoord);
        jcoord.z += invHalfSampleCount;

        vec2 delta = offset.xy;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += 1.0 / 8.0;

        delta = offset.zw;
        if(fragDepth <= texture(_shadowmap, vec3(coordProj.xy + scale * delta, lightIndex)).x) visibility += 1.0 / 8.0;
    }

    return visibility;
}

uniform vec3    _ambient_light_color;
uniform vec3    _light_color[3];
uniform float   _light_radiance[3];
uniform vec3    _light_direction[3];
uniform vec3    _surface_diffuse_reflectance_color;
uniform vec3    _surface_specular_reflectance_color;
uniform float   _exposure;
uniform float   _surface_roughness;  /*  0.02  */
uniform float   _surface_isotropy;   /*  0.15  */

uniform vec4    _global_add_color;
uniform float   _alpha;

in      vec3    v_view_direction;
in      vec3    v_color;
in      vec3    v_normal;
in      vec3    v_tangent;

void main(void)
{
    float r             = _surface_roughness;
    float p             = _surface_isotropy;
    float p2            = p * p;
    float one_minus_r   = 1.0 - r;
    float one_minus_p2  = 1.0 - p2;

    vec3  N     = normalize(v_normal);
    vec3  B     = normalize(v_tangent);
    vec3  T     = cross(B, N);
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

    vec3 jcoord = vec3(gl_FragCoord.xy / 32.0, 0);

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
            float ht            = dot(H - hn * N, T);
            float hn2           = hn * hn;
            float ht2           = ht * ht;
            float AhtDenom      = sqrt(p2 + one_minus_p2 * ht2);

            float GlnDenom      = (r   + one_minus_r * ln);
            float Zhn0          = (1.0 - one_minus_r * hn * hn);
            float D             = (ln * r * p2) / (GvnDenom * GlnDenom * Zhn0 * Zhn0 * AhtDenom);

            float light_visibility      = sampleLightVisibility(v_shadow[i], jcoord, float(i));
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

    out_color.rgb = vec3(1.0) - exp(-exitant_radiance * _exposure);

    out_color.rgb = pow(out_color.rgb, vec3(1.0 / 2.2));

    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
