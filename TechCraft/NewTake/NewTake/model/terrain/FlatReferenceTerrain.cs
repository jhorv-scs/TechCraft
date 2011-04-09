using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.terrain
{
    class FlatReferenceTerrain : IChunkBuilder
    {

        #region build
        public void build(Chunk chunk)
        {

            int sizeY = Chunk.CHUNK_YMAX;
            int sizeX = Chunk.CHUNK_XMAX;
            int sizeZ = Chunk.CHUNK_ZMAX;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {

                    for (int z = 0; z < sizeZ; z++)
                    {
                        Block t = new Block(BlockType.None,true);

                        if (y < sizeY / 4)
                            t.Type = BlockType.Lava;
                        /*
                         * else if (y == (sizeY / 2) - 1) // test caves visibility t
                         * = Type.empty;
                         */
                        else if (y < sizeY / 2)
                            t.Type = BlockType.Rock;
                        else if (y == sizeY / 2)
                        {
                            t.Type = BlockType.Grass;
                        }
                        else
                        {
                            if (y == sizeY / 2 + 1 && (x == 0 || x == sizeX - 1 || z == 0 || z == sizeZ - 1))
                                t.Type = BlockType.Sand;
                            else
                                t.Type = BlockType.None;
                        }

                        if (t.Type == BlockType.None)
                        {
                            SimpleTerrain.SetHighLow(chunk, x, y, z);
                        }

                        chunk.Blocks[x, y, z] = t;
                    }
                }
            }
        }
        #endregion

    }
}
