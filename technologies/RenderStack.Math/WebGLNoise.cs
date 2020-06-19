using System.Collections;

namespace RenderStack.Math
{
    public class WebGLNoise
    {
        //
        // Description : Array and textureless GLSL 2D/3D/4D simplex 
        //               noise functions.
        //      Author : Ian McEwan, Ashima Arts.
        //  Maintainer : ijm
        //     Lastmod : 20110822 (ijm)
        //     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
        //               Distributed under the MIT License. See LICENSE file.
        //               https://github.com/ashima/webgl-noise
        // 

        private static Vector3 Mod289(Vector3 x)
        {
            return x - Vector3.Floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        private static Vector4 Mod289(Vector4 x)
        {
            return x - Vector4.Floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        private static Vector4 Permute(Vector4 x)
        {
            return Mod289(((x * 34.0f) + Vector4.One) * x);
        }

        private static Vector4 TaylorInvSqrt(Vector4 r)
        {
            return new Vector4(1.79284291400159f) - 0.85373472095314f * r;
        }

        public static float Noise(Vector3 v)
        { 
            Vector2 C = new Vector2(1.0f / 6.0f, 1.0f / 3.0f);
            Vector4 D = new Vector4(0.0f, 0.5f, 1.0f, 2.0f);

            // First corner
            Vector3 i  = Vector3.Floor(v + new Vector3(Vector3.Dot(v, C.Yyy)) );
            Vector3 x0 =           v - i + new Vector3(Vector3.Dot(i, C.Xxx)) ;

            // Other corners
            Vector3 g = Vector3.Step(x0.Yzx, x0.Xyz);
            Vector3 l = Vector3.One - g;
            Vector3 i1 = Vector3.Min(g.Xyz, l.Zxy);
            Vector3 i2 = Vector3.Max(g.Xyz, l.Zxy);

            //   x0 = x0 - 0.0 + 0.0 * C.xxx;
            //   x1 = x0 - i1  + 1.0 * C.xxx;
            //   x2 = x0 - i2  + 2.0 * C.xxx;
            //   x3 = x0 - 1.0 + 3.0 * C.xxx;
            Vector3 x1 = x0 - i1 + C.Xxx;
            Vector3 x2 = x0 - i2 + C.Yyy; // 2.0*C.x = 1/3 = C.y
            Vector3 x3 = x0 - D.Yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

            // Permutations
            i = Mod289(i); 
            Vector4 p = 
                Permute( 
                    Permute( 
                        Permute( 
                            new Vector4(i.Z) + new Vector4(0.0f, i1.Z, i2.Z, 1.0f)
                        )
                        + new Vector4(i.Y) + new Vector4(0.0f, i1.Y, i2.Y, 1.0f)
                    ) 
                    + new Vector4(i.X) + new Vector4(0.0f, i1.X, i2.X, 1.0f)
                );

            // Gradients: 7x7 points over a square, mapped onto an octahedron.
            // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
            float   n_ = 0.142857142857f; // 1.0/7.0
            Vector3 ns = n_ * D.Wyz - D.Xzx;

            Vector4 j = p - 49.0f * Vector4.Floor(p * ns.Z * ns.Z);  //  mod(p,7*7)

            Vector4 x_ = Vector4.Floor(j * ns.Z);
            Vector4 y_ = Vector4.Floor(j - 7.0f * x_ );    // mod(j,N)

            Vector4 x = x_ * ns.X + ns.Yyyy;
            Vector4 y = y_ * ns.X + ns.Yyyy;
            Vector4 h = Vector4.One - Vector4.Abs(x) - Vector4.Abs(y);

            Vector4 b0 = new Vector4(x.Xy, y.Xy);
            Vector4 b1 = new Vector4(x.Zw, y.Zw);

            //vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
            //vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
            Vector4 s0 = Vector4.Floor(b0) * 2.0f + Vector4.One;
            Vector4 s1 = Vector4.Floor(b1) * 2.0f + Vector4.One;
            Vector4 sh = -Vector4.Step(h, Vector4.Zero);

            Vector4 a0 = b0.Xzyw + s0.Xzyw * sh.Xxyy;
            Vector4 a1 = b1.Xzyw + s1.Xzyw * sh.Zzww;

            Vector3 p0 = new Vector3(a0.Xy, h.X);
            Vector3 p1 = new Vector3(a0.Zw, h.Y);
            Vector3 p2 = new Vector3(a1.Xy, h.Z);
            Vector3 p3 = new Vector3(a1.Zw, h.W);

            //Normalise gradients
            Vector4 norm = TaylorInvSqrt(
                new Vector4(
                    Vector3.Dot(p0, p0),
                    Vector3.Dot(p1, p1),
                    Vector3.Dot(p2, p2),
                    Vector3.Dot(p3, p3)
                )
            );
            p0 *= norm.X;
            p1 *= norm.Y;
            p2 *= norm.Z;
            p3 *= norm.W;

            // Mix final noise value
            Vector4 m = Vector4.Max(
                new Vector4(0.6f) - new Vector4(
                    Vector3.Dot(x0, x0), 
                    Vector3.Dot(x1, x1), 
                    Vector3.Dot(x2, x2), 
                    Vector3.Dot(x3, x3)
                ), 
                Vector4.Zero
            );
            m = m * m;
            return 42.0f * Vector4.Dot( 
                m * m, 
                new Vector4( 
                    Vector3.Dot(p0, x0), 
                    Vector3.Dot(p1, x1), 
                    Vector3.Dot(p2, x2), 
                    Vector3.Dot(p3, x3)
                )
            );
        }
    }
}

#if false
//
// Description : Array and textureless GLSL 2D/3D/4D simplex 
//               noise functions.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : ijm
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
// 

vec3 mod289(vec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(vec4 x) {
     return mod289(((x*34.0)+1.0)*x);
}

vec4 taylorInvSqrt(vec4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(vec3 v)
  { 
  const vec2  C = vec2(1.0/6.0, 1.0/3.0) ;
  const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

// First corner
  vec3 i  = floor(v + dot(v, C.yyy) );
  vec3 x0 =   v - i + dot(i, C.xxx) ;

// Other corners
  vec3 g = step(x0.yzx, x0.xyz);
  vec3 l = 1.0 - g;
  vec3 i1 = min( g.xyz, l.zxy );
  vec3 i2 = max( g.xyz, l.zxy );

  //   x0 = x0 - 0.0 + 0.0 * C.xxx;
  //   x1 = x0 - i1  + 1.0 * C.xxx;
  //   x2 = x0 - i2  + 2.0 * C.xxx;
  //   x3 = x0 - 1.0 + 3.0 * C.xxx;
  vec3 x1 = x0 - i1 + C.xxx;
  vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
  vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
  i = mod289(i); 
  vec4 p = permute( permute( permute( 
             i.z + vec4(0.0, i1.z, i2.z, 1.0 ))
           + i.y + vec4(0.0, i1.y, i2.y, 1.0 )) 
           + i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

// Gradients: 7x7 points over a square, mapped onto an octahedron.
// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
  float n_ = 0.142857142857; // 1.0/7.0
  vec3  ns = n_ * D.wyz - D.xzx;

  vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

  vec4 x_ = floor(j * ns.z);
  vec4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

  vec4 x = x_ *ns.x + ns.yyyy;
  vec4 y = y_ *ns.x + ns.yyyy;
  vec4 h = 1.0 - abs(x) - abs(y);

  vec4 b0 = vec4( x.xy, y.xy );
  vec4 b1 = vec4( x.zw, y.zw );

  //vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
  //vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
  vec4 s0 = floor(b0)*2.0 + 1.0;
  vec4 s1 = floor(b1)*2.0 + 1.0;
  vec4 sh = -step(h, vec4(0.0));

  vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
  vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

  vec3 p0 = vec3(a0.xy,h.x);
  vec3 p1 = vec3(a0.zw,h.y);
  vec3 p2 = vec3(a1.xy,h.z);
  vec3 p3 = vec3(a1.zw,h.w);

//Normalise gradients
  vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
  p0 *= norm.x;
  p1 *= norm.y;
  p2 *= norm.z;
  p3 *= norm.w;

// Mix final noise value
  vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
  m = m * m;
  return 42.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), 
                                dot(p2,x2), dot(p3,x3) ) );
  }

#endif