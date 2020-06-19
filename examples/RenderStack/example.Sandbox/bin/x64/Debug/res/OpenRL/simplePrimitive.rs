//  simplePrimitive.rs

varying vec3 v_normal;

void main()
{
    accumulate(0.5 * v_normal + 0.5);
    //accumulate(vec3(1.0, 0.0, 0.0));
}
