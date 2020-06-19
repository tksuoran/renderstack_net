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

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;

namespace example.VoxelRenderer
{
    public partial class Chunk
    {
        private Attribute           position;
        private Attribute           texcoord;
        private Attribute           color;
        private Mesh                mesh;
        private IBufferRange        vertexBufferRange;
        private IBufferRange        indexBufferRange;
        private VertexBufferWriter  vertexWriter;
        private IndexBufferWriter   indexWriter;
        private Map                 map;

        public  Mesh                Mesh { get { return mesh; } }

        private void UseMap(Map map)
        {
            this.map = map;
            position = map.Position;
            texcoord = map.Texcoord;
            color = map.Color;

            mesh = new Mesh();
            mesh.VertexBufferRange = vertexBufferRange = map.VertexBuffer.CreateVertexBufferRange();

            indexBufferRange = mesh.FindOrCreateIndexBufferRange(
                MeshMode.PolygonFill,
                map.IndexBuffer,
                BeginMode.Triangles
            );

            vertexWriter = new VertexBufferWriter(mesh.VertexBufferRange);
            indexWriter = new IndexBufferWriter(indexBufferRange);
        }

        public void FastUpdate(long worldX, long worldZ, byte y)
        {
        }

        public void UpdateRender()
        {
            vertexWriter.BeginEdit();
            indexWriter.BeginEdit();
            int visibleCount = 0;
            for(byte y = 0; y < 128; ++y)
            {
                for(long x = 0; x < 16; ++x)
                {
                    long wx = this.worldX + x;
                    for(long z = 0; z < 16; ++z)
                    {
                        long wz = this.worldZ + z;
                        if(map.IsVisible(wx, y, wz))
                        {
                            Cube(wx, y, wz, map[wx, y, wz]);
                            ++visibleCount;
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("Chunk had " + visibleCount + " visible cubes");
            vertexWriter.EndEdit();
            indexWriter.EndEdit();
        }

        public void CubeFace(Vector3 A, Vector3 B, Vector3 C, Vector3 D, byte u, byte v, UInt32 col, float l)
        {
            indexWriter.Quad(
                vertexWriter.CurrentIndex,      vertexWriter.CurrentIndex + 1,
                vertexWriter.CurrentIndex + 2,  vertexWriter.CurrentIndex + 3
            );
            indexWriter.CurrentIndex += 6;
#if false
            byte r = 80;
            byte g = 200;
            byte b = 80;
            byte a = 255;
#else
            float r = l * (float)((col & 0xff0000) >> 16) / 255.0f;
            float g = l * (float)((col & 0xff00) >>  8) / 255.0f;
            float b = l * (float)((col & 0xff)) / 255.0f;
            float a = 1.0f;
#endif
            float u0 = (float)u * 1.0f / 16.0f;
            float v0 = (float)v * 1.0f / 16.0f;
            float u1 = (float)(u + 1) * 1.0f / 16.0f;
            float v1 = (float)(v + 1) * 1.0f / 16.0f;

            // 
            vertexWriter.Set(position, D.X, D.Y, D.Z);
            vertexWriter.Set(texcoord, u0, v1);
            vertexWriter.Set(color, r, g, b, a);
            ++vertexWriter.CurrentIndex;

            vertexWriter.Set(position, C.X, C.Y, C.Z);
            vertexWriter.Set(texcoord, u1, v1);
            vertexWriter.Set(color, r, g, b, a);
            ++vertexWriter.CurrentIndex;

            vertexWriter.Set(position, B.X, B.Y, B.Z);
            vertexWriter.Set(texcoord, u1, v0);
            vertexWriter.Set(color, r, g, b, a);
            ++vertexWriter.CurrentIndex;

            vertexWriter.Set(position, A.X, A.Y, A.Z);
            vertexWriter.Set(texcoord, u0, v0);
            vertexWriter.Set(color, r, g, b, a);
            ++vertexWriter.CurrentIndex;
        }
        public void Cube(long x, long y, long z, byte blockType)
        {
            //   B     C   
            //  A     D    
            //   F     G   
            //  E     H    
            //             
            float right  = (float) x + 1.0f;
            float top    = (float) y + 1.0f;
            float back   = (float) z + 1.0f;
            float left   = (float) x - 0.0f;
            float bottom = (float) y - 0.0f;
            float front  = (float) z - 0.0f;
            Vector3 A = new Vector3(left,  top,    back);
            Vector3 B = new Vector3(left,  top,    front);
            Vector3 C = new Vector3(right, top,    front);
            Vector3 D = new Vector3(right, top,    back);
            Vector3 E = new Vector3(left,  bottom, back);
            Vector3 F = new Vector3(left,  bottom, front);
            Vector3 G = new Vector3(right, bottom, front);
            Vector3 H = new Vector3(right, bottom, back);

            BlockType block = Map.blockTypes[blockType];
            UInt32 color = 0xffffff;
            if(blockType == BlockType.Grass || blockType == BlockType.Dirt)
            {
                color = 0xccff88;
            }
#if false
            CubeFace(A, B, C, D, block.TopU,    block.TopV,     color, 1.0f);   //  top
            CubeFace(A, D, H, E, block.BackU,   block.BackV,    color, 0.8f);   //  back
            CubeFace(B, A, E, F, block.LeftU,   block.LeftV,    color, 0.5f);   //  left
            CubeFace(F, E, H, G, block.BottomU, block.BottomV,  color, 0.3f);   //  bottom
            CubeFace(D, C, G, H, block.RightU,  block.RightV,   color, 0.5f);   //  right
            CubeFace(C, B, F, G, block.FrontU,  block.FrontV,   color, 0.8f);   //  front
#else
            CubeFace(A, B, C, D, block.TopU,    block.TopV,     color,      1.0f);  //  top
            CubeFace(A, D, H, E, block.BackU,   block.BackV,    0xffffff,   0.8f);  //  back
            CubeFace(B, A, E, F, block.LeftU,   block.LeftV,    0xffffff,   0.6f);  //  left
            CubeFace(F, E, H, G, block.BottomU, block.BottomV,  0xffffff,   0.3f);  //  bottom
            CubeFace(D, C, G, H, block.RightU,  block.RightV,   0xffffff,   0.6f);  //  right
            CubeFace(C, B, F, G, block.FrontU,  block.FrontV,   0xffffff,   0.8f);  // front
#endif
        }
    }
}
