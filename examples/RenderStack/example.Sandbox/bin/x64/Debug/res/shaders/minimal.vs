//  minimal.vs

#version 330

in vec3 _position;

void main()
{
    gl_Position = vec4(_position, 1.0);
}
