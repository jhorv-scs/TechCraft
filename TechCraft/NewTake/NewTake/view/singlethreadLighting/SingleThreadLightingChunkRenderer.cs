using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.model;
using NewTake.view.blocks;
using System.Diagnostics;

namespace NewTake.view
{

    //this is the same code as in boundariesChunkRenderer. 

    class SingleThreadLightingChunkRenderer : ChunkRenderer
    {
        private const byte MAX_SUN_VALUE = 16;

        public IndexBuffer indexBuffer;
        private LightingVertexBlockRenderer _blockRenderer;

        public SingleThreadLightingChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk)
            : base(graphicsDevice, world, chunk)
        {
            _blockRenderer = new LightingVertexBlockRenderer(world);
        }

        public override void draw(GameTime gameTime)
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

        public override void DoLighting()
        {
            ClearLighting();
            FillLighting();
        }

        private void ClearLighting()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    bool inShade = false;
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (!inShade)
                        {
                            chunk.Blocks[offset + y].Sun = MAX_SUN_VALUE;
                        }
                        else
                        {
                            chunk.Blocks[offset + y].Sun = 0;
                        }
                        if (chunk.Blocks[offset + y].Type != BlockType.None) inShade = true;
                    }
                }
            }
        }

        private void PropogateLight(byte x, byte y, byte z, byte light)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX + y; 
            Block block = chunk.Blocks[offset];

            if (chunk.Blocks[offset].Sun >= light) return;
            if (light > 16)
            {
                Debug.WriteLine("OUCH");
            }
            chunk.Blocks[offset].Sun = light;

            if (block.Type != BlockType.None) return;

            if (light > 2)
            {
                light = (byte)(light - 1);

                if (x > 0) PropogateLight((byte)(x - 1), y, z, light);
                if (x < Chunk.CHUNK_XMAX - 1) PropogateLight((byte)(x + 1), y, z, light);
                if (y > 0) PropogateLight(x, (byte)(y - 1), z, light);
                if (y < Chunk.CHUNK_XMAX - 1) PropogateLight(x, (byte)(y + 1), z, light);
                if (z > 0) PropogateLight(x, y, (byte)(z - 1), light);
                if (z < Chunk.CHUNK_ZMAX - 1) PropogateLight(x, y, (byte)(z + 1), light);
            }
        }

        private void FillLighting()
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        if (chunk.Blocks[offset + y].Sun > 2)
                        {
                            byte light = (byte)(chunk.Blocks[offset + y].Sun - 1);
                            if (x > 0) PropogateLight((byte)(x - 1), y, z, light);
                            if (x < Chunk.CHUNK_XMAX - 1) PropogateLight((byte)(x + 1), y, z, light);
                            if (y > 0) PropogateLight(x, (byte)(y - 1), z, light);
                            if (y < Chunk.CHUNK_XMAX - 1) PropogateLight(x, (byte)(y + 1), z, light);
                            if (z > 0) PropogateLight(x, y, (byte)(z - 1), light);
                            if (z < Chunk.CHUNK_ZMAX - 1) PropogateLight(x, y, (byte)(z + 1), light);
                        }
                    }
                }
            }
        }


        #region BuildVertexList
        public override void BuildVertexList()
        {
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

            if (v.Length != 0)
            {
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTextureLight), v.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(v);
                indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, i.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(i);
            }

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

            blockTopNW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
            blockTopN = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
            blockTopNE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z + 1));
            blockTopW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
            blockTopM = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
            blockTopE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z));
            blockTopSW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));
            blockTopS = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));
            blockTopSE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y + 1, chunkRelativePosition.Z - 1));

            blockMidNW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
            blockMidN = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
            blockMidNE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z + 1));
            blockMidW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z));
            blockMidM = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z));
            blockMidE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z));
            blockMidSW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));
            blockMidS = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));
            blockMidSE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z - 1));

            blockBotNW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
            blockBotN = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
            blockBotNE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z + 1));
            blockBotW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
            blockBotM = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
            blockBotE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z));
            blockBotSW = BlockAt(new Vector3i(chunkRelativePosition.X - 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));
            blockBotS = BlockAt(new Vector3i(chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));
            blockBotSE = BlockAt(new Vector3i(chunkRelativePosition.X + 1, chunkRelativePosition.Y - 1, chunkRelativePosition.Z - 1));

            float aoTL, aoTR, aoBL, aoBR;

            float lightTopNW, lightTopNE, lightTopSW, lightTopSE;
            float lightBotNW, lightBotNE, lightBotSW, lightBotSE;


            float lTL, lTR, lBL, lBR;

            float aoVertexWeight = 0.05f;
            float light = (1f / MAX_SUN_VALUE) * block.Sun;

            // XDecreasing
            if (!blockMidW.Solid)
            {
                aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                if (blockTopNW.Solid) { aoTL -= aoVertexWeight; }
                if (blockTopW.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                if (blockTopSW.Solid) { aoTR -= aoVertexWeight; }
                if (blockMidNW.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }
                if (blockMidSW.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotNW.Solid) { aoBL -= aoVertexWeight; }
                if (blockBotW.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotSW.Solid) { aoBR -= aoVertexWeight; }

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, aoTL * light, aoTR * light, aoBL * light, aoBR * light);

                //_blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XDecreasing, block.Type, lTL, lTR, lBL, lBR);
            }
            if (!blockMidE.Solid)
            {
                aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                if (blockTopNE.Solid) { aoTR -= aoVertexWeight; }
                if (blockTopE.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                if (blockTopSE.Solid) { aoTL -= aoVertexWeight; }

                if (blockMidNE.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockMidSE.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }

                if (blockBotNE.Solid) { aoBR -= aoVertexWeight; }
                if (blockBotE.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotSE.Solid) { aoBL -= aoVertexWeight; }

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.XIncreasing, block.Type, aoTL * light, aoTR * light, aoBL * light, aoBR * light);
            }
            if (!blockBotM.Solid)
            {
                aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                if (blockBotNW.Solid) { aoTR -= aoVertexWeight; }
                if (blockBotN.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                if (blockBotNE.Solid) { aoTL -= aoVertexWeight; }

                if (blockBotW.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotE.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }

                if (blockBotSW.Solid) { aoBR -= aoVertexWeight; }
                if (blockBotS.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotSE.Solid) { aoBL -= aoVertexWeight; }

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.YDecreasing, block.Type, aoTL * light, aoTR * light, aoBL * light, aoBR * light);

            }
            if (!blockTopM.Solid)
            {
                aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                if (blockTopNW.Solid) { aoTL -= aoVertexWeight; }
                if (blockTopN.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                if (blockTopNE.Solid) { aoTR -= aoVertexWeight; }

                if (blockTopW.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }
                if (blockTopE.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }

                if (blockTopSW.Solid) { aoBL -= aoVertexWeight; }
                if (blockTopS.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockTopSE.Solid) { aoBR -= aoVertexWeight; }

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.YIncreasing, block.Type, aoTL * light, aoTR * light, aoBL * light, aoBR * light);
            }
            if (!blockMidS.Solid)
            {
                aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;
                //aoTL = light; aoTR = light; aoBL = light; aoBR = light;

                if (blockTopSW.Solid) { aoTL -= aoVertexWeight; }
                if (blockTopS.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                if (blockTopSE.Solid) { aoTR -= aoVertexWeight; }

                if (blockMidSW.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }
                if (blockMidSE.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }

                if (blockBotSW.Solid) { aoBL -= aoVertexWeight; }
                if (blockBotS.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotSE.Solid) { aoBR -= aoVertexWeight; }


                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.ZDecreasing, block.Type, aoTL * light, aoTR * light, aoBL * light, aoBR * light);

            }
            if (!blockMidN.Solid)
            {
                aoTL = 1f; aoTR = 1f; aoBL = 1; aoBR = 1;

                if (blockTopNW.Solid) { aoTR -= aoVertexWeight; }
                if (blockTopN.Solid) { aoTL -= aoVertexWeight; aoTR -= aoVertexWeight; }
                if (blockTopNE.Solid) { aoTL -= aoVertexWeight; }

                if (blockMidNW.Solid) { aoTR -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockMidNE.Solid) { aoTL -= aoVertexWeight; aoBL -= aoVertexWeight; }

                if (blockBotNW.Solid) { aoBR -= aoVertexWeight; }
                if (blockBotN.Solid) { aoBL -= aoVertexWeight; aoBR -= aoVertexWeight; }
                if (blockBotNE.Solid) { aoBL -= aoVertexWeight; }

                _blockRenderer.BuildFaceVertices(blockPosition, chunkRelativePosition, BlockFaceDirection.ZIncreasing, block.Type, aoTL * light, aoTR * light, aoBL * light, aoBR * light);
            }

        }
        #endregion

    }
}
