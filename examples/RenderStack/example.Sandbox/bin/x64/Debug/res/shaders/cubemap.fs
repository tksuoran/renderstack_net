// cubemap.fs

#version 330

uniform samplerCube _texture;

uniform vec4  _global_add_color;
uniform float _alpha;

in vec3 v_normal;
in vec3 v_view_direction;

out vec4 out_color;

void main(void)
{
    vec3  N             = normalize(v_normal);
    vec3  V             = normalize(v_view_direction);
    vec3  R             = reflect(-V, N);
    float vn            = dot(V, N);
    float vn_clamped    = max(vn, 0.0);
    //float rim           = 1.0 - vn_clamped;
    //float alpha         = _alpha * rim;

    float alpha         = _alpha;

    // For GLSL 120 use textureCube
    out_color.rgb   = texture(_texture, R).rgb;
    out_color.rgb  *= alpha;
    out_color.a     = alpha;
    out_color.rgba += _global_add_color;
    //out_color.rgb   = 0.5 + 0.5 * R;
}
