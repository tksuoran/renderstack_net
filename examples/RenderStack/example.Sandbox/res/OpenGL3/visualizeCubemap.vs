//  visualizeCubemap.vs

#version 330

in vec3 _position;
in vec3 _texcoord;

out vec3 v_texcoord;

UNIFORMS:

void main()
{
    INSTANCING;
    
    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    v_texcoord  = _texcoord;
}
