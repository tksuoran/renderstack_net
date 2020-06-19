//  IdVec3.vs

#version 330

in vec3 _position;
in vec3 _id_vec3;

flat out vec3 v_id_vec3;

uniform mat4 _model_to_clip_matrix;
uniform vec3 _id_offset_vec3;

void main()
{
    gl_Position = _model_to_clip_matrix * vec4(_position, 1.0);
    v_id_vec3   = _id_vec3 + _id_offset_vec3;
}
