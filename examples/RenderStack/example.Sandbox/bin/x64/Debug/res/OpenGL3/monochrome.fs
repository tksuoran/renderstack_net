//  monochrome.fs

#version 330

in vec2 v_texcoord;
in vec4 v_color;

out vec4 out_color;

UNIFORMS;

void main(void)
{
    vec2 texcoord   = vec2(v_texcoord.x, v_texcoord.y);
    vec4 sample     = texture2D(t_left, texcoord);
    
    float y = 0.2125 * sample.r + 0.7154 * sample.g + 0.0721 * sample.b;
    out_color.rgb   = 1.0 * y * v_color.rgb;
    out_color.a     = max(sample.r, sample.a); 
    out_color.rgb  += sample.a * global.add_color.rgb;
}
