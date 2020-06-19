//  Grid.fs

#version 120

varying vec2    v_texcoord;
varying vec3    v_normal;

uniform vec2    _grid_size;
uniform vec4    _global_add_color;
uniform vec3    _ambient_light_color;
uniform vec3    _light_color[3];
uniform float   _light_radiance[3];
uniform vec3    _light_direction[3];
uniform vec3    _surface_diffuse_reflectance_color;
uniform float   _alpha;

float grid(void)
{
    vec2 frequency   = _grid_size;
    vec2 texcoord    = vec2(v_texcoord.x, v_texcoord.y);
    vec2 checkPos    = fract(texcoord * frequency);
    vec2 p = step(0.5, checkPos);

    return p.x * p.y + (1.0 - p.x) * (1.0 - p.y);
}

void main(void)
{
    vec3  N         = v_normal;
    float grid      = 0.9 + 0.1 * grid();
    float d         = N.y * 0.5 + 0.5;

    gl_FragData[0].rgb = d * grid * _surface_diffuse_reflectance_color;

    gl_FragData[0].rgb  *= _alpha;
    gl_FragData[0].a     = _alpha;
    gl_FragData[0].rgba += _global_add_color;
}
