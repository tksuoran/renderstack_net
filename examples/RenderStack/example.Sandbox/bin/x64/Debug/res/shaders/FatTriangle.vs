//  fat_triangle.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _polygon_normal;

out vec3 g_position;
out vec3 g_normal;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_view_matrix;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    g_position  = vec3(_model_to_view_matrix * vec4(_position,          1.0));
    g_normal    = vec3(_model_to_view_matrix * vec4(_polygon_normal,    0.0));
}
