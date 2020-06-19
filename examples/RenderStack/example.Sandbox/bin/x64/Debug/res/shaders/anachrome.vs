//  anachrome.vs

#version 330

in vec3 _position;
in vec2 _texcoord;
in vec4 _color;

uniform mat4 _model_to_clip_matrix;

out vec2 v_texcoord;
out vec4 v_color;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_texcoord  = _texcoord.xy;
    v_color = _color;
}
