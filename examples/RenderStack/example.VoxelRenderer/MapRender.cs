//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Scene;
using RenderStack.Services;

using example.Renderer;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;

namespace example.VoxelRenderer
{
    public partial class Map : Service
    {
        MaterialManager materialManager;
        IRenderer       renderer;

        public override string Name
        {
            get { return "MapRenderer"; }
        }

        public static BlockType[] blockTypes = new BlockType[256];

        private IBuffer     vertexBuffer;
        private IBuffer     indexBuffer;
        private Attribute   position;
        private Attribute   texcoord;
        private Attribute   color;
        private Material    basic;

        public IBuffer      VertexBuffer    { get { return vertexBuffer; } }
        public IBuffer      IndexBuffer     { get { return indexBuffer; } }
        public Attribute    Position        { get { return position; } }
        public Attribute    Texcoord        { get { return texcoord; } }
        public Attribute    Color           { get { return color; } }
        //private BufferRange indexBufferRange;

        protected override void  InitializeService()
        {
            //basic = materialManager["Basic"];
            basic = materialManager.MakeMaterial("Basic");
            Image terrainImage = new Image("res/images/terrain.png");
            // \todo support material textures
            basic.Textures["t_surface_color"] = materialManager.Textures["terrain"] = new TextureGL(terrainImage);

            blockTypes[BlockType.Grass]         = new BlockType(0,0, 2,0, 3,0, 3,0, 3,0, 3,0);
            blockTypes[BlockType.Stone]         = new BlockType(1,0);
            blockTypes[BlockType.Dirt]          = new BlockType(2,0);
            blockTypes[BlockType.Cobblestone]   = new BlockType(0,1);
            blockTypes[BlockType.Bedrock]       = new BlockType(1,1);
            blockTypes[BlockType.Wood]          = new BlockType(5,1, 5,1, 4,1, 4,1, 4,1,4,1);
            blockTypes[BlockType.Sand]          = new BlockType(2,1);
            //   0, 0 grass (in gray)
            //   1, 0 stone
            //   2, 0 dirt
            //   3, 0 grass left, right, front, back
            //   4, 0 wooden planks
            //   5, 0 stone slab sides
            //   6, 0 stone slab top, bottom
            //   7, 0 brick
            //   8, 0 tnt sides
            //   9, 0 tnt top
            //  10, 0 tnt bottom
            //  11, 0 spider web
            //  12, 0 red flower
            //  13, 0 yellow flower
            //  14, 0 water?
            //  15, 0 sapling
            //   0, 1 cobble stone
            //   1, 1 bedrock
            //   2, 1 sand
            //   3, 1 gravel
            //   4, 1 log sidem
            //   5, 1 log top, bottom
            //   6, 1 iron block
            //   7, 1 gold block
            //   8, 1 diamond block
            //   9, 1 chest top, bottom
            //  10, 1 chest front, left, right
            //  11, 1 chest back
            //  12, 1

            var vertexFormat = new VertexFormat();
            position = vertexFormat.Add(new Attribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3));
            texcoord = vertexFormat.Add(new Attribute(VertexUsage.TexCoord, VertexAttribPointerType.Float, 0, 2));
            //color = vertexFormat.Add(new Attribute(VertexUsage.Color, VertexAttribPointerType.Byte, 0, 4, true));
            color = vertexFormat.Add(new Attribute(VertexUsage.Color, VertexAttribPointerType.Float, 0, 4));
            // Some gfx cards fail to show last vertex right if texcoord is set to have 3 components

            vertexBuffer = BufferFactory.Create(vertexFormat, BufferUsageHint.StaticDraw);
            indexBuffer = BufferFactory.Create(DrawElementsType.UnsignedInt, BufferUsageHint.StaticDraw);
        }

        public void Connect(
            MaterialManager materialManager,
            IRenderer       renderer
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;

            InitializationDependsOn(renderer, materialManager);
        }

        public void RenderChunks(Camera camera)
        {
            renderer.Requested.Camera   = camera;
            renderer.SetFrame(renderer.DefaultFrame);
            renderer.Requested.Material = basic;
            renderer.Requested.Program  = basic.Program;
            renderer.Requested.MeshMode = MeshMode.PolygonFill;
            renderer.SetTexture("t_surface_color", basic.Textures["t_surface_color"]);
            foreach(Chunk chunk in chunks.Values)
            {
                renderer.Requested.Mesh = chunk.Mesh;
                renderer.RenderCurrent();
            }
        }
        public void UpdateRender()
        {
            foreach(Chunk chunk in chunks.Values)
            {
                chunk.UpdateRender();
            }
        }
    }
}
