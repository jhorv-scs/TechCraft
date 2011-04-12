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

        #region build
        public virtual void Generate(Chunk chunk)
        {
            for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                int worldX = (int)chunk.Position.X + x + World.SEED ;

                for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int worldZ = (int)chunk.Position.Z + z;
                    generateTerrain(chunk, x, z, worldX, worldZ);
                }
            }
            chunk.generated = true;
        }
        #endregion

        #region generateTerrain
        protected virtual void generateTerrain(Chunk chunk, int blockXInChunk, int blockZInChunk, int worldX, int worldY)
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
                
                
                chunk.setBlock(  blockXInChunk, y, blockZInChunk,new Block(blockType, sunlit));
                
                
                //  Debug.WriteLine(string.Format("chunk {0} : ({1},{2},{3})={4}", chunk.Position, blockXInChunk, y, blockZInChunk, blockType));
            }
        }
        #endregion

    }
}
