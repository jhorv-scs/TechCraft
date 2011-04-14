using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NewTake;

namespace NewTake.model.terrain.biome
{
    class Grassland_Temperate : SimpleTerrain
    {
        
        public override void Generate(Chunk chunk)
        {
            base.Generate(chunk);
            //GenerateWaterSandLayer(chunk);
        }

        #region generateTerrain
        protected sealed override void generateTerrain(Chunk chunk, int blockXInChunk, int blockZInChunk, int worldX, int worldZ)
        {
            float lowerGroundHeight = GetLowerGroundHeight(chunk, worldX, worldZ, blockXInChunk, blockZInChunk);
            int upperGroundHeight = GetUpperGroundHeight(chunk, worldX, worldZ, lowerGroundHeight);

            bool sunlit = true;
            BlockType blockType;

            for (int y = Chunk.CHUNK_YMAX - 1; y >= 0; y--)
            {
                blockType = BlockType.None;
                if (y > upperGroundHeight)
                {
                    blockType = BlockType.None;
                }
                else
                {
                    blockType = BlockType.Grass;
                }

                if ((y > lowerGroundHeight) && (y < upperGroundHeight))
                {
                    sunlit = false;
                }
                else
                {
                    sunlit = true;
                }
                chunk.setBlock(blockXInChunk, y, blockZInChunk, new Block(blockType, sunlit));
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
                        blockType = BlockType.None;
                        if (chunk.Blocks[x, y, z].Type == BlockType.None)
                        {
                        //    blockType = BlockType.Water;
                        }
                        else
                        {
                            if (chunk.Blocks[x, y, z].Type == BlockType.Grass)
                            {
                                blockType = BlockType.Grass;
                                if (y <= WATERLEVEL)
                                {
                                    sunlit = false;
                                }
                            }
                            break;
                        }
                      
                        chunk.setBlock(x, y, z,new Block(blockType, 0));
                    }

                    for (int y = WATERLEVEL + 27; y >= WATERLEVEL; y--)
                    {
                        if ((y > 11) && (chunk.Blocks[x, y, z].Type == BlockType.Grass)) chunk.setBlock(x, y, z, new Block(BlockType.Grass, sunlit));
                        if ((chunk.Blocks[x, y, z].Type == BlockType.Dirt) || (chunk.Blocks[x, y, z].Type == BlockType.Grass))
                        {
                            chunk.setBlock(x, y, z,new Block(BlockType.Grass, sunlit));
                        }
                    }
                }
            }
        }
        #endregion

        #region GetUpperGroundHeight
        private static int GetUpperGroundHeight(Chunk chunk, int blockX, int blockY, float lowerGroundHeight)
        {

            float octave1 = PerlinSimplexNoise.noise((blockX+50) * 0.0002f, blockY * 0.0002f) * 0.05f;
            float octave2 = PerlinSimplexNoise.noise((blockX+50) * 0.0005f, blockY * 0.0005f) * 0.135f;
            float octave3 = PerlinSimplexNoise.noise((blockX+50) * 0.0025f, blockY * 0.0025f) * 0.15f;
            float octave4 = PerlinSimplexNoise.noise((blockX+50) * 0.0125f, blockY * 0.0125f) * 0.05f;
            float octave5 = PerlinSimplexNoise.noise((blockX+50) * 0.025f,  blockY * 0.025f)  * 0.015f;
            float octave6 = PerlinSimplexNoise.noise((blockX+50) * 0.0125f, blockY * 0.0125f) * 0.04f;

            float octaveSum = octave1 + octave2 + octave3 + octave4 + octave5 + octave6;

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
            //float octave3 = PerlinSimplexNoise.noise(blockX * 0.02f, blockY * 0.02f) * 0.15f;
            float lowerGroundHeight = octave1 + octave2;
            //float lowerGroundHeight = octave1 + octave2 + octave3;

            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

            return lowerGroundHeight;
        }
        #endregion

    }
}
