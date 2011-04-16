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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using NewTake.view;

namespace NewTake.model
{

    public class Chunk
    {
        public const byte CHUNK_XMAX = 16;
        public const byte CHUNK_YMAX = 128;
        public const byte CHUNK_ZMAX = 16;

        public Chunk N, S, E, W, NE, NW, SE, SW; //TODO infinite y would require Top , Bottom, maybe vertical diagonals

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

        public ChunkRenderer Renderer;

        private BoundingBox _boundingBox;

        public Vector3b highestSolidBlock = new Vector3b(0, 0, 0);
        //highestNoneBlock starts at 0 so it will be adjusted. if you start at highest it will never be adjusted ! 

        public Vector3b lowestNoneBlock = new Vector3b(0, CHUNK_YMAX, 0);

        public Chunk(Vector3i index)
        {
            dirty = true;
            visible = true;
            generated = false;

            Index = index;

            Position = new Vector3i(index.X * CHUNK_XMAX, index.Y * CHUNK_YMAX, index.Z * CHUNK_ZMAX);
            //Blocks = new Block[CHUNK_XMAX, CHUNK_YMAX, CHUNK_ZMAX]; //TODO test 3d sparse impl performance and memory
            this.Blocks = new Block[CHUNK_XMAX * CHUNK_ZMAX * CHUNK_YMAX];
            _boundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z), new Vector3(Position.X + CHUNK_XMAX, Position.Y + CHUNK_YMAX, Position.Z + CHUNK_ZMAX));
        }

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

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        public bool outOfBounds(byte x, byte y, byte z)
        {
            return (x < 0 || x >= CHUNK_XMAX || y < 0 || y >= CHUNK_YMAX || z < 0 || z >= CHUNK_ZMAX);
        }

    }
}
