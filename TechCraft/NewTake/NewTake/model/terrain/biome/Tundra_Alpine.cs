using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model.terrain;

namespace NewTake.model.terrain.biome
{
    class Tundra_Alpine : IChunkBuilder
    {

        int waterlevel = 64;
        Random r = new Random(World.SEED);

        #region build
        public virtual void build(Chunk chunk)
        {
            //for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            //{
            //    int worldX = (int)chunk.Position.X + x + World.SEED;

            //    for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
            //    {
            //        int worldZ = (int)chunk.Position.Z + z;
            //        generateTerrain(chunk, x, z, worldX, worldZ);
            //    }
            //}
            MapTools.Clear(chunk);
            GenerateDirtLayer(chunk, 10, true);
            GenerateSandLayer(chunk);
            GenerateWaterLayer(chunk);

            chunk.generated = true;
        }
        #endregion

        #region generateTerrain
        protected virtual void generateTerrain(Chunk chunk, int blockXInChunk, int blockZInChunk, int worldX, int worldY)
        {

        }
        #endregion

        private void GenerateWaterLayer(Chunk chunk)
        {
            for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    for (int y = waterlevel + 35; y > 0; y--)
                    {
                        if (chunk.Blocks[x, y, z].Type == BlockType.None)
                        {
                            chunk.setBlock(x, y, z, new Block(BlockType.Water, 0));
                        }
                        else
                        {
                            if (chunk.Blocks[x, y, z].Type == BlockType.Grass)
                            {
                                chunk.setBlock(x, y, z, new Block(BlockType.Sand, 0));
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void GenerateSandLayer(Chunk chunk)
        {
            List<PerlinNoise2D> noiseFunctions = new List<PerlinNoise2D>();
            noiseFunctions.Add(new PerlinNoise2D(4, 2f));
            noiseFunctions.Add(new PerlinNoise2D(8, .5f));
            noiseFunctions.Add(new PerlinNoise2D(12, .25f));
            noiseFunctions.Add(new PerlinNoise2D(26, .125f));
            noiseFunctions.Add(new PerlinNoise2D(34, .0625f));
            noiseFunctions.Add(new PerlinNoise2D(64, .0425f));

            double[,] data = MapTools.SumNoiseFunctions(Chunk.CHUNK_XMAX, Chunk.CHUNK_ZMAX, noiseFunctions);

            for (int x = 0; x < Chunk.CHUNK_ZMAX; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int height = (int)(data[x, z] * 5) + 20 + waterlevel;
                    for (int y = 0; y < height; y++)
                    {
                        if (chunk.Blocks[x, y, z].Type == BlockType.None)
                        {
                            chunk.setBlock(x, y, z, new Block(BlockType.Sand, 0));
                        }
                    }
                }
            }
        }

        private void GenerateDirtLayer(Chunk chunk, int offset, bool trees)
        {

            List<PerlinNoise2D> noiseFunctions = new List<PerlinNoise2D>();
            noiseFunctions.Add(new PerlinNoise2D(4, 2f));
            noiseFunctions.Add(new PerlinNoise2D(8, .5f));
            noiseFunctions.Add(new PerlinNoise2D(12, .25f));
            noiseFunctions.Add(new PerlinNoise2D(26, .125f));
            noiseFunctions.Add(new PerlinNoise2D(34, .0625f));
            noiseFunctions.Add(new PerlinNoise2D(64, .0125f));

            double[,] data = MapTools.SumNoiseFunctions(Chunk.CHUNK_XMAX, Chunk.CHUNK_ZMAX, noiseFunctions);

            for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int height = (int)(data[x, z] * 20) + offset + waterlevel;
                    if (height >= Chunk.CHUNK_YMAX) height = Chunk.CHUNK_YMAX - 1;
                    for (int y = 0; y < height; y++)
                    {
                        if (chunk.Blocks[x, y, z].Type == BlockType.None)
                        {
                            if (y == height - 1)
                            {
                                //if (r.Next(250) == 1 && trees)
                                //{
                                //    BuildTree(x, y, z);
                                //}
                                //else
                                //{
                                    chunk.setBlock(x, y, z, new Block(BlockType.Grass, 0));
                                //}
                            }
                            else
                            {
                                if (r.Next(20) == 1)
                                {
                                    chunk.setBlock(x, y, z, new Block(BlockType.Rock, 0));
                                    //_map[x, y, z] = BlockType.Gravel;
                                }
                                else
                                {
                                    chunk.setBlock(x, y, z, new Block(BlockType.Dirt, 0));
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
