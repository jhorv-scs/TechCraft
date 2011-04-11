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
                        Block block = new Block(BlockType.None,true);

                        if (y < sizeY / 4)
                            block.Type = BlockType.Lava;
                        /*
                         * else if (y == (sizeY / 2) - 1) // test caves visibility 
                         * block.Type = Type.empty;
                         */
                        else if (y < sizeY / 2)
                            block.Type = BlockType.Rock;
                        else if (y == sizeY / 2)
                        {
                            block.Type = BlockType.Grass;
                        }
                        else
                        {
                            if (y == sizeY / 2 + 1 && (x == 0 || x == sizeX - 1 || z == 0 || z == sizeZ - 1))
                                block.Type = BlockType.Sand;
                            else
                                block.Type = BlockType.None;
                        }

                        
                            chunk.setBlock( x, y, z,block);
                        

                    }
                }
            }
        }
        #endregion

    }
}
