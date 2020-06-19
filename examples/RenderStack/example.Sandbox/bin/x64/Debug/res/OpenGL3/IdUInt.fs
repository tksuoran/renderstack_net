//  IdUint.fs

#version 330

flat in uint v_id_uint;

out uint out_color;

void main(void)
{
    out_color = v_id_uint + id_offset_uint;
}
