//  Schlick.fs

#version 120

varying vec3    v_normal;
uniform vec3    _surface_diffuse_reflectance_color;
uniform vec3    _surface_specular_reflectance_color;

void main(void)
{
    vec3  N             = v_normal;
    float d             = N.y * 0.5 + 0.5;
    float s             = N.z * N.z;
    gl_FragData[0].rgb  = (d * _surface_diffuse_reflectance_color) + (s * s * _surface_specular_reflectance_color);
    gl_FragData[0].a    = 1.0;
}
