//  IdUInt.vs

#version 330

in vec3 _position;
in uint _id_uint;

flat out uint v_id_uint;

uniform mat4 _model_to_clip_matrix;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_id_uint   = _id_uint;
}
