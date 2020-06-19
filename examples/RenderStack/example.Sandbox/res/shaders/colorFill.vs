// colorFill.vs

#version 330 

in vec3 _position;
in vec4 _color;

uniform mat4 _model_to_clip_matrix;

out vec4 v_color;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_color     = _color;
}
