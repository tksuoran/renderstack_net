// lightReflectionSimple.fs

#version 330

uniform vec3  _light_direction[3];
uniform vec3  _light_color[3];
uniform vec3  _surface_rim_color;
uniform vec3  _surface_color;
uniform vec4  _global_add_color;
uniform float _alpha;
uniform mat4 _view_to_world_matrix;

in vec3 v_normal;
in vec3 v_view_direction;
in vec4 v_color;

out vec4 out_color;

void main(void)
{
    vec3  N          = normalize(v_normal);
    vec3  L          = _light_direction[0].xyz;
    vec3  V          = normalize(v_view_direction);
    float ln         = dot(L, N);
    float vn         = dot(V, N);
    float ln_clamped = max(ln, 0.0);
    float vn_clamped = max(vn, 0.0);
    float rim        = 1.0 - vn_clamped;
	float rim2		 = rim * rim;
    
    out_color.rgb   = _surface_color * _light_color[0] * ln_clamped;
    out_color.rgb  += rim2 * rim2 * _surface_rim_color;
    out_color.rgb  *= _alpha;
    out_color.a     = _alpha;
    out_color.rgba += _global_add_color;
}
