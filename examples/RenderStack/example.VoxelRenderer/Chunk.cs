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
        private long worldX;
        private long worldZ;

        private byte[] data = new byte[16 * 16 * 128];

        public static long IndexOf(byte x, byte y, byte z)
        {
            if((x < 0) || (y < 0) || (z < 0) || (x > 15) || (y > 127) || (z > 15))
            {
                throw new System.ArgumentOutOfRangeException();
            }
            return y * 16 * 16 + x * 16 + z;
        }
        public byte this[byte x, byte y, byte z]
        {
            get
            {
                return data[IndexOf(x, y, z)];
            }
            set
            {
                data[IndexOf(x, y, z)] = value;
            }
        }

        public Chunk(Map map, long worldX, long worldZ)
        {
            UseMap(map);

            worldX = worldX & ~0xf;
            worldZ = worldZ & ~0xf;
            this.worldX = worldX;
            this.worldZ = worldZ;
            for(byte y = 0; y < 128; ++y)
            {
                for(byte x = 0; x < 16; ++x)
                {
                    float relX = (float)(worldX + x) / 16.0f;
                    for(byte z = 0; z < 16; ++z)
                    {
                        byte value;
                        float relZ = (float)(worldZ + z) / 16.0f;
                        float key = (float)Math.Sin(relX * 4.0) + (float)Math.Sin(relZ * 3.0);
                        float key2 = (float)Math.Sin(relX * 1.0) + (float)Math.Sin(relZ * 1.0);
                        byte top = (byte)(20.0f + key * 2.0f + key2 * 10.0f);
                        bool wood = (x == 4) && (z == 4);
                        if(y > top)
                        {
                            value = BlockType.Air;
                            if((y == top + 2) && wood)
                            {
                                value = BlockType.Wood;
                            }
                        }
                        else if(y == top)
                        {
                            value = BlockType.Grass;
                        }
                        else
                        {
                            value = BlockType.Dirt;
                            if(y < top - 4)
                            {
                                value = BlockType.Stone;
                            }
                            if(y == 1)
                            {
                                value = BlockType.Cobblestone;
                            }
                            if(y == 0)
                            {
                                value = BlockType.Bedrock;
                            }
                        }
                        if(value == BlockType.Grass || value == BlockType.Dirt)
                        {
                            if(relZ < 0.3)
                            {
                                value = BlockType.Sand;
                            }
                        }

                        this[x, y, z] = value;
                    }
                }
            }
        }
    }
}