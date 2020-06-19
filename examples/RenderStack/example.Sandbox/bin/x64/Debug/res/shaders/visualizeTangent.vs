//  visualizeTangent.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _tangent;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_world_matrix;

out vec3 v_tangent;

void main()
{
    gl_Position   = _model_to_clip_matrix * vec4(_position, 1.0);
    v_tangent     = vec3(_model_to_world_matrix * vec4(_tangent, 0.0));
}
