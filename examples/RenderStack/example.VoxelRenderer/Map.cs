using System.Collections.Generic;
using RenderStack.Math;

namespace example.VoxelRenderer
{
    public partial class Map
    {
        private BoundingBox bounds = new BoundingBox();
        private Dictionary<KeyValuePair<long, long>, Chunk> chunks = new Dictionary<KeyValuePair<long,long>,Chunk>();

        public void UpdateChunksAround(long wx, long wz)
        {
            wx &= ~0xf;
            wz &= ~0xf;
            bounds.Clear();
            for(long x = wx; x < wx + 16; ++x)
            {
                for(long z = wz; z < wz + 16; ++z)
                {
                    FindOrCreateChunk(x, z);
                    bounds.ExtendBy((float)(x), 0.0f, (float)(z));
                    bounds.ExtendBy((float)(x + 16), 128.0f, (float)(z + 16));
                }
            }
        }

        public Chunk FindOrCreateChunk(long x, long z)
        {
            x &= ~0xf;
            z &= ~0xf;
            var key = new KeyValuePair<long,long>(x >> 4, z >> 4);
            if(chunks.ContainsKey(key))
            {
                return chunks[key];
            }
            if(chunks.Count > 30)
            {
                //  \todo Recycle furthermost chunk
                /*float distance = 0.0f;
                long delx = long.MaxValue;
                long delz = long.MaxValue;
                foreach(var kvp in chunks.Keys)
                {
                    
                }*/
            }
            var chunk = new Chunk(this, x, z);
            chunks[key] = chunk;
            return chunk;
        }

        public void RequestChunk(long cx, long cy)
        {
            //  \todo implement
        }
        Chunk GetChunk(long cx, long cz)
        {
            long xkey = cx >> 4;
            long zkey = cz >> 4;
            var key = new KeyValuePair<long, long>(xkey, zkey);
            if(chunks.ContainsKey(key))
            {
                return chunks[key];
            }
            RequestChunk(cx, cz);
            return null;
        }
        public byte this[long x, byte y, long z]
        {
            get
            {
                if(y > 127)
                {
                    System.Diagnostics.Debug.WriteLine("XXX");
                    return 0;
                }
                Chunk chunk = GetChunk(x, z);
                if(chunk == null)
                {
                    return 0;
                }
                return chunk[(byte)(x & 0xf), y, (byte)(z & 0xf)];
            }
            set
            {
                if(y > 127)
                {
                    System.Diagnostics.Debug.WriteLine("XXX");
                    return;
                }
                Chunk chunk = GetChunk(x, z);
                if(chunk == null)
                {
                    return;
                }
                chunk[(byte)(x & 0xf), y, (byte)(z & 0xf)] = value;
            }
        }

        public void Put(long x, byte y, long z, byte blockCode)
        {
            Chunk chunk = GetChunk(x, z);
            if(chunk == null)
            {
                return;
            }
            chunk[(byte)(x & 0xf), y, (byte)(z & 0xf)] = blockCode;
            chunk.UpdateRender();
        }

        public bool IsVisible(long x, byte y, long z)
        {
            if(y > 127)
            {
                System.Diagnostics.Debug.WriteLine("XXX");
                return false;
            }

            //  Air is not renderer
            if(this[x,y,z] == 0) return false;
            if(y == 127)
            {
                return true;
            }
            byte left  = this[x - 1, y,     z    ];
            byte right = this[x + 1, y,     z    ];
            byte front = this[x,     y,     z - 1];
            byte back  = this[x,     y,     z + 1];
            int threshold = 1;
            if(
                (BlockType.Opacity[left]  < threshold) ||
                (BlockType.Opacity[right] < threshold) ||
                (BlockType.Opacity[front] < threshold) ||
                (BlockType.Opacity[back]  < threshold) ||
                ((y < 127) && (BlockType.Opacity[this[x, (byte)(y + 1), z    ]] < threshold)) ||
                ((y >   0) && (BlockType.Opacity[this[x, (byte)(y - 1), z    ]] < threshold))
            )
            {
                return true;
            }
            return false;
        }

    }
}


