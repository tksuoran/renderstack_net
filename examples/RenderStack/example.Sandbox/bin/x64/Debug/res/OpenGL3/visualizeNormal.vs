//  visualizeNormal.vs

#version 330

in vec3 _position;
in vec3 _normal;

out vec3 v_normal;

UNIFORMS:

void main()
{
    INSTANCING;
    
    gl_Position   = model_to_clip_matrix * vec4(_position, 1.0);
    v_normal      = vec3(_model_to_world_matrix * vec4(_normal, 0.0));
}
