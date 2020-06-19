//  IdUInt.vs

#version 330

in vec3 _position;
in uint _id_uint;

flat out uint v_id_uint;

UNIFORMS;

void main()
{
    INSTANCING;
    
    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    v_id_uint   = id_uint;
}
