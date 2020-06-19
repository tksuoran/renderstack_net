// visualizeCubemap.vs

#version 330

in vec3 _position;
in vec3 _texcoord;

uniform mat4 _model_to_clip_matrix;

out vec3 v_texcoord;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_texcoord  = _texcoord;
}
