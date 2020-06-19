//  IdVec3.fs

#version 330

flat in vec3 v_id_vec3;

out vec4 out_color;

void main(void)
{
    out_color = vec4(v_id_vec3, 1.0);
}
