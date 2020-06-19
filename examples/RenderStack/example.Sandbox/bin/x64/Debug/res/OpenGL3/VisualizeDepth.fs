//  VisualizeDepth.fs

#version 330

in  vec2 v_texcoord;

out vec4 out_color;

UNIFORMS;

vec3 HSVToRGB(vec3 hsv)
{
    vec3 rgb = hsv.zzz;
    if(hsv.y != 0)
    {
       float var_h = hsv.x * 6.0;
       float var_i = floor(var_h);   // Or ... var_i = floor(var_h)
       float var_1 = hsv.z * (1.0 - hsv.y);
       float var_2 = hsv.z * (1.0 - hsv.y * (var_h - var_i));
       float var_3 = hsv.z * (1.0 - hsv.y * (1 - (var_h - var_i)));
       if      (var_i == 0) { rgb = vec3(hsv.z, var_3, var_1); }
       else if (var_i == 1) { rgb = vec3(var_2, hsv.z, var_1); }
       else if (var_i == 2) { rgb = vec3(var_1, hsv.z, var_3); }
       else if (var_i == 3) { rgb = vec3(var_1, var_2, hsv.z); }
       else if (var_i == 4) { rgb = vec3(var_3, var_1, hsv.z); }
       else                 { rgb = vec3(hsv.z, var_1, var_2); }
   }
   return rgb;
}

void main(void)
{
    float z = texture(t_left, v_texcoord).x;

    float n = 0.1; // material.main_camera_near_far.x;
    float f = 100.0; //material.main_camera_near_far.y;
    float linearZ = (2.0 * n) / (f + n - z * (f - n));

    float h = 0.4 * linearZ;
    float s = 1.0 - linearZ;
    float v = 6.0 * linearZ;
    vec3 hsv = vec3(h, s, v);

    out_color.rgb = HSVToRGB(hsv);
    out_color.a     = global.alpha;
}
