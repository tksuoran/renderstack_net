// lightOccluder.vs

#version 330 

in vec3 _position;

uniform mat4 _model_to_clip_matrix;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
}
