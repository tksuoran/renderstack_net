//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;
using RenderStack.UI;

using example.Renderer;

using Attribute = RenderStack.Graphics.Attribute;

namespace example.Sandbox
{
    public static class MaterialManagerCode
    {
        public static Material MakeSimpleMaterial(this MaterialManager mm, string name, float r, float g, float b)
        {
            return MakeSimpleMaterial(mm, name, r, g, b, 0.5f, 1.0f, 0.02f);
        }
        public static Material MakeSimpleMaterial(this MaterialManager mm, string name, float r, float g, float b, float diffuse, float specular, float roughness)
        {
            var m = mm.MakeMaterial(name, "Schlick");
            m.Floats("surface_diffuse_reflectance_color"    ).Set(diffuse * r, diffuse * g, diffuse * b);
            m.Floats("surface_specular_reflectance_color"   ).Set(specular * r, specular * r, specular *r);
            m.Floats("surface_roughness"                    ).Set(roughness);
            m.Sync();
            return m;
        }
        public static Material MakeAnisotropic(this MaterialManager mm, string name, float r, float g, float b, float roughness, float isotropy)
        {
            var m = mm.MakeMaterial(name, "Anisotropic");
            m.Floats("surface_diffuse_reflectance_color"    ).Set(0.1f, 0.1f, 0.1f);
            m.Floats("surface_specular_reflectance_color"   ).Set(1.0f, 1.0f, 1.0f);
            m.Floats("surface_specular_reflectance_exponent").Set(80.0f);
            m.Floats("surface_roughness"                    ).Set(roughness);
            m.Floats("surface_isotropy"                     ).Set(isotropy);
            m.Sync();
            return m;
        }
        public static Material MakeSchlick(this MaterialManager mm, string name)
        {
            var m = mm.MakeMaterial(name, "Schlick");
            m.Floats("surface_diffuse_reflectance_color"    ).Set(0.44f, 0.44f, 0.44f);
            m.Floats("surface_specular_reflectance_color"   ).Set(1.0f, 1.0f, 1.0f);
            m.Floats("surface_specular_reflectance_exponent").Set(80.0f);
            m.Floats("surface_roughness"                    ).Set(0.02f);
            m.Sync();
            return m;
        }
    }
}