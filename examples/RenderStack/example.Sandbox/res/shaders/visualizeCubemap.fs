// visualizeCubemap.fs

#version 330

uniform samplerCube _texture;

in vec3 v_texcoord;

out vec4 out_color;

void main(void)
{
    //out_color.rgba  = vec4(1.0, 1.0, 1.0, 1.0);
    // For GLSL 120 use textureCube
    out_color.rgb   = texture(_texture, v_texcoord).rgb;
    out_color.a     = 1.0;
}
