//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;

using RenderStack.Geometry;
using RenderStack.Geometry.Shapes;
using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Services;
using RenderStack.Scene;

using example.Renderer;

namespace example.Sandbox
{
    public partial class SceneManager : Service, ISceneManager
    {
        public delegate void LightCountSetDelegate(int count);
        public event LightCountSetDelegate LightCountSet;

        //  http://www.softimageblog.com/archives/115
        private List<Vector3> PointsOnSphereGoldenSectionSpiral(int n, float minY)
        {
            int max = n;
            var points = new List<Vector3>();

            for(;;)
            {
                double  increment = Math.PI * (3.0 - Math.Sqrt(5.0));
                double  offset = 2.0 / (double)max;
                for(int i = 0; i < max; ++i)
                {
                    double y = i * offset - 1.0 + (offset / 2.0);
                    if(y <= minY)
                    {
                        continue;
                    }
                    double r = Math.Sqrt(1.0 - y * y);
                    double phi = i * increment;
                    double x = Math.Cos(phi) * r;
                    double z = Math.Sin(phi) * r;
                    System.Console.WriteLine("X: " + x.ToString() + ", Y: " + y.ToString() + ", Z: " + z.ToString());
                    points.Add(new Vector3(x, y, z));
                }
                if(points.Count >= n)
                {
                    return points;
                }

                points.Clear();
                ++max;
            }
        }

        private List<Vector3> PointsOnSphereSubdividedIcosahedron(int subdivisions)
        {
            var points = new List<Vector3>();

            Geometry g = new RenderStack.Geometry.Shapes.Icosahedron(1.0);
            g.BuildEdges();
            for(int i = 0; i < subdivisions; ++i)
            {
                g = new Sqrt3GeometryOperation(g).Destination;
            }
            var pointLocations = g.PointAttributes.FindOrCreate<Vector3>("point_locations");
            foreach(var point in g.Points)
            {
                Vector3 pos = pointLocations[point];
                if(pos.Y <= 0.0f)
                {
                    continue;
                }
                points.Add(pos);
            }
            return points;
        }

        private float scale = 1.0f;
        private void AddDirectionalLight(Vector3 direction, Vector4 color)
        {
            int index = renderGroup.Lights.Count;
            var light = new Light(index);

            light.Name                      = "light " + index.ToString();
            light.Projection.ProjectionType = ProjectionType.Orthogonal;
            light.Projection.OrthoLeft      = -floorSize * 0.8f * scale;
            light.Projection.OrthoTop       =  floorSize * 0.8f * scale;
            light.Projection.OrthoWidth     =  floorSize * 1.4f * scale;
            light.Projection.OrthoHeight    =  floorSize * 1.4f * scale;
            light.Projection.Near           =  0.0f;
            light.Projection.Far            =  floorSize * 2.0f * scale;

            direction = Vector3.Normalize(direction);
            LightsUniforms.Direction.SetI(index, direction.X, direction.Y, direction.Z);
            LightsUniforms.Color.SetI(index, color.X, color.Y, color.Z, color.W);

            light.Frame.LocalToParent.Set(
                Matrix4.CreateLookAt(
                    floorSize * direction * scale,  // position affects near and far plane clipping
                    new Vector3(0.0f, 0.0f, 0.0f),
                    Vector3.UnitY
                )
            );
            renderGroup.Lights.Add(light);
        }
        public void ClearLights()
        {
            renderGroup.Lights.Clear();
        }
        public void AddAOLights(int n)
        {
            int             count       = Math.Min(example.Renderer.Configuration.maxLightCount, n - renderGroup.Lights.Count);
            float           rel         = 1.0f / (float)(count);
            Vector4         color       = new Vector4(1.0f, 1.0f, 1.0f, rel);
            List<Vector3>   directions  = PointsOnSphereGoldenSectionSpiral(count, 0.25f);

            foreach(var d in directions)
            {
                AddDirectionalLight(d, color);
            }

            LightsUniforms.Count.Set(count);
            LightsUniforms.Exposure.Set(1.0f);
            LightsUniforms.AmbientLightColor.Set(0.1f, 0.2f, 0.3f);
            LightsUniforms.UniformBufferGL.Sync();

            if(RenderStack.Graphics.Configuration.useGl1)
            {
                //  No shadows for GL1
            }
            else
            {
                renderer.SetTexture("t_shadowmap", materialManager.Textures["NoShadow"]);
                System.Console.WriteLine("renderer.SetTexture(\"t_shadowmap\", materialManager.Textures[\"NoShadow\"]);");
            }

            if(LightCountSet != null)
            {
                LightCountSet(renderGroup.Lights.Count);
            }

            UpdateShadowMap = true;
        }
        public void AddBasicLights()
        {
            Floats light_direction = RenderStack.Scene.LightsUniforms.UniformBufferGL.Floats("direction");

            List<Vector4> colors = new List<Vector4>();
            colors.Add(new Vector4(1.0f, 0.5f, 0.5f, 1.0f));
            colors.Add(new Vector4(0.5f, 1.0f, 0.5f, 1.0f));
            colors.Add(new Vector4(0.5f, 0.5f, 1.0f, 1.0f));

            List<Vector3> directions = new List<Vector3>();
            directions.Add(new Vector3(1.1f, 1.0f, 1.0f));
            directions.Add(new Vector3(1.0f, 1.0f, 1.1f));
            directions.Add(new Vector3(1.0f, 1.1f, 1.0f));

            //  In case we do less than 3 lights, add extra lights to light 0
            {
                for(int i = 0; i < 3; ++i)
                {
                    colors[i] = new Vector4(
                        colors[i].X * colors[i].W,
                        colors[i].Y * colors[i].W,
                        colors[i].Z * colors[i].W,
                        1.0f
                    );
                }
                for(int i = example.Renderer.Configuration.maxLightCount; i < 3; ++i)
                {
                    colors[0] += colors[i];
                }
                colors[0] = new Vector4(
                    colors[0].X,
                    colors[0].Y,
                    colors[0].Z,
                    1.0f
                );
            }

            int count = Math.Min(example.Renderer.Configuration.maxLightCount, 3);
            for(int i = 0; i < count; ++i)
            {
                //float x = (float)(i) / (float)(count);
                //Vector3 d = new Vector3(x, 1.0f, 0.0f);
                AddDirectionalLight(directions[i], colors[i]);
            }

            LightsUniforms.Count.Set(count);
            LightsUniforms.Exposure.Set(1.0f);
            LightsUniforms.AmbientLightColor.Set(0.1f, 0.2f, 0.3f);
            LightsUniforms.UniformBufferGL.Sync();

            if(RenderStack.Graphics.Configuration.useGl1)
            {
                //  No shadows for GL1
            }
            else
            {
                renderer.SetTexture("t_shadowmap", materialManager.Textures["NoShadow"]);
                System.Console.WriteLine("renderer.SetTexture(\"t_shadowmap\", materialManager.Textures[\"NoShadow\"]);");
            }

            if(LightCountSet != null)
            {
                LightCountSet(renderGroup.Lights.Count);
            }
            UpdateShadowMap = true;
        }
    }
}