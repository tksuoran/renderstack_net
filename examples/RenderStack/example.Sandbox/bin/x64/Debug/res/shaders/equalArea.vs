// equalArea.vs

#version 330 

in vec3 _position;
in vec3 _normal;
in vec3 _polygon_normal;

uniform mat4 _model_to_clip_matrix;
uniform mat4 _model_to_world_matrix;
uniform mat4 _view_to_world_matrix;

out vec3 v_normal;
out vec3 v_view_direction;

uniform mat4 _model_to_view_matrix;
uniform vec2 _near_far;

// fDepth is [0, 1]
// 
// half2    vPackedDepth = 
//            half2(  floor(fDepth * 255.0) / 255.0,
//                    fract(fDepth * 255.0);
// 
// float fDepth = vPackedDepth.x + vPackedDepth.y * (1.0 / 255.0)
//
// float    vPosition
//          = float3(g_vScale.xy * vUV + g_vScale.zw, 1.0)
//          * fDepth
//  
//  Get view space position from texture coordinates and depth
//  
//  fDepth [0,1]
//  g_vScale moves vUV into [-1,1] range and scales by inverse projection matrix values

//      X                           Y                       
// SG   x / (1 - z),                y / (1 - z)             
// EA   sqrt(2) / sqrt(1 - z)       sqrt(2) / sqrt(1 - z)   

vec4 StereographicProjection(vec3 v)
{
    vec4 result;

    //  Project to a point on a unit hemisphere
    vec3 hemispherical = normalize(v);

    //  Use hardware to z by (f - n) by placing f_minus_n in .w
    //  Premultiply x and y with f_minus_n to prevent them to be divided
    //  Compute depth proj. independently, using OpenGL orthographic
    float f_minus_n = _near_far.y - _near_far.x;
    result.xy       = hemispherical.xy * f_minus_n;
    result.z        = (-2.0 * v.z - _near_far.y - _near_far.x);
    result.w        = f_minus_n;

    return result;
}


//  (-2 * v.z - _near_far.y - _near_far.x) / f_minus_n;
//  (-2 * 1 - 1 - 11) / 10;
//  (-2 - 1 - 11) / 10
//  (-14) / 10 
vec4 EqualAreaProjection(vec3 v)
{
    vec4 result;

    //  Project to a point on a unit hemisphere
    vec3 hemispherical = normalize(v);

    float f_minus_n = _near_far.y - _near_far.x;
    //float z_         = (-2.0 * v.z - _near_far.y - _near_far.x) / f_minus_n;
    float z_double  = (-2.0 * v.z - _near_far.y - _near_far.x) / f_minus_n;
    float z         = 0.5 + 0.5 * z_double;
    float z_sqrt    = sqrt(z);
    float z_s_d     = (2.0 * z_sqrt) - 1.0;
    result.xy       = hemispherical.xy;
    result.z        = z_s_d;
    //result.z        = (-2.0 * v.z - _near_far.y - _near_far.x) / f_minus_n;
    result.w        = 1.0;

    return result;
}

void main()
{
    vec4 position_in_view = _model_to_view_matrix * vec4(_position, 1.0);

    //gl_Position = StereographicProjection(position_in_view.xyz);
    gl_Position = EqualAreaProjection(position_in_view.xyz);

    v_normal      = vec3(_model_to_world_matrix * vec4(_normal, 0.0));
    vec4 position = _model_to_world_matrix * vec4(_position, 1.0);
    vec3 view_position_in_world = vec3(
        _view_to_world_matrix[3][0],
        _view_to_world_matrix[3][1],
        _view_to_world_matrix[3][2]
    );
    
    v_view_direction = view_position_in_world.xyz - position.xyz;
    
}
