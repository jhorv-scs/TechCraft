using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.model;
using NewTake.view.blocks;

namespace NewTake.view
{
    class BoundariesChunkRenderer : ChunkRenderer
    {

        #region inits

        public Queue<Chunk> _toBuildVertices;

        public bool _vertexbuildrunning = true;
        public Thread _buildingVerticesThread;

        #endregion

        public BoundariesChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk, Camera camera) : base(graphicsDevice, world, chunk, camera)
        {
            _toBuildVertices = new Queue<Chunk>();
            _buildingVerticesThread = new Thread(new ThreadStart(BuildingVerticesThread));
            _buildingVerticesThread.Start();
        }

        public void QueueBuild()
        {
            //Debug.WriteLine(string.Format("Queue Vertex Build at Chunk {0}-{1}-{2}", (int)chunk.Position.X, (int)chunk.Position.Y, (int)chunk.Position.Z));
            lock (_toBuildVertices)
            {
                _toBuildVertices.Enqueue(chunk);
            }
        }

        public void BuildingVerticesThread()
        {
            while (_vertexbuildrunning)
            {
                Chunk buildChunk = null;
                bool doBuild = false;
                lock (_toBuildVertices)
                {
                    if (_toBuildVertices.Count > 0)
                    {
                        buildChunk = _toBuildVertices.Dequeue();
                        doBuild = true;
                    }
                }
                if (doBuild)
                {
                    DoBuild();
                }
                Thread.Sleep(50);
            }
            //there are cleaner way but all this will be rewritten
            //_toBuildVertices.Abort();
        }

        public void DoBuild()
        {
            BuildVertexList();
        }

        #region BuildVertexList
        public override void BuildVertexList()
        {
            //Debug.WriteLine("building vertexlist ...");
            _vertexList.Clear();
            for (uint x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (uint y = 0; y < Chunk.CHUNK_YMAX; y++)
                {
                    for (uint z = 0; z < Chunk.CHUNK_ZMAX; z++)
                    {
                        Block block = chunk.Blocks[x, y, z];
                        if (block.Type != BlockType.None)
                        {
                            // Vector3i blockPosition = chunk.Position + new Vector3i(x, y, z);

                            BuildBlockVertices(ref _vertexList, block, chunk, new Vector3i(x, y, z));
                        }
                        else
                        {
                            //If we're on a boundary
                            if (x == 0)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X - 1, chunk.Index.Z];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    Block neighbouringBlock = neighbouringChunk.Blocks[Chunk.CHUNK_XMAX - 1, y, z];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(Chunk.CHUNK_XMAX - 1, y, z), BlockFaceDirection.XIncreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                            else if (x == Chunk.CHUNK_XMAX - 1)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X + 1, chunk.Index.Z];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    Block neighbouringBlock = neighbouringChunk.Blocks[0, y, z];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(0, y, z), BlockFaceDirection.XDecreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                            if (y == 0)
                            {
                            }
                            else if (y == Chunk.CHUNK_YMAX - 1)
                            {
                            }
                            if (z == 0)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X, chunk.Index.Z - 1];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    Block neighbouringBlock = neighbouringChunk.Blocks[x, y, Chunk.CHUNK_ZMAX - 1];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(x, y, Chunk.CHUNK_ZMAX - 1), BlockFaceDirection.ZIncreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                            else if (z == Chunk.CHUNK_ZMAX - 1)
                            {
                                Chunk neighbouringChunk = world.viewableChunks[chunk.Index.X, chunk.Index.Z + 1];
                                if (neighbouringChunk != null)
                                {
                                    // If we have a loaded neigbouring chunk
                                    Block neighbouringBlock = neighbouringChunk.Blocks[x, y, 0];
                                    if (neighbouringBlock.Solid)
                                    {
                                        // And a solid neighbouring block 
                                        // Then render it's adjacent face as if it was part of this chunk
                                        blocksRenderer.BuildFaceVertices(ref _vertexList, neighbouringChunk.Position + new Vector3i(x, y, 0), BlockFaceDirection.ZDecreasing, neighbouringBlock.Type);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            VertexPositionTextureShade[] a = _vertexList.ToArray();

            if (a.Length != 0)
            {
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTextureShade), a.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(a);

                chunk.dirty = false;
                //Debug.WriteLine("............building Vertexlist done");
            }
        }
        #endregion

        #region BuildBlockVertices
        public void BuildBlockVertices(ref List<VertexPositionTextureShade> vertexList, Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {
            //optimized by using chunk.Blocks[][][] except for "out of current chunk" blocks

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;
            Block blockXDecreasing, blockXIncreasing, blockYDecreasing, blockYIncreasing, blockZDecreasing, blockZIncreasing;
            Block solidBlock = new Block(BlockType.Rock, false);

            // X Boundary
            if (chunkRelativePosition.X == 0)
            {
                blockXDecreasing = solidBlock; //world.BlockAt(blockPosition.X - 1, blockPosition.Y, blockPosition.Z);
            }
            else
            {
                blockXDecreasing = chunk.Blocks[chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z];
            }
            if (chunkRelativePosition.X == Chunk.SIZE.X - 1)
            {
                blockXIncreasing = solidBlock; //world.BlockAt(blockPosition.X + 1, blockPosition.Y, blockPosition.Z);
            }
            else
            {
                blockXIncreasing = chunk.Blocks[chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z];
            }

            // Y Boundary
            if (chunkRelativePosition.Y == 0)
            {
                blockYDecreasing = solidBlock; //world.BlockAt(blockPosition.X, blockPosition.Y - 1, blockPosition.Z);
            }
            else
            {
                blockYDecreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z];
            }
            if (chunkRelativePosition.Y == Chunk.SIZE.Y - 1)
            {
                blockYIncreasing = solidBlock; // world.BlockAt(blockPosition.X, blockPosition.Y + 1, blockPosition.Z);
            }
            else
            {
                blockYIncreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z];
            }

            // Z Boundary
            if (chunkRelativePosition.Z == 0)
            {
                blockZDecreasing = solidBlock; //world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z - 1);
            }
            else
            {
                blockZDecreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z - 1];
            }
            if (chunkRelativePosition.Z == Chunk.SIZE.Z - 1)
            {
                blockZIncreasing = solidBlock; // world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z + 1);
            }
            else
            {
                blockZIncreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z + 1];
            }

            if (!blockXDecreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XDecreasing, block.Type);
            if (!blockXIncreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XIncreasing, block.Type);

            if (!blockYDecreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YDecreasing, block.Type);
            if (!blockYIncreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YIncreasing, block.Type);

            if (!blockZDecreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZDecreasing, block.Type);
            if (!blockZIncreasing.Solid) blocksRenderer.BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZIncreasing, block.Type);
        }
        #endregion

        #region draw
        public override void draw(GameTime gameTime)
        {

            if (_camera != null)
            {
                uint x = (uint)_camera.Position.X;
                uint z = (uint)_camera.Position.Z;

                uint cx = x / Chunk.CHUNK_XMAX;
                uint cz = z / Chunk.CHUNK_ZMAX;

                uint lx = x % Chunk.CHUNK_XMAX;
                uint lz = z % Chunk.CHUNK_ZMAX;

                Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;

                for (uint j = cx - (World.VIEW_CHUNKS_X); j < cx + (World.VIEW_CHUNKS_X); j++)
                {
                    for (uint l = cz - (World.VIEW_CHUNKS_Z); l < cz + (World.VIEW_CHUNKS_Z); l++)
                    {
                        if (!chunk.generated) return;
                        if (chunk.dirty)
                        {
                            QueueBuild();
                        }
                    }
                }

            }

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
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                    // graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertexList.ToArray(), 0, _vertexList.Count / 3);
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
