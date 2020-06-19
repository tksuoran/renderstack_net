using RenderStack.Math;
using Jitter.LinearMath;

namespace RenderStack.Physics
{
    internal class Util
    {
        //  Jitter matrix is transpose of RenderStack matrix and vice versa.
        //  The order of the factors reverses if converting Jitter code to RenderStack,
        //  see http://en.wikipedia.org/wiki/Transpose#Properties
        public static Vector3 FromJitter(JVector jv)
        {
            return new Vector3(jv.X, jv.Y, jv.Z);
        }
#if true
        public static Matrix4 FromJitter(JMatrix jm)
        {
            Matrix4 m = Matrix4.Identity;
            m._00 = jm.M11; m._10 = jm.M12; m._20 = jm.M13;
            m._01 = jm.M21; m._11 = jm.M22; m._21 = jm.M23;
            m._02 = jm.M31; m._12 = jm.M32; m._22 = jm.M33;
            return m;
        }
        public static JMatrix ToJitter(Matrix4 value)
        {
            return new JMatrix(
                value._00 /* M11 */, value._10 /* M12 */, value._20 /* M13 */,
                value._01 /* M21 */, value._11 /* M22 */, value._21 /* M23 */,
                value._02 /* M31 */, value._12 /* M32 */, value._22 /* M33 */
            );
        }
#else
        public static Matrix4 FromJitter(JMatrix jm)
        {
            Matrix4 m = Matrix4.Identity;
            m._00 = jm.M11; m._10 = jm.M21; m._20 = jm.M31;
            m._01 = jm.M12; m._11 = jm.M22; m._21 = jm.M32;
            m._02 = jm.M13; m._12 = jm.M23; m._22 = jm.M33;
            return m;
        }
        public static JMatrix ToJitter(Matrix4 value)
        {
            return new JMatrix(
                value._00 /* M11 */, value._01 /* M12 */, value._02 /* M13 */,
                value._10 /* M21 */, value._11 /* M22 */, value._12 /* M23 */,
                value._20 /* M31 */, value._21 /* M32 */, value._22 /* M33 */
            );
        }
#endif
        public static JVector ToJitter(Vector3 v)
        {
            return new JVector(v.X, v.Y, v.Z);
        }
    }
}
