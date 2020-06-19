//  simplePrimitive.vs

attribute vec3 _position;
attribute vec3 _normal;

transformed varying vec3 v_normal;

void main()
{
    rl_Position = vec4(_position, 1.0);
    v_normal = _normal;
}
