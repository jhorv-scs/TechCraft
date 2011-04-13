using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using NewTake.model.terrain;
using NewTake.model.terrain.biome;

namespace NewTake.model
{
    public class World
    {

        #region inits

        public SparseMatrix<Chunk> viewableChunks;

        public const byte VIEW_CHUNKS_X = 4;
        public const byte VIEW_CHUNKS_Y = 1; 
        public const byte VIEW_CHUNKS_Z = 4;
        public static int SEED = 12345;

        public const byte VIEW_DISTANCE_NEAR_X = VIEW_CHUNKS_X * 2;
        public const byte VIEW_DISTANCE_NEAR_Z = VIEW_CHUNKS_Z * 2;

        public const byte VIEW_DISTANCE_FAR_X = VIEW_CHUNKS_X * 4;
        public const byte VIEW_DISTANCE_FAR_Z = VIEW_CHUNKS_Z * 4;

        #endregion

        //public IChunkGenerator Generator = new SimpleTerrain();
        //public IChunkGenerator Generator = new FlatReferenceTerrain();
        //public IChunkGenerator Generator = new TerrainWithCaves();
        public IChunkGenerator Generator = new DualLayerTerrainWithMediumValleysForRivers();

        // Biomes
        //public IChunkGenerator Generator = new Tundra_Alpine();
        //public IChunkGenerator Generator = new Desert_Subtropical();
        //public IChunkGenerator Generator = new Grassland_Temperate();

        public static uint origin = 1000; 
        //TODO UInt32 requires decoupling rendering coordinates to avoid float problems

        public World()
        {
            viewableChunks = new SparseMatrix<Chunk>();
        }

        public void visitChunks(Action<Vector3i> visitor)
        {
            for (uint x = origin - (World.VIEW_DISTANCE_NEAR_X); x < origin + (World.VIEW_DISTANCE_NEAR_X); x++)
            {
                for (uint z = origin - (World.VIEW_DISTANCE_NEAR_Z); z < origin + (World.VIEW_DISTANCE_NEAR_Z); z++)
                {
                    visitor(new Vector3i(x, 0, z));
                }
            }
        }

        public Block BlockAt(Vector3 position)
        {
            return BlockAt((uint)position.X, (uint)position.Y, (uint)position.Z);
        }

        public Block BlockAt(uint x, uint y, uint z)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = viewableChunks[x / Chunk.CHUNK_XMAX, z / Chunk.CHUNK_ZMAX];
                return chunk.Blocks[Math.Abs(x % Chunk.CHUNK_XMAX), Math.Abs(y % Chunk.CHUNK_YMAX), Math.Abs(z % Chunk.CHUNK_ZMAX)];
            }
            else
            {
                //Debug.WriteLine("no block at  ({0},{1},{2}) ", x, y, z);
                return new Block(BlockType.None, false); //TODO blocktype.unknown ( with matrix films green symbols texture ? ) 
            }
        }

        public Block setBlock(Vector3i pos, Block b)
        {
            return setBlock(pos.X,pos.Y,pos.Z,b);
        }

        //this method is only invoked by user action, not by the engine so optimisation is not a problem for now
        public Block setBlock(uint x, uint y, uint z, Block newType)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = viewableChunks[x / Chunk.CHUNK_XMAX, z / Chunk.CHUNK_ZMAX];
                
                uint localX = x % Chunk.CHUNK_XMAX;
                uint localY = y % Chunk.CHUNK_YMAX;
                uint localZ = z % Chunk.CHUNK_ZMAX;
               //TODO messy chunk coordinates types
                Block old = chunk.Blocks[localX, localY, localZ];
                
               //chunk.setBlock is also called by terrain generators for Y loops min max optimisation
               chunk.setBlock((int)localX, (int)localY, (int)localZ, new Block(newType.Type, old.LightAmount));
               //TODO ( maybe ? )  when digging, mark neighbours chunks as dirty to fill rendering holes                                       
               
                chunk.dirty = true;

                return old;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool InView(uint x, uint y, uint z)
        {
            if (viewableChunks[x / Chunk.CHUNK_XMAX, z / Chunk.CHUNK_ZMAX] == null)
                return false;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint ly = y % Chunk.CHUNK_YMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            if (lx < 0 || ly < 0 || lz < 0
                || lx >= Chunk.CHUNK_XMAX
                || ly >= Chunk.CHUNK_YMAX
                || lz >= Chunk.CHUNK_ZMAX)
            {
                
              //  Debug.WriteLine("no block at  ({0},{1},{2}) ", x, y, z);
                return false;
            }
            return true;
        }
    }
}
