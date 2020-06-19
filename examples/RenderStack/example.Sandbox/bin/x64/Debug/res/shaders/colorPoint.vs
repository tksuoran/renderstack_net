//  colorPoint.vs

#version 330

in vec3 _position;
in vec4 _color;

out vec4 v_color;

uniform mat4 _model_to_clip_matrix;
uniform float _point_z_offset;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    gl_Position.z -= 0.0002;
    v_color = _color;
}
