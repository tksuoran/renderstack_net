//  colorPoint.vs

#version 330

in vec3 _position;
in vec4 _color;

out vec4 v_color;

UNIFORMS;

void main()
{
    INSTANCING;

    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    gl_Position.z -= 0.0002; // \todo use material
    v_color = _color;
}
