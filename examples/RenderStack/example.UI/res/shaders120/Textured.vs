//  Textured.vs

#version 120

attribute vec3 _position;
attribute vec2 _texcoord;

varying vec2 v_texcoord;

uniform mat4 _model_to_clip_matrix;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_texcoord  = _texcoord.xy;
}
