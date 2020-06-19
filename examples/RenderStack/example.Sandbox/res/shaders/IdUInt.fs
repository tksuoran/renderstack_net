//  IdUint.fs

#version 330

uniform uint _id_offset_uint;

flat in uint v_id_uint;

out uint out_color;

void main(void)
{
    out_color = v_id_uint + _id_offset_uint;
}
