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

using Microsoft.Xna.Framework;
using NewTake.view;
using System.Diagnostics;
#endregion

namespace NewTake.model
{

    public class Chunk
    {

        #region inits
        public const byte CHUNK_XMAX = 16;
        public const byte CHUNK_YMAX = 128;
        public const byte CHUNK_ZMAX = 16;

        private Chunk _N, _S, _E, _W, _NE, _NW, _SE, _SW; //TODO infinite y would require Top , Bottom, maybe vertical diagonals

        public static Vector3b SIZE = new Vector3b(CHUNK_XMAX, CHUNK_YMAX, CHUNK_ZMAX);

        //public Block[, ,] Blocks;

        /// <summary>
        /// Contained blocks as a flattened array.
        /// </summary>
        public Block[] Blocks;

        /* 
        For accessing array for x,z,y coordianate use the pattern: Blocks[x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y]
        For allowing sequental access on blocks using iterations, the blocks are stored as [x,z,y]. So basically iterate x first, z then and y last.
        Consider the following pattern;
        for (int x = 0; x < Chunk.WidthInBlocks; x++)
        {
            for (int z = 0; z < Chunk.LenghtInBlocks; z++)
            {
                int offset = x * Chunk.FlattenOffset + z * Chunk.HeightInBlocks; // we don't want this x-z value to be calculated each in in y-loop!
                for (int y = 0; y < Chunk.HeightInBlocks; y++)
                {
                    var block=Blocks[offset + y].Type 
        */

        /// <summary>
        /// Used when accessing flatten blocks array.
        /// </summary>
        public static int FlattenOffset = CHUNK_ZMAX * CHUNK_YMAX;

        public readonly Vector3i Position;
        public readonly Vector3i Index;

        public bool dirty;
        public bool visible;
        public bool generated;
        public bool built;

        public readonly World world;

        private BoundingBox _boundingBox;

        public Vector3b highestSolidBlock = new Vector3b(0, 0, 0);
        //highestNoneBlock starts at 0 so it will be adjusted. if you start at highest it will never be adjusted ! 

        public Vector3b lowestNoneBlock = new Vector3b(0, CHUNK_YMAX, 0);
        #endregion

        public Chunk(World world, Vector3i index)
        {
            this.world = world;
            dirty = true;
            visible = true;
            generated = false;

            Index = index;

            Position = new Vector3i(index.X * CHUNK_XMAX, index.Y * CHUNK_YMAX, index.Z * CHUNK_ZMAX);
            //Blocks = new Block[CHUNK_XMAX, CHUNK_YMAX, CHUNK_ZMAX]; //TODO test 3d sparse impl performance and memory
            this.Blocks = new Block[CHUNK_XMAX * CHUNK_ZMAX * CHUNK_YMAX];
            _boundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z), new Vector3(Position.X + CHUNK_XMAX, Position.Y + CHUNK_YMAX, Position.Z + CHUNK_ZMAX));

            //ensure world is set directly in here to have access to N S E W as soon as possible
            world.viewableChunks[index.X, index.Z] = this;

        }




        #region setBlock
        public void setBlock(byte x, byte y, byte z, Block b)
        {
            if (b.Type == BlockType.None)
            {
                if (lowestNoneBlock.Y > y)
                {
                    lowestNoneBlock = new Vector3b(x, y, z);
                }
            }
            else if (highestSolidBlock.Y < y)
            {
                //TODO uint vs int is currently a mess and in fact here it should be bytes !
                highestSolidBlock = new Vector3b(x, y, z);
            }

            //Blocks[x, y, z] = b;

            //comment this line : you should have nothing on screen, else you ve been setting blocks directly in array !
            Blocks[x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y] = b;
        }
        #endregion

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        public bool outOfBounds(byte x, byte y, byte z)
        {
            return (x < 0 || x >= CHUNK_XMAX || y < 0 || y >= CHUNK_YMAX || z < 0 || z >= CHUNK_ZMAX);
        }

        #region BlockAt

        public Block BlockAt(int relx,int rely, int relz)
        {
                    
            if (rely < 0 || rely > Chunk.CHUNK_YMAX - 1)
            {
                //infinite Y : y bounds currently set as rock for never rendering those y bounds
                return new Block(BlockType.Rock);
            }

            //handle the normal simple case
            if (relx >= 0 && relz >= 0 && relx < Chunk.CHUNK_XMAX && relz < Chunk.CHUNK_ZMAX)
            {
                Block block = Blocks[relx * Chunk.FlattenOffset + relz * Chunk.CHUNK_YMAX + rely];
                return block;                               
            }
          
            //handle all special cases
            //TODO rename stupid MAX that should be size / use size vector instead
            // was tired of it used 15
            int x = relx, z = relz;
            Chunk nChunk = null;

            if (relx < 0)
            {
                //xChunk = W;
                x = 15;
            }

            if (relz < 0)
            {
                //zChunk = N;
                z = 15;
            }

            if (relx > 15)
            {
                //xChunk = E;
                x = 0;
            }

            if (relz > 15)
            {
                //zChunk = S;
                z = 0;
            }

            if (x!=relx && x == 0)
                if (z!=relz && z == 0)
                    nChunk = SE;
                else if (z != relz && z == 15)
                    nChunk = NE;
                else
                    nChunk = E;
            else if (x != relx &&  x == 15)
                if (z != relz && z == 0)
                    nChunk = SW;
                else if (z != relz && z == 15)
                    nChunk = NW;
                else
                    nChunk = W;
            else
                if (z != relz && z == 0)
                    nChunk = S;
                else if (z != relz && z == 15)
                    nChunk = N;

            if (nChunk == null)
            {
                return new Block(BlockType.Rock);
            }
            else
            {
                Block block =  nChunk.Blocks[x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + rely];
                return block;
            }

        }
        #endregion

        //this neighbours check can not be done in constructor, there would be some holes => it has to be done at access time 
        //TODO check for mem leak / may need weakreferences
        public Chunk N
        {
            get
            {
                if (_N == null)
                    _N = world.viewableChunks[Index.X, Index.Z - 1];
                if (_N != null)
                {
                    _N._S = this;//Debug.WriteLine("_N");
                }
                return _N;
            }
        }
        public Chunk S
        {
            get
            {
                if (_S == null)
                    _S = world.viewableChunks[Index.X, Index.Z + 1];
                if (_S != null)
                {
                    _S._N = this; // Debug.WriteLine("_S"); 
                }
                return _S;
            }
        }
        public Chunk E { get { return _E != null ? _E : _E = world.viewableChunks[Index.X + 1, Index.Z]; } }
        public Chunk W { get { return _W != null ? _W : _W = world.viewableChunks[Index.X - 1, Index.Z]; } }
        public Chunk NW { get { return _NW != null ? _NW : _NW = world.viewableChunks[Index.X - 1, Index.Z - 1]; } }
        public Chunk NE { get { return _NE != null ? _NE : _NE = world.viewableChunks[Index.X + 1, Index.Z - 1]; } }
        public Chunk SW { get { return _SW != null ? _SW : _SW = world.viewableChunks[Index.X - 1, Index.Z + 1]; } }
        public Chunk SE { get { return _SE != null ? _SE : _SE = world.viewableChunks[Index.X + 1, Index.Z + 1]; } }

        //this is a unit test for neighbours
        static void Main(string[] args)
        {
            World world = new World();

            uint n = 4, s = 6, w = 4, e = 6;

            Chunk cw = new Chunk(world, new Vector3i(w, 5, 5));            
            Chunk c = new Chunk(world, new Vector3i(5,5,5));            
            Chunk ce = new Chunk(world, new Vector3i(e, 5, 5));
            
            Chunk cn = new Chunk(world, new Vector3i(5, 5, n));
            Chunk cs = new Chunk(world, new Vector3i(5, 5, s));
            Chunk cne = new Chunk(world, new Vector3i(e, 5, n));
            Chunk cnw = new Chunk(world, new Vector3i(w, 5, n));
            Chunk cse = new Chunk(world, new Vector3i(e, 5, s));
            Chunk csw = new Chunk(world, new Vector3i(w, 5, s));


            c.setBlock(0,0,0,new Block(BlockType.Dirt));
            cw.setBlock(15, 0, 0, new Block(BlockType.Grass));

            Block w15 = c.BlockAt(-1, 0, 0);
            Debug.Assert(w15.Type == BlockType.Grass);

            ce.setBlock(0, 0, 0, new Block(BlockType.Tree));
            Block e0 = c.BlockAt(16, 0, 0);
            Debug.Assert(e0.Type == BlockType.Tree);

            csw.setBlock(15, 0, 0, new Block(BlockType.Lava));
            Block swcorner = c.BlockAt(-1, 0, 16);
            Debug.Assert(swcorner.Type == BlockType.Lava);

            cne.setBlock(0, 0,15, new Block(BlockType.Leaves));
            Block necorner = c.BlockAt(16, 0, -1);
            Debug.Assert(necorner.Type == BlockType.Leaves);
        }

    }
}
