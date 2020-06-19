//  line.gs

//  Geometry shader for rendering wide lines
//  with GL 3.3+ with forward compatible context.
//
//  Input and output positions must be in clip space.
//
//  Not optimized.
//
//  You might want to do this in your fragment shader:
//   - make line ends round
//   - add AA like in http://people.csail.mit.edu/ericchan/articles/prefilter/
//
//  Timo Suoranta - tksuoran@gmail.com

#version 330

#define SHOW_DEBUG_LINES        0
#define PASSTHROUGH_BASIC_LINES 0
#define STRIP                   1

layout(lines) in;

#if SHOW_DEBUG_LINES || PASSTHROUGH_BASIC_LINES
layout(line_strip, max_vertices = 10) out;
#else 
layout(triangle_strip, max_vertices = 6) out;
#endif

in vec3 _position[];

out vec2    v_start;
out vec2    v_line;
out float   v_l2;

UNIFORMS;

void main(void) 
{
    //  a - - - - - - - - - - - - - - - - b
    //  |      |                   |      |
    //  |      |                   |      |
    //  |      |                   |      |
    //  | - - -start - - - - - - end- - - |
    //  |      |                   |      |
    //  |      |                   |      |
    //  |      |                   |      |
    //  d - - - - - - - - - - - - - - - - c

    vec4 start  = gl_in[0].gl_Position;
    vec4 end    = gl_in[1].gl_Position;

#if PASSTHROUGH_BASIC_LINES
    gl_Position = start; EmitVertex();
    gl_Position = end;   EmitVertex();
    EndPrimitive();
    return;
#endif

    // It is necessary to manually clip the line before homogenization.
    // Compute line start and end distances to nearplane in clipspace
    // Distances are t0 = dot(start, plane) and t1 = dot(end, plane)
    // If signs are not equal then clip
    float t0 = start.z + start.w;
    float t1 = end.z + end.w;
    if(t0 < 0.0)
    {
        if(t1 < 0.0)
        {
            return;
        }
        start = mix(start, end, (0 - t0) / (t1 - t0));
    }
    if(t1 < 0.0)
    {
        end = mix(start, end, (0 - t0) / (t1 - t0));
    }

    vec2 vpSize         = camera.viewport.zw;

    //  Compute line axis and side vector in screen space
    vec2 startInNDC     = start.xy / start.w;       //  clip to NDC: homogenize and drop z
    vec2 endInNDC       = end.xy / end.w;
    vec2 lineInNDC      = endInNDC - startInNDC;
    vec2 startInScreen  = (0.5 * startInNDC + vec2(0.5)) * vpSize + camera.viewport.xy;
    vec2 endInScreen    = (0.5 * endInNDC   + vec2(0.5)) * vpSize + camera.viewport.xy;
    vec2 lineInScreen   = lineInNDC * vpSize;       //  ndc to screen (direction vector)
    vec2 axisInScreen   = normalize(lineInScreen);
    vec2 sideInScreen   = vec2(-axisInScreen.y, axisInScreen.x);    // rotate
    vec2 axisInNDC      = axisInScreen / vpSize;                    // screen to NDC
    vec2 sideInNDC      = sideInScreen / vpSize;
    vec4 axis           = vec4(axisInNDC, 0.0, 0.0) * material.line_width.x;  // NDC to clip (delta vector)
    vec4 side           = vec4(sideInNDC, 0.0, 0.0) * material.line_width.x;

    vec4 a = (start + (side - axis) * start.w); 
    vec4 b = (end   + (side + axis) * end.w);
    vec4 c = (end   - (side - axis) * end.w);
    vec4 d = (start - (side + axis) * start.w);

    v_start = startInScreen;
    v_line  = endInScreen - startInScreen;
    v_l2    = dot(v_line, v_line);

#if SHOW_DEBUG_LINES
    gl_Position = gl_in[0].gl_Position; EmitVertex();
    gl_Position = gl_in[1].gl_Position; EmitVertex();
    EndPrimitive();

    gl_Position = a; EmitVertex();
    gl_Position = b; EmitVertex();
    EndPrimitive();

    gl_Position = b; EmitVertex();
    gl_Position = c; EmitVertex();
    EndPrimitive();

    gl_Position = c; EmitVertex();
    gl_Position = d; EmitVertex();
    EndPrimitive();

    gl_Position = d; EmitVertex();
    gl_Position = a; EmitVertex();
    EndPrimitive();

#else

#if STRIP
#if 0
    gl_Position = d; EmitVertex();
    gl_Position = a; EmitVertex();
    gl_Position = c; EmitVertex();
    gl_Position = b; EmitVertex();
#else
    gl_Position = a; EmitVertex();
    gl_Position = d; EmitVertex();
    gl_Position = b; EmitVertex();
    gl_Position = c; EmitVertex();
#endif
    EndPrimitive();
#else
    gl_Position = d; EmitVertex();
    gl_Position = a; EmitVertex();
    gl_Position = c; EmitVertex();
    EndPrimitive();
    gl_Position = c; EmitVertex();
    gl_Position = a; EmitVertex();
    gl_Position = b; EmitVertex();
    EndPrimitive();
#endif

#endif
}
