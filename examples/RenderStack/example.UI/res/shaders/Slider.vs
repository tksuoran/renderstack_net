//  Slider.vs

#version 120

attribute vec3 _position;
attribute vec2 _texcoord;
attribute vec4 _color;

uniform mat4 _model_to_clip_matrix;

varying vec2 v_texcoord;
varying vec4 v_color;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_texcoord  = _texcoord.xy;
    v_color = _color;
}
