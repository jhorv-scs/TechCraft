using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;


using NewTake;

namespace NewTake.model.terrain.biome
{
    class Desert_Subtropical : SimpleTerrain
    {
        
        public override void Generate(Chunk chunk)
        {
            base.Generate(chunk);
            //GenerateWaterSandLayer(chunk);
        }

        #region generateTerrain
        protected sealed override void generateTerrain(Chunk chunk, byte blockXInChunk, byte blockZInChunk, uint worldX, uint worldZ)
        {
            float lowerGroundHeight = GetLowerGroundHeight(chunk, worldX, worldZ);
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
                    blockType = BlockType.Sand;
                }

                if ( (y > lowerGroundHeight) && (y < upperGroundHeight) )
                {
                    sunlit = false;
                }
                else
                {
                    sunlit = true;
                }
                chunk.setBlock( blockXInChunk, (byte)y,blockZInChunk,new Block(blockType));
            }

        }
        #endregion

        #region MakeTreeTrunk
        private void MakeTreeTrunk(Chunk chunk, byte tx, byte ty, byte tz, int height)
        {
            Debug.WriteLine("New tree    at {0},{1},{2}={3}", tx, ty, tz, height);
            for (byte y = ty; y < ty + height; y++)
            {
                chunk.setBlock(tx, y, tz,new Block(BlockType.Tree));
            }
        }
        #endregion

        #region MakeTreeFoliage
        private void MakeTreeFoliage(Chunk chunk, int tx, int ty, int tz, int height)
        {
            Debug.WriteLine("New foliage at {0},{1},{2}={3}", tx, ty, tz, height);
            int start = ty + height - 4;
            int end = ty + height + 3;

            int rad;
            int radiusEnd = 2;
            int radiusMiddle = radiusEnd + 1;

            for (int y = start; y < end; y++)
            {
                if ((y > start) && (y < end - 1))
                {
                    rad = radiusMiddle;
                }
                else
                {
                    rad = radiusEnd;
                }

                for (int xoff = -rad; xoff < rad + 1; xoff++)
                {
                    for (int zoff = -rad; zoff < rad + 1; zoff++)
                    {
                        if (chunk.outOfBounds((byte)(tx + xoff), (byte)y, (byte)(tz + zoff)) == false)
                        {
                            chunk.setBlock((byte)(tx + xoff), (byte)y, (byte)(tz + zoff), new Block(BlockType.Leaves));
                            //Debug.WriteLine("rad={0},xoff={1},zoff={2},y={3},start={4},end={5}", rad, xoff, zoff, y, start, end);
                        }
                    }
                }
            }
        }
        #endregion

        #region GenerateWaterSandLayer
        private void GenerateWaterSandLayer(Chunk chunk)
        {
            BlockType blockType;
            
            bool sunlit = true;

            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX;
                    for (byte y = WATERLEVEL + 9; y >= MINIMUMGROUNDHEIGHT; y--)
                    {
                        blockType = BlockType.None;
                        //if (chunk.Blocks[x, y, z].Type == BlockType.None)
                        if (chunk.Blocks[offset + y].Type == BlockType.None)
                        {
                        //    blockType = BlockType.Water;
                        }
                        else
                        {
                            //if (chunk.Blocks[x, y, z].Type == BlockType.Grass)
                            if (chunk.Blocks[offset + y].Type == BlockType.Grass)
                            {
                                blockType = BlockType.Sand;
                                if (y <= WATERLEVEL)
                                {
                                    sunlit = false;
                                }
                            }
                            break;
                        }
                      
                        chunk.setBlock(x, y, z,new Block(blockType));
                    }

                    for (byte y = WATERLEVEL + 27; y >= WATERLEVEL; y--)
                    {
                        //if ((y > 11) && (chunk.Blocks[x, y, z].Type == BlockType.Grass)) chunk.setBlock(x, y, z, new Block(BlockType.Sand, sunlit));
                        if ((y > 11) && (chunk.Blocks[offset + y].Type == BlockType.Grass)) chunk.setBlock(x, y, z, new Block(BlockType.Sand));

                        //if ((chunk.Blocks[x, y, z].Type == BlockType.Dirt) || (chunk.Blocks[x, y, z].Type == BlockType.Grass))
                        if ((chunk.Blocks[offset + y].Type == BlockType.Dirt) || (chunk.Blocks[offset + y].Type == BlockType.Grass))

                        {
                            chunk.setBlock(x, y, z,new Block(BlockType.Sand));
                        }
                    }
                }
            }
        }
        #endregion

        #region GetUpperGroundHeight
        private static int GetUpperGroundHeight(Chunk chunk, uint blockX, uint blockY, float lowerGroundHeight)
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
        private static float GetLowerGroundHeight(Chunk chunk, uint blockX, uint blockY)
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
