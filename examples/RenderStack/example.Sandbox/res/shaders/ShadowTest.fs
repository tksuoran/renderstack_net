//  grid.fs

#version 330

uniform sampler2D   _shadowmap;
in      vec4        v_shadow;
out     vec4        out_color;


vec4 lightVisibility(void)
{
    vec4    shadow      = v_shadow / v_shadow.w;
    float   distance    = shadow.z;
    vec2    moments     = texture2D(_shadowmap, shadow.xy).rg;

    //  Surface is fully lit. as the current fragment is before the light occluder
    if(distance <= moments.x)
    {
        return vec4(1.0, 1.0, 1.0, 1.0);
    }

    // The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
    // How likely this pixel is to be lit (p_max)
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, 0.00002);

    float d     = distance - moments.x;
    float p_max = variance / (variance + d * d);

    return vec4(p_max, p_max, p_max, p_max);
}

void main(void)
{
    out_color = lightVisibility();
}
