//  visualizeTangent.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _tangent;

out vec3 v_tangent;

UNIFORMS;

void main()
{
    INSTANCING;
    
    gl_Position   = model_to_clip_matrix * vec4(_position, 1.0);
    v_tangent     = vec3(model_to_world_matrix * vec4(_tangent, 0.0));
}
