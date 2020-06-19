//  ShadowCaster2.vs

#version 330 

in vec3 _position;

UNIFORMS;

void main()
{
    INSTANCING;

    gl_Position     = model_to_clip_matrix * vec4(_position, 1.0);

    gl_Position.z  += lights.bias.z;
}
