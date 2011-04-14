using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NewTake;

namespace NewTake.model.terrain
{
    class DualLayerTerrainWithMediumValleysForRivers : SimpleTerrain
    {
        
        public override void Generate(Chunk chunk)
        {
            base.Generate(chunk);
            GenerateWaterSandLayer(chunk);
        }

        #region generateTerrain
        protected sealed override void generateTerrain(Chunk chunk, int blockXInChunk, int blockZInChunk, int worldX, int worldZ)
        {

            float lowerGroundHeight = GetLowerGroundHeight(chunk, worldX, worldZ, blockXInChunk, blockZInChunk);
            int upperGroundHeight = GetUpperGroundHeight(chunk, worldX, worldZ, lowerGroundHeight);

            bool sunlit = true;

            for (int y = Chunk.CHUNK_YMAX - 1; y >= 0; y--)
            {
                // Everything above ground height...is air.
                BlockType blockType;
                if (y > upperGroundHeight)
                {
                    blockType = BlockType.None;
                }
                // Are we above the lower ground height?
                else if (y > lowerGroundHeight)
                {
                    // Let's see about some caves er valleys!
                    float caveNoise = PerlinSimplexNoise.noise(worldX * 0.01f, worldZ * 0.01f, y * 0.01f) * (0.015f * y) + 0.1f;
                    caveNoise += PerlinSimplexNoise.noise(worldX * 0.01f, worldZ * 0.01f, y * 0.1f) * 0.06f + 0.1f;
                    caveNoise += PerlinSimplexNoise.noise(worldX * 0.2f, worldZ * 0.2f, y * 0.2f) * 0.03f + 0.01f;
                    // We have a cave, draw air here.
                    if (caveNoise > 0.2f)
                    {
                        blockType = BlockType.None;
                    }
                    else
                    {
                        blockType = BlockType.None;
                        if (sunlit)
                        {
                            if (y > SNOWLEVEL + r.Next(3))
                            {
                                blockType = BlockType.Snow;
                            }
                            else
                            {
                                if (r.Next(100) == 1)
                                {
                                    base.BuildTree(chunk, blockXInChunk, y, blockZInChunk);
                                }
                                else
                                {
                                    blockType = BlockType.Grass;
                                }
                            }
                            sunlit = false;
                        }
                        else
                        {
                            blockType = BlockType.Dirt;
                        }
                    }
                }
                else
                {
                    // We are at the lower ground level
                    if (sunlit)
                    {
                        blockType = BlockType.Grass;
                        sunlit = false;
                    }
                    else
                    {
                        blockType = BlockType.Dirt;
                    }
                }

                if (blockType == BlockType.None)
                {
                    if (y <= WATERLEVEL)
                    {
                        blockType = BlockType.Lava;
                        sunlit = false;
                    }
                }
                chunk.setBlock( blockXInChunk, y,blockZInChunk,new Block(blockType, sunlit));
            }
        }

       
        #endregion

        #region GenerateWaterSandLayer
        private void GenerateWaterSandLayer(Chunk chunk)
        {
            BlockType blockType;
            
            bool sunlit = true;

            for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    for (int y = WATERLEVEL + 9; y >= MINIMUMGROUNDHEIGHT; y--)
                    {
                        if (chunk.Blocks[x, y, z].Type == BlockType.None)
                        {
                            blockType = BlockType.Water;
                        }
                        else
                        {
                            if (chunk.Blocks[x, y, z].Type == BlockType.Grass)
                            {
                                blockType = BlockType.Sand;
                                if (y <= WATERLEVEL)
                                {
                                    sunlit = false;
                                }
                            }
                            break;
                        }
                      
                        chunk.setBlock(x, y, z,new Block(blockType, 0));
                    }

                    for (int y = WATERLEVEL + 11; y >= WATERLEVEL + 8; y--)
                    {
                        if ((chunk.Blocks[x, y, z].Type == BlockType.Dirt) || (chunk.Blocks[x, y, z].Type == BlockType.Grass))
                        {
                            chunk.setBlock(x, y, z,new Block(BlockType.Sand, sunlit));
                        }
                    }
                }
            }
        }
        #endregion

        #region GetUpperGroundHeight
        private static int GetUpperGroundHeight(Chunk chunk, int blockX, int blockY, float lowerGroundHeight)
        {
            float octave1 = PerlinSimplexNoise.noise((blockX + 100) * 0.001f, blockY * 0.001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise((blockX + 100) * 0.002f, blockY * 0.002f) * 0.25f;
            float octave3 = PerlinSimplexNoise.noise((blockX + 100) * 0.01f, blockY * 0.01f) * 0.25f;
            float octaveSum = octave1 + octave2 + octave3;

            return (int)(octaveSum * (Chunk.CHUNK_YMAX / 2f)) + (int)(lowerGroundHeight);
        }
        #endregion

        #region GetLowerGroundHeight
        private static float GetLowerGroundHeight(Chunk chunk, int blockX, int blockY, int blockXInChunk, int blockZInChunk)
        {
            int minimumGroundheight = Chunk.CHUNK_YMAX / 4;
            int minimumGroundDepth = (int)(Chunk.CHUNK_YMAX * 0.5f);

            float octave1 = PerlinSimplexNoise.noise(blockX * 0.0001f, blockY * 0.0001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise(blockX * 0.0005f, blockY * 0.0005f) * 0.35f;
            float octave3 = PerlinSimplexNoise.noise(blockX * 0.02f, blockY * 0.02f) * 0.15f;
            float lowerGroundHeight = octave1 + octave2 + octave3;

            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

            return lowerGroundHeight;
        }
        #endregion

    }
}
