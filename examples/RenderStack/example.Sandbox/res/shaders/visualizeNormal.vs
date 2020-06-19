//  visualizeNormal.vs

#version 330

in vec3 _position;
in vec3 _normal;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_world_matrix;

out vec3 v_normal;

void main()
{
    gl_Position   = _model_to_clip_matrix * vec4(_position, 1.0);
    v_normal      = vec3(_model_to_world_matrix * vec4(_normal, 0.0));
}
