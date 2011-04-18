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
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.model;
using NewTake.view.blocks;
using System.Diagnostics;
#endregion

namespace NewTake.view
{
    //this is the same code as in boundariesChunkRenderer. 

    class MultiThreadLightingChunkRenderer : ChunkRenderer
    {

        #region inits
        private const byte MAX_SUN_VALUE = 16;
        public IndexBuffer indexBuffer;
        private LightingVertexBlockRenderer _blockRenderer;
        private Task buildTask;
        #endregion

        public MultiThreadLightingChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk)
            : base(graphicsDevice, world, chunk)
        {
            _blockRenderer = new LightingVertexBlockRenderer(world);
        }

        public override void DoLighting()
        {
            ClearLighting();
            FillLighting();
        }

        #region ClearLighting
        private void ClearLighting()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    bool inShade = false;
                    for (byte y = Chunk.CHUNK_YMAX - 1; y > 0; y--)
                    {
                        if (chunk.Blocks[offset + y].Type != BlockType.None) inShade = true;
                        if (!inShade)
                        {
                            chunk.Blocks[offset + y].Sun = MAX_SUN_VALUE;
                        }
                        else
                        {
                            chunk.Blocks[offset + y].Sun = 0;
                        }

                        if (chunk.Blocks[offset + y].Type == BlockType.Tree)
                        {
                            chunk.Blocks[offset + y].R = 16;
                            chunk.Blocks[offset + y].G = 0;
                            chunk.Blocks[offset + y].B = 0;
                        }
                        else
                        {
                            chunk.Blocks[offset + y].R = 0;
                            chunk.Blocks[offset + y].G = 0;
                            chunk.Blocks[offset + y].B = 0;
                        }
                    }
                }
            }
        }
        #endregion

        #region PropogateLight
        private void PropogateSunLight(byte x, byte y, byte z, byte light)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].Sun >= light) return;
            chunk.Blocks[offset].Sun = light;

            if (light > 1)
            {
                light = (byte)(light - 1);

                if (x > 0) PropogateSunLight((byte)(x - 1), y, z, light);
                if (x < Chunk.CHUNK_XMAX - 1) PropogateSunLight((byte)(x + 1), y, z, light);
                if (y > 0) PropogateSunLight(x, (byte)(y - 1), z, light);
                if (y < Chunk.CHUNK_YMAX - 1) PropogateSunLight(x, (byte)(y + 1), z, light);
                if (z > 0) PropogateSunLight(x, y, (byte)(z - 1), light);
                if (z < Chunk.CHUNK_ZMAX - 1) PropogateSunLight(x, y, (byte)(z + 1), light);
            }
        }

        private void PropogateLightR(byte x, byte y, byte z, byte lightR)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].R >= lightR) return;
            chunk.Blocks[offset].R = lightR;

            if (lightR > 1)
            {
                lightR = (byte)(lightR - 1);

                if (x > 0) PropogateLightR((byte)(x - 1), y, z, lightR);
                if (x < Chunk.CHUNK_XMAX - 1) PropogateLightR((byte)(x + 1), y, z, lightR);
                if (y > 0) PropogateLightR(x, (byte)(y - 1), z, lightR);
                if (y < Chunk.CHUNK_YMAX - 1) PropogateLightR(x, (byte)(y + 1), z, lightR);
                if (z > 0) PropogateLightR(x, y, (byte)(z - 1), lightR);
                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLightR(x, y, (byte)(z + 1), lightR);
            }
        }

        private void PropogateLightG(byte x, byte y, byte z, byte lightG)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].G >= lightG) return;
            chunk.Blocks[offset].G = lightG;

            if (lightG > 1)
            {
                lightG = (byte)(lightG - 1);

                if (x > 0) PropogateLightG((byte)(x - 1), y, z, lightG);
                if (x < Chunk.CHUNK_XMAX - 1) PropogateLightG((byte)(x + 1), y, z, lightG);
                if (y > 0) PropogateLightG(x, (byte)(y - 1), z, lightG);
                if (y < Chunk.CHUNK_YMAX - 1) PropogateLightG(x, (byte)(y + 1), z, lightG);
                if (z > 0) PropogateLightG(x, y, (byte)(z - 1), lightG);
                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLightG(x, y, (byte)(z + 1), lightG);
            }
        }

        private void PropogateLightB(byte x, byte y, byte z, byte lightB)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].B >= lightB) return;
            chunk.Blocks[offset].B = lightB;

            if (lightB > 1)
            {
                lightB = (byte)(lightB - 1);

                if (x > 0) PropogateLightB((byte)(x - 1), y, z, lightB);
                if (x < Chunk.CHUNK_XMAX - 1) PropogateLightB((byte)(x + 1), y, z, lightB);
                if (y > 0) PropogateLightB(x, (byte)(y - 1), z, lightB);
                if (y < Chunk.CHUNK_YMAX - 1) PropogateLightB(x, (byte)(y + 1), z, lightB);
                if (z > 0) PropogateLightB(x, y, (byte)(z - 1), lightB);
                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLightB(x, y, (byte)(z + 1), lightB);
            }
        }
        #endregion

        #region FillLighting
        private void FillLighting() {
            FillSunLighting();
            FillLightingR();
            FillLightingG();
            FillLightingB();
        }

        private void FillSunLighting()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None)
                        {
                            // Sunlight
                            if (chunk.Blocks[offset + y].Sun > 1)
                            {
                                byte light = (byte)(chunk.Blocks[offset + y].Sun - 1);
                                if (x > 0) PropogateSunLight((byte)(x - 1), y, z, light);
                                if (x < Chunk.CHUNK_XMAX - 1) PropogateSunLight((byte)(x + 1), y, z, light);
                                if (y > 0) PropogateSunLight(x, (byte)(y - 1), z, light);
                                if (y < Chunk.CHUNK_XMAX - 1) PropogateSunLight(x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateSunLight(x, y, (byte)(z - 1), light);
                                if (z < Chunk.CHUNK_ZMAX - 1) PropogateSunLight(x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }

        private void FillLightingR()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.Tree)
                        {
                            // Local light R
                            if (chunk.Blocks[offset + y].R > 1)
                            {
                                byte light = (byte)(chunk.Blocks[offset + y].R - 1);
                                if (x > 0) PropogateLightR((byte)(x - 1), y, z, light);
                                if (x < Chunk.CHUNK_XMAX - 1) PropogateLightR((byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightR(x, (byte)(y - 1), z, light);
                                if (y < Chunk.CHUNK_XMAX - 1) PropogateLightR(x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightR(x, y, (byte)(z - 1), light);
                                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLightR(x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }

        private void FillLightingG()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.Tree)
                        {
                            // Local light G
                            if (chunk.Blocks[offset + y].G > 1)
                            {
                                byte light = (byte)(chunk.Blocks[offset + y].G - 1);
                                if (x > 0) PropogateLightG((byte)(x - 1), y, z, light);
                                if (x < Chunk.CHUNK_XMAX - 1) PropogateLightG((byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightG(x, (byte)(y - 1), z, light);
                                if (y < Chunk.CHUNK_XMAX - 1) PropogateLightG(x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightG(x, y, (byte)(z - 1), light);
                                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLightG(x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }

        private void FillLightingB()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.Tree)
                        {
                            // Local light B
                            if (chunk.Blocks[offset + y].B > 1)
                            {
                                byte light = (byte)(chunk.Blocks[offset + y].B - 1);
                                if (x > 0) PropogateLightB((byte)(x - 1), y, z, light);
                                if (x < Chunk.CHUNK_XMAX - 1) PropogateLightB((byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightB(x, (byte)(y - 1), z, light);
                                if (y < Chunk.CHUNK_XMAX - 1) PropogateLightB(x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightB(x, y, (byte)(z - 1), light);
                                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLightB(x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region BuildVertexList
        public override void BuildVertexList()
        {
            DoLighting();
            _blockRenderer.Clear();

            //lowestNoneBlock and highestNoneBlock come from the terrain gen (Eventually, if the terraingen did not set them you gain nothing)
            //and digging is handled correctly too 
            //TODO generalize highest/lowest None to non-solid
            byte yLow = (byte)(chunk.lowestNoneBlock.Y == 0 ? 0 : chunk.lowestNoneBlock.Y - 1);
            byte yHigh = (byte)(chunk.highestSolidBlock.Y == Chunk.CHUNK_YMAX - 1 ? Chunk.CHUNK_YMAX - 1 : chunk.highestSolidBlock.Y + 1);


            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (chunk.Blocks[offset + y].Type != BlockType.None)
                        {
                            BuildBlockVertices(chunk.Blocks[offset + y], chunk, new Vector3i(x, y, z));
                        }
                    }
                }
            }

            VertexPositionTextureLight[] v = _blockRenderer.vertexList.ToArray();
            short[] i = _blockRenderer.indexList.ToArray();

            //if (v.Length != 0)
            //{
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTextureLight), v.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(v);
            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, i.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(i);
            //}

            chunk.dirty = false;
        }

        #endregion

        #region BuildBlockVertices
        public void BuildBlockVertices(Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {
            //optimized by using chunk.Blocks[][][] except for "out of current chunk" blocks

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;

            Block blockTopNW, blockTopN, blockTopNE, blockTopW, blockTopM, blockTopE, blockTopSW, blockTopS, blockTopSE;
            Block blockMidNW, blockMidN, blockMidNE, blockMidW, blockMidM, blockMidE, blockMidSW, blockMidS, blockMidSE;
            Block blockBotNW, blockBotN, blockBotNE, blockBotW, blockBotM, blockBotE, blockBotSW, blockBotS, blockBotSE;

            Block solidBlock = new Block(BlockType.Rock);

            blockTopNW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
            blockTopN = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
            blockTopNE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
            blockTopW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
            blockTopM = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
            blockTopE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
            blockTopSW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));
            blockTopS = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));
            blockTopSE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));

            blockMidNW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
            blockMidN = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
            blockMidNE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
            blockMidW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z));
            blockMidM = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z));
            blockMidE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z));
            blockMidSW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));
            blockMidS = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));
            blockMidSE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));

            blockBotNW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
            blockBotN = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
            blockBotNE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
            blockBotW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
            blockBotM = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
            blockBotE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
            blockBotSW = chunk.BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));
            blockBotS = chunk.BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));
            blockBotSE = chunk.BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));

            float sunTR, sunTL, sunBR, sunBL;
            float redTR, redTL, redBR, redBL;
            float grnTR, grnTL, grnBR, grnBL;
            float bluTR, bluTL, bluBR, bluBL;
            Color localTR, localTL, localBR, localBL;

            localTR = Color.Black; localTL = Color.Yellow; localBR = Color.Green; localBL = Color.Blue;
            // XDecreasing
            if (!blockMidW.Solid)
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotW.Sun + blockMidNW.Sun + blockMidW.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotW.Sun + blockMidSW.Sun + blockMidW.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopW.R + blockMidNW.R + blockMidW.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopW.R + blockMidSW.R + blockMidW.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotW.R + blockMidNW.R + blockMidW.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotW.R + blockMidSW.R + blockMidW.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopW.G + blockMidNW.G + blockMidW.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopW.G + blockMidSW.G + blockMidW.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotW.G + blockMidNW.G + blockMidW.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotW.G + blockMidSW.G + blockMidW.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopW.B + blockMidNW.B + blockMidW.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopW.B + blockMidSW.B + blockMidW.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotW.B + blockMidNW.B + blockMidW.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotW.B + blockMidSW.B + blockMidW.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (!blockMidE.Solid)
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.Sun + blockTopE.Sun + blockMidSE.Sun + blockMidE.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.Sun + blockTopE.Sun + blockMidNE.Sun + blockMidE.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.Sun + blockBotE.Sun + blockMidSE.Sun + blockMidE.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.Sun + blockBotE.Sun + blockMidNE.Sun + blockMidE.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.R + blockTopE.R + blockMidSE.R + blockMidE.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.R + blockTopE.R + blockMidNE.R + blockMidE.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.R + blockBotE.R + blockMidSE.R + blockMidE.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.R + blockBotE.R + blockMidNE.R + blockMidE.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.G + blockTopE.G + blockMidSE.G + blockMidE.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.G + blockTopE.G + blockMidNE.G + blockMidE.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.G + blockBotE.G + blockMidSE.G + blockMidE.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.G + blockBotE.G + blockMidNE.G + blockMidE.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopSE.B + blockTopE.B + blockMidSE.B + blockMidE.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.B + blockTopE.B + blockMidNE.B + blockMidE.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotSE.B + blockBotE.B + blockMidSE.B + blockMidE.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotNE.B + blockBotE.B + blockMidNE.B + blockMidE.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XIncreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (!blockBotM.Solid)
            {
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotS.Sun + blockBotM.Sun + blockTopW.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.Sun + blockBotS.Sun + blockBotM.Sun + blockTopE.Sun) / 4);
                sunTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotN.Sun + blockBotM.Sun + blockTopW.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.Sun + blockBotN.Sun + blockBotM.Sun + blockTopE.Sun) / 4);

                redBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotS.R + blockBotM.R + blockTopW.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.R + blockBotS.R + blockBotM.R + blockTopE.R) / 4);
                redTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotN.R + blockBotM.R + blockTopW.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.R + blockBotN.R + blockBotM.R + blockTopE.R) / 4);

                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotS.G + blockBotM.G + blockTopW.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.G + blockBotS.G + blockBotM.G + blockTopE.G) / 4);
                grnTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotN.G + blockBotM.G + blockTopW.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.G + blockBotN.G + blockBotM.G + blockTopE.G) / 4);

                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotS.B + blockBotM.B + blockTopW.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.B + blockBotS.B + blockBotM.B + blockTopE.B) / 4);
                bluTL = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotN.B + blockBotM.B + blockTopW.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockBotNE.B + blockBotN.B + blockBotM.B + blockTopE.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.YDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (!blockTopM.Solid)
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopN.Sun + blockTopW.Sun + blockTopM.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.Sun + blockTopN.Sun + blockTopE.Sun + blockTopM.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopS.Sun + blockTopW.Sun + blockTopM.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.Sun + blockTopS.Sun + blockTopE.Sun + blockTopM.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopN.R + blockTopW.R + blockTopM.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.R + blockTopN.R + blockTopE.R + blockTopM.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopS.R + blockTopW.R + blockTopM.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.R + blockTopS.R + blockTopE.R + blockTopM.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopN.G + blockTopW.G + blockTopM.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.G + blockTopN.G + blockTopE.G + blockTopM.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopS.G + blockTopW.G + blockTopM.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.G + blockTopS.G + blockTopE.G + blockTopM.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopN.B + blockTopW.B + blockTopM.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopNE.B + blockTopN.B + blockTopE.B + blockTopM.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopS.B + blockTopW.B + blockTopM.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockTopSE.B + blockTopS.B + blockTopE.B + blockTopM.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.YIncreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (!blockMidS.Solid)
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.Sun + blockTopS.Sun + blockMidSW.Sun + blockMidS.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.Sun + blockTopS.Sun + blockMidSE.Sun + blockMidS.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.Sun + blockBotS.Sun + blockMidSW.Sun + blockMidS.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.Sun + blockBotS.Sun + blockMidSE.Sun + blockMidS.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.R + blockTopS.R + blockMidSW.R + blockMidS.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.R + blockTopS.R + blockMidSE.R + blockMidS.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.R + blockBotS.R + blockMidSW.R + blockMidS.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.R + blockBotS.R + blockMidSE.R + blockMidS.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.G + blockTopS.G + blockMidSW.G + blockMidS.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.G + blockTopS.G + blockMidSE.G + blockMidS.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.G + blockBotS.G + blockMidSW.G + blockMidS.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.G + blockBotS.G + blockMidSE.G + blockMidS.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopSW.B + blockTopS.B + blockMidSW.B + blockMidS.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopSE.B + blockTopS.B + blockMidSE.B + blockMidS.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotSW.B + blockBotS.B + blockMidSW.B + blockMidS.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotSE.B + blockBotS.B + blockMidSE.B + blockMidS.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.ZDecreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }
            if (!blockMidN.Solid)
            {
                sunTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.Sun + blockTopN.Sun + blockMidNE.Sun + blockMidN.Sun) / 4);
                sunTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.Sun + blockTopN.Sun + blockMidNW.Sun + blockMidN.Sun) / 4);
                sunBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.Sun + blockBotN.Sun + blockMidNE.Sun + blockMidN.Sun) / 4);
                sunBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.Sun + blockBotN.Sun + blockMidNW.Sun + blockMidN.Sun) / 4);

                redTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.R + blockTopN.R + blockMidNE.R + blockMidN.R) / 4);
                redTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.R + blockTopN.R + blockMidNW.R + blockMidN.R) / 4);
                redBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.R + blockBotN.R + blockMidNE.R + blockMidN.R) / 4);
                redBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.R + blockBotN.R + blockMidNW.R + blockMidN.R) / 4);

                grnTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.G + blockTopN.G + blockMidNE.G + blockMidN.G) / 4);
                grnTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.G + blockTopN.G + blockMidNW.G + blockMidN.G) / 4);
                grnBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.G + blockBotN.G + blockMidNE.G + blockMidN.G) / 4);
                grnBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.G + blockBotN.G + blockMidNW.G + blockMidN.G) / 4);

                bluTL = (1f / MAX_SUN_VALUE) * ((blockTopNE.B + blockTopN.B + blockMidNE.B + blockMidN.B) / 4);
                bluTR = (1f / MAX_SUN_VALUE) * ((blockTopNW.B + blockTopN.B + blockMidNW.B + blockMidN.B) / 4);
                bluBL = (1f / MAX_SUN_VALUE) * ((blockBotNE.B + blockBotN.B + blockMidNE.B + blockMidN.B) / 4);
                bluBR = (1f / MAX_SUN_VALUE) * ((blockBotNW.B + blockBotN.B + blockMidNW.B + blockMidN.B) / 4);

                localTL = new Color(redTL, grnTL, bluTL);
                localTR = new Color(redTR, grnTR, bluTR);
                localBL = new Color(redBL, grnBL, bluBL);
                localBR = new Color(redBR, grnBR, bluBR);

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.ZIncreasing, block.Type, sunTL, sunTR, sunBL, sunBR, localTL, localTR, localBL, localBR);
            }

        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            if (buildTask == null || (chunk.dirty && buildTask.IsCompleted))
            {
                buildTask = Task.Factory.StartNew(() => BuildVertexList());
            }
        }

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            if (!chunk.generated) return;
            if (!chunk.visible)
            {
                _vertexList.Clear();
                vertexBuffer.Dispose();
                chunk.dirty = false;
            }

            if (vertexBuffer != null)
            {
                if (vertexBuffer.IsDisposed)
                {
                    return;
                }

                if (vertexBuffer.VertexCount > 0)
                {
                    graphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphicsDevice.Indices = indexBuffer;
                    //graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                }

                else
                {
                    Debug.WriteLine("no vertices");
                }
            }
        }
        #endregion

    }
}
