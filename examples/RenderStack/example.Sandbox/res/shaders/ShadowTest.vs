//  grid.vs

#version 330

in vec3 _position;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_shadow_matrix;

out vec4 v_shadow;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_shadow    = _model_to_shadow_matrix * vec4(_position, 1.0);
}
