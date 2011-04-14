﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;


using NewTake;

namespace NewTake.model.terrain.biome
{
    class Desert_Subtropical : SimpleTerrain
    {
        int waterLevel = (int)(Chunk.CHUNK_YMAX * 0.5f);
        int snowLevel = 95;
        int minimumGroundheight = Chunk.CHUNK_YMAX / 4;
        
        Random r = new Random(World.SEED);
        
        public override void Generate(Chunk chunk)
        {
            base.Generate(chunk);
            GenerateWaterSandLayer(chunk);
        }

        #region generateTerrain
        protected override void generateTerrain(Chunk chunk, int blockXInChunk, int blockZInChunk, int worldX, int worldZ)
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
                    //float caveNoise = PerlinSimplexNoise.noise(worldX * 0.01f, worldZ * 0.01f, y * 0.01f) * (0.015f * y) + 0.1f;
                    //caveNoise += PerlinSimplexNoise.noise(worldX * 0.01f, worldZ * 0.01f, y * 0.1f) * 0.06f + 0.1f;
                    //caveNoise += PerlinSimplexNoise.noise(worldX * 0.2f, worldZ * 0.2f, y * 0.2f) * 0.03f + 0.01f;
                    //// We have a cave, draw air here.
                    //if (caveNoise > 0.2f)
                    //{
                    //    blockType = BlockType.None;
                    //}
                    //else
                    //{
                        blockType = BlockType.None;
                        if (sunlit)
                        {
                            if (y > snowLevel + r.Next(3))
                            {
                                blockType = BlockType.Snow;
                            }
                            else
                            {
                                if (r.Next(800) == 1)
                                {
                                    if (chunk.outOfBounds((byte)(blockXInChunk + 3), (byte)y, (byte)(blockZInChunk + 3)) == false)
                                    {
                                        BuildTree(chunk, blockXInChunk, y, blockZInChunk);
                                    }
                                }
                                else
                                {
                                    blockType = BlockType.Grass;
                                }
                                //blockType = BlockType.Grass;
                            }
                            sunlit = false;
                        }
                        else
                        {
                            blockType = BlockType.Dirt;
                        }
                    //}
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
                    if (y <= waterLevel)
                    {
                        blockType = BlockType.Lava;
                        sunlit = false;
                    }
                }

                    chunk.setBlock( blockXInChunk, y,blockZInChunk,new Block(blockType, sunlit));
            }
        }

       
        #endregion

        #region BuildTree
        private void BuildTree(Chunk chunk, int tx, int ty, int tz)
        {
            int height = 7 + r.Next(3);

            if ((ty + height) < Chunk.CHUNK_YMAX-3)
            {


                #region Trunk
                //Debug.WriteLine("New tree");
                for (int y = ty; y < ty + height; y++)
                {
                    chunk.setBlock(tx, y, tz,new Block(BlockType.Tree, 0));
                }
                #endregion


                #region Foliage
                //Debug.WriteLine("New foliage");
                int start = ty + height - 4;
                int end = ty + height + 3;

                int rad;
                int radiusEnd = 2;
                int radiusMiddle = radiusEnd + 1;

                for (int y = start; y < end; y++)
                {
                    if ( (y > start) && ( y < end - 1) )
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
                                chunk.setBlock(tx+xoff, y, tz+zoff, new Block(BlockType.Leaves, 0));
                                //Debug.WriteLine("rad={0},xoff={1},zoff={2},y={3},start={4},end={5}", rad, xoff, zoff, y, start, end);
                            }
                        }
                    }
                }
                #endregion

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
                    for (int y = waterLevel + 9; y >= minimumGroundheight; y--)
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
                                blockType = BlockType.Sand;
                                if (y <= waterLevel)
                                {
                                    sunlit = false;
                                }
                            }
                            break;
                        }
                      
                        chunk.setBlock(x, y, z,new Block(blockType, 0));
                    }

                    for (int y = waterLevel + 27; y >= waterLevel; y--)
                    {
                        if ((y > 11) && (chunk.Blocks[x, y, z].Type == BlockType.Grass)) chunk.setBlock(x, y, z, new Block(BlockType.Sand, sunlit));
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