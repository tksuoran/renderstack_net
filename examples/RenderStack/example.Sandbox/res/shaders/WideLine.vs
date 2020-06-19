//  wide_line.vs

#version 330

in vec3 _position;
in vec4 _edge_color;

out vec4 vs_color;
out vec4 v_color;

uniform mat4 _model_to_clip_matrix;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    vs_color    = _edge_color;
    v_color     = _edge_color;
}
