//  Basic.vs

#version 330

in vec3 _position;
in vec2 _texcoord;
in vec4 _color;

out vec2 v_texcoord;
out vec4 v_color;

UNIFORMS;

void main()
{
    INSTANCING;

    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    v_texcoord  = _texcoord.xy;
    v_color = _color;
}
