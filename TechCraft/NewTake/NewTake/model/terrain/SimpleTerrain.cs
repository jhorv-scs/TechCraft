using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NewTake.model;

namespace NewTake.model.terrain
{
    class SimpleTerrain : IChunkGenerator
    {

        #region inits

        public const int WATERLEVEL = (int)(Chunk.CHUNK_YMAX * 0.5f);
        public const int SNOWLEVEL = 95;
        public const int MINIMUMGROUNDHEIGHT = Chunk.CHUNK_YMAX / 4;

        public Random r = new Random(World.SEED);

        #endregion

        #region build
        public virtual void Generate(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                uint worldX = (uint)chunk.Position.X + x + (uint)World.SEED ;

                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    uint worldZ = (uint)chunk.Position.Z + z;
                    generateTerrain(chunk, x, z, worldX, worldZ);
                }
            }
            chunk.generated = true;
        }
        #endregion

        #region generateTerrain
        protected virtual void generateTerrain(Chunk chunk, byte blockXInChunk, byte blockZInChunk, uint worldX, uint worldY)
        {
            // The lower ground level is at least this high.
            int minimumGroundheight = Chunk.CHUNK_YMAX / 4;
            int minimumGroundDepth = (int)(Chunk.CHUNK_YMAX * 0.75f);

            float octave1 = PerlinSimplexNoise.noise(worldX * 0.0001f, worldY * 0.0001f) * 0.5f;
            float octave2 = PerlinSimplexNoise.noise(worldX * 0.0005f, worldY * 0.0005f) * 0.25f;
            float octave3 = PerlinSimplexNoise.noise(worldX * 0.005f, worldY * 0.005f) * 0.12f;
            float octave4 = PerlinSimplexNoise.noise(worldX * 0.01f, worldY * 0.01f) * 0.12f;
            float octave5 = PerlinSimplexNoise.noise(worldX * 0.03f, worldY * 0.03f) * octave4;
            float lowerGroundHeight = octave1 + octave2 + octave3 + octave4 + octave5;

            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

            bool sunlit = true;

            BlockType blockType = BlockType.None;

            for (int y = Chunk.CHUNK_YMAX - 1; y >= 0; y--)
            {
                if (y <= lowerGroundHeight)
                {
                    if (sunlit)
                    {
                        blockType = BlockType.Grass;
                        sunlit = false;
                    }
                    else
                    {
                        blockType = BlockType.Rock;
                    }
                }
                
                
                chunk.setBlock(  blockXInChunk, (byte)y, blockZInChunk,new Block(blockType));
                
                
                //  Debug.WriteLine(string.Format("chunk {0} : ({1},{2},{3})={4}", chunk.Position, blockXInChunk, y, blockZInChunk, blockType));
            }
        }
        #endregion

        #region BuildTree
        public virtual void BuildTree(Chunk chunk, byte tx, byte ty, byte tz)
        {

            // Trunk
            byte height = (byte)(4 + (byte)r.Next(3));
            if ((ty + height) < Chunk.CHUNK_YMAX - 1)
            {
                for (byte y = ty; y < ty + height; y++)
                {
                    chunk.setBlock(tx, y, tz, new Block(BlockType.Tree));
                }
            }

            // Foliage
            int radius = 3 + r.Next(2);
            int ny = ty + height;
            for (int i = 0; i < 40 + r.Next(4); i++)
            {
                int lx = tx + r.Next(radius) - r.Next(radius);
                int ly = ny + r.Next(radius) - r.Next(radius);
                int lz = tz + r.Next(radius) - r.Next(radius);

                if (chunk.outOfBounds((byte)lx, (byte)ly, (byte)lz) == false)
                {
                    //if (chunk.Blocks[lx, ly, lz].Type == BlockType.None)
                    if (chunk.Blocks[lx * Chunk.FlattenOffset + lz * Chunk.CHUNK_YMAX + ly].Type == BlockType.None)
                        chunk.setBlock((byte)lx, (byte)ly, (byte)lz, new Block(BlockType.Leaves));
                }
            }

        }
        #endregion

    }
}
