//  simpleFrame.fs

//rayattribute vec3 color;

UNIFORMS;

void setup()
{
    rl_OutputRayCount = 1;
}

void main()
{
    //float tanTheta    = tan(camera.fovx_fovy_aspect.y * 0.5);
    float tanTheta    = tan(1.5 * 0.5);
    float aspectRatio = rl_FrameSize.y   / rl_FrameSize.x;
    vec2  frameCoord  = rl_FrameCoord.xy / rl_FrameSize.xy - 0.5;

    vec3 directionInCamera = vec3(frameCoord.x, frameCoord.y * 1.0, -1.0 / (2.0 * tanTheta));
    vec3 _x = vec3(camera.view_to_world_matrix[0].x, camera.view_to_world_matrix[1].x, camera.view_to_world_matrix[2].x);
    vec3 _y = vec3(camera.view_to_world_matrix[0].y, camera.view_to_world_matrix[1].y, camera.view_to_world_matrix[2].y);
    vec3 _z = vec3(camera.view_to_world_matrix[0].z, camera.view_to_world_matrix[1].z, camera.view_to_world_matrix[2].z);
    //vec3 directionInWorld = vec3(
    //    dot(camera.view_to_world_matrix[0].xyz, directionInCamera),
    //    dot(camera.view_to_world_matrix[1].xyz, directionInCamera),
    //    dot(camera.view_to_world_matrix[2].xyz, directionInCamera)
    //);
    vec3 directionInWorld = vec3(
        dot(_x, directionInCamera),
        dot(_y, directionInCamera),
        dot(_z, directionInCamera)
    );

    createRay();
    rl_OutRay.origin    = camera.view_position_in_world.xyz;
    //rl_OutRay.direction = vec3(frameCoord.x, frameCoord.y * camera.fovx_fovy_aspect.z, -1.0 / (2.0 * tanTheta));
    //rl_OutRay.direction = vec3(frameCoord.x, frameCoord.y * 1.0, -1.0 / (2.0 * tanTheta));
    rl_OutRay.direction = directionInWorld;

    //rl_OutRay.color     = vec3(1.0);
    //rl_OutRay.defaultPrimitive = environmentPrimitive;
    emitRay();
    vec3 D = vec3(0.5) + 0.5 * rl_OutRay.direction;
    //accumulate(vec3(0.0, rl_FrameCoord.xy / rl_FrameSize.xy));
    //accumulate(D);
    //accumulate(0.5 * camera.fovx_fovy_aspect.xyz);
}
