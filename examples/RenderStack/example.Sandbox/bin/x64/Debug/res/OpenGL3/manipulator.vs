// manipulator.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _polygon_normal;

out vec3 v_normal;
out vec3 v_view_direction;

UNIFORMS;

invariant gl_Position;

void main()
{
    INSTANCING;

    gl_Position   = model_to_clip_matrix * vec4(_position, 1.0);
    v_normal      = vec3(model_to_world_matrix * vec4(_normal, 0.0));
    vec4 position = model_to_world_matrix * vec4(_position, 1.0);

    v_view_direction = view_position_in_world.xyz - position.xyz;
}
