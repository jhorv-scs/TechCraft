using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model.terrain;

namespace NewTake.model.terrain.biome
{
    class Tundra_Alpine : IChunkBuilder
    {

        #region build
        public virtual void build(Chunk chunk)
        {
            for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                int worldX = (int)chunk.Position.X + x + World.SEED;

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
        }
        #endregion

    }
}
