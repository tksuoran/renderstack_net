// hemispherical.vs

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

void main()
{
    vec4 position_in_view = _model_to_view_matrix * vec4(_position, 1.0);

    //  Project to a point on a unit hemisphere
    vec3 hemispherical = normalize(position_in_view.xyz);

    //  Compute (f - n), but let the hardware divide z by this
    //  in the w component (so premultiply x and y)
    float f_minus_n = _near_far.y - _near_far.x;
    gl_Position.xy = hemispherical.xy * f_minus_n;

    //  Compute depth proj. independently, using OpenGL orthographic
    gl_Position.z = (-2.0 * position_in_view.z - _near_far.y - _near_far.x);
    gl_Position.w = f_minus_n;

    v_normal      = vec3(_model_to_world_matrix * vec4(_normal, 0.0));
    vec4 position = _model_to_world_matrix * vec4(_position, 1.0);
    vec3 view_position_in_world = vec3(
        _view_to_world_matrix[3][0],
        _view_to_world_matrix[3][1],
        _view_to_world_matrix[3][2]
    );
    
    v_view_direction = view_position_in_world.xyz - position.xyz;
    
}
