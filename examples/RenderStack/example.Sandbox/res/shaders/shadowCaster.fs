//  shadowCaster.fs

#version 330

in      vec4    v_position;
in      vec3    v_normal;
flat in vec3    v_id_vec3;

out     vec4    out_color;

void main(void)
{
    float depth = v_position.z / v_position.w;
    depth = depth * 0.5 + 0.5;

    float dx    = dFdx(depth);
    float dy    = dFdy(depth);
    float slope = sqrt(dx * dx + dy * dy);

    float value = min(10 * slope, 0.005);
    out_color = vec4(depth + value);
}
