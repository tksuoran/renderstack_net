//  shadowReceiver.fs

#version 330

in vec4 v_depth_linear;

out float out_depth;

void main(void)
{
    out_depth = v_depth_linear;
}
