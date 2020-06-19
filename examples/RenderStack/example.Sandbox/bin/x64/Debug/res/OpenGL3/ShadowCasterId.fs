//  IdVec3.fs

#version 330

flat in vec3 v_id_vec3;

out vec3 out_color;

void main(void)
{
    out_color = v_id_vec3;
}
