using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NewTake.model;

namespace NewTake.model.terrain
{
    class TerrainWithCaves : SimpleTerrain
    {

        #region generateTerrain
        protected sealed override void generateTerrain(Chunk chunk, int x, int z, int blockX, int blockZ)
        {
            int groundHeight = (int)GetBlockNoise(blockX, blockZ);
            if (groundHeight < 1)
            {
                groundHeight = 1;
            }
            else if (groundHeight > 128)
            {
                groundHeight = 96;
            }

            // Default to sunlit.. for caves
            bool sunlit = true;

            BlockType blockType = BlockType.None;

            chunk.Blocks[x, groundHeight, z] = new Block(BlockType.Grass,true);
            chunk.Blocks[x, 0, z] = new Block(BlockType.Dirt, true);

            for (int y = Chunk.CHUNK_YMAX - 1; y > 0; y--)
            {
                if (y > groundHeight)
                {
                    blockType = BlockType.None;
                }
                // Or we at or below ground height?
                else if (y < groundHeight)
                {
                    // Since we are at or below ground height, let's see if we need
                    // to make
                    // a cave
                    int noiseX = (blockX + World.SEED);
                    float octave1 = PerlinSimplexNoise.noise(noiseX * 0.009f, blockZ * 0.009f, y * 0.009f) * 0.25f;

                    float initialNoise = octave1 + PerlinSimplexNoise.noise(noiseX * 0.04f, blockZ * 0.04f, y * 0.04f) * 0.15f;
                    initialNoise += PerlinSimplexNoise.noise(noiseX * 0.08f, blockZ * 0.08f, y * 0.08f) * 0.05f;

                    if (initialNoise > 0.2f)
                    {
                        blockType = BlockType.None;
                    }
                    else
                    {
                        // We've placed a block of dirt instead...nothing below us
                        // will be sunlit
                        if (sunlit)
                        {
                            sunlit = false;
                            blockType = BlockType.Grass;
                            //chunk.addGrassBlock(x,y,z);

                        }
                        else
                        {
                            blockType = BlockType.Dirt;
                            if (octave1 < 0.2f)
                            {
                                blockType = BlockType.Rock;
                            }
                        }
                    }
                }
                
                chunk.setBlock(x, y, z, new Block(blockType, sunlit));
                
            }
        }
        #endregion

        private float GetBlockNoise(int blockX, int blockZ)
        {
            float mediumDetail = PerlinSimplexNoise.noise(blockX / 300.0f, blockZ / 300.0f, 20);
            float fineDetail = PerlinSimplexNoise.noise(blockX / 80.0f, blockZ / 80.0f, 30);
            float bigDetails = PerlinSimplexNoise.noise(blockX / 800.0f, blockZ / 800.0f);
            float noise = bigDetails * 64.0f + mediumDetail * 32.0f + fineDetail * 16.0f; // *(bigDetails
            // *
            // 64.0f);

            return noise + 16;
        }
    }
}
