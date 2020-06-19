//  Schlick.vs

#version 330

in vec3 _position;
in vec3 _normal;
in vec3 _normal_smooth;
in vec3 _color;

out vec3 v_normal;
out vec3 v_view_direction;
out vec4 v_shadow[MAX_LIGHT_COUNT];
out vec3 v_color;

UNIFORMS;
UTILS;

float generate(vec3 position)
{
    /*
        float octaves     = 4.0;
        float frequency   = 2.2;
        float amplitude   = 0.2;
        float lacunarity  = 3.3;
        float persistence = 0.25;
    */
    float octaves     = material.octaves;
    float frequency   = material.frequency;
    float amplitude   = material.amplitude;
    float lacunarity  = material.lacunarity;
    float persistence = material.persistence;
    return fbm(position + vec3(material.offset), octaves, frequency, amplitude, lacunarity, persistence);
}

void main()
{
    INSTANCING;

    vec3 N0 = _normal;
    vec3 Bt = minAxis(N0);
    vec3 T  = normalize(cross(Bt, N0));
         Bt = cross(N0, T);

    float k = 1.0 / 1024.0;
    float s = k * 0.5;
    float t = k * 0.28867513;
    float u = k * 0.57735027;

    vec3 P0 = _position;
    vec3 A0 = P0 - s * Bt - t * T;
    vec3 B0 = P0          + u * T;
    vec3 C0 = P0 + s * Bt - t * T;

    //vec3 P = P0 + generate(P0) * _normal_smooth;
    vec3 A = A0 + generate(A0) * _normal_smooth;
    vec3 B = B0 + generate(B0) * _normal_smooth;
    vec3 C = C0 + generate(C0) * _normal_smooth;
    vec3 P = P0;

    //vec3 N = normalize(cross(A, B) + cross(B, C) + cross(C, A));
    //vec3 N = normalize(cross(B - A, C - A));
    vec3 N = N0;

    gl_Position = model_to_clip_matrix * vec4(P, 1.0);
    v_normal    = vec3(model_to_world_matrix * vec4(N, 0.0));

    for(int i = 0; i < lights.count; ++i)
    {
        v_shadow[i] = lights.world_to_shadow_matrix[i] * model_to_world_matrix * vec4(_position, 1.0);
    }

    vec4 position = model_to_world_matrix * vec4(P, 1.0);

    v_view_direction = view_position_in_world.xyz - position.xyz;
    v_color = _color;
}
