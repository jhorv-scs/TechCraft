#region license

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using NewTake.model.terrain;
using NewTake.model.terrain.biome;
using NewTake.model.types;
#endregion

namespace NewTake.model
{
    public class World
    {

        #region inits

        //public Dictionary2<Chunk> viewableChunks;
        //too experimental for now 
        public ChunkManager viewableChunks;

        public const byte VIEW_CHUNKS_X = 8;
        public const byte VIEW_CHUNKS_Y = 1; 
        public const byte VIEW_CHUNKS_Z = 8;
        public static int SEED = 12345;

        public const byte VIEW_DISTANCE_NEAR_X = VIEW_CHUNKS_X * 2;
        public const byte VIEW_DISTANCE_NEAR_Z = VIEW_CHUNKS_Z * 2;

        public const byte VIEW_DISTANCE_FAR_X = VIEW_CHUNKS_X * 4;
        public const byte VIEW_DISTANCE_FAR_Z = VIEW_CHUNKS_Z * 4;

        #endregion

        #region choose terrain generation
        //public IChunkGenerator Generator = new SimpleTerrain();
        //public IChunkGenerator Generator = new FlatReferenceTerrain();
        //public IChunkGenerator Generator = new TerrainWithCaves();
        public IChunkGenerator Generator = new DualLayerTerrainWithMediumValleysForRivers();

        // Biomes
        //public IChunkGenerator Generator = new Tundra_Alpine();
        //public IChunkGenerator Generator = new Desert_Subtropical();
        //public IChunkGenerator Generator = new Grassland_Temperate();
        #endregion

        public static uint origin = 1000; 
        //TODO UInt32 requires decoupling rendering coordinates to avoid float problems

        public World()
        {
            //viewableChunks = new Dictionary2<Chunk>();//
            viewableChunks = new ChunkManager(new ChunkPersistence(this));
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

        #region BlockAt
        public Block BlockAt(Vector3 position)
        {
            return BlockAt((uint)position.X, (uint)position.Y, (uint)position.Z);
        }

        public Block BlockAt(uint x, uint y, uint z)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = viewableChunks[x / Chunk.CHUNK_XMAX, z / Chunk.CHUNK_ZMAX];
                //return chunk.Blocks[Math.Abs(x % Chunk.CHUNK_XMAX), Math.Abs(y % Chunk.CHUNK_YMAX), Math.Abs(z % Chunk.CHUNK_ZMAX)];
                return chunk.Blocks[Math.Abs(x % Chunk.CHUNK_XMAX) * Chunk.FlattenOffset + Math.Abs(z % Chunk.CHUNK_ZMAX) * Chunk.CHUNK_YMAX + Math.Abs(y % Chunk.CHUNK_YMAX)];
            }
            else
            {
                //Debug.WriteLine("no block at  ({0},{1},{2}) ", x, y, z);
                return new Block(BlockType.None); //TODO blocktype.unknown ( with matrix films green symbols texture ? ) 
            }
        }
        #endregion

        #region setBlock
        public Block setBlock(Vector3i pos, Block b)
        {
            return setBlock(pos.X,pos.Y,pos.Z,b);
        }

        public Block setBlock(uint x, uint y, uint z, Block newType)
        {
            if (InView(x, y, z))
            {
                Chunk chunk = viewableChunks[x / Chunk.CHUNK_XMAX, z / Chunk.CHUNK_ZMAX];

                byte localX = (byte)(x % Chunk.CHUNK_XMAX);
                byte localY = (byte)(y % Chunk.CHUNK_YMAX);
                byte localZ = (byte)(z % Chunk.CHUNK_ZMAX);
                //TODO messy chunk coordinates types
                //Block old = chunk.Blocks[localX, localY, localZ];
                Block old = chunk.Blocks[localX * Chunk.FlattenOffset + localZ * Chunk.CHUNK_YMAX + localY];
                
               //chunk.setBlock is also called by terrain generators for Y loops min max optimisation
               chunk.setBlock(localX, localY, localZ, new Block(newType.Type));
               //TODO ( maybe ? )  when digging, mark neighbours chunks as dirty to fill rendering holes                                       
               
                chunk.dirty = true;

                //TODO use Chunk.N/SW/E accessors
                if (localX == 0)
                {
                    viewableChunks[(x / Chunk.CHUNK_XMAX) - 1, z / Chunk.CHUNK_ZMAX].dirty = true;
                }
                if (localX == Chunk.CHUNK_XMAX - 1)
                {
                    viewableChunks[(x / Chunk.CHUNK_XMAX) + 1, z / Chunk.CHUNK_ZMAX].dirty = true;
                }
                if (localZ == 0)
                {
                    viewableChunks[x / Chunk.CHUNK_XMAX, (z / Chunk.CHUNK_ZMAX) - 1].dirty = true;
                }
                if (localZ == Chunk.CHUNK_ZMAX - 1)
                {
                    viewableChunks[x / Chunk.CHUNK_XMAX, (z / Chunk.CHUNK_ZMAX) + 1].dirty = true;
                }
                
                return old;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region InView
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
        #endregion

    }
}
