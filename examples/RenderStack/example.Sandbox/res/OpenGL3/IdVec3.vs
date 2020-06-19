//  IdVec3.vs

#version 330

in vec3 _position;
in vec3 _id_vec3;

flat out vec3 v_id_vec3;

UNIFORMS;

void main()
{
    INSTANCING;

    gl_Position = model_to_clip_matrix * vec4(_position, 1.0);
    v_id_vec3   = _id_vec3 + id_offset_vec3;
}
