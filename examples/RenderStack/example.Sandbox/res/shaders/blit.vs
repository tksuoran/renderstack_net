//  blit.vs

#version 330

in vec3 _position;

out vec2 v_texcoord;

void main()
{
    gl_Position = vec4(2.0 * _position, 1.0) - vec4(1.0, 1.0, 0.0, 0.0);
    v_texcoord  = _position.xy;
}
