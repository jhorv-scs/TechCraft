using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NewTake.model.terrain;
using System.Diagnostics;


namespace NewTake.model
{
    class World
    {
        public SparseMatrix<Chunk> viewableChunks;
        //public Chunk[,] viewableChunks;

        public const byte VIEW_CHUNKS_X = 4;
        public const byte VIEW_CHUNKS_Y = 1; //TODO allow Y chunks > 1 
        public const byte VIEW_CHUNKS_Z = 4;
        public static int SEED = 12345;

        //public IChunkBuilder builder = new SimpleTerrain();
        //public IChunkBuilder builder = new FlatReferenceTerrain();
        public IChunkBuilder builder = new DualLayerTerrainWithMediumValleysForRivers();

        public static uint origin = 1000; 
        //TODO UInt32, with uint16 65*65 km of 1m blocks is a bit small 
        // but requires decoupling rendering coordinates to avoid float problems


        public World()
        {
            viewableChunks = new SparseMatrix<Chunk>();
            //viewableChunks = new Chunk[VIEW_CHUNKS_X, VIEW_CHUNKS_Z];
            Debug.WriteLine("Initial terrain generation started ...");
            visitChunks(buildAction);
            Debug.WriteLine("............Initial terrain generation done");
            
        }

        public void buildAction(Vector3i vector)
        {
            viewableChunks[vector.X, vector.Z] = new Chunk(vector);
            builder.build(viewableChunks[vector.X, vector.Z]);

        }

        public void visitChunks(Action<Vector3i> visitor)
        {
            for (uint x = origin - (World.VIEW_CHUNKS_X * 3); x < origin + (World.VIEW_CHUNKS_X * 3); x++)
            {
                for (uint z = origin - (World.VIEW_CHUNKS_Z * 3); z < origin + (World.VIEW_CHUNKS_Z * 3); z++)
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
                return new Block(BlockType.None, false);
            }
        }

        public Block setBlock(uint x, uint y, uint z, Block newType)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = viewableChunks[x / Chunk.CHUNK_XMAX, z / Chunk.CHUNK_ZMAX];
                
                uint localX = x % Chunk.CHUNK_XMAX;
                uint localY = y % Chunk.CHUNK_YMAX;
                uint localZ = z % Chunk.CHUNK_ZMAX;
               
                Block old = chunk.Blocks[localX, localY, localZ];
                
                chunk.Blocks[localX, localY, localZ] = newType;
                chunk.dirty = true;

                if (!newType.Solid)
                {
                    //TODO when digging, mark neighbours chunks as dirty to fill rendering holes                                       
                }

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
